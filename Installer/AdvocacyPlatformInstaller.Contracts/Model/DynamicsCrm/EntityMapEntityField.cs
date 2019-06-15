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
    /// Model representing a field in an entity map.
    /// </summary>
    public class EntityMapEntityField
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        [XmlAttribute("displayname")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the primary key.
        /// </summary>
        [XmlAttribute("primaryKey")]
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this a custom field.
        /// </summary>
        [XmlAttribute("customfield")]
        public bool CustomField { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the lookup type.
        /// </summary>
        [XmlAttribute("lookupType")]
        public string LookupType { get; set; }
    }
}
