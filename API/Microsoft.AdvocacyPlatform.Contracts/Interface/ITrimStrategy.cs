// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Interface for interacting with a trim strategy.
    /// </summary>
    public interface ITrimStrategy
    {
        /// <summary>
        /// Trims the input text using the implemented strategy.
        /// </summary>
        /// <param name="input">Input text.</param>
        /// <param name="maxLength">The maximum length to trim to.</param>
        /// <returns>The trimmed text.</returns>
        StringBuilder Trim(StringBuilder input, int maxLength);
    }
}
