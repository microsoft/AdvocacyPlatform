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
    /// Mock for AzureBlobStorageClient.
    /// </summary>
    public class AzureBlobStorageClientMock : IStorageClient
    {
        /// <summary>
        /// Azure storage endpoint.
        /// </summary>
        public const string AzureStorageEndpoint = "https://blob.core.windows.net/";

        private ConcurrentDictionary<string, Tuple<string, string, bool>> _expectedRequestCache;
        private ConcurrentDictionary<string, long> _expectedResponseCache;
        private ConcurrentDictionary<string, StorageClientException> _expectedExceptions = new ConcurrentDictionary<string, StorageClientException>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageClientMock"/> class.
        /// </summary>
        public AzureBlobStorageClientMock()
        {
            _expectedRequestCache = new ConcurrentDictionary<string, Tuple<string, string, bool>>();
            _expectedResponseCache = new ConcurrentDictionary<string, long>();
            _expectedExceptions = new ConcurrentDictionary<string, StorageClientException>();
        }

        /// <summary>
        /// Simulates writing to a blob stream.
        /// </summary>
        /// <param name="fileStream">The input stream.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="createContainerIfNotExists">Flag indicating whether the container should be created if it doesn't exist.</param>
        /// <returns>Information regarding the blob written.</returns>
        public Task<Contracts.FileInfo> WriteStreamAsync(Stream fileStream, Secret connectionString, string containerName, string destinationPath, bool createContainerIfNotExists)
        {
            return Task.Run(() =>
            {
                string[] destinationPathParts = destinationPath.Split(new char[] { '_' });

                if (destinationPathParts.Length < 3)
                {
                    throw new ArgumentException("Destination path format is incorrect!");
                }

                string cacheKey = $"{destinationPathParts[0]}_";

                if (!_expectedRequestCache.ContainsKey(cacheKey))
                {
                    if (!_expectedExceptions.ContainsKey(cacheKey))
                    {
                        throw new KeyNotFoundException("This request was not expected!");
                    }
                    else
                    {
                        throw _expectedExceptions[cacheKey];
                    }
                }

                if (!_expectedResponseCache.ContainsKey(cacheKey))
                {
                    throw new KeyNotFoundException("No response was registered for this request!");
                }

                long sizeInBytes = _expectedResponseCache[cacheKey];

                DateTime.Parse(
                    string.Join(
                        " ",
                        destinationPathParts[1],
                        destinationPathParts[2]
                            .Replace(".wav", string.Empty)
                            .Replace("-", ":")));

                return new Contracts.FileInfo()
                {
                    SizeInBytes = sizeInBytes,
                    Uri = new Uri($"{AzureStorageEndpoint}{containerName}/{destinationPath}"),
                };
            });
        }

        /// <summary>
        /// Registers an expected HTTP request message.
        /// </summary>
        /// <param name="pathOrPathPrefix">The path or prefix the request is for.</param>
        /// <param name="connectionString">The connection string used in the request.</param>
        /// <param name="containerName">The name of the target container.</param>
        /// <param name="createContainerIfNotExists">Flag indicating whether the request states the container should be created if it does not exist.</param>
        /// <returns>True if request was registered successfully, false if it failed.</returns>
        public bool RegisterExpectedRequestMessage(string pathOrPathPrefix, string connectionString, string containerName, bool createContainerIfNotExists)
        {
            if (_expectedRequestCache.ContainsKey(pathOrPathPrefix))
            {
                throw new InvalidOperationException("A request for this path or path prefix already exists!");
            }

            return _expectedRequestCache.TryAdd(pathOrPathPrefix, new Tuple<string, string, bool>(connectionString, containerName, createContainerIfNotExists));
        }

        /// <summary>
        /// Registers an expected HTTP response message.
        /// </summary>
        /// <param name="destinationPathPrefix">The path or prefix the response is for.</param>
        /// <param name="sizeInBytes">The blob length to return.</param>
        /// <returns>True if response was registered successfully, false if it failed.</returns>
        public bool RegisterExpectedResponseMessage(string destinationPathPrefix, long sizeInBytes)
        {
            if (_expectedResponseCache.ContainsKey(destinationPathPrefix))
            {
                throw new InvalidOperationException("A response for this destination file already exists!");
            }

            return _expectedResponseCache.TryAdd(destinationPathPrefix, sizeInBytes);
        }

        /// <summary>
        /// Registers an expected exception to be thrown.
        /// </summary>
        /// <param name="secretName">The name of the secret the exception is associated with.</param>
        /// <param name="ex">The exception to throw.</param>
        /// <returns>True if expected exception was registered successfully, false if it failed.</returns>
        public bool RegisterExpectedException(string secretName, StorageClientException ex)
        {
            if (_expectedExceptions.ContainsKey(secretName))
            {
                throw new InvalidOperationException($"An exception for the secret with the identifier {secretName} was already registered!");
            }

            return _expectedExceptions.TryAdd(secretName, ex);
        }

        /// <summary>
        /// Clears the cache of expected HTTP request messages.
        /// </summary>
        public void ClearExpectedRequestMessages()
        {
            _expectedRequestCache.Clear();
        }

        /// <summary>
        /// Clears the cache of expected HTTP response messages.
        /// </summary>
        public void ClearExpectedResponseMessages()
        {
            _expectedResponseCache.Clear();
        }

        /// <summary>
        /// Simulates reading to a stream.
        /// </summary>
        /// <param name="outputStream">The stream to read to.</param>
        /// <param name="connectionString">The connection string used.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="sourcePath">The expected blob path.</param>
        /// <returns>An asynchronous task.</returns>
        public Task ReadToStreamAsync(Stream outputStream, Secret connectionString, string containerName, string sourcePath)
        {
            return Task.Run(() =>
            {
                if (!_expectedRequestCache.ContainsKey(sourcePath))
                {
                    if (!_expectedExceptions.ContainsKey(sourcePath))
                    {
                        throw new KeyNotFoundException("This request was not expected!");
                    }
                    else
                    {
                        throw _expectedExceptions[sourcePath];
                    }
                }

                Tuple<string, string, bool> expectedRequest = _expectedRequestCache[sourcePath];

                if (string.Compare(expectedRequest.Item1, connectionString.Value) != 0)
                {
                    throw new ArgumentException("Connection string not expected!");
                }

                if (string.Compare(expectedRequest.Item2, containerName) != 0)
                {
                    throw new ArgumentException("Container name not expected!");
                }
            });
        }
    }
}
