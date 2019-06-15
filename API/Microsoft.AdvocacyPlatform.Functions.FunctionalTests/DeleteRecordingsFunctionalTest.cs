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
    /// Functional test for DeleteRecordingsHttpTrigger.
    /// </summary>
    [TestClass]
    public class DeleteRecordingsFunctionalTest
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
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerRun()
        {
            HttpRequest request = CreateDeleteHttpPostRequest(_config.CallSid);

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteRecordingsHttpTrigger.Run(request, _log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteRecordingsResponse));

            DeleteRecordingsResponse response = (DeleteRecordingsResponse)okResult.Value;

            Assert.AreEqual(_config.CallSid, response.CallSid);
            Assert.IsTrue(response.AreAllRecordingsDeleted);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Verify recordings were actually deleted.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsVerify()
        {
            HttpRequest request = CreatePullHttpPostRequest(_config.InputId, _config.CallSid);

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

            Assert.AreEqual(_config.CallSid, response.CallSid);
            Assert.IsNull(response.RecordingUri);

            Assert.AreEqual((int)CommonStatusCode.Error, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error), response.StatusDesc);

            Assert.IsTrue(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.TwilioCallNoRecordings, response.ErrorCode);
            Assert.AreEqual("No recording", response.ErrorDetails);
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() runs correctly for concurrent executions.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsImplementationConcurrentRuns()
        {
            Task[] threads = new Task[_concurrentConfig.NumberOfThreads];

            for (int i = 0; i < _concurrentConfig.NumberOfThreads; i++)
            {
                threads[i] = Task.Run(
                    new Action(
                        () => DeleteRecordingsRunnerLocal(_concurrentConfig.TestConfigurationQueue)));
            }

            _log.LogInformation("Waiting on threads...");
            Task.WaitAll(threads);
            _log.LogInformation("All threads completed...");

            _concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation runs correctly for concurrent executions.
        /// </summary>
        /// <param name="queue">The queue of tests to run.</param>
        public void DeleteRecordingsRunnerLocal(ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpRequest request = CreateDeleteHttpPostRequest(nextTestConfiguration.CallSid);

                    ExecutionContext context = new ExecutionContext()
                    {
                        FunctionAppDirectory = Directory.GetCurrentDirectory(),
                    };

                    IActionResult result = DeleteRecordingsHttpTrigger.Run(request, _log, context).Result;

                    Assert.IsInstanceOfType(result, typeof(OkObjectResult));

                    OkObjectResult okResult = (OkObjectResult)result;

                    Assert.AreEqual(200, okResult.StatusCode);
                    Assert.IsInstanceOfType(okResult.Value, typeof(DeleteRecordingsResponse));

                    DeleteRecordingsResponse response = (DeleteRecordingsResponse)okResult.Value;

                    Assert.AreEqual(_config.CallSid, response.CallSid);
                    Assert.IsTrue(response.AreAllRecordingsDeleted);

                    Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
                    Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

                    Assert.IsFalse(response.HasError);
                    Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
                    Assert.IsNull(response.ErrorDetails);

                    nextTestConfiguration.AllRecordingsDeleted = response.AreAllRecordingsDeleted;
                }

                TH.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger runs correctly for concurrent executions against remote Azure Function.
        /// </summary>
        [TestMethod]
        public void DeleteRecordingsHttpTriggerConcurrentRuns()
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
                            () => DeleteRecordingsRunnerAsync(httpClient, functionSettings, _concurrentConfig.TestConfigurationQueue).Wait()));
                }

                _log.LogInformation("Waiting on threads...");
                Task.WaitAll(threads);
                _log.LogInformation("All threads completed...");
            }

            _concurrentConfig.Save(GlobalTestConstants.ConcurrentTestConfigurationFilePath);
        }

        /// <summary>
        /// Runner for ensure DeleteRecordingsHttpTrigger runs correctly against remote Azure Function.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance to use.</param>
        /// <param name="functionSettings">Configuration information required to make calls to the Azure Functions.</param>
        /// <param name="queue">The queue of tests to run.</param>
        /// <returns>An asynchronous task.</returns>
        public async Task DeleteRecordingsRunnerAsync(HttpClient httpClient, FunctionSettings functionSettings, ConcurrentQueue<TestConfiguration> queue)
        {
            TestConfiguration nextTestConfiguration = null;

            while (!queue.IsEmpty)
            {
                if (queue.TryDequeue(out nextTestConfiguration))
                {
                    HttpContent requestContent = CreateHttpPostContent(nextTestConfiguration.CallSid);

                    HttpResponseMessage httpResponse = await httpClient.PostAsync(functionSettings.DeleteRecordingsUrl, requestContent);

                    string responseContent = await httpResponse.Content.ReadAsStringAsync();

                    DeleteRecordingsResponse response = JsonConvert.DeserializeObject<DeleteRecordingsResponse>(responseContent);

                    Assert.AreEqual(nextTestConfiguration.CallSid, response.CallSid);
                    Assert.IsTrue(response.AreAllRecordingsDeleted);

                    Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
                    Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

                    Assert.IsFalse(response.HasError);
                    Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
                    Assert.IsNull(response.ErrorDetails);

                    nextTestConfiguration.AllRecordingsDeleted = response.AreAllRecordingsDeleted;
                }

                TH.Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Creates an HTTP POST request for the DeleteRecordings function.
        /// </summary>
        /// <param name="callSid">The callSid to set in the request body.</param>
        /// <returns>The HTTP request.</returns>
        private static HttpRequest CreateDeleteHttpPostRequest(string callSid)
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

        /// <summary>
        /// Creates an HTTP POST request for the DeleteRecordings function.
        /// </summary>
        /// <param name="inputId">The inputId to set in the content.</param>
        /// <param name="callSid">The callSid to set in the content.</param>
        /// <returns>The content.</returns>
        private static HttpRequest CreatePullHttpPostRequest(string inputId, string callSid)
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
        /// Creates content for a request to DeleteRecordings.
        /// </summary>
        /// <param name="callSid">The callSid to set in the content.</param>
        /// <returns>The content.</returns>
        private static HttpContent CreateHttpPostContent(string callSid)
        {
            DeleteRecordingsRequest requestObj = new DeleteRecordingsRequest()
            {
                CallSid = callSid,
            };

            return new StringContent(
                JsonConvert.SerializeObject(requestObj));
        }
    }
}
