Test task for the ASP.NET Core position at DevRain

This application demonstrates a simple question–answering system that uses Azure OpenAI’s embeddings and completions APIs to answer user questions based on a set of local documents. The app:
- Loads documents from a specified folder and computes their embeddings.
- Accepts a user question, computes its embedding, and retrieves the top matching documents.
- Constructs a prompt combining document excerpts with the question.
- Generates an answer using the completions API.

## Required Configuration

The application expects the following settings (set via user secrets or environment variables):

- **EmbeddingsApiKey**  
  Your API key for the embeddings endpoint.

- **EmbeddingsEndpoint**  
  The endpoint URL for the embeddings resource (e.g., `https://<your-embeddings-resource>.openai.azure.com/`).

- **EmbeddingsDeployment**  
  The deployment name for your embeddings model (e.g., `text-embedding-3-small`).

- **CompletionsApiKey**  
  Your API key for the completions endpoint.

- **CompletionsEndpoint**  
  The endpoint URL for the completions resource (e.g., `https://<your-completions-resource>.openai.azure.com/`).

- **CompletionsDeployment**  
  The deployment name for your completions model (e.g., `gpt-4o-mini`).

- **DocumentsFolder**  
  The full path to the folder containing your local `.txt` documents.

## Quick Setup & Usage

1. **Set Your Configuration:**  
   Use user secrets or environment variables to set the properties listed above. For example, using user secrets:
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "EmbeddingsApiKey" "your-embeddings-api-key"
   dotnet user-secrets set "EmbeddingsEndpoint" "https://<your-embeddings-resource>.openai.azure.com/"
   dotnet user-secrets set "EmbeddingsDeployment" "text-embedding-3-small"
   dotnet user-secrets set "CompletionsApiKey" "your-completions-api-key"
   dotnet user-secrets set "CompletionsEndpoint" "https://<your-completions-resource>.openai.azure.com/"
   dotnet user-secrets set "CompletionsDeployment" "gpt-4o-mini"
   dotnet user-secrets set "DocumentsFolder" "C:\\Path\\To\\Your\\Documents"
   ```

2. **Build the Project:**  
   Ensure that you have installed the required NuGet packages.

3. **Run the Application:**  
   From the project folder, execute:
   ```bash
   dotnet run
   ```
   The application will:
   - Prompt you for a question.
   - Answer using the completions API.
