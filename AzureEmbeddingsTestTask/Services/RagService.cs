using System.ClientModel;
using Azure.AI.OpenAI;
using AzureEmbeddingsTestTask.Interfaces;
using AzureEmbeddingsTestTask.Utils;
using OpenAI.Embeddings;

namespace AzureEmbeddingsTestTask.Services;

public class RagService : IRagService
{
    private readonly IDocumentLoader _documentLoader;
    private readonly IChatService _chatService;
    private readonly AzureOpenAISettings _settings;
    private readonly AzureOpenAIClient _embeddingsClient; // used to compute question embeddings

    public RagService(IDocumentLoader documentLoader, IChatService chatService, AzureOpenAISettings settings, AzureOpenAIClient embeddingsClient)
    {
        _documentLoader = documentLoader;
        _chatService = chatService;
        _settings = settings;
        _embeddingsClient = embeddingsClient;
    }

    public async Task RunAsync()
    {
        var documents = await _documentLoader.LoadDocumentsAsync();

        while (true)
        {
            Console.WriteLine("\nEnter your question (or type 'exit' to quit):");
            string question = Console.ReadLine();
            if (question.Trim().ToLower() == "exit")
            {
                break;
            }
            
            var embeddingClient = _embeddingsClient.GetEmbeddingClient(_settings.EmbeddingsDeployment);
            ClientResult<OpenAIEmbedding> questionResult = await embeddingClient.GenerateEmbeddingAsync(question);
            float[] questionEmbedding = questionResult.Value.ToFloats().ToArray();

            // Compute cosine similarity against all documents.
            var topDocs = documents
                .Select(doc => new
                {
                    Doc = doc,
                    Similarity = VectorUtils.CosineSimilarity(questionEmbedding, doc.Embedding)
                })
                .OrderByDescending(x => x.Similarity)
                .Take(3)
                .ToList();

            // Build a prompt using document excerpts.
            string contextPrompt = "Based on the following document excerpts:\n";
            foreach (var item in topDocs)
            {
                string excerpt = item.Doc.Content.Length > 500 ?
                    item.Doc.Content.Substring(0, 500) + "..." :
                    item.Doc.Content;
                contextPrompt += $"- {Path.GetFileName(item.Doc.FileName)}: {excerpt}\n";
            }
            contextPrompt += "\nAnswer the following question:\n" + question;

            // Generate the answer
            string answer = await _chatService.GenerateAnswerAsync(contextPrompt);
            Console.WriteLine("\nAnswer:");
            Console.WriteLine(answer);
        }
    }
}