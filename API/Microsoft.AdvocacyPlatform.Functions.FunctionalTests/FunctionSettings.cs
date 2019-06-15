// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Model representing configuration information required to call a remote Azure Function.
    /// </summary>
    public class FunctionSettings
    {
        /// <summary>
        /// Gets or sets the tenant id.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the application id.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the resource id.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the function's host name.
        /// </summary>
        public string FunctionHostName { get; set; }

        /// <summary>
        /// Gets or sets the function's host key.
        /// </summary>
        public string FunctionHostKey { get; set; }

        /// <summary>
        /// Gets the authorizing authority.
        /// </summary>
        [JsonIgnore]
        public string Authority
        {
            get => $"https://login.microsoftonline.com/{TenantId}";
        }

        /// <summary>
        /// Gets the URL for InitiateCall.
        /// </summary>
        [JsonIgnore]
        public string InitiateCallUrl
        {
            get => $"https://{FunctionHostName}.azurewebsites.net/api/InitiateCall?code={FunctionHostKey}";
        }

        /// <summary>
        /// Gets the URL for CheckCallProgress.
        /// </summary>
        [JsonIgnore]
        public string CheckCallProgressUrl
        {
            get => $"https://{FunctionHostName}.azurewebsites.net/api/CheckCallProgress?code={FunctionHostKey}";
        }

        /// <summary>
        /// Gets the URL for PullRecordings.
        /// </summary>
        [JsonIgnore]
        public string PullRecordingUrl
        {
            get => $"https://{FunctionHostName}.azurewebsites.net/api/PullRecording?code={FunctionHostKey}";
        }

        /// <summary>
        /// Gets the URL for TranscribeCall.
        /// </summary>
        [JsonIgnore]
        public string TranscribeCallUrl
        {
            get => $"https://{FunctionHostName}.azurewebsites.net/api/TranscribeCall?code={FunctionHostKey}";
        }

        /// <summary>
        /// Gets the URL for ExtractInfo.
        /// </summary>
        [JsonIgnore]
        public string ExtractInfoUrl
        {
            get => $"https://{FunctionHostName}.azurewebsites.net/api/ExtractInfo?code={FunctionHostKey}";
        }

        /// <summary>
        /// Gets the URL for DeleteRecordings.
        /// </summary>
        [JsonIgnore]
        public string DeleteRecordingsUrl
        {
            get => $"https://{FunctionHostName}.azurewebsites.net/api/DeleteRecordings?code={FunctionHostKey}";
        }

        /// <summary>
        /// Gets the URL for DeleteAccountRecordings.
        /// </summary>
        [JsonIgnore]
        public string DeleteAccountRecordingsUrl
        {
            get => $"https://{FunctionHostName}.azurewebsites.net/api/DeleteAccountRecordings?code={FunctionHostKey}";
        }

        /// <summary>
        /// Loads settings from a file.
        /// </summary>
        /// <param name="filePath">Path to the setting files.</param>
        /// <returns>The deserialized settings.</returns>
        public static FunctionSettings Load(string filePath)
        {
            return JsonConvert.DeserializeObject<FunctionSettings>(
                File.ReadAllText(filePath));
        }
    }
}
