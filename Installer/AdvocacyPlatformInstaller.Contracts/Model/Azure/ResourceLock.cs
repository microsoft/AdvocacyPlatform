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
    /// Model representing an Azure Resource Lock.
    /// </summary>
    public class ResourceLock : AzureResourceBase
    {
        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        public ResourceLockProperties Properties { get; set; }
    }
}
