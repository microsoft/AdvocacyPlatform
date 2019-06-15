// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents a LUIS entity.
    /// </summary>
    public class LuisEntity
    {
        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the type of entity.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value identified.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the start index of where the entity begins in the text.
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the end index of where the entity ends in the text.
        /// </summary>
        public int EndIndex { get; set; }

        /// <summary>
        /// Gets or sets the resolved values.
        /// </summary>
        public LuisResolution Resolution { get; set; }

        /// <summary>
        /// Gets or sets the confidence score of the identified entity.
        /// </summary>
        public decimal Score { get; set; }
    }
}
