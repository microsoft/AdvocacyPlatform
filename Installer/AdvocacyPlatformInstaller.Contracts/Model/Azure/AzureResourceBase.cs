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
    /// Base model representing an Azure Resource.
    /// </summary>
    public class AzureResourceBase
    {
        /// <summary>
        /// Gets or sets the resource id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resource type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the resource location.
        /// </summary>
        public string Location { get; set; }
    }
}
