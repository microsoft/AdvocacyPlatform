// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions
{
    using System;
    using System.IO;
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
    using TwilioEx = Twilio.Exceptions;

    /// <summary>
    /// DeleteRecordings function implementation with HTTP trigger.
    /// </summary>
    public static class DeleteRecordingsHttpTrigger
    {
        private static IServiceProvider _container = new ContainerBuilder()
            .RegisterModule(new HttpClientModule())
            .RegisterModule(new AzureKeyVaultSecretStoreModule()) // TODO: Currently, this module must be registered before the Twilio module! Fix this!
            .RegisterModule(new TwilioModule())
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
        /// Deletes all recordings associated with a a Twilio call.
        /// </summary>
        /// <param name="req">A request object who's request body respects the <see cref="Microsoft.AdvocacyPlatform.Contracts.DeleteRecordingsRequest"/> schema.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <param name="context">Execution context to determine function app directory.</param>
        /// <returns>
        /// An OkObjectResult on successful executions, and a BadRequestObjectResult on unsuccessful executions
        /// with the result body respecting the <see cref="Microsoft.AdvocacyPlatform.Contracts.DeleteRecordingsResponse"/> schema.
        /// </returns>
        [FunctionName("DeleteRecordings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("DeleteRecordings C# HTTP trigger function processed a request.");

            DeleteRecordingsRequest request = null;
            DeleteRecordingsResponse response = new DeleteRecordingsResponse();

            bool isBadRequest = false;

            try
            {
                log.LogInformation("Reading configuration...");
                IConfigurationRoot config = FunctionHelper.GetConfiguration(context);

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                request = FunctionRequestBase.Parse<DeleteRecordingsRequest>(requestBody);

                response.CallSid = request.CallSid;

                ITwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>();

                await callWrapper.InitializeAsync(
                    config[GlobalConstants.TwilioAccountSidSecretNameAppSettingName],
                    config[GlobalConstants.TwilioAuthTokenSecretNameAppSettingName],
                    config[GlobalConstants.AuthorityAppSettingName]);

                response.AreAllRecordingsDeleted = await callWrapper.DeleteRecordingsAsync(request.CallSid, log);

                return FunctionHelper.ActionResultFactory<DeleteRecordingsResponse>(false, response);
            }
            catch (MalformedRequestBodyException<DeleteRecordingsErrorCode> ex)
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
            catch (Exception ex)
            {
                isBadRequest = true;

                log.LogError($"Exception encountered: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.BadRequest;
            }

            response.HasError = true;

            return FunctionHelper.ActionResultFactory<DeleteRecordingsResponse>(isBadRequest, response);
        }
    }
}
