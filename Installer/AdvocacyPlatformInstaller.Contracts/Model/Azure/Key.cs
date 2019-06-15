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
    /// Model representing an access key.
    /// </summary>
    public class Key
    {
        /// <summary>
        /// Gets or sets the key name.
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// Gets or sets the key value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the permissions for the key.
        /// </summary>
        public string Permissions { get; set; }
    }
}
