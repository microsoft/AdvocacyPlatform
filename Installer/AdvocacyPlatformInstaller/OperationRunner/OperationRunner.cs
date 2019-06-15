// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Threading;

    /// <summary>
    /// A simple process manager.
    /// </summary>
    public class OperationRunner : NotifyPropertyChangedBase
    {
        private ConcurrentQueue<Operation> _operations = new ConcurrentQueue<Operation>();
        private OperationRunnerLogger _logger;
        private WizardPageBase _owner;

        private Operation _currentOperation = null;

        private int _operationsCount = 0;
        private int _currentOperationIndex = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationRunner"/> class.
        /// </summary>
        /// <param name="progressTracker">The progress tracking instance to use to track progress.</param>
        /// <param name="owner">The owner of this runner instance.</param>
        /// <param name="logFileStream">The logging stream to write logs to.</param>
        public OperationRunner(
            OperationsProgress progressTracker,
            WizardPageBase owner,
            StreamWriter logFileStream = null)
        {
            _owner = owner;

            ProgressTracker = progressTracker;

            _logger = new OperationRunnerLogger(this, logFileStream);
        }

        /// <summary>
        /// Event fired when all operations complete.
        /// </summary>
        public event EventHandler OnComplete;

        /// <summary>
        /// Event fired when logging occurs.
        /// </summary>
        public event EventHandler<LogEventArgs> OnLog;

        /// <summary>
        /// Gets the queue of operations to run.
        /// </summary>
        public ConcurrentQueue<Operation> Operations
        {
            get => _operations;
        }

        /// <summary>
        /// Gets or sets the count of operations left to run.
        /// </summary>
        public int OperationsCount
        {
            get => _operationsCount;
            set
            {
                _operationsCount = value;

                NotifyPropertyChanged("OperationsCount");
            }
        }

        /// <summary>
        /// Gets or sets the index of the currently executing operation.
        /// </summary>
        public int CurrentOperationIndex
        {
            get => _currentOperationIndex;
            set
            {
                _currentOperationIndex = value;

                NotifyPropertyChanged("CurrentOperationIndex");
            }
        }

        /// <summary>
        /// Gets the current logger instance.
        /// </summary>
        public OperationRunnerLogger Logger => _logger;

        /// <summary>
        /// Gets or sets the progress tracking instance.
        /// </summary>
        public OperationsProgress ProgressTracker { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operations should be tracked with the indeterminate progress bar.
        /// </summary>
        public bool IndeterminateOps { get; set; } = false;

        /// <summary>
        /// Gets or sets the status code for the last executed operation.
        /// </summary>
        public int LastOperationStatusCode { get; set; } = 0;

        /// <summary>
        /// Clears all operations.
        /// </summary>
        public void ClearOperations()
        {
            _operations = new ConcurrentQueue<Operation>();
        }

        /// <summary>
        /// Begins asynchronous operation execution.
        /// </summary>
        public void BeginOperationsAsync()
        {
            OperationsCount = Operations.Count;

            Task.Run(new Action(() =>
            {
                RunOperations();
            }));
        }

        /// <summary>
        /// Fires the OnLog event.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The level to log the message at.</param>
        public void WriteLog(string message, LogLevel level)
        {
            if (OnLog != null)
            {
                OnLog(this, new LogEventArgs(message, level));
            }
        }

        /// <summary>
        /// Runs operations synchronously.
        /// </summary>
        public void RunOperations()
        {
            while (Operations.Count > 0)
            {
                Operation operation = null;

                Operations.TryPeek(out operation);

                _currentOperation = operation;

                SetInProgressState($"Executing {operation.Name}...");

                object result = null;

                try
                {
                    result = _currentOperation.OperationFunction(this);

                    LastOperationStatusCode = 0;

                    if (_currentOperation.OperationCompletedHandler != null)
                    {
                        _currentOperation.OperationCompletedHandler(result);
                    }
                }
                catch (Exception ex)
                {
                    if (_currentOperation.ExceptionHandler != null)
                    {
                        _currentOperation.ExceptionHandler(ex);
                        SetFailureState();
                    }
                    else
                    {
                        SetFailureState("Failed!");
                    }

                    break;
                }

                if (!_currentOperation.ValidateFunction(this))
                {
                    SetFailureState();

                    break;
                }
                else
                {
                    Operations.TryDequeue(out _);

                    if (ProgressTracker != null &&
                        ProgressTracker.Operations != null)
                    {
                        OperationStatus status = ProgressTracker
                            .Operations
                            .Where(x => _currentOperation.Id == x.Id)
                            .FirstOrDefault();

                        if (status != null)
                        {
                            status.StatusCode = OperationStatusCode.CompletedSuccessfully;
                        }
                    }
                }

                CurrentOperationIndex++;
            }

            if (LastOperationStatusCode == 0)
            {
                SetSuccessState();

                if (OnComplete != null)
                {
                    OnComplete(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Updates progress tracking and calls the SetInProgressState function on the runner owner.
        /// </summary>
        /// <param name="message">A message to show in the UI.</param>
        private void SetInProgressState(string message)
        {
            if (ProgressTracker != null &&
                ProgressTracker.Operations != null)
            {
                OperationStatus status = ProgressTracker
                    .Operations
                    .Where(x => _currentOperation.Id == x.Id)
                    .FirstOrDefault();

                if (status != null)
                {
                    ProgressTracker.CurrentOperation = status;
                    status.StatusCode = OperationStatusCode.InProgress;
                }
            }

            _owner.SetInProgressState(message, IndeterminateOps);
        }

        /// <summary>
        /// Calls the SetSuccessState function on the runner owner.
        /// </summary>
        private void SetSuccessState()
        {
            _owner.SetSuccessState();
        }

        /// <summary>
        /// Updates progress tracking and calls the SetFailureState function on the runner owner.
        /// </summary>
        /// <param name="message">A message to show in the UI.</param>
        private void SetFailureState(string message = null)
        {
            LastOperationStatusCode = -1;

            _logger.LogError(message);

            if (ProgressTracker != null &&
                ProgressTracker.Operations != null)
            {
                OperationStatus status = ProgressTracker
                    .Operations
                    .Where(x => _currentOperation.Id == x.Id)
                    .FirstOrDefault();

                if (status != null)
                {
                    status.StatusCode = OperationStatusCode.Failed;
                }
            }

            _owner.SetFailureState(message);
        }
    }
}
