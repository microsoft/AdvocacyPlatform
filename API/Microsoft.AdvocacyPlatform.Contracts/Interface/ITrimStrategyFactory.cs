// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Interface for interacting with factories creating trim strategies.
    /// </summary>
    public interface ITrimStrategyFactory
    {
        /// <summary>
        /// Creates a trim strategy.
        /// </summary>
        /// <param name="trimStrategyValue">The name of the requested trim strategy.</param>
        /// <returns>The implementation of the trim strategy requested.</returns>
        ITrimStrategy Create(string trimStrategyValue);
    }
}
