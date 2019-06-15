// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests.Mocks.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.Logging;
    using Twilio;
    using TwilioHttp = Twilio.Http;

    /// <summary>
    /// Mocks an IHttpClientWrapper.
    /// </summary>
    public class HttpClientMock : IHttpClientWrapper
    {
        private HttpMessageHandlerMock _messageHandler;
        private HttpClient _httpClient;
        private ConcurrentDictionary<string, string> _expectedRequestCache;
        private ConcurrentDictionary<string, HttpResponseMessage> _expectedResponseCache;
        private ConcurrentDictionary<string, HttpRequestException> _expectedRequestExceptionsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientMock"/> class.
        /// </summary>
        public HttpClientMock()
        {
            _expectedRequestCache = new ConcurrentDictionary<string, string>();
            _expectedResponseCache = new ConcurrentDictionary<string, HttpResponseMessage>();
            _expectedRequestExceptionsCache = new ConcurrentDictionary<string, HttpRequestException>();

            _messageHandler = new HttpMessageHandlerMock();
            _httpClient = new HttpClient(_messageHandler);
        }

        /// <summary>
        /// Gets the mock HTTP message handler.
        /// </summary>
        public HttpMessageHandlerMock MessageHandler => _messageHandler;

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
        /// <param name="accountSid">The SID of the account to connect as.</param>
        /// <param name="authToken">The auth token for the account to connect as.</param>
        public void InitTwilioClient(Secret accountSid, Secret authToken)
        {
            TwilioClient.Init(accountSid.Value, authToken.Value);
        }

        /// <summary>
        /// Registers an expected request.
        /// </summary>
        /// <param name="requestUri">The URI for the expected request.</param>
        /// <param name="content">The expected content in the request.</param>
        /// <returns>True if registered successfully, false if failed.</returns>
        public bool RegisterExpectedRequestMessage(string requestUri, string content)
        {
            if (_expectedResponseCache.ContainsKey(requestUri))
            {
                throw new InvalidOperationException("A request message for this URI already exists!");
            }

            return _expectedRequestCache.TryAdd(requestUri, content);
        }

        /// <summary>
        /// Registers an expected exception.
        /// </summary>
        /// <param name="requestUri">The URI associated with the exception.</param>
        /// <param name="ex">The exception to throw.</param>
        /// <returns>True if registered successfully, false if failed.</returns>
        public bool RegisterExpectedRequestException(string requestUri, HttpRequestException ex)
        {
            if (_expectedRequestExceptionsCache.ContainsKey(requestUri))
            {
                throw new InvalidOperationException("A request exception for this URI already exists!");
            }

            return _expectedRequestExceptionsCache.TryAdd(requestUri, ex);
        }

        /// <summary>
        /// Registers an expected response.
        /// </summary>
        /// <param name="requestUri">The expected request URI for the response.</param>
        /// <param name="message">The expected HTTP response.</param>
        /// <returns>True if registered successfully, false if failed.</returns>
        public bool RegisterExpectedResponseMessage(string requestUri, HttpResponseMessage message)
        {
            if (_expectedResponseCache.ContainsKey(requestUri))
            {
                throw new InvalidOperationException("A response message for this URI already exists!");
            }

            return _expectedResponseCache.TryAdd(requestUri, message);
        }

        /// <summary>
        /// Clears the cache of expected response messages.
        /// </summary>
        public void ClearExpectedResponseMessages()
        {
            _expectedResponseCache.Clear();
        }

        /// <summary>
        /// Simulates an HTTP GET request.
        /// </summary>
        /// <param name="requestUri">The expected request URI.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>An HTTP response.</returns>
        public Task<HttpResponseMessage> GetAsync(string requestUri, ILogger log)
        {
            return Task.Run(() =>
            {
                return _expectedResponseCache[requestUri];
            });
        }

        /// <summary>
        /// Simulates an HTTP POST request.
        /// </summary>
        /// <param name="requestUri">The expected request URI.</param>
        /// <param name="content">The expected request content.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>An HTTP response.</returns>
        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, ILogger log)
        {
            if (!_expectedRequestCache.ContainsKey(requestUri))
            {
                if (!_expectedRequestExceptionsCache.ContainsKey(requestUri))
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
                else
                {
                    throw _expectedRequestExceptionsCache[requestUri];
                }
            }

            string expectedContent = _expectedRequestCache[requestUri];
            string requestContent = await content.ReadAsStringAsync();

            if (string.Compare(expectedContent, requestContent) != 0)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return _expectedResponseCache[requestUri];
        }

        /// <summary>
        /// Simulates an HTTP GET request.
        /// </summary>
        /// <param name="requestUri">The expected request URI.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The HTTP response stream.</returns>
        public Task<Stream> GetStreamAsync(string requestUri, ILogger log)
        {
            return Task.Run(() =>
            {
                if (!_expectedRequestCache.ContainsKey(requestUri))
                {
                    throw new ArgumentException("This request is not expected!");
                }

                return new MemoryStream() as Stream;
            });
        }
    }
}
