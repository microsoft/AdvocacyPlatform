// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests.Mocks.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
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
    public class TwilioCallWrapperMock : ITwilioCallWrapper
    {
        private HttpClientMock _httpClient;

        private ConcurrentDictionary<string, Exception> _expectedResponseExceptionsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwilioCallWrapperMock"/> class.
        /// </summary>
        /// <param name="httpClient">The IHttpClientWrapper implementation to use for making REST calls.</param>
        /// <param name="secretStore">The ISecretStore implementation for obtaining secrets.</param>
        public TwilioCallWrapperMock(
            IHttpClientWrapper httpClient,
            ISecretStore secretStore)
        {
            _httpClient = httpClient as HttpClientMock;

            _expectedResponseExceptionsCache = new ConcurrentDictionary<string, Exception>();
        }

        /// <summary>
        /// Registers an expected exception.
        /// </summary>
        /// <param name="key">Unique key for the expected request.</param>
        /// <param name="ex">The exception to throw.</param>
        public void RegisterExpectedRequestException(string key, Exception ex)
        {
            if (_expectedResponseExceptionsCache.ContainsKey(key))
            {
                throw new ArgumentException($"An expected request exception has already been registered for '{key}'!");
            }

            _expectedResponseExceptionsCache.TryAdd(key, ex);
        }

        /// <summary>
        /// Simulates initializing the Twilio client.
        /// </summary>
        /// <param name="twilioAccountSidSecretName">The name of the secret with the Twilio account SID value.</param>
        /// <param name="twilioAuthTokenSecretName">The name of the secret with the Twilio auth token value.</param>
        /// <param name="authority">The authorizing authority.</param>
        /// <returns>An asynchronous task.</returns>
        public Task InitializeAsync(string twilioAccountSidSecretName, string twilioAuthTokenSecretName, string authority)
        {
            return Task.Run(() =>
            {
                if (_expectedResponseExceptionsCache.ContainsKey(twilioAccountSidSecretName))
                {
                    throw _expectedResponseExceptionsCache[twilioAccountSidSecretName];
                }

                return;
            });
        }

        /// <summary>
        /// Simulates deleting a Twilio call resource.
        /// </summary>
        /// <param name="callSid">The expected call SID.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>True if successful, false if failed.</returns>
        public Task<bool> DeleteCallAsync(string callSid, ILogger log)
        {
            return Task.Run(() =>
            {
                if (_expectedResponseExceptionsCache.ContainsKey(callSid))
                {
                    throw _expectedResponseExceptionsCache[callSid];
                }

                return true;
            });
        }

        /// <summary>
        /// Simulates delete multiple Twilio call recordings.
        /// </summary>
        /// <param name="callSid">The expected call SID.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>True if successful, false if failed.</returns>
        public Task<bool> DeleteRecordingsAsync(string callSid, ILogger log)
        {
            return Task.Run(() =>
            {
                if (_expectedResponseExceptionsCache.ContainsKey(callSid))
                {
                    throw _expectedResponseExceptionsCache[callSid];
                }

                return true;
            });
        }

        /// <summary>
        /// Simulates deleting all Twilio account recordings.
        /// </summary>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>True if successful, false if failed.</returns>
        public Task<bool> DeleteAccountRecordingsAsync(ILogger log)
        {
            return Task.Run(() =>
            {
                if (_expectedResponseExceptionsCache.Count > 0)
                {
                    throw _expectedResponseExceptionsCache.First().Value;
                }

                return true;
            });
        }

        /// <summary>
        /// Simulates fetching a Twilio call resource.
        /// </summary>
        /// <param name="callSid">The expected call SID.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The expected call resource.</returns>
        public Task<CallResource> FetchCallAsync(string callSid, ILogger log)
        {
            return Task.Run(() =>
            {
                if (_expectedResponseExceptionsCache.ContainsKey(callSid))
                {
                    throw _expectedResponseExceptionsCache[callSid];
                }

                return (CallResource)null;
            });
        }

        /// <summary>
        /// Simulates fetching Twilio call recordings.
        /// </summary>
        /// <param name="callSid">The expected call SID.</param>
        /// <param name="log">Trace logger instances.</param>
        /// <returns>The expected call recordings.</returns>
        public Task<IList<RecordingResource>> FetchRecordingsAsync(string callSid, ILogger log)
        {
            return Task.Run(() =>
            {
                if (_expectedResponseExceptionsCache.ContainsKey(callSid))
                {
                    throw _expectedResponseExceptionsCache[callSid];
                }

                return (IList<RecordingResource>)null;
            });
        }

        /// <summary>
        /// Simulates fetching all call recordings in a Twilio account.
        /// </summary>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The expected recordings.</returns>
        public Task<IList<RecordingResource>> FetchAccountRecordingsAsync(ILogger log)
        {
            return Task.Run(() =>
            {
                if (_expectedResponseExceptionsCache.Count > 0)
                {
                    throw _expectedResponseExceptionsCache.First().Value;
                }

                return (IList<RecordingResource>)null;
            });
        }

        /// <summary>
        /// Simulates fetching the status of a Twilio call.
        /// </summary>
        /// <param name="callSid">The expected call SID.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The expected call status.</returns>
        public Task<CallResource.StatusEnum> FetchStatusAsync(string callSid, ILogger log)
        {
            return Task.Run(() =>
            {
                if (_expectedResponseExceptionsCache.ContainsKey(callSid))
                {
                    throw _expectedResponseExceptionsCache[callSid];
                }

                return (CallResource.StatusEnum)null;
            });
        }

        /// <summary>
        /// Simulates hanging up a Twilio call.
        /// </summary>
        /// <param name="callSid">The expected call SID.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>True if successful, false if failed.</returns>
        public Task<bool> HangupCallAsync(string callSid, ILogger log)
        {
            return Task.Run(() =>
            {
                if (_expectedResponseExceptionsCache.ContainsKey(callSid))
                {
                    throw _expectedResponseExceptionsCache[callSid];
                }

                return true;
            });
        }

        /// <summary>
        /// Simulates placing and recording a Twilio call.
        /// </summary>
        /// <param name="twiMLUrl">The expected TwiML.</param>
        /// <param name="numberToCall">The expected number to call.</param>
        /// <param name="twilioLocalNumber">The expected local number to use to place the call.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The initial response.</returns>
        public Task<string> PlaceAndRecordCallAsync(
            Uri twiMLUrl,
            string numberToCall,
            string twilioLocalNumber,
            ILogger log)
        {
            return Task.Run(() =>
            {
                if (_expectedResponseExceptionsCache.ContainsKey(numberToCall))
                {
                    throw _expectedResponseExceptionsCache[numberToCall];
                }

                return (string)null;
            });
        }

        /// <summary>
        /// Gets the full recording URI.
        /// </summary>
        /// <param name="recording">The recording resource.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The full recording URI.</returns>
        public Uri GetFullRecordingUri(RecordingResource recording, ILogger log)
        {
            return null;
        }
    }
}
