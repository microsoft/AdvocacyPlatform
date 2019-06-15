// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.AdvocacyPlatform.Contracts;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Data transformation to remove punctuation from sentences.
    /// </summary>
    public class RemovePunctuationTransformation : IDataTransformation
    {
        /// <summary>
        /// Regular expression for sentence punctuation.
        /// </summary>
        public static readonly Regex PunctuationRegEx = new Regex(@"[!?\.;]", RegexOptions.Compiled);

        /// <summary>
        /// Transforms data to remove punctuation.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="parameters">Parameters to affect the behavior of the transformation.</param>
        /// <param name="log">A logger instance.</param>
        /// <returns>The transformed string.</returns>
        public StringBuilder Transform(StringBuilder input, Dictionary<string, string> parameters, ILogger log)
        {
            log.LogInformation("Removing punctuation from input...");

            return new StringBuilder(PunctuationRegEx.Replace(input.ToString(), string.Empty));
        }
    }
}
