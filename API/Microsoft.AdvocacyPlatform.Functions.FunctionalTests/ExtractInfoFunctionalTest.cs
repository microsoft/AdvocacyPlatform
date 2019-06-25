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
    /// Functional test for ExtractInfoHttpTrigger.
    /// </summary>
    [TestClass]
    public class ExtractInfoFunctionalTest
    {
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
        /// Ensure ExtractInfoHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerRun()
        {
            HttpRequest request = CreateHttpPostRequest(_config.CallSid, _config.Transcript);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = ExtractInfoHttpTrigger.Run(request, _log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

            ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

            Assert.AreEqual(_config.CallSid, response.CallSid);
            Assert.IsNotNull(response.Data);
            Assert.AreEqual(_config.Transcript.Length < 500 ? _config.Transcript : _config.Transcript.Substring(0, 500), response.Data.Transcription);

            if (_config.ExpectedEntities.Contains("Dates"))
            {
                Assert.IsNotNull(response.Data.Dates);
                Assert.IsTrue(response.Data.Dates.Count > 0);
            }

            if (_config.ExpectedEntities.Contains("Location"))
            {
                Assert.IsNotNull(response.Data.Location);
            }

            if (_config.ExpectedEntities.Contains("Person"))
            {
                Assert.IsNotNull(response.Data.Person);
            }

            if (_config.ExpectedAdditionalEntities != null
                && _config.ExpectedAdditionalEntities.Count > 0)
            {
                foreach (KeyValuePair<string, string> expectedAdditionalEntity in _config.ExpectedAdditionalEntities)
                {
                    Assert.IsTrue(
                        response
                            .Data
                            .AdditionalData
                            .ContainsKey(expectedAdditionalEntity.Key));
                    Assert.AreEqual(
                        expectedAdditionalEntity.Value,
                        response.Data.AdditionalData[expectedAdditionalEntity.Key]);
                }
            }

            Assert.IsFalse(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger.Run() implementation runs correctly for concurrent executions.
        /// </summary>
        [TestMethod]
        public void ExtractInfoImplementationConcurrentRuns()
        {
            Task[] threads = new Task[_concurrentConfig.NumberOfThreads];

            for (int i = 0; i < _concurrentConfig.NumberOfThreads; i++)
            {
                threads[i] = Task.Run(
                    new Action(
                        () => ExtractInfoRunnerLocal(_concurrentConfig.TestConfigurationQueue)));
            }

            _log.LogInformation("Waiting on threads...");
            Task.WaitAll(threads);
            _log.LogInformation("All threads completed...");
        }

        /// <summary>
        /// Runner for ensuring ExtractInfoHttpTrigger.Run() implementation runs correctly for concurrent executions.
        /// </summary>
        /// <param name="queue">The queue of tests to run.</param>
        public void ExtractInfoRunnerLocal(ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpRequest request = CreateHttpPostRequest(nextTestConfiguration.CallSid, nextTestConfiguration.Transcript);

                    ExecutionContext context = new ExecutionContext()
                    {
                        FunctionAppDirectory = Directory.GetCurrentDirectory(),
                    };

                    IActionResult result = ExtractInfoHttpTrigger.Run(request, _log, context).Result;

                    Assert.IsInstanceOfType(result, typeof(OkObjectResult));

                    OkObjectResult okResult = (OkObjectResult)result;

                    Assert.AreEqual(200, okResult.StatusCode);
                    Assert.IsInstanceOfType(okResult.Value, typeof(ExtractInfoResponse));

                    ExtractInfoResponse response = (ExtractInfoResponse)okResult.Value;

                    Assert.AreEqual(nextTestConfiguration.CallSid, response.CallSid);
                    Assert.IsNotNull(response.Data);

                    _log.LogInformation("Writing extracted data to test configuration...");
                    nextTestConfiguration.Data = response.Data;
                }

                TH.Thread.Sleep(2000);
            }

            _concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
        }

        /// <summary>
        /// Ensure ExtractInfoHttpTrigger runs correctly for concurrent executions against remote Azure Function.
        /// </summary>
        [TestMethod]
        public void ExtractInfoHttpTriggerConcurrentRuns()
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
                            () => ExtractInfoRunnerAsync(httpClient, functionSettings, _concurrentConfig.TestConfigurationQueue).Wait()));
                }

                _log.LogInformation("Waiting on threads...");
                Task.WaitAll(threads);
                _log.LogInformation("All threads completed...");
            }

            _concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
        }

        /// <summary>
        /// Runner for ensuring ExtractInfoHttpTrigger runs correctly against a remote Azure Function.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance to use.</param>
        /// <param name="functionSettings">Configuration information required for calling the Azure Function.</param>
        /// <param name="queue">The queue of tests to run.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task ExtractInfoRunnerAsync(HttpClient httpClient, FunctionSettings functionSettings, ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpContent requestContent = CreateHttpPostContent(nextTestConfiguration.CallSid, nextTestConfiguration.Transcript);

                    HttpResponseMessage httpResponse = await httpClient.PostAsync(functionSettings.ExtractInfoUrl, requestContent);

                    string responseContent = await httpResponse.Content.ReadAsStringAsync();

                    ExtractInfoResponse response = JsonConvert.DeserializeObject<ExtractInfoResponse>(responseContent);

                    Assert.AreEqual(nextTestConfiguration.CallSid, response.CallSid);
                    Assert.IsNotNull(response.Data);

                    _log.LogInformation("Writing extracted data to test configuration...");
                    nextTestConfiguration.Data = response.Data;
                }

                TH.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Creates an HTTP POST request for the ExtractInfo function.
        /// </summary>
        /// <param name="expectedCallSid">The callSid to set in the request body.</param>
        /// <param name="expectedText">The text to set in the request body.</param>
        /// <returns>The HTTP request.</returns>
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

        /// <summary>
        /// Creates content for a request to the ExtractInfo function.
        /// </summary>
        /// <param name="callSid">The callSid to set in the content.</param>
        /// <param name="transcript">The text to set in the content.</param>
        /// <returns>The content.</returns>
        private static HttpContent CreateHttpPostContent(string callSid, string transcript)
        {
            ExtractInfoRequest requestObj = new ExtractInfoRequest()
            {
                CallSid = callSid,
                Text = transcript,
            };

            return new StringContent(
                JsonConvert.SerializeObject(requestObj));
        }
    }
}
