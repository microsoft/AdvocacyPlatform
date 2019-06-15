// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for interacting with a data store.
    /// </summary>
    public interface IStorageClient
    {
        /// <summary>
        /// Reads data from the data store to a stream.
        /// </summary>
        /// <param name="outputStream">The stream to read data to.</param>
        /// <param name="connectionString">The connection string for connecting to the data store.</param>
        /// <param name="containerName">The name of the container containing the file to read.</param>
        /// <param name="sourcePath">The relative path (from the container) of the file to read.</param>
        /// <returns>A Task returning completion of reading to the stream.</returns>
        Task ReadToStreamAsync(Stream outputStream, Secret connectionString, string containerName, string sourcePath);

        /// <summary>
        /// Writes data from a stream to the data store.
        /// </summary>
        /// <param name="fileStream">The stream to read data from.</param>
        /// <param name="connectionString">The connection string for connecting to the data store.</param>
        /// <param name="containerName">The name of the container to write the file.</param>
        /// <param name="destinationPath">The relative path (from the container) of the file to write.</param>
        /// <param name="createContainerIfNotExists">Specifies if the container should be created if it does not exist.</param>
        /// <returns>A Task returning information regarding the newly created file.</returns>
        Task<FileInfo> WriteStreamAsync(Stream fileStream, Secret connectionString, string containerName, string destinationPath, bool createContainerIfNotExists);
    }
}
