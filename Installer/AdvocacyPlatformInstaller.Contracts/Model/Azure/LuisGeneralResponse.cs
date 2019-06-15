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
    /// Base model representing a generic response from the LUIS Authoring API.
    /// </summary>
    public class LuisGeneralResponse
    {
        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        public string Message { get; set; }
    }
}
