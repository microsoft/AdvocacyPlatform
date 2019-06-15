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
    /// Base model for representing an Azure AD application registration.
    /// </summary>
    public class AzureApplicationRequestBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureApplicationRequestBase"/> class.
        /// </summary>
        public AzureApplicationRequestBase()
        {
            PasswordCredentials = new AzureApplicationPasswordCredential[0];
            RequiredResourceAccess = new AzureApplicationRequiredResourceAccess[0];
        }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the identifier URIs.
        /// </summary>
        public string[] IdentifierUris { get; set; }

        /// <summary>
        /// Gets or sets the client secrets.
        /// </summary>
        public AzureApplicationPasswordCredential[] PasswordCredentials { get; set; }

        /// <summary>
        /// Gets or sets the required API permissions.
        /// </summary>
        public AzureApplicationRequiredResourceAccess[] RequiredResourceAccess { get; set; }

        /// <summary>
        /// Gets or sets the signin audience.
        /// </summary>
        public string SignInAudience { get; set; }
    }
}
