// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Represents the expected request schema to DeleteRecordings.
    /// </summary>
    public class DeleteRecordingsRequest : FunctionRequestBase
    {
        /// <summary>
        /// The expected key for callSid.
        /// </summary>
        public const string CallSidKeyName = "callsid";

        /// <summary>
        /// Gets or sets the SID of the call resource.
        /// </summary>
        public string CallSid { get; set; }

        /// <summary>
        /// Validates model.
        /// </summary>
        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.CallSid))
            {
                throw new MalformedRequestBodyException<DeleteRecordingsErrorCode>(DeleteRecordingsErrorCode.RequestMissingCallSid, CallSidKeyName);
            }
        }
    }
}
