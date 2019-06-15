// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Contracts = Microsoft.AdvocacyPlatform.Contracts;

    /// <summary>
    /// IStorageClient implementation for interacting with an Azure Blob Storage resource.
    /// </summary>
    public class AzureBlobStorageClient : IStorageClient
    {
        /// <summary>
        /// Reads data from a blob to a stream.
        /// </summary>
        /// <param name="outputStream">The stream to read to.</param>
        /// <param name="connectionString">Connection string to the source blob storage.</param>
        /// <param name="containerName">Name of the container containing the blob.</param>
        /// <param name="sourcePath">The relative path (from the container) of the blob.</param>
        /// <returns>A Task representing the work.</returns>
        public async Task ReadToStreamAsync(Stream outputStream, Secret connectionString, string containerName, string sourcePath)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString.Value);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(sourcePath);

                bool exists = await blockBlob.ExistsAsync();

                // TODO: Throw a better exception
                if (!exists)
                {
                    throw new Exception("Blob does not exists!");
                }

                await blockBlob.DownloadToStreamAsync(outputStream);
            }
            catch (StorageException ex)
            {
                throw new StorageClientException("Storage client encountered exception. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Writes data from a stream to a blob.
        /// </summary>
        /// <param name="fileStream">The stream to read from.</param>
        /// <param name="connectionString">The connection string for the destination blob storage.</param>
        /// <param name="containerName">The name of the container to place the blob.</param>
        /// <param name="destinationPath">The relative path (from the container) for the blob.</param>
        /// <param name="createContainerIfNotExists">Specifies if the container should be created if it does not exist.</param>
        /// <returns>A <see cref="Microsoft.AdvocacyPlatform.Contracts.FileInfo"/> object representing the new blob.</returns>
        public async Task<Contracts.FileInfo> WriteStreamAsync(Stream fileStream, Secret connectionString, string containerName, string destinationPath, bool createContainerIfNotExists)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString.Value);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                if (createContainerIfNotExists)
                {
                    await container.CreateIfNotExistsAsync();
                }

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(destinationPath);

                bool exists = await blockBlob.ExistsAsync();

                if (exists)
                {
                    throw new RecordingExistsException(destinationPath);
                }

                await blockBlob.UploadFromStreamAsync(fileStream);
                IEnumerable<ListBlockItem> blockBlobBlocks = await blockBlob.DownloadBlockListAsync();

                return new Contracts.FileInfo()
                {
                    SizeInBytes = blockBlobBlocks.Sum(x => x.Length),
                    Uri = blockBlob.Uri,
                };
            }
            catch (StorageException ex)
            {
                throw new StorageClientException("Storage client encountered exception", ex);
            }
        }
    }
}
