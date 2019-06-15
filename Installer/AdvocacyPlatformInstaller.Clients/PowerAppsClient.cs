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
    using System.Security;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AdvocacyPlatformInstaller.Contracts;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Simple REST client for interacting with PowerApps APIs.
    /// </summary>
    public class PowerAppsClient : TokenBasedClient, IPowerAppsClient
    {
        /// <summary>
        /// The audience for the PowerApps API.
        /// </summary>
        public const string Audience = "https://service.powerapps.com/";

        private const string _baseAddress = "https://api.bap.microsoft.com/";

        private static readonly string _getEnvironmentLocationsUri = $"{_baseAddress}providers/Microsoft.BusinessAppPlatform/locations?api-version=2016-11-01";
        private static readonly string _getEnvironmentsUri = $"{_baseAddress}providers/Microsoft.BusinessAppPlatform/scopes/admin/environments?$expand=permissions&api-version=2016-11-01";
        private static readonly string _getEnvironmentUri = $"{_baseAddress}providers/Microsoft.BusinessAppPlatform/scopes/admin/environments/{{environmentName}}?$expand=permissions&api-version=2016-11-01";
        private static readonly string _getEnvironmentByDisplayNameUri = $"{_baseAddress}providers/Microsoft.BusinessAppPlatform/scopes/admin/environments?$filter=properties.displayName eq '{{displayName}}'&api-version=2016-11-01";
        private static readonly string _newEnvironmentUri = $"{_baseAddress}providers/Microsoft.BusinessAppPlatform/environments?api-version=2018-01-01&id=/providers/Microsoft.BusinessAppPlatform/scopes/admin/environments";
        private static readonly string _getCdsDatabaseCurrenciesUri = $"{_baseAddress}providers/Microsoft.BusinessAppPlatform/locations/{{location}}/environmentCurrencies?api-version=2016-11-01";
        private static readonly string _getCdsDatabaseLanguagesUri = $"{_baseAddress}providers/Microsoft.BusinessAppPlatform/locations/{{location}}/environmentLanguages?api-version=2016-11-01";
        private static readonly string _newCdsDatabaseUri = $"{_baseAddress}providers/Microsoft.BusinessAppPlatform/environments/{{environmentName}}/provisionInstance?api-version=2018-01-01";
        private static readonly string _validateDeleteEnvironmentUri = $"{_baseAddress}providers/Microsoft.BusinessAppPlatform/scopes/admin/environments/{{environmentName}}/validateDelete?api-version=2018-01-01";
        private static readonly string _deleteEnvironmentUri = $"{_baseAddress}providers/Microsoft.BusinessAppPlatform/scopes/admin/environments/{{environmentName}}?api-version=2018-01-01";

        /// <summary>
        /// Initializes a new instance of the <see cref="PowerAppsClient"/> class.
        /// </summary>
        /// <param name="tokenProvider">The token provider to use for acquiring access tokens.</param>
        public PowerAppsClient(ITokenProvider tokenProvider)
                : base(tokenProvider)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new JsonCamelCaseContractResolver(),
            };
        }

        /// <summary>
        /// Gets available environment locations.
        /// </summary>
        /// <returns>Information regarding the available environment locations.</returns>
        public async Task<GetPowerAppsEnvironmentLocationsResponse> GetEnvironmentLocationsAsync()
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(Audience);

            LogInformation("Acquiring PowerApps environment locations...");
            HttpResponseMessage response = await httpClient.GetAsync(_getEnvironmentLocationsUri);

            GetPowerAppsEnvironmentLocationsResponse result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<GetPowerAppsEnvironmentLocationsResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets environments.
        /// </summary>
        /// <param name="environmentName">The name of the environment (null returns all available).</param>
        /// <returns>Information regarding the available environment(s).</returns>
        public async Task<object> GetEnvironmentsAsync(string environmentName = null)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(Audience);

            LogInformation(environmentName != null ? $"Acquiring PowerApps environment with id '{environmentName}'..." : "Acquiring PowerApps environments...");
            HttpResponseMessage response = await httpClient.GetAsync(
                environmentName != null ?
                    _getEnvironmentUri.Replace("{environmentName}", environmentName) :
                        _getEnvironmentsUri);

            if (response.IsSuccessStatusCode)
            {
                if (environmentName == null)
                {
                    return JsonConvert.DeserializeObject<GetPowerAppsEnvironmentsResponse>(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    return JsonConvert.DeserializeObject<PowerAppsEnvironment>(await response.Content.ReadAsStringAsync());
                }
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }
        }

        /// <summary>
        /// Gets an environment based on a display name.
        /// </summary>
        /// <param name="displayName">The display name to search for.</param>
        /// <returns>Information regarding the environment.</returns>
        public async Task<PowerAppsEnvironment> GetEnvironmentByDisplayNameAsync(string displayName)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(Audience);

            LogInformation($"Acquiring PowerApps environment named '{displayName}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _getEnvironmentByDisplayNameUri
                    .Replace("{displayName}", displayName));

            PowerAppsEnvironment result = null;

            if (response.IsSuccessStatusCode)
            {
                GetPowerAppsEnvironmentsResponse environments = JsonConvert.DeserializeObject<GetPowerAppsEnvironmentsResponse>(await response.Content.ReadAsStringAsync());

                result = environments
                    .Value
                    .Where(x =>
                        string.Compare(x.Properties.DisplayName, displayName) == 0 ||
                        (x.Properties.LinkedEnvironmentMetadata != null &&
                         string.Compare(x.Properties.LinkedEnvironmentMetadata.FriendlyName, displayName) == 0)).FirstOrDefault();
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Creates a new environment.
        /// </summary>
        /// <param name="environment">Configuration information for the new environment.</param>
        /// <returns>Information regarding the new environment.</returns>
        public async Task<CreatePowerAppsEnvironmentResponse> CreateEnvironmentAsync(CreatePowerAppsEnvironmentRequest environment)
        {
            LogInformation($"Looking for existing PowerApps environment named '{environment.Properties.DisplayName}'...");
            PowerAppsEnvironment findEnvironment = await GetEnvironmentByDisplayNameAsync(environment.Properties.DisplayName);

            if (findEnvironment != null)
            {
                LogInformation($"PowerApps environment named '{environment.Properties.DisplayName}' already exists.");

                return new CreatePowerAppsEnvironmentResponse()
                {
                    Id = findEnvironment.Id,
                    Name = findEnvironment.Name,
                    Type = findEnvironment.Type,
                    Location = findEnvironment.Location,
                    Properties = findEnvironment.Properties,
                };
            }

            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(Audience);

            HttpContent body = new StringContent(
                JsonConvert.SerializeObject(environment),
                Encoding.UTF8,
                "application/json");

            LogInformation($"Creating PowerApps environment named '{environment.Properties.DisplayName}' in location '{environment.Location}' with SKU '{environment.Properties.EnvironmentSku}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _newEnvironmentUri,
                body);

            CreatePowerAppsEnvironmentResponse result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<CreatePowerAppsEnvironmentResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Validate the ability to delete an environment.
        /// </summary>
        /// <param name="environmentName">The name of the environment to delete.</param>
        /// <returns>True if environment can be deleted, false if it cannot.</returns>
        public async Task<bool?> ValidateDeleteEnvironmentAsync(string environmentName)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(Audience);

            LogInformation($"Validating deletion of PowerApps environment with id '{environmentName}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _validateDeleteEnvironmentUri.Replace("{environmentName}", environmentName),
                null);

            bool? result = null;

            if (response.IsSuccessStatusCode)
            {
                result = bool.Parse(
                    JObject.Parse(await response.Content.ReadAsStringAsync())["canInitiateDelete"].Value<string>());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Deletes an environment.
        /// </summary>
        /// <param name="environmentName">The name of the environment to delete.</param>
        /// <returns>The response content as an AzureResponseBase content.</returns>
        public async Task<AzureResponseBase> DeleteEnvironmentAsync(string environmentName)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(Audience);

            LogInformation($"Deleting PowerApps environment with id '{environmentName}'...");
            HttpResponseMessage response = await httpClient.DeleteAsync(
                _deleteEnvironmentUri
                    .Replace("{environmentName}", environmentName));

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
        /// Gets available CDS database currencies.
        /// </summary>
        /// <param name="location">The location to get CDS database currencies for.</param>
        /// <returns>Information regarding the available CDS database currencies.</returns>
        public async Task<GetPowerAppsCurrenciesResponse> GetCdsDatabaseCurrenciesAsync(string location)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(Audience);

            LogInformation($"Acquiring PowerApps Common Data Services database currencies for location '{location}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _getCdsDatabaseCurrenciesUri
                    .Replace("{location}", location));

            GetPowerAppsCurrenciesResponse result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<GetPowerAppsCurrenciesResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets the available CDS database languages.
        /// </summary>
        /// <param name="location">The location to get CDS database languages for.</param>
        /// <returns>Information regarding the available CDS database languages.</returns>
        public async Task<GetPowerAppsLanguagesResponse> GetCdsDatabaseLanguagesAsync(string location)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(Audience);

            LogInformation($"Acquiring PowerApps Common Data Services database currencies for location '{location}'...");
            HttpResponseMessage response = await httpClient.GetAsync(
                _getCdsDatabaseLanguagesUri
                    .Replace("{location}", location));

            GetPowerAppsLanguagesResponse result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<GetPowerAppsLanguagesResponse>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Create a CDS database.
        /// </summary>
        /// <param name="environmentName">The name of the environment to create the CDS database in.</param>
        /// <param name="cdsDatabase">Configuration information regarding the new CDS database.</param>
        /// <returns>Information regarding the PowerApps environment after the CDS database creation.</returns>
        public PowerAppsEnvironment CreateCdsDatabase(string environmentName, CreatePowerAppsCdsDatabaseRequest cdsDatabase)
        {
            LogInformation($"Looking for existing Common Data Services database for PowerApps environment with id '{environmentName}'...");
            PowerAppsEnvironment findEnvironment = (PowerAppsEnvironment)GetEnvironmentsAsync(environmentName).Result;

            if (findEnvironment != null &&
                findEnvironment.Properties != null &&
                findEnvironment.Properties.LinkedEnvironmentMetadata != null)
            {
                LogInformation($"Common Data Services database for PowerApps environment with id '{environmentName}' already exists.");
                return findEnvironment;
            }

            LogInformation($"Creating Common Data Services database for PowerApps environment '{environmentName}'...");
            AzureResponseBase response = CreateCdsDatabaseAsync(
                environmentName,
                cdsDatabase).Result;

            DateTime startTime = DateTime.Now;
            DateTime currentTime = DateTime.Now;
            TimeSpan timeDiff = currentTime - startTime;

            int timeoutInSeconds = 300;

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

                LogInformation("Polling for Common Data Service database creation completion...");
                httpResponse = GetLocationAsync(Audience, new Uri(response.LocationUri)).Result;
                currentTime = DateTime.Now;
                timeDiff = currentTime - startTime;
            }

            LogInformation("Common Data Service database creation completed.");
            return JsonConvert.DeserializeObject<PowerAppsEnvironment>(httpResponse.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Performs a request against a URI returned in a previous response's Location Header.
        /// </summary>
        /// <param name="audience">The audience to acquire an access token for.</param>
        /// <param name="location">The URI to make the request against.</param>
        /// <returns>The response message.</returns>
        public async Task<HttpResponseMessage> GetLocationAsync(string audience, Uri location)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(audience);

            LogInformation("Querying URI returned in 'Location' header...");
            return await httpClient.GetAsync(location);
        }

        /// <summary>
        /// Creates a CDS database.
        /// </summary>
        /// <param name="environmentName">The name of the environment to create the CDS database in.</param>
        /// <param name="cdsDatabase">Configuration information regarding the CDS database to create.</param>
        /// <returns>The response content as an AzureResponseBase object.</returns>
        public async Task<AzureResponseBase> CreateCdsDatabaseAsync(string environmentName, CreatePowerAppsCdsDatabaseRequest cdsDatabase)
        {
            LogInformation($"Looking for existing Common Data Services database for PowerApps environment with id '{environmentName}'...");
            PowerAppsEnvironment findEnvironment = (PowerAppsEnvironment)await GetEnvironmentsAsync(environmentName);

            if (findEnvironment != null &&
                findEnvironment.Properties != null &&
                findEnvironment.Properties.LinkedEnvironmentMetadata != null)
            {
                LogInformation($"Common Data Services database for PowerApps environment with id '{environmentName}' already exists.");
                return new AzureResponseBase()
                {
                    AlreadyExists = true,
                };
            }

            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(Audience);

            HttpContent body = new StringContent(
                JsonConvert.SerializeObject(cdsDatabase),
                Encoding.UTF8,
                "application/json");

            LogInformation($"Creating Common Data Services database for PowerApps environment '{environmentName}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                _newCdsDatabaseUri.Replace("{environmentName}", environmentName),
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
    }
}
