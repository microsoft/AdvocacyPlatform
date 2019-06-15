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
    /// Test for DeleteAccountRecordings.
    /// </summary>
    [TestClass]
    public class DeleteAccountRecordingsTests
    {
        /// <summary>
        /// Application setting name for Twilio account SID secret name.
        /// </summary>
        public const string TwilioAccountSidSecretNameAppSettingName = "twilioAccountSidSecretName";

        /// <summary>
        /// Application setting name for Twilio auth token secret name.
        /// </summary>
        public const string TwilioAuthTokenSecretNameAppSettingName = "twilioAuthTokenSecretName";

        /// <summary>
        /// Application setting name for authorization authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        /// <summary>
        /// Test Twilio account SID.
        /// </summary>
        public const string TestAccountSid = "ACXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";

        /// <summary>
        /// Path to Twilio composite recording response template file.
        /// </summary>
        public const string RecordingCompositeTemplatePath = @".\Templates\Response\Twilio\Composite\RecordingCompositeTemplate.json";

        /// <summary>
        /// Key in template for placing composite recording response.
        /// </summary>
        public const string RecordingCompositesKey = "recordingsComposite";

        /// <summary>
        /// Path to Twilio recording response template file.
        /// </summary>
        public const string RecordingResponseTemplatePath = @".\Templates\Response\Twilio\RecordingResponseTemplate.json";

        private static MockLogger _log = new MockLogger();

        private IConfiguration _config;

        /// <summary>
        /// Gets the mock trace logger instance.
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

            DeleteAccountRecordingsHttpTrigger.Container = containerBuilder.Build();
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

            Assert.IsNotNull(DeleteAccountRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(DeleteAccountRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(DeleteAccountRecordingsHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(DeleteAccountRecordingsHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(DeleteAccountRecordingsHttpTrigger.Container.GetService<ITwilioCallWrapper>(), "Could not obtain ITwilioCallWrapper instance from DI container!");
            Assert.IsInstanceOfType(DeleteAccountRecordingsHttpTrigger.Container.GetService<ITwilioCallWrapper>(), typeof(TwilioCallWrapper), "Type of ITwilioCallWrapper should be TwilioCallWrapper!");
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

            Assert.IsNotNull(DeleteAccountRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(DeleteAccountRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(DeleteAccountRecordingsHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(DeleteAccountRecordingsHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(DeleteAccountRecordingsHttpTrigger.Container.GetService<ITwilioCallWrapper>(), "Could not obtain ITwilioCallWrapper instance from DI container!");
            Assert.IsInstanceOfType(DeleteAccountRecordingsHttpTrigger.Container.GetService<ITwilioCallWrapper>(), typeof(TwilioCallWrapperMock), "Type of ITwilioCallWrapper should be TwilioCallWrapperMock!");
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
            AzureKeyVaultSecretStoreMock secretStore = DeleteAccountRecordingsHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

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
            AzureKeyVaultSecretStoreMock secretStore = DeleteAccountRecordingsHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            secretStore.RegisterExpectedException(_config[TwilioAccountSidSecretNameAppSettingName], new SecretStoreException("TestException", null));
        }

        /// <summary>
        /// Initializes expected Twilio exceptions.
        /// </summary>
        /// <param name="exceptions">Dictionary of expected exceptions.</param>
        public void InitializeTwilioExceptions(Dictionary<string, Exception> exceptions)
        {
            TwilioCallWrapperMock twilioCallWrapper = DeleteAccountRecordingsHttpTrigger.Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapperMock;

            foreach (KeyValuePair<string, Exception> ex in exceptions)
            {
                twilioCallWrapper.RegisterExpectedRequestException(ex.Key, ex.Value);
            }
        }

        /// <summary>
        /// Ensure DeleteAccountRecordingsHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerFullSuccess()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

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
                        { "accountSid", TestAccountSid },
                        { "recordingSid", recordingSids[0] },
                    },
                    new Dictionary<string, string>
                    {
                        { "accountSid", TestAccountSid },
                        { "recordingSid", recordingSids[1] },
                    },
                });

            HttpResponseMessage expectedRecordingsResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = DeleteAccountRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedRecordingResponse(TestAccountSid, "recordings", expectedRecordingsResponse);

            foreach (string recordingSid in recordingSids)
            {
                httpClient.MessageHandler.RegisterExpectedRecordingResponse(recordingSid, "delete", TestHelper.CreateHttpResponseMessage(HttpStatusCode.NoContent, string.Empty));
            }

            HttpRequest request = CreateHttpPostRequest("Yes");

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)okResult.Value;

            Assert.IsTrue(response.AreAllRecordingsDeleted);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteAccountRecordingsHttpTrigger.Run() implementation handles partial success correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerPartialSuccess()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

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
                        { "accountSid", TestAccountSid },
                        { "recordingSid", recordingSids[0] },
                    },
                    new Dictionary<string, string>
                    {
                        { "accountSid", TestAccountSid },
                        { "recordingSid", recordingSids[1] },
                    },
                });

            HttpResponseMessage expectedRecordingsResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = DeleteAccountRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedRecordingResponse(TestAccountSid, "recordings", expectedRecordingsResponse);
            httpClient.MessageHandler.RegisterExpectedRecordingResponse(recordingSids[0], "delete", TestHelper.CreateHttpResponseMessage(HttpStatusCode.NoContent, string.Empty));
            httpClient.MessageHandler.RegisterExpectedRecordingResponse(recordingSids[1], "delete", TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, string.Empty));

            HttpRequest request = CreateHttpPostRequest("Yes");

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)okResult.Value;

            Assert.IsFalse(response.AreAllRecordingsDeleted);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteAccountRecordingsHttpTrigger.Run() implementation handles no Twilio call recordings correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerNoRecordings()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedResponseContent = TemplateManager.Load(RecordingResponseTemplatePath, new Dictionary<string, string>()
                {
                    { "pageSize", "0" },
                })
                .Replace($"{{{RecordingCompositesKey}}}", string.Empty);

            HttpResponseMessage expectedRecordingsResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = DeleteAccountRecordingsHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedRecordingResponse(TestAccountSid, "recordings", expectedRecordingsResponse);

            HttpRequest request = CreateHttpPostRequest("Yes");

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okRequestObject = (OkObjectResult)result;

            Assert.AreEqual(200, okRequestObject.StatusCode);
            Assert.IsInstanceOfType(okRequestObject.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)okRequestObject.Value;

            Assert.IsTrue(response.AreAllRecordingsDeleted);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteAccountRecordingsHttpTrigger.Run() implementation handles no confirmDelete in request body correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerMissingConfirmation()
        {
            InitializeWithNoTwilioMock();

            HttpRequest request = CreateHttpPostRequest(null);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)badRequestResult.Value;

            Assert.IsFalse(response.AreAllRecordingsDeleted);
            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)DeleteAccountRecordingsErrorCode.RequestMissingConfirmDelete, response.ErrorCode);
            Assert.AreEqual($"You must specify a value for the key '{DeleteAccountRecordingsRequest.ConfirmDeleteKeyName}' in the request body!", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteAccountRecordingsHttpTrigger.Run() implementation handles invalid value for confirmDelete in request body correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerInvalidConfirmation()
        {
            InitializeWithNoTwilioMock();

            HttpRequest request = CreateHttpPostRequest("What?");

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)badRequestResult.Value;

            Assert.IsFalse(response.AreAllRecordingsDeleted);
            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)DeleteAccountRecordingsErrorCode.RequestMissingConfirmDelete, response.ErrorCode);
            Assert.AreEqual($"You must specify a value for the key '{DeleteAccountRecordingsRequest.ConfirmDeleteKeyName}' in the request body!", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteAccountRecordingsHttpTrigger.Run() implementation handles secret store exceptions successfully.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerRunSecretStoreException()
        {
            InitializeWithNoTwilioMock();
            InitializeSecretsExceptions();

            HttpRequest request = CreateHttpPostRequest("Yes");

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)okResult.Value;

            Assert.IsFalse(response.AreAllRecordingsDeleted);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.SecretStoreGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.SecretStoreGenericFailureMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteAccountRecordingsHttpTrigger.Run() implementation handles Twilio authentication exceptions correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerRunTwilioAuthenticationException()
        {
            InitializeWithTwilioMock();
            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { _config[GlobalConstants.TwilioAccountSidSecretNameAppSettingName], new TwilioEx.AuthenticationException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest("Yes");

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)okResult.Value;

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioAuthenticationFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioAuthenticationFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteAccountRecordingsHttpTrigger.Run() implementation handles Twilio API connection exceptions correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerRunTwilioApiConnectionException()
        {
            InitializeWithTwilioMock();

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { TestAccountSid, new TwilioEx.ApiConnectionException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest("Yes");

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)okResult.Value;

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioApiConnectionFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioApiConnectionFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteAccountRecordingsHttpTrigger.Run() implementation handles Twilio API request exceptions correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerRunTwilioApiRequestException()
        {
            InitializeWithTwilioMock();

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { TestAccountSid, new TwilioEx.ApiException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest("Yes");

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)okResult.Value;

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioApiRequestFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioApiRequestFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteAccountRecordingsHttpTrigger.Run() implementation handles Twilio REST request exceptions correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerRunTwilioRestRequestException()
        {
            InitializeWithTwilioMock();

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { TestAccountSid, new TwilioEx.RestException() },
            });

            HttpRequest request = CreateHttpPostRequest("Yes");

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)okResult.Value;

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioRestCallFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioRestCallFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteAccountRecordingsHttpTrigger.Run() implementation handles Twilio generic exceptions correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerRunTwilioGenericException()
        {
            InitializeWithTwilioMock();

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { TestAccountSid, new TwilioEx.TwilioException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest("Yes");

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)okResult.Value;

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioGenericFailureMessage, response.ErrorDetails);
        }

        private static HttpRequest CreateHttpPostRequest(string confirmDelete)
        {
            HttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = new DefaultHttpRequest(httpContext);

            DeleteAccountRecordingsRequest requestObj = new DeleteAccountRecordingsRequest()
            {
                ConfirmDelete = confirmDelete,
            };

            Stream contentBytes = new MemoryStream(
                Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(requestObj)));

            request.Body = contentBytes;

            return request;
        }
    }
}
