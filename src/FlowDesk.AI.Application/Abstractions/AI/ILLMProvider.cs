namespace FlowDesk.AI.Application.Abstractions.AI;

public interface ILLMProvider
{
    Task<string> GenerateAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}
