// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Exception throw for canceled transcriptions.
    /// </summary>
    public class TranscriberCanceledException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranscriberCanceledException"/> class.
        /// </summary>
        /// <param name="message">A message to send with the exception.</param>
        public TranscriberCanceledException(string message)
            : base(message)
        {
        }
    }
}
