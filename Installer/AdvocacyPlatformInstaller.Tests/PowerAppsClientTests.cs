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
    /// Unit tests for PowerAppsClient implementation.
    /// </summary>
    [TestClass]
    public class PowerAppsClientTests
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
        /// Test success of GetEnvironmentLocationsAsync().
        /// </summary>
        [TestMethod]
        public void GetEnvironmentLocationsAsyncSuccess()
        {
            string expectedRequestUri = "https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/locations?api-version=2016-11-01";
            Dictionary<string, string> expectedLocations = new Dictionary<string, string>()
            {
                { "name1", "unitedstates" },
                { "displayName1", "United States" },
                { "code1", "NA" },
                { "name2", "europe" },
                { "displayName2", "Europe" },
                { "code2", "EMEA" },
                { "name3", "asia" },
                { "displayName3", "Asia" },
                { "code3", "APAC" },
            };
            string responseFilePath = @"./data/templates/responses/powerApps/getLocations.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                expectedLocations);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IPowerAppsClient client = new PowerAppsClient(_tokenProvider);

            GetPowerAppsEnvironmentLocationsResponse response = client.GetEnvironmentLocationsAsync().Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.IsNotNull(response.Value, "The response Value member should not be null!");
            Assert.AreEqual(3, response.Value.Length, $"Unexpected number of locations ('3' != '{response.Value.Length}')!");
            Assert.AreEqual(expectedLocations["name1"], response.Value[0].Name, $"Unexpected name for location 1 ('{expectedLocations["name1"]}' != '{response.Value[0].Name}')");
            Assert.IsNotNull(response.Value[0].Properties, "The response Value Properties member should not be null for location 1!");
            Assert.AreEqual(expectedLocations["code1"], response.Value[0].Properties.Code, $"Unexpected name for location 1 ('{expectedLocations["code1"]}' != '{response.Value[0].Name}')");
            Assert.AreEqual(expectedLocations["displayName1"], response.Value[0].Properties.DisplayName, $"Unexpected display name for location 1 ('{expectedLocations["displayName1"]}' != '{response.Value[0].Properties.DisplayName}')");
            Assert.AreEqual(expectedLocations["name2"], response.Value[1].Name, $"Unexpected name for location 2 ('{expectedLocations["name2"]}' != '{response.Value[1].Name}')");
            Assert.IsNotNull(response.Value[1].Properties, "The response Value Properties member should not be null for location 2!");
            Assert.AreEqual(expectedLocations["code2"], response.Value[1].Properties.Code, $"Unexpected name for location 2 ('{expectedLocations["code2"]}' != '{response.Value[1].Name}')");
            Assert.AreEqual(expectedLocations["displayName2"], response.Value[1].Properties.DisplayName, $"Unexpected display name for location 2 ('{expectedLocations["displayName2"]}' != '{response.Value[1].Properties.DisplayName}')");
            Assert.AreEqual(expectedLocations["name3"], response.Value[2].Name, $"Unexpected name for location 3 ('{expectedLocations["name3"]}' != '{response.Value[2].Name}')");
            Assert.IsNotNull(response.Value[2].Properties, "The response Value Properties member should not be null for location 3!");
            Assert.AreEqual(expectedLocations["code3"], response.Value[2].Properties.Code, $"Unexpected name for location 3 ('{expectedLocations["code3"]}' != '{response.Value[2].Name}')");
            Assert.AreEqual(expectedLocations["displayName3"], response.Value[2].Properties.DisplayName, $"Unexpected display name for location 3 ('{expectedLocations["displayName3"]}' != '{response.Value[2].Properties.DisplayName}')");
        }

        /// <summary>
        /// Test success of GetEnvironmentsAsync().
        /// </summary>
        [TestMethod]
        public void GetEnvironmentsAsyncSuccess()
        {
            string expectedEnvironmentName = Guid.NewGuid().ToString();
            string expectedLocation = "unitedstates";
            string expectedDisplayName = "TestEnvironment";
            string expectedResourceId = Guid.NewGuid().ToString();
            string expectedFriendlyName = "TestEnvironment";
            string expectedUniqueName = "orgtest4";
            string expectedDomainName = "orgtest5";
            string expectedRequestUri = "https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/scopes/admin/environments?$expand=permissions&api-version=2016-11-01";
            string responseFilePath = @"./data/templates/responses/powerApps/getEnvironments.json";

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
                    { "environmentName", expectedEnvironmentName },
                    { "location", expectedLocation },
                    { "displayName", expectedDisplayName },
                    { "crmResourceId", expectedResourceId },
                    { "crmFriendlyName", expectedFriendlyName },
                    { "crmUniqueName", expectedUniqueName },
                    { "crmDomainName", expectedDomainName },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IPowerAppsClient client = new PowerAppsClient(_tokenProvider);

            GetPowerAppsEnvironmentsResponse response = (GetPowerAppsEnvironmentsResponse)client.GetEnvironmentsAsync().Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.IsNotNull(response.Value, "The response Value member should not be null!");
            Assert.AreEqual(1, response.Value.Length, $"Unexpected number of locations ('1' != '{response.Value.Length}')!");
            Assert.IsNotNull(response.Value[0].Properties, "The response Value.Properties member should not be null!");
            Assert.AreEqual(expectedEnvironmentName, response.Value[0].Name, $"Unexpected name ('{expectedEnvironmentName}' != '{response.Value[0].Name}')");
            Assert.AreEqual(expectedLocation, response.Value[0].Location, $"Unexpected location ('{expectedLocation}' != '{response.Value[0].Location}')");
            Assert.AreEqual(expectedDisplayName, response.Value[0].Properties.DisplayName, $"Unexpected location ('{expectedDisplayName}' != '{response.Value[0].Properties.DisplayName}')");
            Assert.AreEqual(expectedDisplayName, response.Value[0].Properties.DisplayName, $"Unexpected location ('{expectedDisplayName}' != '{response.Value[0].Properties.DisplayName}')");
            Assert.IsNotNull(response.Value[0].Properties.LinkedEnvironmentMetadata, "The response Value.Properties.LinkedEnvironmentMetadata member should not be null!");
            Assert.AreEqual(expectedResourceId, response.Value[0].Properties.LinkedEnvironmentMetadata.ResourceId, $"Unexpected resource id ('{expectedResourceId}' != '{response.Value[0].Properties.LinkedEnvironmentMetadata.ResourceId}')");
            Assert.AreEqual(expectedFriendlyName, response.Value[0].Properties.LinkedEnvironmentMetadata.FriendlyName, $"Unexpected friendly name ('{expectedFriendlyName}' != '{response.Value[0].Properties.LinkedEnvironmentMetadata.FriendlyName}')");
            Assert.AreEqual(expectedUniqueName, response.Value[0].Properties.LinkedEnvironmentMetadata.UniqueName, $"Unexpected unique name ('{expectedUniqueName}' != '{response.Value[0].Properties.LinkedEnvironmentMetadata.UniqueName}')");
            Assert.AreEqual(expectedDomainName, response.Value[0].Properties.LinkedEnvironmentMetadata.DomainName, $"Unexpected domain name ('{expectedDomainName}' != '{response.Value[0].Properties.LinkedEnvironmentMetadata.DomainName}')");
        }

        /// <summary>
        /// Test success of GetEnvironmentsAsync() with a specific id.
        /// </summary>
        [TestMethod]
        public void GetEnvironmentsAsyncWithIdSuccess()
        {
            string expectedEnvironmentName = Guid.NewGuid().ToString();
            string expectedLocation = "unitedstates";
            string expectedDisplayName = "TestEnvironment";
            string expectedResourceId = Guid.NewGuid().ToString();
            string expectedFriendlyName = "TestEnvironment";
            string expectedUniqueName = "orgtest4";
            string expectedDomainName = "orgtest5";
            string expectedRequestUri = $"https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/scopes/admin/environments/{expectedEnvironmentName}?$expand=permissions&api-version=2016-11-01";
            string responseFilePath = @"./data/templates/responses/powerApps/environment.json";

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
                    { "environmentName", expectedEnvironmentName },
                    { "location", expectedLocation },
                    { "displayName", expectedDisplayName },
                    { "crmResourceId", expectedResourceId },
                    { "crmFriendlyName", expectedFriendlyName },
                    { "crmUniqueName", expectedUniqueName },
                    { "crmDomainName", expectedDomainName },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IPowerAppsClient client = new PowerAppsClient(_tokenProvider);

            PowerAppsEnvironment response = (PowerAppsEnvironment)client.GetEnvironmentsAsync(expectedEnvironmentName).Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.IsNotNull(response.Properties, "The response Properties member should not be null!");
            Assert.AreEqual(expectedEnvironmentName, response.Name, $"Unexpected name ('{expectedEnvironmentName}' != '{response.Name}')");
            Assert.AreEqual(expectedLocation, response.Location, $"Unexpected location ('{expectedLocation}' != '{response.Location}')");
            Assert.AreEqual(expectedDisplayName, response.Properties.DisplayName, $"Unexpected location ('{expectedDisplayName}' != '{response.Properties.DisplayName}')");
            Assert.AreEqual(expectedDisplayName, response.Properties.DisplayName, $"Unexpected location ('{expectedDisplayName}' != '{response.Properties.DisplayName}')");
            Assert.IsNotNull(response.Properties.LinkedEnvironmentMetadata, "The response Properties.LinkedEnvironmentMetadata member should not be null!");
            Assert.AreEqual(expectedResourceId, response.Properties.LinkedEnvironmentMetadata.ResourceId, $"Unexpected resource id ('{expectedResourceId}' != '{response.Properties.LinkedEnvironmentMetadata.ResourceId}')");
            Assert.AreEqual(expectedFriendlyName, response.Properties.LinkedEnvironmentMetadata.FriendlyName, $"Unexpected friendly name ('{expectedFriendlyName}' != '{response.Properties.LinkedEnvironmentMetadata.FriendlyName}')");
            Assert.AreEqual(expectedUniqueName, response.Properties.LinkedEnvironmentMetadata.UniqueName, $"Unexpected unique name ('{expectedUniqueName}' != '{response.Properties.LinkedEnvironmentMetadata.UniqueName}')");
            Assert.AreEqual(expectedDomainName, response.Properties.LinkedEnvironmentMetadata.DomainName, $"Unexpected domain name ('{expectedDomainName}' != '{response.Properties.LinkedEnvironmentMetadata.DomainName}')");
        }

        /// <summary>
        /// Test success of GetEnvironmentByDisplayNameAsync().
        /// </summary>
        [TestMethod]
        public void GetEnvironmentByDisplayNameAsyncSuccess()
        {
            string expectedEnvironmentName = Guid.NewGuid().ToString();
            string expectedLocation = "unitedstates";
            string expectedDisplayName = "TestEnvironment";
            string expectedResourceId = Guid.NewGuid().ToString();
            string expectedFriendlyName = "TestEnvironment";
            string expectedUniqueName = "orgtest4";
            string expectedDomainName = "orgtest5";
            string expectedRequestUri = $"https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/scopes/admin/environments?$filter=properties.displayName%20eq%20'{expectedDisplayName}'&api-version=2016-11-01";
            string responseFilePath = @"./data/templates/responses/powerApps/getEnvironments.json";

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
                    { "environmentName", expectedEnvironmentName },
                    { "location", expectedLocation },
                    { "displayName", expectedDisplayName },
                    { "crmResourceId", expectedResourceId },
                    { "crmFriendlyName", expectedFriendlyName },
                    { "crmUniqueName", expectedUniqueName },
                    { "crmDomainName", expectedDomainName },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IPowerAppsClient client = new PowerAppsClient(_tokenProvider);

            PowerAppsEnvironment response = (PowerAppsEnvironment)client.GetEnvironmentByDisplayNameAsync(expectedDisplayName).Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.IsNotNull(response.Properties, "The response Properties member should not be null!");
            Assert.AreEqual(expectedEnvironmentName, response.Name, $"Unexpected name ('{expectedEnvironmentName}' != '{response.Name}')");
            Assert.AreEqual(expectedLocation, response.Location, $"Unexpected location ('{expectedLocation}' != '{response.Location}')");
            Assert.AreEqual(expectedDisplayName, response.Properties.DisplayName, $"Unexpected location ('{expectedDisplayName}' != '{response.Properties.DisplayName}')");
            Assert.AreEqual(expectedDisplayName, response.Properties.DisplayName, $"Unexpected location ('{expectedDisplayName}' != '{response.Properties.DisplayName}')");
            Assert.IsNotNull(response.Properties.LinkedEnvironmentMetadata, "The response Properties.LinkedEnvironmentMetadata member should not be null!");
            Assert.AreEqual(expectedResourceId, response.Properties.LinkedEnvironmentMetadata.ResourceId, $"Unexpected resource id ('{expectedResourceId}' != '{response.Properties.LinkedEnvironmentMetadata.ResourceId}')");
            Assert.AreEqual(expectedFriendlyName, response.Properties.LinkedEnvironmentMetadata.FriendlyName, $"Unexpected friendly name ('{expectedFriendlyName}' != '{response.Properties.LinkedEnvironmentMetadata.FriendlyName}')");
            Assert.AreEqual(expectedUniqueName, response.Properties.LinkedEnvironmentMetadata.UniqueName, $"Unexpected unique name ('{expectedUniqueName}' != '{response.Properties.LinkedEnvironmentMetadata.UniqueName}')");
            Assert.AreEqual(expectedDomainName, response.Properties.LinkedEnvironmentMetadata.DomainName, $"Unexpected domain name ('{expectedDomainName}' != '{response.Properties.LinkedEnvironmentMetadata.DomainName}')");
        }

        /// <summary>
        /// Test success of CreateEnvironmentAsync().
        /// </summary>
        [TestMethod]
        public void CreateEnvironmentAsyncSuccess()
        {
            string expectedEnvironmentName = Guid.NewGuid().ToString();
            string expectedLocation = "unitedstates";
            string expectedDisplayName = "TestEnvironment";
            string expectedSku = "Trial";
            string expectedRequestUri = $"https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/scopes/admin/environments?$filter=properties.displayName%20eq%20'{expectedDisplayName}'&api-version=2016-11-01";
            string expectedRequestUri2 = "https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/environments?api-version=2018-01-01&id=/providers/Microsoft.BusinessAppPlatform/scopes/admin/environments";
            string responseFilePath = @"./data/templates/responses/powerApps/emptyValueResponse.json";
            string responseFilePath2 = @"./data/templates/responses/powerApps/environment.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));
            HttpRequestMessage expectedRequest2 = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri2);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest2));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                null);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));
            HttpResponseMessage expectedResponse2 = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath2,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "environmentName", expectedEnvironmentName },
                    { "location", expectedLocation },
                    { "displayName", expectedDisplayName },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri2,
                new ExpectedResponse(expectedResponse2));

            IPowerAppsClient client = new PowerAppsClient(_tokenProvider);

            CreatePowerAppsEnvironmentRequest powerAppsEnv = new CreatePowerAppsEnvironmentRequest()
            {
                Location = expectedLocation,
                Properties = new NewPowerAppsEnvironmentProperties()
                {
                    DisplayName = expectedDisplayName,
                    EnvironmentSku = expectedSku,
                },
            };

            CreatePowerAppsEnvironmentResponse response = client.CreateEnvironmentAsync(powerAppsEnv).Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.IsNotNull(response.Properties, "The response Properties member should not be null!");
            Assert.AreEqual(expectedEnvironmentName, response.Name, $"Unexpected name ('{expectedEnvironmentName}' != '{response.Name}')");
            Assert.AreEqual(expectedLocation, response.Location, $"Unexpected location ('{expectedLocation}' != '{response.Location}')");
            Assert.AreEqual(expectedDisplayName, response.Properties.DisplayName, $"Unexpected location ('{expectedDisplayName}' != '{response.Properties.DisplayName}')");
            Assert.AreEqual(expectedDisplayName, response.Properties.DisplayName, $"Unexpected location ('{expectedDisplayName}' != '{response.Properties.DisplayName}')");
            Assert.IsNotNull(response.Properties.LinkedEnvironmentMetadata, "The response Properties.LinkedEnvironmentMetadata member should not be null!");
        }

        /// <summary>
        /// Test success of DeleteEnvironmentAsync().
        /// </summary>
        [TestMethod]
        public void DeleteEnvironmentAsyncSuccess()
        {
            string expectedEnvironmentName = Guid.NewGuid().ToString();
            string expectedRequestUri = $"https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/scopes/admin/environments/{expectedEnvironmentName}?api-version=2018-01-01";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Delete,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.Accepted,
                null,
                null,
                "application/json",
                null);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IPowerAppsClient client = new PowerAppsClient(_tokenProvider);

            AzureResponseBase response = client.DeleteEnvironmentAsync(expectedEnvironmentName).Result;

            Assert.IsNotNull(response, "The response should not be null!");
        }

        /// <summary>
        /// Test success of GetCdsDatabaseCurrenciesAsync().
        /// </summary>
        [TestMethod]
        public void GetCdsDatabaseCurrenciesAsyncSuccess()
        {
            string expectedLocation = "unitedstates";
            string expectedRequestUri = $"https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/locations/{expectedLocation}/environmentCurrencies?api-version=2016-11-01";
            Dictionary<string, string> expectedCurrencies = new Dictionary<string, string>()
            {
                { "location", expectedLocation },
                { "name1", "XCD" },
                { "code1", "XCD" },
                { "symbol1", "EC$" },
                { "name2", "USD" },
                { "code2", "USD" },
                { "symbol2", "US$" },
                { "name3", "AED" },
                { "code3", "AED" },
                { "symbol3", "د.إ." },
            };
            string responseFilePath = @"./data/templates/responses/powerApps/getCdsDatabaseCurrencies.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                expectedCurrencies);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IPowerAppsClient client = new PowerAppsClient(_tokenProvider);

            GetPowerAppsCurrenciesResponse response = client.GetCdsDatabaseCurrenciesAsync(expectedLocation).Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.IsNotNull(response.Value, "The response Value member should not be null!");
            Assert.AreEqual(3, response.Value.Length, $"Unexpected number of locations ('3' != '{response.Value.Length}')!");
            Assert.AreEqual(expectedCurrencies["name1"], response.Value[0].Name, $"Unexpected name for currency 1 ('{expectedCurrencies["name1"]}' != '{response.Value[0].Name}')");
            Assert.IsNotNull(response.Value[0].Properties, "The response Value Properties member should not be null for currency 1!");
            Assert.AreEqual(expectedCurrencies["code1"], response.Value[0].Properties.Code, $"Unexpected name for currency 1 ('{expectedCurrencies["code1"]}' != '{response.Value[0].Name}')");
            Assert.AreEqual(expectedCurrencies["symbol1"], response.Value[0].Properties.Symbol, $"Unexpected symbol for currency 1 ('{expectedCurrencies["symbol1"]}' != '{response.Value[0].Properties.Symbol}')");
            Assert.AreEqual(expectedCurrencies["name2"], response.Value[1].Name, $"Unexpected name for currency 2 ('{expectedCurrencies["name2"]}' != '{response.Value[1].Name}')");
            Assert.IsNotNull(response.Value[1].Properties, "The response Value Properties member should not be null for currency 2!");
            Assert.AreEqual(expectedCurrencies["code2"], response.Value[1].Properties.Code, $"Unexpected name for currency 2 ('{expectedCurrencies["code2"]}' != '{response.Value[1].Name}')");
            Assert.AreEqual(expectedCurrencies["symbol2"], response.Value[1].Properties.Symbol, $"Unexpected symbol for currency 2 ('{expectedCurrencies["symbol2"]}' != '{response.Value[1].Properties.Symbol}')");
            Assert.AreEqual(expectedCurrencies["name3"], response.Value[2].Name, $"Unexpected name for currency 3 ('{expectedCurrencies["name3"]}' != '{response.Value[2].Name}')");
            Assert.IsNotNull(response.Value[2].Properties, "The response Value Properties member should not be null for currency 3!");
            Assert.AreEqual(expectedCurrencies["code3"], response.Value[2].Properties.Code, $"Unexpected name for currency 3 ('{expectedCurrencies["code3"]}' != '{response.Value[2].Name}')");
            Assert.AreEqual(expectedCurrencies["symbol3"], response.Value[2].Properties.Symbol, $"Unexpected symbol for currency 3 ('{expectedCurrencies["symbol3"]}' != '{response.Value[2].Properties.Symbol}')");
        }

        /// <summary>
        /// Test success of GetCdsDatabaseLanguagesAsync().
        /// </summary>
        [TestMethod]
        public void GetCdsDatabaseLanguagesAsyncSuccess()
        {
            string expectedLocation = "unitedstates";
            string expectedRequestUri = $"https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/locations/{expectedLocation}/environmentLanguages?api-version=2016-11-01";
            Dictionary<string, string> expectedLanguages = new Dictionary<string, string>()
            {
                { "location", expectedLocation },
                { "name1", "1033" },
                { "displayName1", "English" },
                { "name2", "1025" },
                { "displayName2", "Arabic" },
                { "name3", "1069" },
                { "displayName3", "euskara (euskara)" },
            };
            string responseFilePath = @"./data/templates/responses/powerApps/getCdsDatabaseLanguages.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                expectedLanguages);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IPowerAppsClient client = new PowerAppsClient(_tokenProvider);

            GetPowerAppsLanguagesResponse response = client.GetCdsDatabaseLanguagesAsync(expectedLocation).Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.IsNotNull(response.Value, "The response Value member should not be null!");
            Assert.AreEqual(3, response.Value.Length, $"Unexpected number of locations ('3' != '{response.Value.Length}')!");
            Assert.AreEqual(expectedLanguages["name1"], response.Value[0].Name, $"Unexpected name for language 1 ('{expectedLanguages["name1"]}' != '{response.Value[0].Name}')");
            Assert.IsNotNull(response.Value[0].Properties, "The response Value Properties member should not be null for language 1!");
            Assert.AreEqual(expectedLanguages["displayName1"], response.Value[0].Properties.DisplayName, $"Unexpected display name for language 1 ('{expectedLanguages["displayName1"]}' != '{response.Value[0].Properties.DisplayName}')");
            Assert.AreEqual(expectedLanguages["name2"], response.Value[1].Name, $"Unexpected name for language 2 ('{expectedLanguages["name2"]}' != '{response.Value[1].Name}')");
            Assert.IsNotNull(response.Value[1].Properties, "The response Value Properties member should not be null for language 2!");
            Assert.AreEqual(expectedLanguages["displayName2"], response.Value[1].Properties.DisplayName, $"Unexpected display name for language 2 ('{expectedLanguages["displayName2"]}' != '{response.Value[1].Properties.DisplayName}')");
            Assert.AreEqual(expectedLanguages["name3"], response.Value[2].Name, $"Unexpected name for language 3 ('{expectedLanguages["name3"]}' != '{response.Value[2].Name}')");
            Assert.IsNotNull(response.Value[2].Properties, "The response Value Properties member should not be null for language 3!");
            Assert.AreEqual(expectedLanguages["displayName3"], response.Value[2].Properties.DisplayName, $"Unexpected display name for language 3 ('{expectedLanguages["displayName3"]}' != '{response.Value[2].Properties.DisplayName}')");
        }

        /// <summary>
        /// Test success of CreateCdsDatabase().
        /// </summary>
        [TestMethod]
        public void CreateCdsDatabaseSuccess()
        {
            string expectedEnvironmentName = Guid.NewGuid().ToString();
            string expectedOperationId = Guid.NewGuid().ToString();
            string expectedBaseLanguage = "1033";
            string expectedCurrency = "USD";
            string expectedLocation = "unitedstates";
            string expectedDisplayName = "TestEnvironment";
            string expectedResourceId = Guid.NewGuid().ToString();
            string expectedFriendlyName = "TestEnvironment";
            string expectedUniqueName = "orgtest4";
            string expectedDomainName = "orgtest5";
            string expectedRequestUri = $"https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/scopes/admin/environments/{expectedEnvironmentName}?$expand=permissions&api-version=2016-11-01";
            string expectedRequestUri2 = $"https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/environments/{expectedEnvironmentName}/provisionInstance?api-version=2018-01-01";
            string expectedOperationRequestUri = $"https://api.bap.microsoft.com/providers/Microsoft.BusinessAppPlatform/environments/{expectedEnvironmentName}/provisionOperations/{expectedOperationId}?api-version=2018-01-01";
            string responseFilePath = @"./data/templates/responses/powerApps/environmentNoCds.json";
            string responseFilePath2 = @"./data/templates/responses/powerApps/environment.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));
            HttpRequestMessage expectedRequest2 = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri2);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest2));
            HttpRequestMessage expectedRequest3 = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedOperationRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest3));
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest3));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "environmentName", expectedEnvironmentName },
                    { "location", expectedLocation },
                    { "displayName", expectedDisplayName },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));
            HttpResponseMessage expectedResponse2 = TestHelper.CreateHttpResponse(
                HttpStatusCode.Accepted,
                new Dictionary<string, string>()
                {
                    { "Location", expectedOperationRequestUri },
                },
                responseFilePath2,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "environmentName", expectedEnvironmentName },
                    { "location", expectedLocation },
                    { "displayName", expectedDisplayName },
                    { "crmResourceId", string.Empty },
                    { "crmFriendlyName", string.Empty },
                    { "crmUniqueName", string.Empty },
                    { "crmDomainName", string.Empty },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri2,
                new ExpectedResponse(expectedResponse2));
            _httpClient.RegisterExpectedResponse(
                expectedOperationRequestUri,
                new ExpectedResponse(expectedResponse2));
            HttpResponseMessage expectedResponse3 = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath2,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "environmentName", expectedEnvironmentName },
                    { "location", expectedLocation },
                    { "displayName", expectedDisplayName },
                    { "crmResourceId", expectedResourceId },
                    { "crmFriendlyName", expectedFriendlyName },
                    { "crmUniqueName", expectedUniqueName },
                    { "crmDomainName", expectedDomainName },
                });
            _httpClient.RegisterExpectedResponse(
                expectedOperationRequestUri,
                new ExpectedResponse(expectedResponse3));

            IPowerAppsClient client = new PowerAppsClient(_tokenProvider);

            CreatePowerAppsCdsDatabaseRequest cdsDatabase = new CreatePowerAppsCdsDatabaseRequest()
            {
                BaseLanguage = expectedBaseLanguage,
                Currency = new PowerAppsCdsDatabaseCurrencyMinimal()
                {
                    Code = expectedCurrency,
                },
            };

            PowerAppsEnvironment response = client.CreateCdsDatabase(
                expectedEnvironmentName,
                cdsDatabase);

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.IsNotNull(response.Properties, "The response Properties member should not be null!");
            Assert.AreEqual(expectedEnvironmentName, response.Name, $"Unexpected name ('{expectedEnvironmentName}' != '{response.Name}')");
            Assert.AreEqual(expectedLocation, response.Location, $"Unexpected location ('{expectedLocation}' != '{response.Location}')");
            Assert.AreEqual(expectedDisplayName, response.Properties.DisplayName, $"Unexpected location ('{expectedDisplayName}' != '{response.Properties.DisplayName}')");
            Assert.AreEqual(expectedDisplayName, response.Properties.DisplayName, $"Unexpected location ('{expectedDisplayName}' != '{response.Properties.DisplayName}')");
            Assert.IsNotNull(response.Properties.LinkedEnvironmentMetadata, "The response Properties.LinkedEnvironmentMetadata member should not be null!");
            Assert.AreEqual(expectedResourceId, response.Properties.LinkedEnvironmentMetadata.ResourceId, $"Unexpected resource id ('{expectedResourceId}' != '{response.Properties.LinkedEnvironmentMetadata.ResourceId}')");
            Assert.AreEqual(expectedFriendlyName, response.Properties.LinkedEnvironmentMetadata.FriendlyName, $"Unexpected friendly name ('{expectedFriendlyName}' != '{response.Properties.LinkedEnvironmentMetadata.FriendlyName}')");
            Assert.AreEqual(expectedUniqueName, response.Properties.LinkedEnvironmentMetadata.UniqueName, $"Unexpected unique name ('{expectedUniqueName}' != '{response.Properties.LinkedEnvironmentMetadata.UniqueName}')");
            Assert.AreEqual(expectedDomainName, response.Properties.LinkedEnvironmentMetadata.DomainName, $"Unexpected domain name ('{expectedDomainName}' != '{response.Properties.LinkedEnvironmentMetadata.DomainName}')");
        }
    }
}
