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
    /// Model with the minimal information required to represent a PowerApps CDS database currency.
    /// </summary>
    public class PowerAppsCdsDatabaseCurrencyMinimal
    {
        /// <summary>
        /// Gets or sets the currency code.
        /// </summary>
        public string Code { get; set; }
    }
}
