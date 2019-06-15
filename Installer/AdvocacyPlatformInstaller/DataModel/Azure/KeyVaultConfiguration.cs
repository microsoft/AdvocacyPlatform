// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Configuration for the Azure Key Vault resource.
    /// </summary>
    public class KeyVaultConfiguration : NotifyPropertyChangedBase
    {
        private string _keyVaultName;
        private string _functionAppClientIdSecretName;
        private string _functionAppClientPasswordSecretName;
        private string _luisSubscriptionKeySecretName;
        private string _speechApiKeySecretName;
        private string _storageAccessKeySecretName;
        private string _storageReadAccessKeySecretName;
        private string _twilioAccountPhoneNumberSecretName;
        private NetworkCredential _twilioAccountPhoneNumberSecretValue;
        private string _twilioAccountSsidSecretName;
        private NetworkCredential _twilioAccountSsidSecretValue;
        private string _twilioAccountTokenSecretName;
        private NetworkCredential _twilioAccountTokenSecretValue;
        private bool _collectSecrets;

        /// <summary>
        /// Gets or sets the name of the Azure Key Vault resource.
        /// </summary>
        public string KeyVaultName
        {
            get => _keyVaultName;
            set
            {
                _keyVaultName = value;

                NotifyPropertyChanged("KeyVaultName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the secret for the Function App service principal.
        /// </summary>
        public string FunctionAppClientIdSecretName
        {
            get => _functionAppClientIdSecretName;
            set
            {
                _functionAppClientIdSecretName = value;

                NotifyPropertyChanged("FunctionAppClientIdSecretName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the secret for the Function App service principal client secret.
        /// </summary>
        public string FunctionAppClientPasswordSecretName
        {
            get => _functionAppClientPasswordSecretName;
            set
            {
                _functionAppClientPasswordSecretName = value;

                NotifyPropertyChanged("FunctionAppClientPasswordSecretName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the secret for the LUIS subscription key.
        /// </summary>
        public string LuisSubscriptionKeySecretName
        {
            get => _luisSubscriptionKeySecretName;
            set
            {
                _luisSubscriptionKeySecretName = value;

                NotifyPropertyChanged("LuisSubscriptionKeySecretName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the secret for the Azure Speech Cognitive Service API key.
        /// </summary>
        public string SpeechApiKeySecretName
        {
            get => _speechApiKeySecretName;
            set
            {
                _speechApiKeySecretName = value;

                NotifyPropertyChanged("SpeechApiKeySecretName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the secret for the Azure Storage read-write shared access signature.
        /// </summary>
        public string StorageAccessKeySecretName
        {
            get => _storageAccessKeySecretName;
            set
            {
                _storageAccessKeySecretName = value;

                NotifyPropertyChanged("StorageAccessKeySecretName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the secret for the Azure Storage read shared access signature.
        /// </summary>
        public string StorageReadAccessKeySecretName
        {
            get => _storageReadAccessKeySecretName;
            set
            {
                _storageReadAccessKeySecretName = value;

                NotifyPropertyChanged("StorageReadAccessKeySecretName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the secret for the Twilio account phone number.
        /// </summary>
        public string TwilioAccountPhoneNumberSecretName
        {
            get => _twilioAccountPhoneNumberSecretName;
            set
            {
                _twilioAccountPhoneNumberSecretName = value;

                NotifyPropertyChanged("TwilioAccountPhoneNumberSecretName");
            }
        }

        /// <summary>
        /// Gets or sets the value of the secret for the Twilio account phone number.
        /// </summary>
        [JsonIgnore]
        public NetworkCredential TwilioAccountPhoneNumberSecretValue
        {
            get => _twilioAccountPhoneNumberSecretValue;
            set
            {
                _twilioAccountPhoneNumberSecretValue = value;

                NotifyPropertyChanged("TwilioAccountPhoneNumberSecretValue");
            }
        }

        /// <summary>
        /// Gets or sets the name of the secret for the Twilio account SSID.
        /// </summary>
        public string TwilioAccountSsidSecretName
        {
            get => _twilioAccountSsidSecretName;
            set
            {
                _twilioAccountSsidSecretName = value;

                NotifyPropertyChanged("TwilioAccountSsidSecretName");
            }
        }

        /// <summary>
        /// Gets or sets the value of the secret for the Twilio account SSID.
        /// </summary>
        [JsonIgnore]
        public NetworkCredential TwilioAccountSsidSecretValue
        {
            get => _twilioAccountSsidSecretValue;
            set
            {
                _twilioAccountSsidSecretValue = value;

                NotifyPropertyChanged("TwilioAccountSsidSecretValue");
            }
        }

        /// <summary>
        /// Gets or sets the name of the secret for the Twilio account token.
        /// </summary>
        public string TwilioAccountTokenSecretName
        {
            get => _twilioAccountTokenSecretName;
            set
            {
                _twilioAccountTokenSecretName = value;

                NotifyPropertyChanged("TwilioAccountTokenSecretName");
            }
        }

        /// <summary>
        /// Gets or sets the value of the secret for the Twilio account token.
        /// </summary>
        [JsonIgnore]
        public NetworkCredential TwilioAccountTokenSecretValue
        {
            get => _twilioAccountTokenSecretValue;
            set
            {
                _twilioAccountTokenSecretValue = value;

                NotifyPropertyChanged("TwilioAccountTokenSecretValue");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether secrets should be collected in the UI.
        /// </summary>
        [JsonIgnore]
        public bool CollectSecrets
        {
            get => _collectSecrets;
            set
            {
                _collectSecrets = value;

                NotifyPropertyChanged("CollectSecrets");
            }
        }

        /// <summary>
        /// Loads configuration from a file.
        /// </summary>
        /// <param name="armTemplateFilePath">Path to the ARM template file path to load defaults from.</param>
        public void LoadConfiguration(string armTemplateFilePath)
        {
            KeyVaultName = string.IsNullOrWhiteSpace(KeyVaultName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "vaults_ap_wu_keyvault_name") : KeyVaultName;
            FunctionAppClientIdSecretName = string.IsNullOrWhiteSpace(FunctionAppClientIdSecretName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "ap_func_aad_name_secret_name") : FunctionAppClientIdSecretName;
            FunctionAppClientPasswordSecretName = string.IsNullOrWhiteSpace(FunctionAppClientPasswordSecretName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "ap_func_aad_key_secret_name") : FunctionAppClientPasswordSecretName;
            LuisSubscriptionKeySecretName = string.IsNullOrWhiteSpace(LuisSubscriptionKeySecretName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "luis_subscription_key_secret_name") : LuisSubscriptionKeySecretName;
            SpeechApiKeySecretName = string.IsNullOrWhiteSpace(SpeechApiKeySecretName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "speech_api_key_secret_name") : SpeechApiKeySecretName;
            StorageAccessKeySecretName = string.IsNullOrWhiteSpace(StorageAccessKeySecretName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "storage_access_key_secret_name") : StorageAccessKeySecretName;
            StorageReadAccessKeySecretName = string.IsNullOrWhiteSpace(StorageReadAccessKeySecretName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "storage_read_access_key_secret_name") : StorageReadAccessKeySecretName;
            TwilioAccountPhoneNumberSecretName = string.IsNullOrWhiteSpace(TwilioAccountPhoneNumberSecretName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "twilioaccountphonenumber_secret_name") : TwilioAccountPhoneNumberSecretName;
            TwilioAccountSsidSecretName = string.IsNullOrWhiteSpace(TwilioAccountSsidSecretName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "twilioaccountssid_secret_name") : TwilioAccountSsidSecretName;
            TwilioAccountTokenSecretName = string.IsNullOrWhiteSpace(TwilioAccountTokenSecretName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "twilioaccounttoken_secret_name") : TwilioAccountTokenSecretName;

            TwilioAccountPhoneNumberSecretValue = new NetworkCredential();
            TwilioAccountSsidSecretValue = new NetworkCredential();
            TwilioAccountTokenSecretValue = new NetworkCredential();
        }

        /// <summary>
        /// Saves configuration to a set of files.
        /// </summary>
        /// <param name="armTemplateFilePaths">Array of file paths to save configuration to.</param>
        public void SaveConfiguration(string[] armTemplateFilePaths)
        {
            foreach (string armTemplateFilePath in armTemplateFilePaths)
            {
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "vaults_ap_wu_keyvault_name", KeyVaultName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "ap_func_aad_name_secret_name", FunctionAppClientIdSecretName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "ap_func_aad_key_secret_name", FunctionAppClientPasswordSecretName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "luis_subscription_key_secret_name", LuisSubscriptionKeySecretName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "speech_api_key_secret_name", SpeechApiKeySecretName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "storage_access_key_secret_name", StorageAccessKeySecretName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "storage_read_access_key_secret_name", StorageReadAccessKeySecretName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "twilioaccountphonenumber_secret_name", TwilioAccountPhoneNumberSecretName);

                if (TwilioAccountPhoneNumberSecretValue != null)
                {
                    ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "twilioaccountphonenumber_secret_value", TwilioAccountPhoneNumberSecretValue.Password);
                }

                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "twilioaccountssid_secret_name", TwilioAccountSsidSecretName);

                if (TwilioAccountSsidSecretValue != null)
                {
                    ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "twilioaccountssid_secret_value", TwilioAccountSsidSecretValue.Password);
                }

                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "twilioaccounttoken_secret_name", TwilioAccountTokenSecretName);

                if (TwilioAccountTokenSecretValue != null)
                {
                    ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "twilioaccounttoken_secret_value", TwilioAccountTokenSecretValue.Password);
                }
            }
        }

        /// <summary>
        /// Clears all non-critical fields.
        /// </summary>
        public void ClearNonCriticalFields()
        {
            TwilioAccountPhoneNumberSecretValue = null;
            TwilioAccountSsidSecretValue = null;
            TwilioAccountTokenSecretValue = null;
        }
    }
}
