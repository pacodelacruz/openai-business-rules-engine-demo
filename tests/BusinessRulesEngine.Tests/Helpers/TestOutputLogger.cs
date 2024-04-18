using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace BusinessRulesEngine.Tests.Helpers
{
    public class TestOutputLogger : ILogger
    {
        private readonly ITestOutputHelper _outputHelper;

        public TestOutputLogger(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null; // No scope support in this example
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true; // Enable all log levels
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // Write log message to the output helper
            _outputHelper.WriteLine(formatter(state, exception));
        }
    }
}
