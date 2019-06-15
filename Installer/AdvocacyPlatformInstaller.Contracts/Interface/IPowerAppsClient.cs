// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for REST clients interacting with PowerApps APIs.
    /// </summary>
    public interface IPowerAppsClient : ILoggedClient
    {
        /// <summary>
        /// Gets available environment locations.
        /// </summary>
        /// <returns>Information regarding the available environment locations.</returns>
        Task<GetPowerAppsEnvironmentLocationsResponse> GetEnvironmentLocationsAsync();

        /// <summary>
        /// Gets environments.
        /// </summary>
        /// <param name="environmentName">The name of the environment (null returns all available).</param>
        /// <returns>Information regarding the available environment(s).</returns>
        Task<object> GetEnvironmentsAsync(string environmentName = null);

        /// <summary>
        /// Gets an environment based on a display name.
        /// </summary>
        /// <param name="displayName">The display name to search for.</param>
        /// <returns>Information regarding the environment.</returns>
        Task<PowerAppsEnvironment> GetEnvironmentByDisplayNameAsync(string displayName);

        /// <summary>
        /// Creates a new environment.
        /// </summary>
        /// <param name="environment">Configuration information for the new environment.</param>
        /// <returns>Information regarding the new environment.</returns>
        Task<CreatePowerAppsEnvironmentResponse> CreateEnvironmentAsync(CreatePowerAppsEnvironmentRequest environment);

        /// <summary>
        /// Validate the ability to delete an environment.
        /// </summary>
        /// <param name="environmentName">The name of the environment to delete.</param>
        /// <returns>True if environment can be deleted, false if it cannot.</returns>
        Task<bool?> ValidateDeleteEnvironmentAsync(string environmentName);

        /// <summary>
        /// Deletes an environment.
        /// </summary>
        /// <param name="environmentName">The name of the environment to delete.</param>
        /// <returns>The response content as an AzureResponseBase content.</returns>
        Task<AzureResponseBase> DeleteEnvironmentAsync(string environmentName);

        /// <summary>
        /// Gets available CDS database currencies.
        /// </summary>
        /// <param name="location">The location to get CDS database currencies for.</param>
        /// <returns>Information regarding the available CDS database currencies.</returns>
        Task<GetPowerAppsCurrenciesResponse> GetCdsDatabaseCurrenciesAsync(string location);

        /// <summary>
        /// Gets the available CDS database languages.
        /// </summary>
        /// <param name="location">The location to get CDS database languages for.</param>
        /// <returns>Information regarding the available CDS database languages.</returns>
        Task<GetPowerAppsLanguagesResponse> GetCdsDatabaseLanguagesAsync(string location);

        /// <summary>
        /// Create a CDS database.
        /// </summary>
        /// <param name="environmentName">The name of the environment to create the CDS database in.</param>
        /// <param name="cdsDatabase">Configuration information regarding the new CDS database.</param>
        /// <returns>Information regarding the PowerApps environment after the CDS database creation.</returns>
        PowerAppsEnvironment CreateCdsDatabase(string environmentName, CreatePowerAppsCdsDatabaseRequest cdsDatabase);

        /// <summary>
        /// Performs a request against a URI returned in a previous response's Location Header.
        /// </summary>
        /// <param name="audience">The audience to acquire an access token for.</param>
        /// <param name="location">The URI to make the request against.</param>
        /// <returns>The response message.</returns>
        Task<HttpResponseMessage> GetLocationAsync(string audience, Uri location);

        /// <summary>
        /// Creates a CDS database.
        /// </summary>
        /// <param name="environmentName">The name of the environment to create the CDS database in.</param>
        /// <param name="cdsDatabase">Configuration information regarding the CDS database to create.</param>
        /// <returns>The response content as an AzureResponseBase object.</returns>
        Task<AzureResponseBase> CreateCdsDatabaseAsync(string environmentName, CreatePowerAppsCdsDatabaseRequest cdsDatabase);
    }
}
