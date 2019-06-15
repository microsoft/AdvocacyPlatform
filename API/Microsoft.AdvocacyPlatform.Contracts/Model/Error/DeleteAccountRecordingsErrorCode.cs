// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Error codes specific to the DeleteAccountRecordingsHttpTrigger function.
    /// </summary>
    public enum DeleteAccountRecordingsErrorCode
    {
        /// <summary>
        /// Request body missing confirmDelete field or value not yes.
        /// </summary>
        RequestMissingConfirmDelete = 1,
    }
}
