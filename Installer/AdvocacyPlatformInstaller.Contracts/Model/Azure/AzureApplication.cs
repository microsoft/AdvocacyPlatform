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
    /// Model representing an Azure AD application registration.
    /// </summary>
    public class AzureApplication : AzureApplicationRequestBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureApplication"/> class.
        /// </summary>
        public AzureApplication()
            : base()
        {
        }

        /// <summary>
        /// Gets or sets the object id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the application id.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the publisher domain.
        /// </summary>
        public string PublisherDomain { get; set; }
    }
}
