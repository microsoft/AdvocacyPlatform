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
    /// Model representing the response to a request to get available PowerApps CDS database currencies.
    /// </summary>
    public class GetPowerAppsCurrenciesResponse
    {
        /// <summary>
        /// Gets or sets the list of available CDS database currencies.
        /// </summary>
        public PowerAppsCurrency[] Value { get; set; }
    }
}
