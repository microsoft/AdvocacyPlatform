// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.CognitiveServices.Speech;
    using Microsoft.CognitiveServices.Speech.Audio;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Factory for creating BinaryReaders.
    /// </summary>
    public class BinaryReaderFactory
    {
        /// <summary>
        /// Returns a BinaryReader instance for reading a public URI or local file.
        /// </summary>
        /// <param name="filePath">Path of the file to read.</param>
        /// <param name="isUri">Species if the path is a URI.</param>
        /// <param name="httpClient">The IHttpClientWrapper implementation to use when reading from remote URIs.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>A BinaryReader instance.</returns>
        public static BinaryReader GetBinaryReader(string filePath, bool isUri, IHttpClientWrapper httpClient, ILogger log)
        {
            if (isUri)
            {
                return new BinaryReader(httpClient.GetStreamAsync(filePath, log).Result);
            }
            else
            {
                return new BinaryReader(File.Open(filePath, FileMode.Open));
            }
        }

        // TODO: Improve this

        /// <summary>
        /// Returns a BinaryReader instance for reading using an IStorageClient.
        /// </summary>
        /// <param name="fileUri">URI of the resource to read.</param>
        /// <param name="outputStream">Stream to read data into.</param>
        /// <param name="storageClient">IStorageClient implementation to use when reading from the source data store.</param>
        /// <param name="storageConnectionString">Connection string to the source data store.</param>
        /// <param name="storageContainerName">The name of the container containing the file to read.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>A BinaryReader instance.</returns>
        public static BinaryReader GetBinaryReader(string fileUri, Stream outputStream, IStorageClient storageClient, Secret storageConnectionString, string storageContainerName, ILogger log)
        {
            storageClient.ReadToStreamAsync(outputStream, storageConnectionString, storageContainerName, fileUri).Wait();
            outputStream.Seek(0, SeekOrigin.Begin);

            return new BinaryReader(outputStream);
        }
    }
}
