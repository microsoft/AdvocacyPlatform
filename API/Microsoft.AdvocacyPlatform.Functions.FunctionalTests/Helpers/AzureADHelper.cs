// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Helper class for obtaining access tokens from Azure AD.
    /// </summary>
    public static class AzureADHelper
    {
        /// <summary>
        /// Acquires an access token.
        /// </summary>
        /// <param name="authority">The authorization authority.</param>
        /// <param name="clientId">The client id of a service principal for a registered application.</param>
        /// <param name="clientSecret">The client secret of a service principal for a registered application.</param>
        /// <param name="resourceId">The resource id you are requesting authorization for.</param>
        /// <returns>An access token.</returns>
        public static async Task<string> GetAccessTokenAsync(string authority, string clientId, string clientSecret, string resourceId)
        {
            AuthenticationContext context = new AuthenticationContext(authority);
            ClientCredential credential = new ClientCredential(clientId, clientSecret);

            AuthenticationResult result = await context.AcquireTokenAsync(resourceId, credential);

            return result.AccessToken;
        }
    }
}
