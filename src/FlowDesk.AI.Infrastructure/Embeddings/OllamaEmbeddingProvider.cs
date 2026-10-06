using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FlowDesk.AI.Application.Abstractions.Embeddings;
using Microsoft.Extensions.Configuration;

namespace FlowDesk.AI.Infrastructure.Embeddings;

public sealed class OllamaEmbeddingProvider(
    HttpClient httpClient,
    IConfiguration configuration) : IEmbeddingProvider
{
    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> inputs,
        CancellationToken cancellationToken = default)
    {
        if (inputs.Count == 0)
        {
            return [];
        }

        var model = configuration["OLLAMA_EMBEDDING_MODEL"] ?? "nomic-embed-text";

        var request = new
        {
            model,
            input = inputs
        };

        using var response = await httpClient.PostAsJsonAsync(
            "api/embed",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"Ollama embedding provider returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(
            cancellationToken: cancellationToken);

        if (result?.Embeddings is null || result.Embeddings.Count != inputs.Count)
        {
            throw new InvalidOperationException(
                "Ollama returned an unexpected number of embeddings.");
        }

        return result.Embeddings;
    }

    private sealed class OllamaEmbeddingResponse
    {
        [JsonPropertyName("embeddings")]
        public List<float[]> Embeddings { get; set; } = [];
    }
}
