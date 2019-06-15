// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Helper class for managing test templates.
    /// </summary>
    public static class TemplateManager
    {
        private static Dictionary<string, string> _templateCache = new Dictionary<string, string>();

        /// <summary>
        /// Loads a template from file.
        /// </summary>
        /// <param name="templatePath">Path to the template file.</param>
        /// <param name="parameters">Placeholders and values to replace in the template.</param>
        /// <returns>The resolved template content.</returns>
        public static string Load(string templatePath, Dictionary<string, string> parameters)
        {
            StringBuilder templateContent = null;

            if (_templateCache.ContainsKey(templatePath))
            {
                templateContent = new StringBuilder(_templateCache[templatePath]);
            }
            else
            {
                templateContent = new StringBuilder(File.ReadAllText(templatePath));
            }

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                templateContent = templateContent.Replace($"{{{parameter.Key}}}", parameter.Value);
            }

            return templateContent.ToString();
        }

        /// <summary>
        /// Loads a template from file with composite items.
        /// </summary>
        /// <param name="templatePath">Path to the template file.</param>
        /// <param name="compositeKey">Placeholder in template for composite content.</param>
        /// <param name="compositeTemplatePath">Path to the composite item template file.</param>
        /// <param name="parameters">Placeholders and values to replace in the template.</param>
        /// <param name="compositeParametersList">Placeholders and values to replace in the composite item template.</param>
        /// <returns>The resolved template content.</returns>
        public static string LoadWithComposites(string templatePath, string compositeKey, string compositeTemplatePath, Dictionary<string, string> parameters, List<Dictionary<string, string>> compositeParametersList)
        {
            StringBuilder templateContent = null;
            StringBuilder fullCompositeContent = new StringBuilder();
            StringBuilder compositeTemplateContent = null;

            templateContent = new StringBuilder(Load(templatePath, parameters));

            foreach (Dictionary<string, string> compositeParameters in compositeParametersList)
            {
                if (_templateCache.ContainsKey(compositeTemplatePath))
                {
                    compositeTemplateContent = new StringBuilder(_templateCache[compositeTemplatePath]);
                }
                else
                {
                    compositeTemplateContent = new StringBuilder(File.ReadAllText(compositeTemplatePath));
                }

                foreach (KeyValuePair<string, string> compositeParameter in compositeParameters)
                {
                    compositeTemplateContent = compositeTemplateContent.Replace($"{{{compositeParameter.Key}}}", compositeParameter.Value);
                }

                fullCompositeContent.Append($"{compositeTemplateContent.ToString()},");
            }

            return templateContent
                .Replace($"{{{compositeKey}}}", fullCompositeContent.Remove(fullCompositeContent.Length - 1, 1).ToString())
                .ToString();
        }
    }
}
