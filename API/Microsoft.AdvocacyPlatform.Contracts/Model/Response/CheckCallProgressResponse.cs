// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents the expected response schema for CheckCallProgress.
    /// </summary>
    public class CheckCallProgressResponse : FunctionResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckCallProgressResponse"/> class.
        /// </summary>
        public CheckCallProgressResponse()
        {
            Duration = 0;
        }

        /// <summary>
        /// Gets or sets the SID of the call resource.
        /// </summary>
        public string CallSid { get; set; }

        /// <summary>
        /// Gets or sets the status of the call.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the length of the call.
        /// </summary>
        public int Duration { get; set; }
    }
}
