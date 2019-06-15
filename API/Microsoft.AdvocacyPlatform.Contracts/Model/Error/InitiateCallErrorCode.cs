// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Error codes specific to the InitiateCallHttpTrigger function.
    /// </summary>
    public enum InitiateCallErrorCode
    {
        /// <summary>
        /// Request body missing inputId field
        /// </summary>
        RequestMissingInputId = 1,
    }
}
