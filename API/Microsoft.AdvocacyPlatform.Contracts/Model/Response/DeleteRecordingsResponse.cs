// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents the expected response schema for DeleteRecordings.
    /// </summary>
    public class DeleteRecordingsResponse : FunctionResponseBase
    {
        /// <summary>
        /// Gets or sets the SID of the call resource.
        /// </summary>
        public string CallSid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all recordings associated with the call were deleted.
        /// </summary>
        public bool AreAllRecordingsDeleted { get; set; }
    }
}
