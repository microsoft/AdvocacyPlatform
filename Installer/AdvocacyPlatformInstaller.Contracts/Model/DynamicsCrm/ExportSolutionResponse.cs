// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Model representing the response from a request to export an unmanaged solution.
    /// </summary>
    public class ExportSolutionResponse
    {
        /// <summary>
        /// Gets or sets a byte array representing the exported solution archive.
        /// </summary>
        public byte[] ExportSolutionFile { get; set; }
    }
}
