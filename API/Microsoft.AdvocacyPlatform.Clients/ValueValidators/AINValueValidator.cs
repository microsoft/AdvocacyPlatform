// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.AdvocacyPlatform.Contracts;

    /// <summary>
    /// Validate an Alien Identification Number (AIN) is valid.
    /// </summary>
    public class AINValueValidator : IValueValidator
    {
        private static readonly Regex _replaceableCharactersRegEx = new Regex("(-)", RegexOptions.Compiled);

        /// <summary>
        /// Validates the AIN.
        /// </summary>
        /// <param name="value">The AIN.</param>
        /// <param name="acceptedValue">The accepted value.</param>
        /// <returns>True if valid and false if invalid.</returns>
        public bool Validate(string value, out string acceptedValue)
        {
            acceptedValue = value;

            if (string.IsNullOrWhiteSpace(acceptedValue))
            {
                acceptedValue = null;
                return false;
            }

            acceptedValue = _replaceableCharactersRegEx.Replace(acceptedValue, string.Empty);

            if (!int.TryParse(acceptedValue, out _))
            {
                acceptedValue = null;
                return false;
            }

            if (acceptedValue.Length < 8
                || acceptedValue.Length > 9)
            {
                acceptedValue = null;
                return false;
            }

            return true;
        }
    }
}
