// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Helper class for creating a summary of resources to deploy as part of a resource deployment.
    /// </summary>
    public class ArmSummarizer
    {
        private const string _parametersKey = "parameters(";

        private JObject _armTemplateObj = null;
        private JObject _armTemplateParametersObj = null;

        private string _armTemplateParametersPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArmSummarizer"/> class.
        /// </summary>
        /// <param name="armTemplatePath">Path to the ARM template file.</param>
        /// <param name="armTemplateParametersPath">Path to the ARM template parameters file.</param>
        public ArmSummarizer(string armTemplatePath, string armTemplateParametersPath)
        {
            _armTemplateObj = JsonConvert.DeserializeObject<JObject>(
                File.ReadAllText(armTemplatePath));

            _armTemplateParametersPath = armTemplateParametersPath;
            _armTemplateParametersObj = JsonConvert.DeserializeObject<JObject>(
                File.ReadAllText(armTemplateParametersPath));
        }

        /// <summary>
        /// Builds a summary of the Azure resources to deploy.
        /// </summary>
        /// <returns>A list of resources with each item in the format of {ResourceType} ({Name}).</returns>
        public List<string> GetResourceSummary()
        {
            List<string> resources = new List<string>();

            foreach (JObject resource in _armTemplateObj["resources"])
            {
                string type = resource["type"].Value<string>();

                switch (type.ToLowerInvariant())
                {
                    case "microsoft.cognitiveservices/accounts":
                        string csKind = resource["kind"].Value<string>();

                        switch (csKind.ToLowerInvariant())
                        {
                            case "speechservices":
                                resources.Add($"Cognitive Services - Speech ({GetNameParameter(resource["name"].Value<string>())})");
                                break;

                            case "luis":
                                resources.Add($"Cognitive Services - Language Understanding ({GetNameParameter(resource["name"].Value<string>())})");
                                break;
                        }

                        break;

                    case "microsoft.insights/components":
                        resources.Add($"Application Insights ({GetNameParameter(resource["name"].Value<string>())})");
                        break;

                    case "microsoft.keyvault/vaults":
                        resources.Add($"Azure Key Vault ({GetNameParameter(resource["name"].Value<string>())})");
                        break;

                    case "microsoft.operationalinsights/workspaces":
                        resources.Add($"Operational Insights Workspace ({GetNameParameter(resource["name"].Value<string>())})");
                        break;

                    case "microsoft.servicebus/namespaces":
                        resources.Add($"Service Bus ({GetNameParameter(resource["name"].Value<string>())})");
                        break;

                    case "microsoft.storage/storageaccounts":
                        resources.Add($"Storage Account ({GetNameParameter(resource["name"].Value<string>())})");
                        break;

                    case "microsoft.web/connections":
                        resources.Add($"Logic App API Connection ({GetNameParameter(resource["name"].Value<string>())})");
                        break;

                    case "microsoft.web/serverfarms":
                        resources.Add($"App Service Plan ({GetNameParameter(resource["name"].Value<string>())})");
                        break;

                    case "microsoft.operationsmanagement/solutions":
                        resources.Add($"Operations Management Solution ({GetNameParameter(resource["name"].Value<string>())})");
                        break;

                    case "microsoft.web/sites":
                        string siteKind = resource["kind"].Value<string>();

                        switch (siteKind.ToLowerInvariant())
                        {
                            case "functionapp":
                                resources.Add($"Function App ({GetNameParameter(resource["name"].Value<string>())})");
                                break;
                        }

                        break;

                    case "microsoft.logic/workflows":
                        resources.Add($"Logic App ({GetNameParameter(resource["name"].Value<string>())})");
                        break;
                }
            }

            return resources;
        }

        /// <summary>
        /// Resolve an ARM expression to a name.
        /// </summary>
        /// <param name="nameExpression">The expression to resolve.</param>
        /// <returns>The resolved expression.</returns>
        private string GetNameParameter(string nameExpression)
        {
            if (nameExpression == null)
            {
                return null;
            }

            int parametersIndex = nameExpression.IndexOf(_parametersKey);

            if (parametersIndex == -1)
            {
                return nameExpression;
            }

            parametersIndex += _parametersKey.Length + 1;

            int endParametersIndex = nameExpression.IndexOf("'", parametersIndex);

            if (endParametersIndex == -1)
            {
                return nameExpression;
            }

            string parameterName = nameExpression.Substring(parametersIndex, endParametersIndex - parametersIndex);

            return ArmTemplateHelper.GetFormattedParameterValue(_armTemplateParametersObj["parameters"][parameterName]["value"].Value<string>());
        }
    }
}
