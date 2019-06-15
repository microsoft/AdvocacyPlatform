// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// ExtractInfo error messages.
    /// </summary>
    public static class ExtractInfoErrorMessage
    {
        /// <summary>
        /// Generic data extractor failure.
        /// </summary>
        public const string DataExtractorGenericFailureMessage = "An unexpected failure encountered with data extractor.";

        /// <summary>
        /// Transcription was canceled.
        /// </summary>
        public const string TranscriptionCanceledMessage = "Transcription was canceled.";

        /// <summary>
        /// Transcription is empty.
        /// </summary>
        public const string TranscriptionEmptyMessage = "Transcription is empty.";
    }
}
