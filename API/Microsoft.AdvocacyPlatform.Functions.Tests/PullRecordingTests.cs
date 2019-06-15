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
    /// Tests for PullRecordings.
    /// </summary>
    [TestClass]
    public class PullRecordingTests
    {
        /// <summary>
        /// Application setting name for the Twilio account SID secret name.
        /// </summary>
        public const string TwilioAccountSidSecretNameAppSettingName = "twilioAccountSidSecretName";

        /// <summary>
        /// Application setting name for the Twilio auth token secret name.
        /// </summary>
        public const string TwilioAuthTokenSecretNameAppSettingName = "twilioAuthTokenSecretName";

        /// <summary>
        /// Application setting name for the Azure Storage access key secret name.
        /// </summary>
        public const string StorageAccessKeySecretNameAppSettingName = "storageAccessKeySecretName";

        /// <summary>
        /// Application setting name for the Azure Storage read-only access key secret name.
        /// </summary>
        public const string StorageReadAccessKeySecretNameAppSettingName = "storageReadAccessKeySecretName";

        /// <summary>
        /// Application setting name with the expected blob container name.
        /// </summary>
        public const string ExpectedStorageContainerNameAppSettingName = "storageContainerName";

        /// <summary>
        /// Application setting name with the authorizing authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        /// <summary>
        /// Path to the Twilio composite recordings response template file.
        /// </summary>
        public const string RecordingCompositeTemplatePath = @".\Templates\Response\Twilio\Composite\RecordingCompositeTemplate.json";

        /// <summary>
        /// Key to inject the Twilio composite recordings responses.
        /// </summary>
        public const string RecordingCompositesKey = "recordingsComposite";

        /// <summary>
        /// Path to the Twilio recording response template file.
        /// </summary>
        public const string RecordingResponseTemplatePath = @".\Templates\Response\Twilio\RecordingResponseTemplate.json";

        /// <summary>
        /// The base Twilio API URL.
        /// </summary>
        public const string TwilioBaseUrl = "https://api.twilio.com";

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

            PullRecordingHttpTrigger.Container = containerBuilder.Build();
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
                new AzureBlobStorageMockModule(),
            });

            Assert.IsNotNull(PullRecordingHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(PullRecordingHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(PullRecordingHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(PullRecordingHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(PullRecordingHttpTrigger.Container.GetService<ITwilioCallWrapper>(), "Could not obtain ITwilioCallWrapper instance from DI container!");
            Assert.IsInstanceOfType(PullRecordingHttpTrigger.Container.GetService<ITwilioCallWrapper>(), typeof(TwilioCallWrapper), "Type of ITwilioCallWrapper should be TwilioCallWrapper!");
            Assert.IsNotNull(PullRecordingHttpTrigger.Container.GetService<IStorageClient>(), "Could not obtain IStorageClient instance from DI container!");
            Assert.IsInstanceOfType(PullRecordingHttpTrigger.Container.GetService<IStorageClient>(), typeof(AzureBlobStorageClientMock), "Type of IStorageClient should be AzureBlobStorageClientMock!");
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
                new AzureBlobStorageMockModule(),
            });

            Assert.IsNotNull(PullRecordingHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(PullRecordingHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(PullRecordingHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(PullRecordingHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(PullRecordingHttpTrigger.Container.GetService<ITwilioCallWrapper>(), "Could not obtain ITwilioCallWrapper instance from DI container!");
            Assert.IsInstanceOfType(PullRecordingHttpTrigger.Container.GetService<ITwilioCallWrapper>(), typeof(TwilioCallWrapperMock), "Type of ITwilioCallWrapper should be TwilioCallWrapperMock!");
            Assert.IsNotNull(PullRecordingHttpTrigger.Container.GetService<IStorageClient>(), "Could not obtain IStorageClient instance from DI container!");
            Assert.IsInstanceOfType(PullRecordingHttpTrigger.Container.GetService<IStorageClient>(), typeof(AzureBlobStorageClientMock), "Type of IStorageClient should be AzureBlobStorageClientMock!");
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
        /// <param name="excludeTwilioAccountSecret">Flag indicating whether to not add the Twilio account SID secret.</param>
        public void InitializeSecrets(bool excludeTwilioAccountSecret = false)
        {
            AzureKeyVaultSecretStoreMock secretStore = PullRecordingHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            if (!excludeTwilioAccountSecret)
            {
                Secret expectedTwilioAccountSidSecret = new Secret(_config[TwilioAccountSidSecretNameAppSettingName], TestHelper.CreateSecureString("mock-account-sid"));

                secretStore.RegisterExpectedSecret(expectedTwilioAccountSidSecret);
            }

            Secret expectedTwilioAuthTokenSecret = new Secret(_config[TwilioAuthTokenSecretNameAppSettingName], TestHelper.CreateSecureString("mock-auth-token"));

            secretStore.RegisterExpectedSecret(expectedTwilioAuthTokenSecret);

            Secret expectedStorageAccountKey = new Secret(_config[StorageAccessKeySecretNameAppSettingName], TestHelper.CreateSecureString("mock-storage-access-key"));

            secretStore.RegisterExpectedSecret(expectedStorageAccountKey);

            Secret expectedStorageAccountReadKey = new Secret(_config[StorageReadAccessKeySecretNameAppSettingName], TestHelper.CreateSecureString("mock-storage-access-read-key"));

            secretStore.RegisterExpectedSecret(expectedStorageAccountReadKey);
        }

        /// <summary>
        /// Initializes expected secret store exceptions.
        /// </summary>
        public void InitializeSecretsExceptions()
        {
            AzureKeyVaultSecretStoreMock secretStore = PullRecordingHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            secretStore.RegisterExpectedException(_config[TwilioAccountSidSecretNameAppSettingName], new SecretStoreException("TestException", null));
        }

        /// <summary>
        /// Initializes expected Twilio exceptions.
        /// </summary>
        /// <param name="exceptions">Dictionary of expected exceptions.</param>
        public void InitializeTwilioExceptions(Dictionary<string, Exception> exceptions)
        {
            TwilioCallWrapperMock twilioCallWrapper = PullRecordingHttpTrigger.Container.GetService<ITwilioCallWrapper>() as TwilioCallWrapperMock;

            foreach (KeyValuePair<string, Exception> ex in exceptions)
            {
                twilioCallWrapper.RegisterExpectedRequestException(ex.Key, ex.Value);
            }
        }

        /// <summary>
        /// Initializes expected storage client exceptions.
        /// </summary>
        /// <param name="exceptions">Dictionary of expected exceptions.</param>
        public void InitializeStorageClientExceptions(Dictionary<string, StorageClientException> exceptions)
        {
            AzureBlobStorageClientMock storageClient = PullRecordingHttpTrigger.Container.GetService<IStorageClient>() as AzureBlobStorageClientMock;

            foreach (KeyValuePair<string, StorageClientException> ex in exceptions)
            {
                storageClient.RegisterExpectedException(ex.Key, ex.Value);
            }
        }

        /// <summary>
        /// Ensure PullRecordingHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerSuccess()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedInputId = "A012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";
            string recordingSid = "RE10000000000000000000000000000001";
            string recordingUri = $"/2010-04-01/Accounts/ACXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX/Recordings/{recordingSid}.json";
            string expectedStorageContainerName = _config[ExpectedStorageContainerNameAppSettingName];
            long expectedBlobLength = 848300; // 848.3KB

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
                        { "recordingSid", recordingSid },
                        { "recordingUri", recordingUri },
                    },
                });

            HttpClientMock httpClient = PullRecordingHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.MessageHandler.RegisterExpectedRecordingResponse(expectedCallSid, "recordings", expectedResponse);

            httpClient.RegisterExpectedRequestMessage($"{TwilioBaseUrl}{recordingUri.Replace(".json", string.Empty)}", null);

            AzureKeyVaultSecretStoreMock secretStore = PullRecordingHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            Secret expectedStorageAccessKey = secretStore.GetSecretAsync(_config[StorageAccessKeySecretNameAppSettingName], _config[AuthorityAppSettingName]).Result;
            Secret expectedStorageReadAccessKey = secretStore.GetSecretAsync(_config[StorageReadAccessKeySecretNameAppSettingName], _config[AuthorityAppSettingName]).Result;

            string expectedDestinationPathPrefix = $"{expectedInputId}/file_"; // We will just check to make sure the remaining is a datetime
            string expectedFullPathPrefix = $"{AzureBlobStorageClientMock.AzureStorageEndpoint}{expectedStorageContainerName}/{expectedDestinationPathPrefix}";

            AzureBlobStorageClientMock blobClient = PullRecordingHttpTrigger.Container.GetService<IStorageClient>() as AzureBlobStorageClientMock;

            blobClient.RegisterExpectedRequestMessage(expectedDestinationPathPrefix, expectedStorageAccessKey.Value, expectedStorageContainerName, true);
            blobClient.RegisterExpectedResponseMessage(expectedDestinationPathPrefix, expectedBlobLength);

            HttpRequest request = CreateHttpPostRequest(expectedInputId, expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = PullRecordingHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsTrue(response.RecordingUri.StartsWith(expectedDestinationPathPrefix));
            Assert.AreEqual(expectedBlobLength, response.RecordingLength);
            Assert.IsTrue(response.FullRecordingUrl.StartsWith(expectedFullPathPrefix));
            Assert.IsTrue(response.FullRecordingUrl.EndsWith($"?{expectedStorageReadAccessKey.Value}"));

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure PullRecordingHttpTrigger.Run() implementation with missing callSid in request body runs correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerMissingCallSid()
        {
            InitializeWithNoTwilioMock();

            string expectedInputId = "A012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/

            HttpRequest request = CreateHttpPostRequest(expectedInputId, null);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = PullRecordingHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)badRequestResult.Value;

            Assert.IsNull(response.CallSid);
            Assert.IsNull(response.RecordingUri);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)PullRecordingErrorCode.RequestMissingCallSid, response.ErrorCode);
            Assert.AreEqual($"You must specify a value for the key '{PullRecordingRequest.CallSidKeyName}' in the request body!", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure PullRecordingHttpTrigger.Run() implementation with missing inputId in request body runs correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerMissingInputId()
        {
            InitializeWithNoTwilioMock();

            string expectedCallSid = "CA10000000000000000000000000000001";

            HttpRequest request = CreateHttpPostRequest(null, expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = PullRecordingHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)badRequestResult.Value;

            Assert.IsNull(response.CallSid);
            Assert.IsNull(response.RecordingUri);
            Assert.AreEqual(0, response.RecordingLength);
            Assert.IsNull(response.FullRecordingUrl);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)PullRecordingErrorCode.RequestMissingInputId, response.ErrorCode);
            Assert.AreEqual($"You must specify a value for the key '{PullRecordingRequest.InputIdKeyName}' in the request body!", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure PullRecordingHttpTrigger.Run() implementation with no Twilio call recordings runs correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerNoRecording()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedInputId = "A012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";

            string expectedResponseContent = TemplateManager.Load(RecordingResponseTemplatePath, new Dictionary<string, string>()
                {
                    { "pageSize", "0" },
                })
                .Replace($"{{{RecordingCompositesKey}}}", string.Empty);

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            HttpClientMock httpClient = PullRecordingHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            httpClient.MessageHandler.RegisterExpectedRecordingResponse(expectedCallSid, "recordings", expectedResponse);

            HttpRequest request = CreateHttpPostRequest(expectedInputId, expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = PullRecordingHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.RecordingUri);
            Assert.AreEqual(0, response.RecordingLength);
            Assert.IsNull(response.FullRecordingUrl);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioCallNoRecordings, response.ErrorCode);
            Assert.AreEqual("No recording", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure PullRecordingHttpTrigger.Run() implementation handles secret store exceptions correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerRunSecretStoreException()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets(true);

            string expectedInputId = "A012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";

            HttpRequest request = CreateHttpPostRequest(expectedInputId, expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            InitializeSecretsExceptions();

            IActionResult result = PullRecordingHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.SecretStoreGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.SecretStoreGenericFailureMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure PullRecordingHttpTrigger.Run() implementation handles Twilio authentication exceptions correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerRunTwilioAuthenticationException()
        {
            InitializeWithTwilioMock();
            InitializeSecrets(true);
            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { _config[GlobalConstants.TwilioAccountSidSecretNameAppSettingName], new TwilioEx.AuthenticationException("TestException") },
            });

            string expectedInputId = "A012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";

            HttpRequest request = CreateHttpPostRequest(expectedInputId, expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = PullRecordingHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioAuthenticationFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioAuthenticationFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure PullRecordingHttpTrigger.Run() implementation handles Twilio API connection exceptions correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerRunTwilioApiConnectionException()
        {
            InitializeWithTwilioMock();
            InitializeSecrets();

            string expectedInputId = "A012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { expectedCallSid, new TwilioEx.ApiConnectionException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest(expectedInputId, expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = PullRecordingHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioApiConnectionFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioApiConnectionFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure PullRecordingHttpTrigger.Run() implementation handles Twilio API request exceptions correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerRunTwilioApiRequestException()
        {
            InitializeWithTwilioMock();
            InitializeSecrets();

            string expectedInputId = "A012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { expectedCallSid, new TwilioEx.ApiException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest(expectedInputId, expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = PullRecordingHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioApiRequestFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioApiRequestFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure PullRecordingHttpTrigger.Run() implementation handles Twilio REST request exceptions correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerRunTwilioRestRequestException()
        {
            InitializeWithTwilioMock();
            InitializeSecrets();

            string expectedInputId = "A012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { expectedCallSid, new TwilioEx.RestException() },
            });

            HttpRequest request = CreateHttpPostRequest(expectedInputId, expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = PullRecordingHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioRestCallFailed, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioRestCallFailedMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure PullRecordingHttpTrigger.Run() implementation handles Twilio generic exceptions correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerRunTwilioGenericException()
        {
            InitializeWithTwilioMock();
            InitializeSecrets();

            string expectedInputId = "A012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";

            InitializeTwilioExceptions(new Dictionary<string, Exception>()
            {
                { expectedCallSid, new TwilioEx.TwilioException("TestException") },
            });

            HttpRequest request = CreateHttpPostRequest(expectedInputId, expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = PullRecordingHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.TwilioGenericFailureMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure PullRecordingHttpTrigger.Run() implementation handles storage client exceptions correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerRunStorageClientException()
        {
            InitializeWithNoTwilioMock();
            InitializeSecrets();

            string expectedInputId = "A012345678"; // Example pulled from https://citizenpath.com/faq/find-alien-registration-number/
            string expectedCallSid = "CA10000000000000000000000000000001";
            string recordingSid = "RE10000000000000000000000000000001";
            string recordingUri = $"/2010-04-01/Accounts/ACXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX/Recordings/{recordingSid}.json";
            string expectedStorageContainerName = _config[ExpectedStorageContainerNameAppSettingName];

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
                        { "recordingSid", recordingSid },
                        { "recordingUri", recordingUri },
                    },
                });

            HttpClientMock httpClient = PullRecordingHttpTrigger.Container.GetService<IHttpClientWrapper>() as HttpClientMock;

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseMessage(HttpStatusCode.OK, expectedResponseContent);

            httpClient.MessageHandler.RegisterExpectedRecordingResponse(expectedCallSid, "recordings", expectedResponse);

            httpClient.RegisterExpectedRequestMessage($"{TwilioBaseUrl}{recordingUri.Replace(".json", string.Empty)}", null);

            string expectedDestinationPathPrefix = $"{expectedInputId}/file_"; // We will just check to make sure the remaining is a datetime

            AzureBlobStorageClientMock blobClient = PullRecordingHttpTrigger.Container.GetService<IStorageClient>() as AzureBlobStorageClientMock;

            InitializeStorageClientExceptions(new Dictionary<string, StorageClientException>()
            {
                { expectedDestinationPathPrefix, new StorageClientException("TestException", null) },
            });

            HttpRequest request = CreateHttpPostRequest(expectedInputId, expectedCallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = PullRecordingHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.RecordingUri);
            Assert.AreEqual(0, response.RecordingLength);
            Assert.IsNull(response.FullRecordingUrl);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.StorageClientGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.StorageClientGenericFailureMessage, response.ErrorDetails);
        }

        private static HttpRequest CreateHttpPostRequest(string inputId, string callSid)
        {
            HttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = new DefaultHttpRequest(httpContext);

            PullRecordingRequest requestObj = new PullRecordingRequest()
            {
                InputId = inputId,
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
