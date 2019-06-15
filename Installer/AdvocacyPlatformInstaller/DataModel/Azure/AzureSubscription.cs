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
    /// View model representing an Azure subscription.
    /// </summary>
    public class AzureSubscription : NotifyPropertyChangedBase
    {
        private string _name;
        private string _id;

        /// <summary>
        /// Gets or sets the name of the subscription.
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
        /// Gets or sets the id of the subscription.
        /// </summary>
        public string Id
        {
            get => _id;
            set
            {
                _id = value;

                NotifyPropertyChanged("Id");
            }
        }

        /// <summary>
        /// Gets the name and id of the subscription.
        /// </summary>
        /// <returns>The name of the subscription in the format {Name} ({Id}).</returns>
        public override string ToString()
        {
            return $"{Name} ({Id})";
        }
    }
}
