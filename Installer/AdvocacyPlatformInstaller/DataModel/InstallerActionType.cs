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
    /// Enumeration of the available installer actions.
    /// </summary>
    public enum InstallerActionType
    {
        /// <summary>
        /// Perform a new installation
        /// </summary>
        New = 1,

        /// <summary>
        /// Modify an existing installation
        /// </summary>
        Modify = 2,

        /// <summary>
        /// Remove an existing installation
        /// </summary>
        Remove = 3,
    }
}
