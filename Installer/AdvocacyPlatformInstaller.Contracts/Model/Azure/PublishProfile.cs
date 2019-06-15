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
    /// Model representing an Azure App Services publishing profile.
    /// </summary>
    public class PublishProfile
    {
        /// <summary>
        /// Gets or sets the profile's name.
        /// </summary>
        [XmlAttribute("profileName")]
        public string ProfileName { get; set; }

        /// <summary>
        /// Gets or sets the profile's publishing method.
        /// </summary>
        [XmlAttribute("publishMethod")]
        public string PublishMethod { get; set; }

        /// <summary>
        /// Gets or sets the username of the profile.
        /// </summary>
        [XmlAttribute("userName")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password for the profile.
        /// </summary>
        [XmlAttribute("userPWD")]
        public string Password { get; set; }
    }
}
