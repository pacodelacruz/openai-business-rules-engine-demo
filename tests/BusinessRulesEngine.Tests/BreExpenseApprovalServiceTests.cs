using BusinessRuleEngine.FunctionApp.Models;
using BusinessRuleEngine.FunctionApp.Services;
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

        [Fact]
        public void Test1()
        {
            //Assert 
            Assert.NotEmpty(_breExpenseApprovalService.ReturnEndpoint());

        }
    }
}