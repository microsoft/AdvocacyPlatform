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
    /// Model representing an Azure Stored Access Policy.
    /// </summary>
    public class AccessPolicy
    {
        /// <summary>
        /// Gets or sets the date and time this policy should go into affect.
        /// </summary>
        public string Start { get; set; }

        /// <summary>
        /// Gets or sets the date and time this policy should expire.
        /// </summary>
        public string Expiry { get; set; }

        /// <summary>
        /// Gets or sets the permissions this policy should grant.
        /// </summary>
        public string Permission { get; set; }
    }
}
