// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// As Azure Functions does not currently have an easy way to support dependency injection (DI), utilizing the method described at https://platform.deloitte.com.au/articles/dependency-injections-on-azure-functions-v2
    /// to provide an DI container to functions.
    ///
    /// Base class for modules.
    /// </summary>
    public abstract class Module : IModule
    {
        /// <summary>
        /// Loads services.
        /// </summary>
        /// <param name="services">The collection of services to load.</param>
        public virtual void Load(IServiceCollection services)
        {
            return;
        }
    }
}
