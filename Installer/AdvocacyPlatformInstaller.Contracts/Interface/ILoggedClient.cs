// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for a client with logging functionality.
    /// </summary>
    public interface ILoggedClient
    {
        /// <summary>
        /// Sets the logger instance.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        void SetLogger(ILogger logger);
    }
}
