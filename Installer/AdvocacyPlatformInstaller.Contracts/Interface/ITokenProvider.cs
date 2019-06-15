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
    using System.Windows;

    /// <summary>
    /// Interface for an access token provider.
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// Gets the client id of the authorized service principal.
        /// </summary>
        /// <returns>The client id of the authorized service principal.</returns>
        string GetClientId();

        /// <summary>
        /// Gets the tenant id of the current user.
        /// </summary>
        /// <returns>The tenant id of the current user.</returns>
        string GetTenantId();

        /// <summary>
        /// Gets the current user's id.
        /// </summary>
        /// <returns>The current user's id.</returns>
        string GetUserId();

        /// <summary>
        /// Gets the UI context.
        /// </summary>
        /// <returns>The UI context.</returns>
        UIElement GetUIContext();

        /// <summary>
        /// Acquire an access token.
        /// </summary>
        /// <param name="audience">The audience to acquire an access token for.</param>
        /// <param name="mainWindow">The UI context.</param>
        /// <returns>An access token.</returns>
        Task<string> GetAccessTokenAsync(string audience, UIElement mainWindow = null);

        /// <summary>
        /// Gets an HTTP client for the intended audience.
        /// </summary>
        /// <param name="audience">The intended audience.</param>
        /// <returns>An HTTP client for the intended audience.</returns>
        IHttpClient GetHttpClient(string audience);

        /// <summary>
        /// Gets a generic HTTP client for any audience.
        /// </summary>
        /// <returns>A generic HTTP client.</returns>
        IHttpClient GetGenericHttpClient();
    }
}
