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
    /// Model representing an Azure Key Vault secret.
    /// </summary>
    public class AzureKeyVaultSecret
    {
        /// <summary>
        /// Gets or sets the id of the secret.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the secret value.
        /// </summary>
        public string Value { get; set; }
    }
}
