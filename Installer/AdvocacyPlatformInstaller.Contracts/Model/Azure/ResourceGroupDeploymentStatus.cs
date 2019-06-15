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
    /// Model representing the status of an Azure Resource Group deployment.
    /// </summary>
    public class ResourceGroupDeploymentStatus
    {
        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string Status { get; set; }
    }
}
