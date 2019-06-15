// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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

    /// <summary>
    /// User control for running the Dynamics 365 CRM solution updates.
    /// </summary>
    public partial class UpdateDynamicsCRMSolutionInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "";

        private DynamicsCrmClient _dynamicsCrmClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateDynamicsCRMSolutionInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public UpdateDynamicsCRMSolutionInstallationControl(
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

            DataModel.SuccessFinalStatusMessage = "Advocacy Platform updated successfully.";
            DataModel.FailureFinalStatusMessage = "Advocacy Platform update failed.";

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
                Name = "UpdateDynamics365Solution",
                OperationFunction = UpdateDynamicsCRMSolutionAsync,
                ValidateFunction = (context) =>
                {
                    return context.LastOperationStatusCode == 0;
                },
                ExceptionHandler = (ex) =>
                {
                    DataModel.StatusMessage = "Failed to update Advocacy Platform solution in Dynamics 365 CRM!";
                },
                RetryOperation = true,
                MaxRetries = 2,
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

        private object UpdateDynamicsCRMSolutionAsync(OperationRunner context)
        {
            return _dynamicsCrmClient.UpdateSolutionAsync(
                DataModel.InstallationConfiguration.DynamicsCrm.SolutionUniqueName,
                DataModel.InstallationConfiguration.DynamicsCrm.SolutionZipFilePath).Result;
        }
    }
}
