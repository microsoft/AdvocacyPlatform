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
    /// View model representing a PowerApps CDS database currency.
    /// </summary>
    public class PowerAppsCdsCurrency : NotifyPropertyChangedBase
    {
        private string _currencyName;
        private string _currencyCode;
        private string _currencySymbol;

        /// <summary>
        /// Gets or sets the name of the currency.
        /// </summary>
        public string CurrencyName
        {
            get => _currencyName;
            set
            {
                _currencyName = value;

                NotifyPropertyChanged("CurrencyName");
            }
        }

        /// <summary>
        /// Gets or sets the currency code.
        /// </summary>
        public string CurrencyCode
        {
            get => _currencyCode;
            set
            {
                _currencyCode = value;

                NotifyPropertyChanged("CurrencyCode");
            }
        }

        /// <summary>
        /// Gets or sets the symbol for this currency.
        /// </summary>
        public string CurrencySymbol
        {
            get => _currencySymbol;
            set
            {
                _currencySymbol = value;

                NotifyPropertyChanged("CurrencySymbol");
            }
        }

        /// <summary>
        /// Gets the currency name and symbol.
        /// </summary>
        /// <returns>The currency name and symbol formatted as {CurrencyName} ({CurrencySymbol}).</returns>
        public override string ToString()
        {
            return $"{CurrencyName} ({CurrencySymbol})";
        }
    }
}
