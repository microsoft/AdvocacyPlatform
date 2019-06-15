// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Interface for interacting with a dependency injection container builder.
    /// </summary>
    public interface IContainerBuilder
    {
        /// <summary>
        /// Registers a module with the container.
        /// </summary>
        /// <param name="module">An implementation of IModule to register.</param>
        /// <returns>The IContainerBuilder object.</returns>
        IContainerBuilder RegisterModule(IModule module = null);

        /// <summary>
        /// Builds the service provider.
        /// </summary>
        /// <returns>The service provider.</returns>
        IServiceProvider Build();
    }
}
