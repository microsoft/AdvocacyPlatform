// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// JSON CamelCase contract resolver that overrides processing dictionary keys as camelCase (keep PascalCase).
    /// </summary>
    public class JsonCamelCaseContractResolver : CamelCasePropertyNamesContractResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonCamelCaseContractResolver"/> class.
        /// </summary>
        public JsonCamelCaseContractResolver()
        {
            NamingStrategy.ProcessDictionaryKeys = false;
        }
    }
}
