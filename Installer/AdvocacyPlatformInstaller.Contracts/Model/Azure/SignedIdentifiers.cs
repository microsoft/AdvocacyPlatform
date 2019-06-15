// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    /// <summary>
    /// Model representing a list of Azure Storage signed identifiers for creating a Stored Access Policy.
    /// </summary>
    public class SignedIdentifiers
    {
        /// <summary>
        /// Gets or sets the list of signed identifiers.
        /// </summary>
        [XmlElement("SignedIdentifier")]
        public SignedIdentifier[] SignedIdentifier { get; set; }
    }
}
