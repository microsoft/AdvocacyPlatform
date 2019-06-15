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
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Mocks an HttpMessageHandler.
    /// </summary>
    public class HttpMessageHandlerMock : HttpMessageHandler
    {
        private ConcurrentDictionary<string, HttpResponseMessage> _expectedCallResponses;
        private ConcurrentDictionary<string, HttpResponseMessage> _expectedRecordingResponses;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpMessageHandlerMock"/> class.
        /// </summary>
        public HttpMessageHandlerMock()
        {
            _expectedCallResponses = new ConcurrentDictionary<string, HttpResponseMessage>();
            _expectedRecordingResponses = new ConcurrentDictionary<string, HttpResponseMessage>();
        }

        /// <summary>
        /// Gets a Twilio call SID from a request URL.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <param name="isQueryParams">Flag indicating whether the URL has query parameters.</param>
        /// <returns>The call SID.</returns>
        public static string GetCallSid(string url, bool isQueryParams = false)
        {
            if (!isQueryParams)
            {
                string[] parts = url.Split(new char[] { '/' });

                if (url.EndsWith(".json"))
                {
                    return parts[parts.Length - 1].Replace(".json", string.Empty);
                }
            }
            else
            {
                System.Collections.Specialized.NameValueCollection queryDictionary = System.Web.HttpUtility.ParseQueryString(url);

                return queryDictionary.Get("CallSid");
            }

            return null;
        }

        /// <summary>
        /// Gets a Twilio account SID from a request URL.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <returns>The account SID.</returns>
        public static string GetAccountSid(string url)
        {
            string[] urlParts = url.Split(new char[] { '/' });

            for (int i = 0; i < urlParts.Length; i++)
            {
                if (string.Compare(urlParts[i], "Accounts", true) == 0)
                {
                    return urlParts[i + 1];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a Twilio recording SID from a request URL.
        /// </summary>
        /// <param name="url">The request URL.</param>
        /// <param name="isQueryParams">Flag indicating whether the URL has query parameters.</param>
        /// <returns>The recording SID.</returns>
        public static string GetRecordingSid(string url, bool isQueryParams = false)
        {
            if (!isQueryParams)
            {
                string[] parts = url.Split(new char[] { '/' });

                if (url.EndsWith(".json"))
                {
                    return parts[parts.Length - 1].Replace(".json", string.Empty);
                }
            }
            else
            {
                System.Collections.Specialized.NameValueCollection queryDictionary = System.Web.HttpUtility.ParseQueryString(url);

                return queryDictionary.Get("RecordingSid");
            }

            return null;
        }

        /// <summary>
        /// Registers an expected Twilio call response.
        /// </summary>
        /// <param name="identifier">The unique identifier for the expected response.</param>
        /// <param name="action">The action type for the response.</param>
        /// <param name="response">The HTTP response message.</param>
        /// <returns>True if registered successfully, false if failed.</returns>
        public bool RegisterExpectedCallResponse(string identifier, string action, HttpResponseMessage response)
        {
            string key = $"{identifier}_{action}";

            if (_expectedCallResponses.ContainsKey(key))
            {
                throw new InvalidOperationException("A response has already been registered for this call SID and action!");
            }

            return _expectedCallResponses.TryAdd(key, response);
        }

        /// <summary>
        /// Clears the cache for expected Twilio call responses.
        /// </summary>
        public void ClearExpectedCallResponses()
        {
            _expectedCallResponses.Clear();
        }

        /// <summary>
        /// Registers an expected Twilio recording response.
        /// </summary>
        /// <param name="callSid">The expected call SID.</param>
        /// <param name="action">The action type.</param>
        /// <param name="response">The HTTP response message.</param>
        /// <returns>True if registered successfully, false if failed.</returns>
        public bool RegisterExpectedRecordingResponse(string callSid, string action, HttpResponseMessage response)
        {
            string key = $"{callSid}_{action}";

            if (_expectedRecordingResponses.ContainsKey(key))
            {
                throw new InvalidOperationException("A response has already been registered for recordings for this call SID and action!");
            }

            return _expectedRecordingResponses.TryAdd(key, response);
        }

        /// <summary>
        /// Clears the cache of expected Twilio recording responses.
        /// </summary>
        public void ClearExpectedRecordingResposes()
        {
            _expectedRecordingResponses.Clear();
        }

        /// <summary>
        /// Processes an HTTP request message.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>An HTTP response message.</returns>
        public async Task<HttpResponseMessage> ProcessRequestAsync(HttpRequestMessage request)
        {
            string url = request.RequestUri.AbsolutePath;

            if (request.Method != HttpMethod.Delete)
            {
                // Create a call
                if (url.EndsWith("Calls.json"))
                {
                    string content = await request.Content.ReadAsStringAsync();

                    System.Collections.Specialized.NameValueCollection queryDictionary = System.Web.HttpUtility.ParseQueryString(content);

                    string toParam = queryDictionary.Get("to");
                    string key = $"{toParam}_call";

                    if (_expectedCallResponses.ContainsKey(key))
                    {
                        return _expectedCallResponses[key];
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK); // TODO: The documentation does not state the response code for when a resource is not found. Update once we have tested.
                }

                // Fetch a recording
                else if (url.EndsWith("Recordings.json"))
                {
                    Uri requestUrl = request.RequestUri;

                    string key = null;
                    string callSid = GetCallSid(requestUrl.Query, true);

                    if (string.IsNullOrWhiteSpace(callSid))
                    {
                        string accountSid = GetAccountSid(requestUrl.AbsoluteUri);

                        key = $"{accountSid}_recordings";
                    }
                    else
                    {
                        key = $"{callSid}_recordings";
                    }

                    if (_expectedRecordingResponses.ContainsKey(key))
                    {
                        return _expectedRecordingResponses[key];
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK); // TODO: The documentation does not state the response code for when a resource is not found. Update once we have tested
                }

                // Fetch a call
                else if (url.Contains("Calls/"))
                {
                    string callSid = GetCallSid(url);

                    string key = $"{callSid}_status";

                    if (_expectedCallResponses.ContainsKey(key))
                    {
                        return _expectedCallResponses[key];
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK); // TODO: The documentation does not state the response code for when a resource is not found. Update once we have tested
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (url.Contains("Calls/"))
                {
                    string callSid = GetCallSid(url);

                    string key = $"{callSid}_delete";

                    if (_expectedCallResponses.ContainsKey(key))
                    {
                        return _expectedCallResponses[key];
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK); // TODO: The documentation does not state the response code for when a resource is not found. Update once we have tested
                }
                else if (url.Contains("Recordings/"))
                {
                    string recordingSid = GetRecordingSid(url);

                    string key = $"{recordingSid}_delete";

                    if (_expectedRecordingResponses.ContainsKey(key))
                    {
                        return _expectedRecordingResponses[key];
                    }

                    return new HttpResponseMessage(HttpStatusCode.OK); // TODO: The documentation does not state the response code for when a resource is not found. Update once we have tested
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Simulates sending an HTTP message.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>An HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await ProcessRequestAsync(request);
        }
    }
}
