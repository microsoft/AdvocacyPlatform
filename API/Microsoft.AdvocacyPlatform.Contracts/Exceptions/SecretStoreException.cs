// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Generic secret store exception.
    /// </summary>
    public class SecretStoreException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecretStoreException"/> class.
        /// </summary>
        /// <param name="message">Additional message describing the exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public SecretStoreException(string message, Exception innerException)
             : base(message, innerException)
        {
        }
    }
}
