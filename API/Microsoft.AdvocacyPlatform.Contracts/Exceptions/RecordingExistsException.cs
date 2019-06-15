// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Exception thrown when recording already exists.
    /// </summary>
    public class RecordingExistsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingExistsException"/> class.
        /// </summary>
        /// <param name="message">A message sent with the exception.</param>
        public RecordingExistsException(string message)
            : base(message)
        {
        }
    }
}
