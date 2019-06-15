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
    /// Model representing the response to a request to get a list of available PowerApps CDS database languages.
    /// </summary>
    public class GetPowerAppsLanguagesResponse
    {
        /// <summary>
        /// Gets or sets the list of available CDS database languages.
        /// </summary>
        public PowerAppsLanguage[] Value { get; set; }
    }
}
