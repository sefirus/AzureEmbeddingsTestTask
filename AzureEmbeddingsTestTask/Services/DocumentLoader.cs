using System.ClientModel;
using Azure.AI.OpenAI;
using AzureEmbeddingsTestTask.Interfaces;
using OpenAI.Embeddings;

namespace AzureEmbeddingsTestTask.Services;

public class DocumentLoader : IDocumentLoader
{
    private readonly AzureOpenAISettings _settings;
    private readonly AzureOpenAIClient _embeddingsClient;

    public DocumentLoader(AzureOpenAISettings settings, IEmbeddingClientProvider embeddingsClient)
    {
        _settings = settings;
        _embeddingsClient = embeddingsClient.Client;
    }

    public async Task<List<Document>> LoadDocumentsAsync()
    {
        var documents = new List<Document>();
        var txtFiles = Directory.GetFiles(_settings.DocumentsFolder, "*.txt", SearchOption.AllDirectories);
        Console.WriteLine("Loading documents from: " + _settings.DocumentsFolder);

        var embeddingClient = _embeddingsClient.GetEmbeddingClient(_settings.EmbeddingsDeployment);

        foreach (var file in txtFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);
                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }
                content = content.Replace("\n", " ").Trim();

                ClientResult<OpenAIEmbedding> result = await embeddingClient.GenerateEmbeddingAsync(content);
                float[] embedding = result.Value.ToFloats().ToArray();

                documents.Add(new Document
                {
                    FileName = file,
                    Content = content,
                    Embedding = embedding
                });
                Console.WriteLine($"Processed document: {Path.GetFileName(file)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {file}: {ex.Message}");
            }
        }
        
        Console.WriteLine($"Total documents processed: {documents.Count}");
        return documents;
    }
}