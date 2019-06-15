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
    /// User control for the installer welcome page.
    /// </summary>
    public partial class WelcomeInstallationControl : WizardPageBase
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public const string PageName = "Welcome";

        /// <summary>
        /// A description for the page.
        /// </summary>
        public const string PageDescription = "Welcome page";

        /// <summary>
        /// Initializes a new instance of the <see cref="WelcomeInstallationControl"/> class.
        /// </summary>
        /// <param name="model">The view model to bind to this control.</param>
        /// <param name="context">The wizard context instance.</param>
        public WelcomeInstallationControl(
            InstallerModel model,
            InstallerWizard context)
            : base(model, context)
        {
            InitializeComponent();

            DataModel.NextEnabled = true;
            DataModel.InstallerAction = InstallerActionType.New;

            WizardContext.LoadPagesCheckpoint();
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
        /// Event handler for the Next button.
        /// </summary>
        /// <param name="context">The wizard context instance.</param>
        public override void HandleNext(InstallerWizard context)
        {
            bool continueAction = true;

            DataModel.InstallationConfiguration.Features.Features = new List<Feature>();

            switch (DataModel.InstallerAction)
            {
                case InstallerActionType.New:
                    BuildNewInstallationFeatureTree();
                    break;

                case InstallerActionType.Modify:
                    continueAction = GetConfiguration();
                    BuildUpdateFeatureTree();
                    break;

                case InstallerActionType.Remove:
                    continueAction = GetConfiguration();
                    SetUninstallMode(context);
                    break;
            }

            if (continueAction)
            {
                context.NextPage();
            }
        }

        private bool GetConfiguration()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();

            ofd.Title = "Please specify the installation configuration to load...";

            ofd.Filter = "json files (*.json)|*.json";

            if (ofd.ShowDialog() == true)
            {
                DataModel.InstallationConfiguration = InstallationConfiguration.LoadConfiguration(ofd.FileName);
            }
            else
            {
                MessageBox.Show("This operation cannot be completed without loading an installation configuration file. The installer will now exit.");

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Application.Current.MainWindow.Close();
                }));

                return false;
            }

            return true;
        }

        private void BuildNewInstallationFeatureTree()
        {
            DataModel.InstallationConfiguration.Azure.FunctionApp.CollectSecrets = true;
            DataModel.InstallationConfiguration.Azure.KeyVault.CollectSecrets = true;
            DataModel.InstallationConfiguration.Features.Features.Clear();

            Feature uiComponents = new Feature()
            {
                Name = FeatureNames.UIComponents,
                ShouldInstall = true,
            };

            Feature powerAppsDynamics365CRMFeature = new Feature()
            {
                Name = FeatureNames.PowerAppsDynamics365CRM,
                ShouldInstall = true,
            };

            powerAppsDynamics365CRMFeature.AddFeatures(new List<Feature>()
            {
                new Feature()
                {
                    Name = FeatureNames.PowerAppsEnvironment,
                    ShouldInstall = true,
                    Pages = new List<InstallerPageDescriptor>()
                    {
                        new InstallerPageDescriptor(typeof(PowerAppsInstallationControl), PowerAppsInstallationControl.PageName, PowerAppsInstallationControl.PageDescription, false, false),
                        new InstallerPageDescriptor(typeof(DeployPowerAppsInstallationControl), DeployPowerAppsInstallationControl.PageName, DeployPowerAppsInstallationControl.PageDescription, false, true),
                    },
                },
                new Feature()
                {
                    Name = FeatureNames.CommonDataServiceDatabase,
                    ShouldInstall = true,
                    Dependencies = new HashSet<string>()
                    {
                        $"{FeatureNames.UIComponents}\\{FeatureNames.PowerAppsDynamics365CRM}\\{FeatureNames.PowerAppsEnvironment}",
                    },
                },
                new Feature()
                {
                    Name = FeatureNames.APDynamics365Solution,
                    ShouldInstall = true,
                    Pages = new List<InstallerPageDescriptor>()
                    {
                        new InstallerPageDescriptor(typeof(DynamicsCRMSolutionInstallationControl), DynamicsCRMSolutionInstallationControl.PageName, DynamicsCRMSolutionInstallationControl.PageDescription, false, false),
                        new InstallerPageDescriptor(typeof(DeployDynamicsCRMSolutionInstallationControl), DeployDynamicsCRMSolutionInstallationControl.PageName, DeployDynamicsCRMSolutionInstallationControl.PageDescription, false, true),
                    },
                },
            });

            Feature uiAzureResourcesFeature = new Feature()
            {
                Name = FeatureNames.AzureResources,
                ShouldInstall = true,
                Pages = new List<InstallerPageDescriptor>()
                {
                    new InstallerPageDescriptor(typeof(AzureInstallationControl), AzureInstallationControl.PageName, AzureInstallationControl.PageDescription, false, false),
                },
            };

            uiAzureResourcesFeature.AddFeature(new Feature()
            {
                Name = FeatureNames.LogicApps,
                ShouldInstall = true,
                Dependencies = new HashSet<string>()
                {
                    $"{FeatureNames.UIComponents}\\{FeatureNames.PowerAppsDynamics365CRM}\\{FeatureNames.APDynamics365Solution}",
                    $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                },
                Pages = new List<InstallerPageDescriptor>()
                {
                    new InstallerPageDescriptor(typeof(AzureLogicAppsInstallationControl), AzureLogicAppsInstallationControl.PageName, AzureLogicAppsInstallationControl.PageDescription, false, false),
                },
            });

            uiComponents.AddFeatures(new List<Feature>()
            {
                powerAppsDynamics365CRMFeature,
                uiAzureResourcesFeature,
            });

            Feature apiComponents = new Feature()
            {
                Name = FeatureNames.APIComponents,
                ShouldInstall = true,
            };

            Feature apiAzureResources = new Feature()
            {
                Name = FeatureNames.AzureResources,
                ShouldInstall = true,
                Dependencies = new HashSet<string>()
                {
                    $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                },
                Pages = new List<InstallerPageDescriptor>()
                {
                    new InstallerPageDescriptor(typeof(InstallerConfirmActionsInstallationControl), InstallerConfirmActionsInstallationControl.PageName, InstallerConfirmActionsInstallationControl.PageDescription, false, true),
                    new InstallerPageDescriptor(typeof(AzureDeployInstallationControl), AzureDeployInstallationControl.PageName, AzureDeployInstallationControl.PageDescription, false, true),
                },
            };

            apiAzureResources.AddFeatures(new List<Feature>()
            {
                new Feature()
                {
                    Name = FeatureNames.AzureADAppRegistration,
                    ShouldInstall = true,
                    Dependencies = new HashSet<string>()
                    {
                        $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                        $"{FeatureNames.APIComponents}\\{FeatureNames.AzureResources}\\{FeatureNames.AzureFunctionAppAuthentication}",
                    },
                },
                new Feature()
                {
                    Name = FeatureNames.AzureRGDeployment,
                    ShouldInstall = true,
                    Dependencies = new HashSet<string>()
                    {
                        $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                    },
                },
                new Feature()
                {
                    Name = FeatureNames.AzureFunctionAppAuthentication,
                    ShouldInstall = true,
                    Pages = new List<InstallerPageDescriptor>()
                    {
                        new InstallerPageDescriptor(typeof(AzureFunctionAppInstallationControl), AzureFunctionAppInstallationControl.PageName, AzureFunctionAppInstallationControl.PageDescription, false, false),
                    },
                    Dependencies = new HashSet<string>()
                    {
                        $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                    },
                },
                new Feature()
                {
                    Name = FeatureNames.AzureKeyVault,
                    ShouldInstall = true,
                    Pages = new List<InstallerPageDescriptor>()
                    {
                        new InstallerPageDescriptor(typeof(AzureKeyVaultInstallationControl), AzureKeyVaultInstallationControl.PageName, AzureKeyVaultInstallationControl.PageDescription, false, false),
                    },
                    Dependencies = new HashSet<string>()
                    {
                        $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                    },
                },
                new Feature()
                {
                    Name = FeatureNames.AzureStorageAccessPolicies,
                    ShouldInstall = true,
                    Dependencies = new HashSet<string>()
                    {
                        $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                    },
                },
                new Feature()
                {
                    Name = FeatureNames.AzureLanguageUnderstandingModel,
                    ShouldInstall = true,
                    Pages = new List<InstallerPageDescriptor>()
                    {
                        new InstallerPageDescriptor(typeof(AzureLuisModelInstallationControl), AzureLuisModelInstallationControl.PageName, AzureLuisModelInstallationControl.PageDescription, false, false),
                    },
                    Dependencies = new HashSet<string>()
                    {
                        $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                    },
                },
            });

            apiComponents.AddFeature(apiAzureResources);

            DataModel.InstallationConfiguration.Features.Features.Add(uiComponents);
            DataModel.InstallationConfiguration.Features.Features.Add(apiComponents);
        }

        private void BuildUpdateFeatureTree()
        {
            DataModel.InstallationConfiguration.Features.Features.Clear();

            Feature uiComponents = new Feature()
            {
                Name = FeatureNames.UIComponents,
                ShouldInstall = true,
            };

            Feature powerAppsDynamics365CRMFeature = new Feature()
            {
                Name = FeatureNames.PowerAppsDynamics365CRM,
                ShouldInstall = true,
            };

            powerAppsDynamics365CRMFeature.AddFeatures(new List<Feature>()
            {
                new Feature()
                {
                    Name = FeatureNames.APDynamics365Solution,
                    ShouldInstall = true,
                    Pages = new List<InstallerPageDescriptor>()
                    {
                        new InstallerPageDescriptor(typeof(DynamicsCRMSolutionInstallationControl), DynamicsCRMSolutionInstallationControl.PageName, DynamicsCRMSolutionInstallationControl.PageDescription, false, false),
                        new InstallerPageDescriptor(typeof(UpdateDynamicsCRMSolutionInstallationControl), DynamicsCRMSolutionInstallationControl.PageName, UpdateDynamicsCRMSolutionInstallationControl.PageDescription, false, true),
                    },
                },
            });

            Feature uiAzureResourcesFeature = new Feature()
            {
                Name = FeatureNames.AzureResources,
                ShouldInstall = true,
                Pages = new List<InstallerPageDescriptor>()
                {
                    new InstallerPageDescriptor(typeof(AzureInstallationControl), AzureInstallationControl.PageName, AzureInstallationControl.PageDescription, false, false),
                },
            };

            uiAzureResourcesFeature.AddFeature(new Feature()
            {
                Name = FeatureNames.LogicApps,
                ShouldInstall = true,
                Dependencies = new HashSet<string>()
                {
                    $"{FeatureNames.UIComponents}\\{FeatureNames.PowerAppsDynamics365CRM}\\{FeatureNames.APDynamics365Solution}",
                    $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                },
                Pages = new List<InstallerPageDescriptor>()
                {
                    new InstallerPageDescriptor(typeof(AzureLogicAppsInstallationControl), AzureLogicAppsInstallationControl.PageName, AzureLogicAppsInstallationControl.PageDescription, false, false),
                    new InstallerPageDescriptor(typeof(AzureKeyVaultInstallationControl), AzureKeyVaultInstallationControl.PageName, AzureKeyVaultInstallationControl.PageDescription, false, false),
                },
            });

            uiComponents.AddFeatures(new List<Feature>()
            {
                powerAppsDynamics365CRMFeature,
                uiAzureResourcesFeature,
            });

            Feature apiComponents = new Feature()
            {
                Name = FeatureNames.APIComponents,
                ShouldInstall = true,
            };

            Feature apiAzureResources = new Feature()
            {
                Name = FeatureNames.AzureResources,
                ShouldInstall = true,
                Dependencies = new HashSet<string>()
                {
                    $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                },
                Pages = new List<InstallerPageDescriptor>()
                {
                    new InstallerPageDescriptor(typeof(InstallerConfirmActionsInstallationControl), InstallerConfirmActionsInstallationControl.PageName, InstallerConfirmActionsInstallationControl.PageDescription, false, true),
                    new InstallerPageDescriptor(typeof(AzureUpdateInstallationControl), AzureUpdateInstallationControl.PageName, AzureUpdateInstallationControl.PageDescription, false, true),
                },
            };

            apiAzureResources.AddFeatures(new List<Feature>()
            {
                new Feature()
                {
                    Name = FeatureNames.APIFunctions,
                    ShouldInstall = true,
                    Pages = new List<InstallerPageDescriptor>()
                    {
                        new InstallerPageDescriptor(typeof(AzureKeyVaultInstallationControl), AzureKeyVaultInstallationControl.PageName, AzureKeyVaultInstallationControl.PageDescription, false, false),
                        new InstallerPageDescriptor(typeof(AzureFunctionAppInstallationControl), AzureFunctionAppInstallationControl.PageName, AzureFunctionAppInstallationControl.PageDescription, false, false),
                    },
                    Dependencies = new HashSet<string>()
                    {
                        $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                    },
                },
                new Feature()
                {
                    Name = FeatureNames.LogicApps,
                    ShouldInstall = true,
                    Dependencies = new HashSet<string>()
                    {
                        $"{FeatureNames.UIComponents}\\{FeatureNames.PowerAppsDynamics365CRM}\\{FeatureNames.APDynamics365Solution}",
                        $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                    },
                    Pages = new List<InstallerPageDescriptor>()
                    {
                        new InstallerPageDescriptor(typeof(AzureKeyVaultInstallationControl), AzureKeyVaultInstallationControl.PageName, AzureKeyVaultInstallationControl.PageDescription, false, false),
                        new InstallerPageDescriptor(typeof(AzureLogicAppsInstallationControl), AzureLogicAppsInstallationControl.PageName, AzureLogicAppsInstallationControl.PageDescription, false, false),
                    },
                },
                new Feature()
                {
                    Name = FeatureNames.AzureLanguageUnderstandingModel,
                    ShouldInstall = true,
                    Pages = new List<InstallerPageDescriptor>()
                    {
                        new InstallerPageDescriptor(typeof(AzureLuisModelInstallationControl), AzureLuisModelInstallationControl.PageName, AzureLuisModelInstallationControl.PageDescription, false, false),
                    },
                    Dependencies = new HashSet<string>()
                    {
                        $"{FeatureNames.UIComponents}\\{FeatureNames.AzureResources}",
                    },
                },
            });

            apiComponents.AddFeature(apiAzureResources);

            DataModel.InstallationConfiguration.Features.Features.Add(uiComponents);
            DataModel.InstallationConfiguration.Features.Features.Add(apiComponents);
        }

        private void SetUninstallMode(InstallerWizard wizard)
        {
            DataModel.InstallationConfiguration.Azure.ShouldDelete = true;
            DataModel.InstallationConfiguration.Azure.ShouldDeleteAppRegistration = true;
            DataModel.InstallationConfiguration.Azure.Luis.ShouldDelete = true;
            DataModel.InstallationConfiguration.PowerApps.ShouldDelete = true;

            wizard.RemovePage(typeof(FeatureSelectionInstallationControl));
            wizard.AddPage(typeof(UninstallInstallationControl), UninstallInstallationControl.PageName, UninstallInstallationControl.PageDescription, false, false);
            wizard.AddPage(typeof(InstallerConfirmActionsInstallationControl), "Confirm Uninstall", "Confirm the removal of resources.", false, true);
            wizard.AddPage(typeof(UninstallRemoveInstallationControl), UninstallRemoveInstallationControl.PageName, UninstallRemoveInstallationControl.PageDescription, false, true);
            wizard.AddPage(typeof(InstallerCompleteInstallationControl), InstallerCompleteInstallationControl.PageName, InstallerCompleteInstallationControl.PageDescription, true, false);
        }

        private void DeployAzureResourcesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(DataModel.InstallationConfiguration.Azure.ArmTempPath))
            {
                Directory.CreateDirectory(DataModel.InstallationConfiguration.Azure.ArmTempPath);
            }

            File.Copy(
                System.IO.Path.Combine(@".\", AzureConfiguration.AzureArmTemplateFilePath),
                System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, System.IO.Path.GetFileName(AzureConfiguration.AzureArmTemplateFilePath)),
                true);

            DataModel.InstallationConfiguration.Azure.SaveConfiguration(new Dictionary<string, string>()
            {
                { AzureConfiguration.AzureArmTemplateParametersFilePath, System.IO.Path.Combine(DataModel.InstallationConfiguration.Azure.ArmTempPath, AzureConfiguration.AzureArmTemplateParametersFilePath) },
            });
        }
    }
}
