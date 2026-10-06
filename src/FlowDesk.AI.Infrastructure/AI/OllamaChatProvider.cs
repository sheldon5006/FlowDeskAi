using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FlowDesk.AI.Application.Abstractions.AI;
using Microsoft.Extensions.Configuration;

namespace FlowDesk.AI.Infrastructure.AI;

public sealed class OllamaChatProvider(
    HttpClient httpClient,
    IConfiguration configuration) : ILLMProvider
{
    public async Task<string> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            throw new ArgumentException("User prompt is required.", nameof(userPrompt));
        }

        var model = configuration["OLLAMA_CHAT_MODEL"] ?? "qwen3:8b";

        var request = new
        {
            model,
            stream = false,
            think = false,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = systemPrompt ?? string.Empty
                },
                new
                {
                    role = "user",
                    content = userPrompt.Trim()
                }
            }
        };

        using var response = await httpClient.PostAsJsonAsync(
            "api/chat",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"Ollama chat provider returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(
            cancellationToken: cancellationToken);

        var content = result?.Message?.Content?.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException(
                "Ollama returned an empty chat response.");
        }

        return content;
    }

    private sealed class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        public OllamaMessage? Message { get; set; }
    }

    private sealed class OllamaMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
