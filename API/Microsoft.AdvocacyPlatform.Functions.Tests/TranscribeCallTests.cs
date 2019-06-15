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
    /// Tests for TranscribeCall.
    /// </summary>
    [TestClass]
    public class TranscribeCallTests
    {
        /// <summary>
        /// Application setting name for Azure Speech Cognitive Services API key secret name.
        /// </summary>
        public const string ApiKeySecretNameAppSettingName = "speechApiKeySecretName";

        /// <summary>
        /// Application setting name for Azure Storage access key secret name.
        /// </summary>
        public const string StorageAccessKeySecretNameAppSettingName = "storageAccessKeySecretName";

        /// <summary>
        /// Application setting name for Azure Storage read-access key secret name.
        /// </summary>
        public const string StorageAccessConnectionStringAppSettingName = "storageAccessConnectionString";

        /// <summary>
        /// Application setting name for the expected blob container name.
        /// </summary>
        public const string ExpectedStorageContainerNameAppSettingName = "storageContainerName";

        /// <summary>
        /// Application setting name for the authorizing authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        /// <summary>
        /// Application setting name with the expected Azure Speech Cognitive Services API region.
        /// </summary>
        public const string ExpectedApiRegionAppSettingName = "speechApiRegion";

        /// <summary>
        /// The expected Azure Speech Cognitive Services API key.
        /// </summary>
        public const string ExpectedApiKey = "mock-speech-api-key";

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

            TranscribeCallHttpTrigger.Container = containerBuilder.Build();
        }

        /// <summary>
        /// Initializes test.
        /// </summary>
        [TestInitialize]
        public void InitializeTests()
        {
            InitializeServiceProvider(new List<Module>()
            {
                new HttpClientMockModule(),
                new AzureKeyVaultSecretStoreMockModule(),
                new AzureBlobStorageMockModule(),
                new AzureTranscriberMockModule(),
            });

            Assert.IsNotNull(TranscribeCallHttpTrigger.Container.GetService<IHttpClientWrapper>(), "Could not obtain IHttpClientWrapper instance from DI container!");
            Assert.IsInstanceOfType(TranscribeCallHttpTrigger.Container.GetService<IHttpClientWrapper>(), typeof(HttpClientMock), "Type of IHttpClientWrapper should be HttpClientMock!");
            Assert.IsNotNull(TranscribeCallHttpTrigger.Container.GetService<ISecretStore>(), "Could not obtain ISecretStore instance from DI container!");
            Assert.IsInstanceOfType(TranscribeCallHttpTrigger.Container.GetService<ISecretStore>(), typeof(AzureKeyVaultSecretStoreMock), "Type of ISecretStore should be AzureKeyVaultSecretStoreMock!");
            Assert.IsNotNull(TranscribeCallHttpTrigger.Container.GetService<IStorageClient>(), "Could not obtain ITranscriber instance from DI container!");
            Assert.IsInstanceOfType(TranscribeCallHttpTrigger.Container.GetService<IStorageClient>(), typeof(AzureBlobStorageClientMock), "Type of IStorageClient should be AzureBlobStorageClientMock!");
            Assert.IsNotNull(TranscribeCallHttpTrigger.Container.GetService<ITranscriber>(), "Could not obtain ITranscriber instance from DI container!");
            Assert.IsInstanceOfType(TranscribeCallHttpTrigger.Container.GetService<ITranscriber>(), typeof(AzureTranscriberMock), "Type of ITranscriber should be AzureTranscriberMock!");

            _config = TestHelper.GetConfiguration();
        }

        /// <summary>
        /// Initializes mock secret store.
        /// </summary>
        public void InitializeSecrets()
        {
            AzureKeyVaultSecretStoreMock secretStore = TranscribeCallHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            Secret expectedSpeechApiKey = new Secret(_config[ApiKeySecretNameAppSettingName], TestHelper.CreateSecureString(ExpectedApiKey));

            secretStore.RegisterExpectedSecret(expectedSpeechApiKey);

            Secret expectedStorageAccessKey = new Secret(_config[StorageAccessKeySecretNameAppSettingName], TestHelper.CreateSecureString("mock-storage-access-key"));

            secretStore.RegisterExpectedSecret(expectedStorageAccessKey);
        }

        /// <summary>
        /// Initializes expected secret store exceptions.
        /// </summary>
        public void InitializeSecretsExceptions()
        {
            AzureKeyVaultSecretStoreMock secretStore = TranscribeCallHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            secretStore.RegisterExpectedException(_config[ApiKeySecretNameAppSettingName], new SecretStoreException("TestException", null));
        }

        /// <summary>
        /// Initializes expected storage client exceptions.
        /// </summary>
        /// <param name="exceptions">Dictionary of expected exceptions.</param>
        public void InitializeStorageClientExceptions(Dictionary<string, StorageClientException> exceptions)
        {
            AzureBlobStorageClientMock storageClient = TranscribeCallHttpTrigger.Container.GetService<IStorageClient>() as AzureBlobStorageClientMock;

            foreach (KeyValuePair<string, StorageClientException> ex in exceptions)
            {
                storageClient.RegisterExpectedException(ex.Key, ex.Value);
            }
        }

        /// <summary>
        /// Initializes expected transcriber exceptions.
        /// </summary>
        /// <param name="exceptions">Dictionary of expected exceptions.</param>
        public void InitializeTranscriberExceptions(Dictionary<string, TranscriberException> exceptions)
        {
            AzureTranscriberMock transcriber = TranscribeCallHttpTrigger.Container.GetService<ITranscriber>() as AzureTranscriberMock;

            foreach (KeyValuePair<string, TranscriberException> ex in exceptions)
            {
                transcriber.RegisterExpectedException(ex.Key, ex.Value);
            }
        }

        /// <summary>
        /// Ensure TranscribeCallHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void TranscribeCallHttpTriggerSuccess()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedRecordingUri = "https://fakestorage/recordings/A012345678.wav";
            string expectedStorageContainerName = _config[ExpectedStorageContainerNameAppSettingName];
            string expectedRegion = _config[ExpectedApiRegionAppSettingName];
            string expectedTranscript = "your next Master hearing date January 19th 2018 at 3 p.m. for Gymboree";

            AzureKeyVaultSecretStoreMock secretStore = TranscribeCallHttpTrigger.Container.GetService<ISecretStore>() as AzureKeyVaultSecretStoreMock;

            string storageAccessConnectionString = _config[StorageAccessConnectionStringAppSettingName];

            Secret storageAccessKey = secretStore.GetSecretAsync(_config[StorageAccessKeySecretNameAppSettingName], _config[AuthorityAppSettingName]).Result;

            Secret fullStorageAccessConnectionString = Utils.GetFullStorageConnectionString(storageAccessConnectionString, storageAccessKey);

            AzureBlobStorageClientMock storageClient = TranscribeCallHttpTrigger.Container.GetService<IStorageClient>() as AzureBlobStorageClientMock;

            storageClient.RegisterExpectedRequestMessage(expectedRecordingUri, fullStorageAccessConnectionString.Value, _config[ExpectedStorageContainerNameAppSettingName], false);

            AzureTranscriberMock transcriber = TranscribeCallHttpTrigger.Container.GetService<ITranscriber>() as AzureTranscriberMock;

            transcriber.RegisterExpectedRequestMessage(expectedRecordingUri, ExpectedApiKey, expectedRegion);
            transcriber.RegisterExpectedResponseMessage(expectedRecordingUri, expectedTranscript);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedRecordingUri);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = TranscribeCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(TranscribeCallResponse));

            TranscribeCallResponse response = (TranscribeCallResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.AreEqual(expectedTranscript, response.Text);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure TranscribeCallHttpTrigger.Run() implementation with missing callSid in request body runs correctly.
        /// </summary>
        [TestMethod]
        public void TranscribeCallHttpTriggerMissingCallSid()
        {
            string expectedRecordingUri = "https://fakestorage/recordings/A012345678.wav";

            HttpRequest request = CreateHttpPostRequest(null, expectedRecordingUri);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = TranscribeCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(TranscribeCallResponse));

            TranscribeCallResponse response = (TranscribeCallResponse)badRequestResult.Value;

            Assert.IsNull(response.CallSid);
            Assert.IsNull(response.Text);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)TranscribeCallErrorCode.RequestMissingCallSid, response.ErrorCode);
            Assert.AreEqual($"You must specify a value for the key '{TranscribeCallRequest.CallSidKeyName}' in the request body!", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure TranscribeCallHttpTrigger.Run() implementation with missing recordingUri in request body runs correctly.
        /// </summary>
        [TestMethod]
        public void TranscribeCallHttpTriggerMissingRecordingUri()
        {
            string expectedCallSid = "CA10000000000000000000000000000001";

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, null);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = TranscribeCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult badRequestResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(TranscribeCallResponse));

            TranscribeCallResponse response = (TranscribeCallResponse)badRequestResult.Value;

            Assert.IsNull(response.CallSid);
            Assert.IsNull(response.Text);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)TranscribeCallErrorCode.RequestMissingRecordingUri, response.ErrorCode);
            Assert.AreEqual($"You must specify a value for the key '{TranscribeCallRequest.RecordingUriKeyName}' in the request body!", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure TranscribeCallHttpTrigger.Run() implementation handles exceptions correctly.
        /// </summary>
        [TestMethod]
        public void TranscribeCallHttpTriggerRunException()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedRecordingUri = "https://fakestorage/recordings/A012345678.wav";

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedRecordingUri);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = TranscribeCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            BadRequestObjectResult okResult = (BadRequestObjectResult)result;

            Assert.AreEqual(400, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(TranscribeCallResponse));

            TranscribeCallResponse response = (TranscribeCallResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.Text);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.BadRequest, response.ErrorCode);
            Assert.AreEqual("Bad request.", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure TranscribeCallHttpTrigger.Run() implementation handles secret store exceptions correctly.
        /// </summary>
        [TestMethod]
        public void TranscribeCallHttpTriggerRunSecretStoreException()
        {
            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedRecordingUri = "https://fakestorage/recordings/A012345678.wav";

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedRecordingUri);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            InitializeSecretsExceptions();

            IActionResult result = TranscribeCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(TranscribeCallResponse));

            TranscribeCallResponse response = (TranscribeCallResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.SecretStoreGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.SecretStoreGenericFailureMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure TranscribeCallHttpTrigger.Run() implementation handles storage client exceptions correctly.
        /// </summary>
        [TestMethod]
        public void TranscribeCallHttpTriggerRunStorageClientException()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedRecordingUri = "https://fakestorage/recordings/A012345678.wav";
            string expectedRegion = _config[ExpectedApiRegionAppSettingName];

            InitializeStorageClientExceptions(new Dictionary<string, StorageClientException>()
            {
                { expectedRecordingUri, new StorageClientException("TestException", null) },
            });

            AzureTranscriberMock transcriber = TranscribeCallHttpTrigger.Container.GetService<ITranscriber>() as AzureTranscriberMock;

            transcriber.RegisterExpectedRequestMessage(expectedRecordingUri, ExpectedApiKey, expectedRegion);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedRecordingUri);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = TranscribeCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(TranscribeCallResponse));

            TranscribeCallResponse response = (TranscribeCallResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.Text);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.StorageClientGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.StorageClientGenericFailureMessage, response.ErrorDetails);
        }

        /// <summary>
        /// Ensure TranscribeCallHttpTrigger.Run() implementation handles transcriber exceptions correctly.
        /// </summary>
        [TestMethod]
        public void TranscribeCallHttpTriggerRunTranscriberException()
        {
            InitializeSecrets();

            string expectedCallSid = "CA10000000000000000000000000000001";
            string expectedRecordingUri = "https://fakestorage/recordings/A012345678.wav";
            string expectedRegion = _config[ExpectedApiRegionAppSettingName];

            InitializeStorageClientExceptions(new Dictionary<string, StorageClientException>()
            {
                { expectedRecordingUri, new StorageClientException("TestException", null) },
            });

            AzureTranscriberMock transcriber = TranscribeCallHttpTrigger.Container.GetService<ITranscriber>() as AzureTranscriberMock;

            transcriber.RegisterExpectedRequestMessage(expectedRecordingUri, ExpectedApiKey, expectedRegion);

            HttpRequest request = CreateHttpPostRequest(expectedCallSid, expectedRecordingUri);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = TranscribeCallHttpTrigger.Run(request, Log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(TranscribeCallResponse));

            TranscribeCallResponse response = (TranscribeCallResponse)okResult.Value;

            Assert.AreEqual(expectedCallSid, response.CallSid);
            Assert.IsNull(response.Text);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.StorageClientGenericFailure, response.ErrorCode);
            Assert.AreEqual(CommonErrorMessage.StorageClientGenericFailureMessage, response.ErrorDetails);
        }

        private static HttpRequest CreateHttpPostRequest(string callSid, string recordingUrl)
        {
            HttpContext httpContext = new DefaultHttpContext();
            HttpRequest request = new DefaultHttpRequest(httpContext);

            TranscribeCallRequest requestObj = new TranscribeCallRequest()
            {
                CallSid = callSid,
                RecordingUri = recordingUrl,
            };

            Stream contentBytes = new MemoryStream(
                Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(requestObj)));

            request.Body = contentBytes;

            return request;
        }
    }
}
