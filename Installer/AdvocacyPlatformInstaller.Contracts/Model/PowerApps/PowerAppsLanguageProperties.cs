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
    /// Model representing properties for describing a PowerApps CDS database language.
    /// </summary>
    public class PowerAppsLanguageProperties
    {
        /// <summary>
        /// Gets or sets the locale id.
        /// </summary>
        public int LocaleId { get; set; }

        /// <summary>
        /// Gets or sets the localized name.
        /// </summary>
        public string LocalizedName { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this language is the tenant default.
        /// </summary>
        public bool IsTenantDefault { get; set; }
    }
}
