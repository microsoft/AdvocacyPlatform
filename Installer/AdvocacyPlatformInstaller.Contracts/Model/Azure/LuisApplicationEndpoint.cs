// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Model representing a LUIS application endpoint.
    /// </summary>
    public class LuisApplicationEndpoint
    {
        /// <summary>
        /// Gets or sets the version id of the application.
        /// </summary>
        public string VersionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ?.
        /// </summary>
        public bool DirectVersionPublish { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL.
        /// </summary>
        public string EndpointUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a staging endpoint.
        /// </summary>
        public bool IsStaging { get; set; }

        /// <summary>
        /// Gets or sets the key for making requests to this endpoint.
        /// </summary>
        public string AssignedEndpointKey { get; set; }

        /// <summary>
        /// Gets or sets the region for this endpoint.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the region for this endpoint.
        /// </summary>
        public string EndpointRegion { get; set; }

        /// <summary>
        /// Gets or sets the date and time this endpoint was published.
        /// </summary>
        public DateTime? PublishedDateTime { get; set; }
    }
}
