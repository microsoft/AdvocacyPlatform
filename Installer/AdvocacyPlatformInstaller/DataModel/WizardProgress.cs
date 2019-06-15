// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Describes progress made through the installation process.
    /// </summary>
    public class WizardProgress : NotifyPropertyChangedBase
    {
        private ObservableCollection<string> _pages;
        private string _currentPage;

        /// <summary>
        /// Initializes a new instance of the <see cref="WizardProgress"/> class.
        /// </summary>
        public WizardProgress()
        {
            Pages = new ObservableCollection<string>();
        }

        /// <summary>
        /// Gets or sets a list of pages in the installation process.
        /// </summary>
        public ObservableCollection<string> Pages
        {
            get => _pages;
            set
            {
                _pages = value;

                NotifyPropertyChanged("Pages");
            }
        }

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        public string CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;

                NotifyPropertyChanged("CurrentPage");
                NotifyPropertyChanged("CurrentPageIndex");
            }
        }
    }
}
