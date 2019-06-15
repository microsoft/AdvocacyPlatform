// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using AdvocacyPlatformInstaller.Clients;
    using AdvocacyPlatformInstaller.Contracts;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Forms = System.Windows.Forms;

    /// <summary>
    /// User control for configuring the Dynamics 365 CRM resources.
    /// </summary>
    public partial class DynamicsCRMSolutionInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Dynamics 365 CRM Solution";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "Configure and deploy the Dynamics 365 CRM Advocacy Platform solution.";

        private const string _defaultDeploymentRegion = "NorthAmerica";
        private const string _defaultPackageZipFilePath = @".\AdvocacyPlatformSolution\AdvocacyPlatformSolution\AdvocacyPlatformSolution_managed.zip";
        private const string _defaultConfigurationZipFilePath = @".\AdvocacyPlatformSolution\AdvocacyPlatformSolution\APConfigurationData.zip";

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicsCRMSolutionInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public DynamicsCRMSolutionInstallationControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

            LogOutputControl = DetailsRichTextBox;

            DataModel.SuccessFinalStatusMessage = "Advocacy Platform installed successfully.";
            DataModel.FailureFinalStatusMessage = "Advocacy Platform failed to install.";

            SetOptions();
            SetDefaults();

            // TODO: Data binding isn't working
            WizardProgress.PagesSource = DataModel.Progress;
        }

        /// <summary>
        /// Sets default view model values.
        /// </summary>
        public override void SetDefaults()
        {
            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.DynamicsCrm.SelectedDeploymentRegion))
            {
                DataModel.InstallationConfiguration.DynamicsCrm.SelectedDeploymentRegion = _defaultDeploymentRegion;
            }

            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.DynamicsCrm.SolutionZipFilePath))
            {
                DataModel.InstallationConfiguration.DynamicsCrm.SolutionZipFilePath = _defaultPackageZipFilePath;
            }

            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.DynamicsCrm.ConfigurationZipFilePath))
            {
                DataModel.InstallationConfiguration.DynamicsCrm.ConfigurationZipFilePath = _defaultConfigurationZipFilePath;
            }

            if (DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment != null &&
                DataModel.InstallationConfiguration.PowerApps.Environments != null)
            {
                DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment = DataModel.InstallationConfiguration.PowerApps.Environments
                    .Where(x => x.EnvironmentName == DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.EnvironmentName)
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Sets default options in UI.
        /// </summary>
        public override void SetOptions()
        {
            if (DataModel.InstallationConfiguration.PowerApps.Environments == null ||
                DataModel.InstallationConfiguration.PowerApps.Environments.Count() == 0)
            {
                RunGetPowerAppsEnvironmentsOperationAsync();
            }
        }

        /// <summary>
        /// Validates selections in UI.
        /// </summary>
        /// <returns>True if valid, false if invalid.</returns>
        public override bool ValidatePage()
        {
            bool isValid = true;
            string message = null;

            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.DynamicsCrm.SelectedDeploymentRegion))
            {
                message = "No deployment region selected!";

                isValid = false;
            }

            if (DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment == null)
            {
                message = "No environment selected!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.OrganizationName) ||
                string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.OrganizationDomainName))
            {
                message = "The Dynamics 365 CRM Organization Unique Name or Domain Name could not be found for the selected environment!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.DynamicsCrm.SolutionZipFilePath))
            {
                message = "No package zip file specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.DynamicsCrm.ConfigurationZipFilePath))
            {
                message = "No configuration zip file specified!";

                isValid = false;
            }
            else if (!File.Exists(DataModel.InstallationConfiguration.DynamicsCrm.SolutionZipFilePath))
            {
                message = "Package zip file does not exist!";

                isValid = false;
            }
            else if (!File.Exists(DataModel.InstallationConfiguration.DynamicsCrm.ConfigurationZipFilePath))
            {
                message = "Configuration zip file does not exist!";

                isValid = false;
            }

            if (File.Exists(DataModel.InstallationConfiguration.DynamicsCrm.SolutionZipFilePath))
            {
                FileInfo packageFileInfo = new FileInfo(DataModel.InstallationConfiguration.DynamicsCrm.SolutionZipFilePath);

                DataModel.InstallationConfiguration.DynamicsCrm.SolutionZipFilePath = packageFileInfo.FullName;
            }

            if (File.Exists(DataModel.InstallationConfiguration.DynamicsCrm.ConfigurationZipFilePath))
            {
                FileInfo packageFileInfo = new FileInfo(DataModel.InstallationConfiguration.DynamicsCrm.ConfigurationZipFilePath);

                DataModel.InstallationConfiguration.DynamicsCrm.ConfigurationZipFilePath = packageFileInfo.FullName;
            }

            if (!isValid)
            {
                DataModel.ShowStatus = true;
                DataModel.StatusMessage = message;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DataModel.StatusMessageBgColor = new SolidColorBrush(Colors.Black);
                    DataModel.StatusMessageFgColor = new SolidColorBrush(Colors.LightPink);
                }));
            }
            else
            {
                DataModel.ShowStatus = false;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DataModel.StatusMessageBgColor = (SolidColorBrush)Background;
                    DataModel.StatusMessageFgColor = (SolidColorBrush)Foreground;
                }));
            }

            return isValid;
        }

        private void RunGetPowerAppsEnvironmentsOperationAsync()
        {
            Task.Run(() =>
            {
                OperationRunner singleRunner = new OperationRunner(
                    null,
                    this,
                    WizardContext.LogFileStream);
                singleRunner.IndeterminateOps = true;
                singleRunner.OnLog += WriteLog;

                singleRunner.Operations.Enqueue(new Operation()
                {
                    Name = "GetPowerAppsEnvironments",
                    OperationFunction = GetPowerAppsEnvironmentsAsync,
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    OperationCompletedHandler = (result) =>
                    {
                        GetPowerAppsEnvironmentsResponse response = (GetPowerAppsEnvironmentsResponse)result;

                        DataModel.InstallationConfiguration.PowerApps.Environments = response
                            .Value
                            .Where(x => x.Properties != null &&
                                        x.Properties.LinkedEnvironmentMetadata != null)
                            .Select(x => new PowerAppsEnvironment()
                            {
                                EnvironmentName = x.Name,
                                DisplayName = x.Properties.DisplayName,
                                OrganizationName = x.Properties.LinkedEnvironmentMetadata != null ? x.Properties.LinkedEnvironmentMetadata.UniqueName : null,
                                OrganizationDomainName = x.Properties.LinkedEnvironmentMetadata != null ? x.Properties.LinkedEnvironmentMetadata.DomainName : null,
                                WebApplicationUrl = x.Properties.LinkedEnvironmentMetadata != null ? x.Properties.LinkedEnvironmentMetadata.InstanceUrl : null,
                            })
                            .ToList();

                        DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment = DataModel.InstallationConfiguration.PowerApps.Environments.FirstOrDefault();
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to acquire PowerApps environments!";
                    },
                });

                singleRunner.RunOperations();
            });
        }

        private object GetPowerAppsEnvironmentsAsync(OperationRunner context)
        {
            PowerAppsClient client = new PowerAppsClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return (GetPowerAppsEnvironmentsResponse)client.GetEnvironmentsAsync().Result;
        }

        private void SolutionPackagePathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();

            ofd.Filter = "zip files (*.zip)|*.zip";

            if (ofd.ShowDialog() == true)
            {
                DataModel.InstallationConfiguration.DynamicsCrm.SolutionZipFilePath = ofd.FileName;
            }
        }

        private void GetEnvironmentsButton_Click(object sender, RoutedEventArgs e)
        {
            RunGetPowerAppsEnvironmentsOperationAsync();
        }

        private void ConfigurationPathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();

            ofd.Filter = "zip files (*.zip)|*.zip";

            if (ofd.ShowDialog() == true)
            {
                DataModel.InstallationConfiguration.DynamicsCrm.SolutionZipFilePath = ofd.FileName;
            }
        }
    }
}
