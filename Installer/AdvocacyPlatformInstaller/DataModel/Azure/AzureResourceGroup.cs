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
    /// View model representing an Azure Resource Group.
    /// </summary>
    public class AzureResourceGroup : NotifyPropertyChangedBase
    {
        private string _name;
        private string _location;
        private Dictionary<string, string> _tags;

        /// <summary>
        /// Gets or sets the name of the resource group.
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
        /// Gets or sets the location of the resource group.
        /// </summary>
        public string Location
        {
            get => _location;
            set
            {
                _location = value;

                NotifyPropertyChanged("Location");
            }
        }

        /// <summary>
        /// Gets or sets tags associated with the resource group.
        /// </summary>
        public Dictionary<string, string> Tags
        {
            get => _tags;
            set
            {
                _tags = value;

                NotifyPropertyChanged("Tags");
            }
        }

        /// <summary>
        /// Gets the name of the resource group.
        /// </summary>
        /// <returns>The name of the resource group.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
