// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.Logging;
    using Twilio;
    using TwilioHttp = Twilio.Http;

    /// <summary>
    /// HttpClient wrapper for making REST calls.
    /// </summary>
    public class HttpClientWrapper : IHttpClientWrapper
    {
        /// <summary>
        /// Internal HttpClient instance.
        /// </summary>
        private HttpClient _httpClient;

        /// <summary>
        /// Internal Twilio HttpClient instance.
        /// </summary>
        private TwilioHttp.HttpClient _twilioHttpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientWrapper"/> class.
        /// </summary>
        public HttpClientWrapper()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="HttpClientWrapper"/> class.
        /// </summary>
        ~HttpClientWrapper()
        {
            _httpClient.Dispose();
            _httpClient = null;
        }

        /// <summary>
        /// Gets the HttpClient instance.
        /// </summary>
        /// <returns>The HttpClient instance.</returns>
        public HttpClient GetHttpClient()
        {
            return _httpClient;
        }

        /// <summary>
        /// Initializes the Twilio client.
        /// </summary>
        /// <param name="accountSid">The account SID to authenticate with Twilio as.</param>
        /// <param name="authToken">The account token to authenticate with Twilio with.</param>
        public void InitTwilioClient(Secret accountSid, Secret authToken)
        {
            TwilioClient.Init(accountSid.Value, authToken.Value);
            _twilioHttpClient = TwilioClient.GetRestClient().HttpClient;
        }

        /// <summary>
        /// Returns the Twilio HttpClient.
        /// </summary>
        /// <returns>The Twilio HttpClient.</returns>
        public TwilioHttp.HttpClient GetTwilioHttpClient()
        {
            return _twilioHttpClient;
        }

        /// <summary>
        /// Performs a GET request.
        /// </summary>
        /// <param name="requestUri">The URI to perform the GET request to.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the response.</returns>
        public Task<HttpResponseMessage> GetAsync(string requestUri, ILogger log)
        {
            return _httpClient.GetAsync(requestUri);
        }

        /// <summary>
        /// Performs a GET request.
        /// </summary>
        /// <param name="requestUri">The URI to GET.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the response as a stream.</returns>
        public Task<Stream> GetStreamAsync(string requestUri, ILogger log)
        {
            return _httpClient.GetStreamAsync(requestUri);
        }

        /// <summary>
        /// Performs a POST request.
        /// </summary>
        /// <param name="requestUri">The URI to POST to.</param>
        /// <param name="content">The content of the request.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the response message.</returns>
        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, ILogger log)
        {
            return _httpClient.PostAsync(requestUri, content);
        }
    }
}
