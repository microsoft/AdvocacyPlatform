// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using AdvocacyPlatformInstaller.Clients;
    using AdvocacyPlatformInstaller.Contracts;

    /// <summary>
    /// Installation wizard.
    /// </summary>
    public class InstallerWizard
    {
        private const string _logFilePath = @".\installer.log";
        private const string _clientIdAppSettingsKey = "clientId";
        private const string _redirectUriAppSettingsKey = "redirectUri";

        private StreamWriter _logFileStream;

        private Grid _contentControl;
        private InstallerModel _dataModel;
        private List<InstallerPageDescriptor> _pages;
        private List<InstallerPageDescriptor> _pagesCheckpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallerWizard"/> class.
        /// </summary>
        /// <param name="uiContext">The owner of this object.</param>
        /// <param name="contentControl">Grid control to place content in.</param>
        /// <param name="dataModel">The UI view model.</param>
        /// <param name="noLogging">Specifies whether logging should occur.</param>
        public InstallerWizard(
            MainWindow uiContext,
            Grid contentControl,
            InstallerModel dataModel,
            bool noLogging = false)
        {
            _contentControl = contentControl;
            _dataModel = dataModel;
            _pages = new List<InstallerPageDescriptor>();

            TokenProvider = new TokenProvider(
                ConfigurationManager.AppSettings[_clientIdAppSettingsKey],
                ConfigurationManager.AppSettings[_redirectUriAppSettingsKey],
                null,
                uiContext);

            string directoryPath = Path.GetDirectoryName(_logFilePath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(_logFilePath);
            }

            if (!noLogging)
            {
                _logFileStream = new StreamWriter(_logFilePath);
                _logFileStream.AutoFlush = true;
            }
        }

        /// <summary>
        /// Event fired when the Next button is clicked.
        /// </summary>
        public event EventHandler OnNext;

        /// <summary>
        /// Event fired when the Previous button is clicked.
        /// </summary>
        public event EventHandler OnPrevious;

        /// <summary>
        /// Event fired when the Cancel button is clicked.
        /// </summary>
        public event EventHandler OnCancel;

        /// <summary>
        /// Event fired when the Finish button is clicked.
        /// </summary>
        public event EventHandler OnFinish;

        /// <summary>
        /// Gets or sets the current page index.
        /// </summary>
        public int CurrentPageIndex { get; set; }

        /// <summary>
        /// Gets or sets the token provider.
        /// </summary>
        public ITokenProvider TokenProvider { get; set; }

        /// <summary>
        /// Gets the logging stream.
        /// </summary>
        public StreamWriter LogFileStream => _logFileStream;

        /// <summary>
        /// Checks if a page of the type specified exists in the page list.
        /// </summary>
        /// <param name="pageType">The type of page class.</param>
        /// <returns>True if exists, false if not.</returns>
        public bool HasPageType(Type pageType)
        {
            return _pages
                .Where(x => x.Type == pageType)
                .FirstOrDefault() != null;
        }

        /// <summary>
        /// Adds a page to the page list.
        /// </summary>
        /// <param name="pageType">The type of page class.</param>
        /// <param name="pageName">The name of the page.</param>
        /// <param name="pageDescription">A description of the page.</param>
        /// <param name="isFinishPage">Specifies if the page is the last page in the process.</param>
        /// <param name="isDeploymentPage">Specifies if the page performs deployment operations.</param>
        /// <returns>The InstallerWizard instance the page was added to.</returns>
        public InstallerWizard AddPage(Type pageType, string pageName, string pageDescription, bool isFinishPage, bool isDeploymentPage)
        {
            _pages.Add(
                new InstallerPageDescriptor(
                    pageType,
                    pageName,
                    pageDescription,
                    isFinishPage,
                    isDeploymentPage));

            if (!string.IsNullOrWhiteSpace(pageName))
            {
                _dataModel.Progress.Pages.Add(pageName);
            }

            return this;
        }

        /// <summary>
        /// Builds the InstallerWizard based on a set of features.
        /// </summary>
        /// <param name="feature">The root feature.</param>
        /// <param name="features">The feature tree.</param>
        public void BuildWizard(Feature feature, FeatureTree features)
        {
            foreach (Feature childFeature in feature.Features.Where(x => x.ShouldInstall))
            {
                BuildWizard(childFeature, features);
            }

            if (feature.ShouldInstall)
            {
                foreach (string dependency in feature.Dependencies)
                {
                    Feature dependencyFeature = features[dependency];

                    if (dependencyFeature != null)
                    {
                        BuildWizardProcessDependency(dependencyFeature, features);
                    }
                }

                foreach (InstallerPageDescriptor page in feature.Pages)
                {
                    if (!HasPageType(page.Type))
                    {
                        AddPage(page.Type, page.Name, page.Description, page.IsFinishPage, page.IsDeploymentPage);
                    }
                }
            }
        }

        /// <summary>
        /// Removes a page from the page list.
        /// </summary>
        /// <param name="pageType">The type of page to remove.</param>
        public void RemovePage(Type pageType)
        {
            InstallerPageDescriptor page = _pages
                .Where(x => x.Type == pageType)
                .First();

            _pages.Remove(page);

            if (!string.IsNullOrWhiteSpace(page.Name))
            {
                _dataModel.Progress.Pages.Remove(page.Name);
            }
        }

        /// <summary>
        /// Creates a page checkpoint.
        /// </summary>
        public void SetPagesCheckpoint()
        {
            _pagesCheckpoint = new List<InstallerPageDescriptor>(_pages);
        }

        /// <summary>
        /// Loads a page checkpoint.
        /// </summary>
        public void LoadPagesCheckpoint()
        {
            _pages = new List<InstallerPageDescriptor>(_pagesCheckpoint);

            _dataModel.Progress.Pages.Clear();

            foreach (string pageName in _pages.Where(x => x.Name != null).Select(x => x.Name))
            {
                _dataModel.Progress.Pages.Add(pageName);
            }
        }

        /// <summary>
        /// Starts the installer wizard.
        /// </summary>
        public void Start()
        {
            CurrentPageIndex = 0;

            SetPage();
        }

        /// <summary>
        /// Jumps to a specific page in the page list.
        /// </summary>
        /// <param name="pageType">The type of page to jump to.</param>
        public void JumpToPage(Type pageType)
        {
            InstallerPageDescriptor findPage = _pages
                .Where(x => x.Type == pageType)
                .FirstOrDefault();

            if (findPage == null)
            {
                return;
            }

            CurrentPageIndex = _pages.IndexOf(findPage);

            SetPage();
        }

        /// <summary>
        /// Fire the OnNext event.
        /// </summary>
        public void NotifyNextPage()
        {
            if (OnNext != null)
            {
                OnNext(this, new EventArgs());
            }
        }

        /// <summary>
        /// Fire the OnPrevious event.
        /// </summary>
        public void NotifyPreviousPage()
        {
            if (OnPrevious != null)
            {
                OnPrevious(this, new EventArgs());
            }
        }

        /// <summary>
        /// Fire the OnFinish event.
        /// </summary>
        public void NotifyFinish()
        {
            if (OnFinish != null)
            {
                OnFinish(this, new EventArgs());
            }
        }

        /// <summary>
        /// Fire the OnCancel event.
        /// </summary>
        public void NotifyCancel()
        {
            if (OnCancel != null)
            {
                OnCancel(this, new EventArgs());
            }
        }

        /// <summary>
        /// Navigates to the next page in the page list.
        /// </summary>
        public void NextPage()
        {
            if (CurrentPageIndex < _pages.Count())
            {
                CurrentPageIndex++;
            }
            else
            {
                return;
            }

            SetPage();
        }

        /// <summary>
        /// Clears event handlers.
        /// </summary>
        public void ClearNotify()
        {
            OnPrevious = null;
            OnNext = null;
            OnCancel = null;
            OnFinish = null;
        }

        /// <summary>
        /// Navigates to the previous page in the page list.
        /// </summary>
        public void PreviousPage()
        {
            if (CurrentPageIndex > 0)
            {
                CurrentPageIndex--;
            }
            else
            {
                return;
            }

            if (_pages[CurrentPageIndex].IsDeploymentPage)
            {
                PreviousPage();
            }
            else
            {
                SetPage();
            }
        }

        private void BuildWizardProcessDependency(Feature dependencyFeature, FeatureTree features)
        {
            foreach (string childDependencyFeatureName in dependencyFeature.Dependencies)
            {
                Feature childDependencyFeature = features[childDependencyFeatureName];

                if (childDependencyFeature != null)
                {
                    BuildWizardProcessDependency(childDependencyFeature, features);
                }
            }

            foreach (InstallerPageDescriptor featurePage in dependencyFeature.Pages.Where(x => !x.IsDeploymentPage))
            {
                if (!HasPageType(featurePage.Type))
                {
                    AddPage(featurePage.Type, featurePage.Name, featurePage.Description, featurePage.IsFinishPage, featurePage.IsDeploymentPage);
                }
            }
        }

        private void SetPage()
        {
            ClearNotify();

            InstallerPageDescriptor page = _pages[CurrentPageIndex];

            if (page.IsFinishPage)
            {
                _dataModel.ShowCancel = false;
                _dataModel.ShowNext = false;
                _dataModel.ShowFinish = true;
            }
            else
            {
                _dataModel.ShowCancel = true;
                _dataModel.ShowNext = true;
                _dataModel.ShowFinish = false;
            }

            if (CurrentPageIndex > 0)
            {
                _dataModel.ShowPrevious = true;
            }
            else
            {
                _dataModel.ShowPrevious = false;
            }

            object control = Activator.CreateInstance(page.Type, new object[] { _dataModel, this });

            if (control == null)
            {
                throw new Exception("Could not create wizard page!");
            }

            _contentControl.Children.Clear();
            _contentControl.Children.Add((UIElement)control);

            if (!string.IsNullOrWhiteSpace(page.Name))
            {
                _dataModel.ShowDescription = true;
                _dataModel.Page.PageName = page.Name;
                _dataModel.Page.PageDescription = page.Description;
            }
            else
            {
                _dataModel.ShowDescription = false;
                _dataModel.Page.PageName = _dataModel.Page.PageDescription = null;
            }

            _dataModel.Progress.CurrentPage = page.Name;
        }
    }
}
