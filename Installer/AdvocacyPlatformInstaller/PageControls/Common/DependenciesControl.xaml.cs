// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
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
    /// User control for confirming dependencies are met.
    /// </summary>
    public partial class DependenciesControl : WizardPageBase
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
        /// Initializes a new instance of the <see cref="DependenciesControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public DependenciesControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

            ConsentBrowser.Navigating += ConsentBrowser_Navigating;

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
            return true;
        }

        private void ConsentBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            NameValueCollection queryParameters = System.Web.HttpUtility.ParseQueryString(e.Uri.Query);

            string adminConsent = queryParameters["admin_consent"];

            if (adminConsent != null)
            {
                if (string.Compare(adminConsent, "true", true) != 0)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("The installer will fail if admin consent is not granted for the application permissions being requested. As consent was not give this installer will now exit.", "Admin Consent Required", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.MainWindow.Close();
                    }));
                }
                else
                {
                    WizardContext.NextPage();
                }
            }

            string error = queryParameters["error"];

            if (error != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show("The installer will fail if admin consent is not granted for the application permissions being requested. As consent was not given this installer will now exit.", "Admin Consent Required", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.MainWindow.Close();
                }));
            }
        }

        private void AdminConsentHyperlink_Click(object sender, RoutedEventArgs e)
        {
            string nonce = Guid.NewGuid().ToString();
            string clientId = ConfigurationManager.AppSettings["clientId"];
            string redirectUri = ConfigurationManager.AppSettings["redirectUri"];
            string consentUri = $"https://login.microsoftonline.com/common/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri=https://localhost&nonce={nonce}&resource={AzureClient.AzureADAudience}&prompt=admin_consent";

            // Process.Start(consentUri);
            DataModel.ShowBrowser = true;
            ConsentBrowser.Navigate(consentUri);
        }
    }
}
