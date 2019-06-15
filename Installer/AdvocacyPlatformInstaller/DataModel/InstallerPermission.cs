// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Describes an API permission required by the installer.
    /// </summary>
    public class InstallerPermission
    {
        /// <summary>
        /// Gets or sets the name of the API.
        /// </summary>
        public string API { get; set; }

        /// <summary>
        /// Gets or sets the API permission name.
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// Gets or sets a description of the API permission.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tenant administrator consent is required to grant the service principal this API permission.
        /// </summary>
        public bool RequiresAdminConsent { get; set; }
    }
}
