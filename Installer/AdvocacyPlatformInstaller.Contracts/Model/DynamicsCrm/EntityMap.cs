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
    /// Model representing an entity map from an entity schema file.
    /// </summary>
    [XmlRoot("entities")]
    public class EntityMap
    {
        /// <summary>
        /// Gets or sets the list of entity maps.
        /// </summary>
        [XmlElement("entity")]
        public EntityMapEntity[] Entities { get; set; }
    }
}
