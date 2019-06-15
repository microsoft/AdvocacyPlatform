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
    /// Model representing an entity record from an entity data file.
    /// </summary>
    public class EntityRecord
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a list of fields.
        /// </summary>
        [XmlElement("field")]
        public EntityRecordField[] Fields { get; set; }
    }
}
