// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using AdvocacyPlatformInstaller.Contracts;

    /// <summary>
    /// Mock implementation of ITokenProvider interface.
    /// </summary>
    public class TokenProviderMock : ITokenProvider
    {
        private HttpClientMock _mock;
        private string _clientId;
        private string _tenantId;
        private string _userId;
        private UIElement _uiContext;

        /// <summary>
        /// Registers an HttpClientMock instance to use.
        /// </summary>
        /// <param name="mock">The instance to use.</param>
        public void RegisterHttpClientMock(HttpClientMock mock)
        {
            _mock = mock;
        }

        /// <summary>
        /// Acquire an access token.
        /// </summary>
        /// <param name="audience">The audience to acquire an access token for.</param>
        /// <param name="mainWindow">The UI context.</param>
        /// <returns>An access token.</returns>
        public Task<string> GetAccessTokenAsync(string audience, UIElement mainWindow = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set the expected client id.
        /// </summary>
        /// <param name="clientId">The expected client id.</param>
        public void SetClientId(string clientId)
        {
            _clientId = clientId;
        }

        /// <summary>
        /// Gets the client id of the authorized service principal.
        /// </summary>
        /// <returns>The client id of the authorized service principal.</returns>
        public string GetClientId()
        {
            return _clientId;
        }

        /// <summary>
        /// Gets an HTTP client for the intended audience.
        /// </summary>
        /// <param name="audience">The intended audience.</param>
        /// <returns>An HTTP client for the intended audience.</returns>
        public IHttpClient GetHttpClient(string audience)
        {
            return _mock;
        }

        /// <summary>
        /// Gets a generic HTTP client for any audience.
        /// </summary>
        /// <returns>A generic HTTP client.</returns>
        public IHttpClient GetGenericHttpClient()
        {
            return _mock;
        }

        /// <summary>
        /// Sets the expected tenant id.
        /// </summary>
        /// <param name="tenantId">The expected tenant id.</param>
        public void SetTenantId(string tenantId)
        {
            _tenantId = tenantId;
        }

        /// <summary>
        /// Gets the tenant id of the current user.
        /// </summary>
        /// <returns>The tenant id of the current user.</returns>
        public string GetTenantId()
        {
            return _tenantId;
        }

        /// <summary>
        /// Set the expected UI context.
        /// </summary>
        /// <param name="context">The expected UI context.</param>
        public void SetUIContext(UIElement context)
        {
            _uiContext = context;
        }

        /// <summary>
        /// Gets the UI context.
        /// </summary>
        /// <returns>The UI context.</returns>
        public UIElement GetUIContext()
        {
            return _uiContext;
        }

        /// <summary>
        /// Sets the expected user id.
        /// </summary>
        /// <param name="userId">The expected user id.</param>
        public void SetUserId(string userId)
        {
            _userId = userId;
        }

        /// <summary>
        /// Gets the current user's id.
        /// </summary>
        /// <returns>The current user's id.</returns>
        public string GetUserId()
        {
            throw new NotImplementedException();
        }
    }
}
