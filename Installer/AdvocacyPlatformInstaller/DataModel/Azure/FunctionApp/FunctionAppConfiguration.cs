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
    /// Configuration information for the AP Function App.
    /// </summary>
    public class FunctionAppConfiguration : NotifyPropertyChangedBase
    {
        private string _applicationRegistrationName;
        private string _applicationId;
        private string _appName;
        private string _appServiceName;
        private string _appDeploymentSourceUrl;
        private bool _collectSecrets;

        private NetworkCredential _applicationRegistrationSecret;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionAppConfiguration"/> class.
        /// </summary>
        public FunctionAppConfiguration()
        {
            int randomInt = new Random().Next() % 1000;

            AppDeploymentSourceUrl = "https://github.com/Microsoft/AdvocacyPlatform/releases/latest/download/Microsoft.AdvocacyPlatform.Functions.zip";
            ApplicationRegistrationSecret = new NetworkCredential("applicationRegistrationSecret", $"{System.Web.Security.Membership.GeneratePassword(36, 8)}{randomInt}"); // TODO: Make better; quick work around to ensure at least one number in client secret
        }

        /// <summary>
        /// Gets or sets the name of the application registration.
        /// </summary>
        public string ApplicationRegistrationName
        {
            get => _applicationRegistrationName;
            set
            {
                _applicationRegistrationName = value;

                NotifyPropertyChanged("ApplicationRegistrationName");
            }
        }

        /// <summary>
        /// Gets or sets the application id of the application registration.
        /// </summary>
        public string ApplicationId
        {
            get => _applicationId;
            set
            {
                _applicationId = value;

                NotifyPropertyChanged("ApplicationId");
            }
        }

        /// <summary>
        /// Gets or sets the client secret for the service principal.
        /// </summary>
        [JsonIgnore]
        public NetworkCredential ApplicationRegistrationSecret
        {
            get => _applicationRegistrationSecret;
            set
            {
                _applicationRegistrationSecret = value;

                NotifyPropertyChanged("ApplicationRegistrationSecret");
            }
        }

        /// <summary>
        /// Gets or sets the Azure Function App resource name.
        /// </summary>
        public string AppName
        {
            get => _appName;
            set
            {
                _appName = value;

                NotifyPropertyChanged("AppName");
            }
        }

        /// <summary>
        /// Gets or sets the Azure App Service resource name.
        /// </summary>
        public string AppServiceName
        {
            get => _appServiceName;
            set
            {
                _appServiceName = value;

                NotifyPropertyChanged("AppServiceName");
            }
        }

        /// <summary>
        /// Gets or sets the URL of the AP Function App ZIP to deploy.
        /// </summary>
        public string AppDeploymentSourceUrl
        {
            get => _appDeploymentSourceUrl;
            set
            {
                _appDeploymentSourceUrl = value;

                NotifyPropertyChanged("AppDeploymentSourceUrl");
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
        /// <param name="armTemplateFilePath">Path to the configuration file.</param>
        public void LoadConfiguration(string armTemplateFilePath)
        {
            AppName = string.IsNullOrWhiteSpace(AppName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "sites_ap_wu_func_name") : AppName;
            AppServiceName = string.IsNullOrWhiteSpace(AppServiceName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "serverfarms_WestUS2Plan_name") : AppServiceName;
        }

        /// <summary>
        /// Saves configuration to a file.
        /// </summary>
        /// <param name="armTemplateFilePaths">Path to save the configuration file.</param>
        public void SaveConfiguration(string[] armTemplateFilePaths)
        {
            foreach (string armTemplateFilePath in armTemplateFilePaths)
            {
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "ap_func_aad_name_secret_value", ApplicationRegistrationName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "ap_func_aad_key_secret_value", ApplicationRegistrationSecret.Password);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "sites_ap_wu_func_name", AppName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "serverfarms_WestUS2Plan_name", AppServiceName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "ap_function_app_zip_uri", AppDeploymentSourceUrl);
            }
        }

        /// <summary>
        /// Clears non-critical configuration fields.
        /// </summary>
        public void ClearNonCriticalFields()
        {
            ApplicationRegistrationSecret = null;
        }
    }
}
