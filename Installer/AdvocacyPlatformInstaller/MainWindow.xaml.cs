// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Main window.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            DataModel = new InstallerModel(this);
            DataContext = DataModel;

            DataModel.StatusMessageBgColor = (SolidColorBrush)Background;
            DataModel.StatusMessageFgColor = (SolidColorBrush)Foreground;

            DataModel.InstallationConfiguration.Features = new FeatureTree();

            Wizard = new InstallerWizard(this, WizardPageGrid, DataModel)
                .AddPage(typeof(WelcomeInstallationControl), null, null, false, false)
                .AddPage(typeof(DependenciesControl), "Dependency Check", "Checking to ensure dependencies for deployment are available.", false, false)
                .AddPage(typeof(FeatureSelectionInstallationControl), "Feature Selection", "Select the features you want to install/deploy.", false, false);

            Wizard.SetPagesCheckpoint();

            Wizard.Start();
        }

        /// <summary>
        /// Gets or sets the view model for installation configuration and progress.
        /// </summary>
        public InstallerModel DataModel { get; set; }

        /// <summary>
        /// Gets or sets the context for managing installer processes.
        /// </summary>
        public InstallerWizard Wizard { get; set; }

        private void WizardPreviousButton_Click(object sender, RoutedEventArgs e)
        {
            Wizard.NotifyPreviousPage();
        }

        private void WizardNextButton_Click(object sender, RoutedEventArgs e)
        {
            Wizard.NotifyNextPage();
        }

        private void WizardCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Wizard.NotifyCancel();
            Close();
        }

        private void WizardFinishButton_Click(object sender, RoutedEventArgs e)
        {
            Wizard.NotifyFinish();
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!DataModel.IsFinished)
            {
                DataModel.InstallationConfiguration.SaveConfiguration();
            }
        }
    }
}
