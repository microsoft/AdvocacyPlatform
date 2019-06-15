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
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Twilio.Rest.Api.V2010.Account;
    using TH = System.Threading;
    using TwilioHttp = Twilio.Http;

    /// <summary>
    /// Functional test for CheckCallProgressHttpTrigger.
    /// </summary>
    [TestClass]
    public class CheckCallProgressFunctionalTest
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
        /// Ensure CheckCallPrgressHttpTrigger.Run() runs correctly.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerRun()
        {
            HttpRequest request = CreateHttpPostRequest(_config.CallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = CheckCallProgressHttpTrigger.Run(request, _log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

            CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

            Assert.AreEqual(_config.CallSid, response.CallSid);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual(0, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure CheckCallPrgressHttpTrigger.Run() runs correctly for concurrent executions.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressImplementationConcurrentRuns()
        {
            Task[] threads = new Task[_concurrentConfig.NumberOfThreads];

            for (int i = 0; i < _concurrentConfig.NumberOfThreads; i++)
            {
                threads[i] = Task.Run(
                    new Action(
                        () => CheckCallProgressRunnerLocal(_concurrentConfig.TestConfigurationQueue)));
            }

            _log.LogInformation("Waiting on threads...");
            Task.WaitAll(threads);
            _log.LogInformation("All threads completed...");
        }

        /// <summary>
        /// Ensure CheckCallPrgressHttpTrigger.Run() runs correctly against local URL.
        /// </summary>
        /// <param name="queue">The queue of tests to run.</param>
        public void CheckCallProgressRunnerLocal(ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpRequest request = CreateHttpPostRequest(nextTestConfiguration.CallSid);

                    ExecutionContext context = new ExecutionContext()
                    {
                        FunctionAppDirectory = Directory.GetCurrentDirectory(),
                    };

                    IActionResult result = CheckCallProgressHttpTrigger.Run(request, _log, context).Result;

                    Assert.IsInstanceOfType(result, typeof(OkObjectResult));

                    OkObjectResult okResult = (OkObjectResult)result;

                    Assert.AreEqual(200, okResult.StatusCode);
                    Assert.IsInstanceOfType(okResult.Value, typeof(CheckCallProgressResponse));

                    CheckCallProgressResponse response = (CheckCallProgressResponse)okResult.Value;

                    Assert.AreEqual(nextTestConfiguration.CallSid, response.CallSid);

                    Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
                    Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

                    Assert.IsFalse(response.HasError);
                    Assert.AreEqual(0, response.ErrorCode);
                    Assert.IsNull(response.ErrorDetails);

                    _log.LogInformation("Writing returned call status to test configuration...");
                    nextTestConfiguration.CallStatus = response.Status;
                }

                TH.Thread.Sleep(2000);
            }

            _concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
        }

        /// <summary>
        /// Ensure CheckCallPrgressHttpTrigger.Run() runs correctly for concurrent runs against deployed Azure Function.
        /// </summary>
        [TestMethod]
        public void CheckCallProgressHttpTriggerConcurrentRuns()
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
                            () => CheckCallProgressRunnerAsync(httpClient, functionSettings, _concurrentConfig.TestConfigurationQueue).Wait()));
                }

                _log.LogInformation("Waiting on threads...");
                Task.WaitAll(threads);
                _log.LogInformation("All threads completed...");
            }

            _concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
        }

        /// <summary>
        /// Runner utility for testing CheckCallProgress.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance to use.</param>
        /// <param name="functionSettings">Configuration information for calling the remote function.</param>
        /// <param name="queue">A queue of test to run.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task CheckCallProgressRunnerAsync(HttpClient httpClient, FunctionSettings functionSettings, ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpContent requestContent = CreateHttpPostContent(nextTestConfiguration.CallSid);

                    HttpResponseMessage httpResponse = await httpClient.PostAsync(functionSettings.CheckCallProgressUrl, requestContent);

                    string responseContent = await httpResponse.Content.ReadAsStringAsync();

                    CheckCallProgressResponse response = JsonConvert.DeserializeObject<CheckCallProgressResponse>(responseContent);

                    Assert.AreEqual(nextTestConfiguration.CallSid, response.CallSid);

                    Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
                    Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

                    Assert.IsFalse(response.HasError);
                    Assert.AreEqual(0, response.ErrorCode);
                    Assert.IsNull(response.ErrorDetails);

                    _log.LogInformation("Writing returned call status to test configuration...");
                    nextTestConfiguration.CallStatus = response.Status;
                }

                TH.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Creates an HTTP POST request message for CheckCallProgress.
        /// </summary>
        /// <param name="callSid">The callSid to set in the request body.</param>
        /// <returns>The generated request message.</returns>
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

        /// <summary>
        /// Creates content for an HTTP request for CheckCallProgress.
        /// </summary>
        /// <param name="callSid">The callSid to set in the request body.</param>
        /// <returns>The request body content.</returns>
        private static HttpContent CreateHttpPostContent(string callSid)
        {
            CheckCallProgressRequest requestObj = new CheckCallProgressRequest()
            {
                CallSid = callSid,
            };

            return new StringContent(
                JsonConvert.SerializeObject(requestObj));
        }
    }
}
