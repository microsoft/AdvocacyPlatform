// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.FunctionalTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Functional tests for installer ARM templates.
    /// </summary>
    [TestClass]
    public class ArmTemplateTests
    {
        private const string _resourceGroupName = "ap-prod-wu-rg";
        private const string _azureTemplateDirectory = @".\config";
        private static readonly string _azureDeployTemplateFilePath = $@"{_azureTemplateDirectory}\azuredeploy.json";
        private static readonly string _azureDeployTemplateParametersFilePath = $@"{_azureTemplateDirectory}\azuredeploy.parameters.json";
        private static readonly string _cdsLogicAppsTemplateFilePath = $@"{_azureTemplateDirectory}\cdsLogicApps.json";
        private static readonly string _cdsLogicAppsTemplateParametersFilePath = $@"{_azureTemplateDirectory}\cdsLogicApps.parameters.json";
        private static readonly string _apiLogicAppsTemplateFilePath = $@"{_azureTemplateDirectory}\apiLogicApps.json";
        private static readonly string _apiLogicAppsTemplateParametersFilePath = $@"{_azureTemplateDirectory}\apiLogicApps.parameters.json";
        private static readonly string _uiLogicAppsTemplateFilePath = $@"{_azureTemplateDirectory}\logicApps.json";
        private static readonly string _uiLogicAppsTemplateParametersFilePath = $@"{_azureTemplateDirectory}\logicApps.parameters.json";

        /// <summary>
        /// Test the primary ARM template is valid.
        /// </summary>
        [TestMethod]
        public void AzureDeployTemplateValid()
        {
            Assert.IsTrue(ArmTemplateTestHelper.ValidateTemplate(_resourceGroupName, _azureDeployTemplateFilePath, _azureDeployTemplateParametersFilePath));
        }

        /// <summary>
        /// Test the CDS ARM template is valid.
        /// </summary>
        [TestMethod]
        public void CdsLogicAppsTemplateValid()
        {
            Assert.IsTrue(ArmTemplateTestHelper.ValidateTemplate(_resourceGroupName, _cdsLogicAppsTemplateFilePath, _cdsLogicAppsTemplateParametersFilePath));
        }

        /// <summary>
        /// Test the UI ARM template is valid.
        /// </summary>
        [TestMethod]
        public void APILogicAppsTemplateValid()
        {
            Assert.IsTrue(ArmTemplateTestHelper.ValidateTemplate(_resourceGroupName, _apiLogicAppsTemplateFilePath, _apiLogicAppsTemplateParametersFilePath));
        }

        /// <summary>
        /// Test the UI Logic Apps ARM template is valid.
        /// </summary>
        [TestMethod]
        public void UILogicAppsTemplateValid()
        {
            Assert.IsTrue(ArmTemplateTestHelper.ValidateTemplate(_resourceGroupName, _uiLogicAppsTemplateFilePath, _uiLogicAppsTemplateParametersFilePath));
        }
    }
}
