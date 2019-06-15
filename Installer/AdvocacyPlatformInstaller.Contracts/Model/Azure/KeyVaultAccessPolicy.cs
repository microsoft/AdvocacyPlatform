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
    /// Model representing an Azure Key Vault Access Policy.
    /// </summary>
    public class KeyVaultAccessPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultAccessPolicy"/> class.
        /// </summary>
        public KeyVaultAccessPolicy()
        {
            ApplicationId = null;
            ObjectId = null;
        }

        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the application id of the service principal to grant access to.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the object id of the security principal to grant access to.
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the permissions to grant.
        /// </summary>
        public KeyVaultAccessPolicyPermissions Permissions { get; set; }
    }
}
