// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Interface for interacting with a factory to create value validators.
    /// </summary>
    public interface IValueValidatorFactory
    {
        /// <summary>
        /// Creates a value validator.
        /// </summary>
        /// <param name="valueTypeName">The name of the requested value validator.</param>
        /// <returns>The implementation of the value validator requested.</returns>
        IValueValidator Create(string valueTypeName);
    }
}
