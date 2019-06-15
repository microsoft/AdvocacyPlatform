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
    /// Model representing the body content for a request to create a PowerApps CDS database.
    /// </summary>
    public class CreatePowerAppsCdsDatabaseRequest
    {
        /// <summary>
        /// Gets or sets the base language.
        /// </summary>
        public string BaseLanguage { get; set; }

        /// <summary>
        /// Gets or sets information regarding the database currency.
        /// </summary>
        public PowerAppsCdsDatabaseCurrencyMinimal Currency { get; set; }
    }
}
