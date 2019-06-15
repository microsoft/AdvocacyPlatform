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
    /// Tests for CheckCallProgress.
    /// </summary>
    [TestClass]
    public class CheckCallProgressTests
    {
        /// <summary>
        /// Path to call response template file.
        /// </summary>
        public const string CallResponseTemplatePath = @".\Templates\Response\Twilio\CallResponseTemplate.json";

        /// <summary>
        /// Application setting name with Twilio account SID secret name.
        /// </summary>
        public const string TwilioAccountSidSecretNameAppSettingName = "twilioAccountSidSecretName";

        /// <summary>
        /// Application setting name with Twilio auth token secret name.
        /// </summary>
        public const string TwilioAuthTokenSecretNameAppSettingName = "twilioAuthTokenSecretName";

        /// <summary>
        /// Application setting name with authorizing authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        private static MockLogger _log = new MockLogger();

        private static List<CallResource.StatusEnum> _failedStatuses = new List<CallResource.StatusEnum>()
        {
            CallResource.StatusEnum.Busy,
            CallResource.StatusEnum.Failed,
            CallResource.StatusEnum.NoAnswer,
            CallResource.StatusEnum.Canceled,
        };

        private static List<CallResource.StatusEnum> _inProgressStatuses = new List<CallResource.StatusEnum>()
        {
            CallResource.StatusEnum.Queued,
            CallResource.StatusEnum.Ringing,
            CallResource.StatusEnum.InProgress,
        };

        private IConfiguration _config;

        /// <summary>
        /// Gets the mock logger instance.
        /// </summary>
        public static MockLogger Log => _log;

        /// <summary>
        /// Gets the list of failed call statuses.
        /// </summary>
        public static List<CallResource.StatusEnum> FailedStatuses => _failedStatuses;

        /// <summary>
        /// Gets the list of in-progress call statuses.
        /// </summary>
        public static List<CallResource.StatusEnum> InProgressStatuses => _inProgressStatuses;

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

            CheckCallProgressHttpTrigger.Container = containerBuilder.Build();
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

            Assert.IsNotNull(CheckCallProgressHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(CheckCallProgressHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(CheckCallProgressHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(CheckCallProgressHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(CheckCallProgressHttpTrigger.Container.GetService<ITwilioCallWrapper>(), "Could not obtain ITwilioCallWrapper instance from DI container!");
            Assert.IsInstanceOfType(CheckCallProgressHttpTrigger.Container.GetService<ITwilioCallWrapper>(), typeof(TwilioCallWrapper), "Type of ITwilioCallWrapper should be TwilioCallWrapper!");
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

            Assert.IsNotNull(CheckCallProgressHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(CheckCallProgressHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(CheckCallProgressHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(CheckCallProgressHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(CheckCallProgressHttpTrigger.Container.GetService<ITwilioCallWrapper>(), "Could not obtain ITwilioCallWrapper instance from DI container!");
            Assert.IsInstanceOfType(CheckCallProgressHttpTrigger.Container.GetService<ITwilioCallWrapper>(), typeof(TwilioCallWrapperMock), "Type of ITwilioCallWrapper should be TwilioCallWrapperMock!");
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
            AzureKeyVaultSecretStoreMock secretStore = CheckCallProgressHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            Secret expectedTwilioAccountSidSecret = new Secret(_config[TwilioAccountSidSecretNameAppSettingName], TestHelper.CreateSecureString("mock-account-sid"));

            secretStore.RegisterExpectedSecret(expectedTwilioAccountSidSecret);

            Secret expectedTwilioAuthTokenSecret = new Secret(_config[TwilioAuthTokenSecretNameAppSettingName], TestHelper.CreateSecureString("mock-auth-token"));

            secretStore.RegisterExpectedSecret(expectedTwilioAuthTokenSecret);
        }

        /// <summary>
        /// Initializes expected secret store exceptions.
        /// </summary>
        public void InitializeSecretsExceptions()
        {
            AzureKeyVaultSecretStoreMock secretStore = CheckCallProgressHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            secretStore.RegisterExpectedException(_config[TwilioAccountSidSecretNameAppSettingName], new SecretStoreException("TestException", null));
        }

        /// <summary>
        /// Initializes expected Twilio exceptions.
        /// </summary>
        /// <param name="exceptions">Dictionary of expected exceptions.</param>
        public void InitializeTwilioExceptions(Dictionary<string, Exception> exceptions)
        {
            TwilioCallWrapperMock twilioCallWrapper = CheckCallProgressHttpTrigger.Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapperMock;

            foreach (KeyValuePair<string, Exception> ex in exceptions)
            {
                twilioCallWrapper.RegisterExpectedRequestException(ex.Key, ex.Value);
            }
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunCompleted()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedStatus = "completed";
            string expectedCallDuration = "500";

            string expectedResponseTemplate = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatus },
                { "expectedCallDuration", expectedCallDuration },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseTemplate);

            HttpClientMock httpClient = CheckCallProgressHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedCallSid, "status", expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

            CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.AreEqual(int.Parse(expectedCallDuration), response.Duration);
            Assert.AreEqual(expectedStatus, response.Status);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation runs correctly with no call duration.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunNoDuration()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedStatus = "completed";
            string expectedCallDuration = "null";

            string expectedResponseTemplate = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
            {
                { "expectedCallSid", expectedCallSid },
                { "expectedStatus", expectedStatus },
                { "expectedCallDuration", expectedCallDuration },
            });

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseTemplate);

            HttpClientMock httpClient = CheckCallProgressHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedCallResponse(expectedCallSid, "status", expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

            CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.AreEqual(0, response.Duration);
            Assert.AreEqual(expectedStatus, response.Status);
            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation handles call failure statuses correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunFailedStatuses()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            HttpClientMock httpClient = CheckCallProgressHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            for (int index = 0; index < FailedStatuses.Count; index++)
            {
                string expectedCallSid = $"CA1000000000000000000000000000000{index + 1}";
                string expectedStatus = FailedStatuses[index].ToString();
                string expectedCallDuration = "20";

                string expectedResponse = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
                {
                    { "expectedCallSid", expectedCallSid },
                    { "expectedStatus", expectedStatus },
                    { "expectedCallDuration", expectedCallDuration },
                });

                HttpResponseMessage expectedResponseMessage = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponse);

                httpClient.MessageHandler.RegisterExpectedCallResponse(expectedCallSid, "status", expectedResponseMessage);

                HttpRequest request = CreateHttpPostRequest(expectedCallSid);

                ExecutionContext context = new ExecutionContext()
                {
                    FunctionAppDirectory = Directory.GetCurrentDirectory(),
                };

                IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

                Assert.IsInstanceOfType(result, typeof(OkObjectResult));

                OkObjectResult okResult = (OkObjectResult)result;

                Assert.AreEqual(200, okResult.StatusCode);
                Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

                CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

                Assert.AreEqual(expectedCallSid, response.CallSid);
                Assert.AreEqual(int.Parse(expectedCallDuration), response.Duration);
                Assert.AreEqual(expectedStatus, response.Status);
                Assert.IsTrue(response.HasError);
                Assert.AreEqual((int)CommonErrorCode.TwilioCallFailed, response.ErrorCode);
                Assert.AreEqual("Call failed.", response.ErrorDetails);
            }
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation handles call in-progress statuses correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunInProgressStatuses()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            HttpClientMock httpClient = CheckCallProgressHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            for (int index = 0; index < InProgressStatuses.Count; index++)
            {
                string expectedCallSid = $"CA10000000000000000000000000000{index + 101}";
                string expectedStatus = InProgressStatuses[index].ToString();
                string expectedCallDuration = "600";

                string expectedResponse = TemplateManager.Load(CallResponseTemplatePath, new Dictionary<string, string>()
                {
                    { "expectedCallSid", expectedCallSid },
                    { "expectedStatus", expectedStatus },
                    { "expectedCallDuration", expectedCallDuration },
                });

                HttpResponseMessage expectedResponseMessage = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponse);

                httpClient.MessageHandler.RegisterExpectedCallResponse(expectedCallSid, "status", expectedResponseMessage);

                HttpRequest request = CreateHttpPostRequest(expectedCallSid);

                ExecutionContext context = new ExecutionContext()
                {
                    FunctionAppDirectory = Directory.GetCurrentDirectory(),
                };

                IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

                Assert.IsInstanceOfType(result, typeof(OkObjectResult));

                OkObjectResult okResult = (OkObjectResult)result;

                Assert.AreEqual(200, okResult.StatusCode);
                Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

                CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

                Assert.AreEqual(expectedCallSid, response.CallSid);
                Assert.AreEqual(int.Parse(expectedCallDuration), response.Duration);
                Assert.AreEqual(expectedStatus, response.Status);
                Assert.IsFalse(response.HasError);
                Assert.AreEqual(0, response.ErrorCode);
                Assert.IsNull(response.ErrorDetails);
            }
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation handles bad requests correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunBadRequest()
        {
            InitializeWithNoTwilioMock();

            string expectedCallSid = $"CA10000000000000000000000000000200";

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(CheckCallProgressResponse));

            CheckCallProgressResponse response = (CheckCallProgressResponse)badRequestResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.Status);
            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.BadRequest, response.ErrorCode);
            Assert.AreEqual("Bad request.", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation handles requests with missing callSid correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunMissingCallSid()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            HttpRequest request = CreateHttpPostRequest(null);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(CheckCallProgressResponse));

            CheckCallProgressResponse response = (CheckCallProgressResponse)badRequestResult.Value;

            Assert.IsNull(response.CallSid);
            Assert.IsNull(response.Status);
            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CheckCallProgressErrorCode.RequestMissingCallSid, response.ErrorCode);
            Assert.AreEqual($"You must specify a value for the key '{CheckCallProgressRequest.CallSidKeyName}' in the request body!", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation handles secret store exceptions correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunSecretStoreException()
        {
            InitializeWithNoTwilioMock();

            string expectedCallSid = $"CA10000000000000000000000000000200";

            HttpRequest request = CreateHttpPostRequest(expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            InitializeSecretsExceptions();

            IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

            CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.Status);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.SecretStoreGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.SecretStoreGenericFailureMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation handles Twilio authentication exceptions correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunTwilioAuthenticationException()
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

            IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

            CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.Status);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioAuthenticationFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioAuthenticationFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation handles Twilio API connection exceptions correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunTwilioApiConnectionException()
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

            IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

            CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.Status);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioApiConnectionFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioApiConnectionFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation handles Twilio API request exceptions correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunTwilioApiRequestException()
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

            IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

            CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.Status);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioApiRequestFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioApiRequestFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation handles Twilio REST request exceptions correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunTwilioRestRequestException()
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

            IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

            CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.Status);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioRestCallFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioRestCallFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure CheckCallProgressHttpTrigger.Run() implementation handles Twilio generic exceptions correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRunTwilioGenericException()
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

            IActionResult result = CheckCallProgressHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

            CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.Status);

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

            CheckCallProgressRequest requestObj = new CheckCallProgressRequest()
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
