// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Generic data extractor exception.
    /// </summary>
    public class DataExtractorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataExtractorException"/> class.
        /// </summary>
        /// <param name="message">Additional details regarding the exception.</param>
        /// <param name="innerException">The original exception thrown.</param>
        public DataExtractorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
