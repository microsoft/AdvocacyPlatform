// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Person information extracted from a transcription.
    /// </summary>
    public class PersonInfo
    {
        /// <summary>
        /// Gets or sets the full name of the person.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of person (e.g. Judge).
        /// </summary>
        public string Type { get; set; }
    }
}
