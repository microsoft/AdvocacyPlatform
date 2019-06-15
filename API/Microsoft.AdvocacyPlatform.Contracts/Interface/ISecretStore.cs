// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for interacting with a secret store.
    /// </summary>
    public interface ISecretStore
    {
        /// <summary>
        /// Gets a secret from the secret store.
        /// </summary>
        /// <param name="secretIdentifier">The identifier of the secret.</param>
        /// <param name="authority">The token issuing authority.</param>
        /// <param name="expireInCacheSeconds">The lifetime of the cached secret.</param>
        /// <param name="associatedUserName">A username to associate with the secret.</param>
        /// <returns>A <see cref="Microsoft.AdvocacyPlatform.Contracts.Secret"/> object containing the secret.</returns>
        Task<Secret> GetSecretAsync(string secretIdentifier, string authority, int expireInCacheSeconds = 600, string associatedUserName = null);
    }
}
