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
    /// Model representing the training status details of a model in a LUIS application.
    /// </summary>
    public class LuisModelTrainingStatusDetails
    {
        /// <summary>
        /// Gets or sets the status id.
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Gets or sets the status text.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the count of examples.
        /// </summary>
        public int ExampleCount { get; set; }

        /// <summary>
        /// Gets or sets the date and time the model was trained.
        /// </summary>
        public DateTime? TrainingDateTime { get; set; }
    }
}
