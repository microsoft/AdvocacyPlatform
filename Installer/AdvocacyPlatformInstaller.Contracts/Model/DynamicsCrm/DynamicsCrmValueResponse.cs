// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Model representing a generic Dynamics 365 CRM API response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response content as.</typeparam>
    public class DynamicsCrmValueResponse<T>
    {
        /// <summary>
        /// Gets or sets the context of the response.
        /// </summary>
        [JsonProperty("@odata.context")]
        public string Context { get; set; }

        /// <summary>
        /// Gets or sets a list of values returned.
        /// </summary>
        public T[] Value { get; set; }
    }
}
