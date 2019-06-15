// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AdvocacyPlatform.Contracts;

    /// <summary>
    /// Default factory for creating IValueValidators.
    /// </summary>
    public class DefaultValueValidatorFactory : IValueValidatorFactory
    {
        /// <summary>
        /// Creates an IValueValidator for <paramref name="valueTypeName"/>.
        /// </summary>
        /// <param name="valueTypeName">The type of value to create an IValueValidator for.</param>
        /// <returns>An instance of the appropriate IValueValidator if known for <paramref name="valueTypeName"/>.</returns>
        public IValueValidator Create(string valueTypeName)
        {
            switch (valueTypeName.ToLowerInvariant())
            {
                case "ain":
                    return new AINValueValidator();
            }

            return null;
        }
    }
}
