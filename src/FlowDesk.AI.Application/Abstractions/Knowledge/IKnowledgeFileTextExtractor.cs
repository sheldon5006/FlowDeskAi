namespace FlowDesk.AI.Application.Abstractions.Knowledge;

public interface IKnowledgeFileTextExtractor
{
    Task<string> ExtractAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default);
}
