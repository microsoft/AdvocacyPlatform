// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents the expected response schema for DeleteAllRecordings.
    /// </summary>
    public class DeleteAccountRecordingsResponse : FunctionResponseBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether all recordings associated with the account were deleted.
        /// </summary>
        public bool AreAllRecordingsDeleted { get; set; }
    }
}
