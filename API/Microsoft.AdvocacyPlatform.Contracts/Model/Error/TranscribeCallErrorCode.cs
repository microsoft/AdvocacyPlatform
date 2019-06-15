// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Error codes specific to the TranscribeCallHttpTrigger function.
    /// </summary>
    public enum TranscribeCallErrorCode
    {
        /// <summary>
        /// Request body missing callSid field
        /// </summary>
        RequestMissingCallSid = 1,

        /// <summary>
        /// Request body missing recordingUri field
        /// </summary>
        RequestMissingRecordingUri = 2,

        /// <summary>
        /// Transcription was canceled.
        /// </summary>
        TranscriptionCanceled = 1000,

        /// <summary>
        /// Transcription is empty.
        /// </summary>
        TranscriptionEmpty = 1001,
    }
}
