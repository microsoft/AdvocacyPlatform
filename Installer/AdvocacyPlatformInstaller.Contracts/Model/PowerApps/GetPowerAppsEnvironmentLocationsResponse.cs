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
    /// Model representing the response to a request to get the available PowerApps environment locations.
    /// </summary>
    public class GetPowerAppsEnvironmentLocationsResponse
    {
        /// <summary>
        /// Gets or sets the list of available environment locations.
        /// </summary>
        public PowerAppsEnvironmentLocation[] Value { get; set; }
    }
}
