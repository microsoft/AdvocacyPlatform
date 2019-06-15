// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
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
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// User control for showing completed installer state.
    /// </summary>
    public partial class InstallerCompleteInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = null;

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallerCompleteInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public InstallerCompleteInstallationControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

            DataModel.FinishEnabled = true;
            DataModel.ShowPrevious = false;
            DataModel.ShowNext = false;
            DataModel.ShowCancel = false;
            DataModel.IsFinished = true;

            BuildFinalMessage();

            if (DataModel.IsSuccess)
            {
                if (File.Exists(InstallationConfiguration.InstallationConfigurationFilePath))
                {
                    File.Delete(InstallationConfiguration.InstallationConfigurationFilePath);
                }
            }
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

        /// <summary>
        /// Event handler for the Finish button.
        /// </summary>
        /// <param name="context">The wizard context instance.</param>
        public override void HandleFinish(InstallerWizard context)
        {
            if (!DataModel.InstallationConfiguration.IsSaved &&
                DataModel.InstallerAction != InstallerActionType.Remove)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    MessageBoxResult result = MessageBox.Show("You did not save the installation configuration. Without this file you will need to manually enter information to perform upgrades or removals.\n\nWould you like to save the installation configuration before closing the installer?", "Warning: Please save installation configuration", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        InstallationConfigurationSaveHyperlink_Click(this, new RoutedEventArgs());
                    }

                    Application.Current.MainWindow.Close();
                }));
            }
        }

        private void BuildFinalMessage()
        {
            Span headerSpan = new Span();

            headerSpan.FontSize = 22;
            headerSpan.FontWeight = FontWeights.Bold;

            headerSpan.Inlines.Add(new Run(
                DataModel.FinalStatusMessage));

            FinalMessageTextBlock.Inlines.Add(headerSpan);

            Span textSpan = new Span();

            textSpan.FontSize = 12;
            textSpan.FontWeight = FontWeights.Normal;

            textSpan.Inlines.Add(new LineBreak());

            if (DataModel.ShowCrmLink &&
                DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment != null &&
                !string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.WebApplicationUrl))
            {
                textSpan.Inlines.Add(new LineBreak());
                textSpan.Inlines.Add(new Run(
                    "Please click on this "));

                Hyperlink crmHyperlink = new Hyperlink();

                crmHyperlink.Inlines.Add(new Run("link"));
                crmHyperlink.NavigateUri = new Uri(DataModel.InstallationConfiguration.PowerApps.SelectedEnvironment.WebApplicationUrl);
                crmHyperlink.RequestNavigate += Hyperlink_Click;

                textSpan.Inlines.Add(crmHyperlink);

                textSpan.Inlines.Add(new Run(
                    " to navigate to the deployed application."));
                textSpan.Inlines.Add(new LineBreak());
            }

            if (DataModel.ShowConfigurationFileLink)
            {
                textSpan.Inlines.Add(new LineBreak());
                textSpan.Inlines.Add(new Run(
                    "In order to more easily update or remove this installation you will want to keep the installer configuration file. Please click on this "));

                Hyperlink saveConfigurationHyperlink = new Hyperlink();

                saveConfigurationHyperlink.Inlines.Add(new Run("link"));
                saveConfigurationHyperlink.NavigateUri = new Uri("about:blank");
                saveConfigurationHyperlink.RequestNavigate += InstallationConfigurationSaveHyperlink_Click;

                textSpan.Inlines.Add(saveConfigurationHyperlink);

                textSpan.Inlines.Add(new Run(
                    " to save the installation configuration in a safe place."));
                textSpan.Inlines.Add(new LineBreak());
            }

            FinalMessageTextBlock.Inlines.Add(textSpan);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(((Hyperlink)sender).NavigateUri.ToString());
        }

        private void InstallationConfigurationSaveHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog ofd = new Microsoft.Win32.SaveFileDialog();

            ofd.Title = "Save installation configuration file...";
            ofd.FileName = $"advocacyPlatformInstallation_{DateTime.Now.ToString("yyyy-MM-dd")}.json";

            ofd.Filter = "json files (*.json)|*.json";

            if (ofd.ShowDialog() == true)
            {
                DataModel.InstallationConfiguration.ClearNonCriticalFields();
                DataModel.InstallationConfiguration.SaveConfiguration(ofd.FileName);
                DataModel.InstallationConfiguration.IsSaved = true;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you don't want to save this file? If you don't, you will have to manually enter information to update or remove the Advocacy Platform solution.", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    InstallationConfigurationSaveHyperlink_Click(sender, e);
                }
            }
        }
    }
}
