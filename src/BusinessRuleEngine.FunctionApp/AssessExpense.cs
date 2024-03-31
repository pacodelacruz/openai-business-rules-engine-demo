using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using BusinessRuleEngine.FunctionApp.Models;
using BusinessRuleEngine.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessRuleEngine.FunctionApp
{
    public class AssessExpense
    {
        private readonly ILogger _logger;
        private IOptions<BusinessRulesEngineOptions> _options;
        private BreExpenseApprovalService _breExpenseApprovalService;
        private JsonSerializerOptions _jsonSerializerOptions;

        public AssessExpense(IOptions<BusinessRulesEngineOptions> options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AssessExpense>();
            _options = options;
            _breExpenseApprovalService = new BreExpenseApprovalService(_options);
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        // Http triggered function that receives an expense in JSON format via a post and returns a JSON object with the assessment results and comments.
        [Function("AssessExpense")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Assess Expense Function triggered via HTTP Request");

            // Get the request body as a string
            string? expensePayload = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(expensePayload))
            {
                _logger.LogError("Request body is empty");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var assessmentResponse = await _breExpenseApprovalService.ProcessExpense(expensePayload);

            // Create a JSON response with the assessment results and comments
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(assessmentResponse, _jsonSerializerOptions));

            return response;
        }
    }
}
