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
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Helper class for unit tests.
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Creates an HTTP request.
        /// </summary>
        /// <param name="method">The request method.</param>
        /// <param name="requestUri">The URI to make the request against.</param>
        /// <returns>The HTTP request.</returns>
        public static HttpRequestMessage CreateHttpRequest(HttpMethod method, string requestUri)
        {
            return new HttpRequestMessage(method, requestUri);
        }

        /// <summary>
        /// Create an HTTP response.
        /// </summary>
        /// <param name="responseCode">The response code.</param>
        /// <param name="headers">Headers to add to the response.</param>
        /// <param name="contentTemplateFilePath">Path to a file to use as the response content.</param>
        /// <param name="contentType">The response content type.</param>
        /// <param name="templateParameters">Parameters to replace in the response content.</param>
        /// <param name="templateArrayParameters">Array parameters to replace in the response content.</param>
        /// <param name="templateDictionaryParameters">Dictionary parameters to replace in the response content.</param>
        /// <returns>The HTTP response.</returns>
        public static HttpResponseMessage CreateHttpResponse(
            HttpStatusCode responseCode,
            Dictionary<string, string> headers,
            string contentTemplateFilePath,
            string contentType,
            Dictionary<string, string> templateParameters,
            Dictionary<string, string[]> templateArrayParameters = null,
            Dictionary<string, Dictionary<string, string>> templateDictionaryParameters = null)
        {
            HttpResponseMessage response = new HttpResponseMessage(responseCode);

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    response.Headers.Add(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(contentTemplateFilePath))
            {
                StringBuilder contentTemplateContents = new StringBuilder(
                    File.ReadAllText(contentTemplateFilePath));

                if (templateParameters != null)
                {
                    foreach (KeyValuePair<string, string> parameter in templateParameters)
                    {
                        contentTemplateContents = contentTemplateContents.Replace($"{{{parameter.Key}}}", parameter.Value);
                    }
                }

                if (templateArrayParameters != null)
                {
                    foreach (KeyValuePair<string, string[]> parameter in templateArrayParameters)
                    {
                        StringBuilder stringArray = new StringBuilder();

                        foreach (string val in parameter.Value)
                        {
                            stringArray = stringArray.Append($"\"{val}\",");
                        }

                        if (stringArray.Length > 0)
                        {
                            stringArray = stringArray.Remove(stringArray.Length - 1, 1);
                        }

                        contentTemplateContents = contentTemplateContents.Replace($"\"{{{parameter.Key}}}\"", stringArray.ToString());
                    }
                }

                if (templateDictionaryParameters != null)
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> parameter in templateDictionaryParameters)
                    {
                        StringBuilder stringArray = new StringBuilder();

                        foreach (KeyValuePair<string, string> parameterValue in parameter.Value)
                        {
                            stringArray = stringArray.Append($"\"{parameterValue.Key}\": \"{parameterValue.Value}\",");
                        }

                        if (stringArray.Length > 0)
                        {
                            stringArray = stringArray.Remove(stringArray.Length - 1, 1);
                        }

                        contentTemplateContents = contentTemplateContents.Replace($"\"{{{parameter.Key}}}\"", stringArray.ToString());
                    }
                }

                HttpContent content = new StringContent(
                    contentTemplateContents.ToString(),
                    Encoding.UTF8,
                    contentType);

                response.Content = content;
            }

            return response;
        }

        /// <summary>
        /// Creates an HTTP response.
        /// </summary>
        /// <param name="responseCode">The response code.</param>
        /// <param name="headers">Headers to add to the response.</param>
        /// <param name="content">The response content.</param>
        /// <param name="contentType">The response content type.</param>
        /// <returns>An HTTP response.</returns>
        public static HttpResponseMessage CreateHttpResponseSimpleContent(
            HttpStatusCode responseCode,
            Dictionary<string, string> headers,
            string content,
            string contentType)
        {
            HttpResponseMessage response = new HttpResponseMessage(responseCode);

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    response.Headers.Add(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                HttpContent responseContent = new StringContent(
                    content,
                    Encoding.UTF8,
                    contentType);

                response.Content = responseContent;
            }

            return response;
        }

        /// <summary>
        /// Verifies an array of strings between a source and target.
        /// </summary>
        /// <param name="source">Source array to compare from.</param>
        /// <param name="target">Target array to compare against.</param>
        public static void VerifyStringArrayContents(string[] source, string[] target)
        {
            HashSet<string> targetLookup = new HashSet<string>(target);

            foreach (string item in source)
            {
                Assert.IsTrue(targetLookup.Contains(item));
            }
        }

        /// <summary>
        /// Verifies dictionary contents between a source and target.
        /// </summary>
        /// <param name="source">Source dictionary to compare from.</param>
        /// <param name="target">Target dictionary to compare against.</param>
        public static void VerifyDictionaryContents(Dictionary<string, string> source, Dictionary<string, string> target)
        {
            foreach (KeyValuePair<string, string> item in source)
            {
                Assert.IsTrue(target.ContainsKey(item.Key), $"Key not found ('{item.Key}')!");
                Assert.AreEqual(item.Value, target[item.Key], $"Unexpected value for key '{item.Key}' ('{item.Value}' != '{target[item.Key]}')");
            }
        }

        /// <summary>
        /// Builds content as a JSON dictionary.
        /// </summary>
        /// <param name="values">Dictionary to build from.</param>
        /// <returns>The JSON dictionary as a string.</returns>
        public static string BuildJsonDictionaryContents(Dictionary<string, string> values)
        {
            StringBuilder result = new StringBuilder();

            foreach (KeyValuePair<string, string> value in values)
            {
                result = result.Append($"\"{value.Key}\": \"{value.Value}\",");
            }

            return result
                .Remove(result.Length - 1, 1)
                .ToString();
        }
    }
}
