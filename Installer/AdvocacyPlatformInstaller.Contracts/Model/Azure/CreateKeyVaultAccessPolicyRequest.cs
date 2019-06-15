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
    /// Model representing body content of a request to create an Azure Key Vault Access Policy.
    /// </summary>
    public class CreateKeyVaultAccessPolicyRequest
    {
        /// <summary>
        /// Gets or sets the properties of the access policies.
        /// </summary>
        public CreateKeyVaultAccessPolicyRequestProperties Properties { get; set; }
    }
}
