// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace AdvocacyPlatformInstaller
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AdvocacyPlatformInstaller.Contracts;

    /// <summary>
    /// Simple logger for logging to a logging stream.
    /// </summary>
    public class OperationRunnerLogger : ILogger
    {
        private OperationRunner _instance;
        private StreamWriter _logFileStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationRunnerLogger"/> class.
        /// </summary>
        /// <param name="instance">The runner instance to attach to this logger.</param>
        /// <param name="logFileStream">The logging stream to write log lines to.</param>
        public OperationRunnerLogger(OperationRunner instance, StreamWriter logFileStream = null)
        {
            _instance = instance;
            _logFileStream = logFileStream;
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        public void LogError(string message)
        {
            _instance.WriteLog(message, LogLevel.Error);

            if (_logFileStream != null)
            {
                _logFileStream.WriteLine($"{DateTime.UtcNow.ToString()} ERROR: {message}");
            }
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The informational message to log.</param>
        public void LogInformation(string message)
        {
            _instance.WriteLog(message, LogLevel.Informational);

            if (_logFileStream != null)
            {
                _logFileStream.WriteLine($"{DateTime.UtcNow.ToString()} INFORMATION: {message}");
            }
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        public void LogWarning(string message)
        {
            _instance.WriteLog(message, LogLevel.Warning);

            if (_logFileStream != null)
            {
                _logFileStream.WriteLine($"{DateTime.UtcNow.ToString()} WARNING: {message}");
            }
        }
    }
}
