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
    /// Model representing the training status of a model in a LUIS application.
    /// </summary>
    public class LuisModelTrainingStatus
    {
        /// <summary>
        /// Gets or sets the id of the model.
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// Gets or sets the training status details for the model.
        /// </summary>
        public LuisModelTrainingStatusDetails Details { get; set; }
    }
}
