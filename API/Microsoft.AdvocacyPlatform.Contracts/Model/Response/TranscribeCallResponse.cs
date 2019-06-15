// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents the expected response schema for TranscribeCall.
    /// </summary>
    public class TranscribeCallResponse : FunctionResponseBase
    {
        /// <summary>
        /// Gets or sets the SID of the call resource.
        /// </summary>
        public string CallSid { get; set; }

        /// <summary>
        /// Gets or sets the transcribed call text.
        /// </summary>
        public string Text { get; set; }
    }
}
