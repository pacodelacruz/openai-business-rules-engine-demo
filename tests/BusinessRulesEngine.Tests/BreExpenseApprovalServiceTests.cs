using BusinessRuleEngine.FunctionApp.Models;
using BusinessRuleEngine.FunctionApp.Services;
using BusinessRulesEngine.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;
using Xunit.Abstractions;

namespace BusinessRulesEngine.Tests
{
    public class BreExpenseApprovalServiceTests
    {
        private IOptions<OpenAiOptions> _options;
        private BreExpenseApprovalService _breExpenseApprovalService;
        private ILoggerFactory _loggerFactory;
        private ILogger<BreExpenseApprovalServiceTests> _consoleLogger;

        public BreExpenseApprovalServiceTests(ITestOutputHelper outputHelper)
        {

            // Load configuration options from the appsettings.json file in the test project. 
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("local.settings.json", false)
               .Build();

            _options = Options.Create(configuration.GetSection("OpenAi").Get<OpenAiOptions>());
            _breExpenseApprovalService = new BreExpenseApprovalService(_options);

            // Send log messages to the output window during debug. 
            // Logging approach as per https://stackoverflow.com/questions/76572703/logger-output-in-c-sharp-net-test
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddProvider(new TestLoggerProvider(outputHelper));
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            _consoleLogger = _loggerFactory.CreateLogger<BreExpenseApprovalServiceTests>();
        }

        [Theory]
        [InlineData("Meal-Manager-Under50.json", "Approved", false)]
        [InlineData("Meal-Manager-Above50.json", "Rejected", true)]
        [InlineData("Meal-Manager-Under50-Weekend.json", "RequiresManualApproval", true)]
        [InlineData("Meal-Boss-Above50.json", "Approved", false)]
        [InlineData("Meal-Boss-Above1000.json", "Rejected", true)]
        [InlineData("Flight-Manager-MEL-SYD-1000.json", "Approved", false)]
        [InlineData("Flight-Manager-MEL-PER-1600.json", "Rejected", true)]
        [InlineData("Flight-Manager-SYD-AKL-3000.json", "RequiresManualApproval", true)]
        [InlineData("Flight-Manager-SYD-AKL-3001.json", "Rejected", true)]
        [InlineData("Flight-Boss-HBA-CBR-2500.json", "Approved", false)]
        [InlineData("Flight-Boss-HBA-CBR-2501.json", "RequiresManualApproval", true)]
        [InlineData("Flight-Boss-BNE-LAX-3450.json", "Approved", false)]
        [InlineData("Flight-Boss-BNE-DFW-3501.json", "RequiresManualApproval", true)]
        public async Task TestExpenses(string payloadFileName, string expectedStatus, bool requiresStatusReason = false)
        {
            _consoleLogger.Log(LogLevel.Information, $"Testing PayloadFileName: {payloadFileName}");

            //Arrange
            var expensePayload = TestDataHelper.GetTestDataStringFromFile(payloadFileName, "Expenses");
            JsonNode expenseNode = JsonNode.Parse(expensePayload)!;
            JsonNode expenseId = expenseNode!["id"]!;

            // Act
            var result = await _breExpenseApprovalService.ProcessExpense(expensePayload);

            // Assert

            _consoleLogger.Log(LogLevel.Information, $"expenseId: {expenseId}, status: {result.Status?.ToString()}, reason: {result.StatusReason?.ToString()}");

            // Is the ExpenseId included in the response? 
            Assert.Equal(expenseId.ToJsonString().Replace("\"", ""), result.ExpenseId?.ToString());
            // Is the status as expected?
            Assert.Equal(expectedStatus, result.Status?.ToString());
            if (requiresStatusReason)
            {
                Assert.NotNull(result.StatusReason);
            }
        }
    }
}