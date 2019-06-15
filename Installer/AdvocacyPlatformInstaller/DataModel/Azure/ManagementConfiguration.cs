// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Configuration for Azure Management resources.
    /// </summary>
    public class ManagementConfiguration : NotifyPropertyChangedBase
    {
        private string _logAnalyticsName;
        private string _logicAppsManagementName;
        private string _appInsightsName;

        /// <summary>
        /// Gets or sets the name of the Azure Log Analytics resource.
        /// </summary>
        public string LogAnalyticsName
        {
            get => _logAnalyticsName;
            set
            {
                _logAnalyticsName = value;

                NotifyPropertyChanged("LogAnalyticsName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the Azure Logic Apps Management resource.
        /// </summary>
        public string LogicAppsManagementName
        {
            get => _logicAppsManagementName;
            set
            {
                _logicAppsManagementName = value;

                NotifyPropertyChanged("LogicAppsManagementName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the Azure App Insights resource for the Azure Function App.
        /// </summary>
        public string AppInsightsName
        {
            get => _appInsightsName;
            set
            {
                _appInsightsName = value;

                NotifyPropertyChanged("AppInsightsName");
            }
        }

        /// <summary>
        /// Loads configuration from a file.
        /// </summary>
        /// <param name="armTemplateFilePath">Path to the configuration file.</param>
        public void LoadConfiguration(string armTemplateFilePath)
        {
            LogAnalyticsName = string.IsNullOrWhiteSpace(LogAnalyticsName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "workspaces_ap_wu_loganalytics_name") : LogAnalyticsName;
            AppInsightsName = string.IsNullOrWhiteSpace(AppInsightsName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "components_ap_eu_appInsights_name") : AppInsightsName;
        }

        /// <summary>
        /// Saves configuration to a set of files.
        /// </summary>
        /// <param name="armTemplateFilePaths">Array of file paths to save configuration to.</param>
        public void SaveConfiguration(string[] armTemplateFilePaths)
        {
            foreach (string armTemplateFilePath in armTemplateFilePaths)
            {
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "workspaces_ap_wu_loganalytics_name", LogAnalyticsName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "components_ap_eu_appInsights_name", AppInsightsName);
            }
        }

        /// <summary>
        /// Clears all non-critical fields.
        /// </summary>
        public void ClearNonCriticalFields()
        {
            // Currently nothing to clear
        }
    }
}
