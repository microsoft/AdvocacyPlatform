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
    using System.Windows.Threading;
    using AdvocacyPlatformInstaller.Contracts;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Token provider for acquiring access tokens.
    /// </summary>
    public class TokenProvider : ITokenProvider
    {
        private string _clientId;
        private string _redirectUri;
        private string _tenantId;
        private string _userId;
        private Dictionary<string, IHttpClient> _httpClients;
        private UIElement _mainWindow;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenProvider"/> class.
        /// </summary>
        /// <param name="clientId">The application id of the authorized service principal to use for making requests.</param>
        /// <param name="redirectUri">The redirect uri assigned to the authorized service principal.</param>
        /// <param name="tenantId">The tenant id the authorized service principal resides in.</param>
        /// <param name="mainWindow">The owning UI context.</param>
        public TokenProvider(string clientId, string redirectUri, string tenantId = null, UIElement mainWindow = null)
        {
            _clientId = clientId;
            _redirectUri = redirectUri;
            _tenantId = tenantId;
            _httpClients = new Dictionary<string, IHttpClient>();
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Gets the client id of the authorized service principal.
        /// </summary>
        /// <returns>The client id of the authorized service principal.</returns>
        public string GetClientId() => _clientId;

        /// <summary>
        /// Gets the tenant id of the current user.
        /// </summary>
        /// <returns>The tenant id of the current user.</returns>
        public string GetTenantId() => _tenantId;

        /// <summary>
        /// Gets the current user's id.
        /// </summary>
        /// <returns>The current user's id.</returns>
        public string GetUserId() => _userId;

        /// <summary>
        /// Gets the UI context.
        /// </summary>
        /// <returns>The UI context.</returns>
        public UIElement GetUIContext()
        {
            return _mainWindow;
        }

        /// <summary>
        /// Acquire an access token.
        /// </summary>
        /// <param name="audience">The audience to acquire an access token for.</param>
        /// <param name="mainWindow">The UI context.</param>
        /// <returns>An access token.</returns>
        public async Task<string> GetAccessTokenAsync(string audience, UIElement mainWindow = null)
        {
            if (mainWindow != null || _mainWindow != null)
            {
                AuthenticationResult authResult = (mainWindow ?? _mainWindow).Dispatcher.Invoke(new Func<AuthenticationResult>(() =>
                {
                    AuthenticationContext authContext = new AuthenticationContext(
                        _tenantId != null ?
                            $"https://login.windows.net/{_tenantId}/" :
                            "https://login.windows.net/common");

                    PlatformParameters platformParameters = new PlatformParameters(
                        PromptBehavior.Auto);

                    return authContext.AcquireTokenAsync(audience, _clientId, new Uri(_redirectUri), platformParameters, UserIdentifier.AnyUser).Result;
                }));

                _tenantId = authResult.TenantId;
                _userId = authResult.UserInfo.UniqueId;

                return authResult.AccessToken;
            }
            else
            {
                AuthenticationContext authContext = new AuthenticationContext(
                    _tenantId != null ?
                        $"https://login.windows.net/{_tenantId}/" :
                        "https://login.windows.net/common");

                PlatformParameters platformParameters = new PlatformParameters(
                    PromptBehavior.Auto);

                AuthenticationResult authResult = await authContext.AcquireTokenAsync(audience, _clientId, new Uri(_redirectUri), platformParameters, UserIdentifier.AnyUser);

                _tenantId = authResult.TenantId;
                _userId = authResult.UserInfo.UniqueId;

                return authResult.AccessToken;
            }
        }

        /// <summary>
        /// Gets an HTTP client for the intended audience.
        /// </summary>
        /// <param name="audience">The intended audience.</param>
        /// <returns>An HTTP client for the intended audience.</returns>
        public IHttpClient GetHttpClient(string audience)
        {
            if (!_httpClients.ContainsKey(audience))
            {
                IHttpClient httpClient = new APHttpClient(
                    new OAuthMessageHandler(
                        audience,
                        this,
                        new HttpClientHandler(),
                        _mainWindow));

                httpClient.SetTimeout(
                    TimeSpan.FromMinutes(10));

                httpClient.SetHeader("OData-MaxVersion", "4.0");
                httpClient.SetHeader("OData-Version", "4.0");
                httpClient.SetHeader("accept", "application/json");

                _httpClients.Add(
                    audience,
                    httpClient);
            }

            return _httpClients[audience];
        }

        /// <summary>
        /// Gets a generic HTTP client for any audience.
        /// </summary>
        /// <returns>A generic HTTP client.</returns>
        public IHttpClient GetGenericHttpClient()
        {
            return new APHttpClient();
        }
    }
}
