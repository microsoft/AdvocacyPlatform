// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents a LUIS composite entity.
    /// </summary>
    public class LuisCompositeEntity
    {
        /// <summary>
        /// Gets or sets the name of the parent entity.
        /// </summary>
        public string ParentType { get; set; }

        /// <summary>
        /// Gets or sets the value identified.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the child entities of this composite entity.
        /// </summary>
        public ICollection<LuisEntity> Children { get; set; }
    }
}
