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
    /// Model representing a Dynamics 365 CRM solution.
    /// </summary>
    public class DynamicsCrmSolution
    {
        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the solution is a managed solution.
        /// </summary>
        public bool IsManaged { get; set; }

        /// <summary>
        /// Gets or sets the solution id.
        /// </summary>
        public string SolutionId { get; set; }

        /// <summary>
        /// Gets or sets the unique name.
        /// </summary>
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the friendly name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the date and time the solution was modified.
        /// </summary>
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the solution is visible.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the date and time the solution was created.
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the date and time the solution was installed.
        /// </summary>
        public DateTime? InstalledOn { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the solution type.
        /// </summary>
        public string SolutionType { get; set; }

        /// <summary>
        /// Gets or sets the version of the solution package.
        /// </summary>
        public string SolutionPackageVersion { get; set; }
    }
}
