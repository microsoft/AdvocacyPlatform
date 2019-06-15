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
    /// Model for representing the installer feature tree.
    /// </summary>
    public class FeatureTree : NotifyPropertyChangedBase
    {
        private List<Feature> _features;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureTree"/> class.
        /// </summary>
        public FeatureTree()
        {
            Features = new List<Feature>();
        }

        /// <summary>
        /// Gets or sets the list of features available.
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
        /// Gets the count of features to install, modify, or remove.
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
        /// Gets a feature based on the provided path key.
        /// </summary>
        /// <param name="path">Feature path (delimiter '\').</param>
        /// <returns>The feature at the request path or null if it does not exist.</returns>
        public Feature this[string path]
        {
            get
            {
                string[] pathParts = path.Split(new char[] { '\\' });

                Feature feature = Features
                    .Where(x => x.Name == pathParts[0])
                    .FirstOrDefault();

                if (feature == null)
                {
                    return null;
                }

                if (pathParts.Length > 1)
                {
                    feature = feature.GetFeatureAtPath(pathParts, 1);
                }

                return feature;
            }
        }
    }
}
