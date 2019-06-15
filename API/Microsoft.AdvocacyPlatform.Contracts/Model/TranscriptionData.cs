// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Data extracted by an IDataExtractor.
    /// </summary>
    public class TranscriptionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranscriptionData"/> class.
        /// </summary>
        public TranscriptionData()
        {
            this.Date = new DateInfo();
            this.Location = new LocationInfo();
            this.Person = new PersonInfo();
        }

        /// <summary>
        /// Gets or sets the intent returned from an INlpDataExtractor.
        /// </summary>
        public string Intent { get; set; }

        /// <summary>
        /// Gets or sets the confidence for the intent returned.
        /// </summary>
        public decimal IntentConfidence { get; set; }

        /// <summary>
        /// Gets or sets the transcription analyzed.
        /// </summary>
        public string Transcription { get; set; }

        /// <summary>
        /// Gets or sets the transcript sent to LUIS for evaluation.
        /// </summary>
        public string EvaluatedTranscription { get; set; }

        /// <summary>
        /// Gets or sets the datetime information extracted.
        /// </summary>
        public DateInfo Date { get; set; }

        /// <summary>
        /// Gets or sets the location information extracted.
        /// </summary>
        public LocationInfo Location { get; set; }

        /// <summary>
        /// Gets or sets the person information extracted.
        /// </summary>
        public PersonInfo Person { get; set; }

        /// <summary>
        /// Gets or sets any additional data to return.
        /// </summary>
        public Dictionary<string, string> AdditionalData { get; set; }
    }
}
