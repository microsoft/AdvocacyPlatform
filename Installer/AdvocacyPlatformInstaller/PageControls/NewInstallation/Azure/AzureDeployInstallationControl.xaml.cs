// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using AdvocacyPlatformInstaller.Clients;
    using AdvocacyPlatformInstaller.Contracts;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// User control for deploying the Azure resources.
    /// </summary>
    public partial class AzureDeployInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Azure Deployment";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "Resources are being deployed.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureDeployInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public AzureDeployInstallationControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

            SequentialRunner = new OperationRunner(
                model.OperationsProgress,
                this,
                WizardContext.LogFileStream);

            LogOutputControl = DetailsRichTextBox;
            SequentialRunner.OnLog += WriteLog;

            SequentialRunner.OnComplete += SequentialRunner_OnComplete;

            DataModel.CurrentOperationRunner = SequentialRunner;

            DataModel.SuccessFinalStatusMessage = "Advocacy Platform installed successfully.";
            DataModel.FailureFinalStatusMessage = "Advocacy Platform failed to install.";

            // TODO: Data binding isn't working
            WizardProgress.PagesSource = DataModel.Progress;
            OperationsProgressControl.OperationsSource = DataModel.OperationsProgress;

            QueueOperations(DataModel.InstallationConfiguration);

            SequentialRunner.BeginOperationsAsync();
        }

        /// <summary>
        /// Sets default view model values.
        /// </summary>
        public override void SetDefaults()
        {
        }

        /// <summary>
        /// Sets default options in UI.
        /// </summary>
        public override void SetOptions()
        {
        }

        /// <summary>
        /// Validates selections in UI.
        /// </summary>
        /// <returns>True if valid, false if invalid.</returns>
        public override bool ValidatePage()
        {
            return true;
        }

        private void SequentialRunner_OnComplete(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                WizardContext.NextPage();
            }));
        }

        private void QueueOperations(InstallationConfiguration installConfig)
        {
            Feature apiAzure = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}"];

            if (apiAzure.ShouldInstall)
            {
                Feature azureADAppRegistration = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.AzureADAppRegistration}"];

                if (azureADAppRegistration != null &&
                    azureADAppRegistration.ShouldInstall)
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "DeployAzureADFunctionAppRegistration",
                        OperationFunction = DeployAzureADFunctionAppRegistrationAsync,
                        OperationCompletedHandler = (result) =>
                        {
                            AzureApplication response = (AzureApplication)result;

                            DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationId = response.AppId;
                        },
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to create application registration in Azure AD for Azure Function App!";
                        },
                    });
                }

                Feature azureRGDeployment = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.AzureRGDeployment}"];

                if (azureRGDeployment != null &&
                    azureRGDeployment.ShouldInstall)
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "DeployAPIAzureResources",
                        OperationFunction = (context) =>
                        {
                            ResourceGroupDeploymentStatus status = (ResourceGroupDeploymentStatus)DeployAPIAzureResourcesAsync(context);

                            if (string.Compare("Succeeded", status.Status, true) != 0)
                            {
                                throw new Exception("Resource group deployment failed!");
                            }

                            return status;
                        },
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to deploy Azure resources for API!";
                        },
                    });
                }

                Feature azureFunctionAppAuth = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.AzureFunctionAppAuthentication}"];

                if (azureFunctionAppAuth != null &&
                    azureFunctionAppAuth.ShouldInstall)
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "SetAzureFunctionAppAuthentication",
                        OperationFunction = SetFunctionAppAuthenticationAsync,
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to set authentication settings for Azure Function App!";
                        },
                    });
                }

                Feature azureKeyVaultAccessPolicies = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.AzureKeyVault}"];

                if (azureKeyVaultAccessPolicies != null &&
                    azureKeyVaultAccessPolicies.ShouldInstall)
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "DeployAzureKeyVaultAccessPolicies",
                        OperationFunction = SetKeyVaultAccessPoliciesAsync,
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to create access policies on Azure Key Vault!";
                        },
                    });
                }

                Feature azureStorageAccessPolicies = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.AzureStorageAccessPolicies}"];

                if (azureStorageAccessPolicies != null &&
                    azureStorageAccessPolicies.ShouldInstall)
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "DeployAzureStorageStoredAccessPolicies",
                        OperationFunction = DeployStorageAccessPoliciesAsync,
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to create stored access policies on Azure Blob container!";
                        },
                    });
                }

                Feature languageUnderstanding = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.AzureLanguageUnderstandingModel}"];

                if (languageUnderstanding != null &&
                    languageUnderstanding.ShouldInstall)
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "DeployLuisApplication",
                        OperationFunction = DeployLuisApplicationAsync,
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to deploy LUIS application!";
                        },
                    });
                }
            }

            Feature uiAzure = DataModel.InstallationConfiguration.Features[$"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}"];

            if (uiAzure.ShouldInstall)
            {
                Feature uiLogicApps = DataModel.InstallationConfiguration.Features[$"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.LogicApps}"];

                if (uiLogicApps != null &&
                    uiLogicApps.ShouldInstall)
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "DeployUIAzureCdsResources",
                        OperationFunction = (context) =>
                        {
                            ResourceGroupDeploymentStatus status = (ResourceGroupDeploymentStatus)DeployUIAzureCdsResourcesAsync(context);

                            if (string.Compare("Succeeded", status.Status, true) != 0)
                            {
                                throw new Exception("Resource group deployment failed!");
                            }

                            return status;
                        },
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to deploy Azure resources for UI (CDS)!";
                        },
                    });

                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "AuthenticateLogicAppsCDSConnection",
                        OperationFunction = AuthorizeCDSConnectionAsync,
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to authenticate CDS!";
                        },
                    });

                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "DeployUIAzureResources",
                        OperationFunction = (context) =>
                        {
                            ResourceGroupDeploymentStatus status = (ResourceGroupDeploymentStatus)DeployUIAzureResourcesAsync(context);

                            if (string.Compare("Succeeded", status.Status, true) != 0)
                            {
                                throw new Exception("Resource group deployment failed!");
                            }

                            return status;
                        },
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to deploy Azure resources for UI!";
                        },
                    });
                }
            }

            DataModel.OperationsProgress.Operations = SequentialRunner.Operations
                .Select(x => new OperationStatus()
                {
                    Id = x.Id,
                    Name = x.Name,
                    StatusCode = OperationStatusCode.NotStarted,
                })
                .ToList();
        }

        private object DeployAzureADFunctionAppRegistrationAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            AzureApplication application = client.GetApplicationAsync(DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationName).Result;

            if (application != null)
            {
                return application;
            }

            return client.CreateApplicationAsync(new AzureApplicationRequestBase()
            {
                DisplayName = DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationName,
                IdentifierUris = new string[]
                {
                    $"https://{DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationName}",
                },
                PasswordCredentials = new AzureApplicationPasswordCredential[]
                {
                    new AzureApplicationPasswordCredential()
                    {
                        StartDateTime = DateTime.Now.ToString("o"),
                        EndDateTime = DateTime.Now.AddYears(1).ToString("o"),
                        SecretText = DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationSecret.Password,
                    },
                },
                RequiredResourceAccess = new AzureApplicationRequiredResourceAccess[]
                {
                    new AzureApplicationRequiredResourceAccess()
                    {
                        ResourceAppId = "00000003-0000-0000-c000-000000000000",
                        ResourceAccess = new ResourceAccess[]
                        {
                            new ResourceAccess()
                            {
                                Id = "e1fe6dd8-ba31-4d61-89e7-88639da4683d",
                                Type = "Scope",
                            },
                        },
                    },
                },
                SignInAudience = "AzureADMyOrg",
            }).Result;
        }

        private object DeployAPIAzureResourcesAsync(OperationRunner context)
        {
            string tempArmTemplateFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, System.IO.Path.GetFileName(AzureConfiguration.AzureArmTemplateFilePath));
            string tempArmTemplateParametersFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, System.IO.Path.GetFileName(AzureConfiguration.AzureArmTemplateParametersFilePath));

            File.Copy(
                    AzureConfiguration.AzureArmTemplateFilePath,
                    tempArmTemplateFilePath,
                    true);

            // TODO: Do this better
            // if (!string.IsNullOrWhiteSpace(DataModel.InstallationStatusCache.Status.FunctionApp.ClientSecret)) DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationSecret = new NetworkCredential("applicationRegistrationSecret", DataModel.InstallationStatusCache.Status.FunctionApp.ClientSecret);
            DataModel.InstallationConfiguration.Azure.LogicApps.AADClientId = DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationId;
            DataModel.InstallationConfiguration.Azure.LogicApps.AADAudience = DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationId;

            DataModel.InstallationConfiguration.PowerApps.SaveConfiguration();
            DataModel.InstallationConfiguration.Azure.SaveConfiguration(new Dictionary<string, string>()
            {
                { AzureConfiguration.AzureArmTemplateParametersFilePath, tempArmTemplateParametersFilePath },
            });

            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.CreateResourceGroupDeployment(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                $"API_Deployment_{Guid.NewGuid().ToString()}",
                tempArmTemplateFilePath,
                tempArmTemplateParametersFilePath);
        }

        private object SetFunctionAppAuthenticationAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            AppServiceAuthSettings authSettings = client.GetAppServiceAuthSettingsAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                DataModel.InstallationConfiguration.Azure.FunctionApp.AppName).Result;

            if (authSettings == null)
            {
                throw new Exception("Could not obtain authentication settings for Azure Function App!");
            }

            authSettings.Enabled = "true";
            authSettings.DefaultProvider = "AzureActiveDirectory";
            authSettings.IsAadAutoProvisioned = "false";
            authSettings.ClientId = DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationId;
            authSettings.Issuer = "https://sts.windows.net/" + WizardContext.TokenProvider.GetTenantId() + "/";
            authSettings.AllowedAudiences = new string[] { DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationId };

            return client.UpdateAppServiceAuthSettingsAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                DataModel.InstallationConfiguration.Azure.FunctionApp.AppName,
                authSettings).Result;
        }

        private object SetKeyVaultAccessPoliciesAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            // TODO: Make sure we get signed in users information
            Dispatcher.Invoke(new Action(() =>
            {
                WizardContext.TokenProvider.GetAccessTokenAsync(AzureClient.KeyVaultAudience, Application.Current.MainWindow).Wait();
            }));

            // Deploying user
            CreateKeyVaultAccessPolicyResponse result = client.CreateKeyVaultAccessPolicyAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                DataModel.InstallationConfiguration.Azure.KeyVault.KeyVaultName,
                new CreateKeyVaultAccessPolicyRequest()
                {
                    Properties = new CreateKeyVaultAccessPolicyRequestProperties()
                    {
                        AccessPolicies = new KeyVaultAccessPolicy[]
                        {
                            new KeyVaultAccessPolicy()
                            {
                                ObjectId = WizardContext.TokenProvider.GetUserId(),
                                TenantId = WizardContext.TokenProvider.GetTenantId(),
                                Permissions = new KeyVaultAccessPolicyPermissions()
                                {
                                    Secrets = new string[] { "Get", "List", "Set", "Delete", "Backup", "Restore", "Recover", "Purge" },
                                    Keys = new string[] { "Decrypt", "Encrypt", "UnwrapKey", "WrapKey", "Verify", "Sign", "Get", "List", "Update", "Create", "Import", "Delete", "Backup", "Restore", "Recover", "Purge" },
                                    Certificates = new string[] { "Get", "List", "Delete", "Create", "Import", "Update", "Managecontacts", "Getissuers", "Listissuers", "Setissuers", "Deleteissuers", "Manageissuers", "Recover", "Backup", "Restore", "Purge" },
                                },
                            },
                        },
                    },
                }).Result;

            if (result == null)
            {
                throw new Exception("Could not create access policy for user!");
            }

            // Function app
            AzureIdentityResourceBase functionApp = client.GetResourceIdentityBaseAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                "Microsoft.Web",
                null,
                "sites",
                DataModel.InstallationConfiguration.Azure.FunctionApp.AppName,
                "2018-11-01").Result;

            if (functionApp == null)
            {
                throw new Exception("Could not get function app resource!");
            }

            result = client.CreateKeyVaultAccessPolicyAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                DataModel.InstallationConfiguration.Azure.KeyVault.KeyVaultName,
                new CreateKeyVaultAccessPolicyRequest()
                {
                    Properties = new CreateKeyVaultAccessPolicyRequestProperties()
                    {
                        AccessPolicies = new KeyVaultAccessPolicy[]
                        {
                            new KeyVaultAccessPolicy()
                            {
                                ObjectId = functionApp.Identity.PrincipalId,
                                TenantId = WizardContext.TokenProvider.GetTenantId(),
                                Permissions = new KeyVaultAccessPolicyPermissions()
                                {
                                    Secrets = new string[] { "Get" },
                                },
                            },
                        },
                    },
                }).Result;

            if (result == null)
            {
                throw new Exception("Could not create access policy for function app!");
            }

            // Process workflow
            AzureIdentityResourceBase processWorkflow = client.GetResourceIdentityBaseAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                "Microsoft.Logic",
                null,
                "workflows",
                DataModel.InstallationConfiguration.Azure.LogicApps.ProcessWorkflowName).Result;

            if (processWorkflow == null)
            {
                throw new Exception("Could not get process logic app resource!");
            }

            result = client.CreateKeyVaultAccessPolicyAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                DataModel.InstallationConfiguration.Azure.KeyVault.KeyVaultName,
                new CreateKeyVaultAccessPolicyRequest()
                {
                    Properties = new CreateKeyVaultAccessPolicyRequestProperties()
                    {
                        AccessPolicies = new KeyVaultAccessPolicy[]
                        {
                            new KeyVaultAccessPolicy()
                            {
                                ObjectId = processWorkflow.Identity.PrincipalId,
                                TenantId = WizardContext.TokenProvider.GetTenantId(),
                                Permissions = new KeyVaultAccessPolicyPermissions()
                                {
                                    Secrets = new string[] { "Get" },
                                },
                            },
                        },
                    },
                }).Result;

            if (processWorkflow == null)
            {
                throw new Exception("Could not create access policy for process logic app!");
            }

            // Request workflow
            AzureIdentityResourceBase requestWorkflow = client.GetResourceIdentityBaseAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                "Microsoft.Logic",
                null,
                "workflows",
                DataModel.InstallationConfiguration.Azure.LogicApps.RequestWorkflowName).Result;

            if (requestWorkflow == null)
            {
                throw new Exception("Could not get request logic app resource!");
            }

            return client.CreateKeyVaultAccessPolicyAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                DataModel.InstallationConfiguration.Azure.KeyVault.KeyVaultName,
                new CreateKeyVaultAccessPolicyRequest()
                {
                    Properties = new CreateKeyVaultAccessPolicyRequestProperties()
                    {
                        AccessPolicies = new KeyVaultAccessPolicy[]
                        {
                            new KeyVaultAccessPolicy()
                            {
                                ObjectId = requestWorkflow.Identity.PrincipalId,
                                TenantId = WizardContext.TokenProvider.GetTenantId(),
                                Permissions = new KeyVaultAccessPolicyPermissions()
                                {
                                    Secrets = new string[] { "Get" },
                                },
                            },
                        },
                    },
                }).Result;
        }

        private object DeployStorageAccessPoliciesAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            // Create shared access signatures
            StorageAccountResource result = client.GetResourceAsync<StorageAccountResource>(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                "Microsoft.Storage",
                null,
                "storageAccounts",
                DataModel.InstallationConfiguration.Azure.StorageAccount.StorageAccountName,
                "2019-04-01").Result;

            if (result == null)
            {
                throw new Exception("Could not acquire storage account!");
            }

            ListKeysResponse accessKeys = client.InvokeResourceAction2Async<ListKeysResponse>(
                result.Id,
                "listkeys",
                string.Empty,
                "2019-04-01").Result;

            if (accessKeys == null ||
                accessKeys.Keys.Count() == 0)
            {
                throw new Exception("Could not acquire storage account access key!");
            }

            NetworkCredential accessKey = new NetworkCredential(string.Empty, accessKeys.Keys[0].Value);

            DataModel.InstallationConfiguration.Azure.StorageAccount.FullAccessPolicyId = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    Guid.NewGuid().ToString()));
            DataModel.InstallationConfiguration.Azure.StorageAccount.ReadAccessPolicyId = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    Guid.NewGuid().ToString()));

            string accessPolicy = client.CreateBlobStoredAccessPolicyAsync(
                DataModel.InstallationConfiguration.Azure.StorageAccount.StorageAccountName,
                StorageAccountConfiguration.RecordingsContainerName,
                new SignedIdentifiers()
                {
                    SignedIdentifier = new SignedIdentifier[]
                    {
                        new SignedIdentifier()
                        {
                            Id = DataModel.InstallationConfiguration.Azure.StorageAccount.FullAccessPolicyId,
                            AccessPolicy = new AccessPolicy()
                            {
                                Start = DateTime.UtcNow.ToString("o"),
                                Expiry = DateTime.UtcNow.AddYears(1).ToString("o"),
                                Permission = "rwd",
                            },
                        },
                        new SignedIdentifier()
                        {
                            Id = DataModel.InstallationConfiguration.Azure.StorageAccount.ReadAccessPolicyId,
                            AccessPolicy = new AccessPolicy()
                            {
                                Start = DateTime.UtcNow.ToString("o"),
                                Expiry = DateTime.UtcNow.AddYears(1).ToString("o"),
                                Permission = "r",
                            },
                        },
                    },
                }).Result;

            if (accessPolicy == null)
            {
                throw new Exception("Could not create stored access policies on Azure Blob container!");
            }

            string fullSharedAccessSignature = client.CreateSharedAccessSignature(
                DataModel.InstallationConfiguration.Azure.StorageAccount.StorageAccountName,
                StorageAccountConfiguration.RecordingsContainerName,
                accessKey,
                DataModel.InstallationConfiguration.Azure.StorageAccount.FullAccessPolicyId,
                new SharedAccessBlobPolicy());

            if (fullSharedAccessSignature.StartsWith("?"))
            {
                fullSharedAccessSignature = fullSharedAccessSignature.Substring(1);
            }

            AzureKeyVaultSecret fullAccessSecret = client.UpdateKeyVaultSecretAsync(
                DataModel.InstallationConfiguration.Azure.KeyVault.KeyVaultName,
                DataModel.InstallationConfiguration.Azure.KeyVault.StorageAccessKeySecretName,
                new NetworkCredential("full", fullSharedAccessSignature)).Result;

            string readSharedAccessSignature = client.CreateSharedAccessSignature(
                DataModel.InstallationConfiguration.Azure.StorageAccount.StorageAccountName,
                StorageAccountConfiguration.RecordingsContainerName,
                accessKey,
                DataModel.InstallationConfiguration.Azure.StorageAccount.ReadAccessPolicyId,
                new SharedAccessBlobPolicy());

            if (readSharedAccessSignature.StartsWith("?"))
            {
                readSharedAccessSignature = readSharedAccessSignature.Substring(1);
            }

            AzureKeyVaultSecret readAccessSecret = client.UpdateKeyVaultSecretAsync(
                DataModel.InstallationConfiguration.Azure.KeyVault.KeyVaultName,
                DataModel.InstallationConfiguration.Azure.KeyVault.StorageReadAccessKeySecretName,
                new NetworkCredential("read", readSharedAccessSignature)).Result;

            return null;
        }

        private object DeployLuisApplicationAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            LuisApplication luisApp = client.GetLuisAppByNameAsync(
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey,
                DataModel.InstallationConfiguration.Azure.Luis.AppName,
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion).Result;

            string luisAppId = null;

            if (luisApp == null)
            {
                luisAppId = client.ImportLuisAppAsync(
                    DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey,
                    DataModel.InstallationConfiguration.Azure.Luis.AppName,
                    DataModel.InstallationConfiguration.Azure.Luis.AppFilePath,
                    DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion).Result;

                if (string.IsNullOrWhiteSpace(luisAppId))
                {
                    throw new Exception("Failed to import LUIS model.");
                }
            }
            else
            {
                luisAppId = luisApp.Id;
            }

            DataModel.InstallationConfiguration.Azure.Luis.ApplicationId = luisAppId;

            LuisGeneralResponse associatedApp = client.AssociateAzureResourceWithLuisAppAsync(
                luisAppId,
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion,
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey,
                new LuisAssociatedAzureResourceRequest()
                {
                    AzureSubscriptionId = DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                    ResourceGroup = DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                    AccountName = DataModel.InstallationConfiguration.Azure.Luis.ResourceName,
                }).Result;

            if (associatedApp == null ||
                string.Compare(associatedApp.Code, "Success", true) != 0)
            {
                throw new Exception("Failed to associated LUIS application with Azure LUIS Cognitive Services resource!");
            }

            LuisModelTrainingStatus[] trainedApp = client.TrainLuisApp(
                DataModel.InstallationConfiguration.Azure.Luis.ApplicationId,
                DataModel.InstallationConfiguration.Azure.Luis.AppVersion,
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion,
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey);

            if (trainedApp
                .Where(x => x.Details.StatusId != 0 &&
                    x.Details.StatusId != 2).Count() > 0)
            {
                throw new Exception("Failed to train LUIS application!");
            }

            LuisPublishResponse publishedApp = client.PublishLuisAppAsync(
                DataModel.InstallationConfiguration.Azure.Luis.ApplicationId,
                DataModel.InstallationConfiguration.Azure.Luis.AppVersion,
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion,
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey).Result;

            if (publishedApp == null)
            {
                throw new Exception("Failed to publish LUIS application!");
            }

            DataModel.InstallationConfiguration.Azure.Luis.EndpointUri = $"{publishedApp.EndpointUrl.Replace(DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion, DataModel.InstallationConfiguration.Azure.Luis.ResourceRegion)}?staging=false&timezoneOffset=-360";

            // Update settings
            AppServiceAppSettings appSettings = client.GetAppServiceAppSettingsAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                DataModel.InstallationConfiguration.Azure.FunctionApp.AppName).Result;

            if (appSettings == null)
            {
                throw new Exception("Failed to get app settings for app service!");
            }

            appSettings.Properties["luisEndpoint"] = DataModel.InstallationConfiguration.Azure.Luis.EndpointUri;

            AppServiceAppSettings updatedAppSettings = client.UpdateAppServiceAppSettingsAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                DataModel.InstallationConfiguration.Azure.FunctionApp.AppName,
                appSettings).Result;

            if (appSettings == null)
            {
                throw new Exception("Failed to update app settings for app service!");
            }

            return true;
        }

        private object DeployUIAzureCdsResourcesAsync(OperationRunner context)
        {
            if (!Directory.Exists(DataModel.InstallationConfiguration.Azure.ArmTempPath))
            {
                Directory.CreateDirectory(DataModel.InstallationConfiguration.Azure.ArmTempPath);
            }

            string tempArmTemplateFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, System.IO.Path.GetFileName(LogicAppsConfiguration.CdsLogicAppsArmTemplateFilePath));
            string tempArmTemplateParametersFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, System.IO.Path.GetFileName(LogicAppsConfiguration.CdsLogicAppsArmTemplateParametersFilePath));

            File.Copy(
                LogicAppsConfiguration.CdsLogicAppsArmTemplateFilePath,
                tempArmTemplateFilePath,
                true);

            DataModel.InstallationConfiguration.PowerApps.SaveConfiguration();

            DataModel.InstallationConfiguration.Azure.SaveConfiguration(new Dictionary<string, string>()
            {
                { LogicAppsConfiguration.CdsLogicAppsArmTemplateParametersFilePath, tempArmTemplateParametersFilePath },
            });

            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.CreateResourceGroupDeployment(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                $"UI_CDS_Deployment_{Guid.NewGuid().ToString()}",
                tempArmTemplateFilePath,
                tempArmTemplateParametersFilePath);
        }

        private object DeployUIAzureResourcesAsync(OperationRunner context)
        {
            if (!Directory.Exists(DataModel.InstallationConfiguration.Azure.ArmTempPath))
            {
                Directory.CreateDirectory(DataModel.InstallationConfiguration.Azure.ArmTempPath);
            }

            string tempArmTemplateFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, System.IO.Path.GetFileName(LogicAppsConfiguration.LogicAppsArmTemplateFilePath));
            string tempArmTemplateParametersFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, System.IO.Path.GetFileName(LogicAppsConfiguration.LogicAppsArmTemplateParametersFilePath));

            File.Copy(
                LogicAppsConfiguration.LogicAppsArmTemplateFilePath,
                tempArmTemplateFilePath,
                true);

            DataModel.InstallationConfiguration.PowerApps.SaveConfiguration();
            DataModel.InstallationConfiguration.Azure.SaveConfiguration(new Dictionary<string, string>()
            {
                { LogicAppsConfiguration.LogicAppsArmTemplateParametersFilePath, tempArmTemplateParametersFilePath },
            });

            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.CreateResourceGroupDeployment(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                $"UI_Deployment_{Guid.NewGuid().ToString()}",
                tempArmTemplateFilePath,
                tempArmTemplateParametersFilePath);
        }

        private object AuthorizeCDSConnectionAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.AuthenticateApiConnection(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                DataModel.InstallationConfiguration.Azure.LogicApps.CdsConnectionName);
        }
    }
}
