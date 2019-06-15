// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions
{
    using System;
    using System.Collections.Generic;
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
    using Twilio.Rest.Api.V2010.Account;
    using Contracts = Microsoft.AdvocacyPlatform.Contracts;
    using TwilioEx = Twilio.Exceptions;

    /// <summary>
    /// PullRecordings function implementation with HTTP trigger.
    /// </summary>
    public static class PullRecordingHttpTrigger
    {
        /// <summary>
        /// Name of the app setting containing the identifier for the secret containing the storage access key (rw).
        /// </summary>
        public const string StorageAccessKeySecretNameAppSettingName = "storageAccessKeySecretName";

        /// <summary>
        /// Name of the app setting containing the identifier for the secret containing the storage access key (w) to append to the returned full recording URL.
        /// </summary>
        public const string StorageReadAccessKeySecretNameAppSettingName = "storageReadAccessKeySecretName";

        /// <summary>
        /// Name of the app setting containing the connection string (sans access key) for connecting to backing storage.
        /// </summary>
        public const string StorageAccessConnectionStringAppSettingName = "storageAccessConnectionString";

        /// <summary>
        /// Name of the app setting containing the name of the container to store recordings.
        /// </summary>
        public const string StorageContainerNameAppSettingName = "storageContainerName";

        /// <summary>
        /// Name of the app setting specifying the token issuing authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        private static IServiceProvider _container = new ContainerBuilder()
            .RegisterModule(new HttpClientModule())
            .RegisterModule(new AzureKeyVaultSecretStoreModule()) // TODO: Currently, this module must be registered before the Twilio module! Fix this!
            .RegisterModule(new TwilioModule())
            .RegisterModule(new AzureBlobStorageModule())
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
        /// Copies a recording from Twilio storage to another data store.
        /// </summary>
        /// <param name="req">A request object who's request body respects the <see cref="Microsoft.AdvocacyPlatform.Contracts.PullRecordingRequest"/> schema.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <param name="context">Execution context to determine function app directory.</param>
        /// <returns>
        /// An OkObjectResult on successful executions, and a BadRequestObjectResult on unsuccessful executions
        /// with the result body respecting the <see cref="Microsoft.AdvocacyPlatform.Contracts.PullRecordingResponse"/> schema.
        /// </returns>
        [FunctionName("PullRecording")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("PullRecording C# HTTP trigger function processed a request.");

            PullRecordingRequest request = null;
            PullRecordingResponse response = new PullRecordingResponse();

            bool isBadRequest = false;

            try
            {
                log.LogInformation("Reading configuration...");
                IConfigurationRoot config = FunctionHelper.GetConfiguration(context);

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                request = FunctionRequestBase.Parse<PullRecordingRequest>(requestBody);

                response.CallSid = request.CallSid;

                string storageAccessConnectionString = config[StorageAccessConnectionStringAppSettingName];

                ISecretStore secretProvider = Container.GetService<ISecretStore>();
                Secret storageAccessKey = await secretProvider.GetSecretAsync(config[StorageAccessKeySecretNameAppSettingName], config[AuthorityAppSettingName]);

                Secret fullStorageAccessConnectionString = Utils.GetFullStorageConnectionString(storageAccessConnectionString, storageAccessKey);

                string storageContainerName = config[StorageContainerNameAppSettingName];

                Secret storageReadAccessKey = await secretProvider.GetSecretAsync(config[StorageReadAccessKeySecretNameAppSettingName], config[AuthorityAppSettingName]);

                ITwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>();

                await callWrapper.InitializeAsync(
                    config[GlobalConstants.TwilioAccountSidSecretNameAppSettingName],
                    config[GlobalConstants.TwilioAuthTokenSecretNameAppSettingName],
                    config[GlobalConstants.AuthorityAppSettingName]);

                IList<RecordingResource> recordings = await callWrapper.FetchRecordingsAsync(request.CallSid, log);

                if (recordings == null
                    || recordings.Count == 0)
                {
                    throw new TwilioNoRecordingsException("No recording");
                }

                Uri recordingUri = callWrapper.GetFullRecordingUri(recordings[0], log);

                log.LogInformation($"Retrieved recording URI = {recordingUri.AbsoluteUri}. Attempting to copy to storage...");

                IHttpClientWrapper httpClient = Container.GetService<IHttpClientWrapper>();
                IStorageClient storageClient = Container.GetService<IStorageClient>();

                string destinationPath = $"{request.InputId}/file_{DateTime.UtcNow.ToString("yyyy-MM-dd_hh-mm-ss")}.wav";

                Contracts.FileInfo recordingBlob = null;

                using (Stream stream = await httpClient.GetStreamAsync(recordingUri.AbsoluteUri, log))
                {
                    recordingBlob = await storageClient.WriteStreamAsync(
                        stream,
                        fullStorageAccessConnectionString,
                        storageContainerName,
                        destinationPath,
                        false);
                }

                response.RecordingLength = recordingBlob.SizeInBytes;
                response.RecordingUri = destinationPath;
                response.FullRecordingUrl = $"{recordingBlob.Uri}?{storageReadAccessKey.Value}";

                return FunctionHelper.ActionResultFactory<PullRecordingResponse>(false, response);
            }
            catch (MalformedRequestBodyException<PullRecordingErrorCode> ex)
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
            catch (TwilioEx.AuthenticationException ex)
            {
                log.LogError($"Twilio authentication exception: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.TwilioAuthenticationFailed;
                response.ErrorDetails = CommonErrorMessage.TwilioAuthenticationFailedMessage;
            }
            catch (TwilioEx.ApiConnectionException ex)
            {
                log.LogError($"Twilio API connection exception: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.TwilioApiConnectionFailed;
                response.ErrorDetails = CommonErrorMessage.TwilioApiConnectionFailedMessage;
            }
            catch (TwilioEx.ApiException ex)
            {
                log.LogError($"Twilio API call exception: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.TwilioApiRequestFailed;
                response.ErrorDetails = CommonErrorMessage.TwilioApiRequestFailedMessage;
            }
            catch (TwilioEx.RestException ex)
            {
                log.LogError($"Twilio REST call exception: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.TwilioRestCallFailed;
                response.ErrorDetails = CommonErrorMessage.TwilioRestCallFailedMessage;
            }
            catch (TwilioEx.TwilioException ex)
            {
                log.LogError($"Twilio generic exception: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.TwilioGenericFailure;
                response.ErrorDetails = CommonErrorMessage.TwilioGenericFailureMessage;
            }
            catch (TwilioNoRecordingsException ex)
            {
                response.ErrorCode = (int)CommonErrorCode.TwilioCallNoRecordings;
                response.ErrorDetails = ex.Message;
            }
            catch (StorageClientException ex)
            {
                log.LogError($"Storage client generic exception: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.StorageClientGenericFailure;
                response.ErrorDetails = CommonErrorMessage.StorageClientGenericFailureMessage;
            }
            catch (Exception ex)
            {
                isBadRequest = true;

                log.LogError($"Exception encountered: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.BadRequest;
            }

            response.HasError = true;

            return FunctionHelper.ActionResultFactory<PullRecordingResponse>(isBadRequest, response);
        }
    }
}
