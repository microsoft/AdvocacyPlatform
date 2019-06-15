// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents the expected response schema for ExtractInfo.
    /// </summary>
    public class ExtractInfoResponse : FunctionResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractInfoResponse"/> class.
        /// </summary>
        public ExtractInfoResponse()
        {
            Flags = new List<string>();
        }

        /// <summary>
        /// Gets or sets the SID of the call resource.
        /// </summary>
        public string CallSid { get; set; }

        /// <summary>
        /// Gets or sets the data extracted from the transcription.
        /// </summary>
        public TranscriptionData Data { get; set; }

        /// <summary>
        /// Gets or sets additional flags conveying additional information for the execution.
        /// </summary>
        public List<string> Flags { get; set; }
    }
}
