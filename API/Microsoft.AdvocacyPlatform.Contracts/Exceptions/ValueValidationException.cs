// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Generic value validation exception.
    /// </summary>
    public class ValueValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValueValidationException"/> class.
        /// </summary>
        /// <param name="message">Additional message describing the exception.</param>
        public ValueValidationException(string message)
            : base(message)
        {
        }
    }
}
