// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents minimal file information.
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// Gets or sets the size of the file in bytes.
        /// </summary>
        public long SizeInBytes { get; set; }

        /// <summary>
        /// Gets or sets the URI to the file.
        /// </summary>
        public Uri Uri { get; set; }
    }
}
