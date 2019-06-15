// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Error codes specific to the PullRecordingHttpTrigger function.
    /// </summary>
    public enum PullRecordingErrorCode
    {
        /// <summary>
        /// Request body missing inputId field.
        /// </summary>
        RequestMissingInputId = 1,

        /// <summary>
        /// Request body missing callSid field.
        /// </summary>
        RequestMissingCallSid = 2,
    }
}
