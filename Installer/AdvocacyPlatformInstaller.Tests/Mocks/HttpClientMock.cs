// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using AdvocacyPlatformInstaller.Contracts;

    /// <summary>
    /// Mock implementation for IHttpClient interface.
    /// </summary>
    public class HttpClientMock : IHttpClient
    {
        private Queue<ExpectedRequest> _expectedRequests;
        private Dictionary<string, Queue<ExpectedResponse>> _expectedResponses;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientMock"/> class.
        /// </summary>
        public HttpClientMock()
        {
            _expectedRequests = new Queue<ExpectedRequest>();
            _expectedResponses = new Dictionary<string, Queue<ExpectedResponse>>();
        }

        /// <summary>
        /// Registers an expected request message.
        /// </summary>
        /// <param name="request">The expected request message.</param>
        public void RegisterExpectedRequest(ExpectedRequest request)
        {
            _expectedRequests.Enqueue(request);
        }

        /// <summary>
        /// Registers an expected response message.
        /// </summary>
        /// <param name="requestUri">The expected URI in the request.</param>
        /// <param name="response">The expected response message.</param>
        public void RegisterExpectedResponse(string requestUri, ExpectedResponse response)
        {
            if (_expectedResponses.ContainsKey(requestUri))
            {
                _expectedResponses[requestUri].Enqueue(response);
            }
            else
            {
                _expectedResponses.Add(
                requestUri,
                new Queue<ExpectedResponse>(new ExpectedResponse[] { response }));
            }
        }

        /// <summary>
        /// Sets a request header to a value.
        /// </summary>
        /// <param name="name">The header to add/set.</param>
        /// <param name="value">The value to set the header to.</param>
        public void SetHeader(string name, string value)
        {
        }

        /// <summary>
        /// Sets the request timeout.
        /// </summary>
        /// <param name="timeout">The amount of time to wait.</param>
        public void SetTimeout(TimeSpan timeout)
        {
        }

        /// <summary>
        /// Send a DELETE request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            return Task.Run(() =>
            {
                ExpectedRequest expectedRequest = _expectedRequests.Dequeue();
                HttpRequestMessage actualRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

                if (string.Compare(expectedRequest.Request.RequestUri.AbsoluteUri, actualRequest.RequestUri.AbsoluteUri, true) != 0)
                {
                    throw new Exception("The expected request was not received!");
                }
                else if (expectedRequest.Request.Method != HttpMethod.Delete)
                {
                    throw new Exception($"A different request method was expected ('{expectedRequest.Request.Method}' != 'DELETE')!");
                }

                if (!_expectedResponses.ContainsKey(actualRequest.RequestUri.AbsoluteUri))
                {
                    throw new KeyNotFoundException("No response registered for this request!");
                }

                return _expectedResponses[actualRequest.RequestUri.AbsoluteUri].Dequeue().Response;
            });
        }

        /// <summary>
        /// Send a GET request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return Task.Run(() =>
            {
                ExpectedRequest expectedRequest = _expectedRequests.Dequeue();
                HttpRequestMessage actualRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

                if (string.Compare(expectedRequest.Request.RequestUri.AbsoluteUri, actualRequest.RequestUri.AbsoluteUri, true) != 0)
                {
                    throw new Exception("The expected request was not received!");
                }
                else if (expectedRequest.Request.Method != HttpMethod.Get)
                {
                    throw new Exception($"A different request method was expected ('{expectedRequest.Request.Method}' != 'GET')!");
                }

                if (!_expectedResponses.ContainsKey(actualRequest.RequestUri.AbsoluteUri))
                {
                    throw new KeyNotFoundException("No response registered for this request!");
                }

                return _expectedResponses[actualRequest.RequestUri.AbsoluteUri].Dequeue().Response;
            });
        }

        /// <summary>
        /// Send a GET request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public Task<HttpResponseMessage> GetAsync(Uri requestUri)
        {
            return Task.Run(() =>
            {
                ExpectedRequest expectedRequest = _expectedRequests.Dequeue();
                HttpRequestMessage actualRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

                if (string.Compare(expectedRequest.Request.RequestUri.AbsoluteUri, actualRequest.RequestUri.AbsoluteUri, true) != 0)
                {
                    throw new Exception("The expected request was not received!");
                }
                else if (expectedRequest.Request.Method != HttpMethod.Get)
                {
                    throw new Exception($"A different request method was expected ('{expectedRequest.Request.Method}' != 'GET')!");
                }

                if (!_expectedResponses.ContainsKey(actualRequest.RequestUri.AbsoluteUri))
                {
                    throw new KeyNotFoundException("No response registered for this request!");
                }

                return _expectedResponses[actualRequest.RequestUri.AbsoluteUri].Dequeue().Response;
            });
        }

        /// <summary>
        /// Send a POST request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The request body content.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return Task.Run(() =>
            {
                ExpectedRequest expectedRequest = _expectedRequests.Dequeue();
                HttpRequestMessage actualRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

                if (string.Compare(expectedRequest.Request.RequestUri.AbsoluteUri, actualRequest.RequestUri.AbsoluteUri, true) != 0)
                {
                    throw new Exception("The expected request was not received!");
                }
                else if (expectedRequest.Request.Method != HttpMethod.Post)
                {
                    throw new Exception($"A different request method was expected ('{expectedRequest.Request.Method}' != 'POST')!");
                }

                if (!_expectedResponses.ContainsKey(actualRequest.RequestUri.AbsoluteUri))
                {
                    throw new KeyNotFoundException("No response registered for this request!");
                }

                return _expectedResponses[actualRequest.RequestUri.AbsoluteUri].Dequeue().Response;
            });
        }

        /// <summary>
        /// Send a PUT request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The request body content.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
        {
            return Task.Run(() =>
            {
                ExpectedRequest expectedRequest = _expectedRequests.Dequeue();
                HttpRequestMessage actualRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

                if (string.Compare(expectedRequest.Request.RequestUri.AbsoluteUri, actualRequest.RequestUri.AbsoluteUri, true) != 0)
                {
                    throw new Exception("The expected request was not received!");
                }
                else if (expectedRequest.Request.Method != HttpMethod.Put)
                {
                    throw new Exception($"A different request method was expected ('{expectedRequest.Request.Method}' != 'PUT')!");
                }

                if (!_expectedResponses.ContainsKey(actualRequest.RequestUri.AbsoluteUri))
                {
                    throw new KeyNotFoundException("No response registered for this request!");
                }

                return _expectedResponses[actualRequest.RequestUri.AbsoluteUri].Dequeue().Response;
            });
        }

        /// <summary>
        /// Send a PUT request.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The request body content.</param>
        /// <returns>HttpResponseMessage object representing the response.</returns>
        public Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content)
        {
            return Task.Run(() =>
            {
                ExpectedRequest expectedRequest = _expectedRequests.Dequeue();
                HttpRequestMessage actualRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

                if (string.Compare(expectedRequest.Request.RequestUri.AbsoluteUri, actualRequest.RequestUri.AbsoluteUri, true) != 0)
                {
                    throw new Exception("The expected request was not received!");
                }
                else if (expectedRequest.Request.Method != HttpMethod.Put)
                {
                    throw new Exception($"A different request method was expected ('{expectedRequest.Request.Method}' != 'PUT')!");
                }

                if (!_expectedResponses.ContainsKey(actualRequest.RequestUri.AbsoluteUri))
                {
                    throw new KeyNotFoundException("No response registered for this request!");
                }

                return _expectedResponses[actualRequest.RequestUri.AbsoluteUri].Dequeue().Response;
            });
        }
    }
}
