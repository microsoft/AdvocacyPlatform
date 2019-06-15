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
    /// Model representing the body content for a request for the listConsentLinks resource action.
    /// </summary>
    public class ListConsentLinksActionRequest
    {
        /// <summary>
        /// Gets or sets the request parameters.
        /// </summary>
        public ListConsentLinksActionParameters[] Parameters { get; set; }
    }
}
