// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Data transformation for trimming at the end.
    /// </summary>
    public class TrimEndTransformation : IDataTransformation
    {
        /// <summary>
        /// Parameter key for specifying the maximum length before trimming.
        /// </summary>
        public const string MaxLengthKey = "MaxLength";

        /// <summary>
        /// Trims <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Text to trim.</param>
        /// <param name="parameters">Parameters to affect the behavior of the transformation.</param>
        /// <param name="log">A logger instance.</param>
        /// <returns>The trimmed text.</returns>
        public StringBuilder Transform(StringBuilder input, Dictionary<string, string> parameters, ILogger log)
        {
            int maxLength = parameters.ContainsKey(MaxLengthKey) ? int.Parse(parameters[MaxLengthKey]) : 500;

            log.LogInformation($"Trimming input from the end if length is greater than {maxLength}...");

            return input.Length > maxLength ? input.Remove(maxLength, input.Length - maxLength) : input;
        }
    }
}
