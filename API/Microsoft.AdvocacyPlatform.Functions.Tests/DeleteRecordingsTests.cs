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
    /// Tests for DeleteRecordings.
    /// </summary>
    [TestClass]
    public class DeleteRecordingsTests
    {
        /// <summary>
        /// Application setting name for the Twilio account SID secret.
        /// </summary>
        public const string TwilioAccountSidSecretNameAppSettingName = "twilioAccountSidSecretName";

        /// <summary>
        /// Application setting name for the Twilio auth token secret.
        /// </summary>
        public const string TwilioAuthTokenSecretNameAppSettingName = "twilioAuthTokenSecretName";

        /// <summary>
        /// Application setting name for the authorization authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        /// <summary>
        /// Test Twilio account SID.
        /// </summary>
        public const string TestAccountSid = "ACXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";

        /// <summary>
        /// Path to the Twilio composite recordings response template file.
        /// </summary>
        public const string RecordingCompositeTemplatePath = @".\Templates\Response\Twilio\Composite\RecordingCompositeTemplate.json";

        /// <summary>
        /// Key for injecting composite recordings response.
        /// </summary>
        public const string RecordingCompositesKey = "recordingsComposite";

        /// <summary>
        /// Path to the Twilio recording response template file.
        /// </summary>
        public const string RecordingResponseTemplatePath = @".\Templates\Response\Twilio\RecordingResponseTemplate.json";

        private static MockLogger _log = new MockLogger();

        private IConfiguration _config;

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

            DeleteRecordingsHttpTrigger.Container = containerBuilder.Build();
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
            });

            Assert.IsNotNull(DeleteRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(DeleteRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(DeleteRecordingsHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(DeleteRecordingsHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(DeleteRecordingsHttpTrigger.Container.GetService<ITwilioCallWrapper>(), "Could not obtain ITwilioCallWrapper instance from DI container!");
            Assert.IsInstanceOfType(DeleteRecordingsHttpTrigger.Container.GetService<ITwilioCallWrapper>(), typeof(TwilioCallWrapper), "Type of ITwilioCallWrapper should be TwilioCallWrapper!");
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
            });

            Assert.IsNotNull(DeleteRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(DeleteRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(DeleteRecordingsHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(DeleteRecordingsHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(DeleteRecordingsHttpTrigger.Container.GetService<ITwilioCallWrapper>(), "Could not obtain ITwilioCallWrapper instance from DI container!");
            Assert.IsInstanceOfType(DeleteRecordingsHttpTrigger.Container.GetService<ITwilioCallWrapper>(), typeof(TwilioCallWrapperMock), "Type of ITwilioCallWrapper should be TwilioCallWrapperMock!");
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
        public void InitializeSecrets()
        {
            AzureKeyVaultSecretStoreMock secretStore = DeleteRecordingsHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            Secret expectedTwilioAccountSidSecret = new Secret(_config[TwilioAccountSidSecretNameAppSettingName], TestHelper.CreateSecureString(TestAccountSid));

            secretStore.RegisterExpectedSecret(expectedTwilioAccountSidSecret);

            Secret expectedTwilioAuthTokenSecret = new Secret(_config[TwilioAuthTokenSecretNameAppSettingName], TestHelper.CreateSecureString("mock-auth-token"));

            secretStore.RegisterExpectedSecret(expectedTwilioAuthTokenSecret);
        }

        /// <summary>
        /// Initializes expected secret store exceptions.
        /// </summary>
        public void InitializeSecretsExceptions()
        {
            AzureKeyVaultSecretStoreMock secretStore = DeleteRecordingsHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            secretStore.RegisterExpectedException(_config[TwilioAccountSidSecretNameAppSettingName], new SecretStoreException("TestException", null));
        }

        /// <summary>
        /// Initializes expected Twilio exceptions.
        /// </summary>
        /// <param name="exceptions">Dictionary of expected exceptions.</param>
        public void InitializeTwilioExceptions(Dictionary<string, Exception> exceptions)
        {
            TwilioCallWrapperMock twilioCallWrapper = DeleteRecordingsHttpTrigger.Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapperMock;

            foreach (KeyValuePair<string, Exception> ex in exceptions)
            {
                twilioCallWrapper.RegisterExpectedRequestException(ex.Key, ex.Value);
            }
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerFullSuccess()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string[] recordingSids =
            {
                "RE10000000000000000000000000000001",
                "RE10000000000000000000000000000002",
            };

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                RecordingResponseTemplatePath,
                RecordingCompositesKey,
                RecordingCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "pageSize", "2" },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "callSid", expectedCallSid },
                        { "recordingSid", recordingSids[0] },
                    },
                    new Dictionary<string, string>
                    {
                        { "callSid", expectedCallSid },
                        { "recordingSid", recordingSids[1] },
                    },
                });

            HttpResponseMessage expectedRecordingsResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = DeleteRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedRecordingResponse(expectedCallSid, "recordings", expectedRecordingsResponse);

            foreach (string recordingSid in recordingSids)
            {
                httpClient.MessageHandler.RegisterExpectedRecordingResponse(recordingSid, "delete", TestHelper.CreateHttpResponseMessage(HttpStatusCode.NoContent, string.Empty));
            }

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteRecordingsResponse));

            DeleteRecordingsResponse response = (DeleteRecordingsResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsTrue(response.AreAllRecordingsDeleted);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation handles partial success correctly.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerPartialSuccess()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";
            string[] recordingSids =
            {
                "RE10000000000000000000000000000001",
                "RE10000000000000000000000000000002",
            };

            string expectedResponseContent = TemplateManager.LoadWithComposites(
                RecordingResponseTemplatePath,
                RecordingCompositesKey,
                RecordingCompositeTemplatePath,
                new Dictionary<string, string>()
                {
                    { "pageSize", "2" },
                },
                new List<Dictionary<string, string>>()
                {
                    new Dictionary<string, string>()
                    {
                        { "callSid", expectedCallSid },
                        { "recordingSid", recordingSids[0] },
                    },
                    new Dictionary<string, string>
                    {
                        { "callSid", expectedCallSid },
                        { "recordingSid", recordingSids[1] },
                    },
                });

            HttpResponseMessage expectedRecordingsResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = DeleteRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedRecordingResponse(expectedCallSid, "recordings", expectedRecordingsResponse);
            httpClient.MessageHandler.RegisterExpectedRecordingResponse(recordingSids[0], "delete", TestHelper.CreateHttpResponseMessage(HttpStatusCode.NoContent, string.Empty));
            httpClient.MessageHandler.RegisterExpectedRecordingResponse(recordingSids[1], "delete", TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, string.Empty));

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteRecordingsResponse));

            DeleteRecordingsResponse response = (DeleteRecordingsResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsFalse(response.AreAllRecordingsDeleted);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation handles no Twilio call recordings correctly.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerNoRecordings()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000300";

            string expectedResponseContent = TemplateManager.Load(RecordingResponseTemplatePath, new Dictionary<string, string>()
                {
                    { "pageSize", "0" },
                })
                .Replace($"{{{RecordingCompositesKey}}}", string.Empty);

            HttpResponseMessage expectedRecordingsResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = DeleteRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedRecordingResponse(expectedCallSid, "recordings", expectedRecordingsResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okRequestObject = (OkObjectResult)result;

            Assert.AreEqual(200, okRequestObject.StatusCode);
            Assert.IsInstanceOfType(okRequestObject.Value, typeof(DeleteRecordingsResponse));

            DeleteRecordingsResponse response = (DeleteRecordingsResponse)okRequestObject.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsTrue(response.AreAllRecordingsDeleted);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation handles a missing callSid in the request body correctly.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerMissingCallSid()
        {
            InitializeWithNoTwilioMock();

            HttpRequest request = CreateHttpPostRequest(null);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(DeleteRecordingsResponse));

            DeleteRecordingsResponse response = (DeleteRecordingsResponse)badRequestResult.Value;

            Assert.IsNull(response.CallSid);
            Assert.IsFalse(response.AreAllRecordingsDeleted);
            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)DeleteRecordingsErrorCode.RequestMissingCallSid, response.ErrorCode);
            Assert.AreEqual($"You must specify a value for the key '{DeleteRecordingsRequest.CallSidKeyName}' in the request body!", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation handles secret store exceptions correctly.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerRunSecretStoreException()
        {
            InitializeWithNoTwilioMock();
            InitializeSecretsExceptions();

            string expectedCallSid = $"CA10000000000000000000000000000200";

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteRecordingsResponse));

            DeleteRecordingsResponse response = (DeleteRecordingsResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.SecretStoreGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.SecretStoreGenericFailureMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation handles Twilio authentication exceptions correctly.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerRunTwilioAuthenticationException()
        {
            InitializeWithTwilioMock();
            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { _config[GlobalConstants.TwilioAccountSidSecretNameAppSettingName], new TwilioEx.AuthenticationException("TestException") },
            });

            string expectedCallSid = $"CA10000000000000000000000000000200";

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteRecordingsResponse));

            DeleteRecordingsResponse response = (DeleteRecordingsResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioAuthenticationFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioAuthenticationFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation handles Twilio API connection exceptions correctly.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerRunTwilioApiConnectionException()
        {
            InitializeWithTwilioMock();

            string expectedCallSid = $"CA10000000000000000000000000000200";

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { expectedCallSid, new TwilioEx.ApiConnectionException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteRecordingsResponse));

            DeleteRecordingsResponse response = (DeleteRecordingsResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioApiConnectionFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioApiConnectionFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation handles Twilio API request exceptions correctly.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerRunTwilioApiRequestException()
        {
            InitializeWithTwilioMock();

            string expectedCallSid = $"CA10000000000000000000000000000200";

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { expectedCallSid, new TwilioEx.ApiException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteRecordingsResponse));

            DeleteRecordingsResponse response = (DeleteRecordingsResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioApiRequestFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioApiRequestFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation handles Twilio REST request exceptions correctly.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerRunTwilioRestRequestException()
        {
            InitializeWithTwilioMock();

            string expectedCallSid = $"CA10000000000000000000000000000200";

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { expectedCallSid, new TwilioEx.RestException() },
            });

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteRecordingsResponse));

            DeleteRecordingsResponse response = (DeleteRecordingsResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioRestCallFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioRestCallFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation handles Twilio generic exceptions correctly.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerRunTwilioGenericException()
        {
            InitializeWithTwilioMock();

            string expectedCallSid = $"CA10000000000000000000000000000200";

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { expectedCallSid, new TwilioEx.TwilioException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteRecordingsResponse));

            DeleteRecordingsResponse response = (DeleteRecordingsResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioGenericFailureMessage, response.ErrorDetails);
        }

        private static HttpRequest CreateHttpPostRequest(string callSid)
        {
            HttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = new DefaultHttpRequest(httpContext);

            DeleteRecordingsRequest requestObj = new DeleteRecordingsRequest()
            {
                CallSid = callSid,
            };

            Stream contentBytes = new MemoryStream(
                Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(requestObj)));

            request.Body = contentBytes;

            return request;
        }
    }
}
