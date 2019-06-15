// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Represents the expected request schema to InitiateCall.
    /// </summary>
    public class InitiateCallRequest : FunctionRequestBase
    {
        /// <summary>
        /// The expected key for the inputId.
        /// </summary>
        public const string InputIdKeyName = "inputId";

        /// <summary>
        /// Gets or sets the input id to use with the call.
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// Gets or sets the type of input id being used (allows for validation if a known type).
        /// </summary>
        public string InputType { get; set; }

        /// <summary>
        /// Gets or sets the Dual-tone Multi-frequency signaling sequence to use with the call.
        /// </summary>
        public DtmfRequest Dtmf { get; set; }

        /// <summary>
        /// Validates model.
        /// </summary>
        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.InputId))
            {
                throw new MalformedRequestBodyException<InitiateCallErrorCode>(InitiateCallErrorCode.RequestMissingInputId, InputIdKeyName);
            }
        }
    }
}
