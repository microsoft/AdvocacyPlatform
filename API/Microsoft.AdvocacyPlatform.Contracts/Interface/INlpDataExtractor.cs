// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Interface for interacting with IDataExtractor implementations using Natural Language Processing (NLP).
    /// </summary>
    public interface INlpDataExtractor : IDataExtractor
    {
        /// <summary>
        /// Initializes the data extractor.
        /// </summary>
        /// <param name="config">Configuration for the data extractor.</param>
        /// <param name="httpClient">The IHttpClientWrapper implementation to use for making REST calls.</param>
        void Initialize(NlpDataExtractorConfiguration config, IHttpClientWrapper httpClient);
    }
}
