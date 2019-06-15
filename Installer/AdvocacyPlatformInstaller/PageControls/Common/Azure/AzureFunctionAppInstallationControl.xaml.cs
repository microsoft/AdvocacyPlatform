// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
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
    /// User control for configuring the Azure Function App resources.
    /// </summary>
    public partial class AzureFunctionAppInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of this page.
        /// </summary>
        public const string PageName = "Azure (Function App)";

        /// <summary>
        /// A description for this page.
        /// </summary>
        public const string PageDescription = "Configure the function app required by the platform.";

        private static readonly string _defaultFunctionAppRegistrationName = $"ap-{Helpers.NewId()}-wu-func-aad";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionAppInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public AzureFunctionAppInstallationControl(
            InstallerModel model,
            InstallerWizard context)
                : base(model, context)
        {
            InitializeComponent();

            SequentialRunner = new OperationRunner(
                model.OperationsProgress,
                this,
                WizardContext.LogFileStream);
            SequentialRunner.OnLog += WriteLog;

            SetDefaults();

            // TODO: Data binding isn't working
            WizardProgress.PagesSource = DataModel.Progress;
        }

        /// <summary>
        /// Sets default view model values.
        /// </summary>
        public override void SetDefaults()
        {
            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationName))
            {
                DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationName = _defaultFunctionAppRegistrationName;
            }

            int randomInt = new Random().Next() % 1000;

            AppRegistrationSecretPasswordBox.Password = $"{System.Web.Security.Membership.GeneratePassword(36, 8)}{randomInt}";
        }

        /// <summary>
        /// Sets default options in UI.
        /// </summary>
        public override void SetOptions()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validates selections in UI.
        /// </summary>
        /// <returns>True if valid, false if invalid.</returns>
        public override bool ValidatePage()
        {
            bool isValid = true;
            string message = null;

            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationName))
            {
                message = "No Azure AD application registration name specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(AppRegistrationSecretPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationSecret.Password))
            {
                message = "No Azure AD application registration client secret specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.FunctionApp.AppName))
            {
                message = "No Azure Function App resource name specified!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.Azure.FunctionApp.AppServiceName))
            {
                message = "No Azure App Service Plan resource name specified!";

                isValid = false;
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

        private void WizardContext_OnNext(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AppRegistrationSecretPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            DataModel.InstallationConfiguration.Azure.FunctionApp.ApplicationRegistrationSecret = new System.Net.NetworkCredential("applicationRegistrationSecret", AppRegistrationSecretPasswordBox.SecurePassword);
        }
    }
}
