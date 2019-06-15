// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Interface for creating registrable modules for the IContainerBuilder dependency injection container builder
    ///
    /// As Azure Functions does not currently have an easy way to support dependency injection (DI), utilizing the method described at https://platform.deloitte.com.au/articles/dependency-injections-on-azure-functions-v2
    /// to provide an DI container to functions.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Registers a service with the dependency injection container.
        /// </summary>
        /// <param name="services">Collection of services to load.</param>
        void Load(IServiceCollection services);
    }
}
