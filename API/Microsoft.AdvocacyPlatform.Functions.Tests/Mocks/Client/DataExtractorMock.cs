// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests.Mocks.Client
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Mocks a DataExtractor.
    /// </summary>
    public class DataExtractorMock : IDataExtractor
    {
        /// <summary>
        /// Configures the data extractor.
        /// </summary>
        /// <param name="parameters">Parameters used to control the behavior of the data extractor.</param>
        /// <param name="httpClient">The IHttpClientWrapper instance to use.</param>
        public void Configure(Dictionary<string, object> parameters, IHttpClientWrapper httpClient)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Performs data extraction.
        /// </summary>
        /// <param name="transcript">The transcript to extract data from.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The extracted data.</returns>
        public Task<TranscriptionData> ExtractAsync(string transcript, ILogger log)
        {
            throw new NotImplementedException();
        }
    }
}
