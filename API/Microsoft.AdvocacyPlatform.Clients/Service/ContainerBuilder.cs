// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// As Azure Functions does not currently have an easy way to support dependency injection (DI),
    /// utilizing the method described at https://platform.deloitte.com.au/articles/dependency-injections-on-azure-functions-v2
    /// to provide an DI container to functions.
    /// </summary>
    public class ContainerBuilder : IContainerBuilder
    {
        /// <summary>
        /// Internal service collection to register services.
        /// </summary>
        private readonly IServiceCollection _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerBuilder"/> class.
        /// </summary>
        public ContainerBuilder()
        {
            this._services = new ServiceCollection();
        }

        /// <summary>
        /// Registers a module with the dependency injection container.
        /// </summary>
        /// <param name="module">An instance of the module to register.</param>
        /// <returns>The current ContainerBuilder object.</returns>
        public IContainerBuilder RegisterModule(IModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("You must specify a module to register!");
            }

            module.Load(this._services);

            return this;
        }

        /// <summary>
        /// Builds the service provider.
        /// </summary>
        /// <returns>The service provider instance.</returns>
        public IServiceProvider Build()
        {
            return this._services.BuildServiceProvider();
        }
    }
}
