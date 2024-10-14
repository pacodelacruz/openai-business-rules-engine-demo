using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace BusinessRulesEngine.Tests.Helpers
{
    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _outputHelper;

        public TestLoggerProvider(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputLogger(_outputHelper);
        }

        public void Dispose()
        {
            // No resources to dispose
        }
    }
}
