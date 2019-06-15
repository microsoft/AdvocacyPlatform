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
    /// Event fired when logging occurs.
    /// </summary>
    public class LogEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The level to log the message at.</param>
        public LogEventArgs(string message, LogLevel level)
        {
            Message = message;
            Level = level;
        }

        /// <summary>
        /// Gets or sets the message to log.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the level to log the message at.
        /// </summary>
        public LogLevel Level { get; set; }
    }
}
