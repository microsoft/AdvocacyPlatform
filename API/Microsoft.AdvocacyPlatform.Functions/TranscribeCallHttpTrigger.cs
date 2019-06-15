// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions
{
    using System;
    using System.IO;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Clients;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.AdvocacyPlatform.Functions.Module;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// TranscribeCall function implementation with HTTP trigger.
    /// </summary>
    public static class TranscribeCallHttpTrigger
    {
        /// <summary>
        /// Name of the app setting containing the identifier for the secret containing the API key.
        /// </summary>
        public const string ApiKeySecretNameAppSettingName = "speechApiKeySecretName";

        /// <summary>
        /// Name of the app setting containing the identifier for the secret containing the storage access key (rw).
        /// </summary>
        public const string StorageAccessKeySecretNameAppSettingName = "storageAccessKeySecretName";

        /// <summary>
        /// Name of the app setting specifying the region of the cognitive service resource.
        /// </summary>
        public const string RegionAppSettingName = "speechApiRegion";

        /// <summary>
        /// Name of the app setting specifying the storage connection string (sans access key).
        /// </summary>
        public const string StorageAccessConnectionStringAppSettingName = "storageAccessConnectionString";

        /// <summary>
        /// Name of the app setting specifying the name of the container recordings are stored in.
        /// </summary>
        public const string StorageContainerNameAppSettingName = "storageContainerName";

        /// <summary>
        /// Name of the app setting specifying the token issuing authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        private static IServiceProvider _container = new ContainerBuilder()
            .RegisterModule(new HttpClientModule())
            .RegisterModule(new AzureKeyVaultSecretStoreModule())
            .RegisterModule(new AzureBlobStorageModule())
            .RegisterModule(new AzureTranscriberModule())
            .Build();

        /// <summary>
        /// Gets or sets the dependency injection container.
        /// </summary>
        public static IServiceProvider Container
        {
            get => _container;
            set
            {
                _container = value;
            }
        }

        /// <summary>
        /// Transcribes a recording.
        /// </summary>
        /// <param name="req">A request object who's request body respects the <see cref="Microsoft.AdvocacyPlatform.Contracts.TranscribeCallRequest"/> schema.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <param name="context">Execution context to determine function app directory.</param>
        /// <returns>
        /// An OkObjectResult on successful executions, and a BadRequestObjectResult on unsuccessful executions
        /// with the result body respecting the <see cref="Microsoft.AdvocacyPlatform.Contracts.TranscribeCallResponse"/> schema.
        /// </returns>
        [FunctionName("TranscribeCall")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("TranscribeCall C# HTTP trigger function processed a request.");

            TranscribeCallRequest request = null;
            TranscribeCallResponse response = new TranscribeCallResponse();

            bool isBadRequest = false;

            try
            {
                log.LogInformation("Reading configuration...");
                IConfigurationRoot config = FunctionHelper.GetConfiguration(context);

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                request = FunctionRequestBase.Parse<TranscribeCallRequest>(requestBody);

                response.CallSid = request.CallSid;

                ISecretStore secretProvider = Container.GetService<ISecretStore>();
                Secret apiKey = await secretProvider.GetSecretAsync(config[ApiKeySecretNameAppSettingName], config[AuthorityAppSettingName]);

                string region = config[RegionAppSettingName];

                string storageAccessConnectionString = config[StorageAccessConnectionStringAppSettingName];

                Secret storageAccessKey = await secretProvider.GetSecretAsync(config[StorageAccessKeySecretNameAppSettingName], config[AuthorityAppSettingName]);

                Secret fullStorageAccessConnectionString = Utils.GetFullStorageConnectionString(storageAccessConnectionString, storageAccessKey);

                string storageContainerName = config[StorageContainerNameAppSettingName];

                ITranscriber transcriber = Container.GetService<ITranscriber>();
                IStorageClient storageClient = Container.GetService<IStorageClient>();

                string transcript = null;

                if (request.IsLocalPath)
                {
                    response.Text = await transcriber.TranscribeAudioFilePathAsync(apiKey, region, request.RecordingUri, log);
                }
                else
                {
                    response.Text = await transcriber.TranscribeAudioFileUriAsync(apiKey, region, request.RecordingUri, storageClient, fullStorageAccessConnectionString, storageContainerName, log);
                }

                log.LogInformation($"Transcript = {transcript}");

                return FunctionHelper.ActionResultFactory<TranscribeCallResponse>(false, response);
            }
            catch (MalformedRequestBodyException<TranscribeCallErrorCode> ex)
            {
                log.LogError($"Malformed request body; missing key: {ex.KeyName}");

                isBadRequest = true;

                response.ErrorCode = (int)ex.ErrorCode;
                response.ErrorDetails = ex.Message;
            }
            catch (SecretStoreException ex)
            {
                log.LogError($"Secret store generic exception: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.SecretStoreGenericFailure;
                response.ErrorDetails = CommonErrorMessage.SecretStoreGenericFailureMessage;
            }
            catch (StorageClientException ex)
            {
                log.LogError($"Storage client generic exception: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.StorageClientGenericFailure;
                response.ErrorDetails = CommonErrorMessage.StorageClientGenericFailureMessage;
            }
            catch (TranscriberCanceledException ex)
            {
                log.LogError($"Transcription was canceled: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)TranscribeCallErrorCode.TranscriptionCanceled;
                response.ErrorDetails = TranscribeCallErrorMessage.TranscriptionCanceledMessage;
            }
            catch (TranscriberEmptyTranscriptException ex)
            {
                log.LogError($"Transcription is empty: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)TranscribeCallErrorCode.TranscriptionEmpty;
                response.ErrorDetails = TranscribeCallErrorMessage.TranscriptionEmptyMessage;
            }
            catch (Exception ex)
            {
                isBadRequest = true;

                log.LogError($"Exception encountered: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.BadRequest;
            }

            response.HasError = true;

            return FunctionHelper.ActionResultFactory<TranscribeCallResponse>(isBadRequest, response);
        }
    }
}
