// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests.Mocks.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Mocks the AzureKeyVaultSecretStore.
    /// </summary>
    public class AzureKeyVaultSecretStoreMock : ISecretStore
    {
        private ConcurrentDictionary<string, Secret> _expectedSecrets = new ConcurrentDictionary<string, Secret>();
        private ConcurrentDictionary<string, SecretStoreException> _expectedExceptions = new ConcurrentDictionary<string, SecretStoreException>();

        /// <summary>
        /// Simulates getting a secret.
        /// </summary>
        /// <param name="secretIdentifier">The unique identifier for the secret.</param>
        /// <param name="authority">The authorizing authority.</param>
        /// <param name="expireInCacheSeconds">The expected cache expiration in seconds.</param>
        /// <param name="associatedUserName">The expected associated user name.</param>
        /// <returns>The requested secret.</returns>
        public Task<Secret> GetSecretAsync(string secretIdentifier, string authority, int expireInCacheSeconds = 600, string associatedUserName = null)
        {
            return Task.Run(() =>
            {
                if (!_expectedSecrets.ContainsKey(secretIdentifier))
                {
                    if (!_expectedExceptions.ContainsKey(secretIdentifier))
                    {
                        throw new KeyNotFoundException($"The request for secret with the identifier {secretIdentifier} was not expected!");
                    }
                    else
                    {
                        throw _expectedExceptions[secretIdentifier];
                    }
                }

                return _expectedSecrets[secretIdentifier];
            });
        }

        /// <summary>
        /// Registers an expected secret with the mock secret store.
        /// </summary>
        /// <param name="secret">The secret to register.</param>
        /// <returns>True if registered successfully, false if failed.</returns>
        public bool RegisterExpectedSecret(Secret secret)
        {
            if (_expectedSecrets.ContainsKey(secret.Identifier))
            {
                throw new InvalidOperationException($"A secret with the identifier {secret.Identifier} was already registered!");
            }

            return _expectedSecrets.TryAdd(secret.Identifier, secret);
        }

        /// <summary>
        /// Registers an expected exception to be thrown.
        /// </summary>
        /// <param name="secretName">The name of the secret associated with the exception.</param>
        /// <param name="ex">The exception to throw.</param>
        /// <returns>True if registered successfully, false if failed.</returns>
        public bool RegisterExpectedException(string secretName, SecretStoreException ex)
        {
            if (_expectedExceptions.ContainsKey(secretName))
            {
                throw new InvalidOperationException($"An exception for the secret with the identifier {secretName} was already registered!");
            }

            return _expectedExceptions.TryAdd(secretName, ex);
        }

        /// <summary>
        /// Clears the expected secrets cache.
        /// </summary>
        public void ClearExpectedSecrets()
        {
            _expectedSecrets.Clear();
        }

        /// <summary>
        /// Clears the expected exceptions cache.
        /// </summary>
        public void ClearExpectedExceptions()
        {
            _expectedExceptions.Clear();
        }
    }
}
