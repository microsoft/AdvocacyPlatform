// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// Custom HTTP request handler to obtain an access token from the token cache and add to the request Authorization header.
    /// </summary>
    public class OAuthMessageHandler : DelegatingHandler
    {
        private AuthenticationHeaderValue _authHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthMessageHandler"/> class.
        /// </summary>
        /// <param name="audience">The audience to acquire an access token for.</param>
        /// <param name="tokenProvider">The token provider instance to use for acquiring access tokens.</param>
        /// <param name="innerHandler">The base message handler to pass to the base handler class.</param>
        /// <param name="mainWindow">The owning UI context.</param>
        public OAuthMessageHandler(string audience, TokenProvider tokenProvider, HttpMessageHandler innerHandler, UIElement mainWindow)
            : base(innerHandler)
        {
            string accessToken = tokenProvider.GetAccessTokenAsync(audience, mainWindow).Result;

            _authHeader = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <summary>
        /// Sends an asynchronous HTTP request.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="cancellationToken">A cancellation token for the request.</param>
        /// <returns>The response message.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = _authHeader;

            return base.SendAsync(request, cancellationToken);
        }
    }
}
