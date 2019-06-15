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
    using Client = AdvocacyPlatformInstaller.Clients;

    /// <summary>
    /// User control for resource removal.
    /// </summary>
    public partial class UninstallRemoveInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Uninstall";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "Remove Advocacy Platform resources.";

        /// <summary>
        /// Initializes a new instance of the <see cref="UninstallRemoveInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public UninstallRemoveInstallationControl(
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

            DataModel.SuccessFinalStatusMessage = "Advocacy Platform uninstall succeeded.";
            DataModel.FailureFinalStatusMessage = "Advocacy Platform uninstall failed.";

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
            if (installConfig.Azure.ShouldDelete)
            {
                SequentialRunner.Operations.Enqueue(new Operation()
                {
                    Name = "RemoveAzureResourceGroup",
                    OperationFunction = RemoveAzureResourceGroupAsync,
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to remove Azure Resource Group!";
                    },
                });
            }

            if (installConfig.Azure.ShouldDeleteAppRegistration)
            {
                SequentialRunner.Operations.Enqueue(new Operation()
                {
                    Name = "RemoveApplicationRegistration",
                    OperationFunction = RemoveApplicationRegistrationAsync,
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to remove registered application from Azure AD!";
                    },
                });
            }

            if (installConfig.Azure.Luis.ShouldDelete)
            {
                SequentialRunner.Operations.Enqueue(new Operation()
                {
                    Name = "RemoveLuisApplication",
                    OperationFunction = RemoveLuisApplicationAsync,
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to remove LUIS application!";
                    },
                });
            }

            if (installConfig.PowerApps.ShouldDelete)
            {
                if (installConfig.PowerApps.ShouldOnlyDeleteSolution)
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "RemoveDynamicsCRMSolution",
                        OperationFunction = RemoveDynamicsCRMSolutionAsync,
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to remove Dynamics 365 CRM solution!";
                        },
                    });
                }
                else
                {
                    SequentialRunner.Operations.Enqueue(new Operation()
                    {
                        Name = "RemovePowerAppsEnvironment",
                        OperationFunction = RemovePowerAppsEnvironmentAsync,
                        ValidateFunction = (context) =>
                        {
                            return context.LastOperationStatusCode == 0;
                        },
                        ExceptionHandler = (ex) =>
                        {
                            DataModel.StatusMessage = "Failed to remove PowerApps environment!";
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

        private object RemoveAzureResourceGroupAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            AzureValueCollectionResponse<ResourceLock> resourceLocks = client.GetResourceGroupLocksAsync(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.SelectedResourceGroup.Name).Result;

            foreach (ResourceLock resourceLock in resourceLocks.Value)
            {
                string result = client.DeleteResourceLockAsync(resourceLock.Id).Result;
            }

            return client.DeleteResourceGroup(
                DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id,
                DataModel.InstallationConfiguration.Azure.SelectedResourceGroup.Name);
        }

        private object RemoveApplicationRegistrationAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            AzureApplication application = client.GetApplicationAsync(DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationName).Result;

            if (application == null)
            {
                return true;
            }

            return client.DeleteApplicationAsync(application.Id).Result;
        }

        private object RemoveLuisApplicationAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            LuisApplication application = client.GetLuisAppByNameAsync(
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey,
                DataModel.InstallationConfiguration.Azure.Luis.AppName,
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion).Result;

            if (application == null)
            {
                return true;
            }

            return client.DeleteLuisAppAsync(
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey,
                application.Id,
                DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion).Result;
        }

        private object RemoveDynamicsCRMSolutionAsync(OperationRunner context)
        {
            DynamicsCrmClient client = new DynamicsCrmClient(
                   DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.OrganizationName,
                   DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.OrganizationDomainName,
                   WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.DeleteSolutionAsync(DataModel.InstallationConfiguration.DynamicsCrm.SolutionUniqueName).Result;
        }

        private object RemovePowerAppsEnvironmentAsync(OperationRunner context)
        {
            PowerAppsClient client = new PowerAppsClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.DeleteEnvironmentAsync(DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.EnvironmentName).Result;
        }
    }
}
