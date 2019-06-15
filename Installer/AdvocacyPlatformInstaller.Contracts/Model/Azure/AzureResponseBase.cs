// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Model representing a generic response to a request to the Azure Management APIs.
    /// </summary>
    public class AzureResponseBase
    {
        /// <summary>
        /// Gets or sets the URI for obtaining status of asynchronous Azure tasks.
        /// </summary>
        public string AzureAsyncOperationUri { get; set; }

        /// <summary>
        /// Gets or sets the URI for obtaining status of asynchronous tasks.
        /// </summary>
        public string LocationUri { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait until polling for status again.
        /// </summary>
        public int RetryAfter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a resource already exists.
        /// </summary>
        public bool AlreadyExists { get; set; }
    }
}
