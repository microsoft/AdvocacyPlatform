// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents a LUSI intent.
    /// </summary>
    public class LuisIntent
    {
        /// <summary>
        /// Gets or sets the identified intent.
        /// </summary>
        public string Intent { get; set; }

        /// <summary>
        /// Gets or sets the confidence in the identified intent.
        /// </summary>
        public decimal Score { get; set; }
    }
}
