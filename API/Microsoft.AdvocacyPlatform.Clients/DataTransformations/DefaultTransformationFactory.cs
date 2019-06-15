// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.AdvocacyPlatform.Contracts;

    /// <summary>
    /// Default factory for creating a TrimStrategy.
    /// </summary>
    public class DefaultTransformationFactory : IDataTransformationFactory
    {
        /// <summary>
        /// Name of the TrimEnd transformation.
        /// </summary>
        public const string TrimEndTransformationName = "trimeend";

        /// <summary>
        /// Name of the RemovePunctuation transformation.
        /// </summary>
        public const string RemovePunctuationTransformationName = "removepunctuation";

        /// <summary>
        /// Returns the IDataTransformation matching <paramref name="dataTransformationType" />.
        /// </summary>
        /// <param name="dataTransformationType">The name of the transformation to create.</param>
        /// <returns>An instance of the IDataTransformation if <paramref name="dataTransformationType"/> is valid.</returns>
        public IDataTransformation Create(string dataTransformationType)
        {
            switch (dataTransformationType.ToLowerInvariant())
            {
                case TrimEndTransformationName:
                    return new TrimEndTransformation();

                case RemovePunctuationTransformationName:
                    return new RemovePunctuationTransformation();
            }

            return null;
        }
    }
}
