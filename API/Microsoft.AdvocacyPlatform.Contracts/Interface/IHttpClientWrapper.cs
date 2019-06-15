// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Twilio = Twilio.Http;

    /// <summary>
    /// Interface for interacting with an HttpClient wrapper.
    /// </summary>
    public interface IHttpClientWrapper
    {
        /// <summary>
        /// Initializes the Twilio Client.
        /// </summary>
        /// <param name="accountSid">The account SID to connect with.</param>
        /// <param name="authToken">The auth token to connect with.</param>
        void InitTwilioClient(Secret accountSid, Secret authToken);

        /// <summary>
        /// Returns the wrapped HttpClient.
        /// </summary>
        /// <returns>The HttpClient instance used to make requests.</returns>
        HttpClient GetHttpClient();

        /// <summary>
        /// Performs a GET request.
        /// </summary>
        /// <param name="requestUri">The URI to GET.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>A Task returning the response message.</returns>
        Task<HttpResponseMessage> GetAsync(string requestUri, ILogger log);

        /// <summary>
        /// Performs a GET request.
        /// </summary>
        /// <param name="requestUri">The URI to GET.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>A Task returning the response as a stream.</returns>
        Task<Stream> GetStreamAsync(string requestUri, ILogger log);

        /// <summary>
        /// Performs a POST request.
        /// </summary>
        /// <param name="requestUri">The URI to POST to.</param>
        /// <param name="content">The content of the request.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the response message.</returns>
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, ILogger log);
    }
}
