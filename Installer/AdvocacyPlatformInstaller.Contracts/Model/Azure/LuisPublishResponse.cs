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
    /// Model representing the response from a request to publish a LUIS application.
    /// </summary>
    public class LuisPublishResponse
    {
        /// <summary>
        /// Gets or sets the version id of the application published.
        /// </summary>
        public string VersionId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ?.
        /// </summary>
        public bool DirectVersionPublish { get; set; }

        /// <summary>
        /// Gets or sets the application's endpoint URL.
        /// </summary>
        public string EndpointUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this application was published to a staging endpoint.
        /// </summary>
        public bool IsStaging { get; set; }

        /// <summary>
        /// Gets or sets the key for accessing this endpoint.
        /// </summary>
        public string AssignedEndpointKey { get; set; }

        /// <summary>
        /// Gets or sets the region for this application.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets the region for this endpoint.
        /// </summary>
        public string EndpointRegion { get; set; }

        /// <summary>
        /// Gets or sets the date and time the application was published.
        /// </summary>
        public DateTime? PublishedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the regions this application failed to publish to.
        /// </summary>
        public string FailedRegions { get; set; }
    }
}
