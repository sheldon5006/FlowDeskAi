using FlowDesk.AI.Application.Abstractions.AI;
using FlowDesk.AI.Application.Abstractions.Embeddings;
using FlowDesk.AI.Application.Abstractions.Knowledge;
using FlowDesk.AI.Infrastructure.AI;
using FlowDesk.AI.Infrastructure.Embeddings;
using FlowDesk.AI.Infrastructure.Knowledge;
using FlowDesk.AI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace FlowDesk.AI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFlowDeskInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IFlowDeskAiService, FlowDeskAiService>();

        services.AddHttpClient<ILLMProvider, OllamaChatProvider>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["OLLAMA_BASE_URL"] ?? "http://localhost:11434/");
            client.Timeout = TimeSpan.FromMinutes(5);
        });
        services.AddScoped<IKnowledgeService, KnowledgeService>();
        services.AddScoped<IKnowledgeEmbeddingService, KnowledgeEmbeddingService>();
        services.AddScoped<IKnowledgeIngestionService, KnowledgeIngestionService>();
        services.AddSingleton<IKnowledgeFileTextExtractor, KnowledgeFileTextExtractor>();
        services.AddScoped<IKnowledgeSearchService, KnowledgeSearchService>();

        RegisterEmbeddingProvider(services, configuration);

        services.AddDbContext<FlowDeskDbContext>((_, options) =>
        {
            var connectionString = BuildPostgresConnectionString(configuration);

            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.UseVector();
            });
        });

        return services;
    }

    private static void RegisterEmbeddingProvider(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["EMBEDDING_PROVIDER"]?.Trim().ToLowerInvariant();

        switch (provider)
        {
            case "ollama":
            case null:
            case "":
                services.AddHttpClient<IEmbeddingProvider, OllamaEmbeddingProvider>(client =>
                {
                    client.BaseAddress = new Uri(
                        configuration["OLLAMA_BASE_URL"] ?? "http://localhost:11434/");
                    client.Timeout = TimeSpan.FromMinutes(5);
                });
                break;

            case "openai":
                services.AddHttpClient<IEmbeddingProvider, OpenAiEmbeddingProvider>(client =>
                {
                    client.BaseAddress = new Uri(
                        configuration["OPENAI_BASE_URL"] ?? "https://api.openai.com/");
                    client.Timeout = TimeSpan.FromSeconds(60);
                });
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported EMBEDDING_PROVIDER '{provider}'. Use 'ollama' or 'openai'.");
        }
    }

    private static string BuildPostgresConnectionString(IConfiguration configuration)
    {
        var portValue = configuration["POSTGRES_PORT"] ?? "5432";

        if (!int.TryParse(portValue, out var port))
        {
            throw new InvalidOperationException("POSTGRES_PORT must be a valid integer.");
        }

        var password = configuration["POSTGRES_PASSWORD"];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "POSTGRES_PASSWORD is missing. Add it to the local .env file.");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration["POSTGRES_HOST"] ?? "localhost",
            Port = port,
            Database = configuration["POSTGRES_DB"] ?? "flowdesk",
            Username = configuration["POSTGRES_USER"] ?? "flowdesk",
            Password = password
        };

        return builder.ConnectionString;
    }
}
