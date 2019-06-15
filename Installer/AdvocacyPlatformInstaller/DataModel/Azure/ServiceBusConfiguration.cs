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
    /// Configuration for the Azure Service Bus resource.
    /// </summary>
    public class ServiceBusConfiguration : NotifyPropertyChangedBase
    {
        private string _serviceBusNamespaceName;

        /// <summary>
        /// Gets or sets the name of the Azure Service Bus namespace resource.
        /// </summary>
        public string ServiceBusNamespaceName
        {
            get => _serviceBusNamespaceName;
            set
            {
                _serviceBusNamespaceName = value;

                NotifyPropertyChanged("ServiceBusNamespaceName");
            }
        }

        /// <summary>
        /// Loads configuration from file.
        /// </summary>
        /// <param name="armTemplateFilePath">Path to the configuration file.</param>
        public void LoadConfiguration(string armTemplateFilePath)
        {
            ServiceBusNamespaceName = string.IsNullOrWhiteSpace(ServiceBusNamespaceName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "namespaces_ap_wu_messagingbus_name") : ServiceBusNamespaceName;
        }

        /// <summary>
        /// Saves configuration to a set of files.
        /// </summary>
        /// <param name="armTemplateFilePaths">Array of file paths to save configuration to.</param>
        public void SaveConfiguration(string[] armTemplateFilePaths)
        {
            foreach (string armTemplateFilePath in armTemplateFilePaths)
            {
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "namespaces_ap_wu_messagingbus_name", ServiceBusNamespaceName);
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
