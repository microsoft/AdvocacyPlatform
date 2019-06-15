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
    /// Specifies configuration information for AP Logic Apps.
    /// </summary>
    public class LogicAppsConfiguration : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Specifies the name of the AP Logic Apps ARM template file.
        /// </summary>
        public const string LogicAppsArmTemplateFileName = "logicApps.json";

        /// <summary>
        /// Specifies the name of the AP Logic Apps ARM template parameters file.
        /// </summary>
        public const string LogicAppsArmTemplateParametersFileName = "logicApps.parameters.json";

        /// <summary>
        /// Specifies the name of the AP CDS Logic Apps ARM template file.
        /// </summary>
        public const string CdsLogicAppsArmTemplateFileName = "cdsLogicApps.json";

        /// <summary>
        /// Specifies the name of the AP CDS Logic Apps ARM template parameters file.
        /// </summary>
        public const string CdsLogicAppsArmTemplateParametersFileName = @"cdsLogicApps.parameters.json";

        /// <summary>
        /// Specifies the name of the AP API Logic Apps ARM template file.
        /// </summary>
        public const string ApiLogicAppsArmTemplateFileName = "apiLogicApps.json";

        /// <summary>
        /// Specifies the name of the AP API Logic Apps ARM template parameters file.
        /// </summary>
        public const string ApiLogicAppsArmTemplateParametersFileName = "apiLogicApps.parameters.json";

        /// <summary>
        /// Specifies the path to the AP Logic Apps ARM template file.
        /// </summary>
        public static readonly string LogicAppsArmTemplateFilePath = $@".\config\{LogicAppsArmTemplateFileName}";

        /// <summary>
        /// Specifies the path to the AP Logic Apps ARM template parameters file.
        /// </summary>
        public static readonly string LogicAppsArmTemplateParametersFilePath = $@".\config\{LogicAppsArmTemplateParametersFileName}";

        /// <summary>
        /// Specifies the path to the AP CDS Logic Apps ARM template file.
        /// </summary>
        public static readonly string CdsLogicAppsArmTemplateFilePath = $@".\config\{CdsLogicAppsArmTemplateFileName}";

        /// <summary>
        /// Specifies the path to the AP CDS Logic Apps ARM template parameters file.
        /// </summary>
        public static readonly string CdsLogicAppsArmTemplateParametersFilePath = $@".\config\{CdsLogicAppsArmTemplateParametersFileName}";

        /// <summary>
        /// Specifies the path to the AP API Logic Apps ARM template file.
        /// </summary>
        public static readonly string ApiLogicAppsArmTemplateFilePath = $@".\config\{ApiLogicAppsArmTemplateFileName}";

        /// <summary>
        /// Specifies the path to the AP API Logic Apps ARM template parameters file.
        /// </summary>
        public static readonly string ApiLogicAppsArmTemplateParametersFilePath = $@".\config\{ApiLogicAppsArmTemplateParametersFileName}";

        private string _requestWorkflowName;
        private string _processWorkflowName;
        private string _newCaseWorkflowName;
        private string _resultsUpdateCaseWorkflowName;
        private string _addressUpdateCaseWorkflowName;
        private string _getRetryRecordsWorkflowName;

        private string _aadClientId;
        private string _aadAudience;

        private string _serviceBusConnectionName;
        private string _cdsConnectionName;
        private string _bingMapsConnectionName;

        private bool _deployLogicApps;
        private bool _authenticateLogicAppsCDSConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicAppsConfiguration"/> class.
        /// </summary>
        public LogicAppsConfiguration()
        {
            DeployLogicApps = true;
            AuthenticateLogicAppsCDSConnection = true;
        }

        /// <summary>
        /// Gets or sets the name of the request Logic App.
        /// </summary>
        public string RequestWorkflowName
        {
            get => _requestWorkflowName;
            set
            {
                _requestWorkflowName = value;

                NotifyPropertyChanged("RequestWorkflowName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the process Logic App resource.
        /// </summary>
        public string ProcessWorkflowName
        {
            get => _processWorkflowName;
            set
            {
                _processWorkflowName = value;

                NotifyPropertyChanged("ProcessWorkflowName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the new case Logic App resource.
        /// </summary>
        public string NewCaseWorkflowName
        {
            get => _newCaseWorkflowName;
            set
            {
                _newCaseWorkflowName = value;

                NotifyPropertyChanged("NewCaseWorkflowName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the update with results Logic App resource.
        /// </summary>
        public string ResultsUpdateCaseWorkflowName
        {
            get => _resultsUpdateCaseWorkflowName;
            set
            {
                _resultsUpdateCaseWorkflowName = value;

                NotifyPropertyChanged("ResultsUpdateCaseWorkflowName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the update with address updates Logic App resource.
        /// </summary>
        public string AddressUpdateCaseWorkflowName
        {
            get => _addressUpdateCaseWorkflowName;
            set
            {
                _addressUpdateCaseWorkflowName = value;

                NotifyPropertyChanged("AddressUpdateCaseWorkflowName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the retry records Logic App resource.
        /// </summary>
        public string GetRetryRecordsWorkflowName
        {
            get => _getRetryRecordsWorkflowName;
            set
            {
                _getRetryRecordsWorkflowName = value;

                NotifyPropertyChanged("GetRetryRecordsWorkflowName");
            }
        }

        /// <summary>
        /// Gets or sets the service principal client ID variable.
        /// </summary>
        public string AADClientId
        {
            get => _aadClientId;
            set
            {
                _aadClientId = value;

                NotifyPropertyChanged("AADClientId");
            }
        }

        /// <summary>
        /// Gets or sets the audience variable.
        /// </summary>
        public string AADAudience
        {
            get => _aadAudience;
            set
            {
                _aadAudience = value;

                NotifyPropertyChanged("AADAudience");
            }
        }

        /// <summary>
        /// Gets or sets the name of the service bus API connection resource.
        /// </summary>
        public string ServiceBusConnectionName
        {
            get => _serviceBusConnectionName;
            set
            {
                _serviceBusConnectionName = value;

                NotifyPropertyChanged("ServiceBusConnectionName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the CDS API connection resource.
        /// </summary>
        public string CdsConnectionName
        {
            get => _cdsConnectionName;
            set
            {
                _cdsConnectionName = value;

                NotifyPropertyChanged("CdsConnectionName");
            }
        }

        /// <summary>
        /// Gets or sets the name of the Bing Maps API connection resource.
        /// </summary>
        public string BingMapsConnectionName
        {
            get => _bingMapsConnectionName;
            set
            {
                _bingMapsConnectionName = value;

                NotifyPropertyChanged("BingMapsConnectionName");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether logic apps should be deployed.
        /// </summary>
        public bool DeployLogicApps
        {
            get => _deployLogicApps;
            set
            {
                _deployLogicApps = value;

                NotifyPropertyChanged("DeployLogicApps");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the logic apps CDS API Connection should be deployed.
        /// </summary>
        public bool AuthenticateLogicAppsCDSConnection
        {
            get => _authenticateLogicAppsCDSConnection;
            set
            {
                _authenticateLogicAppsCDSConnection = value;

                NotifyPropertyChanged("AuthenticateLogicAppsCDSConnection");
            }
        }

        /// <summary>
        /// Loads configuration from a file.
        /// </summary>
        /// <param name="armTemplateFilePath">Path to the configuration file.</param>
        public void LoadConfiguration(string armTemplateFilePath)
        {
            ArmTemplateHelper.LoadArmTemplateParameters(LogicAppsArmTemplateParametersFilePath);
            ArmTemplateHelper.LoadArmTemplateParameters(CdsLogicAppsArmTemplateParametersFilePath);
            ArmTemplateHelper.LoadArmTemplateParameters(ApiLogicAppsArmTemplateParametersFilePath);

            RequestWorkflowName = string.IsNullOrWhiteSpace(RequestWorkflowName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "workflows_ap_request_wu_logicapp_name") : RequestWorkflowName;
            ProcessWorkflowName = string.IsNullOrWhiteSpace(ProcessWorkflowName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "workflows_ap_process_wu_logicapp_name") : ProcessWorkflowName;
            NewCaseWorkflowName = string.IsNullOrWhiteSpace(NewCaseWorkflowName) ? ArmTemplateHelper.GetParameterValue(LogicAppsArmTemplateParametersFilePath, "newCase_logicAppName") : NewCaseWorkflowName;
            ResultsUpdateCaseWorkflowName = string.IsNullOrWhiteSpace(ResultsUpdateCaseWorkflowName) ? ArmTemplateHelper.GetParameterValue(LogicAppsArmTemplateParametersFilePath, "resultsUpdateCase_logicAppName") : ResultsUpdateCaseWorkflowName;
            AddressUpdateCaseWorkflowName = string.IsNullOrWhiteSpace(AddressUpdateCaseWorkflowName) ? ArmTemplateHelper.GetParameterValue(LogicAppsArmTemplateParametersFilePath, "addressUpdateCase_logicAppName") : AddressUpdateCaseWorkflowName;
            GetRetryRecordsWorkflowName = string.IsNullOrWhiteSpace(GetRetryRecordsWorkflowName) ? ArmTemplateHelper.GetParameterValue(LogicAppsArmTemplateParametersFilePath, "getRetryRecords_logicAppName") : GetRetryRecordsWorkflowName;

            AADClientId = ArmTemplateHelper.GetParameterValue(LogicAppsArmTemplateParametersFilePath, "aadclientid_variable");
            AADAudience = ArmTemplateHelper.GetParameterValue(LogicAppsArmTemplateParametersFilePath, "aadaudience_variable");

            ServiceBusConnectionName = string.IsNullOrWhiteSpace(ServiceBusConnectionName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "servicebus_Connection_Name") : ServiceBusConnectionName;
            CdsConnectionName = string.IsNullOrWhiteSpace(CdsConnectionName) ? ArmTemplateHelper.GetParameterValue(CdsLogicAppsArmTemplateParametersFilePath, "commondataservice_Connection_Name") : CdsConnectionName;
            BingMapsConnectionName = string.IsNullOrWhiteSpace(BingMapsConnectionName) ? ArmTemplateHelper.GetParameterValue(LogicAppsArmTemplateParametersFilePath, "bingmaps_Connection_Name") : BingMapsConnectionName;
        }

        /// <summary>
        /// Saves configuration to a file.
        /// </summary>
        /// <param name="armTemplateFilePaths">Path to save the configuration file.</param>
        public void SaveConfiguration(string[] armTemplateFilePaths)
        {
            foreach (string armTemplateFilePath in armTemplateFilePaths)
            {
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "workflows_ap_request_wu_logicapp_name", RequestWorkflowName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "workflows_ap_process_wu_logicapp_name", ProcessWorkflowName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "newCase_logicAppName", NewCaseWorkflowName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "resultsUpdateCase_logicAppName", ResultsUpdateCaseWorkflowName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "addressUpdateCase_logicAppName", AddressUpdateCaseWorkflowName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "getRetryRecords_logicAppName", GetRetryRecordsWorkflowName);

                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "aadclientid_variable", AADClientId);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "aadaudience_variable", AADAudience);

                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "servicebus_Connection_Name", ServiceBusConnectionName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "commondataservice_Connection_Name", CdsConnectionName);
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "bingmaps_Connection_Name", BingMapsConnectionName);
            }
        }

        /// <summary>
        /// Clears all non-critical fields.
        /// </summary>
        public void ClearNonCriticalFields()
        {
            // Currently no fields to clear
        }
    }
}
