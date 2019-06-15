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
    /// Model representing permissions to grant in an Azure Key Vault Access Policy.
    /// </summary>
    public class KeyVaultAccessPolicyPermissions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultAccessPolicyPermissions"/> class.
        /// </summary>
        public KeyVaultAccessPolicyPermissions()
        {
            Certificates = new string[0];
            Keys = new string[0];
            Secrets = new string[0];
            Storage = new string[0];
        }

        /// <summary>
        /// Gets or sets the certificate permissions to grant.
        /// </summary>
        public string[] Certificates { get; set; }

        /// <summary>
        /// Gets or sets the key permissions to grant.
        /// </summary>
        public string[] Keys { get; set; }

        /// <summary>
        /// Gets or sets the secret permissions to grant.
        /// </summary>
        public string[] Secrets { get; set; }

        /// <summary>
        /// Gets or sets the storage permissions to grant.
        /// </summary>
        public string[] Storage { get; set; }
    }
}
