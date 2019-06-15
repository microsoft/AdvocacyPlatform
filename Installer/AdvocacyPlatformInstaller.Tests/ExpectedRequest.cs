// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Model representing an expected HTTP request message.
    /// </summary>
    public class ExpectedRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedRequest"/> class.
        /// </summary>
        /// <param name="request">The expected HTTP request message.</param>
        /// <param name="validationFunc">Delegate for validating the actual request message.</param>
        public ExpectedRequest(HttpRequestMessage request, Func<HttpRequestMessage, bool> validationFunc = null)
        {
            Request = request;
            ValidateRequest = validationFunc;
        }

        /// <summary>
        /// Gets or sets the expected HTTP request message.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Gets or sets the delegate for validating the actual request message.
        /// </summary>
        public Func<HttpRequestMessage, bool> ValidateRequest { get; set; }
    }
}
