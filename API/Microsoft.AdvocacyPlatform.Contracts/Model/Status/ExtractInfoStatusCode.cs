// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Status codes specific to ExtractInfo
    ///
    /// Function-specific status codes should be positive integers to avoid confusion
    /// with common status codes.
    /// </summary>
    public enum ExtractInfoStatusCode
    {
        /// <summary>
        /// Response was missing expected entities
        /// </summary>
        MissingEntities = 1,
    }
}
