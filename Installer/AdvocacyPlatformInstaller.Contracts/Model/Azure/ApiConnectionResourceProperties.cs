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
    /// Model representing an Azure API connection resource's connection properties.
    /// </summary>
    public class ApiConnectionResourceProperties
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the connection statuses.
        /// </summary>
        public ApiConnectionStatus[] Statuses { get; set; }

        /// <summary>
        /// Gets or sets the date and time the resource was created.
        /// </summary>
        public DateTime? CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time the resource was modified.
        /// </summary>
        public DateTime? ChangedTime { get; set; }

        /// <summary>
        /// Gets or sets API metadata regarding this connection.
        /// </summary>
        public ApiConnectionApi Api { get; set; }
    }
}
