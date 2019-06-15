// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
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
    using TwilioEx = Twilio.Exceptions;

    // TODO: Need to introduce rate limiting
    //       1 request / second based on Twilio limitation

    /// <summary>
    /// InitiateCall function implementation with HTTP trigger.
    /// </summary>
    public static class InitiateCallHttpTrigger
    {
        /// <summary>
        /// The base URL for TwiML requests.
        /// </summary>
        public const string TwiMLBaseUrl = "http://twimlets.com/echo?Twiml=";

        /// <summary>
        /// Name of the app setting containing the identifier for the secret containing the Twilio local number to use.
        /// </summary>
        public const string TwilioLocalNumberSecretNameAppSettingName = "twilioLocalNumberSecretName";

        /// <summary>
        /// Name of the app setting specifying the token issuing authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        /// <summary>
        /// Name of the app setting specifying the TwiML template to use for constructing TwiML expressions.
        /// </summary>
        public const string TwiMLTemplateAppSettingName = "twilioTwiMLTemplate";

        /// <summary>
        /// Name of the app setting specifying the number of seconds to pause after placing a call.
        /// </summary>
        public const string CallInitialPauseInSecondsAppSettingName = "callInitialPauseInSeconds";

        /// <summary>
        /// Name of the app setting specifying the number of seconds to pause after all actions have been completed in a call.
        /// </summary>
        public const string CallFinalPauseInSecondsAppSettingName = "callFinalPauseInSeconds";

        /// <summary>
        /// Name of the app setting specifying the number to call.
        /// </summary>
        public const string NumberToCallAppSettingName = "numberToCall";

        /// <summary>
        /// Name of the app setting specifying the default Dual-tone Multi-frequency signaling (DTMF) template to use with the call.
        /// </summary>
        public const string DefaultDtmfTemplateAppSettingName = "defaultDtmfTemplate";

        /// <summary>
        /// Name of the placeholder in the DTMF template to replace with the input id.
        /// </summary>
        public const string InputIdTemplateKey = "inputId";

        private static IServiceProvider _container = new ContainerBuilder()
            .RegisterModule(new HttpClientModule())
            .RegisterModule(new AzureKeyVaultSecretStoreModule()) // TODO: Currently, this module must be registered before the Twilio module! Fix this!
            .RegisterModule(new TwilioModule())
            .RegisterModule(new ValueValidatorFactoryModule())
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
        /// Formats the DTMF template with the input id specified.
        /// </summary>
        /// <param name="defaultDtmfTemplate">The template to format.</param>
        /// <param name="inputId">The input id to inject into the template.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <returns>The formatted template.</returns>
        public static string BuildDefaultDtmfSequence(string defaultDtmfTemplate, string inputId, ILogger log)
        {
            return defaultDtmfTemplate.Replace($"{{{InputIdTemplateKey}}}", inputId);
        }

        /// <summary>
        /// Schedules a call and returns the call sid.
        /// </summary>
        /// <param name="req">A request object who's request body respects the <see cref="Microsoft.AdvocacyPlatform.Contracts.InitiateCallRequest"/> schema.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <param name="context">Execution context to determine function app directory.</param>
        /// <returns>
        /// An OkObjectResult on successful executions, and a BadRequestObjectResult on unsuccessful executions
        /// with the result body respecting the <see cref="Microsoft.AdvocacyPlatform.Contracts.InitiateCallResponse"/> schema.
        /// </returns>
        [FunctionName("InitiateCall")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("InitiateCall C# HTTP trigger function processed a request.");

            InitiateCallRequest request = null;
            InitiateCallResponse response = new InitiateCallResponse();

            bool isBadRequest = false;

            try
            {
                log.LogInformation("Reading configuration...");
                IConfigurationRoot config = FunctionHelper.GetConfiguration(context);

                log.LogInformation("Attempting to read request body...");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                request = FunctionRequestBase.Parse<InitiateCallRequest>(requestBody);

                // TODO: Hard-coding for now so a change does not need to be made
                //       in the logic app
                response.InputType = request.InputType = "ain";

                response.InputId = request.InputId;

                string acceptedInputId = HandleValueValidation(request, log);

                response.AcceptedInputId = acceptedInputId;

                log.LogInformation("Attempting to pull secret for programmatic calling service api key...");
                ISecretStore secretProvider = Container.GetService<ISecretStore>();

                Secret twilioLocalNumber = await secretProvider.GetSecretAsync(config[TwilioLocalNumberSecretNameAppSettingName], config[AuthorityAppSettingName]);

                string twiMLTemplate = config[TwiMLTemplateAppSettingName];
                int callInitialPauseInSeconds = request.Dtmf != null && request.Dtmf.InitPause.HasValue ? request.Dtmf.InitPause.Value : int.Parse(config[CallInitialPauseInSecondsAppSettingName]);
                int callFinalPauseInSeconds = request.Dtmf != null && request.Dtmf.FinalPause.HasValue ? request.Dtmf.FinalPause.Value : int.Parse(config[CallFinalPauseInSecondsAppSettingName]);
                string numberToCall = config[NumberToCallAppSettingName];
                string dtmfSequence = request.Dtmf != null && !string.IsNullOrWhiteSpace(request.Dtmf.Dtmf) ? request.Dtmf.Dtmf : BuildDefaultDtmfSequence(config[DefaultDtmfTemplateAppSettingName], acceptedInputId, log);

                log.LogInformation("Attempting to construct TwiML URL...");
                Uri twiMLUrl = FormatTwiMLUrl(twiMLTemplate, new Dictionary<string, string>()
                {
                    { "callInitialPauseInSeconds", callInitialPauseInSeconds.ToString() },
                    { "callFinalPauseInSeconds", callFinalPauseInSeconds.ToString() },
                    { "dtmfSequence", dtmfSequence },
                });

                log.LogInformation($"Call task received request for AIN = {request.InputId}");

                ITwilioCallWrapper callWrapper = Container.GetService<ITwilioCallWrapper>();

                await callWrapper.InitializeAsync(
                    config[GlobalConstants.TwilioAccountSidSecretNameAppSettingName],
                    config[GlobalConstants.TwilioAuthTokenSecretNameAppSettingName],
                    config[GlobalConstants.AuthorityAppSettingName]);

                response.CallSid = await callWrapper.PlaceAndRecordCallAsync(
                    twiMLUrl,
                    numberToCall,
                    twilioLocalNumber.Value,
                    log);

                log.LogInformation($"Call scheduled, call SID = {response.CallSid}");

                return FunctionHelper.ActionResultFactory<InitiateCallResponse>(false, response);
            }
            catch (MalformedRequestBodyException<InitiateCallErrorCode> ex)
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

            return FunctionHelper.ActionResultFactory<InitiateCallResponse>(isBadRequest, response);
        }

        private static string HandleValueValidation(InitiateCallRequest request, ILogger log)
        {
            IValueValidatorFactory valueValidatorFactory = Container.GetService<IValueValidatorFactory>();

            IValueValidator valueValidator = valueValidatorFactory.Create(request.InputType);

            string acceptedInput = null;

            if (valueValidator != null)
            {
                if (!valueValidator.Validate(request.InputId, out acceptedInput))
                {
                    throw new ValueValidationException($"InputId is not valid for the specified InputType ({request.InputType})!");
                }
            }

            return acceptedInput;
        }

        /// <summary>
        /// Creates a fully-formed TwiML URL based on the template and parameters.
        /// </summary>
        /// <param name="twiMLTemplate">The template to format.</param>
        /// <param name="parameters">Key-value pairs to inject into the template.</param>
        /// <returns>The formatted URI.</returns>
        private static Uri FormatTwiMLUrl(string twiMLTemplate, Dictionary<string, string> parameters)
        {
            StringBuilder twiMLUrl = new StringBuilder(twiMLTemplate);

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                twiMLUrl = twiMLUrl.Replace($"{{{parameter.Key}}}", parameter.Value);
            }

            string encodeTwiMLUrl = HttpUtility.UrlEncode(twiMLUrl.ToString());

            return new Uri($"{TwiMLBaseUrl}{encodeTwiMLUrl}");
        }
    }
}
