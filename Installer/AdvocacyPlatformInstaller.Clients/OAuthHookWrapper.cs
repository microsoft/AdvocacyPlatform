// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Xrm.Tooling.Connector;

    /// <summary>
    /// Authorization hook wrapper for hooking into Dynamics CRM client's OAuth process.
    /// </summary>
    public class OAuthHookWrapper : IOverrideAuthHookWrapper
    {
        private string _accessToken;

        /// <summary>
        /// Sets the access token.
        /// </summary>
        /// <param name="accessToken">The value to set the access token to.</param>
        public void SetAccessToken(string accessToken)
        {
            _accessToken = accessToken;
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        /// <param name="connectedUri">The URI to get the access token for.</param>
        /// <returns>The access token.</returns>
        public string GetAuthToken(Uri connectedUri)
        {
            return _accessToken;
        }
    }
}
