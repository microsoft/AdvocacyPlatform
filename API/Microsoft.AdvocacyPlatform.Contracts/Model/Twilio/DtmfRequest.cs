// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents information to construct a Dual-toned Multi-frequency signaling sequence.
    /// </summary>
    public class DtmfRequest
    {
        /// <summary>
        /// Gets or sets the number of seconds to wait after call is initiated.
        /// </summary>
        public int? InitPause { get; set; }

        /// <summary>
        /// Gets or sets the Dual-toned Multi-frequency signaling sequence.
        /// </summary>
        public string Dtmf { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds to wait after the sequence has completed.
        /// </summary>
        public int? FinalPause { get; set; }
    }
}
