// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Twilio.Rest.Api.V2010.Account;

    /// <summary>
    /// Exception thrown when no recordings for a Twilio call could be found.
    /// </summary>
    public class TwilioNoRecordingsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioNoRecordingsException"/> class.
        /// </summary>
        /// <param name="message">Additional message describing the exception.</param>
        public TwilioNoRecordingsException(string message)
            : base(message)
        {
        }
    }
}
