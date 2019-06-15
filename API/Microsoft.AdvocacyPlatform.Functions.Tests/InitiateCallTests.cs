// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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
    using Twilio.Rest.Api.V2010.Account;
    using TwilioEx = Twilio.Exceptions;
    using TwilioHttp = Twilio.Http;

    /// <summary>
    /// Tests for InitiateCall.
    /// </summary>
    [TestClass]
    public class InitiateCallTests
    {
        /// <summary>
        /// Application setting name for Twilio local number secret name.
        /// </summary>
        public const string TwilioLocalNumberSecretNameAppSettingName = "twilioLocalNumberSecretName";

        /// <summary>
        /// Application setting name for Twilio account SID secret name.
        /// </summary>
        public const string TwilioAccountSidSecretNameAppSettingName = "twilioAccountSidSecretName";

        /// <summary>
        /// Application setting name for Twilio auth token secret name.
        /// </summary>
        public const string TwilioAuthTokenSecretNameAppSettingName = "twilioAuthTokenSecretName";

        /// <summary>
        /// Application setting name for authorizing authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        /// <summary>
        /// Path to Twilio call response template file.
        /// </summary>
        public const string CallResponseTemplatePath = @".\Templates\Response\Twilio\CallResponseTemplate.json";

        /// <summary>
        /// Application setting name with the expected number to call.
        /// </summary>
        public const string ExpectedNumberToCallAppSetting = "numberToCall";

        private static MockLogger _log = new MockLogger();

        private IConfigurationRoot _config;

        /// <summary>
        /// Gets the mock logger instance.
        /// </summary>
        public static MockLogger Log => _log;

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

            InitiateCallHttpTrigger.Container = containerBuilder.Build();
        }

        /// <summary>
        /// Initializes test with no TwilioClient mock.
        /// </summary>
        public void InitializeWithNoTwilioMock()
        {
            InitializeServiceProvider(new List<Module>()
            {
                new HttpClientMockModule(),
                new AzureKeyVaultSecretStoreMockModule(),
                new TwilioModule(),
                new ValueValidatorFactoryModule(),
            });

            Assert.IsNotNull(InitiateCallHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(InitiateCallHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(InitiateCallHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(InitiateCallHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(InitiateCallHttpTrigger.Container.GetService<ITwilioCallWrapper>(), "Could not obtain ITwilioCallWrapper instance from DI container!");
            Assert.IsInstanceOfType(InitiateCallHttpTrigger.Container.GetService<ITwilioCallWrapper>(), typeof(TwilioCallWrapper), "Type of ITwilioCallWrapper should be TwilioCallWrapper!");
        }

        /// <summary>
        /// Initializes test with TwilioClient mock.
        /// </summary>
        public void InitializeWithTwilioMock()
        {
            InitializeServiceProvider(new List<Module>()
            {
                new HttpClientMockModule(),
                new AzureKeyVaultSecretStoreMockModule(),
                new TwilioMockModule(),
                new ValueValidatorFactoryModule(),
            });

            Assert.IsNotNull(InitiateCallHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(InitiateCallHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(InitiateCallHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(InitiateCallHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(InitiateCallHttpTrigger.Container.GetService<ITwilioCallWrapper>(), "Could not obtain ITwilioCallWrapper instance from DI container!");
            Assert.IsInstanceOfType(InitiateCallHttpTrigger.Container.GetService<ITwilioCallWrapper>(), typeof(TwilioCallWrapperMock), "Type of ITwilioCallWrapper should be TwilioCallWrapperMock!");
        }

        /// <summary>
        /// Initializes test.
        /// </summary>
        [TestInitialize]
        public void InitializeTests()
        {
            _config = TestHelper.GetConfiguration();
        }

        /// <summary>
        /// Initializes mock secret store.
        /// </summary>
        /// <param name="excludeTwilioAccountSecret">Flag indicating whether the Twilio account SID secret should not be added.</param>
        public void InitializeSecrets(bool excludeTwilioAccountSecret = false)
        {
            AzureKeyVaultSecretStoreMock secretStore = InitiateCallHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            Secret expectedTwilioLocalNumberSecret = new Secret(_config[TwilioLocalNumberSecretNameAppSettingName], TestHelper.CreateSecureString("0123456789"));

            secretStore.RegisterExpectedSecret(expectedTwilioLocalNumberSecret);

            if (!excludeTwilioAccountSecret)
            {
                Secret expectedTwilioAccountSidSecret = new Secret(_config[TwilioAccountSidSecretNameAppSettingName], TestHelper.CreateSecureString("mock-account-sid"));

                secretStore.RegisterExpectedSecret(expectedTwilioAccountSidSecret);
            }

            Secret expectedTwilioAuthTokenSecret = new Secret(_config[TwilioAuthTokenSecretNameAppSettingName], TestHelper.CreateSecureString("mock-auth-token"));

            secretStore.RegisterExpectedSecret(expectedTwilioAuthTokenSecret);
        }

        /// <summary>
        /// Initializes expected secret store exceptions.
        /// </summary>
        public void InitializeSecretsExceptions()
        {
            AzureKeyVaultSecretStoreMock secretStore = InitiateCallHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            secretStore.RegisterExpectedException(_config[TwilioAccountSidSecretNameAppSettingName], new SecretStoreException("TestException", null));
        }

        /// <summary>
        /// Initializes expected Twilio exceptions.
        /// </summary>
        /// <param name="exceptions">Dictionary of expected exceptions.</param>
        public void InitializeTwilioExceptions(Dictionary<string, Exception> exceptions)
        {
            TwilioCallWrapperMock twilioCallWrapper = InitiateCallHttpTrigger.Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapperMock;

            foreach (KeyValuePair<string, Exception> ex in exceptions)
            {
                twilioCallWrapper.RegisterExpectedRequestException(ex.Key, ex.Value);
            }
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation with no dtmf in request body runs correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerSuccessNoDtmf()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedInputId = "012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedStatus = "in-progress";
            string expectedNumberToCall = _config[ExpectedNumberToCallAppSetting];

            string expectedResponseContent = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatus },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = InitiateCallHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedNumberToCall, "call", expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedInputId);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = InitiateCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.AreEqual(expectedInputId, response.InputId);
            Assert.AreEqual(expectedInputId, response.AcceptedInputId);
            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation with inputId with dashes in request body runs correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerSuccessAINWithDash()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedInputId = "012-345-678";
            string expectedAcceptedInputId = "012345678";
            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedStatus = "in-progress";
            string expectedNumberToCall = _config[ExpectedNumberToCallAppSetting];

            string expectedResponseContent = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatus },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = InitiateCallHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedNumberToCall, "call", expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedInputId);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = InitiateCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.AreEqual(expectedInputId, response.InputId);
            Assert.AreEqual(expectedAcceptedInputId, response.AcceptedInputId);
            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation with dtmf in request body runs correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerSuccessWithDtmf()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedInputId = "012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedStatus = "in-progress";
            string expectedNumberToCall = _config[ExpectedNumberToCallAppSetting];

            string expectedResponseContent = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatus },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = InitiateCallHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedNumberToCall, "call", expectedResponse);

            HttpRequest request = CreateHttpPostRequest(
                expectedInputId,
                new DtmfRequest()
                {
                    Dtmf = "1ww012345678ww1ww1ww1",
                    InitPause = 0,
                    FinalPause = 60,
                });

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = InitiateCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.AreEqual(expectedInputId, response.InputId);
            Assert.AreEqual(expectedInputId, response.AcceptedInputId);
            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation with partial dtmf configuration in request body runs correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerSuccessWithPartialDtmf()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedInputId = "012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedStatus = "in-progress";
            string expectedNumberToCall = _config[ExpectedNumberToCallAppSetting];

            string expectedResponseContent = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatus },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = InitiateCallHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedNumberToCall, "call", expectedResponse);

            HttpRequest request = CreateHttpPostRequest(
                expectedInputId,
                new DtmfRequest()
                {
                    FinalPause = 60,
                });

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = InitiateCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.AreEqual(expectedInputId, response.InputId);
            Assert.AreEqual(expectedInputId, response.AcceptedInputId);
            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation with no inputId in request body runs correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerMissingInputId()
        {
            InitializeWithNoTwilioMock();

            HttpRequest request = CreateHttpPostRequest(null);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = InitiateCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult okResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.IsNull(response.InputId);
            Assert.IsNull(response.AcceptedInputId);
            Assert.IsNull(response.CallSid);
            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);
            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)InitiateCallErrorCode.RequestMissingInputId, response.ErrorCode);
            Assert.AreEqual($"You must specify a value for the key '{InitiateCallRequest.InputIdKeyName}' in the request body!", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation handles secret store exceptions correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerRunSecretStoreException()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets(true);

            string expectedInputId = "012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/

            HttpRequest request = CreateHttpPostRequest(expectedInputId);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            InitializeSecretsExceptions();

            IActionResult result = InitiateCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.AreEqual(expectedInputId, response.InputId);
            Assert.AreEqual(expectedInputId, response.AcceptedInputId);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.SecretStoreGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.SecretStoreGenericFailureMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation handles Twilio authentication exceptions correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerRunTwilioAuthenticationException()
        {
            InitializeWithTwilioMock();
            InitializeSecrets(true);
            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { _config[GlobalConstants.TwilioAccountSidSecretNameAppSettingName], new TwilioEx.AuthenticationException("TestException") },
            });

            string expectedInputId = "012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/

            HttpRequest request = CreateHttpPostRequest(expectedInputId);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = InitiateCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.AreEqual(expectedInputId, response.InputId);
            Assert.AreEqual(expectedInputId, response.AcceptedInputId);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioAuthenticationFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioAuthenticationFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation handles Twilio API connection exceptions correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerRunTwilioApiConnectionException()
        {
            InitializeWithTwilioMock();
            InitializeSecrets();

            string expectedInputId = "012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { _config[ExpectedNumberToCallAppSetting], new TwilioEx.ApiConnectionException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest(expectedInputId);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = InitiateCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.AreEqual(expectedInputId, response.InputId);
            Assert.AreEqual(expectedInputId, response.AcceptedInputId);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioApiConnectionFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioApiConnectionFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation handles Twilio API request exceptions correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerRunTwilioApiRequestException()
        {
            InitializeWithTwilioMock();
            InitializeSecrets();

            string expectedInputId = "012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { _config[ExpectedNumberToCallAppSetting], new TwilioEx.ApiException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest(expectedInputId);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = InitiateCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.AreEqual(expectedInputId, response.InputId);
            Assert.AreEqual(expectedInputId, response.AcceptedInputId);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioApiRequestFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioApiRequestFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation handles Twilio REST request exceptions correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerRunTwilioRestRequestException()
        {
            InitializeWithTwilioMock();
            InitializeSecrets();

            string expectedInputId = "012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { _config[ExpectedNumberToCallAppSetting], new TwilioEx.RestException() },
            });

            HttpRequest request = CreateHttpPostRequest(expectedInputId);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = InitiateCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.AreEqual(expectedInputId, response.InputId);
            Assert.AreEqual(expectedInputId, response.AcceptedInputId);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioRestCallFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioRestCallFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation handles Twilio generic exceptions correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerRunTwilioGenericException()
        {
            InitializeWithTwilioMock();
            InitializeSecrets();

            string expectedInputId = "012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { _config[ExpectedNumberToCallAppSetting], new TwilioEx.TwilioException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest(expectedInputId);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = InitiateCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.AreEqual(expectedInputId, response.InputId);
            Assert.AreEqual(expectedInputId, response.AcceptedInputId);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioGenericFailureMessage, response.ErrorDetails);
        }

        private static HttpRequest CreateHttpPostRequest(string inputId, DtmfRequest dtmf = null)
        {
            HttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = new DefaultHttpRequest(httpContext);

            InitiateCallRequest requestObj = new InitiateCallRequest()
            {
                InputId = inputId,
                Dtmf = dtmf,
            };

            Stream contentBytes = new MemoryStream(
                Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(requestObj)));

            request.Body = contentBytes;

            return request;
        }
    }
}
