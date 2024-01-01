using Azure;
using Azure.AI.OpenAI;
using BusinessRuleEngine.FunctionApp.Extensions;
using BusinessRuleEngine.FunctionApp.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BusinessRuleEngine.FunctionApp.Services
{
    public class BreExpenseApprovalService
    {
        private IOptions<BusinessRulesEngineOptions> _options;
        private OpenAIClient _openAiClient;
        private string businessRule = "";

        public BreExpenseApprovalService(IOptions<BusinessRulesEngineOptions> options)
        {
            _options = options;
            _openAiClient = new OpenAIClient(new Uri(_options.Value.OpenAiEndpoint),
                                             new AzureKeyCredential(_options.Value.OpenAiKey));
        }

        public string ReturnEndpoint()
        {
            return _options.Value.OpenAiEndpoint;
        }

        public async Task<ExpenseApprovalStatus> ProcessExpense(string expenseRequest)
        { 
            var expenseApprovalStatus = new ExpenseApprovalStatus();

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _options.Value.OpenAiModelDeployment,
                // While the respnse format can be specified as JSON object, it is highly recommended that this is explicit as part of the system message. 
                //ResponseFormat = ChatCompletionsResponseFormat.JsonObject,
                // Setting it close to 0 to create more deterministic results. 
                Temperature = 0,
                Messages =
                {
                    //TODO: Move these to app settings
                    // The system message represents instructions about how the chat completion model should behave
                    new ChatRequestSystemMessage(@"
                            You are a business rules engine that specialises in business expense approvals. You receive expenses in JSON format and assess the expense based on the the JSON property values. 
                            You will return your response as a JSON document. Here is an example of the expected JSON output: 
                                { 
                                    ""expenseId"": ""123"", 
                                    ""status"": ""Rejected"", 
                                    ""statusReason"": ""Expenses cannot exceed threshold per day.""
                                } 
                            Once you assess the expense, you need to return one of these statuses: 'Approved', 'Rejected', or 'RequiresManualApproval'.
                            The 'statusReason' field must have a value for all expensese that are not approved. When setting the statusReason field value, please be creative in explaining why the expense was not approved. However, the rule must be very clear. 
                            These are the rules you need to follow to assess the expense:
                             - Expenses of type 'meals' that occurred during a weekday and the employeeType is not 'Boss' , you approve them if they are equal or under 50 AUD. Those that are greater than 50 AUD, must be rejected. 
                             - Expenses of type 'meals' that occurred during a weekend and the employeeType is not 'Boss' , you return a status of 'RequiresManualApproval'. 
                             - Expenses of type 'meals' and the employeeType is 'Boss', are approved unless the amount is greater than 1000 AUD, which are rejected."
                    ),
                    // User messages represent input from the end user
                    new ChatRequestUserMessage(expenseRequest),
                }
            };

            Response<ChatCompletions> chatCompletion = await _openAiClient.GetChatCompletionsAsync(chatCompletionsOptions);
            var responseMessage = chatCompletion.Value.Choices[0].Message;
            Console.WriteLine($"[{responseMessage.Role.ToString().ToUpperInvariant()}]: {responseMessage.Content}");

            expenseApprovalStatus = responseMessage.Content.ToString().DeserializeFromCamelCase<ExpenseApprovalStatus>();
                //JsonSerializer.Deserialize<ExpenseApprovalStatus>(responseMessage.Content);
            return expenseApprovalStatus;
        }
    }
}
