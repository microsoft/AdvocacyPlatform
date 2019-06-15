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
    /// Model representing an API connection's consent link.
    /// </summary>
    public class ApiConnectionConsentLink
    {
        /// <summary>
        /// Gets or sets the link URI.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the connection status.
        /// </summary>
        public string Status { get; set; }
    }
}
