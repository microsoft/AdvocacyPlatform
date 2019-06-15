// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents the expected response schema for PullRecording.
    /// </summary>
    public class PullRecordingResponse : FunctionResponseBase
    {
        /// <summary>
        /// Gets or sets the SID of the call resource.
        /// </summary>
        public string CallSid { get; set; }

        /// <summary>
        /// Gets or sets the length (in bytes) of the recording in the persistent data store.
        /// </summary>
        public long RecordingLength { get; set; }

        /// <summary>
        /// Gets or sets the relative URI to the recording.
        /// </summary>
        public string RecordingUri { get; set; }

        /// <summary>
        /// Gets or sets the full URL to the recording.
        /// </summary>
        public string FullRecordingUrl { get; set; }
    }
}
