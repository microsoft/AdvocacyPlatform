// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Helper class for dealing with Azure ARM template files.
    /// </summary>
    public static class ArmTemplateHelper
    {
        private static Regex _parameterRegex = new Regex("\\{(.)*\\}", RegexOptions.Compiled);
        private static ConcurrentDictionary<string, JObject> _armTemplateParametersDictionary = new ConcurrentDictionary<string, JObject>();

        /// <summary>
        /// Loads parameters from an ARM template parameters file.
        /// </summary>
        /// <param name="filePath">Path to the ARM template parameters file.</param>
        public static void LoadArmTemplateParameters(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            JObject armTemplateParameters = JsonConvert.DeserializeObject<JObject>(
                File.ReadAllText(filePath));

            _armTemplateParametersDictionary.TryAdd(filePath, armTemplateParameters);
        }

        /// <summary>
        /// Saves an ARM template parameters file.
        /// </summary>
        /// <param name="armFilePath">Path to the original ARM template parameters file.</param>
        /// <param name="outputFilePath">Output path for the new ARM template parameters file.</param>
        public static void SaveConfiguration(string armFilePath, string outputFilePath)
        {
            JObject armTemplateParameters = _armTemplateParametersDictionary[armFilePath];

            string installationConfigurationContent = JsonConvert.SerializeObject(armTemplateParameters);

            File.WriteAllText(outputFilePath, installationConfigurationContent, Encoding.UTF8);
        }

        /// <summary>
        /// Gets a list of parameter names from an ARM template parameters file.
        /// </summary>
        /// <param name="filePath">Path to the ARM template parameters file.</param>
        /// <returns>A list of parameter names from the file.</returns>
        public static IEnumerable<string> GetParameterNames(string filePath)
        {
            List<string> parameterList = new List<string>();

            JToken parameters = _armTemplateParametersDictionary[filePath]["parameters"];

            foreach (JProperty parameter in parameters.Children<JProperty>())
            {
                parameterList.Add(parameter.Name);
            }

            return parameterList;
        }

        /// <summary>
        /// Gets the parameter value of a parameter from an ARM template parameters file.
        /// </summary>
        /// <param name="filePath">Path to the ARM template parameters file.</param>
        /// <param name="parameterName">The name of the parameter to get.</param>
        /// <returns>The value of the parameter or null if it does not exist.</returns>
        public static string GetParameterValue(string filePath, string parameterName)
        {
            JToken parameters = _armTemplateParametersDictionary[filePath]["parameters"];
            JProperty parameter = parameters.Children<JProperty>().Where(x => x.Name == parameterName).FirstOrDefault();

            if (parameter == null)
            {
                return null;
            }

            JToken parameterValue = parameter.Value["value"];

            if (parameterValue == null)
            {
                return null;
            }

            return GetFormattedParameterValue(parameterValue.Value<string>());
        }

        /// <summary>
        /// Gets a formatted parameter value.
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns>The formatted parameter value.</returns>
        public static string GetFormattedParameterValue(string parameterValue)
        {
            StringBuilder stringValueBuilder = new StringBuilder(parameterValue);

            MatchCollection matches = _parameterRegex.Matches(stringValueBuilder.ToString());

            foreach (Match match in matches)
            {
                string matchedParameterValue = match.Value;
                int parenthesisIndex = matchedParameterValue.IndexOf("(");
                int endParenthesisIndex = matchedParameterValue.IndexOf(")");

                if (parenthesisIndex == -1 ||
                    endParenthesisIndex == -1)
                {
                    continue;
                }

                string function = matchedParameterValue.Substring(1, parenthesisIndex - 1);
                string functionParameters = null;

                if ((parenthesisIndex + 1) - endParenthesisIndex != 0)
                {
                    functionParameters = matchedParameterValue.Substring(parenthesisIndex + 1, endParenthesisIndex - (parenthesisIndex + 1));
                }

                string[] functionParameterArray = null;

                if (!string.IsNullOrWhiteSpace(functionParameters))
                {
                    functionParameterArray = functionParameters.Split(new char[] { ',' });
                }

                switch (function.ToLowerInvariant())
                {
                    case "newid":
                        if (functionParameterArray != null &&
                            functionParameterArray.Count() == 1)
                        {
                            stringValueBuilder = stringValueBuilder.Replace(match.Value, Helpers.NewId(functionParameterArray[0]));
                        }
                        else
                        {
                            stringValueBuilder = stringValueBuilder.Replace(match.Value, Helpers.NewId());
                        }

                        break;
                }
            }

            return stringValueBuilder.ToString();
        }

        /// <summary>
        /// Sets the value of a parameter.
        /// </summary>
        /// <param name="filePath">Path to the ARM template parameters file.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The value to set the parameter to.</param>
        public static void SetParameterValue(string filePath, string parameterName, string value)
        {
            if (!_armTemplateParametersDictionary.ContainsKey(filePath))
            {
                return;
            }

            JToken parameters = _armTemplateParametersDictionary[filePath]["parameters"];
            JProperty parameter = parameters.Children<JProperty>().Where(x => x.Name == parameterName).FirstOrDefault();

            if (parameter == null)
            {
                return;
            }

            JToken parameterValue = parameter.Value["value"];

            if (parameterValue == null)
            {
                return;
            }

            parameterValue.Replace(JToken.Parse($"\"{value}\""));
        }
    }
}
