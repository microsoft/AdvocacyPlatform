// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    /// ExtractInfo function implementation with HTTP trigger.
    /// </summary>
    public static class ExtractInfoHttpTrigger
    {
        /// <summary>
        /// Name of the app setting specifying the LUIS endpoint to use.
        /// </summary>
        public const string LuisEndpointAppSettingName = "luisEndpoint";

        /// <summary>
        /// Name of the app setting specifying the URI of the secret representing the LUIS subscription key.
        /// </summary>
        public const string LuisSubscriptionKeySecretNameAppSettingName = "luisSubscriptionKeySecretName";

        /// <summary>
        /// Name of the app setting specifying configuration information for setting up the LuisDataExtractor.
        /// </summary>
        public const string LuisConfigurationAppSettingName = "luisConfiguration";

        /// <summary>
        /// Name of the app setting specifying the token issuing authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        /// <summary>
        /// Default transformations.
        /// </summary>
        private static readonly ICollection<DataTransformation> _defaultTransformations = new List<DataTransformation>()
        {
            new DataTransformation()
            {
                Name = DefaultTransformationFactory.RemovePunctuationTransformationName,
            },
            new DataTransformation()
            {
                Name = DefaultTransformationFactory.TrimEndTransformationName,
                Parameters = new Dictionary<string, string>()
                {
                    {
                        "MaxLength",
                        "500"
                    },
                },
            },
        };

        private static IServiceProvider _container = new ContainerBuilder()
            .RegisterModule(new HttpClientModule())
            .RegisterModule(new AzureKeyVaultSecretStoreModule())
            .RegisterModule(new LuisDataExtractorModule())
            .RegisterModule(new DataTransformationFactoryModule())
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
        /// Extracts relevant information from a transcription.
        /// </summary>
        /// <param name="req">A request object who's request body respects the <see cref="Microsoft.AdvocacyPlatform.Contracts.ExtractInfoRequest"/> schema.</param>
        /// <param name="log">Trace logger instance.</param>
        /// <param name="context">Execution context to determine function app directory.</param>
        /// <returns>
        /// An OkObjectResult on successful executions, and a BadRequestObjectResult on unsuccessful executions
        /// with the result body respecting the <see cref="Microsoft.AdvocacyPlatform.Contracts.ExtractInfoResponse"/> schema.
        /// </returns>
        [FunctionName("ExtractInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            ExtractInfoRequest request = null;
            ExtractInfoResponse response = new ExtractInfoResponse();

            bool isBadRequest = false;

            try
            {
                log.LogInformation("Reading configuration...");
                IConfigurationRoot config = FunctionHelper.GetConfiguration(context);

                log.LogInformation("Attempting to read request body...");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                request = FunctionRequestBase.Parse<ExtractInfoRequest>(requestBody);

                string transformedTranscription = HandleTransformations(request, log);

                // Set defaults
                if (request.MinDateTime == null)
                {
                    request.MinDateTime = new DateTime(1900, 1, 1);
                }

                response.CallSid = request.CallSid;

                log.LogInformation("Attempting to pull secret for api key...");
                ISecretStore secretProvider = Container.GetService<ISecretStore>();
                Secret apiKey = await secretProvider.GetSecretAsync(config[LuisSubscriptionKeySecretNameAppSettingName], config[AuthorityAppSettingName]);

                log.LogInformation($"ExtractInfo received text: '{transformedTranscription}'.");

                IDataExtractor dataExtractor = Container.GetService<IDataExtractor>();
                IHttpClientWrapper httpClient = Container.GetService<IHttpClientWrapper>();

                ConfigureDataExtractor(dataExtractor, apiKey, config, log);

                log.LogInformation("Attempting to extract information...");
                response.Data = await dataExtractor.ExtractAsync(transformedTranscription, log);

                if (response.Data != null)
                {
                    response.Data.Transcription = request.Text;

                    if (!ValidateDate(request.MinDateTime, response.Data.Date))
                    {
                        log.LogWarning($"Extracted date rejected! ('{response.Data.Date.FullDate}' < '{request.MinDateTime}')");
                        response.Flags.Add(ExtractInfoFlag.DateRejected);
                    }
                }

                log.LogInformation($"Date = {(response.Data.Date != null ? "yes" : "no")}, Location = {(response.Data.Location != null ? "yes" : "no")}, Person = {(response.Data.Person != null ? "yes" : "no")}");

                Tuple<int, string> responseStatus = GetResponseStatus(response.Data);

                response.StatusCode = responseStatus.Item1;
                response.StatusDesc = responseStatus.Item2;

                return FunctionHelper.ActionResultFactory<ExtractInfoResponse>(false, response);
            }
            catch (MalformedRequestBodyException<ExtractInfoErrorCode> ex)
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
            catch (DataExtractorException ex)
            {
                log.LogError($"Data extractor generic exception: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)ExtractInfoErrorCode.DataExtractorGenericFailure;
                response.ErrorDetails = ExtractInfoErrorMessage.DataExtractorGenericFailureMessage;
            }
            catch (Exception ex)
            {
                isBadRequest = true;

                log.LogError($"Exception encountered: {ex.Message}");
                log.LogError(ex.StackTrace);

                response.ErrorCode = (int)CommonErrorCode.BadRequest;
            }

            response.HasError = true;

            return FunctionHelper.ActionResultFactory<ExtractInfoResponse>(isBadRequest, response);
        }

        private static bool ValidateDate(DateTime? minDateTime, DateInfo dateInfo)
        {
            if (dateInfo != null)
            {
                if (dateInfo.FullDate < minDateTime)
                {
                    dateInfo.FullDate = null;
                    dateInfo.Year = 0;
                    dateInfo.Month = 0;
                    dateInfo.Day = 0;
                    dateInfo.Hour = 0;
                    dateInfo.Minute = 0;

                    return false;
                }
            }

            return true;
        }

        private static string HandleTransformations(ExtractInfoRequest request, ILogger log)
        {
            StringBuilder transformedText = new StringBuilder(request.Text);

            // TODO: Temporarily hard code some transformations if none were sent
            if (request.Transformations == null ||
                request.Transformations.Count == 0)
            {
                request.Transformations = _defaultTransformations;
            }

            if (request.Transformations != null &&
                request.Transformations.Count > 0)
            {
                IDataTransformationFactory factory = Container.GetService<IDataTransformationFactory>();

                foreach (DataTransformation transformation in request.Transformations)
                {
                    IDataTransformation transformationOp = factory.Create(transformation.Name);

                    if (transformationOp != null)
                    {
                        transformedText = transformationOp.Transform(transformedText, transformation.Parameters, log);
                    }
                }
            }

            return transformedText.ToString();
        }

        private static Tuple<int, string> GetResponseStatus(TranscriptionData data)
        {
            if (data.Date == null
                || data.Location == null
                || data.Person == null)
            {
                return new Tuple<int, string>((int)ExtractInfoStatusCode.MissingEntities, Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities));
            }

            return new Tuple<int, string>((int)CommonStatusCode.Ok, Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok));
        }

        /// <summary>
        /// Configures the data extractor implementation returned from the dependency injection container based on the it's type.
        /// </summary>
        /// <param name="dataExtractor">The data extractor implementation.</param>
        /// <param name="apiKey">The api key for accessing the service wrapped by the data extractor implementation.</param>
        /// <param name="config">The function configuration.</param>
        /// <param name="log">Trace logger instance.</param>
        private static void ConfigureDataExtractor(IDataExtractor dataExtractor, Secret apiKey, IConfiguration config, ILogger log)
        {
            if (dataExtractor is INlpDataExtractor)
            {
                log.LogInformation("Attempting to read NLP configuration...");
                LuisConfiguration luisConfig = JsonConvert.DeserializeObject<LuisConfiguration>(config[LuisConfigurationAppSettingName]);

                IHttpClientWrapper httpClient = Container.GetService<IHttpClientWrapper>();

                NlpDataExtractorConfiguration nlpConfig = new NlpDataExtractorConfiguration()
                {
                    NlpEndpoint = config[LuisEndpointAppSettingName],
                    NlpSubscriptionKey = apiKey,
                    DateTimeEntityName = luisConfig.DateTimeEntityName,
                    DateEntityName = luisConfig.DateEntityName,
                    TimeEntityName = luisConfig.TimeEntityName,
                    PersonEntityName = luisConfig.PersonEntityName,
                    LocationEntityName = luisConfig.LocationEntityName,
                    CityEntityName = luisConfig.CityEntityName,
                    StateEntityName = luisConfig.StateEntityName,
                    ZipcodeEntityName = luisConfig.ZipcodeEntityName,
                };

                nlpConfig.PersonIntentTypeMap = new Dictionary<string, string>()
                {
                    { "CourtHearingNameEntity", "Judge" },
                    { "CaseDecisionNameEntity", "Judge" },
                    { "None", "Unknown" },
                };

                ((INlpDataExtractor)dataExtractor).Initialize(nlpConfig, httpClient);
            }
        }
    }
}
