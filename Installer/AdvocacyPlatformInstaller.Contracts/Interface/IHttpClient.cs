// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for HTTP clients.
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        /// Sets a request header to a value.
        /// </summary>
        /// <param name="name">The header to add/set.</param>
        /// <param name="value">The value to set the header to.</param>
        void SetHeader(string name, string value);

        /// <summary>
        /// Sets the request timeout.
        /// </summary>
        /// <param name="timeout">The amount of time to wait.</param>
        void SetTimeout(TimeSpan timeout);

        /// <summary>
        /// Send a DELETE request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        Task<HttpResponseMessage> DeleteAsync(string requestUri);

        /// <summary>
        /// Send a GET request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        Task<HttpResponseMessage> GetAsync(string requestUri);

        /// <summary>
        /// Send a GET request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        Task<HttpResponseMessage> GetAsync(Uri requestUri);

        /// <summary>
        /// Send a POST request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The request body content.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);

        /// <summary>
        /// Send a PUT request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The request body content.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content);

        /// <summary>
        /// Send a PUT request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The request body content.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content);
    }
}
