// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Configuration intended to be pulled from app settings for interacting with the LUIS service.
    /// </summary>
    public class LuisConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the datetime entity used to extract datetime information.
        /// </summary>
        public string DateTimeEntityName { get; set; }

        /// <summary>
        /// Gets or sets the name of the date entity used to extract datetime information.
        /// </summary>
        public string DateEntityName { get; set; }

        /// <summary>
        /// Gets or sets the name of the time entity used to extract datetime information.
        /// </summary>
        public string TimeEntityName { get; set; }

        /// <summary>
        /// Gets or sets the name of the person entity used to extract person information.
        /// </summary>
        public string PersonEntityName { get; set; }

        /// <summary>
        /// Gets or sets the name of the location entity used to extract location information.
        /// </summary>
        public string LocationEntityName { get; set; }

        /// <summary>
        /// Gets or sets the name of the city entity used to extract location information.
        /// </summary>
        public string CityEntityName { get; set; }

        /// <summary>
        /// Gets or sets the name of the state entity used to extract location information.
        /// </summary>
        public string StateEntityName { get; set; }

        /// <summary>
        /// Gets or sets the name of the zipcode entity used to extract location information.
        /// </summary>
        public string ZipcodeEntityName { get; set; }
    }
}
