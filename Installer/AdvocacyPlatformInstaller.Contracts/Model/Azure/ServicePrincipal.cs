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
    /// Model representing an Azure AD Service Principal.
    /// </summary>
    public class ServicePrincipal
    {
        /// <summary>
        /// Gets or sets a value indicating whether the account is enabled.
        /// </summary>
        public bool AccountEnabled { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string AppDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the parent application id.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the organization id of the owning application registration.
        /// </summary>
        public string AppOwnerOrganizationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether role assignment is required.
        /// </summary>
        public bool AppRoleAssignmentRequired { get; set; }
    }
}
