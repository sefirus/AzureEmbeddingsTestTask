using System.ClientModel;
using Azure.AI.OpenAI;

namespace AzureEmbeddingsTestTask.Services;

public class CompletionsClientProvider : ICompletionsClientProvider
{
    public AzureOpenAIClient Client { get; }

    public CompletionsClientProvider(AzureOpenAISettings settings)
    {
        //Create the client for completions
        Client = new AzureOpenAIClient(new Uri(settings.CompletionsEndpoint), new ApiKeyCredential(settings.CompletionsApiKey));
    }
}

public interface ICompletionsClientProvider
{
    AzureOpenAIClient Client { get; }
}

public class EmbeddingClientProvider : IEmbeddingClientProvider
{
    public AzureOpenAIClient Client { get; }

    public EmbeddingClientProvider(AzureOpenAISettings settings)
    {
        //Create the client for embeddings
        Client = new AzureOpenAIClient(new Uri(settings.EmbeddingsEndpoint), new ApiKeyCredential(settings.EmbeddingsApiKey));
    }
}

public interface IEmbeddingClientProvider
{
    AzureOpenAIClient Client { get; }
}