// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
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
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// User control for configuring the PowerApps resources.
    /// </summary>
    public partial class PowerAppsInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Power Apps Environment/CDS Database";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "Configure and create a new PowerApps environment and Common Data Services database.";

        private const string _defaultLocation = "unitedstates";
        private const string _defaultSku = "Production";
        private const string _defaultCurrencyCode = "USD";
        private const string _defaultCurrencySymbol = "$";
        private const string _defaultLanguage = "1033";
        private const string _defaultDeploymentRegion = "NorthAmerica";
        private static readonly string _defaultEnvironmentDisplayName = $"AdvocacyPlatform-{Helpers.NewId()}";

        /// <summary>
        /// Initializes a new instance of the <see cref="PowerAppsInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public PowerAppsInstallationControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

            LogOutputControl = DetailsRichTextBox;

            DataModel.SuccessFinalStatusMessage = "Advocacy Platform installed successfully.";
            DataModel.FailureFinalStatusMessage = "Advocacy Platform failed to install.";

            DataModel.InstallationConfiguration.PowerApps.PropertyChanged -= PowerApps_PropertyChanged;
            DataModel.InstallationConfiguration.PowerApps.PropertyChanged += PowerApps_PropertyChanged;

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
            // if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.PowerApps.SelectedLocation))
            // {
            //     DataModel.InstallationConfiguration.PowerApps.SelectedLocation = _defaultLocation;
            // }
            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.PowerApps.EnvironmentDisplayName))
            {
                DataModel.InstallationConfiguration.PowerApps.EnvironmentDisplayName = _defaultEnvironmentDisplayName;
            }

            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.PowerApps.SelectedSku))
            {
                DataModel.InstallationConfiguration.PowerApps.SelectedSku = _defaultSku;
            }

            if (DataModel.InstallationConfiguration.PowerApps.SelectedLanguage != null)
            {
                DataModel.InstallationConfiguration.PowerApps.SelectedLanguage = DataModel.InstallationConfiguration.PowerApps.Languages
                    .Where(x => x.LanguageName == DataModel.InstallationConfiguration.PowerApps.SelectedLanguage.LanguageName)
                    .FirstOrDefault();
            }
            else
            {
                if (DataModel.InstallationConfiguration.PowerApps.Languages != null)
                {
                    DataModel.InstallationConfiguration.PowerApps.SelectedLanguage = DataModel.InstallationConfiguration.PowerApps.Languages
                        .Where(x => x.LanguageName == _defaultLanguage)
                        .FirstOrDefault();
                }
            }

            if (DataModel.InstallationConfiguration.PowerApps.SelectedCurrency != null)
            {
                DataModel.InstallationConfiguration.PowerApps.SelectedCurrency = DataModel.InstallationConfiguration.PowerApps.Currencies
                    .Where(x => x.CurrencyCode == DataModel.InstallationConfiguration.PowerApps.SelectedCurrency.CurrencyCode &&
                                x.CurrencySymbol == DataModel.InstallationConfiguration.PowerApps.SelectedCurrency.CurrencySymbol)
                    .FirstOrDefault();
            }
            else
            {
                if (DataModel.InstallationConfiguration.PowerApps.Currencies != null)
                {
                    DataModel.InstallationConfiguration.PowerApps.SelectedCurrency = DataModel.InstallationConfiguration.PowerApps.Currencies
                        .Where(x => x.CurrencyCode == _defaultCurrencyCode &&
                                    x.CurrencySymbol == _defaultCurrencySymbol)
                        .FirstOrDefault();
                }
            }

            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.DynamicsCrm.SelectedDeploymentRegion))
            {
                DataModel.InstallationConfiguration.DynamicsCrm.SelectedDeploymentRegion = _defaultDeploymentRegion;
            }
        }

        /// <summary>
        /// Sets default options in UI.
        /// </summary>
        public override void SetOptions()
        {
            if (DataModel.InstallationConfiguration.PowerApps.Locations == null ||
                DataModel.InstallationConfiguration.PowerApps.Locations.Count() == 0)
            {
                RunGetPowerAppsEnvironmentLocationsOperation();
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

            if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.PowerApps.SelectedLocation))
            {
                message = "No location selected!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.PowerApps.SelectedSku))
            {
                message = "No SKU selected!";

                isValid = false;
            }
            else if (string.IsNullOrWhiteSpace(DataModel.InstallationConfiguration.PowerApps.EnvironmentDisplayName))
            {
                message = "No display name specified!";

                isValid = false;
            }
            else if (DataModel.InstallationConfiguration.PowerApps.SelectedCurrency == null)
            {
                message = "No currency selected!";

                isValid = false;
            }
            else if (DataModel.InstallationConfiguration.PowerApps.SelectedLanguage == null)
            {
                message = "No language selected!";

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

        private void PowerApps_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedLocation")
            {
                if (DataModel.InstallationConfiguration.PowerApps.SelectedLocation != null)
                {
                    RunGetPowerAppsCdsDatabaseCurrenciesAsync();
                    RunGetPowerAppsCdsDatabaseLanguagesAsync();
                }
            }
        }

        private object GetPowerAppsEnvironmentLocationsAsync(OperationRunner context)
        {
            context.Logger.LogInformation("Getting PowerApps environments...");

            PowerAppsClient client = new PowerAppsClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.GetEnvironmentLocationsAsync().Result;
        }

        private object GetPowerAppsCdsDatabaseCurrenciesAsync(OperationRunner context)
        {
            context.Logger.LogInformation("Getting PowerApps Common Data Services database currencies...");

            PowerAppsClient client = new PowerAppsClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.GetCdsDatabaseCurrenciesAsync(
                DataModel.InstallationConfiguration.PowerApps.SelectedLocation).Result;
        }

        private object GetPowerAppsCdsDatabaseLanguagesAsync(OperationRunner context)
        {
            context.Logger.LogInformation("Getting PowerApps Common Data Services database languages...");

            PowerAppsClient client = new PowerAppsClient(WizardContext.TokenProvider);
            client.SetLogger(context.Logger);

            return client.GetCdsDatabaseLanguagesAsync(
                DataModel.InstallationConfiguration.PowerApps.SelectedLocation).Result;
        }

        private void RunGetPowerAppsEnvironmentLocationsOperation()
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
                    Name = "GetPowerAppsEnvironmentLocations",
                    OperationFunction = GetPowerAppsEnvironmentLocationsAsync,
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    OperationCompletedHandler = (result) =>
                    {
                        GetPowerAppsEnvironmentLocationsResponse response = (GetPowerAppsEnvironmentLocationsResponse)result;

                        DataModel.InstallationConfiguration.PowerApps.Locations = response.Value.Select(x => x.Name).ToList();

                        string defaultLocation = DataModel.InstallationConfiguration.PowerApps.Locations.Where(x => string.Compare(x, _defaultLocation, StringComparison.Ordinal) == 0).FirstOrDefault();

                        DataModel.InstallationConfiguration.PowerApps.SelectedLocation =
                            defaultLocation != null ?
                                defaultLocation :
                                    DataModel.InstallationConfiguration.PowerApps.Locations.FirstOrDefault();
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to acquire PowerApps environment locations!";
                    },
                });

                singleRunner.RunOperations();
            });
        }

        private void RunGetPowerAppsCdsDatabaseCurrenciesAsync()
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
                    Name = "GetPowerAppsCdsDatabaseCurrencies",
                    OperationFunction = GetPowerAppsCdsDatabaseCurrenciesAsync,
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    OperationCompletedHandler = (result) =>
                    {
                        GetPowerAppsCurrenciesResponse response = (GetPowerAppsCurrenciesResponse)result;

                        DataModel.InstallationConfiguration.PowerApps.Currencies = response.Value.Select(x => new PowerAppsCdsCurrency() { CurrencyCode = x.Properties.Code, CurrencyName = x.Name, CurrencySymbol = x.Properties.Symbol }).ToList();

                        PowerAppsCdsCurrency defaultCurrency = DataModel.InstallationConfiguration.PowerApps.Currencies.Where(x => x.CurrencyCode == _defaultCurrencyCode).FirstOrDefault();

                        DataModel.InstallationConfiguration.PowerApps.SelectedCurrency =
                            defaultCurrency != null ?
                                defaultCurrency :
                                    DataModel.InstallationConfiguration.PowerApps.Currencies.FirstOrDefault();
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to acquire PowerApps CDS database currencies!";
                    },
                });

                singleRunner.RunOperations();
            });
        }

        private void RunGetPowerAppsCdsDatabaseLanguagesAsync()
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
                    Name = "GetPowerAppsCdsDatabaseLanguages",
                    OperationFunction = GetPowerAppsCdsDatabaseLanguagesAsync,
                    ValidateFunction = (context) =>
                    {
                        return context.LastOperationStatusCode == 0;
                    },
                    OperationCompletedHandler = (result) =>
                    {
                        GetPowerAppsLanguagesResponse response = (GetPowerAppsLanguagesResponse)result;

                        DataModel.InstallationConfiguration.PowerApps.Languages = response.Value.Select(x => new PowerAppsCdsLanguage() { LanguageName = x.Name, LanguageDisplayName = x.Properties.DisplayName }).ToList();

                        PowerAppsCdsLanguage defaultLanguage = DataModel.InstallationConfiguration.PowerApps.Languages.Where(x => x.LanguageName == _defaultLanguage).FirstOrDefault();

                        DataModel.InstallationConfiguration.PowerApps.SelectedLanguage =
                            defaultLanguage != null ?
                                defaultLanguage :
                                    DataModel.InstallationConfiguration.PowerApps.Languages.FirstOrDefault();
                    },
                    ExceptionHandler = (ex) =>
                    {
                        DataModel.StatusMessage = "Failed to acquire PowerApps CDS database languages!";
                    },
                });

                singleRunner.RunOperations();
            });
        }

        private void GetPowerAppsLocationsButton_Click(object sender, RoutedEventArgs e)
        {
            RunGetPowerAppsEnvironmentLocationsOperation();
        }

        private void GetPowerAppsCdsCurrenciesButton_Click(object sender, RoutedEventArgs e)
        {
            RunGetPowerAppsCdsDatabaseCurrenciesAsync();
        }

        private void GetPowerAppsCdsLanguagesButton_Click(object sender, RoutedEventArgs e)
        {
            RunGetPowerAppsCdsDatabaseLanguagesAsync();
        }
    }
}
