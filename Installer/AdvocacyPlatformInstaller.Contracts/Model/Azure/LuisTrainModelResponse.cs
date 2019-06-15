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
    /// Model representing the response from a request to train a model in a LUIS application.
    /// </summary>
    public class LuisTrainModelResponse
    {
        /// <summary>
        /// Gets or sets that status id.
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Gets or sets the status text.
        /// </summary>
        public string Status { get; set; }
    }
}
