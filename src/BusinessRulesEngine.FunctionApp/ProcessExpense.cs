using BusinessRulesEngine.FunctionApp.Models;
using BusinessRulesEngine.FunctionApp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace BusinessRulesEngine.FunctionApp
{
    public class ProcessExpense
    {
        private readonly ILogger _logger;
        private IOptions<OpenAiOptions> _options;
        private BreExpenseApprovalService _breExpenseApprovalService;
        private JsonSerializerOptions _jsonSerializerOptions;

        public ProcessExpense(IOptions<OpenAiOptions> options, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProcessExpense>();
            _options = options;
            _breExpenseApprovalService = new BreExpenseApprovalService(_options);
            _jsonSerializerOptions = BreExpenseApprovalService.GetJsonSerializationOptions();
        }

        // Http triggered function that receives an expense in JSON format via a post and returns a JSON object with the results and comments.
        [Function("ProcessExpense")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Process Expense Function triggered via HTTP Request");

            // Get the request body as a string
            string? expenseClaimPayload = await req.ReadAsStringAsync();

            if (string.IsNullOrEmpty(expenseClaimPayload))
            {
                _logger.LogError("Request body is empty");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var assessmentResponse = await _breExpenseApprovalService.ProcessExpenseClaim(expenseClaimPayload);

            // Create a JSON response with the claim assessment results
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(assessmentResponse, _jsonSerializerOptions));

            return response;
        }
    }
}
