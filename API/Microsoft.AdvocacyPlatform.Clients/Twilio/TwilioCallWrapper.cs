// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.Logging;
    using Twilio;
    using Twilio.Base;
    using Twilio.Clients;
    using Twilio.Http;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;

    /// <summary>
    /// Wrapper for making Twilio calls.
    /// </summary>
    public class TwilioCallWrapper : ITwilioCallWrapper
    {
        /// <summary>
        /// Base URL for Twilio REST calls.
        ///
        /// Base is mentioned here: https://www.twilio.com/docs/voice/api/recording .
        /// </summary>
        public const string TwilioUriBase = "https://api.twilio.com";

        /// <summary>
        /// Internal IHttpClientWrapper instance.
        /// </summary>
        private IHttpClientWrapper _httpClient;

        /// <summary>
        /// Internal ISecretStore instance.
        /// </summary>
        private ISecretStore _secretStore;

        /// <summary>
        /// Internal ITwilioRestClient instance.
        /// </summary>
        private ITwilioRestClient _twilioClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioCallWrapper"/> class.
        /// </summary>
        /// <param name="httpClient">The IHttpClientWrapper implementation to use for making REST calls.</param>
        /// <param name="secretStore">The ISecretStore implementation for obtaining secrets.</param>
        public TwilioCallWrapper(
            IHttpClientWrapper httpClient,
            ISecretStore secretStore)
        {
            _httpClient = httpClient;
            _secretStore = secretStore;
        }

        /// <summary>
        /// Initializes the Twilio client.
        /// </summary>
        /// <param name="twilioAccountSidSecretName">The SSID of the Twilio account to authenticate as.</param>
        /// <param name="twilioAuthTokenSecretName">The auth token to use for authentication.</param>
        /// <param name="authority">The authority to authenticate against.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task InitializeAsync(string twilioAccountSidSecretName, string twilioAuthTokenSecretName, string authority)
        {
            Secret twilioAccountSid = await _secretStore.GetSecretAsync(twilioAccountSidSecretName, authority);
            Secret twilioAuthToken = await _secretStore.GetSecretAsync(twilioAuthTokenSecretName, authority);

            Twilio.Http.HttpClient twilioHttpClient = new SystemNetHttpClient(_httpClient.GetHttpClient());

            _twilioClient = new TwilioRestClient(twilioAccountSid.Value, twilioAuthToken.Value, httpClient: twilioHttpClient);
        }

        /// <summary>
        /// Deletes the call with the given call sid.
        /// </summary>
        /// <param name="callSid">The SID of the Twilio call resource.</param>
        /// <param name="log">A logger instance.</param>
        /// <returns>A Task returning true if successful and false if failed.</returns>
        public async Task<bool> DeleteCallAsync(string callSid, ILogger log)
        {
            log.LogInformation($"Attempting to delete Twilio call resource '{callSid}'...");
            return await CallResource.DeleteAsync(callSid, client: _twilioClient);
        }

        /// <summary>
        /// Delete all recordings for given call sid.
        /// </summary>
        /// <param name="callSid">the SID of the Twilio call resource the recordings are associated with.</param>
        /// <param name="log">A logger instance.</param>
        /// <returns>A Task returning true if successful and false if failed.</returns>
        public async Task<bool> DeleteRecordingsAsync(string callSid, ILogger log)
        {
            log.LogInformation($"Attempting to retrieve Twilio recordings for call resource '{callSid}'...");

            ICollection<RecordingResource> recordings = await FetchRecordingsAsync(callSid, log);

            bool hasError = false;

            log.LogInformation($"Received {recordings.Count} recordings. Enumerating...");
            foreach (RecordingResource recording in recordings)
            {
                string recordingSid = recording.Sid;
                log.LogInformation($"Attempting to delete recording '{recordingSid}' from Twilio...");

                bool result = await RecordingResource.DeleteAsync(recordingSid, client: _twilioClient);

                if (result)
                {
                    log.LogInformation($"Recording '{recordingSid}' deleted successfully.");
                }
                else
                {
                    log.LogError($"Failed to delete recording '{recordingSid}'.");
                    hasError = true;
                }
            }

            return !hasError;
        }

        /// <summary>
        /// Delete all recordings for the current account.
        /// </summary>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning true if successful and false if failed.</returns>
        public async Task<bool> DeleteAccountRecordingsAsync(ILogger log)
        {
            log.LogInformation("Attempting to retrieve Twilio recordings for the current account...");

            ICollection<RecordingResource> recordings = await FetchAccountRecordingsAsync(log);

            bool hasError = false;

            log.LogInformation($"Received {recordings.Count} recordings. Enumerating...");
            foreach (RecordingResource recording in recordings)
            {
                string recordingSid = recording.Sid;
                log.LogInformation($"Attempting to delete recording '{recordingSid}' from Twilio...");

                bool result = await RecordingResource.DeleteAsync(recordingSid, client: _twilioClient);

                if (result)
                {
                    log.LogInformation($"Recording '{recordingSid}' deleted successfully.");
                }
                else
                {
                    log.LogError($"Failed to delete recording '{recordingSid}'.");
                    hasError = true;
                }
            }

            return !hasError;
        }

        /// <summary>
        /// Retrieves a call object from the call sid.
        /// </summary>
        /// <param name="callSid">The SID of the call resource to retrieve.</param>
        /// <param name="log">A logger instance.</param>
        /// <returns>A Task returning the call resource.</returns>
        public async Task<CallResource> FetchCallAsync(string callSid, ILogger log)
        {
            log.LogInformation($"Attempting to fetch call '{callSid}' from Twilio...");
            return await CallResource.FetchAsync(callSid, client: _twilioClient);
        }

        /// <summary>
        /// Get list of recordings for given call sid.
        /// </summary>
        /// <param name="callSid">The SID of the call resource to get recordings for.</param>
        /// <param name="log">A logger instance.</param>
        /// <returns>A Task returning a collection of recording resources.</returns>
        public async Task<IList<RecordingResource>> FetchRecordingsAsync(string callSid, ILogger log)
        {
            log.LogInformation($"Attempting to fetch recordings for call '{callSid}' from Twilio...");
            ResourceSet<RecordingResource> recordings = await RecordingResource.ReadAsync(callSid: callSid, client: _twilioClient);

            return new List<RecordingResource>(recordings);
        }

        /// <summary>
        /// Get list of recordings for given call sid.
        /// </summary>
        /// <param name="log">A logger instance.</param>
        /// <returns>A Task returning a collection of recording resources.</returns>
        public async Task<IList<RecordingResource>> FetchAccountRecordingsAsync(ILogger log)
        {
            log.LogInformation($"Attempting to fetch recordings for the current account from Twilio...");
            ResourceSet<RecordingResource> recordings = await RecordingResource.ReadAsync(pathAccountSid: _twilioClient.AccountSid, client: _twilioClient);

            return new List<RecordingResource>(recordings);
        }

        /// <summary>
        /// Get the status of the call with the given call sid.
        /// </summary>
        /// <param name="callSid">The SID of the call resource to get the status of.</param>
        /// <param name="log">A logger instance.</param>
        /// <returns>A Task returning the call status.</returns>
        public async Task<CallResource.StatusEnum> FetchStatusAsync(string callSid, ILogger log)
        {
            log.LogInformation($"Attempting to fetch status of call '{callSid}' from Twilio...");
            CallResource call = await FetchCallAsync(callSid, log);

            return call.Status;
        }

        /// <summary>
        /// Constructs the full recording URI of the Twilio recording resource.
        /// </summary>
        /// <param name="recording">The recording resource.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>The URI of the recording.</returns>
        public Uri GetFullRecordingUri(RecordingResource recording, ILogger log)
        {
            return GetTwilioUri(recording.Uri);
        }

        /// <summary>
        /// Ends a call if it is still in progress
        ///
        /// See: https://www.twilio.com/docs/voice/tutorials/how-to-modify-calls-in-progress-python .
        /// </summary>
        /// <param name="callSid">The SID of the call resource.</param>
        /// <param name="log">A logger instance.</param>
        /// <returns>A Task returning true if successful and false if failed.</returns>
        public async Task<bool> HangupCallAsync(string callSid, ILogger log)
        {
            log.LogInformation($"Checking with Twilio to see if call '{callSid}' exists and is not completed...");
            CallResource call = await FetchCallAsync(callSid, log);

            if (call.Status != CallResource.StatusEnum.Completed)
            {
                log.LogInformation($"Attempting to hangup call '{callSid}'...");
                await CallResource.UpdateAsync(callSid, status: CallResource.UpdateStatusEnum.Completed, client: _twilioClient);

                return true;
            }
            else
            {
                log.LogInformation($"Call '{callSid}' has already completed.");

                return false;
            }
        }

        /// <summary>
        /// Places a call which is recorded.
        /// </summary>
        /// <param name="twiMLUrl">The TwiML URL to use when making the call.</param>
        /// <param name="numberToCall">The number to call.</param>
        /// <param name="twilioLocalNumber">The local number to use to make the call.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the SID of the call resource.</returns>
        public async Task<string> PlaceAndRecordCallAsync(
            Uri twiMLUrl,
            string numberToCall,
            string twilioLocalNumber,
            ILogger log)
        {
            log.LogInformation($"Received the following TwiML URL: {twiMLUrl.AbsoluteUri}");

            log.LogInformation($"Attempting to place outbound call with Twilio from {twilioLocalNumber} to {numberToCall}...");
            CallResource call = await CallResource.CreateAsync(
                to: new PhoneNumber(numberToCall),
                from: new PhoneNumber(twilioLocalNumber),
                url: twiMLUrl,
                record: true,
                client: _twilioClient);

            return call.Sid;
        }

        /// <summary>
        /// Returns the full URI for a Twilio recording.
        /// </summary>
        /// <param name="uriFromRecording">Relative recording URI.</param>
        /// <returns>The full URI for the recording.</returns>
        public Uri GetTwilioUri(string uriFromRecording)
        {
            return new Uri($"{TwilioUriBase}{uriFromRecording.Split(".json")[0]}");
        }
    }
}
