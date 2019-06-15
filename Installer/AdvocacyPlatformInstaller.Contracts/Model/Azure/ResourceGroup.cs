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
    /// Model representing an Azure Resource Group.
    /// </summary>
    public class ResourceGroup : AzureResourceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceGroup"/> class.
        /// </summary>
        public ResourceGroup()
        {
            Tags = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets a dictionary of tags associated with the resource group.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }
    }
}
