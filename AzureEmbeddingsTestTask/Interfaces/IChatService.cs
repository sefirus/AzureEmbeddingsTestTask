namespace AzureEmbeddingsTestTask.Interfaces;

public interface IChatService
{
    Task<string> GenerateAnswerAsync(string contextPrompt);
}
