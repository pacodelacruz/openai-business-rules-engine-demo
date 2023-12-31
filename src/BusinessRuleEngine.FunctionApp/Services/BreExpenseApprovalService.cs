using BusinessRuleEngine.FunctionApp.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Azure;

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
                Messages =
                {
                    // The system message represents instructions or other guidance about how the assistant should behave
                    new ChatRequestSystemMessage(
                            "You are a business rules engine that specialises in business expense approvals. You receive expenses in JSON format and assess the expense based on the the JSON property values. " + 
                            "You will return your response as a JSON document. Here is an example of the expected JSON output: \"expenseId\": \"123\", \"status\": \"Approved\", \"statusReason\": \"Meal expenses cannot exceed 50 AUD per day.\"}. " + 
                            "Once you assess the expense, you need to return one of these statuses: 'Approved', 'Rejected', or 'RequiresManualApproval'." + 
                            "These are the rules you need to follow. " + 
                            "- Expenses of type 'meals' that occurred during a weekday and the employeeType is not 'Boss' , you approve them if they are equal or under 50 AUD. Those that are greater than 50 AUD, must be rejected. " +
                            "- Expenses of type 'meals' that occurred during a weekend and the employeeType is not 'Boss' , you return a status of 'RequiresManualApproval'. " +
                            "- Expenses of type 'meals' and the employeeType is 'Boss', are approved unless the amount is greater than 1000 AUD. "),
                    // User messages represent current or historical input from the end user
                    new ChatRequestUserMessage(expenseRequest),
                    // Assistant messages represent historical responses from the assistant
                    //new ChatRequestAssistantMessage("Arrrr! Of course, me hearty! What can I do for ye?"),
                    //new ChatRequestUserMessage("What's the best way to train a parrot?"),
                }
            };

            Response<ChatCompletions> response = await _openAiClient.GetChatCompletionsAsync(chatCompletionsOptions);
            ChatResponseMessage responseMessage = response.Value.Choices[0].Message;
            Console.WriteLine($"[{responseMessage.Role.ToString().ToUpperInvariant()}]: {responseMessage.Content}");

            expenseApprovalStatus.Status = Constants.ExpenseConstants.ExpenseStatus.None;
            return expenseApprovalStatus;
        }
    }
}
