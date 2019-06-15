// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Represents the expected request schema to TranscribeCall.
    /// </summary>
    public class TranscribeCallRequest : FunctionRequestBase
    {
        /// <summary>
        /// The expected key for callSid.
        /// </summary>
        public const string CallSidKeyName = "callsid";

        /// <summary>
        /// The expected key for recordingUri.
        /// </summary>
        public const string RecordingUriKeyName = "recordinguri";

        /// <summary>
        /// Gets or sets the SID of the call resource.
        /// </summary>
        public string CallSid { get; set; }

        /// <summary>
        /// Gets or sets the URI of the recording.
        /// </summary>
        public string RecordingUri { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the URI is a local path.
        /// </summary>
        public bool IsLocalPath { get; set; }

        /// <summary>
        /// Validates model.
        /// </summary>
        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.CallSid))
            {
                throw new MalformedRequestBodyException<TranscribeCallErrorCode>(TranscribeCallErrorCode.RequestMissingCallSid, CallSidKeyName);
            }

            if (string.IsNullOrWhiteSpace(this.RecordingUri))
            {
                throw new MalformedRequestBodyException<TranscribeCallErrorCode>(TranscribeCallErrorCode.RequestMissingRecordingUri, RecordingUriKeyName);
            }
        }
    }
}
