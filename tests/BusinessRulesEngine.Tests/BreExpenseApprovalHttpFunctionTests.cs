using BusinessRulesEngine.FunctionApp.Models;
using BusinessRulesEngine.Tests.Helpers;
using BusinessRulesEngine.Tests.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit.Abstractions;

namespace BusinessRulesEngine.Tests
{

    public class BreExpenseApprovalHttpFunctionTests
    {
        private IOptions<OpenAiOptions> _openAiOptions;
        private IOptions<FunctionAppOptions> _functionAppOptions;
        private JsonSerializerOptions _jsonSerializerOptionsWithCamel;
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

            _jsonSerializerOptionsWithCamel = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        }

        //[Theory]
        //[InlineData("Meal-Manager-Under50.json", "Approved")]
        //[InlineData("Meal-Manager-Above50.json", "Rejected")]
        //[InlineData("Meal-Manager-Under50-Weekend.json", "RequiresManualApproval")]
        //[InlineData("Meal-Boss-Above50.json", "Approved")]
        //[InlineData("Meal-Boss-Above1000.json", "Rejected")]
        //[InlineData("Flight-Manager-MEL-SYD-1000.json", "Approved")]
        //[InlineData("Flight-Manager-MEL-PER-1600.json", "Rejected")]
        //[InlineData("Flight-Manager-SYD-AKL-3000.json", "RequiresManualApproval")]
        //[InlineData("Flight-Boss-HBA-CBR-2500.json", "Approved")]
        //[InlineData("Flight-Boss-HBA-CBR-2501.json", "RequiresManualApproval")]
        //[InlineData("Flight-Boss-BNE-LAX-3499.json", "Approved")]
        //[InlineData("Flight-Boss-BNE-DFW-3501.json", "RequiresManualApproval")]
        //public async Task TestExpenses(string payloadFileName, string expectedStatus)
        //{
        //    _consoleLogger.Log(LogLevel.Information, $"Testing PayloadFileName: {payloadFileName}");

        //    // Arrange
        //    var expensePayload = TestDataHelper.GetTestDataStringFromFile(payloadFileName, "Expenses");
        //    JsonNode expenseNode = JsonNode.Parse(expensePayload)!;
        //    JsonNode expenseId = expenseNode!["id"]!;
        //    string expectedExpenseId = expenseId.ToJsonString().Replace("\"", "");

        //    var requestContent = new StringContent(expensePayload, Encoding.UTF8, "application/json");
        //    using var httpClient = _httpClientFactory.CreateClient();

        //    // Act
        //    var processExpenseFunctionResponse = await httpClient.PostAsync(_functionAppOptions.Value.FunctionAppEndpoint, requestContent);
        //    var processExpenseFunctionResponseBody = await processExpenseFunctionResponse.Content.ReadAsStringAsync();


        //    // Assert

        //    // Deserialize the responseContent into ExpenseApprovalStatus model
        //    var expenseApprovalStatus = JsonSerializer.Deserialize<ExpenseClaimApprovalStatus>(processExpenseFunctionResponseBody, _jsonSerializerOptionsWithCamel);

        //    if (expenseApprovalStatus is null)
        //        throw new Exception("Failed to deserialize response content into ExpenseApprovalStatus model");                

        //    _consoleLogger.Log(LogLevel.Information, $"expenseId: {expenseId}, status: {expenseApprovalStatus.Status?.ToString()}, reason: {expenseApprovalStatus.StatusReason?.ToString()}");

        //    // Is the ExpenseId included in the response? 
        //    Assert.Equal(expectedExpenseId, expenseApprovalStatus.ExpenseId?.ToString());
        //    // Is the status as expected?
        //    Assert.Equal(expectedStatus, expenseApprovalStatus.Status?.ToString());
        //}
    }
}