// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Configuration for Dynamics 365 CRM resource.
    /// </summary>
    public class DynamicsCrmConfiguration : NotifyPropertyChangedBase
    {
        private string _selectedDeploymentRegion;
        private string _solutionZipFilePath;
        private string _configurationZipFilePath;

        /// <summary>
        /// Gets the unique name of the Dynamics 365 CRM solution.
        /// </summary>
        public string SolutionUniqueName => "AdvocacyPlatformSolution";

        /// <summary>
        /// Gets or sets the selected deployment region.
        /// </summary>
        public string SelectedDeploymentRegion
        {
            get => _selectedDeploymentRegion;
            set
            {
                _selectedDeploymentRegion = value;

                NotifyPropertyChanged("SelectedDeploymentRegion");
            }
        }

        /// <summary>
        /// Gets or sets the path to the Dynamics 365 CRM solution ZIP file to deploy.
        /// </summary>
        public string SolutionZipFilePath
        {
            get => _solutionZipFilePath;
            set
            {
                _solutionZipFilePath = value;

                NotifyPropertyChanged("SolutionZipFilePath");
            }
        }

        /// <summary>
        /// Gets or sets the path to the ZIP file containing initial configuration data to deploy.
        /// </summary>
        public string ConfigurationZipFilePath
        {
            get => _configurationZipFilePath;
            set
            {
                _configurationZipFilePath = value;

                NotifyPropertyChanged("ConfigurationZipFilePath");
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
