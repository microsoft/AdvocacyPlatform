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
    /// Model representing properties for describing a PowerApps environment.
    /// </summary>
    public class PowerAppsEnvironmentProperties
    {
        /// <summary>
        /// Gets or sets the Azure region hint.
        /// </summary>
        public string AzureRegionHint { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the date and time the environment was created.
        /// </summary>
        public DateTime? CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time the environment was last modified.
        /// </summary>
        public DateTime? LastModifiedTime { get; set; }

        /// <summary>
        /// Gets or sets the provisioning state.
        /// </summary>
        public string ProvisioningState { get; set; }

        /// <summary>
        /// Gets or sets the creation type.
        /// </summary>
        public string CreationType { get; set; }

        /// <summary>
        /// Gets or sets the SKU.
        /// </summary>
        public string EnvironmentSku { get; set; }

        /// <summary>
        /// Gets or sets the environment type.
        /// </summary>
        public string EnvironmentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this environment is the default.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of runtime endpoints.
        /// </summary>
        public Dictionary<string, string> RunTimeEndpoints { get; set; }

        /// <summary>
        /// Gets or sets linked environment metadata.
        /// </summary>
        public PowerAppsEnvironmentLinkedEnvironmentMetadata LinkedEnvironmentMetadata { get; set; }

        /// <summary>
        /// Gets or sets a date and time indicating when the environment will expire.
        /// </summary>
        public DateTime? ExpirationTime { get; set; }

        /// <summary>
        /// Gets or sets a date and time indicating when the environment was soft deleted.
        /// </summary>
        public DateTime? SoftDeleteTime { get; set; }
    }
}
