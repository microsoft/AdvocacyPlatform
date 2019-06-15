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
    /// Model representing an entity in an entity data file.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        [XmlAttribute("displayname")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the list of records.
        /// </summary>
        [XmlArray("records")]
        [XmlArrayItem("record")]
        public EntityRecord[] Records { get; set; }
    }
}
