// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests.Mocks.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Mocks the AzureTranscriber.
    /// </summary>
    public class AzureTranscriberMock : ITranscriber
    {
        private ConcurrentDictionary<string, Tuple<string, string>> _expectedRequestCache;
        private ConcurrentDictionary<string, string> _expectedResponseCache;
        private ConcurrentDictionary<string, TranscriberException> _expectedExceptionsCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTranscriberMock"/> class.
        /// </summary>
        public AzureTranscriberMock()
        {
            _expectedRequestCache = new ConcurrentDictionary<string, Tuple<string, string>>();
            _expectedResponseCache = new ConcurrentDictionary<string, string>();
            _expectedExceptionsCache = new ConcurrentDictionary<string, TranscriberException>();
        }

        /// <summary>
        /// Registers an expected request.
        /// </summary>
        /// <param name="audioFilePath">The expected path in the request.</param>
        /// <param name="apiKey">The expected API key used in the request.</param>
        /// <param name="region">The expected region in the request.</param>
        /// <returns>True if registered successfully, false if failed.</returns>
        public bool RegisterExpectedRequestMessage(string audioFilePath, string apiKey, string region)
        {
            if (_expectedRequestCache.ContainsKey(audioFilePath))
            {
                throw new InvalidOperationException("A transcription for this audio file already exists!");
            }

            return _expectedRequestCache.TryAdd(audioFilePath, new Tuple<string, string>(apiKey, region));
        }

        /// <summary>
        /// Registers an expected response.
        /// </summary>
        /// <param name="audioFilePath">The expected path in the response.</param>
        /// <param name="transcript">The expected transcript in the response.</param>
        /// <returns>True if registered successfully, false if failed.</returns>
        public bool RegisterExpectedResponseMessage(string audioFilePath, string transcript)
        {
            if (_expectedResponseCache.ContainsKey(audioFilePath))
            {
                throw new InvalidOperationException("A transcription for this audio file already exists!");
            }

            return _expectedResponseCache.TryAdd(audioFilePath, transcript);
        }

        /// <summary>
        /// Registers an expected exception.
        /// </summary>
        /// <param name="secretName">The secret to associate with the exception.</param>
        /// <param name="ex">The exception to throw.</param>
        /// <returns>True if registered successfully, false if failed.</returns>
        public bool RegisterExpectedException(string secretName, TranscriberException ex)
        {
            if (_expectedExceptionsCache.ContainsKey(secretName))
            {
                throw new InvalidOperationException($"An exception for the secret with the identifier {secretName} was already registered!");
            }

            return _expectedExceptionsCache.TryAdd(secretName, ex);
        }

        /// <summary>
        /// Simulates transcribing and audio file from a specified path.
        /// </summary>
        /// <param name="apiKey">The expected API key used in the request.</param>
        /// <param name="region">The expected region specified in the request.</param>
        /// <param name="audioFilePath">The expected path to the audio file.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The transcribed audio content.</returns>
        public Task<string> TranscribeAudioFilePathAsync(Secret apiKey, string region, string audioFilePath, ILogger log)
        {
            return Task.Run(() =>
            {
                if (!_expectedRequestCache.ContainsKey(audioFilePath))
                {
                    throw new KeyNotFoundException("No expected request for this audio file!");
                }

                Tuple<string, string> expectedParams = _expectedRequestCache[audioFilePath];

                if (string.Compare(expectedParams.Item1, apiKey.Value) != 0)
                {
                    throw new ArgumentException("The wrong api key was used!");
                }

                if (string.Compare(expectedParams.Item2, region) != 0)
                {
                    throw new ArgumentException("The wrong region was used!");
                }

                if (!_expectedResponseCache.ContainsKey(audioFilePath))
                {
                    throw new KeyNotFoundException("No expected response for this audio file!");
                }

                return _expectedResponseCache[audioFilePath];
            });
        }

        /// <summary>
        /// Simulates transcribing an audio file from a URI.
        /// </summary>
        /// <param name="apiKey">The expected API key used in the request.</param>
        /// <param name="region">The expected region specified in the request.</param>
        /// <param name="audioFilePath">The expected audio file path.</param>
        /// <param name="httpClient">The IHttpClientWrapper instance to use.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The transcribed audio content.</returns>
        public Task<string> TranscribeAudioFileUriAsync(Secret apiKey, string region, string audioFilePath, IHttpClientWrapper httpClient, ILogger log)
        {
            return Task.Run(() =>
            {
                if (!_expectedRequestCache.ContainsKey(audioFilePath))
                {
                    throw new KeyNotFoundException("No expected request for this audio file!");
                }

                Tuple<string, string> expectedParams = _expectedRequestCache[audioFilePath];

                if (string.Compare(expectedParams.Item1, apiKey.Value) != 0)
                {
                    throw new ArgumentException("The wrong api key was used!");
                }

                if (string.Compare(expectedParams.Item2, region) != 0)
                {
                    throw new ArgumentException("The wrong region was used!");
                }

                if (!_expectedResponseCache.ContainsKey(audioFilePath))
                {
                    throw new KeyNotFoundException("No expected response for this audio file!");
                }

                return _expectedResponseCache[audioFilePath];
            });
        }

        /// <summary>
        /// Simulates transcribing an audio file from a URI.
        /// </summary>
        /// <param name="apiKey">The expected API key used in the request.</param>
        /// <param name="region">The expected region specified in the request.</param>
        /// <param name="audioFileUri">The expected URI to the audio file.</param>
        /// <param name="storageClient">The IStorageClient instance to use.</param>
        /// <param name="storageAccessKey">The expected storage access key used for connecting to the data store.</param>
        /// <param name="storageContainerName">The expected container name.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The transcribed audio content.</returns>
        public async Task<string> TranscribeAudioFileUriAsync(Secret apiKey, string region, string audioFileUri, IStorageClient storageClient, Secret storageAccessKey, string storageContainerName, ILogger log)
        {
            if (!_expectedRequestCache.ContainsKey(audioFileUri))
            {
                if (!_expectedExceptionsCache.ContainsKey(audioFileUri))
                {
                    throw new KeyNotFoundException("No expected request for this audio file!");
                }
                else
                {
                    throw _expectedExceptionsCache[audioFileUri];
                }
            }

            Tuple<string, string> expectedParams = _expectedRequestCache[audioFileUri];

            if (string.Compare(expectedParams.Item1, apiKey.Value) != 0)
            {
                throw new ArgumentException("The wrong api key was used!");
            }

            if (string.Compare(expectedParams.Item2, region) != 0)
            {
                throw new ArgumentException("The wrong region was used!");
            }

            await TranscribeAudioStorageUriCommonAsync(apiKey, region, audioFileUri, storageClient, storageAccessKey, storageContainerName, log);

            if (!_expectedResponseCache.ContainsKey(audioFileUri))
            {
                throw new KeyNotFoundException("No expected response for this audio file!");
            }

            return _expectedResponseCache[audioFileUri];
        }

        private async Task<string> TranscribeAudioStorageUriCommonAsync(Secret apiKey, string region, string audioFileUri, IStorageClient storageClient, Secret storageConnectionString, string storageContainerName, ILogger log)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                await storageClient.ReadToStreamAsync(outputStream, storageConnectionString, storageContainerName, audioFileUri);
            }

            return string.Empty;
        }
    }
}
