// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Common error messages.
    /// </summary>
    public static class CommonErrorMessage
    {
        /// <summary>
        /// Bad request.
        /// </summary>
        public const string BadRequestMessage = "Bad request.";

        /// <summary>
        /// Generic Twilio failure.
        /// </summary>
        public const string TwilioGenericFailureMessage = "Twilio encountered an unexpected failure.";

        /// <summary>
        /// Twilio authentication failed.
        /// </summary>
        public const string TwilioAuthenticationFailedMessage = "Twilio encountered an error during authentication.";

        /// <summary>
        /// Twilio API connection failed.
        /// </summary>
        public const string TwilioApiConnectionFailedMessage = "Twilio failed to connect to the API.";

        /// <summary>
        /// Twilio API request failed.
        /// </summary>
        public const string TwilioApiRequestFailedMessage = "Twilio failed during an API call.";

        /// <summary>
        /// Twilio REST call failed.
        /// </summary>
        public const string TwilioRestCallFailedMessage = "Twilio failed during a REST call.";

        /// <summary>
        /// Generic secret store failure.
        /// </summary>
        public const string SecretStoreGenericFailureMessage = "An unexpected failure encountered with secret store.";

        /// <summary>
        /// Generic storage client failure.
        /// </summary>
        public const string StorageClientGenericFailureMessage = "An unexpected failure encountered with the storage client.";
    }
}
