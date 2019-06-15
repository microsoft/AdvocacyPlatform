// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    /// <summary>
    /// Base implementation of the IWizardPage interface for a user control.
    /// </summary>
    public abstract class WizardPageBase : UserControl, IWizardPage
    {
        private OperationRunner _sequentialRunner;
        private RichTextBox _logOutputControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="WizardPageBase"/> class.
        /// </summary>
        public WizardPageBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WizardPageBase"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public WizardPageBase(InstallerModel model, InstallerWizard context)
        {
            DataContext = DataModel = model;
            WizardContext = context;

            WizardContext.OnNext += WizardContext_OnNext;
            WizardContext.OnPrevious += WizardContext_OnPrevious;
            WizardContext.OnCancel += WizardContext_OnCancel;
            WizardContext.OnFinish += WizardContext_OnFinish;
        }

        /// <summary>
        /// Gets or sets the data context.
        /// </summary>
        public InstallerModel DataModel { get; set; }

        /// <summary>
        /// Gets or sets the wizard context.
        /// </summary>
        public InstallerWizard WizardContext { get; set; }

        /// <summary>
        /// Gets or sets the operation runner instance for running operations sequentially.
        /// </summary>
        protected OperationRunner SequentialRunner
        {
            get => _sequentialRunner;
            set
            {
                _sequentialRunner = value;
            }
        }

        /// <summary>
        /// Gets or sets the user control for log output.
        /// </summary>
        protected RichTextBox LogOutputControl
        {
            get => _logOutputControl;
            set
            {
                _logOutputControl = value;
            }
        }

        /// <summary>
        /// Base function for handling Finish button clicks.
        /// </summary>
        /// <param name="context">The wizard context instance.</param>
        public virtual void HandleFinish(InstallerWizard context)
        {
        }

        /// <summary>
        /// Base function for handling Next button clicks.
        /// </summary>
        /// <param name="context">The wizard context instance.</param>
        public virtual void HandleNext(InstallerWizard context)
        {
            bool isValid = ValidatePage();

            if (isValid)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    DataModel.StatusMessageBgColor = (SolidColorBrush)Background;
                    DataModel.StatusMessageFgColor = (SolidColorBrush)Foreground;
                }));

                context.NextPage();
            }
        }

        /// <summary>
        /// Base function for handling Previous button clicks.
        /// </summary>
        /// <param name="context">The wizard context instance.</param>
        public virtual void HandlePrevious(InstallerWizard context)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                DataModel.StatusMessageBgColor = (SolidColorBrush)Background;
                DataModel.StatusMessageFgColor = (SolidColorBrush)Foreground;
            }));

            DataModel.NextEnabled = true;
            DataModel.ShowStatus = false;
            DataModel.StatusMessage = null;

            context.PreviousPage();
        }

        /// <summary>
        /// Base function for handling Cancel button clicks.
        /// </summary>
        /// <param name="context">The wizard context instance.</param>
        public virtual void HandleCancel(InstallerWizard context)
        {
        }

        /// <summary>
        /// Base function for setting default view model values.
        /// </summary>
        public abstract void SetDefaults();

        /// <summary>
        /// Base function for setting default options in UI.
        /// </summary>
        public abstract void SetOptions();

        /// <summary>
        /// Base function for validating selections in the UI.
        /// </summary>
        /// <returns>True if valid, false if invalid.</returns>
        public abstract bool ValidatePage();

        /// <summary>
        /// Base function for setting state for when an operation is in progress.
        /// </summary>
        /// <param name="message">A message to show in the UI.</param>
        /// <param name="isIndeterminateOperation">Specifies if the operations should show the indeterminate progress bar.</param>
        public virtual void SetInProgressState(string message, bool isIndeterminateOperation)
        {
            DataModel.ShowStatus = true;

            if (isIndeterminateOperation)
            {
                DataModel.ShowProgressIndeterminate = true;
                DataModel.ShowProgress = false;
            }
            else
            {
                DataModel.ShowProgressIndeterminate = false;
                DataModel.ShowProgress = true;
            }

            DataModel.NextEnabled = false;
            DataModel.PreviousEnabled = false;
            DataModel.OperationInProgress = true;
            DataModel.StatusMessage = message;

            DataModel.ShowConfigurationFileLink = false;
            DataModel.ShowCrmLink = false;
        }

        /// <summary>
        /// Base function for setting state for when all operations have completed successfully.
        /// </summary>
        public virtual void SetSuccessState()
        {
            DataModel.ShowStatus = false;
            DataModel.ShowProgress = false;
            DataModel.ShowProgressIndeterminate = false;
            DataModel.PreviousEnabled = true;
            DataModel.NextEnabled = true;
            DataModel.OperationInProgress = false;

            DataModel.FinalStatusMessage = DataModel.SuccessFinalStatusMessage;
            DataModel.IsSuccess = true;

            switch (DataModel.InstallerAction)
            {
                case InstallerActionType.New:
                    DataModel.ShowCrmLink = true;
                    DataModel.ShowConfigurationFileLink = true;
                    break;

                case InstallerActionType.Modify:
                    DataModel.ShowCrmLink = true;
                    DataModel.ShowConfigurationFileLink = true;
                    break;

                case InstallerActionType.Remove:
                    DataModel.ShowCrmLink = false;
                    DataModel.ShowConfigurationFileLink = false;
                    break;
            }
        }

        /// <summary>
        /// Base functionality for when an operation has failed.
        /// </summary>
        /// <param name="message">A message to show in the UI.</param>
        public virtual void SetFailureState(string message)
        {
            DataModel.ShowStatus = true;

            if (message != null)
            {
                DataModel.StatusMessage = message;
            }

            Dispatcher.Invoke(new Action(() =>
            {
                DataModel.StatusMessageBgColor = new SolidColorBrush(Colors.Black);
                DataModel.StatusMessageFgColor = new SolidColorBrush(Colors.Yellow);
            }));

            DataModel.ShowProgress = false;
            DataModel.ShowProgressIndeterminate = false;
            DataModel.PreviousEnabled = true;
            DataModel.NextEnabled = false;
            DataModel.OperationInProgress = false;

            DataModel.FinalStatusMessage = DataModel.FailureFinalStatusMessage;
            DataModel.IsSuccess = false;

            DataModel.ShowConfigurationFileLink = false;
            DataModel.ShowCrmLink = false;
        }

        /// <summary>
        /// Base functionality for writing log lines to a logging control.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="e">Event arguments.</param>
        public virtual void WriteLog(object sender, LogEventArgs e)
        {
            if (LogOutputControl != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    Paragraph firstPara = LogOutputControl
                        .Document
                        .Blocks
                        .Where(x => x is Paragraph)
                        .FirstOrDefault() as Paragraph;

                    if (firstPara == null)
                    {
                        firstPara = new Paragraph();
                        LogOutputControl.Document.Blocks.Add(firstPara);
                    }

                    Run newRun = new Run($"{e.Message}\n");

                    SolidColorBrush foregroundColor = null;

                    switch (e.Level)
                    {
                        case LogLevel.Informational:
                            break;

                        case LogLevel.Warning:
                            foregroundColor = new SolidColorBrush(Colors.Yellow);
                            break;

                        case LogLevel.Error:
                            foregroundColor = new SolidColorBrush(Colors.Red);
                            break;
                    }

                    if (foregroundColor != null)
                    {
                        newRun.Foreground = foregroundColor;
                    }

                    firstPara.Inlines.Add(newRun);

                    LogOutputControl.ScrollToEnd();
                }));
            }
        }

        /// <summary>
        /// Base event handler for the Finish button.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void WizardContext_OnFinish(object sender, EventArgs e)
        {
            HandleFinish((InstallerWizard)sender);
        }

        /// <summary>
        /// Base event handler for the Cancel button.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void WizardContext_OnCancel(object sender, EventArgs e)
        {
            HandleCancel((InstallerWizard)sender);
        }

        /// <summary>
        /// Base event handler for the Previous button.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void WizardContext_OnPrevious(object sender, EventArgs e)
        {
            HandlePrevious((InstallerWizard)sender);
        }

        /// <summary>
        /// Base event handler for the Next button.
        /// </summary>
        /// <param name="sender">Object that fired the event.</param>
        /// <param name="e">Event arguments.</param>
        private void WizardContext_OnNext(object sender, EventArgs e)
        {
            HandleNext((InstallerWizard)sender);
        }
    }
}
