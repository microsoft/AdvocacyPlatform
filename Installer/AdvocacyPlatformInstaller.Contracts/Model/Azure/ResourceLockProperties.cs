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
    /// Model representing the properties of an Azure Resource Lock.
    /// </summary>
    public class ResourceLockProperties
    {
        /// <summary>
        /// Gets or sets the lock level.
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// Gets or sets notes associated with the lock.
        /// </summary>
        public string Notes { get; set; }
    }
}
