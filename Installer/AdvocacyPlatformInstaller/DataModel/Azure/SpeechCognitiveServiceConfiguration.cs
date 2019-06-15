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
    /// Configuration for the Azure Speech Cognitive Services resource.
    /// </summary>
    public class SpeechCognitiveServiceConfiguration : NotifyPropertyChangedBase
    {
        private string _speechResourceName;
        private string _speechApiKey;

        /// <summary>
        /// Gets or sets the name of the Azure Speech Cognitive Services resource.
        /// </summary>
        public string SpeechResourceName
        {
            get => _speechResourceName;
            set
            {
                _speechResourceName = value;

                NotifyPropertyChanged("SpeechResourceName");
            }
        }

        /// <summary>
        /// Gets or sets the API key for the Azure Speech Cognitive Services resource.
        /// </summary>
        public string SpeechApiKey
        {
            get => _speechApiKey;
            set
            {
                _speechApiKey = value;

                NotifyPropertyChanged("SpeechApiKey");
            }
        }

        /// <summary>
        /// Loads configuration from file.
        /// </summary>
        /// <param name="armTemplateFilePath">Path to the configuration file.</param>
        public void LoadConfiguration(string armTemplateFilePath)
        {
            SpeechResourceName = string.IsNullOrWhiteSpace(SpeechResourceName) ? ArmTemplateHelper.GetParameterValue(armTemplateFilePath, "accounts_ap_wu_speech_cognitivesvc_name") : SpeechResourceName;
        }

        /// <summary>
        /// Saves configuration to a set of files.
        /// </summary>
        /// <param name="armTemplateFilePaths">Array of file paths to save configuration to.</param>
        public void SaveConfiguration(string[] armTemplateFilePaths)
        {
            foreach (string armTemplateFilePath in armTemplateFilePaths)
            {
                ArmTemplateHelper.SetParameterValue(armTemplateFilePath, "accounts_ap_wu_speech_cognitivesvc_name", SpeechResourceName);
            }
        }

        /// <summary>
        /// Clears all non-critical fields.
        /// </summary>
        public void ClearNonCriticalFields()
        {
            // Currently nothing to clear
        }
    }
}
