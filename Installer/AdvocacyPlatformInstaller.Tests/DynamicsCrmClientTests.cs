// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using AdvocacyPlatformInstaller.Clients;
    using AdvocacyPlatformInstaller.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Unit tests for DynamicsCrmClient implementation.
    /// </summary>
    [TestClass]
    public class DynamicsCrmClientTests
    {
        private HttpClientMock _httpClient;
        private ITokenProvider _tokenProvider;

        /// <summary>
        /// Initializes test.
        /// </summary>
        [TestInitialize]
        public void InitializeTest()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };

            _httpClient = new HttpClientMock();
            _tokenProvider = new TokenProviderMock();

            ((TokenProviderMock)_tokenProvider).RegisterHttpClientMock(_httpClient);
        }

        /// <summary>
        /// Test success of GetUserIdAsync().
        /// </summary>
        [TestMethod]
        public void GetUserIdAsyncSuccess()
        {
            string expectedUniqueName = "orgtest1";
            string expectedDomainName = "orgtest5";
            string expectedObjectId = Guid.NewGuid().ToString();
            string expectedUserId = Guid.NewGuid().ToString();
            string expectedOrgId = Guid.NewGuid().ToString();
            string expectedRequestUri = $"https://{expectedDomainName}.crm.dynamics.com/api/data/v9.0/WhoAmI";
            string responseFilePath = @"./data/templates/responses/dynamicsCrm/whoAmI.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "objectId", expectedObjectId },
                    { "userId", expectedUserId },
                    { "orgId", expectedOrgId },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IDynamicsCrmClient client = new DynamicsCrmClient(
                expectedUniqueName,
                expectedDomainName,
                _tokenProvider);

            string response = client.GetUserIdAsync().Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.AreEqual(expectedUserId, response, $"Unexpected user id ('{expectedUserId}' != '{response}')");
        }

        /// <summary>
        /// Test success of GetSolutionsAsync().
        /// </summary>
        [TestMethod]
        public void GetSolutionsAsyncSuccess()
        {
            string expectedUniqueName = "orgtest1";
            string expectedDomainName = "orgtest5";
            string expectedOrgId = Guid.NewGuid().ToString();
            string expectedSolutionId = Guid.NewGuid().ToString();
            string expectedSolutionUniqueName = "TestSolution";
            string expectedSolutionVersion = "1.0.0.5";
            string expectedRequestUri = $"https://{expectedDomainName}.crm.dynamics.com/api/data/v9.0/solutions";
            string responseFilePath = @"./data/templates/responses/dynamicsCrm/getSolutions.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "orgId", expectedOrgId },
                    { "solutionId", expectedSolutionId },
                    { "uniqueName", expectedSolutionUniqueName },
                    { "version", expectedSolutionVersion },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IDynamicsCrmClient client = new DynamicsCrmClient(
                expectedUniqueName,
                expectedDomainName,
                _tokenProvider);

            DynamicsCrmValueResponse<DynamicsCrmSolution> response = client.GetSolutionsAsync().Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.IsNotNull(response.Value, "The response Value member should not be null!");
            Assert.AreEqual(1, response.Value.Length, $"Unexpected number of solutions returned ('1' != '{response.Value.Length}')!");
            Assert.AreEqual(expectedSolutionId, response.Value[0].SolutionId, $"Unexpected solution id ('{expectedSolutionId}' != '{response.Value[0].SolutionId}')");
            Assert.AreEqual(expectedSolutionUniqueName, response.Value[0].UniqueName, $"Unexpected solution id ('{expectedSolutionUniqueName}' != '{response.Value[0].UniqueName}')");
            Assert.AreEqual(expectedSolutionVersion, response.Value[0].Version, $"Unexpected solution id ('{expectedSolutionVersion}' != '{response.Value[0].Version}')");
        }

        /// <summary>
        /// Test success of GetSolutionAsync().
        /// </summary>
        [TestMethod]
        public void GetSolutionAsyncSuccess()
        {
            string expectedUniqueName = "orgtest1";
            string expectedDomainName = "orgtest5";
            string expectedOrgId = Guid.NewGuid().ToString();
            string expectedSolutionId = Guid.NewGuid().ToString();
            string expectedSolutionUniqueName = "TestSolution";
            string expectedSolutionVersion = "1.0.0.5";
            string expectedRequestUri = $"https://{expectedDomainName}.crm.dynamics.com/api/data/v9.0/solutions?$filter=uniquename%20eq%20'{expectedSolutionUniqueName}'";
            string responseFilePath = @"./data/templates/responses/dynamicsCrm/getSolutions.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "orgId", expectedOrgId },
                    { "solutionId", expectedSolutionId },
                    { "uniqueName", expectedSolutionUniqueName },
                    { "version", expectedSolutionVersion },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IDynamicsCrmClient client = new DynamicsCrmClient(
                expectedUniqueName,
                expectedDomainName,
                _tokenProvider);

            DynamicsCrmSolution response = client.GetSolutionAsync(expectedSolutionUniqueName).Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.AreEqual(expectedSolutionId, response.SolutionId, $"Unexpected solution id ('{expectedSolutionId}' != '{response.SolutionId}')");
            Assert.AreEqual(expectedSolutionUniqueName, response.UniqueName, $"Unexpected solution id ('{expectedSolutionUniqueName}' != '{response.UniqueName}')");
            Assert.AreEqual(expectedSolutionVersion, response.Version, $"Unexpected solution id ('{expectedSolutionVersion}' != '{response.Version}')");
        }

        /// <summary>
        /// Test success of ImportSolutionAsync().
        /// </summary>
        [TestMethod]
        public void ImportSolutionAsyncSuccess()
        {
            string expectedUniqueName = "orgtest1";
            string expectedDomainName = "orgtest5";
            string expectedSolutionFilePath = @"./data/templates/requests/dynamicsCrm/dummySolution.zip";
            string expectedRequestUri = $"https://{expectedDomainName}.crm.dynamics.com/api/data/v9.0/ImportSolution";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.NoContent,
                null,
                null,
                "application/json",
                null);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IDynamicsCrmClient client = new DynamicsCrmClient(
                expectedUniqueName,
                expectedDomainName,
                _tokenProvider);

            string response = client.ImportSolutionAsync(expectedSolutionFilePath, false).Result;

            Assert.IsNull(response, "The response should not be null!");
        }

        /// <summary>
        /// Test success of UpdateSolutionAsync().
        /// </summary>
        [TestMethod]
        public void UpdateSolutionAsyncSuccess()
        {
            string expectedUniqueName = "orgtest1";
            string expectedDomainName = "orgtest5";
            string expectedOrgId = Guid.NewGuid().ToString();
            string expectedSolutionId = Guid.NewGuid().ToString();
            string expectedSolutionUniqueName = "TestSolution";
            string expectedSolutionVersion = "1.0.0.5";
            string expectedSolutionFilePath = @"./data/templates/requests/dynamicsCrm/dummySolution.zip";
            string expectedRequestUri = $"https://{expectedDomainName}.crm.dynamics.com/api/data/v9.0/solutions?$filter=uniquename%20eq%20'{expectedSolutionUniqueName}'";
            string expectedRequestUri2 = $"https://{expectedDomainName}.crm.dynamics.com/api/data/v9.0/ImportSolution";
            string responseFilePath = @"./data/templates/responses/dynamicsCrm/getSolutions.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));
            HttpRequestMessage expectedRequest2 = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri2);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest2));
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "orgId", expectedOrgId },
                    { "solutionId", expectedSolutionId },
                    { "uniqueName", expectedSolutionUniqueName },
                    { "version", expectedSolutionVersion },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));
            HttpResponseMessage expectedResponse2 = TestHelper.CreateHttpResponse(
                HttpStatusCode.NoContent,
                null,
                null,
                "application/json",
                null);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri2,
                new ExpectedResponse(expectedResponse2));
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IDynamicsCrmClient client = new DynamicsCrmClient(
                expectedUniqueName,
                expectedDomainName,
                _tokenProvider);

            DynamicsCrmSolution response = client.UpdateSolutionAsync(
                expectedSolutionUniqueName,
                expectedSolutionFilePath).Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.AreEqual(expectedSolutionId, response.SolutionId, $"Unexpected solution id ('{expectedSolutionId}' != '{response.SolutionId}')");
            Assert.AreEqual(expectedSolutionUniqueName, response.UniqueName, $"Unexpected solution id ('{expectedSolutionUniqueName}' != '{response.UniqueName}')");
            Assert.AreEqual(expectedSolutionVersion, response.Version, $"Unexpected solution id ('{expectedSolutionVersion}' != '{response.Version}')");
        }
    }
}
