// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Twilio.Rest.Api.V2010.Account;

    /// <summary>
    /// Exception thrown when a Twilio call fails.
    /// </summary>
    public class TwilioFailedCallException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioFailedCallException"/> class.
        /// </summary>
        /// <param name="status">The final call status.</param>
        /// <param name="message">Additional message regarding failure.</param>
        public TwilioFailedCallException(CallResource.StatusEnum status, string message)
            : base(message)
        {
            Status = status;
        }

        /// <summary>
        /// Gets or sets the final call status.
        /// </summary>
        public CallResource.StatusEnum Status { get; set; }
    }
}
