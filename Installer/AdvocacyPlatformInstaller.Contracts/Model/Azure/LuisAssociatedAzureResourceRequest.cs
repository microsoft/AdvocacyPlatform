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
    /// Model representing information required to associated an Azure LUIS Cognitive Services resource with a LUIS application.
    /// </summary>
    public class LuisAssociatedAzureResourceRequest
    {
        /// <summary>
        /// Gets or sets the id of the Azure subscription.
        /// </summary>
        public string AzureSubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the name of the Azure Resource Group.
        /// </summary>
        public string ResourceGroup { get; set; }

        /// <summary>
        /// Gets or sets the name of the Azure LUIS Cognitive Services resource.
        /// </summary>
        public string AccountName { get; set; }
    }
}
