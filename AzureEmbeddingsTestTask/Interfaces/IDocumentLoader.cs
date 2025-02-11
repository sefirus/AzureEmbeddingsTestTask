namespace AzureEmbeddingsTestTask.Interfaces;

public interface IDocumentLoader
{
    Task<List<Document>> LoadDocumentsAsync();
}