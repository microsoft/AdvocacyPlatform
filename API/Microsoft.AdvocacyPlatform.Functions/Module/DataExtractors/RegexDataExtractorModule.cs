// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Module
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AdvocacyPlatform.Clients;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Module for use with dependency injection container for RegexDataExtractor
    /// implementation of IDataExtractor.
    /// </summary>
    public class RegexDataExtractorModule : Module
    {
        /// <summary>
        /// Loads the LuisDataExtractor to fulfill IDataExtractor dependencies.
        /// </summary>
        /// <param name="services">Service collection to add the service to.</param>
        public override void Load(IServiceCollection services)
        {
            services.AddScoped<IDataExtractor, RegexDataExtractor>();
        }
    }
}
