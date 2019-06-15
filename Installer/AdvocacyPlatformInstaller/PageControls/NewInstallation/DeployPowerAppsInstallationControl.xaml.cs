// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    using Contract = AdvocacyPlatformInstaller.Contracts;
    using TH = System.Threading;

    /// <summary>
    /// User control for deploying the PowerApps resources.
    /// </summary>
    public partial class DeployPowerAppsInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Power Apps Deployment";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "The PowerApps environment and Common Data Services database is being provisioned.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DeployPowerAppsInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public DeployPowerAppsInstallationControl(
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
            Dispatcher.BeginInvoke(new Action(() =>
            {
                WizardContext.NextPage();
            }));
        }

        private void QueueOperations()
        {
            Feature powerAppsEnvironment = DataModel.InstallationConfiguration.Features[$"{FeatureNames.UIComponents}\\{FeatureNames.PowerAppsDynamics365CRM}\\{FeatureNames.PowerAppsEnvironment}"];

            if (powerAppsEnvironment.ShouldInstall)
            {
                SequentialRunner.Operations.Enqueue(new Operation()
                {
                    Name = "CreatePowerAppsEnvironment",
                    OperationFunction = CreatePowerAppsEnvironmentAsync,
                    OperationCompletedHandler = (result) =>
                    {
                        CreatePowerAppsEnvironmentResponse response = (CreatePowerAppsEnvironmentResponse)result;

                        if (response != null)
                        {
                            DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment = new PowerAppsEnvironment()
                            {
                                DisplayName = response.Properties.DisplayName,
                                EnvironmentName = response.Name,
                            };
                        }
                    },
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to create PowerApps environment!";
                    },
                });
            }

            Feature powerAppsCdsDatabase = DataModel.InstallationConfiguration.Features[$"{FeatureNames.UIComponents}\\{FeatureNames.PowerAppsDynamics365CRM}\\{FeatureNames.CommonDataServiceDatabase}"];

            if (powerAppsCdsDatabase.ShouldInstall)
            {
                SequentialRunner.Operations.Enqueue(new Operation()
                {
                    Name = "CreatePowerAppsCdsDatabase",
                    OperationFunction = CreatePowerAppsCdsDatabaseAsync,
                    OperationCompletedHandler = (result) =>
                    {
                        Contract.PowerAppsEnvironment response = (Contract.PowerAppsEnvironment)result;

                        if (response != null)
                        {
                            DataModel.InstallationConfiguration.PowerApps.Environments = new List<PowerAppsEnvironment>();

                            DataModel.InstallationConfiguration.PowerApps.Environments.Add(new PowerAppsEnvironment()
                            {
                                EnvironmentName = response.Name,
                                DisplayName = response.Properties.DisplayName,
                                OrganizationName = response.Properties.LinkedEnvironmentMetadata.UniqueName,
                                OrganizationDomainName = response.Properties.LinkedEnvironmentMetadata.DomainName,
                                WebApplicationUrl = response.Properties.LinkedEnvironmentMetadata.InstanceUrl,
                            });
                        }
                    },
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to deploy PowerApps CDS database!";
                    },
                });
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

        private object CreatePowerAppsEnvironmentAsync(OperationRunner context)
        {
            PowerAppsClient client = new PowerAppsClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.CreateEnvironmentAsync(new CreatePowerAppsEnvironmentRequest()
            {
                Location = DataModel.InstallationConfiguration.PowerApps.SelectedLocation,
                Properties = new NewPowerAppsEnvironmentProperties()
                {
                    DisplayName = DataModel.InstallationConfiguration.PowerApps.EnvironmentDisplayName,
                    EnvironmentSku = DataModel.InstallationConfiguration.PowerApps.SelectedSku,
                },
            }).Result;
        }

        private object CreatePowerAppsCdsDatabaseAsync(OperationRunner context)
        {
            PowerAppsClient client = new PowerAppsClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.CreateCdsDatabase(
                DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.EnvironmentName,
                new CreatePowerAppsCdsDatabaseRequest()
                {
                    BaseLanguage = DataModel.InstallationConfiguration.PowerApps.SelectedLanguage.LanguageName,
                    Currency = new PowerAppsCdsDatabaseCurrencyMinimal()
                    {
                        Code = DataModel.InstallationConfiguration.PowerApps.SelectedCurrency.CurrencyName,
                    },
                });
        }
    }
}
