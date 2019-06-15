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
    /// Describes a page in the installation wizard.
    /// </summary>
    public class WizardPage : NotifyPropertyChangedBase
    {
        private string _pageName;
        private string _pageDescription;

        /// <summary>
        /// Gets or sets the name of the page.
        /// </summary>
        public string PageName
        {
            get => _pageName;
            set
            {
                _pageName = value;

                NotifyPropertyChanged("PageName");
            }
        }

        /// <summary>
        /// Gets or sets a description of the page.
        /// </summary>
        public string PageDescription
        {
            get => _pageDescription;
            set
            {
                _pageDescription = value;

                NotifyPropertyChanged("PageDescription");
            }
        }
    }
}
