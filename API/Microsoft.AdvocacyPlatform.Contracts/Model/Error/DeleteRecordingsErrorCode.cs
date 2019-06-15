// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Error codes specific to the DeleteRecordingsHttpTrigger function.
    /// </summary>
    public enum DeleteRecordingsErrorCode
    {
        /// <summary>
        /// Request body missing callSid field
        /// </summary>
        RequestMissingCallSid = 1,
    }
}
