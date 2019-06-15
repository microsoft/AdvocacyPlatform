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
    /// Model representing the properties of an Azure Key Vault Access Policy.
    /// </summary>
    public class CreateKeyVaultAccessPolicyRequestProperties
    {
        /// <summary>
        /// Gets or sets a list of key vault access policies.
        /// </summary>
        public KeyVaultAccessPolicy[] AccessPolicies { get; set; }
    }
}
