// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Generic transcriber exception.
    /// </summary>
    public class TranscriberException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranscriberException"/> class.
        /// </summary>
        /// <param name="message">Additional details describing the exception.</param>
        /// <param name="innerException">The original exception thrown.</param>
        public TranscriberException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
