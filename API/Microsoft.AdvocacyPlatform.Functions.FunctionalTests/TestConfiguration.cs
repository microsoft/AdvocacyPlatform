// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Newtonsoft.Json;

    /// <summary>
    /// Test configuration information.
    /// </summary>
    public class TestConfiguration
    {
        /// <summary>
        /// Gets or sets the input id to use with the functional test.
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// Gets or sets the  Dual-toned Multi-frequency signaling sequence to use with the functional test.
        /// </summary>
        public string Dtmf { get; set; }

        /// <summary>
        /// Gets or sets the  number of seconds to wait after the call is initiated in the functional test.
        /// </summary>
        public int InitSeconds { get; set; }

        /// <summary>
        /// Gets or sets the  number of seconds to wait after the DTMF sequence is completed in the functional test.
        /// </summary>
        public int FinalSeconds { get; set; }

        /// <summary>
        /// Gets or sets the  SID of the call resource (updated after InitateCallFunctionalTest).
        /// </summary>
        public string CallSid { get; set; }

        /// <summary>
        /// Gets or sets the  status of the call resource (updated after CheckCallProgressTest).
        /// </summary>
        public string CallStatus { get; set; }

        /// <summary>
        /// Gets or sets the  recording URI of the call recording (updated after PullRecordingFunctionalTest).
        /// </summary>
        public string RecordingUri { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all recordings for a call were deleted (updated after DeleteRecordingsFunctionalTest).
        /// </summary>
        public bool AllRecordingsDeleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all recordings for an account were deleted (updated after DeleteAccountRecordingsFunctionalTest).
        /// </summary>
        public bool AllAccountRecordingsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the  transcript of the recording (updated after TranscribeCallFunctionalTest).
        /// </summary>
        public string Transcript { get; set; }

        /// <summary>
        /// Gets or sets the  transcription data returned (updated after ExtractInfoFunctionalTest).
        /// </summary>
        public TranscriptionData Data { get; set; }

        /// <summary>
        /// Gets or sets the  entities expected to be extracted from the transcription (Date, Location, and/or Person).
        /// </summary>
        public HashSet<string> ExpectedEntities { get; set; }

        /// <summary>
        /// Gets or sets the additional entities collection expected to be returned by the data extractor.
        /// </summary>
        public Dictionary<string, string> ExpectedAdditionalEntities { get; set; }

        /// <summary>
        /// Gets or sets the  callback URL to send results to (only used with SendResultFunctionalTest).
        /// </summary>
        public string CallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the RecordingUri is a local path (only used to troubleshoot issues).
        /// </summary>
        public bool IsLocalRecordingPath { get; set; }

        /// <summary>
        /// Loads the configuration from a local file.
        /// </summary>
        /// <param name="filePath">Path to the local file.</param>
        /// <returns>A TestConfiguration file loaded from the file path.</returns>
        public static TestConfiguration Load(string filePath)
        {
            return JsonConvert.DeserializeObject<TestConfiguration>(
                File.ReadAllText(filePath));
        }

        /// <summary>
        /// Saves the configuration to a local file.
        /// </summary>
        /// <param name="filePath">Path to the local file.</param>
        public void Save(string filePath)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this));
        }
    }
}
