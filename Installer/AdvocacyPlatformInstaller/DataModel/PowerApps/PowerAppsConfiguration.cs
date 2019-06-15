// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Configuration for the PowerApps resources.
    /// </summary>
    public class PowerAppsConfiguration : NotifyPropertyChangedBase
    {
        private string _selectedSku;
        private string _selectedLocation;
        private string _environmentDisplayName;
        private string _organizationName;
        private PowerAppsCdsCurrency _selectedCurrency;
        private PowerAppsCdsLanguage _selectedLanguage;
        private PowerAppsEnvironment _selectedEnvironment;
        private List<string> _locations;
        private List<string> _sku = new List<string>()
        {
            "Trial",
            "Production",
        };

        private List<PowerAppsCdsCurrency> _currencies;
        private List<PowerAppsCdsLanguage> _languages;
        private List<PowerAppsEnvironment> _environments;
        private List<string> _deploymentRegions = new List<string>()
        {
            "APAC",
            "CAN",
            "EMEA",
            "IND",
            "JPN",
            "NorthAmerica",
            "NorthAmerica2",
            "Oceania",
            "SouthAmerica",
        };

        private bool _shouldDelete;
        private bool _shouldOnlyDeleteSolution;

        /// <summary>
        /// Gets or sets a value indicating whether the PowerApps environment should be removed.
        /// </summary>
        public bool ShouldDelete
        {
            get => _shouldDelete;
            set
            {
                _shouldDelete = value;

                if (!_shouldDelete)
                {
                    _shouldOnlyDeleteSolution = false;
                }

                NotifyPropertyChanged("ShouldDelete");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether only the Dynamics 365 CRM solution should be removed (e.g. keep PowerApps environment).
        /// </summary>
        public bool ShouldOnlyDeleteSolution
        {
            get => _shouldOnlyDeleteSolution;
            set
            {
                _shouldOnlyDeleteSolution = value;

                NotifyPropertyChanged("ShouldOnlyDeleteSolution");
            }
        }

        /// <summary>
        /// Gets or sets a list of available locations for PowerApps environments.
        /// </summary>
        public List<string> Locations
        {
            get => _locations;
            set
            {
                if (value == null)
                {
                    _locations = null;
                }
                else
                {
                    _locations = new List<string>(value.OrderBy(x => x));
                }

                NotifyPropertyChanged("Locations");
            }
        }

        /// <summary>
        /// Gets the SKU for the environment.
        /// </summary>
        [JsonIgnore]
        public List<string> SKU
        {
            get => _sku;
        }

        /// <summary>
        /// Gets or sets a list of the available CDS currencies.
        /// </summary>
        public List<PowerAppsCdsCurrency> Currencies
        {
            get => _currencies;
            set
            {
                if (value == null)
                {
                    _currencies = null;
                }
                else
                {
                    _currencies = new List<PowerAppsCdsCurrency>(value.OrderBy(x => x.CurrencyName));
                }

                NotifyPropertyChanged("Currencies");
            }
        }

        /// <summary>
        /// Gets or sets a list of available CDS languages.
        /// </summary>
        public List<PowerAppsCdsLanguage> Languages
        {
            get => _languages;
            set
            {
                if (value == null)
                {
                    _languages = null;
                }
                else
                {
                    _languages = new List<PowerAppsCdsLanguage>(value.OrderBy(x => x.LanguageName));
                }

                NotifyPropertyChanged("Languages");
            }
        }

        /// <summary>
        /// Gets or sets a list of available PowerApps environments.
        /// </summary>
        public List<PowerAppsEnvironment> Environments
        {
            get => _environments;
            set
            {
                if (value == null)
                {
                    _environments = value;
                }
                else
                {
                    _environments = new List<PowerAppsEnvironment>(value.OrderBy(x => x.DisplayName));
                }

                NotifyPropertyChanged("Environments");
            }
        }

        /// <summary>
        /// Gets a list of available deployment regions.
        /// </summary>
        [JsonIgnore]
        public List<string> DeploymentRegions
        {
            get => _deploymentRegions;
        }

        /// <summary>
        /// Gets or sets the selected SKU.
        /// </summary>
        public string SelectedSku
        {
            get => _selectedSku;
            set
            {
                _selectedSku = value;

                NotifyPropertyChanged("SelectedSku");
            }
        }

        /// <summary>
        /// Gets or sets the selected location.
        /// </summary>
        public string SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                _selectedLocation = value;

                NotifyPropertyChanged("SelectedLocation");
            }
        }

        /// <summary>
        /// Gets or sets the PowerApps environment display name.
        /// </summary>
        public string EnvironmentDisplayName
        {
            get => _environmentDisplayName;
            set
            {
                _environmentDisplayName = value;

                NotifyPropertyChanged("EnvironmentDisplayName");
            }
        }

        /// <summary>
        /// Gets or sets the CDS database's unique organization name.
        /// </summary>
        public string OrganizationName
        {
            get => _organizationName;
            set
            {
                _organizationName = value;

                NotifyPropertyChanged("OrganizationName");
            }
        }

        /// <summary>
        /// Gets or sets the selected CDS database currency.
        /// </summary>
        public PowerAppsCdsCurrency SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                _selectedCurrency = value;

                NotifyPropertyChanged("SelectedCurrency");
            }
        }

        /// <summary>
        /// Gets or sets the selected CDS database language.
        /// </summary>
        public PowerAppsCdsLanguage SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;

                NotifyPropertyChanged("SelectedLanguage");
            }
        }

        /// <summary>
        /// Gets or sets the selected PowerApps environment.
        /// </summary>
        public PowerAppsEnvironment SelectedEnvironment
        {
            get => _selectedEnvironment;
            set
            {
                _selectedEnvironment = value;

                NotifyPropertyChanged("SelectedEnvironment");
            }
        }

        /// <summary>
        /// Loads configuration from file.
        /// </summary>
        public void LoadConfiguration()
        {
        }

        /// <summary>
        /// Saves configuration to file.
        /// </summary>
        public void SaveConfiguration()
        {
            ArmTemplateHelper.SetParameterValue(LogicAppsConfiguration.LogicAppsArmTemplateParametersFilePath, "dynamicscrm_environment_name", SelectedEnvironment.OrganizationName);
        }

        /// <summary>
        /// Clears all non-critical fields.
        /// </summary>
        public void ClearNonCriticalFields()
        {
            SelectedSku = null;
            SelectedLocation = null;
            SelectedEnvironment = null;
            SelectedCurrency = null;
            SelectedLanguage = null;

            _sku = null;
            _deploymentRegions = null;

            Locations = null;
            Currencies = null;
            Languages = null;
            Environments = null;
        }
    }
}
