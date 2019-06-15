// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
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
    /// User control for confirming installer actions before actions begin.
    /// </summary>
    public partial class InstallerConfirmActionsInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Deployment Confirmation";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "Confirm acceptance of the following changes.";

        private string _tempArmTemplateFilePath;
        private string _tempArmTemplateParametersFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallerConfirmActionsInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public InstallerConfirmActionsInstallationControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

            SequentialRunner = new OperationRunner(
                model.OperationsProgress,
                this,
                WizardContext.LogFileStream);
            SequentialRunner.IndeterminateOps = true;

            LogOutputControl = DetailsRichTextBox;
            SequentialRunner.OnLog += WriteLog;

            SequentialRunner.OnComplete += SequentialRunner_OnComplete;

            DataModel.CurrentOperationRunner = SequentialRunner;

            // TODO: Data binding isn't working
            WizardProgress.PagesSource = DataModel.Progress;

            BuildSummary();

            QueueOperations();

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
            switch (DataModel.InstallerAction)
            {
                case InstallerActionType.New:
                case InstallerActionType.Modify:
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        WizardContext.NextPage();
                    }));
                    break;

                default:
                    break;
            }
        }

        private void BuildSummary()
        {
            Paragraph newPara = new Paragraph();

            SummaryRichTextBox.Document.Blocks.Clear();
            SummaryRichTextBox.Document.Blocks.Add(newPara);

            switch (DataModel.InstallerAction)
            {
                case InstallerActionType.New:
                    BuildInstallSummary(newPara);
                    break;

                case InstallerActionType.Modify:
                    BuildUpdateSummary(newPara);
                    break;

                case InstallerActionType.Remove:
                    BuildUninstallSummary(newPara);
                    break;
            }

            SummaryRichTextBox.ScrollToEnd();
        }

        private void WriteSummaryMessage(IEnumerable<Span> messages, Color? foregroundColor = null)
        {
            Paragraph firstPara = SummaryRichTextBox.Document.Blocks.Where(x => x is Paragraph).FirstOrDefault() as Paragraph;

            if (firstPara == null)
            {
                firstPara = new Paragraph();
                SummaryRichTextBox.Document.Blocks.Add(firstPara);
            }

            firstPara.Inlines.AddRange(messages);

            SummaryRichTextBox.ScrollToEnd();
        }

        private void WriteArmValidationMessage(string type, OperationRunner runner)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Span validARMSpan = new Span(new Run($"Proposed {type} Azure deployment is {(runner.LastOperationStatusCode == 0 ? "valid" : "invalid")}.\n"))
                {
                    Foreground = new SolidColorBrush(runner.LastOperationStatusCode == 0 ? Colors.Green : Colors.Red),
                };

                WriteSummaryMessage(new List<Span>() { validARMSpan });
            }));
        }

        private void QueueOperations()
        {
            switch (DataModel.InstallerAction)
            {
                case InstallerActionType.New:
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "EnsureResourceGroup",
                        OperationFunction = EnsureResourceGroupAsync,
                        OperationCompletedHandler = (result) =>
                        {
                            if (result == null)
                            {
                                SequentialRunner.LastOperationStatusCode = -1;
                            }
                        },
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to ensure Azure resource group!";
                        },
                    });

                    Feature apiAzureResources = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}"];

                    if (apiAzureResources.ShouldInstall)
                    {
                        Feature apiAzureResourceGroupDeployment = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.AzureRGDeployment}"];

                        if (apiAzureResourceGroupDeployment.ShouldInstall)
                        {
                            SequentialRunner.Operations.Enqueue(new Operation()
                            {
                                Name = "ValidateAzureArmTemplate",
                                OperationFunction = BeginApiValidationAsync,
                                OperationCompletedHandler = (result) =>
                                {
                                    bool? response = (bool?)result;

                                    WriteArmValidationMessage("API", SequentialRunner);

                                    if (!response.HasValue || !response.Value)
                                    {
                                        SequentialRunner.LastOperationStatusCode = -1;
                                    }
                                },
                                ValidateFunction = (context) =>
                                {
                                    return context.LastOperationStatusCode == 0;
                                },
                                ExceptionHandler = (ex) =>
                                {
                                    DataModel.StatusMessage = "Failed to validate Azure Resource Group deployment for API resources!";
                                },
                            });
                        }
                    }

                    Feature uiCdsAzureResources = DataModel.InstallationConfiguration.Features[$"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}"];

                    if (uiCdsAzureResources.ShouldInstall)
                    {
                        SequentialRunner.Operations.Enqueue(new Operation()
                        {
                            Name = "ValidateAzureArmTemplate",
                            OperationFunction = BeginCdsLogicAppValidationAsync,
                            OperationCompletedHandler = (result) =>
                            {
                                bool? response = (bool?)result;

                                WriteArmValidationMessage("UI CDS", SequentialRunner);

                                if (!response.HasValue || !response.Value)
                                {
                                    SequentialRunner.LastOperationStatusCode = -1;
                                }
                            },
                            ValidateFunction = (context) =>
                            {
                                return context.LastOperationStatusCode == 0;
                            },
                            ExceptionHandler = (ex) =>
                            {
                                DataModel.StatusMessage = "Failed to validate Azure Resource Group deployment for UI CDS resources!";
                            },
                        });
                    }

                    Feature uiAzureResources = DataModel.InstallationConfiguration.Features[$"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}"];

                    if (uiAzureResources.ShouldInstall)
                    {
                        SequentialRunner.Operations.Enqueue(new Operation()
                        {
                            Name = "ValidateAzureArmTemplate",
                            OperationFunction = BeginLogicAppValidationAsync,
                            OperationCompletedHandler = (result) =>
                            {
                                bool? response = (bool?)result;

                                WriteArmValidationMessage("UI", SequentialRunner);

                                if (!response.HasValue || !response.Value)
                                {
                                    SequentialRunner.LastOperationStatusCode = -1;
                                }
                            },
                            ValidateFunction = (context) =>
                            {
                                return context.LastOperationStatusCode == 0;
                            },
                            ExceptionHandler = (ex) =>
                            {
                                DataModel.StatusMessage = "Failed to validate Azure Resource Group deployment for UI resources!";
                            },
                        });
                    }

                    break;

                case InstallerActionType.Modify:
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "EnsureResourceGroup",
                        OperationFunction = EnsureResourceGroupAsync,
                        OperationCompletedHandler = (result) =>
                        {
                            if (result == null)
                            {
                                SequentialRunner.LastOperationStatusCode = -1;
                            }
                        },
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to ensure Azure resource group!";
                        },
                    });

                    Feature apiAzureResourcesUpdate = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}"];

                    if (apiAzureResourcesUpdate.ShouldInstall)
                    {
                        SequentialRunner.Operations.Enqueue(new Operation()
                        {
                            Name = "ValidateAzureArmTemplate",
                            OperationFunction = BeginUpdateValidationAsync,
                            OperationCompletedHandler = (result) =>
                            {
                                bool? response = (bool?)result;

                                WriteArmValidationMessage("API", SequentialRunner);

                                if (!response.HasValue || !response.Value)
                                {
                                    SequentialRunner.LastOperationStatusCode = -1;
                                }
                            },
                            ValidateFunction = (context) =>
                            {
                                return context.LastOperationStatusCode == 0;
                            },
                            ExceptionHandler = (ex) =>
                            {
                                DataModel.StatusMessage = "Failed to validate Resource Group deployment for API resource updates!";
                            },
                        });
                    }

                    Feature uiAzureResourcesUpdate = DataModel.InstallationConfiguration.Features[$"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}"];

                    if (uiAzureResourcesUpdate.ShouldInstall)
                    {
                        SequentialRunner.Operations.Enqueue(new Operation()
                        {
                            Name = "ValidateAzureArmTemplate",
                            OperationFunction = BeginLogicAppValidationAsync,
                            OperationCompletedHandler = (result) =>
                            {
                                bool? response = (bool?)result;

                                WriteArmValidationMessage("UI", SequentialRunner);

                                if (!response.HasValue || !response.Value)
                                {
                                    SequentialRunner.LastOperationStatusCode = -1;
                                }
                            },
                            ValidateFunction = (context) =>
                            {
                                return context.LastOperationStatusCode == 0;
                            },
                            ExceptionHandler = (ex) =>
                            {
                                DataModel.StatusMessage = "Failed to validate Azure Resource group deployment for UI resource updates!";
                            },
                        });
                    }

                    break;

                case InstallerActionType.Remove:
                    break;
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

        private void BuildInstallSummary(Paragraph para)
        {
            para.Inlines.Add(new Run("The following Azure resources will be deployed:\n"));

            Feature apiAzureResources = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}"];

            if (apiAzureResources.ShouldInstall)
            {
                para.Inlines.Add(new Run("\n"));
                para.Inlines.Add(new Run("\tAPI:\n"));

                Feature apiAzureResourceGroupDeployment = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.AzureRGDeployment}"];

                if (apiAzureResourceGroupDeployment.ShouldInstall)
                {
                    ArmSummarizer summaryFactory = new ArmSummarizer(AzureConfiguration.AzureArmTemplateFilePath, AzureConfiguration.AzureArmTemplateParametersFilePath);

                    IEnumerable<string> resources = summaryFactory.GetResourceSummary();

                    foreach (string resource in resources)
                    {
                        para.Inlines.Add(new Run($"\t\t{resource}\n"));
                    }

                    para.Inlines.Add(new Run("\n"));
                }
            }

            Feature uiAzureResources = DataModel.InstallationConfiguration.Features[$"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}"];

            if (uiAzureResources.ShouldInstall)
            {
                para.Inlines.Add(new Run("\n"));
                para.Inlines.Add(new Run("\tUI:\n"));

                ArmSummarizer summaryFactory = new ArmSummarizer(LogicAppsConfiguration.LogicAppsArmTemplateFilePath, LogicAppsConfiguration.LogicAppsArmTemplateParametersFilePath);

                IEnumerable<string> resources = summaryFactory.GetResourceSummary();

                foreach (string resource in resources)
                {
                    para.Inlines.Add(new Run($"\t\t{resource}\n"));
                }

                para.Inlines.Add(new Run("\n"));
            }
        }

        private void BuildUpdateSummary(Paragraph para)
        {
            para.Inlines.Add(new Run("The following Azure resources will be updated:\n"));

            Feature apiAzureResources = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}"];

            if (apiAzureResources.ShouldInstall)
            {
                para.Inlines.Add(new Run("\n"));
                para.Inlines.Add(new Run("\tAPI:\n"));

                Feature apiFunctions = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.APIFunctions}"];

                if (apiFunctions.ShouldInstall)
                {
                    para.Inlines.Add(new Run($"\t\tAPI Functions\n"));
                }

                Feature apiLogicApps = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.LogicApps}"];

                if (apiLogicApps.ShouldInstall)
                {
                    ArmSummarizer summaryFactory = new ArmSummarizer(LogicAppsConfiguration.ApiLogicAppsArmTemplateFilePath, LogicAppsConfiguration.ApiLogicAppsArmTemplateParametersFilePath);

                    IEnumerable<string> resources = summaryFactory.GetResourceSummary();

                    foreach (string resource in resources)
                    {
                        para.Inlines.Add(new Run($"\t\t{resource}\n"));
                    }

                    para.Inlines.Add(new Run("\n"));
                }

                Feature apiLuisModel = DataModel.InstallationConfiguration.Features[$"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.AzureLanguageUnderstandingModel}"];

                if (apiLuisModel.ShouldInstall)
                {
                    para.Inlines.Add(new Run($"\t\tLUIS application\n"));
                }
            }

            Feature uiAzureResources = DataModel.InstallationConfiguration.Features[$"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}"];

            if (uiAzureResources.ShouldInstall)
            {
                para.Inlines.Add(new Run("\n"));
                para.Inlines.Add(new Run("\tUI:\n"));

                ArmSummarizer summaryFactory = new ArmSummarizer(LogicAppsConfiguration.LogicAppsArmTemplateFilePath, LogicAppsConfiguration.LogicAppsArmTemplateParametersFilePath);

                IEnumerable<string> resources = summaryFactory.GetResourceSummary();

                foreach (string resource in resources)
                {
                    para.Inlines.Add(new Run($"\t\t{resource}\n"));
                }

                para.Inlines.Add(new Run("\n"));
            }
        }

        private object BeginApiValidationAsync(OperationRunner context)
        {
            if (!Directory.Exists(DataModel.InstallationConfiguration.Azure.ArmTempPath))
            {
                Directory.CreateDirectory(DataModel.InstallationConfiguration.Azure.ArmTempPath);
            }

            _tempArmTemplateFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, AzureConfiguration.AzureArmTemplateFileName);
            _tempArmTemplateParametersFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, AzureConfiguration.AzureArmTemplateParametersFileName);

            File.Copy(
                AzureConfiguration.AzureArmTemplateFilePath,
                _tempArmTemplateFilePath,
                true);

            // TODO: Need to store the secret somewhere if we need to re-read it
            // if (!string.IsNullOrWhiteSpace(DataModel.InstallationStatusCache.Status.FunctionApp.ClientSecret)) DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationSecret = new NetworkCredential("applicationRegistrationSecret", DataModel.InstallationStatusCache.Status.FunctionApp.ClientSecret);
            DataModel.InstallationConfiguration.Azure.LogicApps.AADClientId = DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationId;
            DataModel.InstallationConfiguration.Azure.LogicApps.AADAudience = DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationId;

            DataModel.InstallationConfiguration.Azure.SaveConfiguration(new Dictionary<string, string>()
            {
                { AzureConfiguration.AzureArmTemplateParametersFilePath, _tempArmTemplateParametersFilePath },
            });

            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.ValidateResourceGroupDeploymentAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                "validateAzureDeploy",
                _tempArmTemplateFilePath,
                _tempArmTemplateParametersFilePath).Result;
        }

        private object BeginUpdateValidationAsync(OperationRunner context)
        {
            if (!Directory.Exists(DataModel.InstallationConfiguration.Azure.ArmTempPath))
            {
                Directory.CreateDirectory(DataModel.InstallationConfiguration.Azure.ArmTempPath);
            }

            _tempArmTemplateFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, LogicAppsConfiguration.ApiLogicAppsArmTemplateFileName);
            _tempArmTemplateParametersFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, LogicAppsConfiguration.ApiLogicAppsArmTemplateParametersFileName);

            File.Copy(
                LogicAppsConfiguration.ApiLogicAppsArmTemplateFilePath,
                _tempArmTemplateFilePath,
                true);

            DataModel.InstallationConfiguration.Azure.LogicApps.AADClientId = DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationId;
            DataModel.InstallationConfiguration.Azure.LogicApps.AADAudience = DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationId;

            DataModel.InstallationConfiguration.Azure.SaveConfiguration(new Dictionary<string, string>()
            {
                { LogicAppsConfiguration.ApiLogicAppsArmTemplateParametersFilePath, _tempArmTemplateParametersFilePath },
            });

            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.ValidateResourceGroupDeploymentAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                "validateAzureDeploy",
                _tempArmTemplateFilePath,
                _tempArmTemplateParametersFilePath).Result;
        }

        private object BeginCdsLogicAppValidationAsync(OperationRunner context)
        {
            if (!Directory.Exists(DataModel.InstallationConfiguration.Azure.ArmTempPath))
            {
                Directory.CreateDirectory(DataModel.InstallationConfiguration.Azure.ArmTempPath);
            }

            _tempArmTemplateFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, LogicAppsConfiguration.CdsLogicAppsArmTemplateFileName);
            _tempArmTemplateParametersFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, LogicAppsConfiguration.CdsLogicAppsArmTemplateParametersFileName);

            File.Copy(
                LogicAppsConfiguration.CdsLogicAppsArmTemplateFilePath,
                _tempArmTemplateFilePath,
                true);

            DataModel.InstallationConfiguration.Azure.SaveConfiguration(new Dictionary<string, string>()
            {
                { LogicAppsConfiguration.CdsLogicAppsArmTemplateParametersFilePath, _tempArmTemplateParametersFilePath },
            });

            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.ValidateResourceGroupDeploymentAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                "validateAzureDeploy",
                _tempArmTemplateFilePath,
                _tempArmTemplateParametersFilePath).Result;
        }

        private object BeginLogicAppValidationAsync(OperationRunner context)
        {
            if (!Directory.Exists(DataModel.InstallationConfiguration.Azure.ArmTempPath))
            {
                Directory.CreateDirectory(DataModel.InstallationConfiguration.Azure.ArmTempPath);
            }

            _tempArmTemplateFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, LogicAppsConfiguration.LogicAppsArmTemplateFileName);
            _tempArmTemplateParametersFilePath = System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, LogicAppsConfiguration.LogicAppsArmTemplateParametersFileName);

            File.Copy(
                LogicAppsConfiguration.LogicAppsArmTemplateFilePath,
                _tempArmTemplateFilePath,
                true);

            DataModel.InstallationConfiguration.Azure.SaveConfiguration(new Dictionary<string, string>()
            {
                { LogicAppsConfiguration.LogicAppsArmTemplateParametersFilePath, _tempArmTemplateParametersFilePath },
            });

            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.ValidateResourceGroupDeploymentAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                "validateAzureDeploy",
                _tempArmTemplateFilePath,
                _tempArmTemplateParametersFilePath).Result;
        }

        private void BuildUninstallSummary(Paragraph para)
        {
            if (DataModel.InstallationConfiguration.PowerApps.ShouldDelete)
            {
                para.Inlines.Add(new Run($"The AdvocacyPlatform solution and the Dynamics CRM organization '{DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.OrganizationName}' will be removed.\n"));
                para.Inlines.Add(new Run($"The PowerApps environment '{DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.DisplayName}' will be removed.\n"));
            }

            if (DataModel.InstallationConfiguration.Azure.ShouldDelete)
            {
                para.Inlines.Add(new Run($"The Azure resource group '{DataModel.InstallationConfiguration.Azure.SelectedResourceGroup.Name}' will be removed.\n"));
            }

            if (DataModel.InstallationConfiguration.Azure.ShouldDeleteAppRegistration)
            {
                para.Inlines.Add(new Run($"The Azure AD Application Registration '{DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationName}' will be removed.\n"));
            }

            if (DataModel.InstallationConfiguration.Azure.Luis.ShouldDelete)
            {
                para.Inlines.Add(new Run($"The LUIS application '{DataModel.InstallationConfiguration.Azure.Luis.AppName}' will be removed.\n"));
            }
        }

        private object EnsureResourceGroupAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            bool exists = client.ResourceGroupExistsAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.ResourceGroupName).Result;

            Contracts.ResourceGroup resourceGroup = null;

            if (exists)
            {
                resourceGroup = client.GetResourceGroupAsync(
                    DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                    DataModel.InstallationConfiguration.Azure.ResourceGroupName).Result;

                if (!resourceGroup.Tags.ContainsKey(InstallerModel.ResourceGroupTagKey))
                {
                    resourceGroup.Tags.Add(InstallerModel.ResourceGroupTagKey, InstallerModel.ResourceGroupTagValue);
                }
                else
                {
                    resourceGroup.Tags[InstallerModel.ResourceGroupTagKey] = InstallerModel.ResourceGroupTagValue;
                }
            }
            else
            {
                resourceGroup = new Contracts.ResourceGroup()
                {
                    Name = DataModel.InstallationConfiguration.Azure.ResourceGroupName,
                    Location = DataModel.InstallationConfiguration.Azure.ResourceGroupLocation,
                    Tags = new Dictionary<string, string>()
                    {
                        { InstallerModel.ResourceGroupTagKey, InstallerModel.ResourceGroupTagValue },
                    },
                };
            }

            return client.CreateOrUpdateResourceGroupAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                resourceGroup).Result;
        }
    }
}
