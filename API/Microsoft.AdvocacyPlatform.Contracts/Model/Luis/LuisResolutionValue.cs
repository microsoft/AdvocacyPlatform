// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents a value in a LUIS resolution.
    /// </summary>
    public class LuisResolutionValue
    {
        /// <summary>
        /// Gets or sets the timex datetime.
        /// </summary>
        public string Timex { get; set; }

        /// <summary>
        /// Gets or sets the type of identified value.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the identified value.
        /// </summary>
        public string Value { get; set; }
    }
}
