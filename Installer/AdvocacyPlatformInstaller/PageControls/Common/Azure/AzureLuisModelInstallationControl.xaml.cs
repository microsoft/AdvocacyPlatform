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
    /// User control for configuring the Azure LUIS Cognitive Services resources.
    /// </summary>
    public partial class AzureLuisModelInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Azure (Language Understanding)";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "Configure the language understanding (LUIS) cognitive service required by the platform.";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureLuisModelInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public AzureLuisModelInstallationControl(
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

            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.Luis.ResourceName))
            {
                message = "No LUIS cognitive services resource name specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(AuthoringKeyPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey.Password))
            {
                message = "No LUIS authoring key specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.Luis.AppName))
            {
                message = "No LUIS application name specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.Luis.AppVersion))
            {
                message = "No LUIS application version specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.Luis.AppFilePath))
            {
                message = "No LUIS application definition file specified!";

                isValid = false;
            }
            else if (!File.Exists(DataModel.InstallationConfiguration.Azure.Luis.AppFilePath))
            {
                message = "The specified LUIS application definition file does not exist!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.Luis.AuthoringRegion))
            {
                message = "No LUIS authoring region specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.Luis.ResourceRegion))
            {
                message = "No LUIS resource region specified!";

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

        private void AuthoringKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            DataModel.InstallationConfiguration.Azure.Luis.AuthoringKey.SecurePassword = AuthoringKeyPasswordBox.SecurePassword;
        }

        private void LuisModelFilePathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();

            ofd.Filter = "zip files (*.zip)|*.zip";

            if (ofd.ShowDialog() == true)
            {
                DataModel.InstallationConfiguration.Azure.Luis.AppFilePath = ofd.FileName;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }
    }
}
