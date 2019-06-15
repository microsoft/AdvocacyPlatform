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
    /// Model representing the state of an ongoing operation.
    /// </summary>
    public class OperationsProgress : NotifyPropertyChangedBase
    {
        private List<OperationStatus> _operations;
        private OperationStatus _currentOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationsProgress"/> class.
        /// </summary>
        public OperationsProgress()
        {
            Operations = new List<OperationStatus>();
        }

        /// <summary>
        /// Gets or sets a list of operations and their associated statuses.
        /// </summary>
        public List<OperationStatus> Operations
        {
            get => _operations;
            set
            {
                _operations = value;

                NotifyPropertyChanged("Operations");
            }
        }

        /// <summary>
        /// Gets or sets the operation currently in progress.
        /// </summary>
        public OperationStatus CurrentOperation
        {
            get => _currentOperation;
            set
            {
                _currentOperation = value;

                NotifyPropertyChanged("CurrentOperation");
            }
        }
    }
}
