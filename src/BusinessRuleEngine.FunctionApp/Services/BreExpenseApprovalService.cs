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
        private string _businessRulesEnginePrompt = "";

        public BreExpenseApprovalService(IOptions<BusinessRulesEngineOptions> options)
        {
            _options = options;
            _openAiClient = new OpenAIClient(new Uri(_options.Value.OpenAiEndpoint),
                                             new AzureKeyCredential(_options.Value.OpenAiKey));
            _businessRulesEnginePrompt = GetBusinessRulesEnginePrompt();
        }

        public string ReturnEndpoint()
        {
            return _options.Value.OpenAiEndpoint;
        }

        public async Task<ExpenseApprovalStatus> ProcessExpense(string expenseRequest)
        {
            var expenseApprovalStatus = new ExpenseApprovalStatus();
            //var businessRulesEnginePrompt = GetBusinessRulesEnginePrompt();

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _options.Value.OpenAiModelDeployment,
                // Setting the temperature to 0 to create more deterministic results
                Temperature = 1,
                Messages =
                    {
                        // In this case, the system message represents the business rules engine prompt
                        // that describes how the chat completion model should behave
                        // And the user message represents the received expense, i.e., the input from the end user
                        new ChatRequestSystemMessage(_businessRulesEnginePrompt),
                        new ChatRequestUserMessage(expenseRequest)
                    }
            };

            Response<ChatCompletions> chatCompletion = await _openAiClient.GetChatCompletionsAsync(chatCompletionsOptions);
            var responseMessage = chatCompletion.Value.Choices[0].Message;
            Console.WriteLine($"[{responseMessage.Role.ToString().ToUpperInvariant()}]: {responseMessage.Content}");

            expenseApprovalStatus = responseMessage.Content.ToString().DeserializeFromCamelCase<ExpenseApprovalStatus>();
            //JsonSerializer.Deserialize<ExpenseApprovalStatus>(responseMessage.Content);
            return expenseApprovalStatus;
        }

        // Get the business rules engine prompts from the prompts folder
        private static string GetBusinessRulesEnginePrompt()
        {
            string businessRulesContextPrompt = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts", "BreContextPrompt.txt"));
            string businessRulesPrompt = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Prompts", "BreRulesPrompt.txt"));

            return businessRulesContextPrompt + "\n" + businessRulesPrompt;
        }
    }
}
