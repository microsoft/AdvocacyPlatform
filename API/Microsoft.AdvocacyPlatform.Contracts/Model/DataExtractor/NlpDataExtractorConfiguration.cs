// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Configuration information for Natural Language Processing (NLP) services.
    /// </summary>
    public class NlpDataExtractorConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NlpDataExtractorConfiguration"/> class.
        /// </summary>
        public NlpDataExtractorConfiguration()
        {
            PersonIntentTypeMap = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the  endpoint to use.
        /// </summary>
        public string NlpEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the  subscription key used when connecting to the service.
        /// </summary>
        public Secret NlpSubscriptionKey { get; set; }

        /// <summary>
        /// Gets or sets the  name of the datetime entity for extracting datetime information.
        /// </summary>
        public string DateTimeEntityName { get; set; }

        /// <summary>
        /// Gets or sets the  name of the date entity for extracting datetime information.
        /// </summary>
        public string DateEntityName { get; set; }

        /// <summary>
        /// Gets or sets the  name of the time entity for extracting datetime information.
        /// </summary>
        public string TimeEntityName { get; set; }

        /// <summary>
        /// Gets or sets the  name of the person entity for extracting person information.
        /// </summary>
        public string PersonEntityName { get; set; }

        /// <summary>
        /// Gets or sets the  name of the location entity for extracting location information.
        /// </summary>
        public string LocationEntityName { get; set; }

        /// <summary>
        /// Gets or sets the  name of the city entity for extracting location information.
        /// </summary>
        public string CityEntityName { get; set; }

        /// <summary>
        /// Gets or sets the  name of the state entity for extracting location information.
        /// </summary>
        public string StateEntityName { get; set; }

        /// <summary>
        /// Gets or sets the  name of the zipcode entity for extracting location information.
        /// </summary>
        public string ZipcodeEntityName { get; set; }

        /// <summary>
        /// Gets or sets the maps intents to person types (e.g. CourtHearingIntent -> PersonType.Judge).
        /// </summary>
        public Dictionary<string, string> PersonIntentTypeMap { get; set; }
    }
}
