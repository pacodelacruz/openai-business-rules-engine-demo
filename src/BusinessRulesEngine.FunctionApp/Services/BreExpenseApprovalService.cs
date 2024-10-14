using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using BusinessRulesEngine.FunctionApp.Models;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace BusinessRulesEngine.FunctionApp.Services
{
    public class BreExpenseApprovalService
    {
        private static IOptions<OpenAiOptions> _options;
        private AzureOpenAIClient _azureOpenAIClient;
        private ChatClient _chatClient;
        private string _businessRulesEnginePrompt = "";
        private JsonSerializerOptions _jsonSerializerOptions;

        public BreExpenseApprovalService(IOptions<OpenAiOptions> options)
        {
            _options = options;

            _azureOpenAIClient = new(
                new Uri(_options.Value.OpenAiEndpoint),
                new ApiKeyCredential(_options.Value.OpenAiKey));

            _chatClient = _azureOpenAIClient.GetChatClient(_options.Value.OpenAiModelDeployment);

            _businessRulesEnginePrompt = GetBusinessRulesEnginePrompt();
            _jsonSerializerOptions = GetJsonSerializationOptions();
        }

        public async Task<ExpenseClaimApprovalStatus> ProcessExpenseClaim(string expenseClaimRequest)
        {

            var chatCompletionsOptions = GetChatCompletionOptions();

            ChatCompletion chatCompletion = _chatClient.CompleteChat(
                [
                    new SystemChatMessage(_businessRulesEnginePrompt),
                    new UserChatMessage(expenseClaimRequest),
                ], 
                chatCompletionsOptions);

            using JsonDocument structuredJson = JsonDocument.Parse(chatCompletion.Content[0].Text);

            Console.WriteLine($"Expense approval status: {structuredJson.RootElement.GetProperty("status").GetString()}");

            var expenseApprovalStatus = JsonSerializer.Deserialize<ExpenseClaimApprovalStatus>(structuredJson, _jsonSerializerOptions);

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

        public static JsonSerializerOptions GetJsonSerializationOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private static AzureOpenAIClientOptions GetAzureOpenAIClientOptions()
        {
            return new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2024_08_01_Preview);
        }


        /// <summary>
        /// Sets temperature and response format for the chat completion options
        /// </summary>
        /// <returns></returns>
        private static ChatCompletionOptions GetChatCompletionOptions()
        {
            return new ChatCompletionOptions()
            {
                Temperature = _options.Value.Temperature,

                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "expense-assessment",
                    jsonSchema: BinaryData.FromBytes("""
                        {
                            "type": "object",
                            "properties": {
                                "expenseId": {
                                  "type": "string"
                                },
                                "status": {
                                  "type": "string"
                                },
                                "statusReason": {
                                  "type": "string"
                                }
                              },
                            "required": ["expenseId", "status", "statusReason"],
                            "additionalProperties": false
                        }
                        """u8.ToArray()),
                    jsonSchemaIsStrict: true)
            };
        }
    }
}
