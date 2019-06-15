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
    /// Configuration for the Bing Maps account.
    /// </summary>
    public class BingMapsConfiguration : NotifyPropertyChangedBase
    {
        private NetworkCredential _apiKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="BingMapsConfiguration"/> class.
        /// </summary>
        public BingMapsConfiguration()
        {
            ApiKey = new NetworkCredential();
        }

        /// <summary>
        /// Gets or sets the API key for the associated Bing Maps account.
        /// </summary>
        [JsonIgnore]
        public NetworkCredential ApiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;

                NotifyPropertyChanged("ApiKey");
            }
        }

        /// <summary>
        /// Loads configuration from file.
        /// </summary>
        /// <param name="armTemplateFilePath">Path to the configuration file.</param>
        public void LoadConfiguration(string armTemplateFilePath)
        {
        }

        /// <summary>
        /// Saves configuration to a set of files.
        /// </summary>
        /// <param name="armTemplateFilePaths">Array of file paths to save configuration to.</param>
        public void SaveConfiguration(string[] armTemplateFilePaths)
        {
            foreach (string armTemplateFilePath in armTemplateFilePaths)
            {
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "bingmaps_api_key", ApiKey.Password);
            }
        }

        /// <summary>
        /// Clears all non-critical fields.
        /// </summary>
        public void ClearNonCriticalFields()
        {
            ApiKey = null;
        }
    }
}
