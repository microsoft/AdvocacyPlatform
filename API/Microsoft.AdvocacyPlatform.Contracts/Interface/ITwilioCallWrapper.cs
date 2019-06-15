// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Twilio.Rest.Api.V2010.Account;

    /// <summary>
    /// Interface for interacting with a Twilio call wrapper.
    /// </summary>
    public interface ITwilioCallWrapper
    {
        /// <summary>
        /// Initialize TwilioClient.
        /// </summary>
        /// <param name="twilioAccountSidSecretName">Represents the name of the secret in the secret store with the Twilio account SID value.</param>
        /// <param name="twilioAuthTokenSecretName">Represents the name of the secret in the secret store with the Twilio authentication token value.</param>
        /// <param name="authority">The token-issuing authority.</param>
        /// <returns>An asynchronous task.</returns>
        Task InitializeAsync(string twilioAccountSidSecretName, string twilioAuthTokenSecretName, string authority);

        /// <summary>
        /// Starts and records a new call.
        /// </summary>
        /// <param name="twiMLUrl">The TwiML URL to use when placing the call.</param>
        /// <param name="numberToCall">The number to call.</param>
        /// <param name="twilioLocalNumber">The local number to call from.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the call SID of the call resource.</returns>
        Task<string> PlaceAndRecordCallAsync(
            Uri twiMLUrl,
            string numberToCall,
            string twilioLocalNumber,
            ILogger log);

        /// <summary>
        /// Gets a call resource.
        /// </summary>
        /// <param name="callSid">The SID of the call resource.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the call resource.</returns>
        Task<CallResource> FetchCallAsync(string callSid, ILogger log);

        /// <summary>
        /// Hangs up a call.
        /// </summary>
        /// <param name="callSid">The SID of the call resource.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning true if successful and false if failed.</returns>
        Task<bool> HangupCallAsync(string callSid, ILogger log);

        /// <summary>
        /// Deletes a call.
        /// </summary>
        /// <param name="callSid">The SID of the call resource.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning true if successful and false if failed.</returns>
        Task<bool> DeleteCallAsync(string callSid, ILogger log);

        /// <summary>
        /// Gets the status of a call.
        /// </summary>
        /// <param name="callSid">The SID of the call resource.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the status of the call.</returns>
        Task<CallResource.StatusEnum> FetchStatusAsync(string callSid, ILogger log);

        /// <summary>
        /// Gets recordings associated with a call.
        /// </summary>
        /// <param name="callSid">The SID of the call resource.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning a collection of recording resources for the call.</returns>
        Task<IList<RecordingResource>> FetchRecordingsAsync(string callSid, ILogger log);

        /// <summary>
        /// Gets recordings associated with the current account.
        /// </summary>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning a collection of recording resources for the current account.</returns>
        Task<IList<RecordingResource>> FetchAccountRecordingsAsync(ILogger log);

        /// <summary>
        /// Deletes all recordings associated with a call.
        /// </summary>
        /// <param name="callSid">The SID of the call resource.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning true if successful and false if failed.</returns>
        Task<bool> DeleteRecordingsAsync(string callSid, ILogger log);

        /// <summary>
        /// Deletes all recordings associated with the current account.
        /// </summary>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning true if successful and false if failed.</returns>
        Task<bool> DeleteAccountRecordingsAsync(ILogger log);

        /// <summary>
        /// Returns the full recording URI for a recording.
        /// </summary>
        /// <param name="recording">The recording resource.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>The full recording URI.</returns>
        Uri GetFullRecordingUri(RecordingResource recording, ILogger log);
    }
}
