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
    /// View model representing a PowerApps environment.
    /// </summary>
    public class PowerAppsEnvironment : NotifyPropertyChangedBase
    {
        private string _environmentName;
        private string _displayName;
        private string _organizationName;
        private string _organizationDomainName;
        private string _webApplicationUrl;

        /// <summary>
        /// Gets or sets the name of the environment.
        /// </summary>
        public string EnvironmentName
        {
            get => _environmentName;
            set
            {
                _environmentName = value;

                NotifyPropertyChanged("EnvironmentName");
            }
        }

        /// <summary>
        /// Gets or sets the display name of the environment.
        /// </summary>
        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;

                NotifyPropertyChanged("DisplayName");
            }
        }

        /// <summary>
        /// Gets or sets the unique organization name of the CDS database.
        /// </summary>
        public string OrganizationName
        {
            get => _organizationName;
            set
            {
                _organizationName = value;

                NotifyPropertyChanged("OrganizationName");
            }
        }

        /// <summary>
        /// Gets or sets the unique domain name of the CDS database.
        /// </summary>
        public string OrganizationDomainName
        {
            get => _organizationDomainName;
            set
            {
                _organizationDomainName = value;

                NotifyPropertyChanged("OrganizationDomainName");
            }
        }

        /// <summary>
        /// Gets or sets the URL of the web application for the Dynamics 365 CRM instance.
        /// </summary>
        public string WebApplicationUrl
        {
            get => _webApplicationUrl;
            set
            {
                _webApplicationUrl = value;

                NotifyPropertyChanged("WebApplicationUrl");
            }
        }

        /// <summary>
        /// Gets the display name of the environment.
        /// </summary>
        /// <returns>The display name of the environment.</returns>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
