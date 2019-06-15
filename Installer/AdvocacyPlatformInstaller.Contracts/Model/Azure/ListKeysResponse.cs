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
    /// Model representing the response to a request to the listKeys resource action.
    /// </summary>
    public class ListKeysResponse
    {
        /// <summary>
        /// Gets or sets a list of keys.
        /// </summary>
        public Key[] Keys { get; set; }
    }
}
