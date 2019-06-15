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
    /// Model representing an entity map in an entity schema file.
    /// </summary>
    public class EntityMapEntity
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
        /// Gets or sets additional information.
        /// </summary>
        [XmlAttribute("etc")]
        public string Etc { get; set; }

        /// <summary>
        /// Gets or sets the primary field id.
        /// </summary>
        [XmlAttribute("primaryfieldid")]
        public string PrimaryFieldId { get; set; }

        /// <summary>
        /// Gets or sets the primary name field.
        /// </summary>
        [XmlAttribute("primarynamefield")]
        public string PrimaryNameField { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether plugins are disabled.
        /// </summary>
        [XmlAttribute("disableplugins")]
        public string DisablePlugins { get; set; }

        /// <summary>
        /// Gets or sets a list of the entity map's fields.
        /// </summary>
        [XmlArray("fields")]
        [XmlArrayItem("field")]
        public EntityMapEntityField[] Fields { get; set; }
    }
}
