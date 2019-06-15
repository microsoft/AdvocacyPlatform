// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Interface for interacting with a transcription (Speech-to-Text) service.
    /// </summary>
    public interface ITranscriber
    {
        /// <summary>
        /// Transcribes a audio file at a specific non-public URI.
        /// </summary>
        /// <param name="apiKey">The API key to use to connect to the transcription service.</param>
        /// <param name="region">The region the transcription service is in.</param>
        /// <param name="audioFileUri">The URI of the audio file.</param>
        /// <param name="storageClient">An IStorageClient implementation to use to read the audio file.</param>
        /// <param name="storageConnectionString">The connection string to use when connecting to the source data store.</param>
        /// <param name="storageContainerName">The container the audio file is in.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the transcription.</returns>
        Task<string> TranscribeAudioFileUriAsync(Secret apiKey, string region, string audioFileUri, IStorageClient storageClient, Secret storageConnectionString, string storageContainerName, ILogger log);

        /// <summary>
        /// Transcribes an audio file at a specific public URI.
        /// </summary>
        /// <param name="apiKey">The API key to use to connect to the transcription service.</param>
        /// <param name="region">The region the transcription service is in.</param>
        /// <param name="audioFileUri">The URI of the audio file.</param>
        /// <param name="httpClient">The IHttpClientWrapper implementation to use when making REST calls.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the transcription.</returns>
        Task<string> TranscribeAudioFileUriAsync(Secret apiKey, string region, string audioFileUri, IHttpClientWrapper httpClient, ILogger log);

        /// <summary>
        /// Transcribes an audio file at a local file path.
        /// </summary>
        /// <param name="apiKey">The API key to use to connect to the transcription service.</param>
        /// <param name="region">The region the transcription service is in.</param>
        /// <param name="audioFilePath">The path of the audio file.</param>
        /// <param name="log">Trace logging instance.</param>
        /// <returns>A Task returning the transcription.</returns>
        Task<string> TranscribeAudioFilePathAsync(Secret apiKey, string region, string audioFilePath, ILogger log);
    }
}
