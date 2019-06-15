// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    /// <summary>
    /// Wrapper for ensuring a secret remains encrypted as much as possible.
    /// </summary>
    public class Secret
    {
        /// <summary>
        /// Internal data structure used to store secret encrypted.
        /// </summary>
        private NetworkCredential _secret;

        /// <summary>
        /// Initializes a new instance of the <see cref="Secret"/> class.
        /// </summary>
        /// <param name="identifier">Identifier for this secret.</param>
        /// <param name="secret">An encrypted string with the secret value.</param>
        public Secret(string identifier, SecureString secret)
        {
            Identifier = identifier;

            _secret = new NetworkCredential(null, secret);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Secret"/> class.
        /// </summary>
        /// <param name="identifier">Identifier for this secret.</param>
        /// <param name="userName">A username to associated with this secret.</param>
        /// <param name="secret">An encrypted string with the secret value.</param>
        public Secret(string identifier, string userName, SecureString secret)
        {
            Identifier = identifier;

            _secret = new NetworkCredential(userName, secret);
        }

        /// <summary>
        /// Gets or sets the identifier for this secret.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets the associated username.
        /// </summary>
        public string UserName => _secret.UserName;

        /// <summary>
        /// Gets the unencrypted secret value.
        /// </summary>
        public string Value => _secret.Password;
    }
}
