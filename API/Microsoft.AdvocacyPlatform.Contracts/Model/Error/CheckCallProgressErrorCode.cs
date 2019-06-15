// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Error codes specific to the CheckCallProgressHttpTrigger function.
    /// </summary>
    public enum CheckCallProgressErrorCode
    {
        /// <summary>
        /// Request body missing callSid field.
        /// </summary>
        RequestMissingCallSid = 1,

        /// <summary>
        /// Call status returned is unexpected (e.g. we don't understand it).
        /// </summary>
        UnexpectedCallStatus = 100,
    }
}
