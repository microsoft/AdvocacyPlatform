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
    /// Model representing an Azure API connection resource.
    /// </summary>
    public class ApiConnectionResource : AzureResourceBase
    {
        /// <summary>
        /// Gets or sets connection properties.
        /// </summary>
        public ApiConnectionResourceProperties Properties { get; set; }
    }
}
