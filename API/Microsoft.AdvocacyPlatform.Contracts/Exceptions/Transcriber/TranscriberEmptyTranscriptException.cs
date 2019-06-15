// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Exception thrown for blank transcriptions.
    /// </summary>
    public class TranscriberEmptyTranscriptException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranscriberEmptyTranscriptException"/> class.
        /// </summary>
        /// <param name="message">A message to send with the exception.</param>
        public TranscriberEmptyTranscriptException(string message)
            : base(message)
        {
        }
    }
}
