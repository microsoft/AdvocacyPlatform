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
    /// Model representing the body content for a request to create a PowerApps environment.
    /// </summary>
    public class CreatePowerAppsEnvironmentRequest
    {
        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets properties describing the environment.
        /// </summary>
        public NewPowerAppsEnvironmentProperties Properties { get; set; }
    }
}
