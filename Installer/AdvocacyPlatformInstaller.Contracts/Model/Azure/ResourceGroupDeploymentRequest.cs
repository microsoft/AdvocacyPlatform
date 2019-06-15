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
    /// Model representing the body content of a request to perform an Azure Resource Group deployment.
    /// </summary>
    public class ResourceGroupDeploymentRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceGroupDeploymentRequest"/> class.
        /// </summary>
        public ResourceGroupDeploymentRequest()
        {
            Properties = new ResourceGroupDeploymentRequestProperties();
        }

        /// <summary>
        /// Gets or sets the properties for this request.
        /// </summary>
        public ResourceGroupDeploymentRequestProperties Properties { get; set; }
    }
}
