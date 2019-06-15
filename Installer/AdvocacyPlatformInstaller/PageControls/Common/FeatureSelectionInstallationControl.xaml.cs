// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
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

    /// <summary>
    /// User control for selecting features to install or modify.
    /// </summary>
    public partial class FeatureSelectionInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureSelectionInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public FeatureSelectionInstallationControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

            // TODO: Data binding isn't working
            WizardProgress.PagesSource = DataModel.Progress;
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
            bool isValid = true;
            string message = null;

            if (DataModel.InstallationConfiguration.Features.ShouldInstallCount == 0)
            {
                message = "You must select at least one feature to install";

                isValid = false;
            }

            if (!isValid)
            {
                DataModel.ShowStatus = true;
                DataModel.StatusMessage = message;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    DataModel.StatusMessageBgColor = new SolidColorBrush(Colors.Black);
                    DataModel.StatusMessageFgColor = new SolidColorBrush(Colors.Yellow);
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

        /// <summary>
        /// Handles the click event for the Next button.
        /// </summary>
        /// <param name="context">The installer wizard context instance.</param>
        public override void HandleNext(InstallerWizard context)
        {
            SetOperations(context);

            base.HandleNext(context);
        }

        /// <summary>
        /// Sets the operations for a selected installer action.
        /// </summary>
        /// <param name="wizardContext">The wizard context instance.</param>
        private void SetOperations(InstallerWizard wizardContext)
        {
            switch (DataModel.InstallerAction)
            {
                case InstallerActionType.New:
                    BuildInstallationSteps(wizardContext);
                    break;

                case InstallerActionType.Modify:
                    BuildUpdateSteps(wizardContext);
                    break;
            }
        }

        private void BuildInstallationSteps(InstallerWizard wizardContext)
        {
            Feature uiComponents = DataModel.InstallationConfiguration.Features[FeatureNames.UIComponents];

            if (uiComponents.ShouldInstall)
            {
                wizardContext.BuildWizard(uiComponents, DataModel.InstallationConfiguration.Features);
            }

            Feature apiComponents = DataModel.InstallationConfiguration.Features[FeatureNames.APIComponents];

            if (apiComponents.ShouldInstall)
            {
                wizardContext.BuildWizard(apiComponents, DataModel.InstallationConfiguration.Features);
            }

            wizardContext.AddPage(typeof(InstallerCompleteInstallationControl), null, null, true, false);
        }

        private void BuildUpdateSteps(InstallerWizard wizardContext)
        {
            Feature uiComponents = DataModel.InstallationConfiguration.Features[FeatureNames.UIComponents];

            if (uiComponents.ShouldInstall)
            {
                wizardContext.BuildWizard(uiComponents, DataModel.InstallationConfiguration.Features);
            }

            Feature apiComponents = DataModel.InstallationConfiguration.Features[FeatureNames.APIComponents];

            if (apiComponents.ShouldInstall)
            {
                wizardContext.BuildWizard(apiComponents, DataModel.InstallationConfiguration.Features);
            }

            wizardContext.AddPage(typeof(InstallerCompleteInstallationControl), null, null, true, false);
        }
    }
}
