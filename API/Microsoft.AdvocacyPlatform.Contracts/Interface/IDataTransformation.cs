// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Interface for interacting with a data transformation.
    /// </summary>
    public interface IDataTransformation
    {
        /// <summary>
        /// Transforms the input text using the implemented strategy.
        /// </summary>
        /// <param name="input">Input text.</param>
        /// <param name="parameters">Parameters to control the behavior of the data transformation function.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The transformed text.</returns>
        StringBuilder Transform(StringBuilder input, Dictionary<string, string> parameters, ILogger log);
    }
}
