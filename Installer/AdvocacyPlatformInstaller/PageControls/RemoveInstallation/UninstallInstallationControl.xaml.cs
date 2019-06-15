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
    using Contract = AdvocacyPlatformInstaller.Contracts;

    /// <summary>
    /// User control for configuring removal options.
    /// </summary>
    public partial class UninstallInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Uninstall Options";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "Uninstall Advocacy Platform from existing environment";

        private const string _defaultLocation = "unitedstates";
        private const string _defaultSku = "Production";
        private const string _defaultCurrencyCode = "USD";
        private const string _defaultCurrencySymbol = "$";
        private const string _defaultLanguage = "1033";
        private static readonly string _defaultEnvironmentDisplayName = $"AdvocacyPlatform-{Helpers.NewId()}";

        /// <summary>
        /// Initializes a new instance of the <see cref="UninstallInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public UninstallInstallationControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

            LogOutputControl = DetailsRichTextBox;

            DataModel.SuccessFinalStatusMessage = "Advocacy Platform uninstall succeeded.";
            DataModel.FailureFinalStatusMessage = "Advocacy Platform uninstall failed.";

            DataModel.InstallationConfiguration.Azure.PropertyChanged += Azure_PropertyChanged;
            DataModel.InstallationConfiguration.PowerApps.PropertyChanged += PowerApps_PropertyChanged;

            SetOptions();
            SetDefaults();

            // TODO: Data binding isn't working
            WizardProgress.PagesSource = DataModel.Progress;
        }

        /// <summary>
        /// Sets default view model values.
        /// </summary>
        public override void SetDefaults()
        {
            if (DataModel.InstallationConfiguration.Azure.SelectedSubscription != null)
            {
                DataModel.InstallationConfiguration.Azure.SelectedSubscription = DataModel
                    .InstallationConfiguration
                    .Azure
                    .Subscriptions
                    .Where(x => x.Id == DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id)
                    .FirstOrDefault();
            }
            else if (DataModel.InstallationConfiguration.Azure.Subscriptions != null)
            {
                DataModel.InstallationConfiguration.Azure.SelectedSubscription = DataModel
                    .InstallationConfiguration
                    .Azure
                    .Subscriptions
                    .FirstOrDefault();
            }

            if (DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment != null)
            {
                DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment = DataModel
                    .InstallationConfiguration
                    .PowerApps
                    .Environments
                    .Where(x => x.EnvironmentName == DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.EnvironmentName)
                    .FirstOrDefault();
            }
            else if (DataModel.InstallationConfiguration.PowerApps.Environments != null)
            {
                DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment = DataModel
                    .InstallationConfiguration
                    .PowerApps
                    .Environments
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Sets default options in UI.
        /// </summary>
        public override void SetOptions()
        {
            DataModel.InstallationConfiguration.Azure.CheckedForSubscriptions = false;
            if (DataModel.InstallationConfiguration.Azure.Subscriptions == null ||
                DataModel.InstallationConfiguration.Azure.Subscriptions.Count() == 0)
            {
                RunGetAzureSubscriptionsOperationAsync();
            }

            if (DataModel.InstallationConfiguration.PowerApps.Environments == null ||
               DataModel.InstallationConfiguration.PowerApps.Environments.Count() == 0)
            {
                RunGetPowerAppsEnvironmentOperationAsync();
            }
        }

        /// <summary>
        /// Validates selections in UI.
        /// </summary>
        /// <returns>True if valid, false if invalid.</returns>
        public override bool ValidatePage()
        {
            bool isValid = true;
            string message = null;

            if (DataModel.InstallationConfiguration.Azure.ShouldDelete &&
                (DataModel.InstallationConfiguration.Azure.SelectedSubscription == null ||
                 DataModel.InstallationConfiguration.Azure.SelectedResourceGroup == null))
            {
                isValid = false;

                message = "You must specify the Azure resource group to remove and the subscription to remove it from.";
            }
            else if (DataModel.InstallationConfiguration.Azure.ShouldDeleteAppRegistration &&
                (DataModel.InstallationConfiguration.Azure.SelectedSubscription == null ||
                 string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationName)))
            {
                isValid = false;

                message = "You must specify the name of the application registration to remove from Azure AD and the subscription (associated tenant) to remove it from.";
            }
            else if (DataModel.InstallationConfiguration.Azure.Luis.ShouldDelete &&
                     (string.IsNullOrEmpty(DataModel.InstallationConfiguration.Azure.Luis.AppName) ||
                      string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion) ||
                      string.IsNullOrWhiteSpace(AuthoringKeyPasswordBox.Password) ||
                      string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey.Password)))
            {
                isValid = false;

                message = "You must specify the name of the LUIS application to remove, the authoring region, and authoring key.";
            }
            else if (DataModel.InstallationConfiguration.PowerApps.ShouldDelete &&
                     (DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment == null ||
                      string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.OrganizationName)))
            {
                isValid = false;

                if (DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment == null)
                {
                    message = "You must specify the PowerApps environment to remove.";
                }
                else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.OrganizationName))
                {
                    message = "There was no unique Dynamics CRM organization name associated with the selected environment. Please refresh the list and reselect.";
                }
            }
            else if (!DataModel.InstallationConfiguration.Azure.ShouldDelete &&
                !DataModel.InstallationConfiguration.Azure.ShouldDeleteAppRegistration &&
                !DataModel.InstallationConfiguration.Azure.Luis.ShouldDelete &&
                !DataModel.InstallationConfiguration.PowerApps.ShouldDelete)
            {
                isValid = false;

                message = "You must specify at least one feature to uninstall.";
            }

            if (!isValid)
            {
                DataModel.ShowStatus = true;
                DataModel.StatusMessage = message;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DataModel.StatusMessageBgColor = new SolidColorBrush(Colors.Black);
                    DataModel.StatusMessageFgColor = new SolidColorBrush(Colors.Yellow);
                }));
            }
            else
            {
                DataModel.ShowStatus = false;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DataModel.StatusMessageBgColor = (SolidColorBrush)Background;
                    DataModel.StatusMessageFgColor = (SolidColorBrush)Foreground;
                }));
            }

            return isValid;
        }

        private void PowerApps_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.Compare(e.PropertyName, "SelectedEnvironment") == 0
                && DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment != null)
            {
                RunCheckDynamics365CRMSolutionAsync();
            }
            else if (string.Compare(e.PropertyName, "ShouldDelete") == 0)
            {
                if (!DataModel.InstallationConfiguration.PowerApps.ShouldDelete)
                {
                    SetSuccessState();
                }
            }
        }

        private void Azure_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (string.Compare(e.PropertyName, "SelectedSubscription") == 0
                && DataModel.InstallationConfiguration.Azure.SelectedSubscription != null)
            {
                RunGetAzureResourceGroupsOperationAsync();
            }
        }

        private void RunGetPowerAppsEnvironmentOperationAsync()
        {
            Task.Run(() =>
            {
                OperationRunner singleRunner = new OperationRunner(
                    null,
                    this,
                    WizardContext.LogFileStream);
                singleRunner.IndeterminateOps = true;
                singleRunner.OnLog += WriteLog;

                singleRunner.Operations.Enqueue(new Operation()
                {
                    Name = "GetPowerAppsEnvironments",
                    OperationFunction = GetPowerAppsEnvironmentsAsync,
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    OperationCompletedHandler = (result) =>
                    {
                        GetPowerAppsEnvironmentsResponse response = (GetPowerAppsEnvironmentsResponse)result;

                        DataModel.InstallationConfiguration.PowerApps.Environments = response
                            .Value
                            .Select(x => new PowerAppsEnvironment()
                            {
                                DisplayName = x.Properties.DisplayName,
                                EnvironmentName = x.Name,
                                OrganizationName = x.Properties.LinkedEnvironmentMetadata != null ? x.Properties.LinkedEnvironmentMetadata.UniqueName : null,
                                OrganizationDomainName = x.Properties.LinkedEnvironmentMetadata != null ? x.Properties.LinkedEnvironmentMetadata.DomainName : null,
                                WebApplicationUrl = x.Properties.LinkedEnvironmentMetadata != null ? x.Properties.LinkedEnvironmentMetadata.InstanceUrl : null,
                            }).ToList();

                        PowerAppsEnvironment defaultEnvironment = DataModel
                            .InstallationConfiguration
                            .PowerApps
                            .Environments
                            .Where(x => DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment != null &&
                                string.Compare(x.EnvironmentName, DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.EnvironmentName, true) == 0)
                            .FirstOrDefault();

                        DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment =
                            defaultEnvironment ??
                                DataModel
                                    .InstallationConfiguration
                                    .PowerApps
                                    .Environments
                                    .FirstOrDefault();
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to acquire PowerApps environments!";
                    },
                });

                singleRunner.RunOperations();
            });
        }

        private void RunGetAzureSubscriptionsOperationAsync()
        {
            Task.Run(() =>
            {
                OperationRunner singleRunner = new OperationRunner(
                    null,
                    this,
                    WizardContext.LogFileStream);
                singleRunner.IndeterminateOps = true;
                singleRunner.OnLog += WriteLog;

                singleRunner.Operations.Enqueue(new Operation()
                {
                    Name = "GetAzureSubscriptions",
                    OperationFunction = GetAzureSubscriptionsAsync,
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    OperationCompletedHandler = (result) =>
                    {
                        Subscription[] response = (Subscription[])result;

                        DataModel.InstallationConfiguration.Azure.CheckedForSubscriptions = true;

                        DataModel.InstallationConfiguration.Azure.Subscriptions = response
                            .Select(x => new AzureSubscription()
                            {
                                Id = x.SubscriptionId,
                                Name = x.DisplayName,
                            });

                        AzureSubscription defaultSubscription = DataModel
                            .InstallationConfiguration
                            .Azure
                            .Subscriptions
                            .Where(x => DataModel.InstallationConfiguration.Azure.SelectedSubscription != null &&
                                string.Compare(x.Id, DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id, true) == 0)
                            .FirstOrDefault();

                        DataModel.InstallationConfiguration.Azure.SelectedSubscription =
                            defaultSubscription ??
                                    DataModel
                                        .InstallationConfiguration
                                        .Azure
                                        .Subscriptions
                                        .FirstOrDefault();
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to acquire Azure subscriptions!";
                    },
                });
                singleRunner.RunOperations();
            });
        }

        private void RunCheckDynamics365CRMSolutionAsync()
        {
            Task.Run(() =>
            {
                OperationRunner singleRunner = new OperationRunner(
                    null,
                    this,
                    WizardContext.LogFileStream);
                singleRunner.IndeterminateOps = true;
                singleRunner.OnLog += WriteLog;

                singleRunner.Operations.Enqueue(new Operation()
                {
                    Name = "CheckDynamics365CRMSolution",
                    OperationFunction = CheckDynamics365CRMSolutionAsync,
                    OperationCompletedHandler = (result) =>
                    {
                        if (result == null)
                        {
                            DataModel.InstallationConfiguration.PowerApps.Environments.Remove(
                                DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment);
                            DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment = null;

                            throw new Exception($"Could not find {DataModel.InstallationConfiguration.DynamicsCrm.SolutionUniqueName} solution in selected PowerApps environment!");
                        }
                    },
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = ex.Message;
                    },
                });
                singleRunner.RunOperations();
            });
        }

        private void RunGetAzureResourceGroupsOperationAsync()
        {
            Task.Run(() =>
            {
                OperationRunner singleRunner = new OperationRunner(
                    null,
                    this,
                    WizardContext.LogFileStream);
                singleRunner.IndeterminateOps = true;
                singleRunner.OnLog += WriteLog;

                singleRunner.Operations.Enqueue(new Operation()
                {
                    Name = "GetAzureResourceGroups",
                    OperationFunction = GetAzureResourceGroupsAsync,
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    OperationCompletedHandler = (result) =>
                    {
                        Contract.ResourceGroup[] response = (Contract.ResourceGroup[])result;

                        DataModel.InstallationConfiguration.Azure.ResourceGroups = response
                            .Where(x => x.Tags != null &&
                                x.Tags.ContainsKey(InstallerModel.ResourceGroupTagKey))
                            .Select(x => new AzureResourceGroup()
                            {
                                Name = x.Name,
                                Location = x.Location,
                                Tags = x.Tags,
                            });

                        AzureResourceGroup defaultResourceGroup = DataModel
                            .InstallationConfiguration
                            .Azure
                            .ResourceGroups
                            .Where(x => string.Compare(x.Name, DataModel.InstallationConfiguration.Azure.ResourceGroupName, StringComparison.Ordinal) == 0)
                            .FirstOrDefault();

                        DataModel.InstallationConfiguration.Azure.SelectedResourceGroup =
                            defaultResourceGroup ??
                                    DataModel
                                        .InstallationConfiguration
                                        .Azure
                                        .ResourceGroups
                                        .FirstOrDefault();
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to acquire Azure Resource Groups!";
                    },
                });

                singleRunner.RunOperations();
            });
        }

        private object GetAzureSubscriptionsAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.GetSubscriptionsAsync().Result;
        }

        private object GetAzureResourceGroupsAsync(OperationRunner context)
        {
            AzureClient client = new AzureClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.GetResourceGroupsAsync(DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id).Result;
        }

        private object CheckDynamics365CRMSolutionAsync(OperationRunner context)
        {
            DynamicsCrmClient client = new DynamicsCrmClient(
                DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.OrganizationName,
                DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.OrganizationDomainName,
                WizardContext.TokenProvider);

            return client.GetSolutionAsync(DataModel.InstallationConfiguration.DynamicsCrm.SolutionUniqueName).Result;
        }

        private object GetPowerAppsEnvironmentsAsync(OperationRunner context)
        {
            PowerAppsClient client = new PowerAppsClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.GetEnvironmentsAsync().Result;
        }

        private void RefreshAzureSubscriptionsButton_Click(object sender, RoutedEventArgs e)
        {
            RunGetAzureSubscriptionsOperationAsync();
        }

        private void RefreshAzureResourceGroupsButton_Click(object sender, RoutedEventArgs e)
        {
            RunGetAzureResourceGroupsOperationAsync();
        }

        private void GetEnvironmentsButton_Click(object sender, RoutedEventArgs e)
        {
            RunGetPowerAppsEnvironmentOperationAsync();
        }

        private void AuthoringKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey.SecurePassword = AuthoringKeyPasswordBox.SecurePassword;
        }
    }
}
