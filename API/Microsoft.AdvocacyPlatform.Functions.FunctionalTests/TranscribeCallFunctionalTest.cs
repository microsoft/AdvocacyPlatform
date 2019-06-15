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
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using TH = System.Threading;

    /// <summary>
    /// Functional test for TranscribeCallHttpTrigger.
    /// </summary>
    [TestClass]
    public class TranscribeCallFunctionalTest
    {
        /// <summary>
        /// Internal logging.
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
        /// Initialize tests.
        /// </summary>
        [TestInitialize]
        public void InitializeTests()
        {
            Tuple<TestConfiguration, ConcurrentTestConfiguration> configs = TestHelper.InitializeTest(_log);

            _config = configs.Item1;
            _concurrentConfig = configs.Item2;
        }

        /// <summary>
        /// Ensures TranscribeCallHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void TranscribeCallRun()
        {
            HttpRequest request = CreateHttpPostRequest(_config.CallSid, _config.RecordingUri);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = TranscribeCallHttpTrigger.Run(request, _log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(TranscribeCallResponse));

            TranscribeCallResponse response = (TranscribeCallResponse)okResult.Value;

            Assert.AreEqual(_config.CallSid, response.CallSid);
            Assert.IsFalse(string.IsNullOrWhiteSpace(response.Text));

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);

            _log.LogInformation("Writing returned transcription to test configuration...");
            _config.Transcript = response.Text;
            _config.Save(GlobalTestConstants.TestConfigurationFilePath);
        }

        /// <summary>
        /// Ensures TranscribeCallHttpTrigger.Run() implementation runs correctly for concurrent executions.
        /// </summary>
        [TestMethod]
        public void TranscribeCallImplementationOpenSpeechRepositoryConcurrentRuns()
        {
            ConcurrentTestConfiguration testConfiguration = ConcurrentTestConfiguration.Load(@".\TestConfigurations\TranscribeCall\concurrent\testconfig_test1.json");

            Task[] threads = new Task[testConfiguration.NumberOfThreads];

            for (int i = 0; i < testConfiguration.NumberOfThreads; i++)
            {
                threads[i] = Task.Run(
                    new Action(
                        () => TranscribeCallRunnerLocal(testConfiguration.TestConfigurationQueue)));
            }

            _log.LogInformation("Waiting on threads...");
            Task.WaitAll(threads);
            _log.LogInformation("All threads completed...");
        }

        /// <summary>
        /// Runner for ensuring TranscribeCallHttpTrigger.Run() implementation runs correctly for concurrent executions.
        /// </summary>
        [TestMethod]
        public void TranscribeCallHttpTriggerOpenSpeechRepositoryConcurrentRuns()
        {
            ConcurrentTestConfiguration testConfiguration = ConcurrentTestConfiguration.Load(@".\TestConfigurations\TranscribeCall\multiThreaded\testconfig_test1.json");
            FunctionSettings functionSettings = FunctionSettings.Load(@".\functionSettings.json");

            using (HttpClient httpClient = new HttpClient())
            {
                string accessToken = AzureADHelper.GetAccessTokenAsync(functionSettings.Authority, functionSettings.ApplicationId, functionSettings.ClientSecret, functionSettings.ResourceId).Result;

                if (accessToken == null)
                {
                    throw new Exception("Could not obtain access token!");
                }

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                Task[] threads = new Task[testConfiguration.NumberOfThreads];

                for (int i = 0; i < testConfiguration.NumberOfThreads; i++)
                {
                    threads[i] = Task.Run(
                        new Action(
                            () => TranscribeCallRunnerAsync(httpClient, functionSettings, testConfiguration.TestConfigurationQueue).Wait()));
                }

                _log.LogInformation("Waiting on threads...");
                Task.WaitAll(threads);
                _log.LogInformation("All threads completed...");
            }
        }

        /// <summary>
        /// Ensures TranscribeCallHttpTrigger.Run() implementation runs correctly for concurrent executions.
        /// </summary>
        [TestMethod]
        public void TranscribeCallImplementationConcurrentRuns()
        {
            Task[] threads = new Task[_concurrentConfig.NumberOfThreads];

            for (int i = 0; i < _concurrentConfig.NumberOfThreads; i++)
            {
                threads[i] = Task.Run(
                    new Action(
                        () => TranscribeCallRunnerLocal(_concurrentConfig.TestConfigurationQueue)));
            }

            _log.LogInformation("Waiting on threads...");
            Task.WaitAll(threads);
            _log.LogInformation("All threads completed...");
        }

        /// <summary>
        /// Runner for ensuring TranscribeCallHttpTrigger.Run() implementation runs correctly for concurrent executions.
        /// </summary>
        /// <param name="queue">The queue of tests to runs.</param>
        public void TranscribeCallRunnerLocal(ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpRequest request = CreateHttpPostRequest(nextTestConfiguration.CallSid, nextTestConfiguration.RecordingUri);

                    ExecutionContext context = new ExecutionContext()
                    {
                        FunctionAppDirectory = Directory.GetCurrentDirectory(),
                    };

                    IActionResult result = TranscribeCallHttpTrigger.Run(request, _log, context).Result;

                    Assert.IsInstanceOfType(result, typeof(OkObjectResult));

                    OkObjectResult okResult = (OkObjectResult)result;

                    Assert.AreEqual(200, okResult.StatusCode);
                    Assert.IsInstanceOfType(okResult.Value, typeof(TranscribeCallResponse));

                    TranscribeCallResponse response = (TranscribeCallResponse)okResult.Value;

                    Assert.AreEqual(nextTestConfiguration.CallSid, response.CallSid);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(response.Text));

                    Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
                    Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

                    Assert.IsFalse(response.HasError);
                    Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
                    Assert.IsNull(response.ErrorDetails);

                    _log.LogInformation($"Transcription returned ({nextTestConfiguration.CallSid}):");
                    _log.LogInformation(response.Text);

                    nextTestConfiguration.Transcript = response.Text;
                }

                TH.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Ensures TranscribeCallHttpTrigger runs correctly for concurrent executions against a remote Azure Function.
        /// </summary>
        [TestMethod]
        public void TranscribeCallHttpTriggerConcurrentRuns()
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
                            () => TranscribeCallRunnerAsync(httpClient, functionSettings, _concurrentConfig.TestConfigurationQueue).Wait()));
                }

                _log.LogInformation("Waiting on threads...");
                Task.WaitAll(threads);
                _log.LogInformation("All threads completed...");
            }

            _concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
        }

        /// <summary>
        /// Runner for ensuring TranscribeCallHttpTrigger runs correctly for concurrent executions against a remote Azure Function.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance to use.</param>
        /// <param name="functionSettings">Configuration information required to make calls to the Azure Function.</param>
        /// <param name="queue">The queue of tests to run.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task TranscribeCallRunnerAsync(HttpClient httpClient, FunctionSettings functionSettings, ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpContent requestContent = CreateHttpPostContent(nextTestConfiguration.CallSid, nextTestConfiguration.RecordingUri);

                    HttpResponseMessage httpResponse = await httpClient.PostAsync(functionSettings.TranscribeCallUrl, requestContent);

                    string responseContent = await httpResponse.Content.ReadAsStringAsync();

                    TranscribeCallResponse response = JsonConvert.DeserializeObject<TranscribeCallResponse>(responseContent);

                    Assert.AreEqual(nextTestConfiguration.CallSid, response.CallSid);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(response.Text));

                    Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
                    Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

                    Assert.IsFalse(response.HasError);
                    Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
                    Assert.IsNull(response.ErrorDetails);

                    _log.LogInformation($"Transcription returned ({nextTestConfiguration.CallSid}):");
                    _log.LogInformation(response.Text);

                    nextTestConfiguration.Transcript = response.Text;
                }

                TH.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Creates an HTTP POST request for TranscribeCall.
        /// </summary>
        /// <param name="callSid">The callSid to set in the request body.</param>
        /// <param name="recordingUrl">The recordingUrl to set in the request body.</param>
        /// <returns>The HTTP request.</returns>
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

        /// <summary>
        /// Creates content for a request to TranscribeCall.
        /// </summary>
        /// <param name="callSid">The callSid to set in the content.</param>
        /// <param name="recordingUrl">The recordingUrl to set in the content.</param>
        /// <returns>The content.</returns>
        private static HttpContent CreateHttpPostContent(string callSid, string recordingUrl)
        {
            TranscribeCallRequest requestObj = new TranscribeCallRequest()
            {
                CallSid = callSid,
                RecordingUri = recordingUrl,
            };

            return new StringContent(
                JsonConvert.SerializeObject(requestObj));
        }
    }
}
