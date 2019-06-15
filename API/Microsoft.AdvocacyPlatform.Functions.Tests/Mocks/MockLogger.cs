// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
namespace Microsoft.AdvocacyPlatform.Functions.Tests.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Mock trace logger.
    /// </summary>
    public class MockLogger : ILogger
    {
        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <typeparam name="TState">The state type.</typeparam>
        /// <param name="state">The current state.</param>
        /// <returns>An object implementing IDisposable.</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="logLevel">The log level to query.</param>
        /// <returns>True if the log level is enabled, false if it is not.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Logs output.
        /// </summary>
        /// <typeparam name="TState">The type of the state object.</typeparam>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">An id for the event.</param>
        /// <param name="state">The state to log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="formatter">Delegate for formatting.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine($"[UNITTEST] {logLevel}: {state}");
        }
    }
}
