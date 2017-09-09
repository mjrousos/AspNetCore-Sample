// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace CustomersDemo.Tests.Helpers
{
    /// <summary>
    /// Logging: This class creates a custom logger that is used in the unit tests to verify that
    ///          logging is working as expected in the methods being tested.
    /// </summary>
    public class TestLogger<TCategory> : ILogger<TCategory>
    {
        private const string LogFormat = "[{0}] -- {1}";

        public List<string> LoggedMessages { get; private set; } = new List<string>();

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            LoggedMessages.Add(string.Format(LogFormat, logLevel, state));
        }

        /// <summary>
        /// Removes all items from the LoggedMessages
        /// </summary>
        public void ClearMessages()
        {
            LoggedMessages.Clear();
        }

        /// <summary>
        /// Builds a string representing the log message based on the passed in LogLevel and Message
        /// </summary>
        public string BuildLogString(LogLevel logLevel, string message)
        {
            return string.Format(LogFormat, logLevel, message);
        }
    }
}
