// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Contract = AdvocacyPlatformInstaller.Contracts;

    /// <summary>
    /// User control for configuring the Azure resources.
    /// </summary>
    public partial class AzureInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Azure";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "Select an Azure subscription to deploy resources to.";

        private static readonly string _defaultResourceGroupName = $"ap-{Helpers.NewId()}-wu-rg";
        private static readonly Regex _resourceGroupRegex = new Regex("^[-\\w\\._\\(\\)]+$", RegexOptions.Compiled);
        private static readonly Regex _storageAccountRegex = new Regex("^[a-zA-Z0-9]*$", RegexOptions.Compiled);
        private static readonly Regex _serviceBusRegex = new Regex("^[a-zA-Z][a-zA-Z0-9-]*$", RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public AzureInstallationControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

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
            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.ResourceGroupName))
            {
                DataModel.InstallationConfiguration.Azure.ResourceGroupName = _defaultResourceGroupName;
            }

            if (DataModel.InstallationConfiguration.Azure.SelectedSubscription != null)
            {
                DataModel.InstallationConfiguration.Azure.SelectedSubscription = DataModel.InstallationConfiguration.Azure.Subscriptions
                    .Where(x => x.Id == DataModel.InstallationConfiguration.Azure.SelectedSubscription.Id)
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
                RunGetAzureSubscriptionsOperation();
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

            if (DataModel.InstallationConfiguration.Azure.SelectedSubscription == null)
            {
                message = "No Azure Subscription selected!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.ResourceGroupName))
            {
                message = "No Azure Resource Group name specified!";

                isValid = false;
            }
            else if (!_resourceGroupRegex.IsMatch(DataModel.InstallationConfiguration.Azure.ResourceGroupName))
            {
                message = "Invalid Azure Resource Group name specified!";

                isValid = false;
            }
            else if (DataModel.InstallationConfiguration.Azure.ResourceGroupName.Length > 90)
            {
                message = "Invalid Azure Resource Group name specified! Length must be less than 90 characters.";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.StorageAccount.StorageAccountName))
            {
                message = "No Azure Storage Account name specified!";

                isValid = false;
            }
            else if (!_storageAccountRegex.IsMatch(DataModel.InstallationConfiguration.Azure.StorageAccount.StorageAccountName))
            {
                message = "Invalid Azure Storage Account name specified! Only alphanumeric lowercase characters allowed.";

                isValid = false;
            }
            else if (DataModel.InstallationConfiguration.Azure.StorageAccount.StorageAccountName.Length < 3)
            {
                message = "Invalid Azure Storage Account name specified! Length must be at least 3 characters.";

                isValid = false;
            }
            else if (DataModel.InstallationConfiguration.Azure.StorageAccount.StorageAccountName.Length > 24)
            {
                message = "Invalid Azure Storage Account name specified! Length must be less than 24 characters.";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.ServiceBus.ServiceBusNamespaceName))
            {
                message = "No Azure Service Bus name specified!";

                isValid = false;
            }
            else if (!_serviceBusRegex.IsMatch(DataModel.InstallationConfiguration.Azure.ServiceBus.ServiceBusNamespaceName))
            {
                message = "Invalid Azure Service Bus name specified! Must start with a letter and only alphanumeric and hyphen characters allowed.";

                isValid = false;
            }
            else if (DataModel.InstallationConfiguration.Azure.ServiceBus.ServiceBusNamespaceName.EndsWith("-") ||
                DataModel.InstallationConfiguration.Azure.ServiceBus.ServiceBusNamespaceName.EndsWith("-sb") ||
                DataModel.InstallationConfiguration.Azure.ServiceBus.ServiceBusNamespaceName.EndsWith("-mgmt"))
            {
                message = "Invalid Azure Service Bus name specified! Name cannot end with -, -sb, or -mgmt.";

                isValid = false;
            }
            else if (DataModel.InstallationConfiguration.Azure.ServiceBus.ServiceBusNamespaceName.Length < 6)
            {
                message = "Invalid Azure Service Bus name specified! Length must be at least 6 characters.";

                isValid = false;
            }
            else if (DataModel.InstallationConfiguration.Azure.ServiceBus.ServiceBusNamespaceName.Length > 50)
            {
                message = "Invalid Azure Service Bus name specified! Length must be less than 50 characters.";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.SpeechCognitiveService.SpeechResourceName))
            {
                message = "No Azure Speech Cognitive Service resource name specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.Management.LogAnalyticsName))
            {
                message = "No Azure Log Analytics resource name specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.Management.AppInsightsName))
            {
                message = "No Azure Application Insights resource name specified!";

                isValid = false;
            }

            if (!isValid)
            {
                DataModel.ShowStatus = true;
                DataModel.StatusMessage = message;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DataModel.StatusMessageBgColor = new SolidColorBrush(Colors.Black);
                    DataModel.StatusMessageFgColor = new SolidColorBrush(Colors.LightPink);
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

        private void RunGetAzureSubscriptionsOperation()
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
                        Contract.Subscription[] response = (Contract.Subscription[])result;

                        DataModel.InstallationConfiguration.Azure.CheckedForSubscriptions = true;

                        DataModel.InstallationConfiguration.Azure.Subscriptions = response
                            .Select(x => new AzureSubscription()
                            {
                                Id = x.SubscriptionId,
                                Name = x.DisplayName,
                            });

                        // Threading is causing issues
                        // string defaultLocation = DataModel.InstallationConfiguration.PowerApps.Locations.Where(x => string.Compare(x, _defaultLocation, StringComparison.Ordinal) == 0).FirstOrDefault();

                        // DataModel.InstallationConfiguration.PowerApps.SelectedLocation =
                        //    (defaultLocation != null ?
                        //        defaultLocation :
                        //            DataModel.InstallationConfiguration.PowerApps.Locations.FirstOrDefault());
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to acquire Azure subscriptions!";
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

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }

        private void RefreshAzureSubscriptionsButton_Click(object sender, RoutedEventArgs e)
        {
            RunGetAzureSubscriptionsOperation();
        }
    }
}
