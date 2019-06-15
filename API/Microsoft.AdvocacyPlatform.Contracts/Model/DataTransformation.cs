// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Model representing a requested data transformation.
    /// </summary>
    public class DataTransformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataTransformation"/> class.
        /// </summary>
        public DataTransformation()
        {
            Parameters = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the name of the transformation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of parameters for the transformation function.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }
    }
}
