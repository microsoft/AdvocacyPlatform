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
    /// Model representing a field in an entity record from an entity data file.
    /// </summary>
    public class EntityRecordField
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [XmlAttribute("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the lookup entity.
        /// </summary>
        [XmlAttribute("lookupentity")]
        public string LookupEntity { get; set; }

        /// <summary>
        /// Gets or sets the name of the lookup entity.
        /// </summary>
        [XmlAttribute("lookupentityname")]
        public string LookupEntityName { get; set; }
    }
}
