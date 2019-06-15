// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Model representing an Azure Subscription.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Gets or sets the object id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }
    }
}
