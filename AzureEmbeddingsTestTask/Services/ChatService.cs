using Azure.AI.OpenAI;
using AzureEmbeddingsTestTask.Interfaces;
using OpenAI.Chat;

namespace AzureEmbeddingsTestTask.Services;

public class ChatService : IChatService
{
    private readonly AzureOpenAISettings _settings;
    private readonly AzureOpenAIClient _completionsClient;

    public ChatService(AzureOpenAISettings settings, ICompletionsClientProvider completionsClient)
    {
        _settings = settings;
        _completionsClient = completionsClient.Client;
    }

    public async Task<string> GenerateAnswerAsync(string contextPrompt)
    {
        try
        {
            var chatClient = _completionsClient.GetChatClient(_settings.CompletionsDeployment);

            var messages = new List<ChatMessage>
            {
                // Message to instruct AI behavior.
                new UserChatMessage("You are an AI assistant that answers questions based on the provided context."), //TODO: better move to env file
                new UserChatMessage(contextPrompt)
            };

            var chatResponse = await chatClient.CompleteChatAsync(messages);

            var answer = chatResponse.Value.Content.Last().Text;
            return answer.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error generating answer: " + ex.Message);
            return "Error generating answer.";
        }
    }
}