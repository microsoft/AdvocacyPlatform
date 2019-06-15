// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Module
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.AdvocacyPlatform.Clients;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Module for use with dependency injection container for TwilioCallWrapper
    /// implementation of ITwilioCallWrapper.
    /// </summary>
    public class TwilioModule : Module
    {
        /// <summary>
        /// Loads the TwilioCallWrapper to fulfill ITwilioCallWrapper dependencies.
        ///
        /// We can improve on how we currently load this dependency.
        /// </summary>
        /// <param name="services">Service collection to add the service to.</param>
        public override void Load(IServiceCollection services)
        {
            services.AddTransient<ITwilioCallWrapper, TwilioCallWrapper>();
        }
    }
}
