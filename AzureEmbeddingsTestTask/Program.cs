using System.ClientModel;
using OpenAI.Chat;
using OpenAI.Embeddings;
using AzureOpenAIClient = Azure.AI.OpenAI.AzureOpenAIClient;
using Console = System.Console;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(optional: true) 
    .Build();

//Get configs
string apiKey = configuration["AZURE_OPENAI_API_KEY"];
string endpoint = configuration["AZURE_OPENAI_ENDPOINT"];
string apiKeyCompletions = configuration["AZURE_OPENAI_COMPLETIONS_API_KEY"];
string endpointCompletions = configuration["AZURE_OPENAI_COMPLETIONS_ENDPOINT"];
string embeddingsDeployment = configuration["AZURE_OPENAI_EMBEDDINGS_DEPLOYMENT"];
string completionsDeployment = configuration["AZURE_OPENAI_COMPLETIONS_DEPLOYMENT"];
string documentsFolder = configuration["DOCUMENTS_FOLDER"];

if (string.IsNullOrWhiteSpace(apiKey) ||
    string.IsNullOrWhiteSpace(endpoint) ||
    string.IsNullOrWhiteSpace(embeddingsDeployment) ||
    string.IsNullOrWhiteSpace(completionsDeployment) ||
    string.IsNullOrWhiteSpace(documentsFolder) ||
    string.IsNullOrWhiteSpace(apiKeyCompletions) ||
    string.IsNullOrWhiteSpace(endpointCompletions))
{
    Console.WriteLine("Missing configuration. Please set AZURE_OPENAI_API_KEY, AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_EMBEDDINGS_DEPLOYMENT, AZURE_OPENAI_COMPLETIONS_DEPLOYMENT, and DOCUMENTS_FOLDER in your user secrets or environment variables.");
    return;
}

Console.WriteLine("Initializing Azure OpenAI client...");

AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
var embeddingClient = azureClient.GetEmbeddingClient(embeddingsDeployment);
AzureOpenAIClient azureClientCompletions = new AzureOpenAIClient(new Uri(endpointCompletions), new ApiKeyCredential(apiKeyCompletions));
var chatClient = azureClientCompletions.GetChatClient(completionsDeployment);

Console.WriteLine("Loading documents from folder: " + documentsFolder);
List<Document> documents = new List<Document>();
var txtFiles = Directory.GetFiles(documentsFolder, "*.txt", SearchOption.AllDirectories);

foreach (var file in txtFiles)
{
    try
    {
        string content = await File.ReadAllTextAsync(file);
        if (string.IsNullOrWhiteSpace(content))
            continue;
        content = content.Replace("\n", " ").Trim();

        // Compute the document embedding.
        ClientResult<OpenAIEmbedding> embedResult = await embeddingClient.GenerateEmbeddingAsync(content);
        float[] docEmbedding = embedResult.Value.ToFloats().ToArray();

        documents.Add(new Document
        {
            FileName = file,
            Content = content,
            Embedding = docEmbedding
        });
        Console.WriteLine($"Processed document: {Path.GetFileName(file)}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing file {file}: {ex.Message}");
    }
}

Console.WriteLine($"Total documents processed: {documents.Count}");

while (true)
{
    Console.WriteLine("\nEnter your question (or type 'exit' to quit):");
    string question = Console.ReadLine();
    if (question.Trim().ToLower() == "exit")
        break;

    ClientResult<OpenAIEmbedding> questionResult = await embeddingClient.GenerateEmbeddingAsync(question);
    float[] questionEmbedding = questionResult.Value.ToFloats().ToArray();

    // Calculate cosine similarity for each document.
    var topDocs = documents.Select(doc => new
        {
            Doc = doc,
            Similarity = CosineSimilarity(questionEmbedding, doc.Embedding)
        })
        .OrderByDescending(x => x.Similarity)
        .Take(3)
        .ToList();

    string contextPrompt = "Based on the following document excerpts:\n";
    foreach (var item in topDocs)
    {
        string excerpt = item.Doc.Content.Length > 500 
            ? item.Doc.Content.Substring(0, 500) + "..." 
            : item.Doc.Content;
        contextPrompt += $"- {Path.GetFileName(item.Doc.FileName)}: {excerpt}\n";
    }
    contextPrompt += "\nAnswer the following question:\n" + question;

    var messages = new List<ChatMessage>();
    messages.Add(new UserChatMessage("You are an AI assistant that answers questions based on the provided context."));
    messages.Add(new UserChatMessage(contextPrompt));

    var chatResponse = await chatClient.CompleteChatAsync(messages);
    string answer = chatResponse.Value.Content.Last().Text;

    Console.WriteLine("\nAnswer:");
    Console.WriteLine(answer);
}

static double CosineSimilarity(float[] vectorA, float[] vectorB)
{
    if (vectorA.Length != vectorB.Length)
    {
        throw new ArgumentException("Vectors must have the same length.");
    }
    double dot = 0, normA = 0, normB = 0;
    for (int i = 0; i < vectorA.Length; i++)
    {
        dot += vectorA[i] * vectorB[i];
        normA += vectorA[i] * vectorA[i];
        normB += vectorB[i] * vectorB[i];
    }
    if (normA == 0 || normB == 0)
    {
        return 0;
    }   
    return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
}


class Document
{
    public string FileName { get; set; }
    public string Content { get; set; }
    public float[] Embedding { get; set; }
}

