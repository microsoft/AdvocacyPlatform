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
    /// Model representing a required resource access.
    /// </summary>
    public class ResourceAccess
    {
        /// <summary>
        /// Gets or sets the target resource id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the target resource type.
        /// </summary>
        public string Type { get; set; }
    }
}
