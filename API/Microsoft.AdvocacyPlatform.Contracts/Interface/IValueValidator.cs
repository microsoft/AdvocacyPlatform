// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Interface for interacting with a value validator.
    /// </summary>
    public interface IValueValidator
    {
        /// <summary>
        /// Validates the value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="acceptedValue">The accepted value.</param>
        /// <returns>True if valid and false if invalid.</returns>
        bool Validate(string value, out string acceptedValue);
    }
}
