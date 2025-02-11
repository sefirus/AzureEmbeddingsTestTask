namespace AzureEmbeddingsTestTask.Utils;

public static class VectorUtils
{
    public static double CosineSimilarity(float[] vectorA, float[] vectorB)
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

        var cosineSimilarity = dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        return cosineSimilarity;
    }
}