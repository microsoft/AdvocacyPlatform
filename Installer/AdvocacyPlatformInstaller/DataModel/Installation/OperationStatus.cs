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
    /// Model representing the status of an operation.
    /// </summary>
    public class OperationStatus : NotifyPropertyChangedBase
    {
        private static readonly Dictionary<OperationStatusCode, string> _statusText = new Dictionary<OperationStatusCode, string>()
        {
            { OperationStatusCode.Unknown, "Unknown" },
            { OperationStatusCode.NotStarted, "Not Started" },
            { OperationStatusCode.InProgress, "In Progress" },
            { OperationStatusCode.CompletedSuccessfully, "Completed" },
            { OperationStatusCode.Failed, "Failed" },
        };

        private Guid _id;
        private string _name;
        private OperationStatusCode _statusCode;
        private bool _inProgress;

        /// <summary>
        /// Gets or sets the unique identifier of the operation.
        /// </summary>
        public Guid Id
        {
            get => _id;
            set
            {
                _id = value;

                NotifyPropertyChanged("Id");
            }
        }

        /// <summary>
        /// Gets or sets the name of the operation.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;

                NotifyPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets the status of the operation.
        /// </summary>
        public OperationStatusCode StatusCode
        {
            get => _statusCode;
            set
            {
                _statusCode = value;

                if (_statusCode == OperationStatusCode.InProgress)
                {
                    InProgress = true;
                }
                else
                {
                    InProgress = false;
                }

                NotifyPropertyChanged("StatusCode");
                NotifyPropertyChanged("Status");
            }
        }

        /// <summary>
        /// Gets the friendly text of a status code.
        /// </summary>
        public string Status
        {
            get => _statusText[StatusCode];
        }

        /// <summary>
        /// Gets or sets a value indicating whether the operation is in progress.
        /// </summary>
        public bool InProgress
        {
            get => _inProgress;
            set
            {
                _inProgress = value;

                NotifyPropertyChanged("InProgress");
            }
        }
    }
}
