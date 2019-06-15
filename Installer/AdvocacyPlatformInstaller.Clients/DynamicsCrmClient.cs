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
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Serialization;
    using AdvocacyPlatformInstaller.Contracts;
    using Microsoft.Crm.Sdk;
    using Microsoft.Crm.Sdk.Messages;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;
    using Microsoft.Xrm.Tooling.Connector;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using Contract = AdvocacyPlatformInstaller.Contracts;
    using Sdk = Microsoft.Xrm.Sdk;

    /// <summary>
    /// Simple REST client for interacting with Dynamics 365 CRM APIs.
    /// </summary>
    public class DynamicsCrmClient : TokenBasedClient, IDynamicsCrmClient
    {
        private const string _audienceFormat = "https://{domainName}.crm.dynamics.com";
        private static readonly string _baseAddressFormat = "{audience}/api/data/v9.0/";

        private string _audience;
        private string _uniqueName;
        private string _domainName;
        private string _baseAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicsCrmClient"/> class.
        /// </summary>
        /// <param name="uniqueName">The unique name of the Dynamics 365 CRM instance.</param>
        /// <param name="domainName">The domain name of the Dynamics 365 CRM instance.</param>
        /// <param name="tokenProvider">The token provider to use for acquiring access tokens.</param>
        public DynamicsCrmClient(string uniqueName, string domainName, ITokenProvider tokenProvider)
                : base(tokenProvider)
        {
            _uniqueName = uniqueName;
            _domainName = domainName;
            _audience = _audienceFormat.Replace("{domainName}", _domainName);
            _baseAddress = _baseAddressFormat.Replace("{audience}", _audience);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new JsonCamelCaseContractResolver(),
            };
        }

        /// <summary>
        /// Gets the user id of the current user.
        /// </summary>
        /// <returns>The user id of the current user.</returns>
        public async Task<string> GetUserIdAsync()
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(_audience);

            LogInformation($"Acquiring user id from Dynamics CRM instance '{_uniqueName}'...");
            HttpResponseMessage response = await httpClient.GetAsync($"{_baseAddress}WhoAmI");

            if (response.IsSuccessStatusCode)
            {
                JObject body = JObject.Parse(await response.Content.ReadAsStringAsync());

                return body["UserId"].Value<string>();
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }
        }

        /// <summary>
        /// Gets available solutions.
        /// </summary>
        /// <returns>A list of available solutions.</returns>
        public async Task<DynamicsCrmValueResponse<DynamicsCrmSolution>> GetSolutionsAsync()
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(_audience);

            LogInformation($"Acquiring solutions from dynamics CRM instance '{_uniqueName}'...");
            HttpResponseMessage response = await httpClient.GetAsync($"{_baseAddress}solutions");

            DynamicsCrmValueResponse<DynamicsCrmSolution> result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<DynamicsCrmValueResponse<DynamicsCrmSolution>>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result;
        }

        /// <summary>
        /// Gets a solution.
        /// </summary>
        /// <param name="solutionUniqueName">The unique name of the solution.</param>
        /// <returns>The solution.</returns>
        public async Task<DynamicsCrmSolution> GetSolutionAsync(string solutionUniqueName)
        {
            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(_audience);

            LogInformation($"Acquiring solution named '{solutionUniqueName}' from Dynamics CRM instance '{_uniqueName}'...");
            HttpResponseMessage response = await httpClient.GetAsync($"{_baseAddress}solutions?$filter=uniquename eq '{solutionUniqueName}'");

            DynamicsCrmValueResponse<DynamicsCrmSolution> result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<DynamicsCrmValueResponse<DynamicsCrmSolution>>(await response.Content.ReadAsStringAsync());

                if (result == null || result.Value == null)
                {
                    return null;
                }
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return result.Value.FirstOrDefault();
        }

        /// <summary>
        /// Imports a solution.
        /// </summary>
        /// <param name="solutionFilePath">Path to the solution ZIP archive.</param>
        /// <param name="isHoldingSolution">Specifies if the solution is a holding solution.</param>
        /// <returns>The response content as a string.</returns>
        public async Task<string> ImportSolutionAsync(string solutionFilePath, bool isHoldingSolution = false)
        {
            Guid importJobId = Guid.NewGuid();

            string result = null;

            LogInformation($"Reading solution from file path '{solutionFilePath}'...");
            using (Stream fileReader = File.Open(solutionFilePath, FileMode.Open))
            {
                using (MemoryStream fileBytes = new MemoryStream())
                {
                    fileReader.CopyTo(fileBytes);

                    string base64CustomizationFile = Convert.ToBase64String(
                        fileBytes.ToArray());

                    string importSolution = $@"{{
                        ""OverwriteUnmanagedCustomizations"": ""true"",
                        ""PublishWorkflows"": ""true"",
                        ""CustomizationFile"": ""{base64CustomizationFile}"",
                        ""ImportJobId"": ""{importJobId.ToString()}"",
                        ""HoldingSolution"": ""{isHoldingSolution.ToString()}""
                    }}";

                    HttpContent body = new StringContent(
                        importSolution,
                        Encoding.UTF8,
                        "application/json");

                    LogInformation("Acquiring access token...");
                    IHttpClient httpClient = TokenProvider.GetHttpClient(_audience);

                    LogInformation($"Importing solution to Dynamics CRM instance '{_uniqueName}'...");
                    HttpResponseMessage response = await httpClient.PostAsync(
                        $"{_baseAddress}ImportSolution",
                        body);

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
                }
            }

            return result;
        }

        /// <summary>
        /// Imports entities.
        /// </summary>
        /// <param name="schemaXml">The name of the schema file.</param>
        /// <param name="dataXml">The name of the data file.</param>
        /// <param name="areFilePaths">Specifies if schemaXml and dataXml are file paths (true) or archive file names (false).</param>
        /// <returns>An asynchronous task.</returns>
        public async Task ImportEntitiesAsync(string schemaXml, string dataXml, bool areFilePaths = false)
        {
            LogInformation("Getting current user id to replace systemuser and owner fields...");
            string userId = await GetUserIdAsync();

            XmlSerializer xmlSer = new XmlSerializer(typeof(EntityMap));

            EntityMap map = null;
            EntityData data = null;

            if (areFilePaths)
            {
                LogInformation($"Loading schema file from '{schemaXml}'...");
                using (StreamReader reader = new StreamReader(schemaXml))
                {
                    XmlTextReader xmlTextReader = new XmlTextReader(reader);

                    xmlTextReader.DtdProcessing = DtdProcessing.Ignore;

                    map = (EntityMap)xmlSer.Deserialize(xmlTextReader);
                }

                xmlSer = new XmlSerializer(typeof(EntityData));

                LogInformation($"Loading data file from '{dataXml}'...");
                using (StreamReader reader = new StreamReader(dataXml))
                {
                    XmlTextReader xmlTextReader = new XmlTextReader(reader);

                    xmlTextReader.DtdProcessing = DtdProcessing.Ignore;

                    data = (EntityData)xmlSer.Deserialize(xmlTextReader);
                }
            }
            else
            {
                LogInformation($"Parsing schema xml...");
                using (TextReader reader = new StringReader(schemaXml))
                {
                    XmlTextReader xmlTextReader = new XmlTextReader(reader);

                    xmlTextReader.DtdProcessing = DtdProcessing.Ignore;

                    map = (EntityMap)xmlSer.Deserialize(xmlTextReader);
                }

                xmlSer = new XmlSerializer(typeof(EntityData));

                LogInformation($"Loading data xml...");
                using (TextReader reader = new StringReader(dataXml))
                {
                    XmlTextReader xmlTextReader = new XmlTextReader(reader);

                    xmlTextReader.DtdProcessing = DtdProcessing.Ignore;

                    data = (EntityData)xmlSer.Deserialize(xmlTextReader);
                }
            }

            ExecuteMultipleRequest execMulRequest = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = false,
                    ReturnResponses = true,
                },
                Requests = new OrganizationRequestCollection(),
            };

            foreach (Contract.Entity entity in data.Entities)
            {
                LogInformation($"Processing entity '{entity.Name}' records...");

                LogInformation($"Looking for mapping in schema...");
                EntityMapEntity entitySchema = map
                    .Entities
                    .Where(x => string.Compare(entity.Name, x.Name, true) == 0)
                    .First();

                foreach (EntityRecord entityRecord in entity.Records)
                {
                    LogInformation($"Processing record with id '{entityRecord.Id}'...");

                    Sdk.Entity crmEntity = new Sdk.Entity(entity.Name);

                    crmEntity.Id = Guid.Parse(entityRecord.Id);

                    foreach (EntityRecordField recordField in entityRecord.Fields)
                    {
                        EntityMapEntityField field = entitySchema
                            .Fields
                            .Where(x => string.Compare(recordField.Name, x.Name, true) == 0)
                            .First();

                        switch (field.Type.ToLowerInvariant())
                        {
                            case "guid":
                                crmEntity[recordField.Name] = Guid.Parse(recordField.Value);
                                break;

                            case "optionsetvalue":
                                crmEntity[recordField.Name] = new OptionSetValue(int.Parse(recordField.Value));
                                break;

                            case "bigint":
                                crmEntity[recordField.Name] = long.Parse(recordField.Value);
                                break;

                            case "owner":
                                // Replace the original owner with the current deploying user
                                crmEntity[recordField.Name] = new EntityReference("systemuser", Guid.Parse(userId));
                                break;

                            case "entityreference":
                                crmEntity[recordField.Name] = new EntityReference(
                                    recordField.LookupEntity,
                                    Guid.Parse(
                                        string.Compare(field.LookupType, "systemuser", true) == 0 ?
                                            userId :
                                                recordField.Value));
                                break;

                            case "datetime":
                                crmEntity[recordField.Name] = DateTime.Parse(recordField.Value);
                                break;

                            case "state":
                            case "status":
                            case "number":
                                crmEntity[recordField.Name] = int.Parse(recordField.Value);
                                break;

                            case "string":
                                crmEntity[recordField.Name] = recordField.Value;
                                break;
                        }
                    }

                    CreateRequest createRequest = new CreateRequest()
                    {
                        Target = crmEntity,
                    };

                    execMulRequest.Requests.Add(createRequest);
                }
            }

            LogInformation("Acquiring access token...");
            string accessToken = await TokenProvider.GetAccessTokenAsync(_audience);

            OAuthHookWrapper oauthHook = new OAuthHookWrapper();

            oauthHook.SetAccessToken(accessToken);

            CrmServiceClient.AuthOverrideHook = oauthHook;

            LogInformation($"Creating Dynamics 365 CRM service client for '{_audience}'...");
            using (CrmServiceClient client = new CrmServiceClient(new Uri(_audience), true))
            {
                if (!client.IsReady)
                {
                    client.Dispose();

                    LogError("Could not connect to Dynamics 365 CRM!");
                    throw new Exception("Could not connect to Dynamics 365 CRM!");
                }

                try
                {
                    IOrganizationService service = client.OrganizationWebProxyClient != null ? (IOrganizationService)client.OrganizationWebProxyClient : client.OrganizationServiceProxy;

                    LogInformation($"Ingesting {execMulRequest.Requests.Count} records...");
                    ExecuteMultipleResponse response = (ExecuteMultipleResponse)service.Execute(execMulRequest);

                    int i = 0;

                    foreach (ExecuteMultipleResponseItem responseItem in response.Responses)
                    {
                        if (responseItem != null)
                        {
                            LogInformation($"Created '{((Sdk.Entity)execMulRequest.Requests[i].Parameters["Target"]).LogicalName}' record with id '{responseItem.Response.Results["id"].ToString()}'");
                        }

                        i++;
                    }
                }
                catch
                {
                    client.Dispose();

                    throw;
                }
            }
        }

        /// <summary>
        /// Updates an existing solution.
        /// </summary>
        /// <param name="solutionUniqueName">The unique name of the solution.</param>
        /// <param name="solutionFilePath">Path to the solution ZIP archive.</param>
        /// <returns>Information regarding the updated solution.</returns>
        public async Task<DynamicsCrmSolution> UpdateSolutionAsync(string solutionUniqueName, string solutionFilePath)
        {
            DynamicsCrmSolution solution = await GetSolutionAsync(solutionUniqueName);

            if (solution == null)
            {
                throw new Exception("Solution does not exists!");
            }
            else
            {
                string importResult = await ImportSolutionAsync(solutionFilePath);

                return await GetSolutionAsync(solutionUniqueName);
            }
        }

        /// <summary>
        /// Deletes a solutoion.
        /// </summary>
        /// <param name="solutionUniqueName">The unique name of the solution.</param>
        /// <returns>Information regarding the deleted solution.</returns>
        public async Task<DynamicsCrmSolution> DeleteSolutionAsync(string solutionUniqueName)
        {
            DynamicsCrmSolution solution = await GetSolutionAsync(solutionUniqueName);

            if (solution == null)
            {
                return null;
            }

            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(_audience);

            LogInformation($"Deleting solution from Dynamics CRM instance with id '{solution.SolutionId}'...");
            HttpResponseMessage response = await httpClient.DeleteAsync(
                $"{_baseAddress}solutions({solution.SolutionId})");

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return solution;
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }
        }

        /// <summary>
        /// Export an unmanaged solution.
        /// </summary>
        /// <param name="solutionUniqueName">The unique name of the solution.</param>
        /// <param name="exportFilePath">Path to where to export the solution ZIP archive to.</param>
        /// <returns>The response content as a string.</returns>
        public async Task<string> ExportSolutionAsync(string solutionUniqueName, string exportFilePath)
        {
            string exportSolution = $@"{{
                ""SolutionName"": ""{solutionUniqueName}"",
                ""Managed"": ""true""
            }}";

            HttpContent body = new StringContent(
                exportSolution,
                Encoding.UTF8,
                "application/json");

            LogInformation("Acquiring access token...");
            IHttpClient httpClient = TokenProvider.GetHttpClient(_audience);

            LogInformation($"Exporting solution from Dynamics CRM instance '{_uniqueName}' with unique name '{solutionUniqueName}' to '{exportFilePath}'...");
            HttpResponseMessage response = await httpClient.PostAsync(
                $"{_baseAddress}ExportSolution",
                body);

            Contract.ExportSolutionResponse result = null;

            if (response.IsSuccessStatusCode)
            {
                result = JsonConvert.DeserializeObject<Contract.ExportSolutionResponse>(await response.Content.ReadAsStringAsync());

                if (result != null)
                {
                    File.WriteAllBytes(exportFilePath, result.ExportSolutionFile);
                }
            }
            else
            {
                LogError($"ERROR: ({response.StatusCode}) {response.ReasonPhrase}");
                throw new RequestException(response);
            }

            return exportFilePath;
        }
    }
}
