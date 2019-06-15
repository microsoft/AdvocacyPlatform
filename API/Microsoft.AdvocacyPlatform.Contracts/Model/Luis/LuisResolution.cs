// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents a LUIS resolution.
    /// </summary>
    public class LuisResolution
    {
        /// <summary>
        /// Gets or sets the collection of resolved values.
        /// </summary>
        public ICollection<LuisResolutionValue> Values { get; set; }
    }
}
