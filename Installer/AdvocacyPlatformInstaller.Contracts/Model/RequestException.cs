// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Base exception for handling HTTP request exceptions.
    /// </summary>
    public class RequestException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestException"/> class.
        /// </summary>
        /// <param name="response">The HTTP response message.</param>
        public RequestException(HttpResponseMessage response)
        {
            Response = response;
        }

        /// <summary>
        /// Gets or sets the HTTP response message.
        /// </summary>
        public HttpResponseMessage Response { get; set; }
    }
}
