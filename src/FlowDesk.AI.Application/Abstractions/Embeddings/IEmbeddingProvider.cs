namespace FlowDesk.AI.Application.Abstractions.Embeddings;

public interface IEmbeddingProvider
{
    Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> inputs,
        CancellationToken cancellationToken = default);
}
