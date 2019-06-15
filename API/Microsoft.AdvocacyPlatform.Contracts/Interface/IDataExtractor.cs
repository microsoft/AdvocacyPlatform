// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Interface for interacting with an data extractor.
    /// </summary>
    public interface IDataExtractor
    {
        /// <summary>
        /// Extracts data from text.
        /// </summary>
        /// <param name="transcript">The text.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the extracted data.</returns>
        Task<TranscriptionData> ExtractAsync(string transcript, ILogger log);
    }
}
