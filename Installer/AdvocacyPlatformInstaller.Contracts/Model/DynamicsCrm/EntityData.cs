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
    /// Model representing an entity data file to import.
    /// </summary>
    [XmlRoot("entities")]
    public class EntityData
    {
        /// <summary>
        /// Gets or sets the list of entities.
        /// </summary>
        [XmlElement("entity")]
        public Entity[] Entities { get; set; }
    }
}
