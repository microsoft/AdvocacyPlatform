// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Exception thrown when URI is missing a required query parameter.
    /// </summary>
    public class MissingQueryParamException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingQueryParamException"/> class.
        /// </summary>
        /// <param name="queryParamName">The name of the missing query parameter.</param>
        public MissingQueryParamException(string queryParamName)
            : base($"You must specify the query parameter '{queryParamName}'.")
        {
            this.QueryParamName = queryParamName;
        }

        /// <summary>
        /// Gets or sets the name of the missing query parameter.
        /// </summary>
        public string QueryParamName { get; set; }
    }
}
