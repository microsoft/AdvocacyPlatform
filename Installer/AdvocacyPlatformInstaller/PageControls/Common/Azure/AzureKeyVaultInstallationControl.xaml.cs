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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// User control for configuring the Azure Key Vault resources.
    /// </summary>
    public partial class AzureKeyVaultInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Azure (Key Vault)";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "Configure the key vault and secrets required by the platform.";

        private static readonly string _defaultKeyVaultName = $"ap-{Helpers.NewId()}-wu-keyvault";
        private static readonly string _defaultAADKeySecretName = $"ap-{Helpers.NewId()}-func-aad-key";
        private static readonly string _defaultAADNameSecretName = $"ap-{Helpers.NewId()}-func-aad-name";
        private static readonly string _defaultLuisSubscriptionKeySecretName = $"ap-{Helpers.NewId()}-luis-subscription-key";
        private static readonly string _defaultSpeechApiKeySecretName = $"ap-{Helpers.NewId()}-speech-api-key";
        private static readonly string _defaultStorageAccessKeySecretName = $"ap-{Helpers.NewId()}-storage-access-key";
        private static readonly string _defaultStorageReadAccessKeySecretName = $"ap-{Helpers.NewId()}-storage-read-access-key";
        private static readonly string _defaultTwilioAccountPhoneNumberSecretName = $"ap-{Helpers.NewId()}-twilio-account-phonenumber";
        private static readonly string _defaultTwilioAccountSsidSecretName = $"ap-{Helpers.NewId()}-twilio-account-ssid";
        private static readonly string _defaultTwilioAccountTokenSecretName = $"ap-{Helpers.NewId()}-twilio-account-token";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureKeyVaultInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public AzureKeyVaultInstallationControl(
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

            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.KeyVaultName))
            {
                message = "No Azure Key Vault name specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.FunctionAppClientIdSecretName))
            {
                message = "No secret name specified for the function app application registration id!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.FunctionAppClientPasswordSecretName))
            {
                message = "No secret name specified for the function app application registration client secret!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.StorageAccessKeySecretName))
            {
                message = "No secret name specified for the storage access key!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.StorageReadAccessKeySecretName))
            {
                message = "No secret name specified for the storage read access key!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.LuisSubscriptionKeySecretName))
            {
                message = "No secret name specified for the LUIS subscription key!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.SpeechApiKeySecretName))
            {
                message = "No secret name specified for the speech cognitive services API key!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountSsidSecretName))
            {
                message = "No secret name specified for the Twilio Account SSID!";

                isValid = false;
            }
            else if (DataModel.InstallationConfiguration.Azure.KeyVault.CollectSecrets &&
                     (string.IsNullOrWhiteSpace(TwilioAccountSsidPasswordBox.Password) ||
                      DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountSsidSecretValue == null ||
                      string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountSsidSecretValue.Password)))
            {
                message = "No secret value specified for the Twilio Account SSID!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountTokenSecretName))
            {
                message = "No secret name specified for the Twilio Account authentication token!";

                isValid = false;
            }
            else if (DataModel.InstallationConfiguration.Azure.KeyVault.CollectSecrets &&
                     (string.IsNullOrWhiteSpace(TwilioAccountTokenPasswordBox.Password) ||
                      DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountTokenSecretValue == null ||
                      string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountTokenSecretValue.Password)))
            {
                message = "No secret value specified for the Twilio Account authentication token!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountPhoneNumberSecretName))
            {
                message = "No secret name specified for the Twilio local phone number!";

                isValid = false;
            }
            else if (DataModel.InstallationConfiguration.Azure.KeyVault.CollectSecrets &&
                     (string.IsNullOrWhiteSpace(TwilioAccountPhoneNumberPasswordBox.Password) ||
                      DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountPhoneNumberSecretValue == null ||
                      string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountPhoneNumberSecretValue.Password)))
            {
                message = "No secret value specified for the Twilio local phone number!";

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

        private void TwilioAccountTokenPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountTokenSecretValue.SecurePassword = TwilioAccountTokenPasswordBox.SecurePassword;
        }

        private void TwilioAccountSsidPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountSsidSecretValue.SecurePassword = TwilioAccountSsidPasswordBox.SecurePassword;
        }

        private void TwilioAccountPhoneNumberPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            DataModel.InstallationConfiguration.Azure.KeyVault.TwilioAccountPhoneNumberSecretValue.SecurePassword = TwilioAccountPhoneNumberPasswordBox.SecurePassword;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }
    }
}
