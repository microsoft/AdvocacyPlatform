// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents the expected based response schema for all functions.
    /// </summary>
    public abstract class FunctionResponseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionResponseBase"/> class.
        /// </summary>
        public FunctionResponseBase()
        {
            StatusCode = (int)CommonStatusCode.Ok;
            StatusDesc = Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok);
            ErrorCode = (int)CommonErrorCode.NoError;
        }

        /// <summary>
        /// Gets or sets the status code representing the state of a successful execution
        /// If an error occurred, this should always be <see cref="Microsoft.AdvocacyPlatform.Contracts.CommonStatusCode.Error"/>.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the description representing the state of a successful execution
        /// If an error occurred, this should always be the name for <see cref="Microsoft.AdvocacyPlatform.Contracts.CommonStatusCode.Error"/>.
        /// </summary>
        public string StatusDesc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an error occurred (true).
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Gets or sets the code specifying the exact error that occurred
        /// If no error occurred, this should always be <see cref="Microsoft.AdvocacyPlatform.Contracts.CommonErrorCode.NoError"/>.
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the description representing the details of the error that occurred.
        /// </summary>
        public string ErrorDetails { get; set; }
    }
}
