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
    /// Event fired when an operation completes.
    /// </summary>
    public class ProcessCompletedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="operationName">The name of the operation.</param>
        public ProcessCompletedEventArgs(string operationName)
        {
            OperationName = operationName;
        }

        /// <summary>
        /// Gets or sets the name of the operation.
        /// </summary>
        public string OperationName { get; set; }
    }
}
