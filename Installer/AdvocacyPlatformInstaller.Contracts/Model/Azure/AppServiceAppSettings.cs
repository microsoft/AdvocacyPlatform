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
    /// Model representing an Azure App Service's application settings.
    /// </summary>
    public class AppServiceAppSettings : AzureResourceBase
    {
        /// <summary>
        /// Gets or sets the dictionary of application settings.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }
    }
}
