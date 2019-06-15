// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Common error codes
    ///
    /// These should be negative integers .
    /// to avoid collision with function specific status codes.
    /// </summary>
    public enum CommonErrorCode
    {
        // Generic Errors [-1,-999]

        /// <summary>
        /// There was no error.
        /// </summary>
        NoError = 0,

        /// <summary>
        /// The request body was malformed.
        /// </summary>
        MalformedRequestBody = -1,

        /// <summary>
        /// Bad request.
        /// </summary>
        BadRequest = -2,

        // Twilio Errors [-1000,-1999]

        /// <summary>
        /// Twilio generic failure.
        /// </summary>
        TwilioGenericFailure = -1000,

        /// <summary>
        /// Twilio authentication failed.
        /// </summary>
        TwilioAuthenticationFailed = -1050,

        /// <summary>
        /// Twilio API connection failed.
        /// </summary>
        TwilioApiConnectionFailed = -1100,

        /// <summary>
        /// Twilio API request failed.
        /// </summary>
        TwilioApiRequestFailed = -1150,

        /// <summary>
        /// Twilio REST call failed.
        /// </summary>
        TwilioRestCallFailed = -1200,

        /// <summary>
        /// Twilio automated call failed.
        /// </summary>
        TwilioCallFailed = -1250,

        /// <summary>
        /// Twilio call has no recordings.
        /// </summary>
        TwilioCallNoRecordings = -1251,

        /// <summary>
        /// Twilio returned an expected call status.
        /// </summary>
        TwilioUnexpectedCallStatus = -1300,

        // Secret Store [-2000,-2999]

        /// <summary>
        /// Generic secret store failure.
        /// </summary>
        SecretStoreGenericFailure = -2000,

        // Storage Client [-4000, -4999]

        /// <summary>
        /// Generic storage client failure.
        /// </summary>
        StorageClientGenericFailure = -4000,
    }
}
