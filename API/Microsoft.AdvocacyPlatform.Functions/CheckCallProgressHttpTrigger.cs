// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions
{
    using System;
    using System.Collections.Generic;
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
    /// CheckCallProgress function implementation with HTTP trigger.
    /// </summary>
    public static class CheckCallProgressHttpTrigger
    {
        private static List<CallResource.StatusEnum> _failedCallStatus = new List<CallResource.StatusEnum>()
        {
            CallResource.StatusEnum.Busy,
            CallResource.StatusEnum.Failed,
            CallResource.StatusEnum.NoAnswer,
            CallResource.StatusEnum.Canceled,
        };

        private static List<CallResource.StatusEnum> _inProgressCallStates = new List<CallResource.StatusEnum>()
        {
            CallResource.StatusEnum.Queued,
            CallResource.StatusEnum.Ringing,
            CallResource.StatusEnum.InProgress,
        };

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
        /// Gets the enumeration of call statuses considered to represent failures.
        /// </summary>
        public static List<CallResource.StatusEnum> FailedCallStatus => _failedCallStatus;

        /// <summary>
        /// Gets the enumeration of call statuses considered to represent an in-progress call.
        /// </summary>
        public static List<CallResource.StatusEnum> InProgressCallStates => _inProgressCallStates;

        /// <summary>
        /// Retrieves the state of a Twilio call.
        /// </summary>
        /// <param name="req">A request object who's request body respects the <see cref="Microsoft.AdvocacyPlatform.Contracts.CheckCallProgressRequest"/> schema.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <param name="context">Execution context to determine function app directory.</param>
        /// <returns>
        /// An OkObjectResult on successful executions, and a BadRequestObjectResult on unsuccessful executions
        /// with the result body respecting the <see cref="Microsoft.AdvocacyPlatform.Contracts.CheckCallProgressResponse"/> schema.
        /// </returns>
        [FunctionName("CheckCallProgress")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("CheckCallProgress C# HTTP trigger function processed a request.");

            CheckCallProgressRequest request = null;
            CheckCallProgressResponse response = new CheckCallProgressResponse();

            bool isBadRequest = false;

            try
            {
                log.LogInformation("Reading configuration...");
                IConfigurationRoot config = FunctionHelper.GetConfiguration(context);

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                request = FunctionRequestBase.Parse<CheckCallProgressRequest>(requestBody);

                response.CallSid = request.CallSid;

                ITwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>();

                await callWrapper.InitializeAsync(
                    config[GlobalConstants.TwilioAccountSidSecretNameAppSettingName],
                    config[GlobalConstants.TwilioAuthTokenSecretNameAppSettingName],
                    config[GlobalConstants.AuthorityAppSettingName]);

                CallResource call = await callWrapper.FetchCallAsync(request.CallSid, log);

                log.LogInformation($"Status of call {request.CallSid} is '{call.Status}'");

                int duration = 0;

                if (int.TryParse(call.Duration, out duration))
                {
                    response.Duration = duration;
                }
                else
                {
                    log.LogError("No call duration!");
                }

                response.Status = call.Status != null ? call.Status.ToString() : null;

                if (FailedCallStatus.Contains(call.Status))
                {
                    throw new TwilioFailedCallException(call.Status, "Call failed.");
                }

                // If we got here, the call should have a status of Completed. If not, treat unexpected status as an error
                if (call.Status != CallResource.StatusEnum.Completed
                    && !InProgressCallStates.Contains(call.Status))
                {
                    throw new TwilioUnknownCallStatusException(call.Status, "Unexpected call status.");
                }

                return FunctionHelper.ActionResultFactory<CheckCallProgressResponse>(false, response);
            }
            catch (MalformedRequestBodyException<CheckCallProgressErrorCode> ex)
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
            catch (TwilioFailedCallException ex)
            {
                response.ErrorCode = (int)CommonErrorCode.TwilioCallFailed;
                response.ErrorDetails = ex.Message;
            }
            catch (TwilioUnknownCallStatusException ex)
            {
                response.ErrorCode = (int)CommonErrorCode.TwilioUnexpectedCallStatus;
                response.ErrorDetails = ex.Message;
            }
            catch (Exception ex)
            {
                isBadRequest = true;

                log.LogError($"Exception encountered: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.BadRequest;
            }

            response.HasError = true;

            return FunctionHelper.ActionResultFactory<CheckCallProgressResponse>(isBadRequest, response);
        }
    }
}
