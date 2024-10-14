using BusinessRulesEngine.FunctionApp.Models;
using BusinessRulesEngine.FunctionApp.Services;
using BusinessRulesEngine.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;
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
        [InlineData("Meal-Manager-Under50.json", "Approved")]
        [InlineData("Meal-Manager-Above50.json", "Rejected")]
        [InlineData("Meal-Manager-Under50-Weekend.json", "RequiresManualApproval")]
        [InlineData("Meal-Boss-Above50.json", "Approved")]
        [InlineData("Meal-Boss-Above1000.json", "Rejected")]
        [InlineData("Flight-Manager-MEL-SYD-1000.json", "Approved")]
        [InlineData("Flight-Manager-MEL-PER-1600.json", "Rejected")]
        [InlineData("Flight-Manager-SYD-AKL-3000.json", "RequiresManualApproval")]
        [InlineData("Flight-Boss-HBA-CBR-2500.json", "Approved")]
        [InlineData("Flight-Boss-HBA-CBR-2501.json", "RequiresManualApproval")]
        [InlineData("Flight-Boss-BNE-LAX-3499.json", "Approved")]
        [InlineData("Flight-Boss-BNE-DFW-3501.json", "RequiresManualApproval")]
        public async Task TestExpenses(string payloadFileName, string expectedStatus)
        {
            _consoleLogger.Log(LogLevel.Information, $"Testing PayloadFileName: {payloadFileName}");

            //Arrange
            var expensePayload = TestDataHelper.GetTestDataStringFromFile(payloadFileName, "Expenses");
            JsonNode expenseNode = JsonNode.Parse(expensePayload)!;
            JsonNode expenseId = expenseNode!["id"]!;
            string expectedExpenseId = expenseId.ToJsonString().Replace("\"", "");

            // Act
            var processExpressClaimResult = await _breExpenseApprovalService.ProcessExpenseClaim(expensePayload);
            
            _consoleLogger.Log(LogLevel.Information, $"expenseId: {expenseId}, status: {processExpressClaimResult.Status?.ToString()}, reason: {processExpressClaimResult.StatusReason?.ToString()}");

            // Assert

            // Is the ExpenseId included in the response? 
            Assert.Equal(expectedExpenseId, processExpressClaimResult.ExpenseId?.ToString());
            // Is the status as expected?
            Assert.Equal(expectedStatus, processExpressClaimResult.Status?.ToString());
        }
    }
}