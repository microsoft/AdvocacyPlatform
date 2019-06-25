// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Clients;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.AdvocacyPlatform.Functions.Module;
    using Microsoft.AdvocacyPlatform.Functions.Tests.Mocks;
    using Microsoft.AdvocacyPlatform.Functions.Tests.Mocks.Client;
    using Microsoft.AdvocacyPlatform.Functions.Tests.Mocks.Module;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    /// <summary>
    /// Tests for ExtractInfo.
    /// </summary>
    [TestClass]
    public class ExtractInfoTests
    {
        /// <summary>
        /// Application setting name for the LUIS endpoint.
        /// </summary>
        public const string LuisEndpointAppSettingName = "luisEndpoint";

        /// <summary>
        /// Application setting name for the LUIS subscription key secret name.
        /// </summary>
        public const string LuisSubscriptionKeySecretNameAppSettingName = "luisSubscriptionKeySecretName";

        /// <summary>
        /// The expected LUIS subscription key.
        /// </summary>
        public const string ExpectedLuisSubscriptionKey = "mock-luis-subscription-key";

        /// <summary>
        /// Application setting name for the LUIS configuration.
        /// </summary>
        public const string LuisConfigurationAppSettingName = "luisConfiguration";

        /// <summary>
        /// Path to composite LUIS response template with DateTime entity.
        /// </summary>
        public const string LuisDateTimeWithCompositeResponseTemplatePath = @".\Templates\Response\LUIS\LuisDateTimeWithCompositeResponse.json";

        /// <summary>
        /// Path to composite LUIS response template with multiple DateTime entities.
        /// </summary>
        public const string LuisMultipleDateTimeWithCompositeResponseTemplatePath = @".\Templates\Response\LUIS\LuisMultipleDateTimeWithCompositeResponse.json";

        /// <summary>
        /// Path to  LUIS response template with no top scoring entity.
        /// </summary>
        public const string LuisNoTopScoringIntentResponseTemplatePath = @".\Templates\Response\LUIS\LuisNoTopScoringIntentResponse.json";

        /// <summary>
        /// Path to LUIS response template with no entities.
        /// </summary>
        public const string LuisNoEntitiesResponseTemplatePath = @".\Templates\Response\LUIS\LuisNoEntitiesResponse.json";

        /// <summary>
        /// Path to LUIS response template with a null entity.
        /// </summary>
        public const string LuisNullEntitiesResponseTemplatePath = @".\Templates\Response\LUIS\LuisNullEntitiesResponse.json";

        /// <summary>
        /// Path to composite LUIS response template with no DateTime entity.
        /// </summary>
        public const string LuisNoDateTimeWithCompositeResponseTemplatePath = @".\Templates\Response\LUIS\LuisNoDateTimeWithCompositeResponse.json";

        /// <summary>
        /// Path to composite LUIS response template with DateTime but no location entity.
        /// </summary>
        public const string LuisDateTimeNoLocationWithCompositeResponseTemplatePath = @".\Templates\Response\LUIS\LuisDateTimeNoLocationWithCompositeResponse.json";

        /// <summary>
        /// Path to composite LUIS response template.
        /// </summary>
        public const string LuisNoCompositeResponseTemplatePath = @".\Templates\Response\LUIS\LuisNoCompositeResponse.json";

        /// <summary>
        /// Path to composite LUIS response template with null composite entities.
        /// </summary>
        public const string LuisNullCompositeResponseTemplatePath = @".\Templates\Response\LUIS\LuisNullCompositeResponse.json";

        /// <summary>
        /// Path to composite LUIS response template with no person entity.
        /// </summary>
        public const string LuisNoPersonResponseTemplatePath = @".\Templates\Response\LUIS\LuisNoPersonResponse.json";

        /// <summary>
        /// Path to composite LUIS response template with additional entities.
        /// </summary>
        public const string LuisDateTimeWithAdditionalEntityResponseTemplatePath = @".\Templates\Response\LUIS\LuisDateTimeWithAdditionalEntityResponse.json";

        /// <summary>
        /// Path to composite LUIS response template with DateTime, location, and additional entities.
        /// </summary>
        public const string LuisDateTimeLocationWithAdditionalEntityResponseTemplatePath = @".\Templates\Response\LUIS\LuisDateTimeLocationWithAdditionalEntityResponse.json";

        /// <summary>
        /// Path to composite LUIS response template with DateTime, location, and duplicate additional entities.
        /// </summary>
        public const string LuisDateTimeLocationWithDuplicateAdditionalEntityResponseTemplatePath = @".\Templates\Response\LUIS\LuisDateTimeLocationWithDuplicateAdditionalEntityResponse.json";

        /// <summary>
        /// Path to composite LUIS response template with DateTime, location, and triplicate additional entities.
        /// </summary>
        public const string LuisDateTimeLocationWithTriplicateAdditionalEntityResponseTemplatePath = @".\Templates\Response\LUIS\LuisDateTimeLocationWithTriplicateAdditionalEntityResponse.json";

        /// <summary>
        /// Path to composite LUIS response template with DateTime, location, composite, and additional entities.
        /// </summary>
        public const string LuisDateTimeLocationWithAdditionalEntityFullCompositeResponseTemplatePath = @".\Templates\Response\LUIS\LuisDateTimeLocationWithAdditionalEntityFullCompositeResponse.json";

        /// <summary>
        /// Path to LUIS template for entities for use in composite templates.
        /// </summary>
        public const string LuisDateTimeEntityCompositeTemplatePath = @".\Templates\Response\LUIS\Composite\LuisDateTimeEntityCompositeTemplate.json";

        /// <summary>
        /// Name of the key to replace in composite LUIS response template for datetime entities.
        /// </summary>
        public const string LuisDateTimeEntitiesCompositeKey = "dateTimeEntitiesComposite";

        private static MockLogger _log = new MockLogger();
        private IConfigurationRoot _config;
        private LuisConfiguration _luisConfig;

        /// <summary>
        /// Gets the mock logger instance.
        /// </summary>
        public static MockLogger Log => _log;

        /// <summary>
        /// Gets the expected request URI.
        /// </summary>
        public string RequestUri => $"{_config[ExtractInfoHttpTrigger.LuisEndpointAppSettingName]}&subscription-key={ExpectedLuisSubscriptionKey}";

        /// <summary>
        /// Builds the dependency injection container.
        /// </summary>
        /// <param name="modules">A list of modules to add to the container.</param>
        public void InitializeServiceProvider(IEnumerable<Module> modules)
        {
            IContainerBuilder containerBuilder = new ContainerBuilder();

            foreach (Module module in modules)
            {
                containerBuilder = containerBuilder.RegisterModule(module);
            }

            ExtractInfoHttpTrigger.Container = containerBuilder.Build();
        }

        /// <summary>
        /// Initializes test with no DataExtractor mock.
        /// </summary>
        public void InitializeWithNoDataExtractorMock()
        {
            InitializeServiceProvider(new List<Module>()
            {
                new HttpClientMockModule(),
                new AzureKeyVaultSecretStoreMockModule(),
                new LuisDataExtractorModule(),
                new DataTransformationFactoryModule(),
            });

            Assert.IsNotNull(ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(ExtractInfoHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(ExtractInfoHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(ExtractInfoHttpTrigger.Container.GetService<IDataExtractor>(), "Could not obtain IDataExtractor instance from DI container!");
            Assert.IsInstanceOfType(ExtractInfoHttpTrigger.Container.GetService<IDataExtractor>(), typeof(LuisDataExtractor), "Type of IDataExtractor should be LuisDataExtractor!");
        }

        /// <summary>
        /// Initializes test.
        /// </summary>
        [TestInitialize]
        public void InitializeTests()
        {
            _config = TestHelper.GetConfiguration();
            _luisConfig = JsonConvert.DeserializeObject<LuisConfiguration>(_config[ExtractInfoHttpTrigger.LuisConfigurationAppSettingName]);
        }

        /// <summary>
        /// Initializes mock secret store.
        /// </summary>
        public void InitializeSecrets()
        {
            AzureKeyVaultSecretStoreMock secretStore = ExtractInfoHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            Secret expectedLuisSubscriptionKey = new Secret(_config[LuisSubscriptionKeySecretNameAppSettingName], TestHelper.CreateSecureString(ExpectedLuisSubscriptionKey));

            secretStore.RegisterExpectedSecret(expectedLuisSubscriptionKey);
        }

        /// <summary>
        /// Initializes expected secret store exceptions.
        /// </summary>
        public void InitializeSecretsExceptions()
        {
            AzureKeyVaultSecretStoreMock secretStore = ExtractInfoHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            secretStore.RegisterExpectedException(_config[LuisSubscriptionKeySecretNameAppSettingName], new SecretStoreException("TestException", null));
        }

        /// <summary>
        /// Initializes expected DataExtractor exceptions.
        /// </summary>
        public void InitializeDataExtractorExceptions()
        {
            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestException($"{_config[ExtractInfoHttpTrigger.LuisEndpointAppSettingName]}&subscription-key={ExpectedLuisSubscriptionKey}", new HttpRequestException("Request exception test!"));
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTime = "2021-12-13 13:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            DateInfo expectedDateInfo = new DateInfo(2021, 12, 13, 13, 0);

            expectedDateInfo.FullDate = new System.DateTime(2021, 12, 13, 13, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeWithCompositeResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedPersonName", expectedPersonName },
                { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with no trim data transformation runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerTrimEndNoTrimSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTime = "2021-12-13 13:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            DateInfo expectedDateInfo = new DateInfo(2021, 12, 13, 13, 0);

            expectedDateInfo.FullDate = new System.DateTime(2021, 12, 13, 13, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeWithCompositeResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedPersonName", expectedPersonName },
                { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with TrimEnd data transformation runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerTrimEndTrimmedSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            int maxLength = 500;
            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedTrimmedText = expectedTransformedText.Substring(0, maxLength);
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTime = "2021-12-13 13:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            DateInfo expectedDateInfo = new DateInfo(2021, 12, 13, 13, 0);

            expectedDateInfo.FullDate = new System.DateTime(2021, 12, 13, 13, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTrimmedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeWithCompositeResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedPersonName", expectedPersonName },
                { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTrimmedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with missing top scoring intent runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerResponseMissingTopScoringIntentSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Unknown";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            DateInfo expectedDateInfo = new DateInfo(2021, 12, 13, 13, 0);

            expectedDateInfo.FullDate = new System.DateTime(2021, 12, 13, 13, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisNoTopScoringIntentResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedPersonName", expectedPersonName },
                { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNull(response.Data.Intent);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with no entities runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerResponseNoEntitiesSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            DateInfo expectedDateInfo = new DateInfo(2021, 12, 13, 13, 0);

            expectedDateInfo.FullDate = new System.DateTime(2021, 12, 13, 13, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisNoEntitiesResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(0, response.Data.Dates.Count);

            Assert.IsNull(response.Data.Location);
            Assert.IsNull(response.Data.Person);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with null entities runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerResponseNullEntitiesSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            DateInfo expectedDateInfo = new DateInfo(2021, 12, 13, 13, 0);

            expectedDateInfo.FullDate = new System.DateTime(2021, 12, 13, 13, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisNullEntitiesResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(0, response.Data.Dates.Count);

            Assert.IsNull(response.Data.Location);
            Assert.IsNull(response.Data.Person);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with date-only entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerDateTimeOnlyDateSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTime = "2021-12-13";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            DateInfo expectedDateInfo = new DateInfo(2021, 12, 13, 0, 0);

            expectedDateInfo.FullDate = new System.DateTime(2021, 12, 13, 0, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeWithCompositeResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedPersonName", expectedPersonName },
                { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with date-only entity below min DateTime value threshold runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerDateTimeOnlyDateBelowThresholdDateRejectedSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is January, 13th 1890 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is January, 13th 1890 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTime = "1890-01-13";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeWithCompositeResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedPersonName", expectedPersonName },
                { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.IsNull(firstDateInfo.FullDate, "The full date in the DateInfo object should be null!");
            Assert.AreEqual(0, firstDateInfo.Year, "The year value in the DateInfo object should be 0!");
            Assert.AreEqual(0, firstDateInfo.Month, "The month value in the DateInfo object should be 0!");
            Assert.AreEqual(0, firstDateInfo.Day, "The day value in the DateInfo object should be 0!");
            Assert.AreEqual(0, firstDateInfo.Hour, "The hour value in the DateInfo object should be 0!");
            Assert.AreEqual(0, firstDateInfo.Minute, "The minute value in the DateInfo object should be 0!");

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with time-only entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerDateTimeOnlyTimeBelowThresholdSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTime = "13:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            DateInfo expectedDateInfo = new DateInfo(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, 13, 0);

            expectedDateInfo.FullDate = new System.DateTime(expectedDateInfo.Year, expectedDateInfo.Month, expectedDateInfo.Day, 13, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeWithCompositeResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.TimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedPersonName", expectedPersonName },
                { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.IsNull(firstDateInfo.FullDate, "The full date in the DateInfo object should be null!");
            Assert.AreEqual(0, firstDateInfo.Year, "The year value in the DateInfo object should be 0!");
            Assert.AreEqual(0, firstDateInfo.Month, "The month value in the DateInfo object should be 0!");
            Assert.AreEqual(0, firstDateInfo.Day, "The day value in the DateInfo object should be 0!");
            Assert.AreEqual(0, firstDateInfo.Hour, "The hour value in the DateInfo object should be 0!");
            Assert.AreEqual(0, firstDateInfo.Minute, "The minute value in the DateInfo object should be 0!");

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with no datetime entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerNoDateTime()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisNoDateTimeWithCompositeResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedPersonName", expectedPersonName },
                { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(0, response.Data.Dates.Count);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with no location entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerNoLocationEntitySuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTime = "2021-12-13 13:00:00";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            DateInfo expectedDateInfo = new DateInfo(2021, 12, 13, 13, 0);

            expectedDateInfo.FullDate = new System.DateTime(2021, 12, 13, 13, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeNoLocationWithCompositeResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedPersonName", expectedPersonName },
                { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNull(response.Data.Location);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with no composite entities runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerNoCompositeEntitiesSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTime = "2021-12-13 13:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";

            DateInfo expectedDateInfo = new DateInfo(2021, 12, 13, 13, 0);

            expectedDateInfo.FullDate = new System.DateTime(2021, 12, 13, 13, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisNoCompositeResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedPersonName", expectedPersonName },
                { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.IsNull(response.Data.Location.City);
            Assert.IsNull(response.Data.Location.State);
            Assert.IsNull(response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with null composite entities runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerNullCompositeEntitiesSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTime = "2021-12-13 13:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";

            DateInfo expectedDateInfo = new DateInfo(2021, 12, 13, 13, 0);

            expectedDateInfo.FullDate = new System.DateTime(2021, 12, 13, 13, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisNoCompositeResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedPersonName", expectedPersonName },
                { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.IsNull(response.Data.Location.City);
            Assert.IsNull(response.Data.Location.State);
            Assert.IsNull(response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with no person entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerNoPersonEntitySuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTime = "2021-12-13 13:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            DateInfo expectedDateInfo = new DateInfo(2021, 12, 13, 13, 0);

            expectedDateInfo.FullDate = new System.DateTime(2021, 12, 13, 13, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisNoPersonResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNull(response.Data.Person);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with missing callSid in request body runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerMissingCallSid()
        {
            string expectedText = "your next Master hearing date January 19th 2018 at 3 p.m. for Gymboree";

            HttpRequest request = CreateHttpPostRequest(null, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)badRequestResult.Value;

            Assert.IsNull(response.CallSid);
            Assert.IsNull(response.Data);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)ExtractInfoErrorCode.RequestMissingCallSid, response.ErrorCode);
            Assert.AreEqual($"You must specify a value for the key '{ExtractInfoRequest.CallSidKeyName}' in the request body!", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with missing text in request body runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerMissingText()
        {
            string expectedCallSid = "CA10000000000000000000000000000300";

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, null);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)badRequestResult.Value;

            Assert.IsNull(response.CallSid);
            Assert.IsNull(response.Data);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)ExtractInfoErrorCode.RequestMissingText, response.ErrorCode);
            Assert.AreEqual($"You must specify a value for the key '{ExtractInfoRequest.TextKeyName}' in the request body!", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with administrative order entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerAdministrativeOrder()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system will your next the immigration judge issued an administrative decision on your case at on August 1st, 2017 for your next hearing date press one. For case processing information press 2. Poor decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options press zero.";
            string expectedTransformedText = "The alien registration number You entered the system will your next the immigration judge issued an administrative decision on your case at on August 1st, 2017 for your next hearing date press one For case processing information press 2 Poor decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options press zero";
            string expectedIntent = "CaseDecisionNameEntity";
            string expectedDateTime = "2017-08-01";
            string expectedAdditionalEntityType = "administrativeOrder";
            string expectedAdditionalEntity = "administrative decision";

            DateInfo expectedDateInfo = new DateInfo(2017, 8, 1, 0, 0);

            expectedDateInfo.FullDate = new System.DateTime(2017, 8, 1, 0, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeWithAdditionalEntityResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedAdditionalEntity", expectedAdditionalEntity },
                { "expectedAdditionalEntityType", expectedAdditionalEntityType },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNull(response.Data.Location);
            Assert.IsNull(response.Data.Person);

            Assert.IsNotNull(response.Data.AdditionalData);
            Assert.AreEqual(1, response.Data.AdditionalData.Count);
            Assert.IsTrue(response.Data.AdditionalData.ContainsKey(expectedAdditionalEntityType));
            Assert.AreEqual(expectedAdditionalEntity, response.Data.AdditionalData[expectedAdditionalEntityType]);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with granted relief entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerGrantedRelief()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system will your next the immigration judge issued an administrative decision on your case at on August 1st, 2017 for your next hearing date press one. For case processing information press 2. Poor decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options press zero.";
            string expectedTransformedText = "The alien registration number You entered the system will your next the immigration judge issued an administrative decision on your case at on August 1st, 2017 for your next hearing date press one For case processing information press 2 Poor decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options press zero";
            string expectedIntent = "CaseDecisionNameEntity";
            string expectedDateTime = "2016-01-19";
            string expectedLocation = "970 broad st room twelve hundred newark nj 07 ? 02 .";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedAdditionalEntityType = "grantedRelief";
            string expectedAdditionalEntity = "granted relief";

            DateInfo expectedDateInfo = new DateInfo(2016, 01, 19, 0, 0);

            expectedDateInfo.FullDate = new System.DateTime(2016, 01, 19, 0, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeLocationWithAdditionalEntityResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedAdditionalEntity", expectedAdditionalEntity },
                { "expectedAdditionalEntityType", expectedAdditionalEntityType },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.IsNull(response.Data.Location.Zipcode);

            Assert.IsNull(response.Data.Person);

            Assert.IsNotNull(response.Data.AdditionalData);
            Assert.AreEqual(1, response.Data.AdditionalData.Count);
            Assert.IsTrue(response.Data.AdditionalData.ContainsKey(expectedAdditionalEntityType));
            Assert.AreEqual(expectedAdditionalEntity, response.Data.AdditionalData[expectedAdditionalEntityType]);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with duplicate entities runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerDuplicateExtraEntity()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system will your next the immigration judge issued an administrative decision on your case at on August 1st, 2017 for your next hearing date press one. For case processing information press 2. Poor decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options press zero.";
            string expectedTransformedText = "The alien registration number You entered the system will your next the immigration judge issued an administrative decision on your case at on August 1st, 2017 for your next hearing date press one For case processing information press 2 Poor decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options press zero";
            string expectedIntent = "CaseDecisionNameEntity";
            string expectedDateTime = "2016-01-19";
            string expectedLocation = "970 broad st room twelve hundred newark nj 07 ? 02 .";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedAdditionalEntityType = "grantedRelief";
            string expectedAdditionalEntity = "granted relief";

            DateInfo expectedDateInfo = new DateInfo(2016, 01, 19, 0, 0);

            expectedDateInfo.FullDate = new System.DateTime(2016, 01, 19, 0, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeLocationWithDuplicateAdditionalEntityResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedAdditionalEntity", expectedAdditionalEntity },
                { "expectedAdditionalEntityType", expectedAdditionalEntityType },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.IsNull(response.Data.Location.Zipcode);

            Assert.IsNull(response.Data.Person);

            Assert.IsNotNull(response.Data.AdditionalData);
            Assert.AreEqual(2, response.Data.AdditionalData.Count);
            Assert.IsTrue(response.Data.AdditionalData.ContainsKey(expectedAdditionalEntityType));
            Assert.AreEqual(expectedAdditionalEntity, response.Data.AdditionalData[expectedAdditionalEntityType]);
            Assert.IsTrue(response.Data.AdditionalData.ContainsKey($"{expectedAdditionalEntityType}-2"));
            Assert.AreEqual(expectedAdditionalEntity, response.Data.AdditionalData[$"{expectedAdditionalEntityType}-2"]);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with triplicate entities runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerTriplicateExtraEntity()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system will your next the immigration judge issued an administrative decision on your case at on August 1st, 2017 for your next hearing date press one. For case processing information press 2. Poor decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options press zero.";
            string expectedTransformedText = "The alien registration number You entered the system will your next the immigration judge issued an administrative decision on your case at on August 1st, 2017 for your next hearing date press one For case processing information press 2 Poor decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options press zero";
            string expectedIntent = "CaseDecisionNameEntity";
            string expectedDateTime = "2016-01-19";
            string expectedLocation = "970 broad st room twelve hundred newark nj 07 ? 02 .";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedAdditionalEntityType = "grantedRelief";
            string expectedAdditionalEntity = "granted relief";

            DateInfo expectedDateInfo = new DateInfo(2016, 01, 19, 0, 0);

            expectedDateInfo.FullDate = new System.DateTime(2016, 01, 19, 0, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeLocationWithTriplicateAdditionalEntityResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedAdditionalEntity", expectedAdditionalEntity },
                { "expectedAdditionalEntityType", expectedAdditionalEntityType },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.IsNull(response.Data.Location.Zipcode);

            Assert.IsNull(response.Data.Person);

            Assert.IsNotNull(response.Data.AdditionalData);
            Assert.AreEqual(3, response.Data.AdditionalData.Count);
            Assert.IsTrue(response.Data.AdditionalData.ContainsKey(expectedAdditionalEntityType));
            Assert.AreEqual(expectedAdditionalEntity, response.Data.AdditionalData[expectedAdditionalEntityType]);
            Assert.IsTrue(response.Data.AdditionalData.ContainsKey($"{expectedAdditionalEntityType}-2"));
            Assert.AreEqual(expectedAdditionalEntity, response.Data.AdditionalData[$"{expectedAdditionalEntityType}-2"]);
            Assert.IsTrue(response.Data.AdditionalData.ContainsKey($"{expectedAdditionalEntityType}-3"));
            Assert.AreEqual(expectedAdditionalEntity, response.Data.AdditionalData[$"{expectedAdditionalEntityType}-3"]);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with removal order entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerRemovalOrder()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. The immigration judge ordered removal on your case at Avenida ashford, San Juan Puerto Rico 00919-9089. On august 13 2018 for your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal Information Press, 4 for filing information press";
            string expectedTransformedText = "The alien registration number You entered the system for your next The immigration judge ordered removal on your case at Avenida ashford, San Juan Puerto Rico 00919-9089 On august 13 2018 for your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal Information Press, 4 for filing information press";
            string expectedIntent = "CaseDecisionNameEntity";
            string expectedDateTime = "2018-08-13";
            string expectedLocation = "avenida ashford , san juan puerto rico 00919 - 9089";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";
            string expectedAdditionalEntityType = "removalOrder";
            string expectedAdditionalEntity = "ordered removal";

            DateInfo expectedDateInfo = new DateInfo(2018, 8, 13, 0, 0);

            expectedDateInfo.FullDate = new System.DateTime(2018, 8, 13, 0, 0, 0);

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.Load(LuisDateTimeLocationWithAdditionalEntityFullCompositeResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedQuery", expectedText },
                { "expectedIntent", expectedIntent },
                { "expectedDateTimeEntityName", _luisConfig.DateTimeEntityName },
                { "expectedDateTime", expectedDateTime },
                { "expectedAdditionalEntity", expectedAdditionalEntity },
                { "expectedAdditionalEntityType", expectedAdditionalEntityType },
                { "expectedLocation", expectedLocation },
                { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                { "expectedCityEntityName", _luisConfig.CityEntityName },
                { "expectedCompositeCity", expectedCompositeCity },
                { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                { "expectedCompositeZipcode", expectedCompositeZipcode },
                { "expectedStateEntityName", _luisConfig.StateEntityName },
                { "expectedCompositeState", expectedCompositeState },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(1, response.Data.Dates.Count);

            DateInfo firstDateInfo = response.Data.Dates.First();

            Assert.AreEqual(expectedDateInfo.FullDate, firstDateInfo.FullDate);
            Assert.AreEqual(expectedDateInfo.Year, firstDateInfo.Year);
            Assert.AreEqual(expectedDateInfo.Month, firstDateInfo.Month);
            Assert.AreEqual(expectedDateInfo.Day, firstDateInfo.Day);
            Assert.AreEqual(expectedDateInfo.Hour, firstDateInfo.Hour);
            Assert.AreEqual(expectedDateInfo.Minute, firstDateInfo.Minute);

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNull(response.Data.Person);

            Assert.IsNotNull(response.Data.AdditionalData);
            Assert.AreEqual(1, response.Data.AdditionalData.Count);
            Assert.IsTrue(response.Data.AdditionalData.ContainsKey(expectedAdditionalEntityType));
            Assert.AreEqual(expectedAdditionalEntity, response.Data.AdditionalData[expectedAdditionalEntityType]);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation handles secret store exceptions correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerSecretStoreException()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecretsExceptions();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNull(response.Data);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.SecretStoreGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.SecretStoreGenericFailureMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation handles data extractor exceptions correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerDataExtractorException()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();
            InitializeDataExtractorExceptions();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should be empty!");
            Assert.IsNull(response.Data);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)ExtractInfoErrorCode.DataExtractorGenericFailure, response.ErrorCode);
            Assert.AreEqual(ExtractInfoErrorMessage.DataExtractorGenericFailureMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with multiple date entities runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerMultipleDateTimesOnlyDateSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTimeEntity1 = "february 2nd 2018";
            string expectedDateTimeEntity2 = "november 27th 2018";
            string expectedDateTime1 = "2018-02-02";
            string expectedDateTime2 = "2018-11-27";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 0, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2),
                },
                new DateInfo(2018, 11, 27, 0, 0)
                {
                    FullDate = new System.DateTime(2018, 11, 27),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity1 },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "0" },
                        { "expectedDateTime", expectedDateTime1 },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity2 },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "0" },
                        { "expectedDateTime", expectedDateTime2 },
                        { "expectedResolution", "date" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with one date and one time entities runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger1DateAndTimeSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateEntity = "february 2nd 2018";
            string expectedTimeEntity = "one pm";
            string expectedDate = "2018-02-02";
            string expectedTime = "1:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "0" },
                        { "expectedDateTime", expectedDate },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "0" },
                        { "expectedDateTime", expectedTime },
                        { "expectedResolution", "time" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(0, response.Flags.Count, "Response flags should not be set!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with two date and two time entities runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger2DateAndTimeSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateEntity = "february 2nd 2018";
            string expectedTimeEntity = "one pm";
            string expectedDateEntity2 = "january 12nd 2019";
            string expectedTimeEntity2 = "four pm";
            string expectedDate = "2018-02-02";
            string expectedTime = "13:00:00";
            string expectedDate2 = "2019-01-12";
            string expectedTime2 = "16:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 13, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 13, 0, 0),
                },
                new DateInfo(2019, 1, 12, 16, 0)
                {
                    FullDate = new System.DateTime(2019, 1, 12, 16, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "50" },
                        { "expectedDateTime", expectedDate },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity },
                        { "expectedStartIndex", "150" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedTime },
                        { "expectedResolution", "time" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity2 },
                        { "expectedStartIndex", "250" },
                        { "expectedEndIndex", "300" },
                        { "expectedDateTime", expectedDate2 },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity2 },
                        { "expectedStartIndex", "350" },
                        { "expectedEndIndex", "400" },
                        { "expectedDateTime", expectedTime2 },
                        { "expectedResolution", "time" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a date, time, and date entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger1DateAndTimeAdd1DateSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateEntity = "february 2nd 2018";
            string expectedTimeEntity = "one pm";
            string expectedDateEntity2 = "december 4th 2018";
            string expectedDate = "2018-02-02";
            string expectedTime = "1:00:00";
            string expectedDate2 = "2018-12-04";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
                new DateInfo(2018, 12, 4, 0, 0)
                {
                    FullDate = new System.DateTime(2018, 12, 4, 0, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "100" },
                        { "expectedDateTime", expectedDate },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity },
                        { "expectedStartIndex", "150" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedTime },
                        { "expectedResolution", "time" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity2 },
                        { "expectedStartIndex", "350" },
                        { "expectedEndIndex", "400" },
                        { "expectedDateTime", expectedDate2 },
                        { "expectedResolution", "date" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a date, time, date, and date entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger1DateAndTimeAdd2DateSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateEntity = "february 2nd 2018";
            string expectedTimeEntity = "one pm";
            string expectedDateEntity2 = "december 4th 2018";
            string expectedDateEntity3 = "feb 14th 2019";
            string expectedDate = "2018-02-02";
            string expectedTime = "1:00:00";
            string expectedDate2 = "2018-12-04";
            string expectedDate3 = "2019-02-14";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
                new DateInfo(2018, 12, 4, 0, 0)
                {
                    FullDate = new System.DateTime(2018, 12, 4, 0, 0, 0),
                },
                new DateInfo(2019, 2, 14, 0, 0)
                {
                    FullDate = new System.DateTime(2019, 2, 14, 0, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "100" },
                        { "expectedDateTime", expectedDate },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity },
                        { "expectedStartIndex", "150" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedTime },
                        { "expectedResolution", "time" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity2 },
                        { "expectedStartIndex", "350" },
                        { "expectedEndIndex", "400" },
                        { "expectedDateTime", expectedDate2 },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity3 },
                        { "expectedStartIndex", "500" },
                        { "expectedEndIndex", "510" },
                        { "expectedDateTime", expectedDate3 },
                        { "expectedResolution", "date" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a datetime and date entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger1DateTimeAdd1DateSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTimeEntity = "february 2nd 2018 1 am";
            string expectedDateEntity = "december 4th 2018";
            string expectedDateTime = "2018-02-02 1:00:00";
            string expectedDate = "2018-12-04";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
                new DateInfo(2018, 12, 4, 0, 0)
                {
                    FullDate = new System.DateTime(2018, 12, 4, 0, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "100" },
                        { "expectedDateTime", expectedDateTime },
                        { "expectedResolution", "datetime" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity },
                        { "expectedStartIndex", "50" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedDate },
                        { "expectedResolution", "date" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a datetime, date, and time entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger1DateTimeAdd1DateAndTimeSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTimeEntity = "february 2nd 2018 1 am";
            string expectedDateEntity = "december 4th 2018";
            string expectedTimeEntity = "1 pm";
            string expectedDateTime = "2018-02-02 1:00:00";
            string expectedDate = "2018-12-04";
            string expectedTime = "13:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
                new DateInfo(2018, 12, 4, 13, 0)
                {
                    FullDate = new System.DateTime(2018, 12, 4, 13, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "100" },
                        { "expectedDateTime", expectedDateTime },
                        { "expectedResolution", "datetime" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity },
                        { "expectedStartIndex", "50" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedDate },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity },
                        { "expectedStartIndex", "250" },
                        { "expectedEndIndex", "300" },
                        { "expectedDateTime", expectedTime },
                        { "expectedResolution", "time" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a datetime, date, time, and time entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger1DateTimeAdd1DateAnd2TimeSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTimeEntity = "february 2nd 2018 1 am";
            string expectedDateEntity = "december 4th 2018";
            string expectedTimeEntity = "1 pm";
            string expectedTimeEntity2 = "2 am";
            string expectedDateTime = "2018-02-02 1:00:00";
            string expectedDate = "2018-12-04";
            string expectedTime = "13:00:00";
            string expectedTime2 = "2:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
                new DateInfo(2018, 12, 4, 13, 0)
                {
                    FullDate = new System.DateTime(2018, 12, 4, 13, 0, 0),
                },
                new DateInfo(1, 1, 1, 2, 0)
                {
                    FullDate = new System.DateTime(1, 1, 1, 2, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "100" },
                        { "expectedDateTime", expectedDateTime },
                        { "expectedResolution", "datetime" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity },
                        { "expectedStartIndex", "50" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedDate },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity },
                        { "expectedStartIndex", "250" },
                        { "expectedEndIndex", "300" },
                        { "expectedDateTime", expectedTime },
                        { "expectedResolution", "time" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity2 },
                        { "expectedStartIndex", "350" },
                        { "expectedEndIndex", "400" },
                        { "expectedDateTime", expectedTime2 },
                        { "expectedResolution", "time" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a datetime, date, and date entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger1DateTimeAdd2DateSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTimeEntity = "february 2nd 2018 1 am";
            string expectedDateEntity = "december 4th 2018";
            string expectedDateEntity2 = "feb 16th 2019";
            string expectedDateTime = "2018-02-02 1:00:00";
            string expectedDate = "2018-12-04";
            string expectedDate2 = "2019-02-16";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
                new DateInfo(2018, 12, 4, 0, 0)
                {
                    FullDate = new System.DateTime(2018, 12, 4, 0, 0, 0),
                },
                new DateInfo(2019, 2, 16, 0, 0)
                {
                    FullDate = new System.DateTime(2019, 2, 16, 0, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "100" },
                        { "expectedDateTime", expectedDateTime },
                        { "expectedResolution", "datetime" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity },
                        { "expectedStartIndex", "50" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedDate },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity2 },
                        { "expectedStartIndex", "250" },
                        { "expectedEndIndex", "300" },
                        { "expectedDateTime", expectedDate2 },
                        { "expectedResolution", "date" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a datetime, datetime, time, date, and date entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger2DateTimeAdd2DateSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTimeEntity = "february 2nd 2018 1 am";
            string expectedDateTimeEntity2 = "february 2nd 2018 1 am";
            string expectedDateEntity = "december 4th 2018";
            string expectedDateEntity2 = "june 12th 2019";
            string expectedDateTime = "2018-02-02 1:00:00";
            string expectedDateTime2 = "2019-06-12 16:00:00";
            string expectedDate = "2018-12-04";
            string expectedDate2 = "2019-02-16";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
                new DateInfo(2019, 6, 12, 16, 0)
                {
                    FullDate = new System.DateTime(2019, 6, 12, 16, 0, 0),
                },
                new DateInfo(2018, 12, 4, 0, 0)
                {
                    FullDate = new System.DateTime(2018, 12, 4, 0, 0, 0),
                },
                new DateInfo(2019, 2, 16, 0, 0)
                {
                    FullDate = new System.DateTime(2019, 2, 16, 0, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "100" },
                        { "expectedDateTime", expectedDateTime },
                        { "expectedResolution", "datetime" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity },
                        { "expectedStartIndex", "50" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedDate },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity2 },
                        { "expectedStartIndex", "260" },
                        { "expectedEndIndex", "300" },
                        { "expectedDateTime", expectedDateTime2 },
                        { "expectedResolution", "datetime" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity2 },
                        { "expectedStartIndex", "350" },
                        { "expectedEndIndex", "400" },
                        { "expectedDateTime", expectedDate2 },
                        { "expectedResolution", "date" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a datetime and time entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger1DateTimeAdd1TimeSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTimeEntity = "february 2nd 2018 1 am";
            string expectedTimeEntity = "one pm";
            string expectedDateTime = "2018-02-02 1:00:00";
            string expectedTime = "13:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
                new DateInfo(1, 1, 1, 13, 0)
                {
                    FullDate = new System.DateTime(1, 1, 1, 13, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "100" },
                        { "expectedDateTime", expectedDateTime },
                        { "expectedResolution", "datetime" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity },
                        { "expectedStartIndex", "50" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedTime },
                        { "expectedResolution", "time" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a datetime, time, and time entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger1DateTimeAdd2TimeSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTimeEntity = "february 2nd 2018 1 am";
            string expectedTimeEntity = "3 pm";
            string expectedTimeEntity2 = "two am";
            string expectedDateTime = "2018-02-02 1:00:00";
            string expectedTime = "15:00:00";
            string expectedTime2 = "2:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
                new DateInfo(1, 1, 1, 15, 0)
                {
                    FullDate = new System.DateTime(1, 1, 1, 15, 0, 0),
                },
                new DateInfo(1, 1, 1, 2, 0)
                {
                    FullDate = new System.DateTime(1, 1, 1, 2, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "100" },
                        { "expectedDateTime", expectedDateTime },
                        { "expectedResolution", "datetime" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity },
                        { "expectedStartIndex", "50" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedTime },
                        { "expectedResolution", "time" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity2 },
                        { "expectedStartIndex", "250" },
                        { "expectedEndIndex", "300" },
                        { "expectedDateTime", expectedTime2 },
                        { "expectedResolution", "time" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a date, time, and time entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger1DateAndTimeAdd1TimeSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateEntity = "february 2nd 2018";
            string expectedTimeEntity = "one pm";
            string expectedTimeEntity2 = "two pm";
            string expectedDate = "2018-02-02";
            string expectedTime = "1:00:00";
            string expectedTime2 = "14:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
                new DateInfo(1, 1, 1, 14, 0)
                {
                    FullDate = new System.DateTime(1, 1, 1, 14, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "100" },
                        { "expectedDateTime", expectedDate },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity },
                        { "expectedStartIndex", "150" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedTime },
                        { "expectedResolution", "time" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity2 },
                        { "expectedStartIndex", "350" },
                        { "expectedEndIndex", "400" },
                        { "expectedDateTime", expectedTime2 },
                        { "expectedResolution", "time" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a date, time, time, and time entity runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTrigger1DateAndTimeAdd2TimeSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateEntity = "february 2nd 2018";
            string expectedTimeEntity = "one pm";
            string expectedTimeEntity2 = "two pm";
            string expectedTimeEntity3 = "8 pm";
            string expectedDate = "2018-02-02";
            string expectedTime = "1:00:00";
            string expectedTime2 = "14:00:00";
            string expectedTime3 = "8:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(2018, 2, 2, 1, 0)
                {
                    FullDate = new System.DateTime(2018, 2, 2, 1, 0, 0),
                },
                new DateInfo(1, 1, 1, 14, 0)
                {
                    FullDate = new System.DateTime(1, 1, 1, 14, 0, 0),
                },
                new DateInfo(1, 1, 1, 8, 0)
                {
                    FullDate = new System.DateTime(1, 1, 1, 8, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateEntity },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "100" },
                        { "expectedDateTime", expectedDate },
                        { "expectedResolution", "date" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity },
                        { "expectedStartIndex", "150" },
                        { "expectedEndIndex", "200" },
                        { "expectedDateTime", expectedTime },
                        { "expectedResolution", "time" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity2 },
                        { "expectedStartIndex", "350" },
                        { "expectedEndIndex", "400" },
                        { "expectedDateTime", expectedTime2 },
                        { "expectedResolution", "time" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedTimeEntity3 },
                        { "expectedStartIndex", "450" },
                        { "expectedEndIndex", "500" },
                        { "expectedDateTime", expectedTime3 },
                        { "expectedResolution", "time" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < expectedDateInfos.Count; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation with a multiple time entities runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerMultipleDateTimesOnlyTimeBelowThresholdSuccess()
        {
            InitializeWithNoDataExtractorMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string expectedText = "The alien registration number. You entered the system for your next. Your next individual hearing date is December, 13th 2021 at one PM. Jeremiah Johnson at 100, Montgomery St Suite. Eight hundred San Francisco CA 94105. For your next hearing date press one. For case processing information press 2. 4 decision information press 3. For case appeal information press 4. For filing Information Press, 5 to repeat these options.";
            string expectedTransformedText = "The alien registration number You entered the system for your next Your next individual hearing date is December, 13th 2021 at one PM Jeremiah Johnson at 100, Montgomery St Suite Eight hundred San Francisco CA 94105 For your next hearing date press one For case processing information press 2 4 decision information press 3 For case appeal information press 4 For filing Information Press, 5 to repeat these options";
            string expectedIntent = "CourtHearingNameEntity";
            string expectedDateTimeEntity1 = "twelve am";
            string expectedDateTimeEntity2 = "three pm";
            string expectedDateTime1 = "12:00:00";
            string expectedDateTime2 = "3:00:00";
            string expectedLocation = "100 , montgomery st suite . eight hundred san francisco ca 94105";
            string expectedPersonName = "Jeremiah Johnson";
            string expectedPersonType = "Judge";
            string expectedCompositeCity = "san francisco";
            string expectedCompositeState = "ca";
            string expectedCompositeZipcode = "94105";

            IList<DateInfo> expectedDateInfos = new List<DateInfo>()
            {
                new DateInfo(1, 1, 1, 12, 0)
                {
                    FullDate = new System.DateTime(1, 1, 1, 12, 0, 0),
                },
                new DateInfo(1, 1, 1, 3, 0)
                {
                    FullDate = new System.DateTime(1, 1, 1, 3, 0, 0),
                },
            };

            HttpClientMock httpClient = ExtractInfoHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.RegisterExpectedRequestMessage(RequestUri, $"\"{expectedTransformedText}\"");

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                LuisMultipleDateTimeWithCompositeResponseTemplatePath,
                LuisDateTimeEntitiesCompositeKey,
                LuisDateTimeEntityCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "expectedQuery", expectedText },
                    { "expectedIntent", expectedIntent },
                    { "expectedPersonName", expectedPersonName },
                    { "expectedPersonEntityName", _luisConfig.PersonEntityName },
                    { "expectedLocation", expectedLocation },
                    { "expectedLocationEntityName", _luisConfig.LocationEntityName },
                    { "expectedCityEntityName", _luisConfig.CityEntityName },
                    { "expectedCompositeCity", expectedCompositeCity },
                    { "expectedZipcodeEntityName", _luisConfig.ZipcodeEntityName },
                    { "expectedCompositeZipcode", expectedCompositeZipcode },
                    { "expectedStateEntityName", _luisConfig.StateEntityName },
                    { "expectedCompositeState", expectedCompositeState },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity1 },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "0" },
                        { "expectedDateTime", expectedDateTime1 },
                        { "expectedResolution", "time" },
                    },
                    new Dictionary<string, string>()
                    {
                        { "expectedDateTimeEntity", expectedDateTimeEntity2 },
                        { "expectedStartIndex", "0" },
                        { "expectedEndIndex", "0" },
                        { "expectedDateTime", expectedDateTime2 },
                        { "expectedResolution", "time" },
                    },
                });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.RegisterExpectedResponseMessage(RequestUri, expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedText);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNotNull(response.Flags, "Response flags should not be null!");
            Assert.AreEqual(1, response.Flags.Count, "Response flags should have one item!");
            Assert.AreEqual(ExtractInfoFlag.DateRejected, response.Flags[0], "Response flag should indicate date was rejected!");
            Assert.IsNotNull(response.Data);

            Assert.AreEqual(expectedIntent, response.Data.Intent);
            Assert.AreEqual(expectedText, response.Data.Transcription);
            Assert.AreEqual(expectedTransformedText, response.Data.EvaluatedTranscription);

            Assert.IsNotNull(response.Data.Dates);
            Assert.AreEqual(expectedDateInfos.Count, response.Data.Dates.Count);

            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(expectedDateInfos[i].FullDate, response.Data.Dates[i].FullDate);
                Assert.AreEqual(expectedDateInfos[i].Year, response.Data.Dates[i].Year);
                Assert.AreEqual(expectedDateInfos[i].Month, response.Data.Dates[i].Month);
                Assert.AreEqual(expectedDateInfos[i].Day, response.Data.Dates[i].Day);
                Assert.AreEqual(expectedDateInfos[i].Hour, response.Data.Dates[i].Hour);
                Assert.AreEqual(expectedDateInfos[i].Minute, response.Data.Dates[i].Minute);
            }

            Assert.IsNotNull(response.Data.Location);
            Assert.AreEqual(expectedLocation, response.Data.Location.Location);
            Assert.AreEqual(expectedCompositeCity, response.Data.Location.City);
            Assert.AreEqual(expectedCompositeState, response.Data.Location.State);
            Assert.AreEqual(expectedCompositeZipcode, response.Data.Location.Zipcode);

            Assert.IsNotNull(response.Data.Person);
            Assert.AreEqual(expectedPersonName, response.Data.Person.Name);
            Assert.AreEqual(expectedPersonType, response.Data.Person.Type);

            Assert.AreEqual((int)ExtractInfoStatusCode.MissingEntities, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(ExtractInfoStatusCode), ExtractInfoStatusCode.MissingEntities), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        private static HttpRequest CreateHttpPostRequest(string expectedCallSid, string expectedText)
        {
            HttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = new DefaultHttpRequest(httpContext);

            request.Method = "POST";

            ExtractInfoRequest requestObj = new ExtractInfoRequest()
            {
                CallSid = expectedCallSid,
                Text = expectedText,
            };

            Stream contentBytes = new MemoryStream(
                Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(requestObj)));

            request.Body = contentBytes;

            return request;
        }
    }
}
