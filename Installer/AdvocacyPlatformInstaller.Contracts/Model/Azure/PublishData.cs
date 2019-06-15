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
    /// Model representing a collection of Azure App Services publishing profiles.
    /// </summary>
    [XmlRoot("publishData")]
    public class PublishData
    {
        /// <summary>
        /// Gets or sets the list of publishing profiles.
        /// </summary>
        [XmlElement("publishProfile")]
        public PublishProfile[] Profiles { get; set; }
    }
}
