// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// View model representing the installation configuration.
    /// </summary>
    public class InstallationConfiguration : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Path to the temporary installation configuration.
        /// </summary>
        public const string InstallationConfigurationFilePath = @".\config\installationConfiguration.json";

        private AzureConfiguration _azure;
        private PowerAppsConfiguration _powerApps;
        private DynamicsCrmConfiguration _dynamicsCrm;
        private FeatureTree _features;
        private bool _updateAvailable;
        private bool _isSaved;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallationConfiguration"/> class.
        /// </summary>
        public InstallationConfiguration()
        {
            Features = new FeatureTree();
            UpdateAvailable = true;
        }

        /// <summary>
        /// Gets or sets the model representing the Azure resource configuration.
        /// </summary>
        public AzureConfiguration Azure
        {
            get => _azure;
            set
            {
                _azure = value;

                NotifyPropertyChanged("Azure");
            }
        }

        /// <summary>
        /// Gets or sets the model representing the PowerApps configuration.
        /// </summary>
        public PowerAppsConfiguration PowerApps
        {
            get => _powerApps;
            set
            {
                _powerApps = value;

                NotifyPropertyChanged("PowerApps");
            }
        }

        /// <summary>
        /// Gets or sets the model representing the Dynamics 365 CRM configuration.
        /// </summary>
        public DynamicsCrmConfiguration DynamicsCrm
        {
            get => _dynamicsCrm;
            set
            {
                _dynamicsCrm = value;

                NotifyPropertyChanged("DynamicsCrm");
            }
        }

        /// <summary>
        /// Gets or sets the feature tree representing available features.
        /// </summary>
        [JsonIgnore]
        public FeatureTree Features
        {
            get => _features;
            set
            {
                _features = value;

                NotifyPropertyChanged("Features");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether updates are available.
        /// </summary>
        public bool UpdateAvailable
        {
            get => _updateAvailable;
            set
            {
                _updateAvailable = value;

                NotifyPropertyChanged("UpdateAvailable");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the configuration has been saved.
        /// </summary>
        [JsonIgnore]
        public bool IsSaved
        {
            get => _isSaved;
            set
            {
                _isSaved = value;

                NotifyPropertyChanged("IsSaved");
            }
        }

        /// <summary>
        /// Loads configuration from file.
        /// </summary>
        /// <param name="filePath">Path to the configuration file.</param>
        /// <returns>The configuration loaded from the specified file.</returns>
        public static InstallationConfiguration LoadConfiguration(string filePath)
        {
            InstallationConfiguration configuration = new InstallationConfiguration();

            if (File.Exists(filePath))
            {
                configuration = JsonConvert.DeserializeObject<InstallationConfiguration>(
                    File.ReadAllText(filePath));

                configuration.Azure.LoadConfiguration();
            }

            return configuration;
        }

        /// <summary>
        /// Loads configuration from file.
        /// </summary>
        public void LoadConfiguration()
        {
            if (File.Exists(InstallationConfigurationFilePath))
            {
                InstallationConfiguration existingConfiguration = JsonConvert.DeserializeObject<InstallationConfiguration>(
                    File.ReadAllText(InstallationConfigurationFilePath));

                Azure = existingConfiguration.Azure;
                PowerApps = existingConfiguration.PowerApps;
                DynamicsCrm = existingConfiguration.DynamicsCrm;
            }
            else
            {
                Azure = new AzureConfiguration();
                PowerApps = new PowerAppsConfiguration();
                DynamicsCrm = new DynamicsCrmConfiguration();
            }

            Azure.LoadConfiguration();
        }

        /// <summary>
        /// Saves configuration to file.
        /// </summary>
        public void SaveConfiguration()
        {
            string installationConfigurationContent = JsonConvert.SerializeObject(this);

            File.WriteAllText(InstallationConfigurationFilePath, installationConfigurationContent, Encoding.UTF8);
        }

        /// <summary>
        /// Saves configuration to file.
        /// </summary>
        /// <param name="filePath">Path to the file to save to.</param>
        public void SaveConfiguration(string filePath)
        {
            string installationConfigurationContent = JsonConvert.SerializeObject(this);

            File.WriteAllText(filePath, installationConfigurationContent, Encoding.UTF8);
        }

        /// <summary>
        /// Clears all non-critical fields.
        /// </summary>
        public void ClearNonCriticalFields()
        {
            _azure.ClearNonCriticalFields();
            _powerApps.ClearNonCriticalFields();
            _dynamicsCrm.ClearNonCriticalFields();
        }
    }
}
