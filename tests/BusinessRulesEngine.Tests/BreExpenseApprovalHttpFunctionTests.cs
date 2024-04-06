using BusinessRuleEngine.FunctionApp.Models;
using BusinessRuleEngine.Tests.Models;
using BusinessRuleEngine.FunctionApp.Services;
using BusinessRulesEngine.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace BusinessRulesEngine.Tests
{

    public class BreExpenseApprovalHttpFunctionTests
    {
        private IOptions<OpenAiOptions> _openAiOptions;
        private IOptions<FunctionAppOptions> _functionAppOptions;
        private JsonSerializerOptions _jsonSerializerOptions;
        private ILoggerFactory _loggerFactory;
        private ILogger<BreExpenseApprovalHttpFunctionTests> _consoleLogger;
        private readonly IHttpClientFactory _httpClientFactory;

        public BreExpenseApprovalHttpFunctionTests(ITestOutputHelper outputHelper)
        {
            // Load configuration options from the appsettings.json file in the test project. 
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("local.settings.json", false)
               .Build();

            _openAiOptions = Options.Create(configuration.GetSection("OpenAi").Get<OpenAiOptions>());
            _functionAppOptions = Options.Create(configuration.GetSection("FunctionApp").Get<FunctionAppOptions>());

            _httpClientFactory = new ServiceCollection()
                .AddHttpClient()
                .BuildServiceProvider()
                .GetRequiredService<IHttpClientFactory>();


            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddProvider(new TestLoggerProvider(outputHelper));
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            _consoleLogger = _loggerFactory.CreateLogger<BreExpenseApprovalHttpFunctionTests>();

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

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
        [InlineData("Flight-Boss-BNE-LAX-3500.json", "Approved", false)]
        [InlineData("Flight-Boss-BNE-DFW-3501.json", "RequiresManualApproval", true)]
        public async Task TestExpenses(string payloadFileName, string expectedStatus, bool requiresStatusReason = false)
        {
            _consoleLogger.Log(LogLevel.Information, $"Testing PayloadFileName: {payloadFileName}");

            // Arrange
            var expensePayload = TestDataHelper.GetTestDataStringFromFile(payloadFileName, "Expenses");
            JsonNode expenseNode = JsonNode.Parse(expensePayload)!;
            JsonNode expenseId = expenseNode!["id"]!;
            var requestContent = new StringContent(expensePayload, Encoding.UTF8, "application/json");

            using var httpClient = _httpClientFactory.CreateClient();

            // Act
            var response = await httpClient.PostAsync(_functionAppOptions.Value.FunctionAppEndpoint, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();


            // Assert

            // Deserialize the responseContent into ExpenseApprovalStatus model
            var expenseApprovalStatus = JsonSerializer.Deserialize<ExpenseApprovalStatus>(responseContent, _jsonSerializerOptions);

            if (expenseApprovalStatus is null)
                throw new Exception("Failed to deserialize response content into ExpenseApprovalStatus model");                

            _consoleLogger.Log(LogLevel.Information, $"expenseId: {expenseId}, status: {expenseApprovalStatus.Status?.ToString()}, reason: {expenseApprovalStatus.StatusReason?.ToString()}");

            // Is the ExpenseId included in the response? 
            Assert.Equal(expenseId.ToJsonString().Replace("\"", ""), expenseApprovalStatus.ExpenseId?.ToString());
            // Is the status as expected?
            Assert.Equal(expectedStatus, expenseApprovalStatus.Status?.ToString());
            if (requiresStatusReason)
            {
                Assert.NotNull(expenseApprovalStatus.StatusReason);
            }
        }
    }
}