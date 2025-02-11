namespace AzureEmbeddingsTestTask;

public class Document
{
    public string FileName { get; set; }
    public string Content { get; set; }
    public float[] Embedding { get; set; }
}