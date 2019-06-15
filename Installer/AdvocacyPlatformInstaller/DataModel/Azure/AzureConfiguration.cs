// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Configuration information for Azure resources.
    /// </summary>
    public class AzureConfiguration : NotifyPropertyChangedBase
    {
        /// <summary>
        /// The name of the Azure resource ARM template file.
        /// </summary>
        public const string AzureArmTemplateFileName = "azuredeploy.json";

        /// <summary>
        /// The name of the Azure resource ARM template parameters file.
        /// </summary>
        public const string AzureArmTemplateParametersFileName = "azuredeploy.parameters.json";

        /// <summary>
        /// The path to the Azure resource ARM template file.
        /// </summary>
        public static readonly string AzureArmTemplateFilePath = $@".\config\{AzureArmTemplateFileName}";

        /// <summary>
        /// The path to the Azure resource ARM template parameters file.
        /// </summary>
        public static readonly string AzureArmTemplateParametersFilePath = $@".\config\{AzureArmTemplateParametersFileName}";

        private AzureSubscription _selectedSubscription;
        private AzureResourceGroup _selectedResourceGroup;
        private FunctionAppConfiguration _functionApp;
        private StorageAccountConfiguration _storageAccount;
        private ServiceBusConfiguration _serviceBus;
        private KeyVaultConfiguration _keyVault;
        private SpeechCognitiveServiceConfiguration _speechCognitiveService;
        private LuisConfiguration _luis;
        private BingMapsConfiguration _bingMaps;
        private LogicAppsConfiguration _logicApps;
        private ManagementConfiguration _management;

        private string _armTempPath = @".\ArmTemp";

        private string _resourceGroupName;
        private string _resourceGroupLocation;

        private bool _checkedForSubscriptions;
        private bool _shouldDelete;
        private bool _shouldDeleteAppRegistration;
        private IEnumerable<AzureSubscription> _subscriptions;
        private IEnumerable<AzureResourceGroup> _resourceGroups;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureConfiguration"/> class.
        /// </summary>
        public AzureConfiguration()
        {
            FunctionApp = new FunctionAppConfiguration();
            StorageAccount = new StorageAccountConfiguration();
            ServiceBus = new ServiceBusConfiguration();
            KeyVault = new KeyVaultConfiguration();
            SpeechCognitiveService = new SpeechCognitiveServiceConfiguration();
            Luis = new LuisConfiguration();
            BingMaps = new BingMapsConfiguration();
            LogicApps = new LogicAppsConfiguration();
            Management = new ManagementConfiguration();
            Luis = new LuisConfiguration();

            ResourceGroupLocation = "westus2";
        }

        /// <summary>
        /// Gets or sets the available Azure subscriptions.
        /// </summary>
        public IEnumerable<AzureSubscription> Subscriptions
        {
            get => _subscriptions;
            set
            {
                _subscriptions = value;

                NotifyPropertyChanged("Subscriptions");
                NotifyPropertyChanged("HasNoSubscriptions");
            }
        }

        /// <summary>
        /// Gets or sets the available Azure Resource Groups.
        /// </summary>
        public IEnumerable<AzureResourceGroup> ResourceGroups
        {
            get => _resourceGroups;
            set
            {
                _resourceGroups = value;

                NotifyPropertyChanged("ResourceGroups");
            }
        }

        /// <summary>
        /// Gets a value indicating whether a security principal has no associated Azure subscriptions.
        /// </summary>
        public bool HasNoSubscriptions
        {
            get => _checkedForSubscriptions && (_subscriptions == null || _subscriptions.Count() == 0);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the installer has checked for Azure subscriptions.
        /// </summary>
        public bool CheckedForSubscriptions
        {
            get => _checkedForSubscriptions;
            set
            {
                _checkedForSubscriptions = value;

                NotifyPropertyChanged("CheckedForSubscriptions");
                NotifyPropertyChanged("HasNoSubscriptions");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Azure AP resources should be deleted.
        /// </summary>
        public bool ShouldDelete
        {
            get => _shouldDelete;
            set
            {
                _shouldDelete = value;

                NotifyPropertyChanged("ShouldDelete");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the AP Azure AD application registration should be removed.
        /// </summary>
        public bool ShouldDeleteAppRegistration
        {
            get => _shouldDeleteAppRegistration;
            set
            {
                _shouldDeleteAppRegistration = value;

                NotifyPropertyChanged("ShouldDeleteAppRegistration");
            }
        }

        /// <summary>
        /// Gets or sets the selected Azure subscription.
        /// </summary>
        public AzureSubscription SelectedSubscription
        {
            get => _selectedSubscription;
            set
            {
                _selectedSubscription = value;

                NotifyPropertyChanged("SelectedSubscription");
            }
        }

        /// <summary>
        /// Gets or sets the selected Azure Resource Group.
        /// </summary>
        public AzureResourceGroup SelectedResourceGroup
        {
            get => _selectedResourceGroup;
            set
            {
                _selectedResourceGroup = value;

                NotifyPropertyChanged("SelectedResourceGroup");
            }
        }

        /// <summary>
        /// Gets or sets the path to the ARM template file.
        /// </summary>
        public string ArmTempPath
        {
            get => _armTempPath;
            set
            {
                _armTempPath = value;

                NotifyPropertyChanged("ArmTempPath");
            }
        }

        /// <summary>
        /// Gets or sets the location of the Azure Resource Group to create.
        /// </summary>
        public string ResourceGroupLocation
        {
            get => _resourceGroupLocation;
            set
            {
                _resourceGroupLocation = value;

                NotifyPropertyChanged("ResourceGroupLocation");
            }
        }

        /// <summary>
        /// Gets or sets the name of the Azure Resource Group to create/deploy to.
        /// </summary>
        public string ResourceGroupName
        {
            get => _resourceGroupName;
            set
            {
                _resourceGroupName = value;

                NotifyPropertyChanged("ResourceGroupName");
            }
        }

        /// <summary>
        /// Gets or sets the configuration model for the AP Azure Function App resources.
        /// </summary>
        public FunctionAppConfiguration FunctionApp
        {
            get => _functionApp;
            set
            {
                _functionApp = value;

                NotifyPropertyChanged("FunctionApp");
            }
        }

        /// <summary>
        /// Gets or sets the configuration model for the AP Azure Storage account resource.
        /// </summary>
        public StorageAccountConfiguration StorageAccount
        {
            get => _storageAccount;
            set
            {
                _storageAccount = value;

                NotifyPropertyChanged("StorageAccount");
            }
        }

        /// <summary>
        /// Gets or sets the configuration model for the AP Azure Service Bus resource.
        /// </summary>
        public ServiceBusConfiguration ServiceBus
        {
            get => _serviceBus;
            set
            {
                _serviceBus = value;

                NotifyPropertyChanged("ServiceBus");
            }
        }

        /// <summary>
        /// Gets or sets the configuration model for the AP Azure Key Vault resource.
        /// </summary>
        public KeyVaultConfiguration KeyVault
        {
            get => _keyVault;
            set
            {
                _keyVault = value;

                NotifyPropertyChanged("KeyVault");
            }
        }

        /// <summary>
        /// Gets or sets the configuration model for the AP Azure Speech Cognitive Services resource.
        /// </summary>
        public SpeechCognitiveServiceConfiguration SpeechCognitiveService
        {
            get => _speechCognitiveService;
            set
            {
                _speechCognitiveService = value;

                NotifyPropertyChanged("SpeechCognitiveService");
            }
        }

        /// <summary>
        /// Gets or sets the configuration model for the AP LUIS Cognitive Services resource.
        /// </summary>
        public LuisConfiguration Luis
        {
            get => _luis;
            set
            {
                _luis = value;

                NotifyPropertyChanged("Luis");
            }
        }

        /// <summary>
        /// Gets or sets the configuration model for the AP Bing Maps resources.
        /// </summary>
        public BingMapsConfiguration BingMaps
        {
            get => _bingMaps;
            set
            {
                _bingMaps = value;

                NotifyPropertyChanged("BingMaps");
            }
        }

        /// <summary>
        /// Gets or sets the configuration model for the AP Azure Logic Apps resources.
        /// </summary>
        public LogicAppsConfiguration LogicApps
        {
            get => _logicApps;
            set
            {
                _logicApps = value;

                NotifyPropertyChanged("LogicApps");
            }
        }

        /// <summary>
        /// Gets or sets the configuration model for the AP Azure Management resources.
        /// </summary>
        public ManagementConfiguration Management
        {
            get => _management;
            set
            {
                _management = value;

                NotifyPropertyChanged("Management");
            }
        }

        /// <summary>
        /// Loads configuration from a file.
        /// </summary>
        public void LoadConfiguration()
        {
            ArmTemplateHelper.LoadArmTemplateParameters(AzureConfiguration.AzureArmTemplateParametersFilePath);

            FunctionApp.LoadConfiguration(AzureConfiguration.AzureArmTemplateParametersFilePath);
            StorageAccount.LoadConfiguration(AzureConfiguration.AzureArmTemplateParametersFilePath);
            ServiceBus.LoadConfiguration(AzureConfiguration.AzureArmTemplateParametersFilePath);
            KeyVault.LoadConfiguration(AzureConfiguration.AzureArmTemplateParametersFilePath);
            SpeechCognitiveService.LoadConfiguration(AzureConfiguration.AzureArmTemplateParametersFilePath);
            Luis.LoadConfiguration(AzureConfiguration.AzureArmTemplateParametersFilePath);
            BingMaps.LoadConfiguration(AzureConfiguration.AzureArmTemplateParametersFilePath);
            LogicApps.LoadConfiguration(AzureConfiguration.AzureArmTemplateParametersFilePath);
            Management.LoadConfiguration(AzureConfiguration.AzureArmTemplateParametersFilePath);

            ResourceGroupLocation = "westus2";
        }

        /// <summary>
        /// Saves configuration to a file.
        /// </summary>
        /// <param name="outputs">An array of files to save configuration information to.</param>
        public void SaveConfiguration(Dictionary<string, string> outputs)
        {
            string[] templates = outputs.Select(x => x.Key).ToArray();

            FunctionApp.SaveConfiguration(templates);
            StorageAccount.SaveConfiguration(templates);
            ServiceBus.SaveConfiguration(templates);
            KeyVault.SaveConfiguration(templates);
            SpeechCognitiveService.SaveConfiguration(templates);
            Luis.SaveConfiguration(templates);
            BingMaps.SaveConfiguration(templates);
            LogicApps.SaveConfiguration(templates);
            Management.SaveConfiguration(templates);

            foreach (KeyValuePair<string, string> output in outputs)
            {
                ArmTemplateHelper.SaveConfiguration(output.Key, output.Value);
            }
        }

        /// <summary>
        /// Clears all non-critical fields.
        /// </summary>
        public void ClearNonCriticalFields()
        {
            _selectedSubscription = null;
            _selectedResourceGroup = null;

            _functionApp.ClearNonCriticalFields();
            _storageAccount.ClearNonCriticalFields();
            _serviceBus.ClearNonCriticalFields();
            _keyVault.ClearNonCriticalFields();
            _speechCognitiveService.ClearNonCriticalFields();
            _luis.ClearNonCriticalFields();
            _bingMaps.ClearNonCriticalFields();
            _logicApps.ClearNonCriticalFields();
            _management.ClearNonCriticalFields();
        }
    }
}
