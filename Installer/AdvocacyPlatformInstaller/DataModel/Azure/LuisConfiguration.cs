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
    /// Configuration for Azure LUIS Cognitive Service resource.
    /// </summary>
    public class LuisConfiguration : NotifyPropertyChangedBase
    {
        private string _resourceName;
        private NetworkCredential _authoringKey;
        private string _applicationId;
        private string _appName;
        private string _appVersion;
        private string _appFilePath;
        private string _authoringRegion;
        private string _resourceRegion;
        private string _endpointUri;
        private bool _shouldDelete;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuisConfiguration"/> class.
        /// </summary>
        public LuisConfiguration()
        {
            AppName = "APEntityExtraction";
            AppVersion = "0.2";
            AppFilePath = @".\config\APEntityExtraction.json";
            AuthoringRegion = "westus";
            ResourceRegion = "westus2";

            AuthoringKey = new NetworkCredential();
        }

        /// <summary>
        /// Gets or sets the name of the Azure LUIS Cognitive Service resource.
        /// </summary>
        public string ResourceName
        {
            get => _resourceName;
            set
            {
                _resourceName = value;

                NotifyPropertyChanged("ResourceName");
            }
        }

        /// <summary>
        /// Gets or sets the authoring key for the associated LUIS account.
        /// </summary>
        [JsonIgnore]
        public NetworkCredential AuthoringKey
        {
            get => _authoringKey;
            set
            {
                _authoringKey = value;

                NotifyPropertyChanged("AuthoringKey");
            }
        }

        /// <summary>
        /// Gets or sets the application ID of the LUIS application.
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
        /// Gets or sets the name of the LUIS application.
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
        /// Gets or sets the version of the LUIS application.
        /// </summary>
        public string AppVersion
        {
            get => _appVersion;
            set
            {
                _appVersion = value;

                NotifyPropertyChanged("AppVersion");
            }
        }

        /// <summary>
        /// Gets or sets the file path to the LUIS application definition.
        /// </summary>
        public string AppFilePath
        {
            get => _appFilePath;
            set
            {
                _appFilePath = value;

                NotifyPropertyChanged("AppFilePath");
            }
        }

        /// <summary>
        /// Gets or sets the authoring region for the associated LUIS account.
        /// </summary>
        public string AuthoringRegion
        {
            get => _authoringRegion;
            set
            {
                _authoringRegion = value;

                NotifyPropertyChanged("AuthoringRegion");
            }
        }

        /// <summary>
        /// Gets or sets the resource region for the associated Azure LUIS Cognitive Service resource.
        /// </summary>
        public string ResourceRegion
        {
            get => _resourceRegion;
            set
            {
                _resourceRegion = value;

                NotifyPropertyChanged("ResourceRegion");
            }
        }

        /// <summary>
        /// Gets or sets the URI of the LUIS application endpoint API.
        /// </summary>
        public string EndpointUri
        {
            get => _endpointUri;
            set
            {
                _endpointUri = value;

                NotifyPropertyChanged("EndpointUri");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the LUIS application should be removed.
        /// </summary>
        public bool ShouldDelete
        {
            get => _shouldDelete;
            set
            {
                _shouldDelete = value;

                NotifyPropertyChanged("ShouldDelete");
            }
        }

        /// <summary>
        /// Loads configuration from a file.
        /// </summary>
        /// <param name="armTemplateFilePath">Path to the configuration file.</param>
        public void LoadConfiguration(string armTemplateFilePath)
        {
            ResourceName = string.IsNullOrWhiteSpace(ResourceName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "accounts_ap_wu_luisapp_name") : ResourceName;
        }

        /// <summary>
        /// Saves configuration to a set of files.
        /// </summary>
        /// <param name="armTemplateFilePaths">Array of file paths to save configuration to.</param>
        public void SaveConfiguration(string[] armTemplateFilePaths)
        {
            foreach (string armTemplateFilePath in armTemplateFilePaths)
            {
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "accounts_ap_wu_luisapp_name", ResourceName);
            }
        }

        /// <summary>
        /// Clears all non-critical fields.
        /// </summary>
        public void ClearNonCriticalFields()
        {
            AuthoringKey = null;
        }
    }
}
