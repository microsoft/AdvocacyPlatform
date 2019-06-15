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
    /// Model for representing an expected HTTP response message.
    /// </summary>
    public class ExpectedResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedResponse"/> class.
        /// </summary>
        /// <param name="response">The expected HTTP response message.</param>
        public ExpectedResponse(HttpResponseMessage response)
        {
            Response = response;
        }

        /// <summary>
        /// Gets or sets the expected HTTP response message.
        /// </summary>
        public HttpResponseMessage Response { get; set; }
    }
}
