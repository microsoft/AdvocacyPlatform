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
    /// Configuration for the Azure Storage account resource.
    /// </summary>
    public class StorageAccountConfiguration : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Name of the expected recordings container.
        /// </summary>
        public const string RecordingsContainerName = "recordings";

        private string _storageAccountName;
        private string _fullAccessPolicyId;
        private string _readAccessPolicyId;

        /// <summary>
        /// Gets or sets the name of the Azure Storage account resource.
        /// </summary>
        public string StorageAccountName
        {
            get => _storageAccountName;
            set
            {
                _storageAccountName = value;

                NotifyPropertyChanged("StorageAccountName");
            }
        }

        /// <summary>
        /// Gets or sets the ID of the Stored Access Policy for full access.
        /// </summary>
        public string FullAccessPolicyId
        {
            get => _fullAccessPolicyId;
            set
            {
                _fullAccessPolicyId = value;

                NotifyPropertyChanged("FullAccessPolicyId");
            }
        }

        /// <summary>
        /// Gets or sets the ID of the Stored Access Policy for read access.
        /// </summary>
        public string ReadAccessPolicyId
        {
            get => _readAccessPolicyId;
            set
            {
                _readAccessPolicyId = value;

                NotifyPropertyChanged("ReadAccessPolicyId");
            }
        }

        /// <summary>
        /// Loads configuration from file.
        /// </summary>
        /// <param name="armTemplateFilePath">Path to the configuration file.</param>
        public void LoadConfiguration(string armTemplateFilePath)
        {
            StorageAccountName = string.IsNullOrWhiteSpace(StorageAccountName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "storageAccounts_apwustorage_name") : StorageAccountName;
        }

        /// <summary>
        /// Saves configuration to a set of files.
        /// </summary>
        /// <param name="armTemplateFilePaths">Array of file paths to save configuration to.</param>
        public void SaveConfiguration(string[] armTemplateFilePaths)
        {
            foreach (string armTemplateFilePath in armTemplateFilePaths)
            {
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "storageAccounts_apwustorage_name", StorageAccountName);
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
