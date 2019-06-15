// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Helper class for common functionality.
    /// </summary>
    public static class FunctionHelper
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
        /// Gets configuration information.
        /// </summary>
        /// <param name="context">The execution context instance.</param>
        /// <returns>Configuration information.</returns>
        public static IConfigurationRoot GetConfiguration(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile(GlobalConstants.LocalJsonSettingsFileName, optional: true, reloadOnChange: false)
                    .AddJsonFile(GlobalConstants.JsonAppSettingsFileName, optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();
        }

        /// <summary>
        /// Gets the name of an enum value.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The value to get the name for.</param>
        /// <returns>The enum value name.</returns>
        public static string GetEnumName<T>(T value)
        {
            return Enum.GetName(typeof(T), value);
        }

        /// <summary>
        /// Factory for setting values common across all responses.
        /// </summary>
        /// <typeparam name="T">The type of response to create.</typeparam>
        /// <param name="isBadRequest">Flag indicating whether the request was invalid.</param>
        /// <param name="responseContent">Content to set in the response object.</param>
        /// <returns>The generated response object.</returns>
        public static IActionResult ActionResultFactory<T>(bool isBadRequest, T responseContent)
            where T : FunctionResponseBase
        {
            if (responseContent.HasError)
            {
                responseContent.StatusCode = (int)CommonStatusCode.Error;
                responseContent.StatusDesc = Enum.GetName(typeof(CommonStatusCode), CommonStatusCode.Error);
            }

            if (isBadRequest)
            {
                if (string.IsNullOrWhiteSpace(responseContent.ErrorDetails))
                {
                    responseContent.ErrorDetails = CommonErrorMessage.BadRequestMessage;
                }

                return new BadRequestObjectResult(responseContent);
            }

            return new OkObjectResult(responseContent);
        }
    }
}
