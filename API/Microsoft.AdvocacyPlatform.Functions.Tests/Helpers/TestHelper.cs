// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Security;
    using System.Text;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Helper class for unit tests.
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Gets configuration information.
        /// </summary>
        /// <returns>Configuration information.</returns>
        public static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                    .AddJsonFile(GlobalConstants.LocalJsonSettingsFileName, optional: true, reloadOnChange: false)
                    .AddJsonFile(GlobalConstants.JsonAppSettingsFileName, optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();
        }

        /// <summary>
        /// Creates a SecureString.
        /// </summary>
        /// <param name="value">The value to create the SecureString with.</param>
        /// <returns>The secured string.</returns>
        public static SecureString CreateSecureString(string value)
        {
            SecureString securedString = new SecureString();

            foreach (char character in value)
            {
                securedString.AppendChar(character);
            }

            securedString.MakeReadOnly();

            return securedString;
        }

        /// <summary>
        /// Creates an HTTP response message.
        /// </summary>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <param name="content">Content in the response body.</param>
        /// <returns>The HTTP response.</returns>
        public static HttpResponseMessage CreateHttpResponseMessage(HttpStatusCode statusCode, string content)
        {
            HttpResponseMessage response = new HttpResponseMessage(statusCode);

            response.Content = new StringContent(content);

            return response;
        }
    }
}
