// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Exception thrown when request body is malformed.
    /// </summary>
    /// <typeparam name="T">The error code enum type the error code is a member of.</typeparam>
    public class MalformedRequestBodyException<T> : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MalformedRequestBodyException{T}"/> class.
        /// </summary>
        /// <param name="errorCode">The error code to set.</param>
        /// <param name="keyName">The name of the invalid key.</param>
        public MalformedRequestBodyException(T errorCode, string keyName)
            : base($"You must specify a value for the key '{keyName}' in the request body!")
        {
            this.KeyName = keyName;
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Gets or sets the name of the invalid key.
        /// </summary>
        public string KeyName { get; set; }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public T ErrorCode { get; set; }
    }
}
