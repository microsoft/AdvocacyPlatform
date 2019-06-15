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
    /// Model representing a generic value collection response from the Azure Management APIs.
    /// </summary>
    /// <typeparam name="T">The type of model to deserialize the response as.</typeparam>
    public class AzureValueCollectionResponse<T>
    {
        /// <summary>
        /// Gets or sets the URI to acquire the next batch of resource groups.
        /// </summary>
        public string NextLink { get; set; }

        /// <summary>
        /// Gets or sets a list of resources returned by the request.
        /// </summary>
        public T[] Value { get; set; }
    }
}
