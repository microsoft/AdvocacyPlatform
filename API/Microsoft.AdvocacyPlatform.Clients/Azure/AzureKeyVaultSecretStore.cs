// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Azure;
    using Microsoft.Azure.KeyVault;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Azure.Services.AppAuthentication;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// ISecretStore implementation for interacting with an Azure Key Vault resource.
    /// </summary>
    public class AzureKeyVaultSecretStore : ISecretStore
    {
        /// <summary>
        /// Name of the memory cache for caching retrieved secrets.
        /// </summary>
        private const string _cacheKey = "AzureKeyVaultCache";

        /// <summary>
        /// Memory cache for caching retrieved secrets.
        /// </summary>
        private static MemoryCache _cache = new MemoryCache(Options.Create<MemoryCacheOptions>(new MemoryCacheOptions()));

        /// <summary>
        /// Retrieves a secret from an Azure Key Vault resource.
        /// </summary>
        /// <param name="secretIdentifier">The URI identifying the secret to receive.</param>
        /// <param name="authority">The token issuing authority.</param>
        /// <param name="expireInCacheSeconds">The lifetime of a cached secret.</param>
        /// <param name="associatedUserName">A user name to associate with the secret when creating the cached secret.</param>
        /// <returns>A <see cref="Microsoft.AdvocacyPlatform.Contracts.Secret"/> object representing the secret.</returns>
        public async Task<Secret> GetSecretAsync(string secretIdentifier, string authority, int expireInCacheSeconds = 600, string associatedUserName = null)
        {
            Secret cachedSecret = null;

            try
            {
                if (!_cache.TryGetValue<Secret>(secretIdentifier, out cachedSecret))
                {
                    AzureServiceTokenProvider tokenProvider = new AzureServiceTokenProvider();
                    KeyVaultClient kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback));

                    SecretBundle secret = await kvClient.GetSecretAsync(secretIdentifier);

                    SecureString securedSecretString = new SecureString();

                    foreach (char character in secret.Value)
                    {
                        securedSecretString.AppendChar(character);
                    }

                    securedSecretString.MakeReadOnly();

                    cachedSecret = new Secret(secret.Id, associatedUserName, securedSecretString);

                    _cache.Set<Secret>(
                        secretIdentifier,
                        cachedSecret,
                        new MemoryCacheEntryOptions()
                        {
                            SlidingExpiration = TimeSpan.FromSeconds(expireInCacheSeconds),
                        });
                }

                return cachedSecret;
            }
            catch (KeyVaultErrorException ex)
            {
                throw new SecretStoreException("Exception encountered retrieving secret. See inner exception for details.", ex);
            }
        }
    }
}
