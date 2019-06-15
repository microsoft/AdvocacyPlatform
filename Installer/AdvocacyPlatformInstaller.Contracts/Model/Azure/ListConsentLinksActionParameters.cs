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
    /// Model representing the parameters to present for the listConsentLinks resource action.
    /// </summary>
    public class ListConsentLinksActionParameters
    {
        /// <summary>
        /// Gets or sets the parameter name.
        /// </summary>
        public string ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the redirect URL.
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}
