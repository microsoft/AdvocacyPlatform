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
    /// Model representing properties regarding an PowerApps CDS database currency.
    /// </summary>
    public class PowerAppsCurrencyProperties
    {
        /// <summary>
        /// Gets or sets the currency code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the currency symbol.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this currency is the tenant default.
        /// </summary>
        public bool IsTenantDefault { get; set; }
    }
}
