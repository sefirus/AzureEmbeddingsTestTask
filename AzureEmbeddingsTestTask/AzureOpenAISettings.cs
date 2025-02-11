namespace AzureEmbeddingsTestTask;

public class AzureOpenAISettings
{
    public string EmbeddingsApiKey { get; set; }
    public string EmbeddingsEndpoint { get; set; }
    public string EmbeddingsDeployment { get; set; }

    public string CompletionsApiKey { get; set; }
    public string CompletionsEndpoint { get; set; }
    public string CompletionsDeployment { get; set; }

    public string DocumentsFolder { get; set; }
}