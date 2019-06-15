// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Twilio.Rest.Api.V2010.Account;

    /// <summary>
    /// Exception thrown when the returned call status is unknown.
    /// </summary>
    public class TwilioUnknownCallStatusException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioUnknownCallStatusException"/> class.
        /// </summary>
        /// <param name="status">The final call status.</param>
        /// <param name="message">Additional message describing the exception.</param>
        public TwilioUnknownCallStatusException(CallResource.StatusEnum status, string message)
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
