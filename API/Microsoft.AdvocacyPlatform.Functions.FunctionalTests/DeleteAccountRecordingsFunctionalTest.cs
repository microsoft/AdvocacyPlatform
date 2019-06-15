// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.FunctionalTests
{
    using System;
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
    using TwilioHttp = Twilio.Http;

    /// <summary>
    /// Functional test for DeleteRecordingsHttpTrigger.
    /// </summary>
    [TestClass]
    public class DeleteAccountRecordingsFunctionalTest
    {
        /// <summary>
        /// Internal logging.
        /// </summary>
        private ConsoleLogger _log = new ConsoleLogger();

        /// <summary>
        /// Initialize tests.
        /// </summary>
        [TestInitialize]
        public void InitializeTests()
        {
        }

        /// <summary>
        /// Ensure DeleteRecordingsHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsImplementationRun()
        {
            DeleteRecordingsRunnerLocal();
        }

        /// <summary>
        /// Runner for testing DeleteRecordingsHttpTrigger.Run() implementation runs correctly.
        /// </summary>
        public void DeleteRecordingsRunnerLocal()
        {
            HttpRequest request = CreateHttpPostRequest("Yes");

            ExecutionContext context = new ExecutionContext()
            {
                FunctionAppDirectory = Directory.GetCurrentDirectory(),
            };

            IActionResult result = DeleteAccountRecordingsHttpTrigger.Run(request, _log, context).Result;

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            OkObjectResult okResult = (OkObjectResult)result;

            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsInstanceOfType(okResult.Value, typeof(DeleteAccountRecordingsResponse));

            DeleteAccountRecordingsResponse response = (DeleteAccountRecordingsResponse)okResult.Value;

            Assert.IsTrue(response.AreAllRecordingsDeleted);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Ensure calls to DeleteRecordingsHttpTrigger for a deployed Azure Function runs correctly.
        /// </summary>
        [TestMethod]
        public void DeleteAccountRecordingsHttpTriggerRun()
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

                DeleteRecordingsRunner(httpClient, functionSettings);
            }
        }

        /// <summary>
        /// Runner for ensuring calls to DeleteRecordingsHttpTrigger for a deployed Azure Function runs correctly.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance to use.</param>
        /// <param name="functionSettings">Configuration information required to make calls to the deployed Azure Function.</param>
        public void DeleteRecordingsRunner(HttpClient httpClient, FunctionSettings functionSettings)
        {
            HttpContent requestContent = CreateHttpPostContent("Yes");

            HttpResponseMessage httpResponse = httpClient.PostAsync(functionSettings.DeleteAccountRecordingsUrl, requestContent).Result;

            string responseContent = httpResponse.Content.ReadAsStringAsync().Result;

            DeleteAccountRecordingsResponse response = JsonConvert.DeserializeObject<DeleteAccountRecordingsResponse>(responseContent);

            Assert.IsTrue(response.AreAllRecordingsDeleted);

            Assert.AreEqual((int)CommonStatusCode.Ok, response.StatusCode);
            Assert.AreEqual(Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Ok), response.StatusDesc);

            Assert.IsFalse(response.HasError);
            Assert.AreEqual((int)CommonErrorCode.NoError, response.ErrorCode);
            Assert.IsNull(response.ErrorDetails);
        }

        /// <summary>
        /// Creates an HTTP POST request for DeleteAccountRecordings.
        /// </summary>
        /// <param name="confirmDelete">The value to set for confirmDelete in the content body.</param>
        /// <returns>The generated HTTP request.</returns>
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

        /// <summary>
        /// Creates the request content.
        /// </summary>
        /// <param name="confirmDelete">The value to set for confirmDelete in the content body.</param>
        /// <returns>The content.</returns>
        private static HttpContent CreateHttpPostContent(string confirmDelete)
        {
            DeleteAccountRecordingsRequest requestObj = new DeleteAccountRecordingsRequest()
            {
                ConfirmDelete = confirmDelete,
            };

            return new StringContent(
                JsonConvert.SerializeObject(requestObj));
        }
    }
}
