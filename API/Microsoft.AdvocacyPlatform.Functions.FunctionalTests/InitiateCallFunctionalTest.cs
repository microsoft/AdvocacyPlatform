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
    /// Functional test for InitiateCallHttpTrigger.
    /// </summary>
    [TestClass]
    public class InitiateCallFunctionalTest
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
        /// Initializes test.
        /// </summary>
        [TestInitialize]
        public void InitializeTests()
        {
            Tuple<TestConfiguration, ConcurrentTestConfiguration> configs = TestHelper.InitializeTest(_log, true);

            _config = configs.Item1;
            _concurrentConfig = configs.Item2;
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerRun()
        {
            HttpRequest request = CreateHttpPostRequest(_config.InputId);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            _log.LogInformation("Calling function to initiate call...");
            IActionResult result = InitiateCallHttpTrigger.Run(request, _log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

            InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

            Assert.AreEqual(_config.InputId, response.InputId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(response.CallSid));

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);

            _log.LogInformation("Writing returned call sid to test configuration...");
            _config.CallSid = response.CallSid;
            _config.Save(GlobalTestConstants.TestConfigurationFilePath);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger.Run() implementation runs correctly for concurrent executions.
        /// </summary>
        [TestMethod]
        public void InitiateCallImplementationConcurrentRuns()
        {
            Task[] threads = new Task[_concurrentConfig.NumberOfThreads];

            for (int i = 0; i < _concurrentConfig.NumberOfThreads; i++)
            {
                threads[i] = Task.Run(
                    new Action(
                        () => InitiateCallRunnerLocal(_concurrentConfig.TestConfigurationQueue)));
            }

            _log.LogInformation("Waiting on threads...");
            Task.WaitAll(threads);
            _log.LogInformation("All threads completed...");
        }

        /// <summary>
        /// Runner for ensuring InitiateCallHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        /// <param name="queue">The queue of tests to run.</param>
        public void InitiateCallRunnerLocal(ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpRequest request = CreateHttpPostRequest(nextTestConfiguration.InputId, null);

                    ExecutionContext context = new ExecutionContext()
                    {
                        FunctionAppDirectory = Directory.GetCurrentDirectory(),
                    };

                    IActionResult result = InitiateCallHttpTrigger.Run(request, _log, context).Result;

                    Assert.IsInstanceOfType(result, typeof(OkObjectResult));

                    OkObjectResult okResult = (OkObjectResult)result;

                    Assert.AreEqual(200, okResult.StatusCode);
                    Assert.IsInstanceOfType(okResult.Value, typeof(InitiateCallResponse));

                    InitiateCallResponse response = (InitiateCallResponse)okResult.Value;

                    Assert.AreEqual(nextTestConfiguration.InputId, response.InputId);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(response.CallSid));

                    Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
                    Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

                    Assert.IsFalse(response.HasError);
                    Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
                    Assert.IsNull(response.ErrorDetails);

                    _log.LogInformation("Writing returned call sid to test configuration...");
                    nextTestConfiguration.CallSid = response.CallSid;
                }

                TH.Thread.Sleep(2000);
            }

            _concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
        }

        /// <summary>
        /// Ensure InitiateCallHttpTrigger runs correctly for concurrent calls against a remote Azure Function.
        /// </summary>
        [TestMethod]
        public void InitiateCallHttpTriggerConcurrentRuns()
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
                            () => InitiateCallRunnerAsync(httpClient, functionSettings, _concurrentConfig.TestConfigurationQueue).Wait()));
                }

                _log.LogInformation("Waiting on threads...");
                Task.WaitAll(threads);
                _log.LogInformation("All threads completed...");
            }

            _concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
        }

        /// <summary>
        /// Runner for ensuring InitiateCallHttpTrigger runs correctly against a remote Azure Function.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance to use.</param>
        /// <param name="functionSettings">Configuration information required to make calls to the Azure Function.</param>
        /// <param name="queue">The queue of tests to run.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task InitiateCallRunnerAsync(HttpClient httpClient, FunctionSettings functionSettings, ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpContent requestContent = CreateHttpPostContent(nextTestConfiguration.InputId, null);

                    HttpResponseMessage httpResponse = await httpClient.PostAsync(functionSettings.InitiateCallUrl, requestContent);

                    string responseContent = await httpResponse.Content.ReadAsStringAsync();

                    InitiateCallResponse response = JsonConvert.DeserializeObject<InitiateCallResponse>(responseContent);

                    Assert.AreEqual(nextTestConfiguration.InputId, response.InputId);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(response.CallSid));

                    Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
                    Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

                    Assert.IsFalse(response.HasError);
                    Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
                    Assert.IsNull(response.ErrorDetails);

                    _log.LogInformation("Writing returned call sid to test configuration...");
                    nextTestConfiguration.CallSid = response.CallSid;
                }

                TH.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Create an HTTP POST request for InitiateCall.
        /// </summary>
        /// <param name="inputId">The inputId to set in the request body.</param>
        /// <param name="dtmf">The dtmf to set in the request body.</param>
        /// <returns>The HTTP request.</returns>
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

        /// <summary>
        /// Creates content for a request to InitiateCall.
        /// </summary>
        /// <param name="inputId">The inputId to set in the content.</param>
        /// <param name="dtmf">The dtmf to set in the content.</param>
        /// <returns>The content.</returns>
        private static HttpContent CreateHttpPostContent(string inputId, DtmfRequest dtmf = null)
        {
            InitiateCallRequest requestObj = new InitiateCallRequest()
            {
                InputId = inputId,
                Dtmf = dtmf,
            };

            return new StringContent(
                JsonConvert.SerializeObject(requestObj));
        }
    }
}
