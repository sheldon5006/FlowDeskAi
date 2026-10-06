using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using FlowDesk.AI.Application.Abstractions.Embeddings;

namespace FlowDesk.AI.Infrastructure.Embeddings;

public sealed class OpenAiEmbeddingProvider(
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

        var apiKey = configuration["OPENAI_API_KEY"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OPENAI_API_KEY is missing. Add it to the local .env file.");
        }

        var model = configuration["OPENAI_EMBEDDING_MODEL"] ?? "text-embedding-3-small";

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/embeddings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new
        {
            model,
            input = inputs,
            encoding_format = "float"
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Embedding provider returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<OpenAiEmbeddingResponse>(
            cancellationToken: cancellationToken);

        if (result?.Data is null || result.Data.Count != inputs.Count)
        {
            throw new InvalidOperationException(
                "Embedding provider returned an unexpected number of embeddings.");
        }

        return result.Data
            .OrderBy(x => x.Index)
            .Select(x => x.Embedding)
            .ToArray();
    }

    private sealed class OpenAiEmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<OpenAiEmbeddingItem> Data { get; set; } = [];
    }

    private sealed class OpenAiEmbeddingItem
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = [];
    }
}
