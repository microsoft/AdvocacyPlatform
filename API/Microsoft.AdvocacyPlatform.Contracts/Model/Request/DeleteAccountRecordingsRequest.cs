// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Represents the expected request schema to DeleteAllRecordings.
    /// </summary>
    public class DeleteAccountRecordingsRequest : FunctionRequestBase
    {
        /// <summary>
        /// The expected key for confirmDelete.
        /// </summary>
        public const string ConfirmDeleteKeyName = "confirmDelete";

        /// <summary>
        /// Gets or sets the value to confirm deletion request.
        /// </summary>
        public string ConfirmDelete { get; set; }

        /// <summary>
        /// Validates model.
        /// </summary>
        public override void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.ConfirmDelete)
                || (string.Compare(this.ConfirmDelete, "yes", true) != 0))
            {
                throw new MalformedRequestBodyException<DeleteAccountRecordingsErrorCode>(DeleteAccountRecordingsErrorCode.RequestMissingConfirmDelete, ConfirmDeleteKeyName);
            }
        }
    }
}
