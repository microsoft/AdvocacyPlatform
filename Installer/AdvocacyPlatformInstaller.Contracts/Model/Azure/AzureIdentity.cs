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
    /// Model representing the identity of an Azure Resource.
    /// </summary>
    public class AzureIdentity
    {
        /// <summary>
        /// Gets or sets the identity type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the service principal id.
        /// </summary>
        public string PrincipalId { get; set; }

        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        public string TenantId { get; set; }
    }
}
