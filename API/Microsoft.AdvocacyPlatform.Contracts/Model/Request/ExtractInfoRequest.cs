// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Represents the expected request schema to ExtractInfo.
    /// </summary>
    public class ExtractInfoRequest : FunctionRequestBase
    {
        /// <summary>
        /// The expected key for callSid.
        /// </summary>
        public const string CallSidKeyName = "callsid";

        /// <summary>
        /// The expected key for text.
        /// </summary>
        public const string TextKeyName = "text";

        /// <summary>
        /// Gets or sets the SID of the call resource.
        /// </summary>
        public string CallSid { get; set; }

        /// <summary>
        /// Gets or sets the transcribed text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the data transformations requested for the input text.
        /// </summary>
        public ICollection<DataTransformation> Transformations { get; set; }

        /// <summary>
        /// Gets or sets the maximum length to use with the trim strategy.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum DateTime value accepted for date extraction. Values below this threshold will set the extracted datetime to null.
        /// </summary>
        public DateTime? MinDateTime { get; set; }

        /// <summary>
        /// Validates model.
        /// </summary>
        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.CallSid))
            {
                throw new MalformedRequestBodyException<ExtractInfoErrorCode>(ExtractInfoErrorCode.RequestMissingCallSid, CallSidKeyName);
            }

            if (string.IsNullOrWhiteSpace(this.Text))
            {
                throw new MalformedRequestBodyException<ExtractInfoErrorCode>(ExtractInfoErrorCode.RequestMissingText, TextKeyName);
            }
        }
    }
}
