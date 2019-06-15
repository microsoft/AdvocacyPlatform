// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Enumeration of operation status codes.
    /// </summary>
    public enum OperationStatusCode
    {
        /// <summary>
        /// Operation state is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Operation has not started yet.
        /// </summary>
        NotStarted = 1,

        /// <summary>
        /// Operation is currently running.
        /// </summary>
        InProgress = 2,

        /// <summary>
        /// Operation completed successfully.
        /// </summary>
        CompletedSuccessfully = 3,

        /// <summary>
        /// Operation failed.
        /// </summary>
        Failed = 4,
    }
}
