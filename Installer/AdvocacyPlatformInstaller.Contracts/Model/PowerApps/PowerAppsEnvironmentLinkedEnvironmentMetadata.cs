// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Model representing linked environment metadata regarding a PowerApps environment.
    /// </summary>
    public class PowerAppsEnvironmentLinkedEnvironmentMetadata
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the resource id.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the friendly name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the unique name.
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the domain name.
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the instance URL.
        /// </summary>
        public string InstanceUrl { get; set; }

        /// <summary>
        /// Gets or sets the instance's API URL.
        /// </summary>
        public string InstanceApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the base language.
        /// </summary>
        public int BaseLanguage { get; set; }

        /// <summary>
        /// Gets or sets the instance state.
        /// </summary>
        public string InstanceState { get; set; }

        /// <summary>
        /// Gets or sets the date and time the instance was created.
        /// </summary>
        public DateTime? CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time the instance was modified.
        /// </summary>
        public DateTime? ModifiedTime { get; set; }

        /// <summary>
        /// Gets or sets the host name suffice.
        /// </summary>
        public string HostNameSuffice { get; set; }

        /// <summary>
        /// Gets or sets the BAP solution id.
        /// </summary>
        public string BapSolutionId { get; set; }

        /// <summary>
        /// Gets or sets the list of creation templates.
        /// </summary>
        public string[] CreationTemplates { get; set; }

        /// <summary>
        /// Gets or sets the web API version.
        /// </summary>
        public string WebApiVersion { get; set; }
    }
}
