// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Clients
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Resolvers;
    using System.Xml.Serialization;
    using AdvocacyPlatformInstaller.Contracts;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Simple REST client for interacting with Azure APIs.
    /// </summary>
    public class AzureClient : TokenBasedClient, IAzureClient
    {
        /// <summary>
        /// Audience for the Azure Management API.
        /// </summary>
        public const string ManagementAudience = "https://management.azure.com/";

        /// <summary>
        /// Audience for the Microsoft Graph API.
        /// </summary>
        public const string AzureADAudience = "https://graph.microsoft.com/";

        /// <summary>
        /// Audience for the Azure Key Vault API.
        /// </summary>
        public const string KeyVaultAudience = "https://vault.azure.net";

        /// <summary>
        /// Audience for the Azure Storage API.
        /// </summary>
        public const string StorageAudience = "https://storage.azure.com/";

        private const string _luisAppsWithIdUri = "https://{authoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/{appId}";
        private const string _luisAppsUri = "https://{authoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/";
        private const string _luisImportUri = "https://{authoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/import?{appName}";
        private const string _luisAssociatedAzureResourceUri = "https://{authoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/{appId}/azureaccounts";
        private const string _luisTrainUri = "https://{authoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/{appId}/versions/{appVersion}/train";
        private const string _luisPublishUri = "https://{authoringRegion}.api.cognitive.microsoft.com/luis/api/v2.0/apps/{appId}/publish";

        private static readonly Regex _connectionAuthRegEx = new Regex("error=[^&]*|code=[^&]*", RegexOptions.Compiled);
        private static readonly Regex _codeRegEx = new Regex("(code=)(.*)$", RegexOptions.Compiled);

        private static readonly string _tenantsListUri = $"{ManagementAudience}tenants?api-version=2016-06-01";
        private static readonly string _subscriptionsListUri = $"{ManagementAudience}subscriptions?api-version=2016-06-01";
        private static readonly string _resourceLockWithScopeUri = $"{ManagementAudience}{{scope}}?api-version=2016-09-01";
        private static readonly string _resourceGroupsListUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourcegroups?api-version=2018-05-01";
        private static readonly string _resourceGroupWithNameUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourcegroups/{{resourceGroupName}}?api-version=2018-05-01";
        private static readonly string _resourceWithNameUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourcegroups/{{resourceGroupName}}/providers/{{resourceProviderNamespace}}/{{parentResourcePath}}{{resourceType}}/{{resourceName}}?api-version={{apiVersion}}";
        private static readonly string _resourceGroupDeploymentValidateUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourcegroups/{{resourceGroupName}}/providers/Microsoft.Resources/deployments/{{deploymentName}}/validate?api-version=2018-05-01";
        private static readonly string _resourceGroupDeploymentUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourcegroups/{{resourceGroupName}}/providers/Microsoft.Resources/deployments/{{deploymentName}}?api-version=2018-05-01";
        private static readonly string _resourceGroupLocksUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourceGroups/{{resourceGroupName}}/providers/Microsoft.Authorization/locks?api-version=2016-09-01";
        private static readonly string _resourceGroupLockWithNameUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourceGroups/{{resourceGroupName}}/providers/Microsoft.Authorization/locks/{{lockName}}?api-version=2016-09-01";
        private static readonly string _appServicePublishingProfileUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourceGroups/{{resourceGroupName}}/providers/Microsoft.Web/sites/{{name}}/publishxml?api-version=2016-08-01";
        private static readonly string _appServiceGetAuthSettingsUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourceGroups/{{resourceGroupName}}/providers/Microsoft.Web/sites/{{name}}/config/authsettings/list?api-version=2016-08-01";
        private static readonly string _appServiceUpdateAuthSettingsUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourceGroups/{{resourceGroupName}}/providers/Microsoft.Web/sites/{{name}}/config/authsettings?api-version=2016-08-01";
        private static readonly string _appServiceGetAppSettingsUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourceGroups/{{resourceGroupName}}/providers/Microsoft.Web/sites/{{name}}/config/appsettings/list?api-version=2016-08-01";
        private static readonly string _appServiceUpdateAppSettingsUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourceGroups/{{resourceGroupName}}/providers/Microsoft.Web/sites/{{name}}/config/appsettings?api-version=2016-08-01";
        private static readonly string _invokeResourceActionUri = $"{ManagementAudience}{{resourceId}}/{{action}}?api-version={{apiVersion}}";

        private static readonly string _applicationUri = $"{AzureADAudience}beta/applications";
        private static readonly string _applicationWithIdUri = $"{AzureADAudience}beta/applications/{{id}}";
        private static readonly string _servicePrincipalWithIdUri = $"{AzureADAudience}beta/servicePrincipals/{{id}}";

        private static readonly string _akvAccessPolicyUri = $"{ManagementAudience}subscriptions/{{subscriptionId}}/resourceGroups/{{resourceGroupName}}/providers/Microsoft.KeyVault/vaults/{{vaultName}}/accessPolicies/{{operationKind}}?api-version=2018-02-14";
        private static readonly string _akvSecretWithNameUri = "https://{vaultName}.vault.azure.net/secrets/{secretName}?api-version=7.0";

        private static readonly string _blobStoredAccessPolicyUri = "https://{storageAccountName}.blob.core.windows.net/{containerName}?restype=container&comp=acl";

        private static readonly string _zipDeployUrl = "https://{appServiceName}.scm.azurewebsites.net/api/zipdeploy";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureClient"/> class.
        /// </summary>
        /// <param name="tokenProvider">Token provider instance.</param>
        public AzureClient(ITokenProvider tokenProvider)
            : base(tokenProvider)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new JsonCamelCaseContractResolver(),
            };
        }

        /// <summary>
        /// Gets a list of available tenants.
        /// </summary>
        /// <returns>List of tenants.</returns>
        public async Task<Tenant[]> GetTenantsAsync()
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation("Acquiring tenants...");
            HttpResponseMessage response = await httpClient.GetAsync(_tenantsListUri);

            TenantListResult result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<TenantListResult>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result != null ? result.Value : null;
        }

        /// <summary>
        /// Gets a list of available subscriptions.
        /// </summary>
        /// <returns>A list of available subscriptions.</returns>
        public async Task<Subscription[]> GetSubscriptionsAsync()
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation("Acquiring subscriptions...");
            HttpResponseMessage response = await httpClient.GetAsync(_subscriptionsListUri);

            SubscriptionListResult result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<SubscriptionListResult>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result != null ? result.Value : null;
        }

        /// <summary>
        /// Checks if a resource group exists.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="name">The name of the resource group.</param>
        /// <returns>True if the resource group exists, false if not.</returns>
        public async Task<bool> ResourceGroupExistsAsync(string subscriptionId, string name)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Checking if resource group '{name}' in subscription '{subscriptionId}' exists...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _resourceGroupWithNameUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", name));

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }

                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }
        }

        /// <summary>
        /// Gets available resource groups.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to look for resource groups in.</param>
        /// <returns>A list of available resource groups.</returns>
        public async Task<ResourceGroup[]> GetResourceGroupsAsync(string subscriptionId)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Acquiring resource groups in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _resourceGroupsListUri.Replace("{subscriptionId}", subscriptionId));

            ResourceGroupListResult result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResourceGroupListResult>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result != null ? result.Value : null;
        }

        /// <summary>
        /// Gets an Azure resource.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription id the resource resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the resource resides in.</param>
        /// <param name="resourceProviderNamespace">The name of the Azure Resource Provider.</param>
        /// <param name="parentResourcePath">Path to the parent resource.</param>
        /// <param name="resourceType">The resource type.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <param name="apiVersion">The API version to use.</param>
        /// <returns>The response content as a string.</returns>
        public async Task<string> GetResourceAsync(string subscriptionId, string resourceGroupName, string resourceProviderNamespace, string parentResourcePath, string resourceType, string resourceName, string apiVersion = "2016-06-01")
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Acquiring resource '{resourceProviderNamespace}/{parentResourcePath}/{resourceType}/{resourceName}' in resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _resourceWithNameUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{resourceProviderNamespace}", resourceProviderNamespace)
                    .Replace("{parentResourcePath}", parentResourcePath != null ? $"{parentResourcePath}/" : string.Empty)
                    .Replace("{resourceType}", resourceType)
                    .Replace("{resourceName}", resourceName)
                    .Replace("{apiVersion}", apiVersion));

            string result = null;

            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets an Azure Resource.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response as.</typeparam>
        /// <param name="subscriptionId">The id of the subscription id the resource resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the resource resides in.</param>
        /// <param name="resourceProviderNamespace">The name of the Azure Resource Provider.</param>
        /// <param name="parentResourcePath">Path to the parent resource.</param>
        /// <param name="resourceType">The resource type.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <param name="apiVersion">The API version to use.</param>
        /// <returns>The response deserialized as the type specified.</returns>
        public async Task<T> GetResourceAsync<T>(string subscriptionId, string resourceGroupName, string resourceProviderNamespace, string parentResourcePath, string resourceType, string resourceName, string apiVersion = "2016-06-01")
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Acquiring resource '{resourceProviderNamespace}/{parentResourcePath}/{resourceType}/{resourceName}' in resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _resourceWithNameUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{resourceProviderNamespace}", resourceProviderNamespace)
                    .Replace("{parentResourcePath}", parentResourcePath != null ? $"{parentResourcePath}/" : string.Empty)
                    .Replace("{resourceType}", resourceType)
                    .Replace("{resourceName}", resourceName)
                    .Replace("{apiVersion}", apiVersion));

            T result = default(T);

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets an Azure Resource.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription id the resource resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the resource resides in.</param>
        /// <param name="resourceProviderNamespace">The name of the Azure Resource Provider.</param>
        /// <param name="parentResourcePath">Path to the parent resource.</param>
        /// <param name="resourceType">The resource type.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <param name="apiVersion">The API version to use.</param>
        /// <returns>The response deserialized as an AzureResourceBase object.</returns>
        public async Task<AzureResourceBase> GetResourceBaseAsync(string subscriptionId, string resourceGroupName, string resourceProviderNamespace, string parentResourcePath, string resourceType, string resourceName, string apiVersion = "2016-06-01")
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Acquiring resource '{resourceProviderNamespace}/{parentResourcePath}/{resourceType}/{resourceName}' in resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _resourceWithNameUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{resourceProviderNamespace}", resourceProviderNamespace)
                    .Replace("{parentResourcePath}", parentResourcePath != null ? $"{parentResourcePath}/" : string.Empty)
                    .Replace("{resourceType}", resourceType)
                    .Replace("{resourceName}", resourceName)
                    .Replace("{apiVersion}", apiVersion));

            AzureResourceBase result = null;

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<AzureResourceBase>(content);
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets an Azure Resource with an identity.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription id the resource resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the resource resides in.</param>
        /// <param name="resourceProviderNamespace">The name of the Azure Resource Provider.</param>
        /// <param name="parentResourcePath">Path to the parent resource.</param>
        /// <param name="resourceType">The resource type.</param>
        /// <param name="resourceName">The name of the resource.</param>
        /// <param name="apiVersion">The API version to use.</param>
        /// <returns>The response deserialized as an AzureResourceBase object.</returns>
        public async Task<AzureIdentityResourceBase> GetResourceIdentityBaseAsync(string subscriptionId, string resourceGroupName, string resourceProviderNamespace, string parentResourcePath, string resourceType, string resourceName, string apiVersion = "2016-06-01")
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Acquiring resource '{resourceProviderNamespace}/{parentResourcePath}/{resourceType}/{resourceName}' in resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _resourceWithNameUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{resourceProviderNamespace}", resourceProviderNamespace)
                    .Replace("{parentResourcePath}", parentResourcePath != null ? $"{parentResourcePath}/" : string.Empty)
                    .Replace("{resourceType}", resourceType)
                    .Replace("{resourceName}", resourceName)
                    .Replace("{apiVersion}", apiVersion));

            AzureIdentityResourceBase result = null;

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<AzureIdentityResourceBase>(content);
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets locks on a resource group.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group.</param>
        /// <returns>A list of resource locks.</returns>
        public async Task<AzureValueCollectionResponse<ResourceLock>> GetResourceGroupLocksAsync(string subscriptionId, string resourceGroupName)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Acquiring resource group locks for resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _resourceGroupLocksUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName));

            AzureValueCollectionResponse<ResourceLock> result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<AzureValueCollectionResponse<ResourceLock>>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Deletes a resource group lock.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the lock resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the lock resides in.</param>
        /// <param name="lockId">The id of the lock.</param>
        /// <returns>The response content as a string.</returns>
        public async Task<string> DeleteResourceGroupLockAsync(string subscriptionId, string resourceGroupName, string lockId)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Deleting resource group lock with id '{lockId}' on resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.DeleteAsync(
                _resourceGroupLockWithNameUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{lockName}", lockId));

            string result = null;

            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Deletes a resource lock.
        /// </summary>
        /// <param name="scope">The scope of the lock.</param>
        /// <returns>The response content as a string.</returns>
        public async Task<string> DeleteResourceLockAsync(string scope)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Deleting resource lock with scope '{scope}'...");
            HttpResponseMessage response = await httpClient.DeleteAsync(
                _resourceLockWithScopeUri
                    .Replace("{scope}", scope));

            string result = null;

            if (response.IsSuccessStatusCode)
            {
                if (response.Content != null)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Validates a resource group deployment.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to validate in.</param>
        /// <param name="resourceGroupName">The name of the resource group to validate in.</param>
        /// <param name="deploymentName">The name to give to the deployment validation.</param>
        /// <param name="templateFilePath">Path to the ARM template file to validate.</param>
        /// <param name="templateParametersFilePath">Path to the ARM template parameters file to validate.</param>
        /// <returns>True if valid, false if invalid.</returns>
        public async Task<bool> ValidateResourceGroupDeploymentAsync(string subscriptionId, string resourceGroupName, string deploymentName, string templateFilePath, string templateParametersFilePath)
        {
            ResourceGroupDeploymentRequest validateResourceGroupDeployment = new ResourceGroupDeploymentRequest();

            JObject rgDeploy = JsonConvert.DeserializeObject<JObject>(
                JsonConvert.SerializeObject(validateResourceGroupDeployment));

            LogInformation($"Loading template parameters from '{templateParametersFilePath}'...");
            JObject templateParameters = JObject.Parse(
                File.ReadAllText(templateParametersFilePath));
            LogInformation($"Loading template from '{templateFilePath}'...");
            JObject template = JObject.Parse(
                File.ReadAllText(templateFilePath));

            rgDeploy["properties"]["parameters"] = templateParameters["parameters"];
            rgDeploy["properties"]["template"] = template;

            string bodyContent = JsonConvert.SerializeObject(rgDeploy);
            HttpContent body = new StringContent(
                bodyContent,
                Encoding.UTF8,
                "application/json");

            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation("Validating template...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _resourceGroupDeploymentValidateUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{deploymentName}", deploymentName),
                body);

            string result = null;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = await response.Content.ReadAsStringAsync();

                return true;
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                return false;
            }
        }

        /// <summary>
        /// Creates a resource group deployment.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to deploy to.</param>
        /// <param name="resourceGroupName">The name of the resource group to deploy to.</param>
        /// <param name="deploymentName">The name to give to the deployment.</param>
        /// <param name="templateFilePath">Path to the ARM template file.</param>
        /// <param name="templateParametersFilePath">Path to the ARM template parameters file.</param>
        /// <returns>The initial resource group deployment status.</returns>
        public ResourceGroupDeploymentStatus CreateResourceGroupDeployment(string subscriptionId, string resourceGroupName, string deploymentName, string templateFilePath, string templateParametersFilePath)
        {
            AzureResponseBase response = CreateResourceGroupDeploymentAsync(subscriptionId, resourceGroupName, deploymentName, templateFilePath, templateParametersFilePath).Result;

            if (response == null)
            {
                return null;
            }

            DateTime startTime = DateTime.Now;
            DateTime currentTime = DateTime.Now;
            TimeSpan timeDiff = currentTime - startTime;

            int timeoutInSeconds = 600;

            HttpResponseMessage httpResponse = null;

            ResourceGroupDeploymentStatus status = new ResourceGroupDeploymentStatus()
            {
                Status = "running",
            };

            while (httpResponse == null ||
                (string.Compare(status.Status, "running", true) == 0 &&
                 timeDiff.TotalSeconds < timeoutInSeconds &&
                 !response.AlreadyExists))
            {
                LogInformation("Sleeping until next poll...");
                Thread.Sleep(5000);

                LogInformation("Polling for Azure Resource Group deployment completion...");
                httpResponse = GetLocationAsync(ManagementAudience, new Uri(response.AzureAsyncOperationUri ?? response.LocationUri)).Result;

                status = JsonConvert.DeserializeObject<ResourceGroupDeploymentStatus>(httpResponse.Content.ReadAsStringAsync().Result);
                currentTime = DateTime.Now;
                timeDiff = currentTime - startTime;
            }

            if (httpResponse.IsSuccessStatusCode)
            {
                if (string.Compare("Succeeded", status.Status, true) != 0)
                {
                    LogError($"ERROR: ({httpResponse.StatusCode}) {httpResponse.ReasonPhrase}");
                    return status;
                }

                return status;
            }
            else
            {
                LogError($"ERROR: ({httpResponse.StatusCode}) {httpResponse.ReasonPhrase}");
                return status;
            }
        }

        /// <summary>
        /// Gets the response from a URL provided via a previous response's Location header.
        /// </summary>
        /// <param name="audience">The audience to acquire an access token for.</param>
        /// <param name="location">The URL to perform the request against.</param>
        /// <returns>The response message.</returns>
        public async Task<HttpResponseMessage> GetLocationAsync(string audience, Uri location)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(audience);

            LogInformation("Querying URI returned in 'Location' header...");
            return await httpClient.GetAsync(location);
        }

        /// <summary>
        /// Creates a resource group deployment.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to deploy to.</param>
        /// <param name="resourceGroupName">The name of the resource group to deploy to.</param>
        /// <param name="deploymentName">The name to give to the deployment.</param>
        /// <param name="templateFilePath">Path to the ARM template file.</param>
        /// <param name="templateParametersFilePath">Path to the ARM template parameters file.</param>
        /// <returns>The response content as an AzureResponseBase object.</returns>
        public async Task<AzureResponseBase> CreateResourceGroupDeploymentAsync(string subscriptionId, string resourceGroupName, string deploymentName, string templateFilePath, string templateParametersFilePath)
        {
            if (string.IsNullOrWhiteSpace(deploymentName))
            {
                deploymentName = $"azureDeploy_{Guid.NewGuid().ToString()}";
            }

            ResourceGroupDeploymentRequest resourceGroupDeployment = new ResourceGroupDeploymentRequest();

            JObject rgDeploy = JsonConvert.DeserializeObject<JObject>(
                JsonConvert.SerializeObject(resourceGroupDeployment));

            LogInformation($"Loading template parameters from {templateParametersFilePath}...");
            JObject templateParameters = JObject.Parse(
                File.ReadAllText(templateParametersFilePath));

            LogInformation($"Loading template from {templateFilePath}...");
            JObject template = JObject.Parse(
                File.ReadAllText(templateFilePath));

            rgDeploy["properties"]["parameters"] = templateParameters["parameters"];
            rgDeploy["properties"]["template"] = template;

            string bodyContent = JsonConvert.SerializeObject(rgDeploy);
            HttpContent body = new StringContent(
                bodyContent,
                Encoding.UTF8,
                "application/json");

            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation("Starting resource group deployment...");
            HttpResponseMessage response = await httpClient.PutAsync(
                _resourceGroupDeploymentUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{deploymentName}", deploymentName),
                body);

            AzureResponseBase result = new AzureResponseBase();

            if (response.IsSuccessStatusCode)
            {
                if (response.Headers.Contains("Azure-AsyncOperation"))
                {
                    result.AzureAsyncOperationUri = response.Headers.GetValues("Azure-AsyncOperation").First();
                }
                else if (response.Headers.Contains("Location"))
                {
                    result.LocationUri = response.Headers.GetValues("Location").First();
                }

                if (response.Headers.Contains("Retry-After"))
                {
                    result.RetryAfter = int.Parse(response.Headers.GetValues("Retry-After").First());
                }

                await response.Content.ReadAsStringAsync();
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets the status of an asynchronous Azure operation.
        /// </summary>
        /// <param name="url">The URL to make the request against.</param>
        /// <returns>The response content as a string.</returns>
        public async Task<string> GetAsyncOperationStatusAsync(string url)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation("Acquiring operation status...");
            HttpResponseMessage response = await httpClient.GetAsync(url);

            string result = null;

            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets a resource group.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="name">The name of the resource group.</param>
        /// <returns>Metadata about the resource group.</returns>
        public async Task<ResourceGroup> GetResourceGroupAsync(string subscriptionId, string name)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Acquiring resource group '{name}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _resourceGroupWithNameUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", name));

            ResourceGroup result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResourceGroup>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Creates a new or updates an existing resource group.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="resourceGroup">The name of the resource group.</param>
        /// <returns>Metadata about the new or updates resource group.</returns>
        public async Task<ResourceGroup> CreateOrUpdateResourceGroupAsync(string subscriptionId, ResourceGroup resourceGroup)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            HttpContent body = new StringContent(
                JsonConvert.SerializeObject(resourceGroup),
                Encoding.UTF8,
                "application/json");

            LogInformation($"Creating resource group '{resourceGroup.Name}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.PutAsync(
                _resourceGroupWithNameUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroup.Name),
                body);

            ResourceGroup result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<ResourceGroup>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Deletes a resource group.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="name">The name of the resource group.</param>
        /// <returns>True if successful, false if failed.</returns>
        public bool DeleteResourceGroup(string subscriptionId, string name)
        {
            AzureResponseBase response = DeleteResourceGroupAsync(subscriptionId, name).Result;

            if (response == null)
            {
                return false;
            }

            DateTime startTime = DateTime.Now;
            DateTime currentTime = DateTime.Now;
            TimeSpan timeDiff = currentTime - startTime;

            int timeoutInSeconds = 600;

            HttpResponseMessage httpResponse = null;

            while (httpResponse == null ||
                (httpResponse.StatusCode != HttpStatusCode.OK &&
                 httpResponse.StatusCode != HttpStatusCode.NotFound &&
                 httpResponse.StatusCode != HttpStatusCode.InternalServerError &&
                 timeDiff.TotalSeconds < timeoutInSeconds &&
                 !response.AlreadyExists))
            {
                LogInformation("Sleeping until next poll...");
                Thread.Sleep(5000);

                LogInformation("Polling for Azure Resource Group removal completion...");
                httpResponse = GetLocationAsync(ManagementAudience, new Uri(response.AzureAsyncOperationUri ?? response.LocationUri)).Result;

                currentTime = DateTime.Now;
                timeDiff = currentTime - startTime;
            }

            if (httpResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                LogError($"ERROR: ({httpResponse.StatusCode}) {httpResponse.ReasonPhrase}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a resource group.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="name">The name of the resource group.</param>
        /// <returns>The response content as an AzureResponseBase object.</returns>
        public async Task<AzureResponseBase> DeleteResourceGroupAsync(string subscriptionId, string name)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Staring deletion of resource group '{name}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.DeleteAsync(
                _resourceGroupWithNameUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", name));

            AzureResponseBase result = new AzureResponseBase();

            if (response.IsSuccessStatusCode)
            {
                if (response.Headers.Contains("Azure-AsyncOperation"))
                {
                    result.AzureAsyncOperationUri = response.Headers.GetValues("Azure-AsyncOperation").First();
                }
                else if (response.Headers.Contains("Location"))
                {
                    result.LocationUri = response.Headers.GetValues("Location").First();
                }

                if (response.Headers.Contains("Retry-After"))
                {
                    result.RetryAfter = int.Parse(response.Headers.GetValues("Retry-After").First());
                }
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets application registrations.
        /// Note: Requires tenant admin consent.
        /// </summary>
        /// <returns>A list of application registrations.</returns>
        public async Task<AzureValueCollectionResponse<AzureApplication>> GetApplicationsAsync()
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(AzureADAudience);

            LogInformation("Acquiring registered applications...");
            HttpResponseMessage response = await httpClient.GetAsync(_applicationUri);

            AzureValueCollectionResponse<AzureApplication> result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<AzureValueCollectionResponse<AzureApplication>>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets an application registration.
        /// Note: Requires admin consent.
        /// </summary>
        /// <param name="applicationName">The display name of the application registration.</param>
        /// <returns>The application registration.</returns>
        public async Task<AzureApplication> GetApplicationAsync(string applicationName)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(AzureADAudience);

            LogInformation($"Acquiring registered application named '{applicationName}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                $"{_applicationUri}?$filter=displayName eq '{applicationName}'");

            if (response.IsSuccessStatusCode)
            {
                AzureValueCollectionResponse<AzureApplication> result = JsonConvert.DeserializeObject<AzureValueCollectionResponse<AzureApplication>>(await response.Content.ReadAsStringAsync());

                return result.Value.FirstOrDefault();
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }
        }

        /// <summary>
        /// Creates an application registration.
        /// Note: Requires admin consent.
        /// </summary>
        /// <param name="application">Configuration information for the application registration.</param>
        /// <returns>The application registration.</returns>
        public async Task<AzureApplication> CreateApplicationAsync(AzureApplicationRequestBase application)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(AzureADAudience);

            HttpContent body = new StringContent(
                JsonConvert.SerializeObject(
                    application,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    }),
                Encoding.UTF8,
                "application/json");

            LogInformation($"Registering application named '{application.DisplayName}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _applicationUri,
                body);

            AzureApplication result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<AzureApplication>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets a service principal.
        /// </summary>
        /// <param name="servicePrincipalId">The id of the service principal.</param>
        /// <returns>The service principal.</returns>
        public async Task<ServicePrincipal> GetServicePrincipal(string servicePrincipalId)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(AzureADAudience);

            LogInformation($"Getting service principal with id '{servicePrincipalId}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _servicePrincipalWithIdUri
                    .Replace("{id}", servicePrincipalId));

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<ServicePrincipal>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }
        }

        /// <summary>
        /// Deletes an application registration.
        /// Note: requires admin consent.
        /// </summary>
        /// <param name="applicationId">The id of the application registration.</param>
        /// <returns>True if successful, false if failed.</returns>
        public async Task<bool> DeleteApplicationAsync(string applicationId)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(AzureADAudience);

            LogInformation($"Deleting application with id '{applicationId}'...");
            HttpResponseMessage response = await httpClient.DeleteAsync(
                _applicationWithIdUri
                    .Replace("{id}", applicationId));

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }
        }

        /// <summary>
        /// Creates an Azure Key Vault Access Policy.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the key vault resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the key vault resides in.</param>
        /// <param name="keyVaultName">The name of the key vault resource.</param>
        /// <param name="accessPolicy">The name to assign to the access policy.</param>
        /// <returns>The response content.</returns>
        public async Task<CreateKeyVaultAccessPolicyResponse> CreateKeyVaultAccessPolicyAsync(string subscriptionId, string resourceGroupName, string keyVaultName, CreateKeyVaultAccessPolicyRequest accessPolicy)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            HttpContent body = new StringContent(
                JsonConvert.SerializeObject(accessPolicy),
                Encoding.UTF8,
                "application/json");

            LogInformation($"Creating key vault access policy on '{keyVaultName}' in resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.PutAsync(
                _akvAccessPolicyUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{vaultName}", keyVaultName)
                    .Replace("{operationKind}", "add"),
                body);

            CreateKeyVaultAccessPolicyResponse result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<CreateKeyVaultAccessPolicyResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Creates or updates an Azure Key Vault secret.
        /// </summary>
        /// <param name="keyVaultName">The name of the key vault.</param>
        /// <param name="secretName">The name of the secret.</param>
        /// <param name="secretValue">The value to set the secret to.</param>
        /// <returns>Information about the secret.</returns>
        public async Task<AzureKeyVaultSecret> UpdateKeyVaultSecretAsync(string keyVaultName, string secretName, NetworkCredential secretValue)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(KeyVaultAudience);

            string secret = $@"{{
    ""value"": ""{secretValue.Password}""
}}";

            HttpContent body = new StringContent(
                secret,
                Encoding.UTF8,
                "application/json");

            LogInformation($"Updating key vault secret named '{secretName}' in key vault '{keyVaultName}'...");
            HttpResponseMessage response = await httpClient.PutAsync(
                _akvSecretWithNameUri
                    .Replace("{vaultName}", keyVaultName)
                    .Replace("{secretName}", secretName),
                body);

            AzureKeyVaultSecret result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<AzureKeyVaultSecret>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Creates an Azure Blob Storage container Stored Access Policy.
        /// </summary>
        /// <param name="storageAccountName">The name of the storage account.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="storedAccessPolicy">The name to assign to the stored access policy.</param>
        /// <returns>The response content as a string.</returns>
        public async Task<string> CreateBlobStoredAccessPolicyAsync(string storageAccountName, string containerName, SignedIdentifiers storedAccessPolicy)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(StorageAudience);

            httpClient.SetHeader("x-ms-version", "2018-03-28");

            XmlSerializer xmlSer = new XmlSerializer(typeof(SignedIdentifiers));
            TextWriter xmlBodyWriter = new StringWriter();

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);

            xmlSer.Serialize(xmlBodyWriter, storedAccessPolicy, ns);
            string xmlBodyText = xmlBodyWriter.ToString();

            HttpContent body = new StringContent(
                xmlBodyText,
                Encoding.Unicode,
                "application/xml");

            LogInformation($"Creating stored access policy on container '{containerName}' in storage account '{storageAccountName}'...");
            HttpResponseMessage response = await httpClient.PutAsync(
                _blobStoredAccessPolicyUri
                    .Replace("{storageAccountName}", storageAccountName)
                    .Replace("{containerName}", containerName),
                body);

            string result = null;

            if (response.IsSuccessStatusCode)
            {
                if (response.Content != null)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Creates an Azure Storage Blob container shared access signature from a stored access policy.
        /// </summary>
        /// <param name="storageAccountName">The name of the storage account.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="accessKey">A storage access key for authentication.</param>
        /// <param name="sharedAccessPolicyId">The id of the stored access policy to create the shared access signature from.</param>
        /// <param name="policy">Configuration information describing the desired shared access signature.</param>
        /// <returns>The response content as a string.</returns>
        public string CreateSharedAccessSignature(string storageAccountName, string containerName, NetworkCredential accessKey, string sharedAccessPolicyId, SharedAccessBlobPolicy policy)
        {
            StorageCredentials credential = new StorageCredentials(storageAccountName, accessKey.Password);
            CloudStorageAccount storageAccount = new CloudStorageAccount(
                credential,
                storageAccountName,
                endpointSuffix: null,
                useHttps: true);

            if (storageAccount == null)
            {
                return null;
            }

            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            if (blobClient == null)
            {
                return null;
            }

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            if (container == null)
            {
                return null;
            }

            return container.GetSharedAccessSignature(policy, sharedAccessPolicyId);
        }

        /// <summary>
        /// Gets an Azure App Service's publishing profiles.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the app service resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the app service resides in.</param>
        /// <param name="appServiceName">The name of the app service.</param>
        /// <returns>The publishing profiles.</returns>
        public async Task<PublishData> GetAppServicePublishingProfileAsync(string subscriptionId, string resourceGroupName, string appServiceName)
        {
            string publishingProfile = $@"{{
                ""format"": ""WebDeploy""
}}";

            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            HttpContent body = new StringContent(
                publishingProfile,
                Encoding.UTF8,
                "application/xml");

            LogInformation($"Acquiring publishing profile for app service '{appServiceName}' in resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _appServicePublishingProfileUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{name}", appServiceName),
                body);

            if (response.IsSuccessStatusCode)
            {
                using (StreamReader reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
                {
                    XmlTextReader xmlTextReader = new XmlTextReader(reader);

                    xmlTextReader.DtdProcessing = DtdProcessing.Ignore;

                    XmlSerializer xmlSer = new XmlSerializer(typeof(PublishData));

                    return (PublishData)xmlSer.Deserialize(xmlTextReader);
                }
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }
        }

        /// <summary>
        /// Deploys a ZIP deployment of an Azure Function.
        /// </summary>
        /// <param name="appServiceName">The name of the app service to deploy to.</param>
        /// <param name="zipUri">The URI of the binaries ZIP archive.</param>
        /// <param name="publishProfile">Credentials for the publishing profile to use.</param>
        /// <returns>The response content as a string.</returns>
        public async Task<string> ZipDeployAppServiceAsync(string appServiceName, string zipUri, NetworkCredential publishProfile)
        {
            string zipDeploy = $@"{{
    ""packageUri"": ""{zipUri}""
}}";
            string base64AuthInfo = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{publishProfile.UserName}:{publishProfile.Password}"));
            IHttpClient httpClient = TokenProvider.GetGenericHttpClient();

            httpClient.SetHeader("Authorization", $"Basic {base64AuthInfo}");

            HttpContent body = new StringContent(
                zipDeploy,
                Encoding.UTF8,
                "application/json");

            LogInformation($"Deploying app service package from '{zipUri}' to app service '{appServiceName}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _zipDeployUrl
                    .Replace("{appServiceName}", appServiceName),
                body);

            string result = null;

            if (response.IsSuccessStatusCode)
            {
                if (response.Content != null)
                {
                    result = await response.Content.ReadAsStringAsync();
                }
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets an Azure App Service's authentication settings.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the app service resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the app service resides in.</param>
        /// <param name="appServiceName">The name of the app service.</param>
        /// <returns>The app service's authentication settings.</returns>
        public async Task<AppServiceAuthSettings> GetAppServiceAuthSettingsAsync(string subscriptionId, string resourceGroupName, string appServiceName)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Acquiring authentication settings for app service '{appServiceName}' in resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _appServiceGetAuthSettingsUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{name}", appServiceName),
                null);

            AppServiceAuthSettings result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<AppServiceAuthSettings>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerSettings()
                    {
                        ContractResolver = new DefaultContractResolver(),
                    });
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Updates an Azure App Service's authentication settings.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the app service resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the app service resides in.</param>
        /// <param name="appServiceName">The name of the app service.</param>
        /// <param name="settings">The updated authentication settings.</param>
        /// <returns>Returns the updated authentication settings.</returns>
        public async Task<AppServiceAuthSettings> UpdateAppServiceAuthSettingsAsync(string subscriptionId, string resourceGroupName, string appServiceName, AppServiceAuthSettings settings)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            HttpContent body = new StringContent(
                JsonConvert.SerializeObject(
                    settings,
                    new JsonSerializerSettings()
                    {
                        ContractResolver = new DefaultContractResolver(),
                    }),
                Encoding.UTF8,
                "application/json");

            LogInformation($"Updating authentication settings for app service '{appServiceName}' in resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.PutAsync(
                _appServiceUpdateAuthSettingsUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{name}", appServiceName),
                body);

            AppServiceAuthSettings result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<AppServiceAuthSettings>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets an Azure App Service's application settings.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the app service resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the app service resides in.</param>
        /// <param name="appServiceName">The name of the app service.</param>
        /// <returns>The application settings.</returns>
        public async Task<AppServiceAppSettings> GetAppServiceAppSettingsAsync(string subscriptionId, string resourceGroupName, string appServiceName)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            LogInformation($"Acquiring application settings for app service '{appServiceName}' in resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _appServiceGetAppSettingsUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{name}", appServiceName),
                null);

            AppServiceAppSettings result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<AppServiceAppSettings>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerSettings()
                    {
                        ContractResolver = new DefaultContractResolver(),
                    });
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Updates an Azure App Service's application settings.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the app service resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the app service resides in.</param>
        /// <param name="appServiceName">The name of the app service.</param>
        /// <param name="settings">The updated application settings.</param>
        /// <returns>Returns the updated application settings.</returns>
        public async Task<AppServiceAppSettings> UpdateAppServiceAppSettingsAsync(string subscriptionId, string resourceGroupName, string appServiceName, AppServiceAppSettings settings)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            HttpContent body = new StringContent(
                JsonConvert.SerializeObject(
                    settings,
                    new JsonSerializerSettings()
                    {
                        ContractResolver = new DefaultContractResolver(),
                    }),
                Encoding.UTF8,
                "application/json");

            LogInformation($"Updating application settings for app service '{appServiceName}' in resource group '{resourceGroupName}' in subscription '{subscriptionId}'...");
            HttpResponseMessage response = await httpClient.PutAsync(
                _appServiceUpdateAppSettingsUri
                    .Replace("{subscriptionId}", subscriptionId)
                    .Replace("{resourceGroupName}", resourceGroupName)
                    .Replace("{name}", appServiceName),
                body);

            AppServiceAppSettings result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<AppServiceAppSettings>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Invokes an action on an Azure Resource.
        /// </summary>
        /// <param name="resourceId">The path to the resource.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="parameters">Parameters to pass to the action.</param>
        /// <param name="apiVersion">The API version to use.</param>
        /// <returns>The response content as a string.</returns>
        public async Task<string> InvokeResourceActionAsync(string resourceId, string action, string parameters, string apiVersion)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            HttpContent body = new StringContent(
                parameters,
                Encoding.UTF8,
                "application/json");

            LogInformation($"Invoking action '{action}' on resource '{resourceId}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _invokeResourceActionUri
                    .Replace("{resourceId}", resourceId)
                    .Replace("{action}", action)
                    .Replace("{apiVersion}", apiVersion),
                body);

            string result = null;

            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Invokes an action on an Azure Resource.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response content as.</typeparam>
        /// <param name="resourceId">The path to the resource.</param>
        /// <param name="action">The name of the action to invoke.</param>
        /// <param name="parameters">Parameters to pass to the action.</param>
        /// <param name="apiVersion">The API version to use.</param>
        /// <returns>The response content deserialized as the type requested.</returns>
        public async Task<T> InvokeResourceAction2Async<T>(string resourceId, string action, string parameters, string apiVersion)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            HttpContent body = new StringContent(
                parameters,
                Encoding.UTF8,
                "application/json");

            LogInformation($"Invoking action '{action}' on resource '{resourceId}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _invokeResourceActionUri
                    .Replace("{resourceId}", resourceId)
                    .Replace("{action}", action)
                    .Replace("{apiVersion}", apiVersion),
                body);

            T result = default(T);

            if (response.IsSuccessStatusCode)
            {
                if (response.Content != null)
                {
                    result = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                }
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Invokes an action on an Azure Resource.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response as.</typeparam>
        /// <param name="resourceId">The path to the resource.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="parameters">Parameters to pass to the action.</param>
        /// <param name="apiVersion">The API version to use.</param>
        /// <returns>The response content deserialized as an AzureValueCollectionResponse with values of the type requested.</returns>
        public async Task<AzureValueCollectionResponse<T>> InvokeResourceActionAsync<T>(string resourceId, string action, string parameters, string apiVersion)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            HttpContent body = new StringContent(
                parameters,
                Encoding.UTF8,
                "application/json");

            LogInformation($"Invoking action '{action}' on resource '{resourceId}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _invokeResourceActionUri
                    .Replace("{resourceId}", resourceId)
                    .Replace("{action}", action)
                    .Replace("{apiVersion}", apiVersion),
                body);

            AzureValueCollectionResponse<T> result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<AzureValueCollectionResponse<T>>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Shows a Windows Form with a Browser control for performing OAuth requests.
        /// </summary>
        /// <param name="uri">The URI to navigate to.</param>
        /// <param name="documentCompleted">Delegate for handling document completed events.</param>
        public void ShowOAuthDialog(Uri uri, Action<string> documentCompleted)
        {
            System.Windows.Forms.Form oauthDialog = new System.Windows.Forms.Form()
            {
                Width = 600,
                Height = 800,
                StartPosition = System.Windows.Forms.FormStartPosition.CenterParent,
                Text = "Please authenticate...",
            };

            System.Windows.Forms.WebBrowser oauthBrowser = new System.Windows.Forms.WebBrowser()
            {
                Width = 580,
                Height = 780,
                Url = uri,
            };

            oauthBrowser.ScriptErrorsSuppressed = true;
            oauthBrowser.DocumentCompleted += (object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e) =>
            {
                System.Windows.Forms.WebBrowser browser = (System.Windows.Forms.WebBrowser)sender;

                if (_connectionAuthRegEx.IsMatch(browser.Url.AbsoluteUri))
                {
                    documentCompleted(browser.Url.AbsoluteUri);
                    ((System.Windows.Forms.Form)browser.Parent).Close();
                }
            };

            oauthDialog.Controls.Add(oauthBrowser);
            oauthDialog.Shown += (object sender, EventArgs e) =>
            {
                ((System.Windows.Forms.Form)sender).Activate();
            };
            oauthDialog.ShowDialog();
        }

        /// <summary>
        /// Gets a LUIS application.
        /// </summary>
        /// <param name="authoringKey">The authoring key to use for making requests.</param>
        /// <param name="appId">The id of the application.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <returns>The LUIS application.</returns>
        public async Task<LuisApplication> GetLuisAppAsync(NetworkCredential authoringKey, string appId, string authoringRegion)
        {
            IHttpClient httpClient = TokenProvider.GetGenericHttpClient();

            httpClient.SetHeader("Ocp-Apim-Subscription-Key", authoringKey.Password);

            LogInformation($"Acquiring LUIS application with id '{appId}' in region '{authoringRegion}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _luisAppsWithIdUri
                    .Replace("{authoringRegion}", authoringRegion)
                    .Replace("{appId}", appId));

            LuisApplication result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<LuisApplication>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Get a LUIS application based on a name.
        /// </summary>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <param name="appName">The name of the LUIS application.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <returns>The LUIS application.</returns>
        public async Task<LuisApplication> GetLuisAppByNameAsync(NetworkCredential authoringKey, string appName, string authoringRegion)
        {
            IHttpClient httpClient = TokenProvider.GetGenericHttpClient();

            httpClient.SetHeader("Ocp-Apim-Subscription-Key", authoringKey.Password);

            LogInformation($"Acquiring LUIS applications in region '{authoringRegion}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _luisAppsUri
                    .Replace("{authoringRegion}", authoringRegion));

            LuisApplication[] result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<LuisApplication[]>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            LogInformation($"Returning LUIS application named '{appName}' if it exists...");
            return result
                .Where(x => string.Compare(x.Name, appName, true) == 0)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets LUIS applications.
        /// </summary>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <returns>A list of LUIS applications.</returns>
        public async Task<LuisApplication[]> GetLuisAppsAsync(NetworkCredential authoringKey, string authoringRegion)
        {
            IHttpClient httpClient = TokenProvider.GetGenericHttpClient();

            httpClient.SetHeader("Ocp-Apim-Subscription-Key", authoringKey.Password);

            LogInformation($"Acquiring LUIS applications in region '{authoringRegion}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _luisAppsUri
                    .Replace("{authoringRegion}", authoringRegion));

            LuisApplication[] result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<LuisApplication[]>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Imports a LUIS application.
        /// </summary>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <param name="appName">The name of the application to import.</param>
        /// <param name="appFilePath">Path to the LUIS application definition file.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <returns>The response content as a string.</returns>
        public async Task<string> ImportLuisAppAsync(NetworkCredential authoringKey, string appName, string appFilePath, string authoringRegion)
        {
            IHttpClient httpClient = TokenProvider.GetGenericHttpClient();

            httpClient.SetHeader("Ocp-Apim-Subscription-Key", authoringKey.Password);

            HttpContent body = new StringContent(
                File.ReadAllText(appFilePath),
                Encoding.UTF8,
                "application/json");

            LogInformation($"Importing LUIS application from '{appFilePath}' as '{appName}' in region '{authoringRegion}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _luisImportUri
                    .Replace("{authoringRegion}", authoringRegion)
                    .Replace("{appName}", appName),
                body);

            string result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Deletes a LUIS application.
        /// </summary>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <param name="appId">The id of the application.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <returns>The response content as a LuisGeneralResponse object.</returns>
        public async Task<LuisGeneralResponse> DeleteLuisAppAsync(NetworkCredential authoringKey, string appId, string authoringRegion)
        {
            IHttpClient httpClient = TokenProvider.GetGenericHttpClient();

            httpClient.SetHeader("Ocp-Apim-Subscription-Key", authoringKey.Password);

            LogInformation($"Deleting LUIS application from '{appId}' in region '{authoringRegion}'...");
            HttpResponseMessage response = await httpClient.DeleteAsync(
                _luisAppsWithIdUri
                    .Replace("{authoringRegion}", authoringRegion)
                    .Replace("{appId}", appId));

            LuisGeneralResponse result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<LuisGeneralResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Associates an Azure LUIS Cognitive Services resource with a LUIS application.
        /// </summary>
        /// <param name="appId">The id of the LUIS application.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <param name="authoringKey">The authoring key used to make requests to the LUIS Authoring API.</param>
        /// <param name="resourceToApp">Configuration information for associating the Azure LUIS Cognitive Services resource with the LUIS application.</param>
        /// <returns>The response content as a LuisGeneralResponse object.</returns>
        public async Task<LuisGeneralResponse> AssociateAzureResourceWithLuisAppAsync(string appId, string authoringRegion, NetworkCredential authoringKey, LuisAssociatedAzureResourceRequest resourceToApp)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(ManagementAudience);

            httpClient.SetHeader("Ocp-Apim-Subscription-Key", authoringKey.Password);

            HttpContent body = new StringContent(
                JsonConvert.SerializeObject(resourceToApp),
                Encoding.UTF8,
                "application/json");

            LogInformation($"Associating Azure resource '{resourceToApp.AccountName}' in resource group '{resourceToApp.ResourceGroup}' in subscription '{resourceToApp.AzureSubscriptionId}' with LUIS application '{appId}' in region '{authoringRegion}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _luisAssociatedAzureResourceUri
                    .Replace("{authoringRegion}", authoringRegion)
                    .Replace("{appId}", appId),
                body);

            LuisGeneralResponse result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<LuisGeneralResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Trains models in a LUIS application.
        /// </summary>
        /// <param name="appId">The id of the application.</param>
        /// <param name="appVersion">The version of the application to train.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <param name="authoringKey">The authoring key to use to make requests.</param>
        /// <returns>A list of model training statuses.</returns>
        public LuisModelTrainingStatus[] TrainLuisApp(string appId, string appVersion, string authoringRegion, NetworkCredential authoringKey)
        {
            LuisTrainModelResponse startTrainingResponse = TrainLuisAppAsync(appId, appVersion, authoringRegion, authoringKey).Result;

            if (startTrainingResponse.StatusId != 9 &&
                startTrainingResponse.StatusId != 2)
            {
                LogWarning($"Unexpected status ({startTrainingResponse.StatusId})");
                return null;
            }

            LuisModelTrainingStatus[] trainingStatus = null;

            do
            {
                LogInformation("Sleeping until next poll...");
                Thread.Sleep(2000);

                trainingStatus = GetLuisTrainingStatusAsync(appId, appVersion, authoringRegion, authoringKey).Result;
            }
            while (trainingStatus.Where(x => x.Details.StatusId == 3).Count() > 0);

            return trainingStatus;
        }

        /// <summary>
        /// Trains models in a LUIS application.
        /// </summary>
        /// <param name="appId">The id of the application.</param>
        /// <param name="appVersion">The version of the application to train.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <returns>The initial training response.</returns>
        public async Task<LuisTrainModelResponse> TrainLuisAppAsync(string appId, string appVersion, string authoringRegion, NetworkCredential authoringKey)
        {
            IHttpClient httpClient = TokenProvider.GetGenericHttpClient();

            httpClient.SetHeader("Ocp-Apim-Subscription-Key", authoringKey.Password);

            LogInformation($"Training LUIS application '{appId}' version '{appVersion}' in region '{authoringRegion}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _luisTrainUri
                    .Replace("{authoringRegion}", authoringRegion)
                    .Replace("{appId}", appId)
                    .Replace("{appVersion}", appVersion),
                null);

            LuisTrainModelResponse result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<LuisTrainModelResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets the training status of a LUIS application.
        /// </summary>
        /// <param name="appId">The id of the application.</param>
        /// <param name="appVersion">The version of the application to check the training status for.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <returns>A list of the model training statuses.</returns>
        public async Task<LuisModelTrainingStatus[]> GetLuisTrainingStatusAsync(string appId, string appVersion, string authoringRegion, NetworkCredential authoringKey)
        {
            IHttpClient httpClient = TokenProvider.GetGenericHttpClient();

            httpClient.SetHeader("Ocp-Apim-Subscription-Key", authoringKey.Password);

            LogInformation($"Acquiring training status for LUIS application '{appId}' version '{appVersion}' in region '{authoringRegion}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _luisTrainUri
                    .Replace("{authoringRegion}", authoringRegion)
                    .Replace("{appId}", appId)
                    .Replace("{appVersion}", appVersion));

            LuisModelTrainingStatus[] result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<LuisModelTrainingStatus[]>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Publishes a LUIS application.
        /// </summary>
        /// <param name="appId">The id of the application.</param>
        /// <param name="appVersion">The version of the application to publish.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <returns>Information regarding the published application.</returns>
        public async Task<LuisPublishResponse> PublishLuisAppAsync(string appId, string appVersion, string authoringRegion, NetworkCredential authoringKey)
        {
            string publishApp = $@"{{
        ""versionId"": ""{appVersion}"",
        ""isStaging"": false
}}";
            IHttpClient httpClient = TokenProvider.GetGenericHttpClient();

            httpClient.SetHeader("Ocp-Apim-Subscription-Key", authoringKey.Password);

            HttpContent body = new StringContent(
                publishApp,
                Encoding.UTF8,
                "application/json");

            LogInformation($"Publishing LUIS application '{appId}' in region '{authoringRegion}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _luisPublishUri
                    .Replace("{authoringRegion}", authoringRegion)
                    .Replace("{appId}", appId),
                body);

            LuisPublishResponse result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<LuisPublishResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Authenticates an API connection.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the API connection resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the API connection resides in.</param>
        /// <param name="connectionName">The name of the API connection.</param>
        /// <returns>True if successful, false if failed.</returns>
        public bool AuthenticateApiConnection(string subscriptionId, string resourceGroupName, string connectionName)
        {
            ApiConnectionResource resource = GetResourceAsync<ApiConnectionResource>(
                subscriptionId,
                resourceGroupName,
                "Microsoft.Web",
                null,
                "connections",
                connectionName).Result;

            if (resource == null)
            {
                throw new Exception("Resource does not exists!");
            }

            ListConsentLinksActionRequest listConsentLinksParameters = new ListConsentLinksActionRequest()
            {
                Parameters = new ListConsentLinksActionParameters[]
                {
                    new ListConsentLinksActionParameters()
                    {
                        ParameterName = "token",
                        RedirectUrl = "https://ema1.exp.azure.com/ema/default/authredirect",
                    },
                },
            };

            AzureValueCollectionResponse<ApiConnectionConsentLink> result = InvokeResourceActionAsync<ApiConnectionConsentLink>(
                resource.Id,
                "listConsentLinks",
                JsonConvert.SerializeObject(listConsentLinksParameters),
                "2016-06-01").Result;

            if (result == null)
            {
                throw new Exception("Could not acquire consent links for API connection!");
            }

            ApiConnectionConsentLink consentLink = result.Value.First();

            if (string.Compare(consentLink.Status, "authenticated", true) != 0)
            {
                string code = null;

                TokenProvider.GetUIContext().Dispatcher.Invoke(new Action(() =>
                {
                    ShowOAuthDialog(
                        new Uri(result.Value[0].Link),
                        (uri) =>
                        {
                            code = _codeRegEx.Matches(uri)[0].Groups[2].Value;
                        });
                }));

                string consentParameters = @"{
                    ""code"": ""{code}""
                }";

                InvokeResourceActionAsync(
                    resource.Id,
                    "confirmConsentCode",
                    consentParameters.Replace("{code}", code),
                    "2016-06-01").Wait();
            }

            return true;
        }
    }
}
