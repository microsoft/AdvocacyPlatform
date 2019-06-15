// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.FunctionalTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    /// <summary>
    /// ARM template helper for functional tests.
    /// </summary>
    public static class ArmTemplateTestHelper
    {
        private const string _tempArmDir = @".\temp";

        /// <summary>
        /// Validates an ARM template.
        /// </summary>
        /// <param name="resourceGroupName">The name of the resource group to validate against.</param>
        /// <param name="templateFilePath">Path to the ARM template file to validate.</param>
        /// <param name="templateParametersFilePath">Path to the ARM template parameters file to use with validation.</param>
        /// <returns>True if valid, and false if invalid.</returns>
        public static bool ValidateTemplate(string resourceGroupName, string templateFilePath, string templateParametersFilePath)
        {
            if (!Directory.Exists(_tempArmDir))
            {
                Directory.CreateDirectory(_tempArmDir);
            }

            ArmTemplateHelper.LoadArmTemplateParameters(templateParametersFilePath);

            IEnumerable<string> parameterNames = ArmTemplateHelper.GetParameterNames(templateParametersFilePath);

            foreach (string parameterName in parameterNames)
            {
                ArmTemplateHelper.SetParameterValue(
                    templateParametersFilePath,
                    parameterName,
                    ArmTemplateHelper.GetParameterValue(
                        templateParametersFilePath,
                        parameterName));
            }

            string templateParameterFileName = Path.GetFileName(templateParametersFilePath);
            string tempParametersFilePath = $@"{_tempArmDir}\{templateParameterFileName}";
            ArmTemplateHelper.SaveConfiguration(templateParametersFilePath, tempParametersFilePath);

            using (PowerShell powerShellInstance = PowerShell.Create())
            {
                powerShellInstance.AddCommand("Test-AzResourceGroupDeployment");
                powerShellInstance.AddParameter("-ResourceGroupName", resourceGroupName);
                powerShellInstance.AddParameter("-TemplateFile", templateFilePath);
                powerShellInstance.AddParameter("-TemplateParameterFile", tempParametersFilePath);

                PSDataCollection<PSObject> outputCollection = new PSDataCollection<PSObject>();

                powerShellInstance.Streams.Error.DataAdded += Error_DataAdded;

                IAsyncResult result = powerShellInstance.BeginInvoke<PSObject, PSObject>(null, outputCollection);

                while (!result.IsCompleted)
                {
                    Debug.WriteLine("Waiting for pipeline to finish.");

                    Thread.Sleep(1000);
                }

                Debug.WriteLine("Execution has stopped. The pipeline state: " + powerShellInstance.InvocationStateInfo.State);

                string serializedOutput = PSSerializer.Serialize(outputCollection);

                XNamespace ns = "http://schemas.microsoft.com/powershell/2004/04";
                XDocument output = XDocument.Parse(serializedOutput);

                List<XElement> messageElements = output.Root.Descendants(ns + "S")
                    .Where(x => x.Attribute("N") != null &&
                                x.Attribute("N").Value == "Message")
                    .ToList();

                foreach (XElement messageElement in messageElements)
                {
                    Debug.WriteLine($"ERROR: {messageElement.Value}");
                }

                if (messageElements.Count() != 0)
                {
                    return false;
                }

                if (powerShellInstance.Streams.Error.Count() > 0)
                {
                    return false;
                }

                return true;
            }
        }

        private static void Error_DataAdded(object sender, DataAddedEventArgs e)
        {
            PSDataCollection<ErrorRecord> records = (PSDataCollection<ErrorRecord>)sender;

            Debug.WriteLine($"ERROR: {records[e.Index].ToString()}");
        }
    }
}
