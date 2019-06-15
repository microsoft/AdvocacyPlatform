// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using AdvocacyPlatformInstaller.Contracts;

    /// <summary>
    /// HttpClient wrapper.
    /// </summary>
    public class APHttpClient : IHttpClient
    {
        /// <summary>
        /// Internal HttpClient instance.
        /// </summary>
        private HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="APHttpClient"/> class.
        /// </summary>
        public APHttpClient()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="APHttpClient"/> class.
        /// </summary>
        /// <param name="handler">Custom HTTP message handler.</param>
        public APHttpClient(HttpMessageHandler handler)
        {
            _httpClient = new HttpClient(handler);
        }

        /// <summary>
        /// Sets a request header to a value.
        /// </summary>
        /// <param name="name">The header to add/set.</param>
        /// <param name="value">The value to set the header to.</param>
        public void SetHeader(string name, string value)
        {
            if (_httpClient.DefaultRequestHeaders.Contains(name))
            {
                _httpClient.DefaultRequestHeaders.Remove(name);
            }

            _httpClient.DefaultRequestHeaders.Add(name, value);
        }

        /// <summary>
        /// Sets the request timeout.
        /// </summary>
        /// <param name="timeout">The amount of time to wait.</param>
        public void SetTimeout(TimeSpan timeout)
        {
            _httpClient.Timeout = timeout;
        }

        /// <summary>
        /// Send a DELETE request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public async Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            return await _httpClient.DeleteAsync(requestUri);
        }

        /// <summary>
        /// Send a GET request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return await _httpClient.GetAsync(requestUri);
        }

        /// <summary>
        /// Send a GET request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public async Task<HttpResponseMessage> GetAsync(Uri requestUri)
        {
            return await _httpClient.GetAsync(requestUri);
        }

        /// <summary>
        /// Send a POST request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The request body content.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return await _httpClient.PostAsync(requestUri, content);
        }

        /// <summary>
        /// Send a PUT request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The request body content.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
        {
            return await _httpClient.PutAsync(requestUri, content);
        }

        /// <summary>
        /// Send a PUT request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The request body content.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public async Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content)
        {
            return await _httpClient.PutAsync(requestUri, content);
        }
    }
}
