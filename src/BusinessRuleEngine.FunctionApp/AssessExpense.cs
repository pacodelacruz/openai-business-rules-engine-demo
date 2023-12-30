using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BusinessRuleEngine.FunctionApp
{
    public class AssessExpense
    {
        private readonly ILogger _logger;

        public AssessExpense(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AssessExpense>();
        }

        [Function("Function1")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }

        // Http triggered function that receives an expense in JSON format via a post and returns a JSON object with the assessment results and comments.








    }
}
