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
    using System.Security;
    using System.Text;
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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// User control for running the Azure resource updates.
    /// </summary>
    public partial class AzureUpdateInstallationControl : WizardPageBase
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
        /// Initializes a new instance of the <see cref="AzureUpdateInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public AzureUpdateInstallationControl(
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

            DataModel.SuccessFinalStatusMessage = "Advocacy Platform updated successfully.";
            DataModel.FailureFinalStatusMessage = "Advocacy Platform update failed.";

            QueueOperations(DataModel.InstallationConfiguration);

            // TODO: Data binding isn't working
            WizardProgress.PagesSource = DataModel.Progress;
            OperationsProgressControl.OperationsSource = DataModel.OperationsProgress;

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
                Feature apiFunctions = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.APIFunctions}"];

                if (apiFunctions != null &&
                    apiFunctions.ShouldInstall)
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "UpdateAzureFunctions",
                        OperationFunction = UpdateAzureFunctionsAsync,
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to update Azure Function App!";
                        },
                    });
                }

                Feature apiLogicApps = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.LogicApps}"];

                if (apiLogicApps != null &&
                    apiLogicApps.ShouldInstall)
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "UpdateAPIAzureResources",
                        OperationFunction = (context) =>
                        {
                            ResourceGroupDeploymentStatus status = (ResourceGroupDeploymentStatus)UpdateAPIAzureResourcesAsync(context);

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

                Feature apiLuis = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.AzureLanguageUnderstandingModel}"];

                if (apiLuis != null &&
                    apiLuis.ShouldInstall)
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "UpdateLuisApplication",
                        OperationFunction = UpdateLuisApplicationAsync,
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to update LUIS application!";
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
                        Name = "UpdateUIAzureResources",
                        OperationFunction = (context) =>
                        {
                            ResourceGroupDeploymentStatus status = (ResourceGroupDeploymentStatus)UpdateUIAzureLogicAppsResourcesAsync(context);

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
                            DataModel.StatusMessage = "Failed to deploy Azure resource updates for UI!";
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

        private object UpdateLuisApplicationAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            LuisApplication luisApp = client.GetLuisAppByNameAsync(
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey,
                DataModel.InstallationConfiguration.Azure.Luis.AppName,
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion).Result;

            if (luisApp != null)
            {
                LuisGeneralResponse response = client.DeleteLuisAppAsync(
                    DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey,
                    luisApp.Id,
                    DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion).Result;

                if (response == null)
                {
                    throw new Exception("Failed to delete existing LUIS application.");
                }
            }

            string luisAppId = client.ImportLuisAppAsync(
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey,
                DataModel.InstallationConfiguration.Azure.Luis.AppName,
                DataModel.InstallationConfiguration.Azure.Luis.AppFilePath,
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion).Result;

            if (string.IsNullOrWhiteSpace(luisAppId))
            {
                throw new Exception("Failed to import LUIS application.");
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

            DataModel.InstallationConfiguration.Azure.Luis.EndpointUri = publishedApp.EndpointUrl.Replace(DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion, DataModel.InstallationConfiguration.Azure.Luis.ResourceRegion);

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

        private object UpdateAzureFunctionsAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            PublishData publishingProfiles = client.GetAppServicePublishingProfileAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                DataModel.InstallationConfiguration.Azure.FunctionApp.AppName).Result;
            PublishProfile webDeployProfile =
                publishingProfiles
                    .Profiles
                    .Where(x => x.ProfileName.Contains("Web Deploy"))
                    .FirstOrDefault();

            SecureString publishingProfilePassword = new SecureString();

            foreach (char c in webDeployProfile.Password)
            {
                publishingProfilePassword.AppendChar(c);
            }

            publishingProfilePassword.MakeReadOnly();

            return client.ZipDeployAppServiceAsync(
                DataModel.InstallationConfiguration.Azure.FunctionApp.AppName,
                DataModel.InstallationConfiguration.Azure.FunctionApp.AppDeploymentSourceUrl,
                new NetworkCredential(webDeployProfile.UserName, publishingProfilePassword)).Result;
        }

        private object UpdateAPIAzureResourcesAsync(OperationRunner context)
        {
            string tempArmTemplateFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, System.IO.Path.GetFileName(LogicAppsConfiguration.ApiLogicAppsArmTemplateFilePath));
            string tempArmTemplateParametersFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, System.IO.Path.GetFileName(LogicAppsConfiguration.ApiLogicAppsArmTemplateParametersFilePath));

            File.Copy(
                    LogicAppsConfiguration.ApiLogicAppsArmTemplateFilePath,
                    tempArmTemplateFilePath,
                    true);

            DataModel.InstallationConfiguration.Azure.LogicApps.AADClientId = DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationId;
            DataModel.InstallationConfiguration.Azure.LogicApps.AADAudience = DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationId;

            DataModel.InstallationConfiguration.PowerApps.SaveConfiguration();
            DataModel.InstallationConfiguration.Azure.SaveConfiguration(new Dictionary<string, string>()
            {
                { LogicAppsConfiguration.ApiLogicAppsArmTemplateParametersFilePath, tempArmTemplateParametersFilePath },
            });

            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.CreateResourceGroupDeployment(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                $"API_Update_Deployment_{Guid.NewGuid().ToString()}",
                tempArmTemplateFilePath,
                tempArmTemplateParametersFilePath);
        }

        private object UpdateUIAzureLogicAppsResourcesAsync(OperationRunner context)
        {
            string tempArmTemplateFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, System.IO.Path.GetFileName(LogicAppsConfiguration.LogicAppsArmTemplateFileName));
            string tempArmTemplateParametersFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, System.IO.Path.GetFileName(LogicAppsConfiguration.LogicAppsArmTemplateParametersFileName));

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
                $"UI_Update_Deployment_{Guid.NewGuid().ToString()}",
                tempArmTemplateFilePath,
                tempArmTemplateParametersFilePath);
        }
    }
}
