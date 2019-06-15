// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents a LUIS response.
    /// </summary>
    public class LuisResponse
    {
        /// <summary>
        /// Gets or sets the input query.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets the top scoring intent.
        /// </summary>
        public LuisIntent TopScoringIntent { get; set; }

        /// <summary>
        /// Gets or sets the collection of identified entities.
        /// </summary>
        public ICollection<LuisEntity> Entities { get; set; }

        /// <summary>
        /// Gets or sets the collection of identified composite entities.
        /// </summary>
        public ICollection<LuisCompositeEntity> CompositeEntities { get; set; }
    }
}
