// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Describes an installer page.
    /// </summary>
    public class InstallerPageDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallerPageDescriptor"/> class.
        /// </summary>
        /// <param name="type">The type of the user control to show when navigated to this page.</param>
        /// <param name="name">The name of the page.</param>
        /// <param name="description">A description of the page.</param>
        /// <param name="isFinishPage">Specifies if the page is the last page in the process.</param>
        /// <param name="isDeploymentPage">Specifies if the page performs deployment operations.</param>
        public InstallerPageDescriptor(Type type, string name, string description, bool isFinishPage, bool isDeploymentPage)
        {
            Type = type;
            Name = name;
            Description = description;
            IsFinishPage = isFinishPage;
            IsDeploymentPage = isDeploymentPage;
        }

        /// <summary>
        /// Gets or sets the type of the user control to show when navigated to this page.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the page.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the page.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this page is the last page in the process.
        /// </summary>
        public bool IsFinishPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this page performs deployment operations.
        /// </summary>
        public bool IsDeploymentPage { get; set; }
    }
}
