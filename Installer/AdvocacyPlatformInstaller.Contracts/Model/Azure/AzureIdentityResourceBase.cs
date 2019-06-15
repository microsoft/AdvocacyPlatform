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
    /// Base model representing an Azure Resource with an identity.
    /// </summary>
    public class AzureIdentityResourceBase : AzureResourceBase
    {
        /// <summary>
        /// Gets or sets the identity of the resource.
        /// </summary>
        public AzureIdentity Identity { get; set; }
    }
}
