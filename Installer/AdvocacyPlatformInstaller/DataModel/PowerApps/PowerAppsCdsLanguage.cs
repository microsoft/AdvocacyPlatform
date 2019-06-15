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
    /// View model representing a PowerApps CDS database language.
    /// </summary>
    public class PowerAppsCdsLanguage : NotifyPropertyChangedBase
    {
        private string _languageName;
        private string _languageDisplayName;

        /// <summary>
        /// Gets or sets the name of the language.
        /// </summary>
        public string LanguageName
        {
            get => _languageName;
            set
            {
                _languageName = value;

                NotifyPropertyChanged("LanguageName");
            }
        }

        /// <summary>
        /// Gets or sets the display name for the language.
        /// </summary>
        public string LanguageDisplayName
        {
            get => _languageDisplayName;
            set
            {
                _languageDisplayName = value;

                NotifyPropertyChanged("LanguageDisplayName");
            }
        }

        /// <summary>
        /// Gets the language name and display name.
        /// </summary>
        /// <returns>The language name and display name formatted as {LanguageName} ({LanguageDisplayName}).</returns>
        public override string ToString()
        {
            return $"{LanguageName} ({LanguageDisplayName})";
        }
    }
}
