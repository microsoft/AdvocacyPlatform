// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Common status codes
    ///
    /// These should be negative integers to avoid collision with function specific status codes.
    /// </summary>
    public enum CommonStatusCode
    {
        /// <summary>
        /// Operation was successful
        /// </summary>
        Ok = 0,

        /// <summary>
        /// Operation encountered an error
        /// </summary>
        Error = -1,
    }
}
