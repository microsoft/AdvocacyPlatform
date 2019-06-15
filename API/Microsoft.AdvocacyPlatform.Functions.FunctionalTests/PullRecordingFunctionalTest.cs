// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.FunctionalTests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AdvocacyPlatform.Clients;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.AdvocacyPlatform.Functions.Module;
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
    using TH = System.Threading;
    using TwilioHttp = Twilio.Http;

    /// <summary>
    /// Functional test for PullRecordingHttpTrigger.
    /// </summary>
    [TestClass]
    public class PullRecordingFunctionalTest
    {
        /// <summary>
        /// Twilio base URL for REST calls.
        /// </summary>
        public const string TwilioBaseUrl = "https://api.twilio.com";

        /// <summary>
        /// Name of app setting with data store connection string.
        /// </summary>
        public const string StorageAccessConnectionStringAppSettingName = "storageAccessConnectionString";

        /// <summary>
        /// Name of app setting with secret identifier for storage read access key.
        /// </summary>
        public const string StorageReadAccessKeySecretNameAppSettingName = "storageReadAccessKeySecretName";

        /// <summary>
        /// Name of app setting with container name.
        /// </summary>
        public const string StorageContainerNameAppSettingName = "storageContainerName";

        /// <summary>
        /// Name of app setting for URL of token issuing authority.
        /// </summary>
        public const string AuthorityAppSettingName = "authority";

        /// <summary>
        /// Function configuration.
        /// </summary>
        private IConfiguration _functionConfig;

        /// <summary>
        /// Internal logger.
        /// </summary>
        private ConsoleLogger _log = new ConsoleLogger();

        /// <summary>
        /// Test configuration.
        /// </summary>
        private TestConfiguration _config;

        /// <summary>
        /// Test configuration for concurrent executions.
        /// </summary>
        private ConcurrentTestConfiguration _concurrentConfig;

        /// <summary>
        /// Formats connection string with data store access key.
        /// </summary>
        /// <param name="connectionString">Partial connection string.</param>
        /// <returns>The full connection string.</returns>
        public static string GetBlobEndpoint(string connectionString)
        {
            string[] connectionStringParts = connectionString.Split(new char[] { ';' });

            foreach (string connectionStringPart in connectionStringParts)
            {
                if (connectionStringPart.Contains("BlobEndpoint", System.StringComparison.OrdinalIgnoreCase))
                {
                    string[] kvp = connectionStringPart.Split(new char[] { '=' });

                    return kvp[1];
                }
            }

            return null;
        }

        /// <summary>
        /// Initialize tests.
        /// </summary>
        [TestInitialize]
        public void InitializeTests()
        {
            Tuple<TestConfiguration, ConcurrentTestConfiguration> configs = TestHelper.InitializeTest(_log);

            _config = configs.Item1;
            _concurrentConfig = configs.Item2;

            _functionConfig = TestHelper.GetConfiguration();
        }

        /// <summary>
        /// Ensures PullRecordingHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerRun()
        {
            HttpRequest request = CreateHttpPostRequest(_config.InputId, _config.CallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            string expectedStorageContainerName = _functionConfig[StorageContainerNameAppSettingName];
            string expectedDestinationPathPrefix = $"{_config.InputId}/file_";
            string expectedBlobEndpoint = GetBlobEndpoint(_functionConfig[StorageAccessConnectionStringAppSettingName]);
            string expectedFullPathPrefix = $"{expectedBlobEndpoint}{expectedStorageContainerName}/{expectedDestinationPathPrefix}";

            ISecretStore secretStore = PullRecordingHttpTrigger.Container.GetService<ISecretStore>();

            Secret expectedStorageReadAccessKey = secretStore.GetSecretAsync(_functionConfig[StorageReadAccessKeySecretNameAppSettingName], _functionConfig[AuthorityAppSettingName]).Result;

            IActionResult result = PullRecordingHttpTrigger.Run(request, _log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(PullRecordingResponse));

            PullRecordingResponse response = (PullRecordingResponse)okResult.Value;

            Assert.AreEqual(_config.CallSid, response.CallSid);
            Assert.IsNotNull(response.RecordingUri);
            Assert.IsTrue(response.RecordingUri.StartsWith(expectedDestinationPathPrefix));
            Assert.IsTrue(response.FullRecordingUrl.StartsWith(expectedFullPathPrefix));
            Assert.IsTrue(response.FullRecordingUrl.EndsWith($"?{expectedStorageReadAccessKey.Value}"));
            Assert.IsTrue(response.RecordingLength > 600000); // Recording size should be ~800KB
            Assert.IsNotNull(response.FullRecordingUrl);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);

            _log.LogInformation("Writing returned recording URI to test configuration...");
            _config.RecordingUri = response.RecordingUri;
            _config.Save(GlobalTestConstants.TestConfigurationFilePath);
        }

        /// <summary>
        /// Ensures PullRecordingHttpTrigger.Run() implementation runs correctly for concurrent executions.
        /// </summary>
        [TestMethod]
        public void PullRecordingImplementationConcurrentRuns()
        {
            Task[] threads = new Task[_concurrentConfig.NumberOfThreads];

            for (int i = 0; i < _concurrentConfig.NumberOfThreads; i++)
            {
                threads[i] = Task.Run(
                    new Action(
                        () => PullRecordingRunnerLocal(_concurrentConfig.TestConfigurationQueue)));
            }

            _log.LogInformation("Waiting on threads...");
            Task.WaitAll(threads);
            _log.LogInformation("All threads completed...");
        }

        /// <summary>
        /// Runner for ensuring PullRecordingHttpTrigger.Run() implementation runs correctly for concurrent executions.
        /// </summary>
        /// <param name="queue">The queue of tests to run.</param>
        public void PullRecordingRunnerLocal(ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpRequest request = CreateHttpPostRequest(nextTestConfiguration.InputId, nextTestConfiguration.CallSid);

                    ExecutionContext context = new ExecutionContext()
                    {
                        FunctionAppDirectory = Directory.GetCurrentDirectory(),
                    };

                    IActionResult result = PullRecordingHttpTrigger.Run(request, _log, context).Result;

                    Assert.IsInstanceOfType(result, typeof(OkObjectResult));

                    OkObjectResult okResult = (OkObjectResult)result;

                    Assert.AreEqual(200, okResult.StatusCode);
                    Assert.IsInstanceOfType(okResult.Value, typeof(PullRecordingResponse));

                    PullRecordingResponse response = (PullRecordingResponse)okResult.Value;

                    Assert.AreEqual(nextTestConfiguration.CallSid, response.CallSid);
                    Assert.IsNotNull(response.RecordingUri);

                    // Assert.IsTrue(response.RecordingLength > 600000); // Recording size should be ~800KB
                    Assert.IsNotNull(response.FullRecordingUrl);

                    Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
                    Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

                    Assert.IsFalse(response.HasError);
                    Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
                    Assert.IsNull(response.ErrorDetails);

                    _log.LogInformation("Writing returned recording uri to test configuration...");
                    nextTestConfiguration.RecordingUri = response.RecordingUri;
                }

                TH.Thread.Sleep(2000);
            }

            _concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
        }

        /// <summary>
        /// Ensures PullRecordingHttpTrigger runs correctly for concurrent executions against a remote Azure Function.
        /// </summary>
        [TestMethod]
        public void PullRecordingHttpTriggerConcurrentRuns()
        {
            FunctionSettings functionSettings = FunctionSettings.Load(@".\functionSettings.json");

            using (HttpClient httpClient = new HttpClient())
            {
                string accessToken = AzureADHelper.GetAccessTokenAsync(functionSettings.Authority, functionSettings.ApplicationId, functionSettings.ClientSecret, functionSettings.ResourceId).Result;

                if (accessToken == null)
                {
                    throw new Exception("Could not obtain access token!");
                }

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                Task[] threads = new Task[_concurrentConfig.NumberOfThreads];

                for (int i = 0; i < _concurrentConfig.NumberOfThreads; i++)
                {
                    threads[i] = Task.Run(
                        new Action(
                            () => PullRecordingRunnerAsync(httpClient, functionSettings, _concurrentConfig.TestConfigurationQueue).Wait()));
                }

                _log.LogInformation("Waiting on threads...");
                Task.WaitAll(threads);
                _log.LogInformation("All threads completed...");
            }

            _concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
        }

        /// <summary>
        /// Runner to ensure PullRecordingHttpTrigger runs correctly for concurrent executions against a remote Azure Function.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance to use.</param>
        /// <param name="functionSettings">Configuration information required to make calls to the Azure Function.</param>
        /// <param name="queue">The queue of tests to run.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task PullRecordingRunnerAsync(HttpClient httpClient, FunctionSettings functionSettings, ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpContent requestContent = CreateHttpPostContent(nextTestConfiguration.InputId, nextTestConfiguration.CallSid);

                    HttpResponseMessage httpResponse = await httpClient.PostAsync(functionSettings.PullRecordingUrl, requestContent);

                    string responseContent = await httpResponse.Content.ReadAsStringAsync();

                    PullRecordingResponse response = JsonConvert.DeserializeObject<PullRecordingResponse>(responseContent);

                    Assert.AreEqual(nextTestConfiguration.CallSid, response.CallSid);
                    Assert.IsNotNull(response.RecordingUri);

                    // Assert.IsTrue(response.RecordingLength > 600000); // Recording size should be ~800KB
                    Assert.IsNotNull(response.FullRecordingUrl);

                    Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
                    Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

                    Assert.IsFalse(response.HasError);
                    Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
                    Assert.IsNull(response.ErrorDetails);

                    _log.LogInformation("Writing returned recording uri to test configuration...");
                    nextTestConfiguration.RecordingUri = response.RecordingUri;
                }

                TH.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Creates an HTTP POST request for PullRecording.
        /// </summary>
        /// <param name="inputId">The inputId to set in the request body.</param>
        /// <param name="callSid">The callSid to set in the request body.</param>
        /// <returns>The HTTP request.</returns>
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

        /// <summary>
        /// Creates content for a request to PullRecording.
        /// </summary>
        /// <param name="inputId">The inputId to set in the content.</param>
        /// <param name="callSid">The callSid to set in the content.</param>
        /// <returns>The content.</returns>
        private static HttpContent CreateHttpPostContent(string inputId, string callSid)
        {
            PullRecordingRequest requestObj = new PullRecordingRequest()
            {
                InputId = inputId,
                CallSid = callSid,
            };

            return new StringContent(
                JsonConvert.SerializeObject(requestObj));
        }
    }
}
