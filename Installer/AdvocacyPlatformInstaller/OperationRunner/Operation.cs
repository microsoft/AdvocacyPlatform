// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an operation to run.
    /// </summary>
    public class Operation
    {
        private Guid _id = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="Operation"/> class.
        /// </summary>
        public Operation()
        {
            MaxRetries = 2;
        }

        /// <summary>
        /// Gets the unique identifier for this operation.
        /// </summary>
        public Guid Id => _id;

        /// <summary>
        /// Gets or sets the name of this operation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the delegate to run for this operation.
        /// </summary>
        public Func<OperationRunner, object> OperationFunction { get; set; }

        /// <summary>
        /// Gets or sets the delegate to run to validate the output of this operation.
        /// </summary>
        public Func<OperationRunner, bool> ValidateFunction { get; set; }

        /// <summary>
        /// Gets or sets the delegate to run when this operation completes.
        /// </summary>
        public Action<object> OperationCompletedHandler { get; set; }

        /// <summary>
        /// Gets or sets the delegate to run when an exception is encountered for this operation.
        /// </summary>
        public Action<Exception> ExceptionHandler { get; set; }

        /// <summary>
        /// Gets or sets the delegate to run when validation fails for this operation.
        /// </summary>
        public Func<bool> FailedValidationHandler { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the runner should retry this operation on failure.
        /// </summary>
        public bool RetryOperation { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times to retry this operation before failing.
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the current retry count for the current operation.
        /// </summary>
        public int CurrentRetryCount { get; set; }
    }
}
