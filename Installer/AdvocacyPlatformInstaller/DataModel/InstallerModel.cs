// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// View model representing configuration information for the installation process.
    /// </summary>
    public class InstallerModel : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Tag to attach to resource group deployed to.
        /// </summary>
        public const string ResourceGroupTagKey = "AdvocacyPlatform";

        /// <summary>
        /// Value to set for the tag attached to the resource group deployed to.
        /// </summary>
        public static readonly string ResourceGroupTagValue = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private InstallationConfiguration _installationConfiguration;
        private WizardProgress _progress;
        private OperationsProgress _operationsProgress;
        private OperationRunner _currentOperationRunner;

        private WizardPage _page;

        private InstallerActionType _installerAction;

        private string _statusMessage;
        private string _finalStatusMessage;
        private string _successFinalStatusMessage;
        private string _failureFinalStatusMessage;
        private bool _isSuccess;
        private bool _isFinished;
        private bool _showCrmLink;
        private bool _showConfigurationFileLink;

        private SolidColorBrush _statusMessageBgColor;
        private SolidColorBrush _statusMessageFgColor;

        private bool _nextEnabled;
        private bool _previousEnabled;
        private bool _finishEnabled;

        private bool _showStatus;
        private bool _showProgress;
        private bool _showProgressIndeterminate;
        private bool _showDescription;
        private bool _showPrevious;
        private bool _showNext;
        private bool _showCancel;
        private bool _showFinish;
        private bool _operationInProgress;
        private bool _showBrowser;

        private bool _showDetails;

        private IEnumerable<InstallerPermission> _permissions = new List<InstallerPermission>()
        {
            new InstallerPermission() { API = "Azure Key Vault", Permission = "user_impersonation", Description = "Have full access to the Azure Key Vault service", RequiresAdminConsent = false },
            new InstallerPermission() { API = "Azure Service Management", Permission = "user_impersonation", Description = "Access Azure Service Management as organization users", RequiresAdminConsent = false },
            new InstallerPermission() { API = "Azure Storage", Permission = "user_impersonation", Description = "Access Azure Storage", RequiresAdminConsent = false },
            new InstallerPermission() { API = "Dynamics CRM", Permission = "user_impersonation", Description = "Access Dynamics 365 as organization users", RequiresAdminConsent = false },
            new InstallerPermission() { API = "Microsoft Graph", Permission = "Directory.AccessAsUser.All", Description = "Access directory as the signed in user", RequiresAdminConsent = true },
            new InstallerPermission() { API = "Microsoft Graph", Permission = "User.Read", Description = "Sign in and read user profile", RequiresAdminConsent = false },
            new InstallerPermission() { API = "PowerApps Service", Permission = "User", Description = "Access the PowerApps Service API", RequiresAdminConsent = false },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallerModel"/> class.
        /// </summary>
        /// <param name="mainWindow">The owning UI element.</param>
        public InstallerModel(UIElement mainWindow)
        {
            InstallationConfiguration = new InstallationConfiguration();
            Progress = new WizardProgress();
            Page = new WizardPage();
            OperationsProgress = new OperationsProgress();

            InstallationConfiguration.LoadConfiguration();

            ShowCrmLink = false;

            PreviousEnabled = true;
            NextEnabled = true;

            ShowStatus = false;
            ShowProgress = false;
            ShowPrevious = false;
            ShowNext = true;
            ShowCancel = true;
            ShowFinish = false;

            InstallerAction = InstallerActionType.New;
        }

        /// <summary>
        /// Gets a list of API permissions required by the installer.
        /// </summary>
        public IEnumerable<InstallerPermission> Permissions => _permissions;

        /// <summary>
        /// Gets or sets the model representing the installation configuration.
        /// </summary>
        public InstallationConfiguration InstallationConfiguration
        {
            get => _installationConfiguration;
            set
            {
                _installationConfiguration = value;

                NotifyPropertyChanged("InstallationConfiguration");
            }
        }

        /// <summary>
        /// Gets or sets the installation action.
        /// </summary>
        public InstallerActionType InstallerAction
        {
            get => _installerAction;
            set
            {
                _installerAction = value;

                NotifyPropertyChanged("InstallerAction");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether an operation is in progress.
        /// </summary>
        public bool OperationInProgress
        {
            get => _operationInProgress;
            set
            {
                _operationInProgress = value;

                if (_operationInProgress)
                {
                    NextEnabled = !_operationInProgress;
                    PreviousEnabled = !_operationInProgress;
                }

                NotifyPropertyChanged("OperationInProgress");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the browser control should be shown.
        /// </summary>
        public bool ShowBrowser
        {
            get => _showBrowser;
            set
            {
                _showBrowser = value;

                NotifyPropertyChanged("ShowBrowser");
            }
        }

        /// <summary>
        /// Gets or sets the progress tracking (WizardProgress) instance.
        /// </summary>
        public WizardProgress Progress
        {
            get => _progress;
            set
            {
                _progress = value;

                NotifyPropertyChanged("Progress");
            }
        }

        /// <summary>
        /// Gets or sets the current page of the installer UI.
        /// </summary>
        public WizardPage Page
        {
            get => _page;
            set
            {
                _page = value;

                NotifyPropertyChanged("Page");
            }
        }

        /// <summary>
        /// Gets or sets the operation progress tracking models.
        /// </summary>
        public OperationsProgress OperationsProgress
        {
            get => _operationsProgress;
            set
            {
                _operationsProgress = value;

                NotifyPropertyChanged("OperationsProgress");
            }
        }

        /// <summary>
        /// Gets or sets the current OperationRunner instance.
        /// </summary>
        public OperationRunner CurrentOperationRunner
        {
            get => _currentOperationRunner;
            set
            {
                _currentOperationRunner = value;

                NotifyPropertyChanged("CurrentOperationRunner");
            }
        }

        /// <summary>
        /// Gets or sets a status message to display in the UI.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;

                NotifyPropertyChanged("StatusMessage");
            }
        }

        /// <summary>
        /// Gets or sets the final status message to display on installation completion.
        /// </summary>
        public string FinalStatusMessage
        {
            get => _finalStatusMessage;
            set
            {
                _finalStatusMessage = value;

                NotifyPropertyChanged("FinalStatusMessage");
            }
        }

        /// <summary>
        /// Gets or sets the final status message to display when installation completes successfully.
        /// </summary>
        public string SuccessFinalStatusMessage
        {
            get => _successFinalStatusMessage;
            set
            {
                _successFinalStatusMessage = value;

                NotifyPropertyChanged("SuccessFinalStatusMessage");
            }
        }

        /// <summary>
        /// Gets or sets the final status message to display when installation fails.
        /// </summary>
        public string FailureFinalStatusMessage
        {
            get => _failureFinalStatusMessage;
            set
            {
                _failureFinalStatusMessage = value;

                NotifyPropertyChanged("FailureFinalStatusMessage");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether installation was successful.
        /// </summary>
        public bool IsSuccess
        {
            get => _isSuccess;
            set
            {
                _isSuccess = value;

                NotifyPropertyChanged("IsSuccess");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether installation has completed.
        /// </summary>
        public bool IsFinished
        {
            get => _isFinished;
            set
            {
                _isFinished = value;

                NotifyPropertyChanged("IsFinished");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a link to the Dynamics 365 CRM web application URI should be shown on the last page.
        /// </summary>
        public bool ShowCrmLink
        {
            get => _showCrmLink;
            set
            {
                _showCrmLink = value;

                NotifyPropertyChanged("ShowCrmLink");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a link to saves the installation configuration should be shown on the last page.
        /// </summary>
        public bool ShowConfigurationFileLink
        {
            get => _showConfigurationFileLink;
            set
            {
                _showConfigurationFileLink = value;

                NotifyPropertyChanged("ShowConfigurationFileLink");
            }
        }

        /// <summary>
        /// Gets or sets the background color for the status message.
        /// </summary>
        public SolidColorBrush StatusMessageBgColor
        {
            get => _statusMessageBgColor;
            set
            {
                _statusMessageBgColor = value;

                NotifyPropertyChanged("StatusMessageBgColor");
            }
        }

        /// <summary>
        /// Gets or sets the foreground color for the status message.
        /// </summary>
        public SolidColorBrush StatusMessageFgColor
        {
            get => _statusMessageFgColor;
            set
            {
                _statusMessageFgColor = value;

                NotifyPropertyChanged("StatusMessageFgColor");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the previous button is enabled.
        /// </summary>
        public bool PreviousEnabled
        {
            get => _previousEnabled;
            set
            {
                _previousEnabled = value;

                NotifyPropertyChanged("PreviousEnabled");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the next button is enabled.
        /// </summary>
        public bool NextEnabled
        {
            get => _nextEnabled;
            set
            {
                _nextEnabled = value;

                NotifyPropertyChanged("NextEnabled");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the finish button is enabled.
        /// </summary>
        public bool FinishEnabled
        {
            get => _finishEnabled;
            set
            {
                _finishEnabled = value;

                NotifyPropertyChanged("FinishEnabled");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the status message should be shown.
        /// </summary>
        public bool ShowStatus
        {
            get => _showStatus;
            set
            {
                _showStatus = value;

                NotifyPropertyChanged("ShowStatus");
            }
        }

        /// <summary>
        /// Gets a value indicating whether the progress bar should be shown.
        /// </summary>
        public bool ShowAnyProgress => ShowProgress || ShowProgressIndeterminate;

        /// <summary>
        /// Gets or sets a value indicating whether the determinate progress bar should be shown.
        /// </summary>
        public bool ShowProgress
        {
            get => _showProgress;
            set
            {
                _showProgress = value;

                NotifyPropertyChanged("ShowProgress");
                NotifyPropertyChanged("ShowAnyProgress");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the indeterminate progress bar should be shown.
        /// </summary>
        public bool ShowProgressIndeterminate
        {
            get => _showProgressIndeterminate;
            set
            {
                _showProgressIndeterminate = value;

                NotifyPropertyChanged("ShowProgressIndeterminate");
                NotifyPropertyChanged("ShowAnyProgress");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the feature description should be shown.
        /// </summary>
        public bool ShowDescription
        {
            get => _showDescription;
            set
            {
                _showDescription = value;

                NotifyPropertyChanged("ShowDescription");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the previous button should be shown.
        /// </summary>
        public bool ShowPrevious
        {
            get => _showPrevious;
            set
            {
                _showPrevious = value;

                NotifyPropertyChanged("ShowPrevious");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the next button should be shown.
        /// </summary>
        public bool ShowNext
        {
            get => _showNext;
            set
            {
                _showNext = value;

                NotifyPropertyChanged("ShowNext");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cancel button should be shown.
        /// </summary>
        public bool ShowCancel
        {
            get => _showCancel;
            set
            {
                _showCancel = value;

                NotifyPropertyChanged("ShowCancel");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the finish button should be shown.
        /// </summary>
        public bool ShowFinish
        {
            get => _showFinish;
            set
            {
                _showFinish = value;

                NotifyPropertyChanged("ShowFinish");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the operation details should be shown.
        /// </summary>
        public bool ShowDetails
        {
            get => _showDetails;
            set
            {
                _showDetails = value;

                NotifyPropertyChanged("ShowDetails");
            }
        }
    }
}
