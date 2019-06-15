// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Interface for REST clients interacting with Azure APIs.
    /// </summary>
    public interface IAzureClient : ILoggedClient
    {
        /// <summary>
        /// Gets a list of available tenants.
        /// </summary>
        /// <returns>List of tenants.</returns>
        Task<Tenant[]> GetTenantsAsync();

        /// <summary>
        /// Gets a list of available subscriptions.
        /// </summary>
        /// <returns>A list of available subscriptions.</returns>
        Task<Subscription[]> GetSubscriptionsAsync();

        /// <summary>
        /// Checks if a resource group exists.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="name">The name of the resource group.</param>
        /// <returns>True if the resource group exists, false if not.</returns>
        Task<bool> ResourceGroupExistsAsync(string subscriptionId, string name);

        /// <summary>
        /// Gets available resource groups.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to look for resource groups in.</param>
        /// <returns>A list of available resource groups.</returns>
        Task<ResourceGroup[]> GetResourceGroupsAsync(string subscriptionId);

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
        Task<string> GetResourceAsync(string subscriptionId, string resourceGroupName, string resourceProviderNamespace, string parentResourcePath, string resourceType, string resourceName, string apiVersion = "2016-06-01");

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
        Task<T> GetResourceAsync<T>(string subscriptionId, string resourceGroupName, string resourceProviderNamespace, string parentResourcePath, string resourceType, string resourceName, string apiVersion = "2016-06-01");

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
        Task<AzureResourceBase> GetResourceBaseAsync(string subscriptionId, string resourceGroupName, string resourceProviderNamespace, string parentResourcePath, string resourceType, string resourceName, string apiVersion = "2016-06-01");

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
        Task<AzureIdentityResourceBase> GetResourceIdentityBaseAsync(string subscriptionId, string resourceGroupName, string resourceProviderNamespace, string parentResourcePath, string resourceType, string resourceName, string apiVersion = "2016-06-01");

        /// <summary>
        /// Gets locks on a resource group.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group.</param>
        /// <returns>A list of resource locks.</returns>
        Task<AzureValueCollectionResponse<ResourceLock>> GetResourceGroupLocksAsync(string subscriptionId, string resourceGroupName);

        /// <summary>
        /// Deletes a resource group lock.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the lock resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the lock resides in.</param>
        /// <param name="lockId">The id of the lock.</param>
        /// <returns>The response content as a string.</returns>
        Task<string> DeleteResourceGroupLockAsync(string subscriptionId, string resourceGroupName, string lockId);

        /// <summary>
        /// Deletes a resource lock.
        /// </summary>
        /// <param name="scope">The scope of the lock.</param>
        /// <returns>The response content as a string.</returns>
        Task<string> DeleteResourceLockAsync(string scope);

        /// <summary>
        /// Validates a resource group deployment.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to validate in.</param>
        /// <param name="resourceGroupName">The name of the resource group to validate in.</param>
        /// <param name="deploymentName">The name to give to the deployment validation.</param>
        /// <param name="templateFilePath">Path to the ARM template file to validate.</param>
        /// <param name="templateParametersFilePath">Path to the ARM template parameters file to validate.</param>
        /// <returns>True if valid, false if invalid.</returns>
        Task<bool> ValidateResourceGroupDeploymentAsync(string subscriptionId, string resourceGroupName, string deploymentName, string templateFilePath, string templateParametersFilePath);

        /// <summary>
        /// Creates a resource group deployment.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to deploy to.</param>
        /// <param name="resourceGroupName">The name of the resource group to deploy to.</param>
        /// <param name="deploymentName">The name to give to the deployment.</param>
        /// <param name="templateFilePath">Path to the ARM template file.</param>
        /// <param name="templateParametersFilePath">Path to the ARM template parameters file.</param>
        /// <returns>The initial resource group deployment status.</returns>
        ResourceGroupDeploymentStatus CreateResourceGroupDeployment(string subscriptionId, string resourceGroupName, string deploymentName, string templateFilePath, string templateParametersFilePath);

        /// <summary>
        /// Gets the response from a URL provided via a previous response's Location header.
        /// </summary>
        /// <param name="audience">The audience to acquire an access token for.</param>
        /// <param name="location">The URL to perform the request against.</param>
        /// <returns>The response message.</returns>
        Task<HttpResponseMessage> GetLocationAsync(string audience, Uri location);

        /// <summary>
        /// Creates a resource group deployment.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription to deploy to.</param>
        /// <param name="resourceGroupName">The name of the resource group to deploy to.</param>
        /// <param name="deploymentName">The name to give to the deployment.</param>
        /// <param name="templateFilePath">Path to the ARM template file.</param>
        /// <param name="templateParametersFilePath">Path to the ARM template parameters file.</param>
        /// <returns>The response content as an AzureResponseBase object.</returns>
        Task<AzureResponseBase> CreateResourceGroupDeploymentAsync(string subscriptionId, string resourceGroupName, string deploymentName, string templateFilePath, string templateParametersFilePath);

        /// <summary>
        /// Gets the status of an asynchronous Azure operation.
        /// </summary>
        /// <param name="url">The URL to make the request against.</param>
        /// <returns>The response content as a string.</returns>
        Task<string> GetAsyncOperationStatusAsync(string url);

        /// <summary>
        /// Gets a resource group.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="name">The name of the resource group.</param>
        /// <returns>Metadata about the resource group.</returns>
        Task<ResourceGroup> GetResourceGroupAsync(string subscriptionId, string name);

        /// <summary>
        /// Creates a new or updates an existing resource group.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="resourceGroup">The name of the resource group.</param>
        /// <returns>Metadata about the new or updates resource group.</returns>
        Task<ResourceGroup> CreateOrUpdateResourceGroupAsync(string subscriptionId, ResourceGroup resourceGroup);

        /// <summary>
        /// Deletes a resource group.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="name">The name of the resource group.</param>
        /// <returns>True if successful, false if failed.</returns>
        bool DeleteResourceGroup(string subscriptionId, string name);

        /// <summary>
        /// Deletes a resource group.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the resource group resides in.</param>
        /// <param name="name">The name of the resource group.</param>
        /// <returns>The response content as an AzureResponseBase object.</returns>
        Task<AzureResponseBase> DeleteResourceGroupAsync(string subscriptionId, string name);

        /// <summary>
        /// Gets application registrations.
        /// Note: Requires tenant admin consent.
        /// </summary>
        /// <returns>A list of application registrations.</returns>
        Task<AzureValueCollectionResponse<AzureApplication>> GetApplicationsAsync();

        /// <summary>
        /// Gets an application registration.
        /// Note: Requires admin consent.
        /// </summary>
        /// <param name="applicationName">The display name of the application registration.</param>
        /// <returns>The application registration.</returns>
        Task<AzureApplication> GetApplicationAsync(string applicationName);

        /// <summary>
        /// Creates an application registration.
        /// Note: Requires admin consent.
        /// </summary>
        /// <param name="application">Configuration information for the application registration.</param>
        /// <returns>The application registration.</returns>
        Task<AzureApplication> CreateApplicationAsync(AzureApplicationRequestBase application);

        /// <summary>
        /// Deletes an application registration.
        /// Note: requires admin consent.
        /// </summary>
        /// <param name="applicationId">The id of the application registration.</param>
        /// <returns>True if successful, false if failed.</returns>
        Task<bool> DeleteApplicationAsync(string applicationId);

        /// <summary>
        /// Gets a service principal.
        /// </summary>
        /// <param name="servicePrincipalId">The id of the service principal.</param>
        /// <returns>The service principal.</returns>
        Task<ServicePrincipal> GetServicePrincipal(string servicePrincipalId);

        /// <summary>
        /// Creates an Azure Key Vault Access Policy.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the key vault resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the key vault resides in.</param>
        /// <param name="keyVaultName">The name of the key vault resource.</param>
        /// <param name="accessPolicy">The name to assign to the access policy.</param>
        /// <returns>The response content.</returns>
        Task<CreateKeyVaultAccessPolicyResponse> CreateKeyVaultAccessPolicyAsync(string subscriptionId, string resourceGroupName, string keyVaultName, CreateKeyVaultAccessPolicyRequest accessPolicy);

        /// <summary>
        /// Creates or updates an Azure Key Vault secret.
        /// </summary>
        /// <param name="keyVaultName">The name of the key vault.</param>
        /// <param name="secretName">The name of the secret.</param>
        /// <param name="secretValue">The value to set the secret to.</param>
        /// <returns>Information about the secret.</returns>
        Task<AzureKeyVaultSecret> UpdateKeyVaultSecretAsync(string keyVaultName, string secretName, NetworkCredential secretValue);

        /// <summary>
        /// Creates an Azure Blob Storage container Stored Access Policy.
        /// </summary>
        /// <param name="storageAccountName">The name of the storage account.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="storedAccessPolicy">The name to assign to the stored access policy.</param>
        /// <returns>The response content as a string.</returns>
        Task<string> CreateBlobStoredAccessPolicyAsync(string storageAccountName, string containerName, SignedIdentifiers storedAccessPolicy);

        /// <summary>
        /// Creates an Azure Storage Blob container shared access signature from a stored access policy.
        /// </summary>
        /// <param name="storageAccountName">The name of the storage account.</param>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="accessKey">A storage access key for authentication.</param>
        /// <param name="sharedAccessPolicyId">The id of the stored access policy to create the shared access signature from.</param>
        /// <param name="policy">Configuration information describing the desired shared access signature.</param>
        /// <returns>The response content as a string.</returns>
        string CreateSharedAccessSignature(string storageAccountName, string containerName, NetworkCredential accessKey, string sharedAccessPolicyId, SharedAccessBlobPolicy policy);

        /// <summary>
        /// Gets an Azure App Service's publishing profiles.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the app service resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the app service resides in.</param>
        /// <param name="appServiceName">The name of the app service.</param>
        /// <returns>The publishing profiles.</returns>
        Task<PublishData> GetAppServicePublishingProfileAsync(string subscriptionId, string resourceGroupName, string appServiceName);

        /// <summary>
        /// Deploys a ZIP deployment of an Azure Function.
        /// </summary>
        /// <param name="appServiceName">The name of the app service to deploy to.</param>
        /// <param name="zipUri">The URI of the binaries ZIP archive.</param>
        /// <param name="publishProfile">Credentials for the publishing profile to use.</param>
        /// <returns>The response content as a string.</returns>
        Task<string> ZipDeployAppServiceAsync(string appServiceName, string zipUri, NetworkCredential publishProfile);

        /// <summary>
        /// Gets an Azure App Service's authentication settings.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the app service resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the app service resides in.</param>
        /// <param name="appServiceName">The name of the app service.</param>
        /// <returns>The app service's authentication settings.</returns>
        Task<AppServiceAuthSettings> GetAppServiceAuthSettingsAsync(string subscriptionId, string resourceGroupName, string appServiceName);

        /// <summary>
        /// Updates an Azure App Service's authentication settings.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the app service resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the app service resides in.</param>
        /// <param name="appServiceName">The name of the app service.</param>
        /// <param name="settings">The updated authentication settings.</param>
        /// <returns>Returns the updated authentication settings.</returns>
        Task<AppServiceAuthSettings> UpdateAppServiceAuthSettingsAsync(string subscriptionId, string resourceGroupName, string appServiceName, AppServiceAuthSettings settings);

        /// <summary>
        /// Gets an Azure App Service's application settings.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the app service resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the app service resides in.</param>
        /// <param name="appServiceName">The name of the app service.</param>
        /// <returns>The application settings.</returns>
        Task<AppServiceAppSettings> GetAppServiceAppSettingsAsync(string subscriptionId, string resourceGroupName, string appServiceName);

        /// <summary>
        /// Updates an Azure App Service's application settings.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the app service resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the app service resides in.</param>
        /// <param name="appServiceName">The name of the app service.</param>
        /// <param name="settings">The updated application settings.</param>
        /// <returns>Returns the updated application settings.</returns>
        Task<AppServiceAppSettings> UpdateAppServiceAppSettingsAsync(string subscriptionId, string resourceGroupName, string appServiceName, AppServiceAppSettings settings);

        /// <summary>
        /// Invokes an action on an Azure Resource.
        /// </summary>
        /// <param name="resourceId">The path to the resource.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="parameters">Parameters to pass to the action.</param>
        /// <param name="apiVersion">The API version to use.</param>
        /// <returns>The response content as a string.</returns>
        Task<string> InvokeResourceActionAsync(string resourceId, string action, string parameters, string apiVersion);

        /// <summary>
        /// Invokes an action on an Azure Resource.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response as.</typeparam>
        /// <param name="resourceId">The path to the resource.</param>
        /// <param name="action">The action to invoke.</param>
        /// <param name="parameters">Parameters to pass to the action.</param>
        /// <param name="apiVersion">The API version to use.</param>
        /// <returns>The response content deserialized as an AzureValueCollectionResponse with values of the type requested.</returns>
        Task<AzureValueCollectionResponse<T>> InvokeResourceActionAsync<T>(string resourceId, string action, string parameters, string apiVersion);

        /// <summary>
        /// Invokes an action on an Azure Resource.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response content as.</typeparam>
        /// <param name="resourceId">The path to the resource.</param>
        /// <param name="action">The name of the action to invoke.</param>
        /// <param name="parameters">Parameters to pass to the action.</param>
        /// <param name="apiVersion">The API version to use.</param>
        /// <returns>The response content deserialized as the type requested.</returns>
        Task<T> InvokeResourceAction2Async<T>(string resourceId, string action, string parameters, string apiVersion);

        /// <summary>
        /// Shows a Windows Form with a Browser control for performing OAuth requests.
        /// </summary>
        /// <param name="uri">The URI to navigate to.</param>
        /// <param name="documentCompleted">Delegate for handling document completed events.</param>
        void ShowOAuthDialog(Uri uri, Action<string> documentCompleted);

        /// <summary>
        /// Gets a LUIS application.
        /// </summary>
        /// <param name="authoringKey">The authoring key to use for making requests.</param>
        /// <param name="appId">The id of the application.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <returns>The LUIS application.</returns>
        Task<LuisApplication> GetLuisAppAsync(NetworkCredential authoringKey, string appId, string authoringRegion);

        /// <summary>
        /// Get a LUIS application based on a name.
        /// </summary>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <param name="appName">The name of the LUIS application.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <returns>The LUIS application.</returns>
        Task<LuisApplication> GetLuisAppByNameAsync(NetworkCredential authoringKey, string appName, string authoringRegion);

        /// <summary>
        /// Gets LUIS applications.
        /// </summary>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <returns>A list of LUIS applications.</returns>
        Task<LuisApplication[]> GetLuisAppsAsync(NetworkCredential authoringKey, string authoringRegion);

        /// <summary>
        /// Imports a LUIS application.
        /// </summary>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <param name="appName">The name of the application to import.</param>
        /// <param name="appFilePath">Path to the LUIS application definition file.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <returns>The response content as a string.</returns>
        Task<string> ImportLuisAppAsync(NetworkCredential authoringKey, string appName, string appFilePath, string authoringRegion);

        /// <summary>
        /// Deletes a LUIS application.
        /// </summary>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <param name="appId">The id of the application.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <returns>The response content as a LuisGeneralResponse object.</returns>
        Task<LuisGeneralResponse> DeleteLuisAppAsync(NetworkCredential authoringKey, string appId, string authoringRegion);

        /// <summary>
        /// Associates an Azure LUIS Cognitive Services resource with a LUIS application.
        /// </summary>
        /// <param name="appId">The id of the LUIS application.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <param name="authoringKey">The authoring key used to make requests to the LUIS Authoring API.</param>
        /// <param name="resourceToApp">Configuration information for associating the Azure LUIS Cognitive Services resource with the LUIS application.</param>
        /// <returns>The response content as a LuisGeneralResponse object.</returns>
        Task<LuisGeneralResponse> AssociateAzureResourceWithLuisAppAsync(string appId, string authoringRegion, NetworkCredential authoringKey, LuisAssociatedAzureResourceRequest resourceToApp);

        /// <summary>
        /// Trains models in a LUIS application.
        /// </summary>
        /// <param name="appId">The id of the application.</param>
        /// <param name="appVersion">The version of the application to train.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <param name="authoringKey">The authoring key to use to make requests.</param>
        /// <returns>A list of model training statuses.</returns>
        LuisModelTrainingStatus[] TrainLuisApp(string appId, string appVersion, string authoringRegion, NetworkCredential authoringKey);

        /// <summary>
        /// Trains models in a LUIS application.
        /// </summary>
        /// <param name="appId">The id of the application.</param>
        /// <param name="appVersion">The version of the application to train.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <returns>The initial training response.</returns>
        Task<LuisTrainModelResponse> TrainLuisAppAsync(string appId, string appVersion, string authoringRegion, NetworkCredential authoringKey);

        /// <summary>
        /// Gets the training status of a LUIS application.
        /// </summary>
        /// <param name="appId">The id of the application.</param>
        /// <param name="appVersion">The version of the application to check the training status for.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <returns>A list of the model training statuses.</returns>
        Task<LuisModelTrainingStatus[]> GetLuisTrainingStatusAsync(string appId, string appVersion, string authoringRegion, NetworkCredential authoringKey);

        /// <summary>
        /// Publishes a LUIS application.
        /// </summary>
        /// <param name="appId">The id of the application.</param>
        /// <param name="appVersion">The version of the application to publish.</param>
        /// <param name="authoringRegion">The authoring region of the LUIS account.</param>
        /// <param name="authoringKey">The authoring key to use for requests.</param>
        /// <returns>Information regarding the published application.</returns>
        Task<LuisPublishResponse> PublishLuisAppAsync(string appId, string appVersion, string authoringRegion, NetworkCredential authoringKey);

        /// <summary>
        /// Authenticates an API connection.
        /// </summary>
        /// <param name="subscriptionId">The id of the subscription the API connection resides in.</param>
        /// <param name="resourceGroupName">The name of the resource group the API connection resides in.</param>
        /// <param name="connectionName">The name of the API connection.</param>
        /// <returns>True if successful, false if failed.</returns>
        bool AuthenticateApiConnection(string subscriptionId, string resourceGroupName, string connectionName);
    }
}
