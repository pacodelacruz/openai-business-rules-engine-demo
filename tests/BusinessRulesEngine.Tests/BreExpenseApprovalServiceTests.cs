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
        private IOptions<BusinessRulesEngineOptions> _options;
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

            _options = Options.Create(configuration.GetSection("Bre").Get<BusinessRulesEngineOptions>());
            _breExpenseApprovalService = new BreExpenseApprovalService(_options);

            // Send log messages to the output window during debug. 
            // Logging approach as per https://stackoverflow.com/questions/76572703/logger-output-in-c-sharp-net-test
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddProvider(new TestLoggerProvider(outputHelper));
                builder.SetMinimumLevel(LogLevel.Debug);
            });


 //           (builder => builder.AddConsole());
            _consoleLogger = _loggerFactory.CreateLogger<BreExpenseApprovalServiceTests>();
        }

        [Theory]
        [InlineData("MealUnder50.json", "Approved", false)]
        [InlineData("MealAbove50.json", "Rejected", true)]
        [InlineData("MealAbove50Boss.json", "Approved", false)]
        [InlineData("MealAbove1000Boss.json", "Rejected", true)]
        [InlineData("MealUnder50Weekend.json", "RequiresManualApproval", true)]
        public async Task TestMealExpenses(string payloadFileName, string expectedStatus, bool requiresStatusReason = false)
        {

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