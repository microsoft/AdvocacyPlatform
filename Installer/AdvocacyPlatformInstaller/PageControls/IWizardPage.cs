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
    /// Interface for a page within the installation wizard.
    /// </summary>
    public interface IWizardPage
    {
        /// <summary>
        /// Sets default view model values.
        /// </summary>
        void SetDefaults();

        /// <summary>
        /// Sets default options in UI.
        /// </summary>
        void SetOptions();

        /// <summary>
        /// Validates selections in UI.
        /// </summary>
        /// <returns>True if valid, false if invalid.</returns>
        bool ValidatePage();
    }
}
