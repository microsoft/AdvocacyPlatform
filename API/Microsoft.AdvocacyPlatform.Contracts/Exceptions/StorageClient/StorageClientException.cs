// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Generic storage client exception.
    /// </summary>
    public class StorageClientException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StorageClientException"/> class.
        /// </summary>
        /// <param name="message">Additional details describing the exception.</param>
        /// <param name="innerException">The original exception thrown.</param>
        public StorageClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
