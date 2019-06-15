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
    /// Model representing a feature to install, modify, or remove.
    /// </summary>
    public class Feature : NotifyPropertyChangedBase
    {
        private bool _shouldInstall;
        private string _name;
        private HashSet<string> _dependencies;
        private List<InstallerPageDescriptor> _pages;
        private List<Feature> _features;
        private Feature _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="Feature"/> class.
        /// </summary>
        public Feature()
        {
            Features = new List<Feature>();
            Dependencies = new HashSet<string>();
            Pages = new List<InstallerPageDescriptor>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the feature should be installed, modified, or removed.
        /// </summary>
        public bool ShouldInstall
        {
            get => _shouldInstall;
            set
            {
                SetShouldInstall(value, true);
            }
        }

        /// <summary>
        /// Gets a value specifying the count of features to install.
        /// </summary>
        public int ShouldInstallCount
        {
            get
            {
                int shouldInstallCount = 0;

                IEnumerable<Feature> shouldInstall = Features.Where(x => x.ShouldInstall);

                foreach (Feature feature in shouldInstall)
                {
                    shouldInstallCount += feature.ShouldInstallCount;
                }

                shouldInstallCount += shouldInstall.Count();

                return shouldInstallCount;
            }
        }

        /// <summary>
        /// Gets or sets the name of the feature.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;

                NotifyPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets a hash table representing the feature dependencies.
        /// </summary>
        public HashSet<string> Dependencies
        {
            get => _dependencies;
            set
            {
                _dependencies = value;

                NotifyPropertyChanged("Dependencies");
            }
        }

        /// <summary>
        /// Gets or sets a list of pages to show in the UI for this feature.
        /// </summary>
        public List<InstallerPageDescriptor> Pages
        {
            get => _pages;
            set
            {
                _pages = value;

                NotifyPropertyChanged("Pages");
            }
        }

        /// <summary>
        /// Gets or sets a list of child features parented to this feature.
        /// </summary>
        public List<Feature> Features
        {
            get => _features;
            set
            {
                _features = value;

                NotifyPropertyChanged("Features");
            }
        }

        /// <summary>
        /// Gets or sets the parent for this feature.
        /// </summary>
        public Feature Parent
        {
            get => _parent;
            set
            {
                _parent = value;

                NotifyPropertyChanged("Parent");
            }
        }

        /// <summary>
        /// Adds a child feature.
        /// </summary>
        /// <param name="newFeature">The child feature to add.</param>
        public void AddFeature(Feature newFeature)
        {
            newFeature.Parent = this;

            Features.Add(newFeature);
        }

        /// <summary>
        /// Adds a set of child features.
        /// </summary>
        /// <param name="newFeatures">The child features to add.</param>
        public void AddFeatures(IEnumerable<Feature> newFeatures)
        {
            foreach (Feature newFeature in newFeatures)
            {
                AddFeature(newFeature);
            }
        }

        /// <summary>
        /// Recursively or non-recursively sets the ShouldInstall property to a value for a feature and it's children (if recursive).
        /// </summary>
        /// <param name="value">The value to set the property to.</param>
        /// <param name="recursive">Specifies if all features below this feature should also be set.</param>
        public void SetShouldInstall(bool value, bool recursive = false)
        {
            if (recursive)
            {
                _shouldInstall = value;

                if (Features.Count() > 0)
                {
                    foreach (Feature feature in Features)
                    {
                        feature.ShouldInstall = value;
                    }
                }
            }

            if (Parent != null)
            {
                Parent.SetShouldInstall(value, false);
            }

            if (!recursive)
            {
                // Only disable parent node if all children are disabled
                if (!value)
                {
                    IEnumerable<Feature> enabledFeatures = Features.Where(x => x.ShouldInstall);

                    if (enabledFeatures.Count() == 0)
                    {
                        _shouldInstall = value;
                    }
                }
                else
                {
                    _shouldInstall = value;
                }
            }

            NotifyPropertyChanged("ShouldInstall");
        }

        /// <summary>
        /// Gets a feature based on a path key.
        /// </summary>
        /// <param name="pathParts">Separated path parts (delimiter is '\').</param>
        /// <param name="nextPartIndex">Index of the next part in pathParts.</param>
        /// <returns>The feature at the requested path or null if it does not exist.</returns>
        public Feature GetFeatureAtPath(string[] pathParts, int nextPartIndex)
        {
            Feature feature = Features
                .Where(x => x.Name == pathParts[nextPartIndex])
                .FirstOrDefault();

            if (feature == null)
            {
                return null;
            }

            if (nextPartIndex < (pathParts.Length - 1))
            {
                feature = feature.GetFeatureAtPath(pathParts, ++nextPartIndex);
            }

            return feature;
        }
    }
}
