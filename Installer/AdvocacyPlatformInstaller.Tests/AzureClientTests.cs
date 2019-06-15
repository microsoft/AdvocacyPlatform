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
    /// Unit tests for AzureClient implementation.
    /// </summary>
    [TestClass]
    public class AzureClientTests
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
        /// Test success of GetTenantsAsync().
        /// </summary>
        [TestMethod]
        public void GetTenantsAsyncSuccess()
        {
            string expectedRequestUri = "https://management.azure.com/tenants?api-version=2016-06-01";
            string expectedObjectId = Guid.NewGuid().ToString();
            string expectedTenantId = Guid.NewGuid().ToString();
            string responseFilePath = @"./data/templates/responses/azure/getTenants-1tenant.json";

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
                    { "id", expectedObjectId },
                    { "tenantId", expectedTenantId },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);

            Tenant[] response = client.GetTenantsAsync().Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.IsTrue(response.Length == 1, $"The array returned in the response has an invalid length (1 != {response.Length})!");
            Assert.AreEqual(expectedObjectId, response[0].Id, $"Unexpected object id for the returned tenant ('{expectedObjectId}' != '{response[0].Id}')");
            Assert.AreEqual(expectedTenantId, response[0].TenantId, $"Unexpected tenant id for the returned tenant ('{expectedTenantId}' != '{response[0].TenantId}')!");
        }

        /// <summary>
        /// Test success of GetSubscriptionsAsync().
        /// </summary>
        [TestMethod]
        public void GetSubscriptionsAsyncSuccess()
        {
            string expectedRequestUri = "https://management.azure.com/subscriptions?api-version=2016-06-01";
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedDisplayName = "TEST-SUB1";
            string responseFilePath = @"./data/templates/responses/azure/getSubscriptions-1subscription.json";

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
                    { "subscriptionId", expectedSubscriptionId },
                    { "displayName", expectedDisplayName },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);

            Subscription[] response = client.GetSubscriptionsAsync().Result;

            Assert.IsNotNull(response, "The response should not be null!");
            Assert.IsTrue(response.Length == 1, $"The array returned in the response has an invalid length (1 != {response.Length})!");
            Assert.AreEqual(expectedSubscriptionId, response[0].SubscriptionId, $"Unexpected subscription id for the returned subscription ('{expectedSubscriptionId}' != '{response[0].SubscriptionId}')");
            Assert.AreEqual(expectedDisplayName, response[0].DisplayName, $"Unexpected subscription display name for the returned subscription ('{expectedDisplayName}' != '{response[0].DisplayName}')!");
        }

        /// <summary>
        /// Test success of ResourceGroupExistsAsync().
        /// </summary>
        [TestMethod]
        public void ResourceGroupExistsAsyncSuccess()
        {
            string expectedResourceGroupName = "test-wu-rg";
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourcegroups/{expectedResourceGroupName}?api-version=2018-05-01";
            string responseFilePath = @"./data/templates/responses/azure/resourceGroupExists-False.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.NotFound,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "resourceGroup", expectedResourceGroupName },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);

            bool response = client.ResourceGroupExistsAsync(
                expectedSubscriptionId,
                expectedResourceGroupName).Result;

            Assert.IsFalse(response, "The response should have been false!");
        }

        /// <summary>
        /// Test success of GetResourceGroupsAsync().
        /// </summary>
        [TestMethod]
        public void GetResourceGroupsAsyncSuccess()
        {
            string expectedResourceGroupName = "test-wu-rg";
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            Dictionary<string, string> tags = new Dictionary<string, string>()
            {
                { "test-tag", "test-value" },
            };
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourcegroups?api-version=2018-05-01";
            string responseFilePath = @"./data/templates/responses/azure/getResourceGroups-1resourceGroup.json";

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
                    { "resourceGroup", expectedResourceGroupName },
                    { "tags", TestHelper.BuildJsonDictionaryContents(tags) },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            ResourceGroup[] response = client.GetResourceGroupsAsync(expectedSubscriptionId).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(1, response.Length, "There should only have been 1 resource groups returned!");

            ResourceGroup resourceGroup = response[0];

            Assert.AreEqual(expectedResourceGroupName, resourceGroup.Name, $"Unexpected name ('{expectedResourceGroupName}' != '{resourceGroup.Name}')!");
            Assert.IsNotNull(resourceGroup.Tags, "Response object should have tag!");
            Assert.AreEqual(tags.Count, resourceGroup.Tags.Count, $"Unexpected count of tags ('{tags.Count}' != '{resourceGroup.Tags.Count}')!");

            TestHelper.VerifyDictionaryContents(tags, resourceGroup.Tags);
        }

        /// <summary>
        /// Test success of GetResourceBaseAsync().
        /// </summary>
        [TestMethod]
        public void GetResourceBaseAsyncSuccess()
        {
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedResourceProvider = "Microsoft.Web";
            string expectedResourceType = "sites";
            string expectedResourceName = "test-wu-as-func";
            string expectedResourceKind = "function";
            string expectedApiVersion = "2018-11-01";
            string expectedIdentityPrincipalId = Guid.NewGuid().ToString();
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourcegroups/{expectedResourceGroup}/providers/{expectedResourceProvider}/{expectedResourceType}/{expectedResourceName}?api-version={expectedApiVersion}";
            string responseFilePath = @"./data/templates/responses/azure/getResource-Generic.json";

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
                    { "subscriptionId", expectedSubscriptionId },
                    { "resourceGroup", expectedResourceGroup },
                    { "resourceProvider", expectedResourceProvider },
                    { "resourceType", expectedResourceType },
                    { "resourceName", expectedResourceName },
                    { "resourceKind", expectedResourceKind },
                    { "tenantId", expectedTenantId },
                    { "identityPrincipalId", expectedIdentityPrincipalId },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            AzureIdentityResourceBase response = client.GetResourceIdentityBaseAsync(
                expectedSubscriptionId,
                expectedResourceGroup,
                expectedResourceProvider,
                null,
                expectedResourceType,
                expectedResourceName,
                expectedApiVersion).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedResourceName, response.Name, $"Unexpected resource name ('{expectedResourceName}' != '{response.Name}')");
            Assert.AreEqual($"{expectedResourceProvider}/{expectedResourceType}", response.Type, $"Unexpected resource type ('{expectedResourceProvider}/{expectedResourceType}' != '{response.Type}')");
            Assert.IsNotNull(response.Identity, "Response identity member should not be null!");
            Assert.AreEqual(expectedTenantId, response.Identity.TenantId, $"Unexpected tenant id ('{expectedTenantId}' != '{response.Identity.TenantId}')");
        }

        /// <summary>
        /// Test success of GetResourceGroupLocksAsync().
        /// </summary>
        [TestMethod]
        public void GetResourceGroupLocksAsyncSuccess()
        {
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedResourceProvider = "Microsoft.Web";
            string expectedResourceType = "sites";
            string expectedResourceName = "test-wu-as-func";
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourceGroups/{expectedResourceGroup}/providers/Microsoft.Authorization/locks?api-version=2016-09-01";
            string responseFilePath = @"./data/templates/responses/azure/getResourceGroupLocks-1resourceGroupLock.json";

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
                    { "subscriptionId", expectedSubscriptionId },
                    { "resourceGroup", expectedResourceGroup },
                    { "resourceProvider", expectedResourceProvider },
                    { "resourceType", expectedResourceType },
                    { "resourceName", expectedResourceName },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            AzureValueCollectionResponse<ResourceLock> response = client.GetResourceGroupLocksAsync(
                expectedSubscriptionId,
                expectedResourceGroup).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.IsNotNull(response.Value, "Response object contains no locks!");
            Assert.AreEqual(1, response.Value.Length, "Response object should only contain 1 lock!");
            Assert.AreEqual(expectedResourceName, response.Value[0].Name, $"Unexpected resource lock for resource ('{expectedResourceName}' != '{response.Value[0].Name}')");
        }

        /// <summary>
        /// Test success of DeleteResourceLockAsync().
        /// </summary>
        [TestMethod]
        public void DeleteResourceLockAsyncSuccess()
        {
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedResourceProvider = "Microsoft.Web";
            string expectedResourceType = "sites";
            string expectedResourceName = "test-wu-as-func";
            string expectedLockName = "test-wu-as-func-lock";
            string expectedScope = $"/subscriptions/{expectedSubscriptionId}/resourceGroups/{expectedResourceGroup}/providers/{expectedResourceProvider}/{expectedResourceType}/{expectedResourceName}/providers/Microsoft.Authorization/locks/{expectedLockName}";
            string expectedRequestUri = $"https://management.azure.com/{expectedScope}?api-version=2016-09-01";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Delete,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                null,
                "application/json",
                null);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            string response = client.DeleteResourceLockAsync(expectedScope).Result;

            Assert.IsNull(response, "The response should have no content!");
        }

        /// <summary>
        /// Test success of ValidateResourceGroupDeploymentAsync().
        /// </summary>
        [TestMethod]
        public void ValidateResourceGroupDeploymentAsyncSuccess()
        {
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedResourceProvider = "Microsoft.Web";
            string expectedResourceType = "sites";
            string expectedResourceName = "test-wu-as-func";
            string expectedResourceKind = "function";
            string expectedValidationName = "validateResourceGroupDeployment";
            string expectedTemplateFilePath = @"./data/templates/requests/azure/armTemplate.json";
            string expectedTemplateParametersFilePath = @"./data/templates/requests/azure/armTemplate.parameters.json";
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourcegroups/{expectedResourceGroup}/providers/Microsoft.Resources/deployments/{expectedValidationName}/validate?api-version=2018-05-01";
            string responseFilePath = @"./data/templates/responses/azure/validateResourceGroupDeployment.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "subscriptionId", expectedSubscriptionId },
                    { "resourceGroup", expectedResourceGroup },
                    { "validationName", expectedValidationName },
                    { "resourceProvider", expectedResourceProvider },
                    { "resourceType", expectedResourceType },
                    { "resourceName", expectedResourceName },
                    { "resourceKind", expectedResourceKind },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            bool response = client.ValidateResourceGroupDeploymentAsync(
                expectedSubscriptionId,
                expectedResourceGroup,
                expectedValidationName,
                expectedTemplateFilePath,
                expectedTemplateParametersFilePath).Result;

            Assert.IsTrue(response, "Response should be true!");
        }

        /// <summary>
        /// Test success of CreateResourceGroupDeployment().
        /// </summary>
        [TestMethod]
        public void CreateResourceGroupDeploymentSuccess()
        {
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedResourceProvider = "Microsoft.Web";
            string expectedResourceType = "sites";
            string expectedResourceName = "test-wu-as-func";
            string expectedResourceKind = "function";
            string expectedDeploymentName = "resourceGroupDeployment";
            string expectedTemplateFilePath = @"./data/templates/requests/azure/armTemplate.json";
            string expectedTemplateParametersFilePath = @"./data/templates/requests/azure/armTemplate.parameters.json";
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourcegroups/{expectedResourceGroup}/providers/Microsoft.Resources/deployments/{expectedDeploymentName}?api-version=2018-05-01";
            string expectedOperationRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourcegroups/{expectedResourceGroup}/providers/Microsoft.Resources/deployments/{expectedDeploymentName}/operationStatuses/08586425968504329067?api-version=2018-05-01";
            string responseFilePath = @"./data/templates/responses/azure/resourceGroupDeployment.json";
            string operationResponseFilePath = @"./data/templates/responses/azure/resourceGroupDeployment-Status.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Put,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));
            HttpRequestMessage expectedOperationRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedOperationRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedOperationRequest));
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedOperationRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                new Dictionary<string, string>()
                {
                    { "Azure-AsyncOperation", expectedOperationRequestUri },
                },
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "subscriptionId", expectedSubscriptionId },
                    { "resourceGroup", expectedResourceGroup },
                    { "validationName", expectedDeploymentName },
                    { "resourceProvider", expectedResourceProvider },
                    { "resourceType", expectedResourceType },
                    { "resourceName", expectedResourceName },
                    { "resourceKind", expectedResourceKind },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));
            HttpResponseMessage expectedStatusResponse1 = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                operationResponseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "status", "Running" },
                });
            _httpClient.RegisterExpectedResponse(
                expectedOperationRequestUri,
                new ExpectedResponse(expectedStatusResponse1));
            HttpResponseMessage expectedStatusResponse2 = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                operationResponseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "status", "Succeeded" },
                });
            _httpClient.RegisterExpectedResponse(
                expectedOperationRequestUri,
                new ExpectedResponse(expectedStatusResponse2));

            IAzureClient client = new AzureClient(_tokenProvider);
            ResourceGroupDeploymentStatus response = client.CreateResourceGroupDeployment(
                expectedSubscriptionId,
                expectedResourceGroup,
                expectedDeploymentName,
                expectedTemplateFilePath,
                expectedTemplateParametersFilePath);

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual("Succeeded", response.Status);
        }

        /// <summary>
        /// Test success of GetResourceGroupAsync().
        /// </summary>
        [TestMethod]
        public void GetResourceGroupAsyncSuccess()
        {
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedLocation = "westus";
            Dictionary<string, string> tags = new Dictionary<string, string>()
            {
                { "test-tag", "test-value" },
            };
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourcegroups/{expectedResourceGroup}?api-version=2018-05-01";
            string responseFilePath = @"./data/templates/responses/azure/resourceGroup.json";

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
                    { "resourceGroup", expectedResourceGroup },
                    { "location", expectedLocation },
                    { "tags", TestHelper.BuildJsonDictionaryContents(tags) },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            ResourceGroup response = client.GetResourceGroupAsync(
                expectedSubscriptionId,
                expectedResourceGroup).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedResourceGroup, response.Name);
            Assert.AreEqual(expectedLocation, response.Location);
            Assert.IsNotNull(response.Tags, "Response object should have tag!");
            Assert.AreEqual(tags.Count, response.Tags.Count, $"Unexpected count of tags ('{tags.Count}' != '{response.Tags.Count}')!");

            TestHelper.VerifyDictionaryContents(tags, response.Tags);
        }

        /// <summary>
        /// Test success of CreateResourceGroupAsync().
        /// </summary>
        [TestMethod]
        public void CreateResourceGroupAsyncSuccess()
        {
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedLocation = "westus";
            Dictionary<string, string> tags = new Dictionary<string, string>()
            {
                { "test-tag", "test-value" },
            };
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourcegroups/{expectedResourceGroup}?api-version=2018-05-01";
            string responseFilePath = @"./data/templates/responses/azure/resourceGroup.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Put,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "subscriptionId", expectedSubscriptionId },
                    { "resourceGroup", expectedResourceGroup },
                    { "location", expectedLocation },
                    { "tags", TestHelper.BuildJsonDictionaryContents(tags) },
                    { "provisioningState", "Succeeded" },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            ResourceGroup resourceGroup = new ResourceGroup()
            {
                Name = expectedResourceGroup,
                Location = expectedLocation,
                Tags = tags,
            };

            IAzureClient client = new AzureClient(_tokenProvider);
            ResourceGroup response = client.CreateOrUpdateResourceGroupAsync(
                expectedSubscriptionId,
                resourceGroup).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedResourceGroup, response.Name);
            Assert.AreEqual(expectedLocation, response.Location);
            Assert.IsNotNull(response.Tags, "Response object should have tags!");
            Assert.AreEqual(tags.Count, response.Tags.Count, $"Unexpected number of tags ('{tags.Count}' !+ '{response.Tags.Count}')!");

            TestHelper.VerifyDictionaryContents(tags, response.Tags);
        }

        /// <summary>
        /// Test success of DeleteResourceGroup().
        /// </summary>
        [TestMethod]
        public void DeleteResourceGroupSuccess()
        {
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourcegroups/{expectedResourceGroup}?api-version=2018-05-01";
            string expectedOperationRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/operationresults/sdfjsdoiroih234234234SDFWw234234?api-version=2018-05-01";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Delete,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));
            HttpRequestMessage expectedOperationRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedOperationRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedOperationRequest));
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedOperationRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.Accepted,
                new Dictionary<string, string>()
                {
                    { "Location", expectedOperationRequestUri },
                },
                null,
                "application/json",
                null);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));
            HttpResponseMessage expectedOperationResponse1 = TestHelper.CreateHttpResponse(
                HttpStatusCode.Accepted,
                null,
                null,
                "application/json",
                null);
            _httpClient.RegisterExpectedResponse(
                expectedOperationRequestUri,
                new ExpectedResponse(expectedOperationResponse1));
            HttpResponseMessage expectedOperationResponse2 = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                null,
                "application/json",
                null);
            _httpClient.RegisterExpectedResponse(
                expectedOperationRequestUri,
                new ExpectedResponse(expectedOperationResponse2));

            IAzureClient client = new AzureClient(_tokenProvider);
            bool response = client.DeleteResourceGroup(
                expectedSubscriptionId,
                expectedResourceGroup);

            Assert.AreEqual(true, response, "Response should be true!");
        }

        /// <summary>
        /// Test success of GetApplicationAsync().
        /// </summary>
        [TestMethod]
        public void GetApplicationAsyncSuccess()
        {
            string expectedDisplayName = "app-test-aad";
            string expectedObjectId = Guid.NewGuid().ToString();
            string expectedAppId = Guid.NewGuid().ToString();
            string expectedIdentifierUri = "https://test-app.azurewebsites.net";
            string expectedDomain = "test-app";
            string expectedSignInAudience = "AzureADMyOrg";
            string expectedKeyId = "test-key";
            string expectedKeyHint = "test-hint";
            string expectedResourceAppId = Guid.NewGuid().ToString();
            string expectedAccessObjectId = Guid.NewGuid().ToString();
            string expectedAccessScopeOrRole = "Scope";
            string expectedRequestUri = $"https://graph.microsoft.com/beta/applications?$filter=displayName%20eq%20'{expectedDisplayName}' ";
            string responseFilePath = @"./data/templates/responses/azure/getApplication.json";

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
                    { "displayName", expectedDisplayName },
                    { "objectId", expectedObjectId },
                    { "appId", expectedAppId },
                    { "identifierUri", expectedIdentifierUri },
                    { "domain", expectedDomain },
                    { "signInAudience", expectedSignInAudience },
                    { "keyId", expectedKeyId },
                    { "keyHint", expectedKeyHint },
                    { "resourceAppId", expectedResourceAppId },
                    { "accessObjectId", expectedAccessObjectId },
                    { "accessScopeOrRole", expectedAccessScopeOrRole },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequest.RequestUri.AbsoluteUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            AzureApplication response = client.GetApplicationAsync(expectedDisplayName).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedDisplayName, response.DisplayName, $"Unexpected display name ('{expectedDisplayName}' != '{response.DisplayName}')");
            Assert.AreEqual(expectedObjectId, response.Id, $"Unexpected object id ('{expectedObjectId}' != '{response.Id}')");
            Assert.AreEqual(expectedAppId, response.AppId, $"Unexpected application id ('{expectedAppId}' != '{response.AppId}')");
            Assert.AreEqual(1, response.IdentifierUris.Length, $"There should only be 1 identifier uri (1 != {response.IdentifierUris.Length})!");
            Assert.AreEqual(expectedIdentifierUri, response.IdentifierUris[0], $"Unexpected identifier uri ('{expectedIdentifierUri}' != '{response.IdentifierUris[0]}')");
            Assert.AreEqual($"{expectedDomain}.onmicrosoft.com", response.PublisherDomain, $"Unexpected domain ('{expectedDomain}.onmicrosoft.com' != '{response.PublisherDomain}')");
            Assert.AreEqual(expectedSignInAudience, response.SignInAudience, $"Unexpected sign in audience ('{expectedSignInAudience}' != '{response.SignInAudience}')");
            Assert.AreEqual(1, response.PasswordCredentials.Length, $"There should only be 1 password credential (1 != {response.PasswordCredentials.Length})");
            Assert.AreEqual(1, response.RequiredResourceAccess.Length, $"There should only be 1 required resource access (1 != {response.RequiredResourceAccess.Length})");
            Assert.AreEqual(expectedResourceAppId, response.RequiredResourceAccess[0].ResourceAppId, $"Unexpected resource app id ('{expectedResourceAppId}' != '{response.RequiredResourceAccess[0].ResourceAppId}')");
            Assert.AreEqual(1, response.RequiredResourceAccess[0].ResourceAccess.Length, $"There should only be 1 API permission (1 != '{response.RequiredResourceAccess[0].ResourceAccess.Length}')!");
            Assert.AreEqual(expectedAccessObjectId, response.RequiredResourceAccess[0].ResourceAccess[0].Id, $"Unexpected resource access object id ('{expectedAccessObjectId}' != '{response.RequiredResourceAccess[0].ResourceAccess[0].Id}')");
            Assert.AreEqual(expectedAccessScopeOrRole, response.RequiredResourceAccess[0].ResourceAccess[0].Type, $"Unexpected resource access type ('{expectedAccessScopeOrRole}' != '{response.RequiredResourceAccess[0].ResourceAccess[0].Type}')");
        }

        /// <summary>
        /// Test success of CreateApplicationAsync().
        /// </summary>
        [TestMethod]
        public void CreateApplicationAsyncSuccess()
        {
            string expectedDisplayName = "app-test-aad";
            string expectedObjectId = Guid.NewGuid().ToString();
            string expectedAppId = Guid.NewGuid().ToString();
            string expectedIdentifierUri = "https://test-app.azurewebsites.net";
            string expectedDomain = "test-app";
            string expectedSignInAudience = "AzureADMyOrg";
            string expectedKeyId = "test-key";
            string expectedKeyHint = "test-hint";
            string expectedResourceAppId = Guid.NewGuid().ToString();
            string expectedAccessObjectId = Guid.NewGuid().ToString();
            string expectedAccessScopeOrRole = "Scope";
            string expectedRequestUri = "https://graph.microsoft.com/beta/applications";
            string responseFilePath = @"./data/templates/responses/azure/createApplication.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "displayName", expectedDisplayName },
                    { "objectId", expectedObjectId },
                    { "appId", expectedAppId },
                    { "identifierUri", expectedIdentifierUri },
                    { "domain", expectedDomain },
                    { "signInAudience", expectedSignInAudience },
                    { "keyId", expectedKeyId },
                    { "keyHint", expectedKeyHint },
                    { "resourceAppId", expectedResourceAppId },
                    { "accessObjectId", expectedAccessObjectId },
                    { "accessScopeOrRole", expectedAccessScopeOrRole },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequest.RequestUri.AbsoluteUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            AzureApplicationRequestBase appRegistration = new AzureApplicationRequestBase()
            {
                DisplayName = expectedDisplayName,
                IdentifierUris = new string[] { expectedIdentifierUri },
                PasswordCredentials = new AzureApplicationPasswordCredential[]
                {
                    new AzureApplicationPasswordCredential()
                    {
                        StartDateTime = DateTime.UtcNow.ToString("o"),
                        EndDateTime = DateTime.UtcNow.AddYears(1).ToString("o"),
                        SecretText = "test-secret",
                    },
                },
                SignInAudience = expectedSignInAudience,
                RequiredResourceAccess = new AzureApplicationRequiredResourceAccess[]
                {
                    new AzureApplicationRequiredResourceAccess()
                    {
                        ResourceAppId = expectedResourceAppId,
                        ResourceAccess = new ResourceAccess[]
                        {
                            new ResourceAccess()
                            {
                                Id = expectedAccessObjectId,
                                Type = expectedAccessScopeOrRole,
                            },
                        },
                    },
                },
            };

            AzureApplication response = client.CreateApplicationAsync(appRegistration).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedDisplayName, response.DisplayName, $"Unexpected display name ('{expectedDisplayName}' != '{response.DisplayName}')");
            Assert.AreEqual(expectedObjectId, response.Id, $"Unexpected object id ('{expectedObjectId}' != '{response.Id}')");
            Assert.AreEqual(expectedAppId, response.AppId, $"Unexpected application id ('{expectedAppId}' != '{response.AppId}')");
            Assert.AreEqual(1, response.IdentifierUris.Length, $"There should only be 1 identifier uri (1 != {response.IdentifierUris.Length})!");
            Assert.AreEqual(expectedIdentifierUri, response.IdentifierUris[0], $"Unexpected identifier uri ('{expectedIdentifierUri}' != '{response.IdentifierUris[0]}')");
            Assert.AreEqual($"{expectedDomain}.onmicrosoft.com", response.PublisherDomain, $"Unexpected domain ('{expectedDomain}.onmicrosoft.com' != '{response.PublisherDomain}')");
            Assert.AreEqual(expectedSignInAudience, response.SignInAudience, $"Unexpected sign in audience ('{expectedSignInAudience}' != '{response.SignInAudience}')");
            Assert.AreEqual(1, response.PasswordCredentials.Length, $"There should only be 1 password credential (1 != {response.PasswordCredentials.Length})");
            Assert.AreEqual(1, response.RequiredResourceAccess.Length, $"There should only be 1 required resource access (1 != {response.RequiredResourceAccess.Length})");
            Assert.AreEqual(expectedResourceAppId, response.RequiredResourceAccess[0].ResourceAppId, $"Unexpected resource app id ('{expectedResourceAppId}' != '{response.RequiredResourceAccess[0].ResourceAppId}')");
            Assert.AreEqual(1, response.RequiredResourceAccess[0].ResourceAccess.Length, $"There should only be 1 API permission (1 != '{response.RequiredResourceAccess[0].ResourceAccess.Length}')!");
            Assert.AreEqual(expectedAccessObjectId, response.RequiredResourceAccess[0].ResourceAccess[0].Id, $"Unexpected resource access object id ('{expectedAccessObjectId}' != '{response.RequiredResourceAccess[0].ResourceAccess[0].Id}')");
            Assert.AreEqual(expectedAccessScopeOrRole, response.RequiredResourceAccess[0].ResourceAccess[0].Type, $"Unexpected resource access type ('{expectedAccessScopeOrRole}' != '{response.RequiredResourceAccess[0].ResourceAccess[0].Type}')");
        }

        /// <summary>
        /// Test success of DeleteApplicationAsync().
        /// </summary>
        [TestMethod]
        public void DeleteApplicationAsyncSuccess()
        {
            string expectedApplicationId = Guid.NewGuid().ToString();
            string expectedRequestUri = $"https://graph.microsoft.com/beta/applications/{expectedApplicationId}";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Delete,
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

            IAzureClient client = new AzureClient(_tokenProvider);
            bool response = client.DeleteApplicationAsync(expectedApplicationId).Result;

            Assert.IsTrue(response, "Response should be true!");
        }

        /// <summary>
        /// Test success of CreateKeyVaultAccessPolicyAsync().
        /// </summary>
        [TestMethod]
        public void CreateKeyVaultAccessPolicyAsyncSuccess()
        {
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedKeyVaultName = "test-wu-kv";
            string expectedObjectId = Guid.NewGuid().ToString();
            string[] expectedCertificatePerms = new string[0];
            string[] expectedKeyPerms = new string[0];
            string[] expectedSecretPerms = new string[] { "List", "Get", "Set" };
            string[] expectedStoragePerms = new string[0];
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourceGroups/{expectedResourceGroup}/providers/Microsoft.KeyVault/vaults/{expectedKeyVaultName}/accessPolicies/add?api-version=2018-02-14";
            string responseFilePath = @"./data/templates/responses/azure/keyVaultAccessPolicy.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Put,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "subscriptionId", expectedSubscriptionId },
                    { "resourceGroup", expectedResourceGroup },
                    { "keyVaultName", expectedKeyVaultName },
                    { "tenantId", expectedTenantId },
                    { "objectId", expectedObjectId },
                },
                new Dictionary<string, string[]>()
                {
                    { "certificatesPermArray", expectedCertificatePerms },
                    { "keysPermArray", expectedKeyPerms },
                    { "secretsPermArray", expectedSecretPerms },
                    { "storagePermArray", expectedStoragePerms },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            CreateKeyVaultAccessPolicyRequest request = new CreateKeyVaultAccessPolicyRequest()
            {
                Properties = new CreateKeyVaultAccessPolicyRequestProperties()
                {
                    AccessPolicies = new KeyVaultAccessPolicy[]
                    {
                        new KeyVaultAccessPolicy()
                        {
                            TenantId = expectedTenantId,
                            ObjectId = expectedObjectId,
                            Permissions = new KeyVaultAccessPolicyPermissions()
                            {
                                Certificates = expectedCertificatePerms,
                                Keys = expectedKeyPerms,
                                Secrets = expectedSecretPerms,
                                Storage = expectedStoragePerms,
                            },
                        },
                    },
                },
            };

            IAzureClient client = new AzureClient(_tokenProvider);
            CreateKeyVaultAccessPolicyResponse response = client.CreateKeyVaultAccessPolicyAsync(
                expectedSubscriptionId,
                expectedResourceGroup,
                expectedKeyVaultName,
                request).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.IsNotNull(response.Properties, "Response Properties member should not be null!");
            Assert.IsNotNull(response.Properties.AccessPolicies, "Response Properties AccessPolicies member should not be null!");
            Assert.AreEqual(request.Properties.AccessPolicies.Length, response.Properties.AccessPolicies.Length, $"Response should have the same number of access policies as request ('{request.Properties.AccessPolicies.Length}' != '{response.Properties.AccessPolicies.Length}')");

            int i = 0;
            foreach (KeyVaultAccessPolicy policy in request.Properties.AccessPolicies)
            {
                KeyVaultAccessPolicy responsePolicy = response.Properties.AccessPolicies[i];

                Assert.IsNotNull(policy.Permissions, "Access policy Permissions member should not be null!");
                Assert.IsNotNull(policy.Permissions.Certificates, "Access policy Permissions Certificates member should not be null!");
                Assert.AreEqual(policy.Permissions.Certificates.Length, responsePolicy.Permissions.Certificates.Length, $"Response should have the same number of permissions as request ('{policy.Permissions.Certificates.Length}' != '{responsePolicy.Permissions.Certificates.Length}')");

                TestHelper.VerifyStringArrayContents(policy.Permissions.Certificates, responsePolicy.Permissions.Certificates);

                Assert.IsNotNull(policy.Permissions.Keys, "Access policy Permissions Keys member should not be null!");
                Assert.AreEqual(policy.Permissions.Keys.Length, responsePolicy.Permissions.Keys.Length, $"Response should have the same number of permissions as request ('{policy.Permissions.Keys.Length}' != '{responsePolicy.Permissions.Keys.Length}')");

                TestHelper.VerifyStringArrayContents(policy.Permissions.Keys, responsePolicy.Permissions.Keys);

                Assert.IsNotNull(policy.Permissions.Secrets, "Access policy Permissions Secrets member should not be null!");
                Assert.AreEqual(policy.Permissions.Secrets.Length, responsePolicy.Permissions.Secrets.Length, $"Response should have the same number of permissions as request ('{policy.Permissions.Secrets.Length}' != '{responsePolicy.Permissions.Secrets.Length}')");

                TestHelper.VerifyStringArrayContents(policy.Permissions.Secrets, responsePolicy.Permissions.Secrets);

                Assert.IsNotNull(policy.Permissions.Storage, "Access policy Permissions Storage member should not be null!");
                Assert.AreEqual(policy.Permissions.Storage.Length, responsePolicy.Permissions.Storage.Length, $"Response should have the same number of permissions as request ('{policy.Permissions.Storage.Length}' != '{responsePolicy.Permissions.Storage.Length}')");

                TestHelper.VerifyStringArrayContents(policy.Permissions.Storage, responsePolicy.Permissions.Storage);
            }
        }

        /// <summary>
        /// Test success of UpdateKeyVaultSecretAsync().
        /// </summary>
        [TestMethod]
        public void UpdateKeyVaultSecretAsyncSuccess()
        {
            string expectedApplicationId = Guid.NewGuid().ToString();
            string expectedKeyVaultName = "test-wu-kv";
            string expectedSecretName = "test-secret";
            string expectedSecretId = Guid.NewGuid().ToString();
            string expectedValue = "test-secret-value";
            string expectedSecretVersion = "0123456";
            string expectedRequestUri = $"https://{expectedKeyVaultName}.vault.azure.net/secrets/{expectedSecretName}?api-version=7.0";
            string responseFilePath = @"./data/templates/responses/azure/updateKeyVaultSecret.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Put,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "value", expectedValue },
                    { "keyVaultName", expectedKeyVaultName },
                    { "secretName", expectedSecretName },
                    { "secretVersion", expectedSecretVersion },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            AzureKeyVaultSecret response = client.UpdateKeyVaultSecretAsync(
                expectedKeyVaultName,
                expectedSecretName,
                new NetworkCredential(expectedSecretName, expectedValue)).Result;

            Assert.IsNotNull(response, "Response object should not be null!");

            string expectedResponseId = $"https://{expectedKeyVaultName}.vault.azure.net/secrets/{expectedSecretName}/{expectedSecretVersion}";

            Assert.AreEqual(expectedResponseId, response.Id, $"Unexpected id ('{expectedResponseId}' != '{response.Id}')!");
            Assert.AreEqual(expectedValue, response.Value, $"Unexpected value ('{expectedValue}' != '{response.Value}'");
        }

        /// <summary>
        /// Test success of CreateBlobStoreAccessPolicyAsync().
        /// </summary>
        [TestMethod]
        public void CreateBlobStoredAccessPolicyAsyncSuccess()
        {
            string expectedAccessPolicyId = Guid.NewGuid().ToString();
            string expectedAccessPolicyStart = DateTime.UtcNow.ToString("o");
            string expectedAccessPolicyExpiry = DateTime.UtcNow.AddYears(1).ToString("o");
            string expectedAccessPolicyPermissions = "rwd";
            string expectedStorageAccountName = "teststoragewu";
            string expectedContainerName = "testcontainer";
            string expectedRequestUri = $"https://{expectedStorageAccountName}.blob.core.windows.net/{expectedContainerName}?restype=container&comp=acl";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Put,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                null,
                "application/json",
                null);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            SignedIdentifiers accessPolicies = new SignedIdentifiers()
            {
                SignedIdentifier = new SignedIdentifier[]
                {
                    new SignedIdentifier()
                    {
                        Id = expectedAccessPolicyId,
                        AccessPolicy = new AccessPolicy()
                        {
                            Start = expectedAccessPolicyStart,
                            Expiry = expectedAccessPolicyExpiry,
                            Permission = expectedAccessPolicyPermissions,
                        },
                    },
                },
            };

            IAzureClient client = new AzureClient(_tokenProvider);
            string response = client.CreateBlobStoredAccessPolicyAsync(
                expectedStorageAccountName,
                expectedContainerName,
                accessPolicies).Result;

            Assert.IsNull(response, "Response object should be null!");
        }

        /// <summary>
        /// Test success of GetAppServicePublishingProfileAsync().
        /// </summary>
        [TestMethod]
        public void GetAppServicePublishingProfileAsyncSuccess()
        {
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedAppServiceName = "test-wu-as-func";
            string expectedPassword = "test-password";
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourceGroups/{expectedResourceGroup}/providers/Microsoft.Web/sites/{expectedAppServiceName}/publishxml?api-version=2016-08-01";
            string responseFilePath = @"./data/templates/responses/azure/getAppServicePublishingProfile.xml";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "appServiceName", expectedAppServiceName },
                    { "password", expectedPassword },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            PublishData response = client.GetAppServicePublishingProfileAsync(
                expectedSubscriptionId,
                expectedResourceGroup,
                expectedAppServiceName).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(2, response.Profiles.Length, $"Unexpected profile count ('2' != '{response.Profiles.Length}')!");

            PublishProfile webDeploy = response
                .Profiles
                .Where(x => x.ProfileName.EndsWith("Web Deploy"))
                .FirstOrDefault();

            Assert.IsNotNull(webDeploy, "Could not find Web Deploy publishing profile!");
            Assert.AreEqual($"${expectedAppServiceName}", webDeploy.UserName, $"Unexpected user name ('${expectedAppServiceName}' != '{webDeploy.UserName}')!");
            Assert.AreEqual(expectedPassword, webDeploy.Password, $"Unexpected password ('{expectedPassword}' != '{webDeploy.Password}')!");
        }

        /// <summary>
        /// Test success of ZipDeployAppServiceAsync().
        /// </summary>
        [TestMethod]
        public void ZipDeployAppServiceAsyncSuccess()
        {
            string expectedAppServiceName = "test-wu-as-func";
            string expectedZipFileUri = @"https://nowhere.com/latest.zip";
            string expectedPublishProfileUserName = "test-user";
            string expectedPublishProfilePassword = "test-password";
            string expectedRequestUri = $"https://{expectedAppServiceName}.scm.azurewebsites.net/api/zipdeploy";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                null,
                "application/json",
                null);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            string response = client.ZipDeployAppServiceAsync(
                expectedAppServiceName,
                expectedZipFileUri,
                new NetworkCredential(expectedPublishProfileUserName, expectedPublishProfilePassword)).Result;

            Assert.IsNull(response, "Response should be null!");
        }

        /// <summary>
        /// Test success of GetAppServiceAuthSettingsAsync().
        /// </summary>
        [TestMethod]
        public void GetAppServiceAuthSettingsAsyncSuccess()
        {
            bool expectedEnabled = true;
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedAppServiceName = "test-wu-as-func";
            string expectedClientId = Guid.NewGuid().ToString();
            string expectedIssuer = $"https://sts.windows.net/{expectedTenantId}/";
            string expectedAllowedAudience = expectedClientId;
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourceGroups/{expectedResourceGroup}/providers/Microsoft.Web/sites/{expectedAppServiceName}/config/authsettings/list?api-version=2016-08-01";
            string responseFilePath = @"./data/templates/responses/azure/appServiceAuthSettings.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "subscriptionId", expectedSubscriptionId },
                    { "resourceGroup", expectedResourceGroup },
                    { "appServiceName", expectedAppServiceName },
                    { "clientId", expectedClientId },
                    { "issuer", expectedIssuer },
                    { "allowedAudience", expectedAllowedAudience },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            AppServiceAuthSettings response = client.GetAppServiceAuthSettingsAsync(
                expectedSubscriptionId,
                expectedResourceGroup,
                expectedAppServiceName).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedEnabled, response.Enabled, $"Unexpected enabled ('{expectedEnabled}' != '{response.Enabled}')!");
            Assert.AreEqual(expectedClientId, response.ClientId, $"Unexpected client id ('{expectedClientId}' != '{response.ClientId}')!");
            Assert.AreEqual(expectedIssuer, response.Issuer, $"Unexpected issuer ('{expectedIssuer}' != '{response.Issuer}')!");
            Assert.IsNotNull(response.AllowedAudiences, "AllowedAudiences should not be null!");
            Assert.IsInstanceOfType(response.AllowedAudiences, typeof(JArray), "Unexpected type for allowed audiences!");

            TestHelper.VerifyStringArrayContents(new string[] { expectedAllowedAudience }, ((JArray)response.AllowedAudiences).ToObject<string[]>());
        }

        /// <summary>
        /// Test success of UpdateAppServiceAuthSettingsAsync().
        /// </summary>
        [TestMethod]
        public void UpdateAppServiceAuthSettingsAsyncSuccess()
        {
            bool expectedEnabled = true;
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedAppServiceName = "test-wu-as-func";
            string expectedClientId = Guid.NewGuid().ToString();
            string expectedIssuer = $"https://sts.windows.net/{expectedTenantId}/";
            string expectedAllowedAudience = expectedClientId;
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourceGroups/{expectedResourceGroup}/providers/Microsoft.Web/sites/{expectedAppServiceName}/config/authsettings?api-version=2016-08-01";
            string responseFilePath = @"./data/templates/responses/azure/appServiceAuthSettings.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Put,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "subscriptionId", expectedSubscriptionId },
                    { "resourceGroup", expectedResourceGroup },
                    { "appServiceName", expectedAppServiceName },
                    { "clientId", expectedClientId },
                    { "issuer", expectedIssuer },
                    { "allowedAudience", expectedAllowedAudience },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            AppServiceAuthSettings authSettings = new AppServiceAuthSettings()
            {
                Enabled = expectedEnabled,
                ClientId = expectedClientId,
                Issuer = expectedIssuer,
                AllowedAudiences = new string[]
                {
                    expectedAllowedAudience,
                },
            };

            IAzureClient client = new AzureClient(_tokenProvider);
            AppServiceAuthSettings response = client.UpdateAppServiceAuthSettingsAsync(
                expectedSubscriptionId,
                expectedResourceGroup,
                expectedAppServiceName,
                authSettings).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedEnabled, response.Enabled, $"Unexpected enabled ('{expectedEnabled}' != '{response.Enabled}')!");
            Assert.AreEqual(expectedClientId, response.ClientId, $"Unexpected client id ('{expectedClientId}' != '{response.ClientId}')!");
            Assert.AreEqual(expectedIssuer, response.Issuer, $"Unexpected issuer ('{expectedIssuer}' != '{response.Issuer}')!");
            Assert.IsNotNull(response.AllowedAudiences, "AllowedAudiences should not be null!");
            Assert.IsInstanceOfType(response.AllowedAudiences, typeof(JArray), "Unexpected type for allowed audiences!");

            TestHelper.VerifyStringArrayContents(new string[] { expectedAllowedAudience }, ((JArray)response.AllowedAudiences).ToObject<string[]>());
        }

        /// <summary>
        /// Test success of GetAppServiceAppSettingsAsync().
        /// </summary>
        [TestMethod]
        public void GetAppServiceAppSettingsAsyncSuccess()
        {
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedAppServiceName = "test-wu-as-func";
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourceGroups/{expectedResourceGroup}/providers/Microsoft.Web/sites/{expectedAppServiceName}/config/appsettings/list?api-version=2016-08-01";
            string responseFilePath = @"./data/templates/responses/azure/appServiceAppSettings.json";

            Dictionary<string, string> expectedAppSettings = new Dictionary<string, string>()
            {
                { "TEST_SETTING1", "test_value_1" },
                { "TEST_SETTING2", "test_value_2" },
            };

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "subscriptionId", expectedSubscriptionId },
                    { "resourceGroup", expectedResourceGroup },
                    { "appServiceName", expectedAppServiceName },
                },
                null,
                new Dictionary<string, Dictionary<string, string>>()
                {
                    { "settings", expectedAppSettings },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            AppServiceAppSettings response = client.GetAppServiceAppSettingsAsync(
                expectedSubscriptionId,
                expectedResourceGroup,
                expectedAppServiceName).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual("appsettings", response.Name, $"Unexpected name ('appsettings' != '{response.Name}')!");
            Assert.IsNotNull(response.Properties, "Response Properties member should not be null!");
            Assert.AreEqual(expectedAppSettings.Count, response.Properties.Count, $"Unexpected app settings count ('{expectedAppSettings.Count}' != '{response.Properties.Count}')!");

            foreach (KeyValuePair<string, string> expectedAppSetting in expectedAppSettings)
            {
                Assert.IsTrue(response.Properties.ContainsKey(expectedAppSetting.Key), $"Response app settings does not contain expected app setting '{expectedAppSetting.Key}'!");
                Assert.AreEqual(expectedAppSetting.Value, response.Properties[expectedAppSetting.Key], $"Response app setting '{expectedAppSettings.Keys}' has unexpected value ('{expectedAppSetting.Value}' != '{response.Properties[expectedAppSetting.Key]}')!");
            }
        }

        /// <summary>
        /// Test success of UpdateAppServiceAuthSettingsAsync().
        /// </summary>
        [TestMethod]
        public void UpdateAppServiceAppSettingsAsyncSuccess()
        {
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedAppServiceName = "test-wu-as-func";
            string expectedRequestUri = $"https://management.azure.com/subscriptions/{expectedSubscriptionId}/resourceGroups/{expectedResourceGroup}/providers/Microsoft.Web/sites/{expectedAppServiceName}/config/appsettings?api-version=2016-08-01";
            string responseFilePath = @"./data/templates/responses/azure/appServiceAppSettings.json";

            Dictionary<string, string> expectedAppSettings = new Dictionary<string, string>()
            {
                { "TEST_SETTING1", "test_value_1" },
                { "TEST_SETTING2", "test_value_2" },
            };

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Put,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "subscriptionId", expectedSubscriptionId },
                    { "resourceGroup", expectedResourceGroup },
                    { "appServiceName", expectedAppServiceName },
                },
                null,
                new Dictionary<string, Dictionary<string, string>>()
                {
                    { "settings", expectedAppSettings },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            AppServiceAppSettings appSettings = new AppServiceAppSettings()
            {
                Name = expectedAppServiceName,
                Properties = expectedAppSettings,
            };

            IAzureClient client = new AzureClient(_tokenProvider);
            AppServiceAppSettings response = client.UpdateAppServiceAppSettingsAsync(
                expectedSubscriptionId,
                expectedResourceGroup,
                expectedAppServiceName,
                appSettings).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual("appsettings", response.Name, $"Unexpected name ('appsettings' != '{response.Name}')!");
            Assert.IsNotNull(response.Properties, "Response Properties member should not be null!");
            Assert.AreEqual(expectedAppSettings.Count, response.Properties.Count, $"Unexpected app settings count ('{expectedAppSettings.Count}' != '{response.Properties.Count}')!");

            foreach (KeyValuePair<string, string> expectedAppSetting in expectedAppSettings)
            {
                Assert.IsTrue(response.Properties.ContainsKey(expectedAppSetting.Key), $"Response app settings does not contain expected app setting '{expectedAppSetting.Key}'!");
                Assert.AreEqual(expectedAppSetting.Value, response.Properties[expectedAppSetting.Key], $"Response app setting '{expectedAppSettings.Keys}' has unexpected value ('{expectedAppSetting.Value}' != '{response.Properties[expectedAppSetting.Key]}')!");
            }
        }

        /// <summary>
        /// Test success of InvokeResourceActionAsync().
        /// </summary>
        [TestMethod]
        public void InvokeResourceActionAsyncSuccess()
        {
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedResourceProvider = "Microsoft.Web";
            string expectedResourceType = "connections";
            string expectedResourceName = "test-api-cds";
            string expectedResourceId = $"/subscriptions/{expectedSubscriptionId}/resourceGroups/{expectedResourceGroup}/providers/{expectedResourceProvider}/{expectedResourceType}/{expectedResourceName}";
            string expectedResourceAction = "confirmConsentCode";
            string expectedApiVersion = "2016-06-01";
            string expectedConsentCode = "234234swer";
            string expectedRequestUri = $"https://management.azure.com/{expectedResourceId}/{expectedResourceAction}?api-version={expectedApiVersion}";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                null,
                "application/json",
                null);
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            string parameters = $@"{{
    ""code"": ""{expectedConsentCode}""
}}";

            IAzureClient client = new AzureClient(_tokenProvider);
            ListKeysResponse response = client.InvokeResourceAction2Async<ListKeysResponse>(
                expectedResourceId,
                expectedResourceAction,
                parameters,
                expectedApiVersion).Result;

            Assert.IsNull(response, "Response object should be null!");
        }

        /// <summary>
        /// Test success of InvokeResourceAction2() for listKeys.
        /// </summary>
        [TestMethod]
        public void InvokeResourceAction2Async_GenericListKeysSuccess()
        {
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedResourceProvider = "Microsoft.Storage";
            string expectedResourceType = "storageAccounts";
            string expectedResourceName = "teststoragewu";
            string expectedResourceId = $"/subscriptions/{expectedSubscriptionId}/resourceGroups/{expectedResourceGroup}/providers/{expectedResourceProvider}/{expectedResourceType}/{expectedResourceName}";
            string expectedResourceAction = "listKeys";
            string expectedApiVersion = "2019-04-01";
            string[] expectedKeys = new string[]
            {
                "@#Fsdf23423432fsar234234234==",
                "234fsdfwe423423rsf234234233==",
            };
            string expectedRequestUri = $"https://management.azure.com/{expectedResourceId}/{expectedResourceAction}?api-version={expectedApiVersion}";
            string responseFilePath = @"./data/templates/responses/azure/listKeys.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "key1", expectedKeys[0] },
                    { "key2", expectedKeys[1] },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            ListKeysResponse response = client.InvokeResourceAction2Async<ListKeysResponse>(
                expectedResourceId,
                expectedResourceAction,
                string.Empty,
                expectedApiVersion).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.IsNotNull(response.Keys, "Response Keys member should not be null!");
            Assert.AreEqual(2, response.Keys.Length, $"Unexpected Value array length ('2' != '{response.Keys.Length}')!");
            Assert.AreEqual(expectedKeys[0], response.Keys[0].Value, $"Unexpected link ('{expectedKeys[0]}' != '{response.Keys[0].Value}')");
            Assert.AreEqual(expectedKeys[1], response.Keys[1].Value, $"Unexpected link ('{expectedKeys[1]}' != '{response.Keys[1].Value}')");
        }

        /// <summary>
        /// Test success of InvokeResourceActionAsync().
        /// </summary>
        [TestMethod]
        public void InvokeResourceActionAsyncGenericSuccess()
        {
            string expectedTenantId = Guid.NewGuid().ToString();
            string expectedSubscriptionId = Guid.NewGuid().ToString();
            string expectedResourceGroup = "test-wu-rg";
            string expectedResourceProvider = "Microsoft.Web";
            string expectedResourceType = "connections";
            string expectedResourceName = "test-api-cds";
            string expectedResourceId = $"/subscriptions/{expectedSubscriptionId}/resourceGroups/{expectedResourceGroup}/providers/{expectedResourceProvider}/{expectedResourceType}/{expectedResourceName}";
            string expectedResourceAction = "listConsentLinks";
            string expectedApiVersion = "2016-06-01";
            string expectedLink = "https://logic-apis-westus2.consent.azure-apim.net/login?data={login}";
            string expectedFirstPartyLoginUri = "https://logic-apis-westus2.consent.azure-apim.net/firstPartyLogin?data=SDFSDF@#$";
            string expectedStatus = "Unauthenticated";
            string expectedRequestUri = $"https://management.azure.com/{expectedResourceId}/{expectedResourceAction}?api-version={expectedApiVersion}";
            string responseFilePath = @"./data/templates/responses/azure/listConsentLinks.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.NoContent,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "link", expectedLink },
                    { "firstPartyLoginUri", expectedFirstPartyLoginUri },
                    { "status", expectedStatus },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            string expectedParameters = @"{
    ""parameters"": [
        {
            ""parameterName"": ""token"",
            ""redirectUrl"":""https://ema1.exp.azure.com/ema/default/authredirect""
        }
    ]
}";

            IAzureClient client = new AzureClient(_tokenProvider);
            AzureValueCollectionResponse<ApiConnectionConsentLink> response = client.InvokeResourceActionAsync<ApiConnectionConsentLink>(
                expectedResourceId,
                expectedResourceAction,
                expectedParameters,
                expectedApiVersion).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.IsNotNull(response.Value, "Response Value member should not be null!");
            Assert.AreEqual(1, response.Value.Length, $"Unexpected Value array length ('1' != '{response.Value.Length}')!");
            Assert.AreEqual(expectedLink, response.Value[0].Link, $"Unexpected link ('{expectedLink}' != '{response.Value[0].Link}')");
            Assert.IsNull(response.Value[0].DisplayName, $"Unexpected display name ('null' != '{response.Value[0].DisplayName}')!");
            Assert.AreEqual(expectedStatus, response.Value[0].Status, $"Unexpected status ('{expectedStatus}' != '{response.Value[0].Status}')");
        }

        /// <summary>
        /// Test success of GetLuisAppsAsync().
        /// </summary>
        [TestMethod]
        public void GetLuisAppsAsyncSuccess()
        {
            string expectedAuthoringKey = "sdfsdfuywoerjwoekj234234";
            string expectedApplicationId = Guid.NewGuid().ToString();
            string expectedAuthoringRegion = "westus";
            string expectedName = "TestApplication";
            string expectedDescription = "A test LUIS application.";
            string expectedRequestUri = $"https://{expectedAuthoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/";
            string responseFilePath = @"./data/templates/responses/azure/getLuisApplications.json";

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
                    { "applicationId", expectedApplicationId },
                    { "name", expectedName },
                    { "description", expectedDescription },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            LuisApplication[] response = client.GetLuisAppsAsync(
                new NetworkCredential("luis", expectedAuthoringKey),
                expectedAuthoringRegion).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(1, response.Length, $"Unexpected Value array length ('1' != '{response.Length}')!");
            Assert.AreEqual(expectedApplicationId, response[0].Id, $"Unexpected id ('{expectedApplicationId}' != '{response[0].Id}')");
            Assert.AreEqual(expectedName, response[0].Name, $"Unexpected id ('{expectedName}' != '{response[0].Name}')");
            Assert.AreEqual(expectedDescription, response[0].Description, $"Unexpected id ('{expectedDescription}' != '{response[0].Description}')");
        }

        /// <summary>
        /// Test success of GetLuisAppsAsync().
        /// </summary>
        [TestMethod]
        public void GetLuisAppAsyncSuccess()
        {
            string expectedAuthoringKey = "sdfsdfuywoerjwoekj234234";
            string expectedApplicationId = Guid.NewGuid().ToString();
            string expectedAuthoringRegion = "westus";
            string expectedName = "TestApplication";
            string expectedDescription = "A test LUIS application.";
            string expectedRequestUri = $"https://{expectedAuthoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/{expectedApplicationId}";
            string responseFilePath = @"./data/templates/responses/azure/getLuisApplication.json";

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
                    { "applicationId", expectedApplicationId },
                    { "name", expectedName },
                    { "description", expectedDescription },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            LuisApplication response = client.GetLuisAppAsync(
                new NetworkCredential("luis", expectedAuthoringKey),
                expectedApplicationId,
                expectedAuthoringRegion).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedApplicationId, response.Id, $"Unexpected id ('{expectedApplicationId}' != '{response.Id}')");
            Assert.AreEqual(expectedName, response.Name, $"Unexpected id ('{expectedName}' != '{response.Name}')");
            Assert.AreEqual(expectedDescription, response.Description, $"Unexpected id ('{expectedDescription}' != '{response.Description}')");
        }

        /// <summary>
        /// Test success of GetLuisAppsByNameAsync().
        /// </summary>
        [TestMethod]
        public void GetLuisAppByNameAsyncSuccess()
        {
            string expectedAuthoringKey = "sdfsdfuywoerjwoekj234234";
            string expectedApplicationId = Guid.NewGuid().ToString();
            string expectedAuthoringRegion = "westus";
            string expectedName = "TestApplication";
            string expectedDescription = "A test LUIS application.";
            string expectedRequestUri = $"https://{expectedAuthoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/";
            string responseFilePath = @"./data/templates/responses/azure/getLuisApplications.json";

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
                    { "applicationId", expectedApplicationId },
                    { "name", expectedName },
                    { "description", expectedDescription },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            LuisApplication response = client.GetLuisAppByNameAsync(
                new NetworkCredential("luis", expectedAuthoringKey),
                expectedName,
                expectedAuthoringRegion).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedApplicationId, response.Id, $"Unexpected id ('{expectedApplicationId}' != '{response.Id}')");
            Assert.AreEqual(expectedName, response.Name, $"Unexpected id ('{expectedName}' != '{response.Name}')");
            Assert.AreEqual(expectedDescription, response.Description, $"Unexpected id ('{expectedDescription}' != '{response.Description}')");
        }

        /// <summary>
        /// Test success of ImportLuisAppAsync().
        /// </summary>
        [TestMethod]
        public void ImportLuisAppAsyncSuccess()
        {
            string expectedAuthoringKey = "sdfsdfuywoerjwoekj234234";
            string expectedApplicationId = Guid.NewGuid().ToString();
            string expectedAuthoringRegion = "westus";
            string expectedName = "TestApplication";
            string expectedAppFilePath = @".\data\templates\dummyLuisApp.json";
            string expectedRequestUri = $"https://{expectedAuthoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/import?{expectedName}";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponseSimpleContent(
                HttpStatusCode.OK,
                null,
                $"\"{expectedApplicationId}\"",
                "application/json");
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            string response = client.ImportLuisAppAsync(
                new NetworkCredential("luis", expectedAuthoringKey),
                expectedName,
                expectedAppFilePath,
                expectedAuthoringRegion).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedApplicationId, response, $"Unexpected id ('{expectedApplicationId}' != '{response}')");
        }

        /// <summary>
        /// Test success of DeleteLuisAppAsync().
        /// </summary>
        [TestMethod]
        public void DeleteLuisAppAsyncSuccess()
        {
            string expectedAuthoringKey = "sdfsdfuywoerjwoekj234234";
            string expectedApplicationId = Guid.NewGuid().ToString();
            string expectedAuthoringRegion = "westus";
            string expectedCode = "Success";
            string expectedMessage = "Operation Successful";
            string expectedRequestUri = $"https://{expectedAuthoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/{expectedApplicationId}";
            string responseFilePath = @"./data/templates/responses/azure/deleteLuisApplication.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Delete,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "code", expectedCode },
                    { "message", expectedMessage },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            LuisGeneralResponse response = client.DeleteLuisAppAsync(
                new NetworkCredential("luis", expectedAuthoringKey),
                expectedApplicationId,
                expectedAuthoringRegion).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedCode, response.Code, $"Unexpected code ('{expectedCode}' != '{response.Code}')");
            Assert.AreEqual(expectedMessage, response.Message, $"Unexpected message ('{expectedMessage}' != '{response.Message}')");
        }

        /// <summary>
        /// Test success of AssociateAzureResourceWithLuisAppAsync().
        /// </summary>
        [TestMethod]
        public void AssociateAzureResourceWithLuisAppAsyncSuccess()
        {
            string expectedAuthoringKey = "sdfsdfuywoerjwoekj234234";
            string expectedApplicationId = Guid.NewGuid().ToString();
            string expectedAuthoringRegion = "westus";
            string expectedAzureResourceName = "luis-wus-cs";
            string expectedAzureSubscriptionId = Guid.NewGuid().ToString();
            string expectedAzureResourceGroup = "test-wus-rg";
            string expectedCode = "Success";
            string expectedMessage = "Operation Successful";
            string expectedRequestUri = $"https://{expectedAuthoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/{expectedApplicationId}/azureaccounts";
            string responseFilePath = @"./data/templates/responses/azure/deleteLuisApplication.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "code", expectedCode },
                    { "message", expectedMessage },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            LuisAssociatedAzureResourceRequest luisRequest = new LuisAssociatedAzureResourceRequest()
            {
                AccountName = expectedAzureResourceName,
                AzureSubscriptionId = expectedAzureSubscriptionId,
                ResourceGroup = expectedAzureResourceGroup,
            };

            IAzureClient client = new AzureClient(_tokenProvider);
            LuisGeneralResponse response = client.AssociateAzureResourceWithLuisAppAsync(
                expectedApplicationId,
                expectedAuthoringRegion,
                new NetworkCredential("luis", expectedAuthoringKey),
                luisRequest).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedCode, response.Code, $"Unexpected code ('{expectedCode}' != '{response.Code}')");
            Assert.AreEqual(expectedMessage, response.Message, $"Unexpected message ('{expectedMessage}' != '{response.Message}')");
        }

        /// <summary>
        /// Test success of TrainLuisApp().
        /// </summary>
        [TestMethod]
        public void TrainLuisAppSuccess()
        {
            string expectedAuthoringKey = "sdfsdfuywoerjwoekj234234";
            string expectedApplicationId = Guid.NewGuid().ToString();
            string expectedAppVersion = "0.2";
            string expectedAuthoringRegion = "westus";
            string expectedAzureResourceName = "luis-wus-cs";
            string expectedAzureSubscriptionId = Guid.NewGuid().ToString();
            string expectedAzureResourceGroup = "test-wus-rg";
            string expectedInitialStatusId = "9";
            string expectedInitialStatus = "Queued";
            string expectedModel1Id = Guid.NewGuid().ToString();
            string expectedModel2Id = Guid.NewGuid().ToString();
            string expectedModel3Id = Guid.NewGuid().ToString();
            string expectedRequestUri = $"https://{expectedAuthoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/{expectedApplicationId}/versions/0.2/train";
            string expectedOperationRequestUri = $"https://{expectedAuthoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/{expectedApplicationId}/versions/0.2/train";
            string responseFilePath = @"./data/templates/responses/azure/trainLuisApp-Stage1.json";
            string responseOperationFilePath = @"./data/templates/responses/azure/trainLuisApp-Stage2.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));
            HttpRequestMessage expectedOperationRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Get,
                expectedOperationRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedOperationRequest));
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedOperationRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                new Dictionary<string, string>()
                {
                    { "Location", expectedOperationRequestUri },
                },
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "statusId", expectedInitialStatusId },
                    { "message", expectedInitialStatus },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            HttpResponseMessage expectedOperationResponse = TestHelper.CreateHttpResponse(
               HttpStatusCode.OK,
               null,
               responseOperationFilePath,
               "application/json",
               new Dictionary<string, string>()
               {
                    { "model1Id", expectedModel1Id },
                    { "model1StatusId", "3" },
                    { "model1Status", "InProgress" },
                    { "model1Substatus", "CollectingData" },
                    { "model2Id", expectedModel2Id },
                    { "model2StatusId", "3" },
                    { "model2Status", "InProgress" },
                    { "model2Substatus", "CollectingData" },
                    { "model3Id", expectedModel3Id },
                    { "model3StatusId", "0" },
                    { "model3Status", "Success" },
                    { "model3Substatus", string.Empty },
               });
            _httpClient.RegisterExpectedResponse(
                expectedOperationRequestUri,
                new ExpectedResponse(expectedOperationResponse));
            HttpResponseMessage expectedOperationResponse2 = TestHelper.CreateHttpResponse(
               HttpStatusCode.OK,
               null,
               responseOperationFilePath,
               "application/json",
               new Dictionary<string, string>()
               {
                    { "model1Id", expectedModel1Id },
                    { "model1StatusId", "0" },
                    { "model1Status", "Success" },
                    { "model1Substatus", string.Empty },
                    { "model2Id", expectedModel2Id },
                    { "model2StatusId", "0" },
                    { "model2Status", "Success" },
                    { "model2Substatus", string.Empty },
                    { "model3Id", expectedModel3Id },
                    { "model3StatusId", "0" },
                    { "model3Status", "Success" },
                    { "model3Substatus", string.Empty },
               });
            _httpClient.RegisterExpectedResponse(
                expectedOperationRequestUri,
                new ExpectedResponse(expectedOperationResponse2));

            LuisAssociatedAzureResourceRequest luisRequest = new LuisAssociatedAzureResourceRequest()
            {
                AccountName = expectedAzureResourceName,
                AzureSubscriptionId = expectedAzureSubscriptionId,
                ResourceGroup = expectedAzureResourceGroup,
            };

            IAzureClient client = new AzureClient(_tokenProvider);
            LuisModelTrainingStatus[] response = client.TrainLuisApp(
                expectedApplicationId,
                expectedAppVersion,
                expectedAuthoringRegion,
                new NetworkCredential("luis", expectedAuthoringKey));

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(3, response.Length, $"Unexpected number of models ('3' != '{response.Length}')!");

            Assert.AreEqual(expectedModel1Id, response[0].ModelId, $"Unexpected id for model 1 ('{expectedModel1Id}' != '{response[0].ModelId}')");
            Assert.IsNotNull(response[0].Details, "Response Details member for model 1 should not be null!");
            Assert.AreEqual(0, response[0].Details.StatusId, $"Unexpected status id for model 1 ('0' != '{response[0].Details.StatusId}')");
            Assert.AreEqual(expectedModel2Id, response[1].ModelId, $"Unexpected id for model 2 ('{expectedModel1Id}' != '{response[1].ModelId}')");
            Assert.IsNotNull(response[1].Details, "Response Details member for model 2 should not be null!");
            Assert.AreEqual(0, response[1].Details.StatusId, $"Unexpected status id for model 2 ('0' != '{response[1].Details.StatusId}')");
            Assert.AreEqual(expectedModel3Id, response[2].ModelId, $"Unexpected id for model 3 ('{expectedModel1Id}' != '{response[2].ModelId}')");
            Assert.IsNotNull(response[2].Details, "Response Details member for model 3 should not be null!");
            Assert.AreEqual(0, response[2].Details.StatusId, $"Unexpected status id for model 3 ('0' != '{response[2].Details.StatusId}')");
        }

        /// <summary>
        /// Test success of PublishLuisAppAsync().
        /// </summary>
        [TestMethod]
        public void PublishLuisAppAsyncSuccess()
        {
            string expectedAuthoringKey = "sdfsdfuywoerjwoekj234234";
            string expectedApplicationId = Guid.NewGuid().ToString();
            string expectedApplicationVersion = "0.2";
            string expectedAuthoringRegion = "westus";
            string expectedEndpointRegion = "westus";
            string expectedEndpointUrl = $"https://{expectedEndpointRegion}.api.cognitive.microsoft.com/luis/v2.0/apps/{expectedApplicationId}";
            string expectedRequestUri = $"https://{expectedAuthoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/{expectedApplicationId}/publish";
            string responseFilePath = @"./data/templates/responses/azure/publishLuisApplication.json";

            HttpRequestMessage expectedRequest = TestHelper.CreateHttpRequest(
                HttpMethod.Post,
                expectedRequestUri);
            _httpClient.RegisterExpectedRequest(new ExpectedRequest(expectedRequest));

            HttpResponseMessage expectedResponse = TestHelper.CreateHttpResponse(
                HttpStatusCode.OK,
                null,
                responseFilePath,
                "application/json",
                new Dictionary<string, string>()
                {
                    { "version", expectedApplicationVersion },
                    { "endpointUrl", expectedEndpointUrl },
                    { "authoringRegion", expectedAuthoringRegion },
                    { "endpointRegion", expectedEndpointRegion },
                });
            _httpClient.RegisterExpectedResponse(
                expectedRequestUri,
                new ExpectedResponse(expectedResponse));

            IAzureClient client = new AzureClient(_tokenProvider);
            LuisPublishResponse response = client.PublishLuisAppAsync(
                expectedApplicationId,
                expectedApplicationVersion,
                expectedAuthoringRegion,
                new NetworkCredential("luis", expectedAuthoringKey)).Result;

            Assert.IsNotNull(response, "Response object should not be null!");
            Assert.AreEqual(expectedApplicationVersion, response.VersionId, $"Unexpected version id ('{expectedApplicationVersion}' != '{response.VersionId}')");
            Assert.AreEqual(expectedEndpointRegion, response.EndpointRegion, $"Unexpected endpoint region ('{expectedEndpointRegion}' != '{response.EndpointRegion}')");
            Assert.AreEqual(expectedEndpointUrl, response.EndpointUrl, $"Unexpected version id ('{expectedEndpointUrl}' != '{response.EndpointUrl}')");
            Assert.AreEqual(expectedAuthoringRegion, response.Region, $"Unexpected version id ('{expectedAuthoringRegion}' != '{response.Region}')");
        }
    }
}
