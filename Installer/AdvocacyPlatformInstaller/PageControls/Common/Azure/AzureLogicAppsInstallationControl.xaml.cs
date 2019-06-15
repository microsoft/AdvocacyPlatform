// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
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

    /// <summary>
    /// User control for configuring the Azure Logic Apps resources.
    /// </summary>
    public partial class AzureLogicAppsInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Azure (Logic Apps)";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "Configure the logic apps required by the platform.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureLogicAppsInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public AzureLogicAppsInstallationControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

            // TODO: Data binding isn't working
            WizardProgress.PagesSource = DataModel.Progress;
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
            bool isValid = true;
            string message = null;

            if (string.IsNullOrWhiteSpace(ApiKeyPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.BingMaps.ApiKey.Password))
            {
                message = "No Bing Maps API key specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.LogicApps.ServiceBusConnectionName))
            {
                message = "No connection name specified for the service bus connection!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.LogicApps.CdsConnectionName))
            {
                message = "No connection name specified for the common data services connection!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.LogicApps.BingMapsConnectionName))
            {
                message = "No connection name specified for the Bing Maps API connection!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.LogicApps.RequestWorkflowName))
            {
                message = "No name specified for the request workflow!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.LogicApps.ProcessWorkflowName))
            {
                message = "No name specified for the process workflow!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.LogicApps.NewCaseWorkflowName))
            {
                message = "No name specified for the new case workflow!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.LogicApps.ResultsUpdateCaseWorkflowName))
            {
                message = "No name specified for the update case with results workflow!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.LogicApps.AddressUpdateCaseWorkflowName))
            {
                message = "No name specified for the update case with address workflow!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.LogicApps.GetRetryRecordsWorkflowName))
            {
                message = "No name specified for the retry workflow!";

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

        private void ApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            DataModel.InstallationConfiguration.Azure.BingMaps.ApiKey.SecurePassword = ApiKeyPasswordBox.SecurePassword;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }
    }
}
