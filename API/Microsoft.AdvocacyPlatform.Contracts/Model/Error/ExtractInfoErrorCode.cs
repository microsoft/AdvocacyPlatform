// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Error codes specific to the ExtractInfoHttpTrigger function.
    /// </summary>
    public enum ExtractInfoErrorCode
    {
        /// <summary>
        /// Request body missing callSid field.
        /// </summary>
        RequestMissingCallSid = 1,

        /// <summary>
        /// Request body missing text field.
        /// </summary>
        RequestMissingText = 1,

        /// <summary>
        /// Generic data extractor failure.
        /// </summary>
        DataExtractorGenericFailure = -1000,
    }
}
