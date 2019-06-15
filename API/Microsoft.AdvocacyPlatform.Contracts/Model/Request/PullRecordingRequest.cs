// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Represents the expected request schema to PullRecording.
    /// </summary>
    public class PullRecordingRequest : FunctionRequestBase
    {
        /// <summary>
        /// The key expected for inputId.
        /// </summary>
        public const string InputIdKeyName = "inputId";

        /// <summary>
        /// The key expected for callSid.
        /// </summary>
        public const string CallSidKeyName = "callsid";

        /// <summary>
        /// Gets or sets the input id to use when pulling the recording.
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// Gets or sets the SID of the call resource.
        /// </summary>
        public string CallSid { get; set; }

        /// <summary>
        /// Validates model.
        /// </summary>
        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.InputId))
            {
                throw new MalformedRequestBodyException<PullRecordingErrorCode>(PullRecordingErrorCode.RequestMissingInputId, InputIdKeyName);
            }

            if (string.IsNullOrWhiteSpace(this.CallSid))
            {
                throw new MalformedRequestBodyException<PullRecordingErrorCode>(PullRecordingErrorCode.RequestMissingCallSid, CallSidKeyName);
            }
        }
    }
}
