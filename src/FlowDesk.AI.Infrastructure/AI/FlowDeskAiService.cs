using System.Text;
using FlowDesk.AI.Application.Abstractions.AI;
using FlowDesk.AI.Application.Abstractions.Knowledge;
using FlowDesk.AI.Application.AI;
using Microsoft.Extensions.Configuration;

namespace FlowDesk.AI.Infrastructure.AI;

public sealed class FlowDeskAiService(
    IKnowledgeSearchService knowledgeSearchService,
    ILLMProvider llmProvider,
    IConfiguration configuration) : IFlowDeskAiService
{
    public Task<string> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            "FlowDesk AI can store business knowledge, find relevant information, and generate answers from that knowledge.");
    }

    public async Task<AiAnswerDto> AskAsync(
        Guid businessId,
        string question,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
        {
            throw new ArgumentException("Business ID is required.", nameof(businessId));
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Question is required.", nameof(question));
        }

        var sources = await knowledgeSearchService.SearchAsync(
            businessId,
            question,
            topK,
            cancellationToken);

        var minimumSimilarity = double.TryParse(
            configuration["RAG_MIN_SIMILARITY"],
            out var configuredThreshold)
            ? configuredThreshold
            : 0.25;

        var relevantSources = sources
            .Where(x => x.Similarity >= minimumSimilarity)
            .ToList();

        if (relevantSources.Count == 0)
        {
            throw new InvalidOperationException(
                "The selected business does not contain enough relevant information to answer this question.");
        }

        var context = new StringBuilder();

        foreach (var source in relevantSources)
        {
            context.AppendLine($"Source: {source.Source}");
            context.AppendLine($"Content: {source.Content}");
            context.AppendLine();
        }

        var systemPrompt = """
            You are FlowDesk AI.
            Answer the user's question using only the provided knowledge context for the selected business.
            Never use knowledge from another business.
            Do not invent facts that are not present in the context.
            If the context does not contain the answer, say that the selected business's knowledge base does not contain enough information.
            Keep the answer clear and concise.
            """;

        var userPrompt = $"""
            Knowledge context for the selected business:
            {context}

            User question:
            {question}
            """;

        var answer = await llmProvider.GenerateAsync(
            systemPrompt,
            userPrompt,
            cancellationToken);

        return new AiAnswerDto(answer, relevantSources);
    }
}
