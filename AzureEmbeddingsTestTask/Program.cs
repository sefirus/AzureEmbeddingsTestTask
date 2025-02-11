using System.ClientModel;
using AzureEmbeddingsTestTask;
using AzureEmbeddingsTestTask.Interfaces;
using AzureEmbeddingsTestTask.Services;
using AzureOpenAIClient = Azure.AI.OpenAI.AzureOpenAIClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        // Load configuration from environment variables and user secrets
        config.AddEnvironmentVariables();
        config.AddUserSecrets<Program>(optional: true);
    })
    .ConfigureServices((context, services) =>
    {
        var settings = new AzureOpenAISettings();
        context.Configuration.Bind(settings);
        services.AddSingleton(settings);

        if (string.IsNullOrWhiteSpace(settings.EmbeddingsApiKey) 
            || string.IsNullOrWhiteSpace(settings.EmbeddingsEndpoint) 
            || string.IsNullOrWhiteSpace(settings.EmbeddingsDeployment) 
            || string.IsNullOrWhiteSpace(settings.CompletionsApiKey) 
            || string.IsNullOrWhiteSpace(settings.CompletionsEndpoint) 
            || string.IsNullOrWhiteSpace(settings.CompletionsDeployment) 
            || string.IsNullOrWhiteSpace(settings.DocumentsFolder))
        {
            throw new Exception("Missing required Azure OpenAI settings");
        }

        //Singleton is not the best solution, better have some wrapper/pool. But will work for console app 
        services.AddSingleton(new AzureOpenAIClient(
            new Uri(settings.EmbeddingsEndpoint),
            new ApiKeyCredential(settings.EmbeddingsApiKey))); 
        services.AddSingleton(new AzureOpenAIClient(
            new Uri(settings.CompletionsEndpoint),
            new ApiKeyCredential(settings.CompletionsApiKey)));

        services.AddSingleton<IDocumentLoader, DocumentLoader>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<IRagService, RagService>();
    })
    .Build();

var ragService = host.Services.GetRequiredService<IRagService>();
await ragService.RunAsync();
