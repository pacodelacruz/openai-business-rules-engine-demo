using BusinessRuleEngine.FunctionApp.Models;
using BusinessRuleEngine.FunctionApp.Services;
using BusinessRulesEngine.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessRulesEngine.Tests
{
    public class BreExpenseApprovalServiceTests
    {
        private IOptions<BusinessRulesEngineOptions> _options;
        private BreExpenseApprovalService _breExpenseApprovalService;
        private ILogger _consoleLogger;

        public BreExpenseApprovalServiceTests()
        {

            // Load configuration options from the appsettings.json file in the test project. 
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("local.settings.json", false)
               .Build();


            _options = Options.Create(configuration.GetSection("Values").Get<BusinessRulesEngineOptions>());
            _breExpenseApprovalService = new BreExpenseApprovalService(_options);

            // Send log messages to the output window during debug. 
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            _consoleLogger = loggerFactory.CreateLogger<BreExpenseApprovalService>();
        }

        [Theory]
        [InlineData("MealUnder50.json", "Approved")]
        public async Task TestMealExpenses(string payloadFileName, string expectedStatus)
        {

            //Arrange
            var payload = TestDataHelper.GetTestDataStringFromFile(payloadFileName);

            // Act
            var result = await _breExpenseApprovalService.ProcessExpense(payload);
            var breStatus = result.Status.ToString();

            // Assert
            Assert.Equal(expectedStatus, breStatus);
        }
    }
}