// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents the expected response schema for InitiateCall.
    /// </summary>
    public class InitiateCallResponse : FunctionResponseBase
    {
        /// <summary>
        /// Gets or sets the input id used in the call.
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// Gets or sets the type of input id being used (allows for validation if a known type).
        /// </summary>
        public string InputType { get; set; }

        /// <summary>
        /// Gets or sets the form the input id was accepted as based on modifications made in input validators. This may not be the same form as it was sent.
        /// </summary>
        public string AcceptedInputId { get; set; }

        /// <summary>
        /// Gets or sets the SID of the call resource.
        /// </summary>
        public string CallSid { get; set; }
    }
}
