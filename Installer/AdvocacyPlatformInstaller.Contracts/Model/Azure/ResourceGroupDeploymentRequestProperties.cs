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
    /// Model representing the properties required for a request to perform an Azure Resource Group deployment.
    /// </summary>
    public class ResourceGroupDeploymentRequestProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceGroupDeploymentRequestProperties"/> class.
        /// </summary>
        public ResourceGroupDeploymentRequestProperties()
        {
            Mode = "Incremental";
        }

        /// <summary>
        /// Gets or sets the deployment mode.
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets the template parameters.
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// Gets or sets the template.
        /// </summary>
        public string Template { get; set; }
    }
}
