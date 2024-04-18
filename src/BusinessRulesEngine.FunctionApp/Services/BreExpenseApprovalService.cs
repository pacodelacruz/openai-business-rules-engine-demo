using Azure;
using Azure.AI.OpenAI;
using BusinessRulesEngine.FunctionApp.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace BusinessRulesEngine.FunctionApp.Services
{
    public class BreExpenseApprovalService
    {
        private IOptions<OpenAiOptions> _options;
        private OpenAIClient _openAiClient;
        private string _businessRulesEnginePrompt = "";
        private JsonSerializerOptions _jsonSerializerOptionsWithCamel;

        public BreExpenseApprovalService(IOptions<OpenAiOptions> options)
        {
            _options = options;
            _openAiClient = new OpenAIClient(new Uri(_options.Value.OpenAiEndpoint),
                                             new AzureKeyCredential(_options.Value.OpenAiKey));
            _businessRulesEnginePrompt = GetBusinessRulesEnginePrompt();
            _jsonSerializerOptionsWithCamel = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<ExpenseClaimApprovalStatus> ProcessExpenseClaim(string expenseClaimRequest)
        {
            // Setting the temperature to 0 to create more deterministic results
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _options.Value.OpenAiModelDeployment,
                Temperature = _options.Value.Temperature,
                Messages =
                    {
                        // In this case, the system message represents the business rules engine prompt
                        // that describes how the chat completion model should behave
                        // And the user message represents the received expense, i.e., the input from the end user
                        new ChatRequestSystemMessage(_businessRulesEnginePrompt),
                        new ChatRequestUserMessage(expenseClaimRequest)
                    }
            };

            Response<ChatCompletions> chatCompletion = await _openAiClient.GetChatCompletionsAsync(chatCompletionsOptions);
            var responseMessage = chatCompletion.Value.Choices[0].Message;
            Console.WriteLine($"[{responseMessage.Role.ToString().ToUpperInvariant()}]: {responseMessage.Content}");

            var expenseApprovalStatus = JsonSerializer.Deserialize<ExpenseClaimApprovalStatus>(responseMessage.Content, _jsonSerializerOptionsWithCamel);

            if (expenseApprovalStatus is null)
                throw new Exception("Failed to deserialize the response from the OpenAI API");

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
