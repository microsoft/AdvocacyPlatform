// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AdvocacyPlatformInstaller.Contracts;

    /// <summary>
    /// Base implementation for a token-based REST client.
    /// </summary>
    public abstract class TokenBasedClient
    {
        private ILogger _logger;
        private ITokenProvider _tokenProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenBasedClient"/> class.
        /// </summary>
        /// <param name="tokenProvider">The token provider instance to use for acquiring access tokens.</param>
        public TokenBasedClient(ITokenProvider tokenProvider)
        {
            TokenProvider = tokenProvider;
        }

        /// <summary>
        /// Gets or sets the logger instance to use for logging.
        /// </summary>
        protected ILogger Logger
        {
            get => _logger;
            set
            {
                _logger = value;
            }
        }

        /// <summary>
        /// Gets or sets the token provider instance to use for acquiring access tokens.
        /// </summary>
        protected ITokenProvider TokenProvider
        {
            get => _tokenProvider;
            set
            {
                _tokenProvider = value;
            }
        }

        /// <summary>
        /// Sets the logger instance to use for logging.
        /// </summary>
        /// <param name="logger">The logger instance to use.</param>
        public void SetLogger(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        protected void LogInformation(string message)
        {
            if (Logger != null)
            {
                Logger.LogInformation(message);
            }
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        protected void LogWarning(string message)
        {
            if (Logger != null)
            {
                Logger.LogWarning(message);
            }
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        protected void LogError(string message)
        {
            if (Logger != null)
            {
                Logger.LogError(message);
            }
        }
    }
}
