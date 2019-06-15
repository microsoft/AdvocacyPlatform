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
    /// Model representing an Azure Password Credential.
    /// </summary>
    public class AzureApplicationPasswordCredential
    {
        /// <summary>
        /// Gets or sets the date and time the credential should become valid.
        /// </summary>
        public string StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time the credential should expire.
        /// </summary>
        public string EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the secret value.
        /// </summary>
        public string SecretText { get; set; }
    }
}
