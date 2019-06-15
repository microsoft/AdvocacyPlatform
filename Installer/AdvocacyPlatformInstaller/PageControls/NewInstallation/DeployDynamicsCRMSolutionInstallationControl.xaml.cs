// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
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

    /// <summary>
    /// User control for deploying the Dynamics 365 CRM solution.
    /// </summary>
    public partial class DeployDynamicsCRMSolutionInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Dynamics 365 CRM Solution Deployment";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "The Dynamics 365 CRM solution is being deployed.";

        private DynamicsCrmClient _dynamicsCrmClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeployDynamicsCRMSolutionInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public DeployDynamicsCRMSolutionInstallationControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

            SequentialRunner = new OperationRunner(
                model.OperationsProgress,
                this,
                WizardContext.LogFileStream);

            LogOutputControl = DetailsRichTextBox;
            SequentialRunner.OnLog += WriteLog;

            SequentialRunner.OnComplete += SequentialRunner_OnComplete;

            DataModel.CurrentOperationRunner = SequentialRunner;

            _dynamicsCrmClient = new DynamicsCrmClient(
                DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.OrganizationName,
                DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.OrganizationDomainName,
                WizardContext.TokenProvider);

            _dynamicsCrmClient.SetLogger(SequentialRunner.Logger);

            DataModel.SuccessFinalStatusMessage = "Advocacy Platform installed successfully.";
            DataModel.FailureFinalStatusMessage = "Advocacy Platform failed to install.";

            // TODO: Data binding isn't working
            WizardProgress.PagesSource = DataModel.Progress;
            OperationsProgressControl.OperationsSource = DataModel.OperationsProgress;

            QueueOperations();

            SequentialRunner.BeginOperationsAsync();
        }

        /// <summary>
        /// Sets default view model values.
        /// </summary>
        public override void SetDefaults()
        {
        }

        /// <summary>
        /// Sets default options in UI.
        /// </summary>
        public override void SetOptions()
        {
        }

        /// <summary>
        /// Validates selections in UI.
        /// </summary>
        /// <returns>True if valid, false if invalid.</returns>
        public override bool ValidatePage()
        {
            return true;
        }

        private void SequentialRunner_OnComplete(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                WizardContext.NextPage();
            }));
        }

        private void QueueOperations()
        {
            SequentialRunner.Operations.Enqueue(new Operation()
            {
                Name = "DeployDynamics365Solution",
                OperationFunction = DeployDynamicsCRMSolutionAsync,
                OperationCompletedHandler = (result) =>
                {
                },
                ValidateFunction = (context) =>
                {
                    return context.LastOperationStatusCode == 0;
                },
                ExceptionHandler = (ex) =>
                {
                    DataModel.StatusMessage = "Failed to deploy Advocacy Platform solution to Dynamics 365 CRM!";
                },
            });
            SequentialRunner.Operations.Enqueue(new Operation()
            {
                Name = "ImportDynamics365SolutionConfigurationData",
                OperationFunction = ImportDynamicsCRMSolutionConfigurationAsync,
                OperationCompletedHandler = (result) =>
                {
                },
                ValidateFunction = (context) =>
                {
                    return context.LastOperationStatusCode == 0;
                },
                ExceptionHandler = (ex) =>
                {
                    DataModel.StatusMessage = "Failed to import configuration data for Advocacy Platform into Dynamics 365 CRM!";
                },
            });

            DataModel.OperationsProgress.Operations = SequentialRunner.Operations
                .Select(x => new OperationStatus()
                {
                    Id = x.Id,
                    Name = x.Name,
                    StatusCode = OperationStatusCode.NotStarted,
                })
                .ToList();
        }

        private object DeployDynamicsCRMSolutionAsync(OperationRunner context)
        {
            DataModel.ShowStatus = true;
            DataModel.StatusMessage = "Deploying Advocacy Platform Dynamics 365 CRM managed solution...";

            DataModel.ShowProgress = true;
            DataModel.OperationInProgress = true;

            DataModel.NextEnabled = false;
            DataModel.PreviousEnabled = false;

            DynamicsCrmSolution solution = _dynamicsCrmClient.GetSolutionAsync("AdvocacyPlatformSolution").Result;

            if (solution == null)
            {
                return _dynamicsCrmClient.ImportSolutionAsync(DataModel.InstallationConfiguration.DynamicsCrm.SolutionZipFilePath).Result;
            }

            return solution;
        }

        private object ImportDynamicsCRMSolutionConfigurationAsync(OperationRunner context)
        {
            string schemaXml = null;
            string dataXml = null;

            using (Stream fileStream = File.Open(DataModel.InstallationConfiguration.DynamicsCrm.ConfigurationZipFilePath, FileMode.Open))
            {
                using (ZipArchive zipFile = new ZipArchive(fileStream, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry schemaFile = null;
                    ZipArchiveEntry dataFile = null;

                    foreach (ZipArchiveEntry entry in zipFile.Entries)
                    {
                        if (string.Compare(entry.Name, "data_schema.xml", true) == 0)
                        {
                            schemaFile = entry;
                        }
                        else if (string.Compare(entry.Name, "data.xml", true) == 0)
                        {
                            dataFile = entry;
                        }
                    }

                    if (schemaFile == null || dataFile == null)
                    {
                        throw new Exception("Invalid configuration archive!");
                    }

                    using (StreamReader schemaFileStream = new StreamReader(schemaFile.Open()))
                    {
                        schemaXml = schemaFileStream.ReadToEnd();
                    }

                    using (StreamReader dataFileStream = new StreamReader(dataFile.Open()))
                    {
                        dataXml = dataFileStream.ReadToEnd();
                    }
                }
            }

            _dynamicsCrmClient.ImportEntitiesAsync(
                schemaXml,
                dataXml).Wait();

            return true;
        }
    }
}
