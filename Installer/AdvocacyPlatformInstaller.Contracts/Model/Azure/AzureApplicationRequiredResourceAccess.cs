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
    /// Model representing a required API permission for an application registration.
    /// </summary>
    public class AzureApplicationRequiredResourceAccess
    {
        /// <summary>
        /// Gets or sets the target resource's application id.
        /// </summary>
        public string ResourceAppId { get; set; }

        /// <summary>
        /// Gets or sets the required permissions on the target resource.
        /// </summary>
        public ResourceAccess[] ResourceAccess { get; set; }
    }
}
