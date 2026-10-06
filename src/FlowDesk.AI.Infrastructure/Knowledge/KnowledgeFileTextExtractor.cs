using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FlowDesk.AI.Application.Abstractions.Knowledge;
using UglyToad.PdfPig;

namespace FlowDesk.AI.Infrastructure.Knowledge;

public sealed class KnowledgeFileTextExtractor : IKnowledgeFileTextExtractor
{
    private static readonly HashSet<string> TextExtensions =
        [".txt", ".md", ".csv", ".json"];

    public async Task<string> ExtractAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (fileStream is null || !fileStream.CanRead)
        {
            throw new ArgumentException("The file stream must be readable.", nameof(fileStream));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name is required.", nameof(fileName));
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        var content = extension switch
        {
            ".pdf" => ExtractPdf(fileStream),
            ".docx" => ExtractDocx(fileStream),
            _ when TextExtensions.Contains(extension) =>
                await ExtractTextAsync(fileStream, cancellationToken),
            _ => throw new ArgumentException(
                "Supported file types are .txt, .md, .csv, .json, .pdf, and .docx.",
                nameof(fileName))
        };

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "No readable text was found in the file.",
                nameof(fileName));
        }

        return content.Trim();
    }

    private static string ExtractPdf(Stream fileStream)
    {
        using var document = PdfDocument.Open(fileStream);

        var builder = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            if (!string.IsNullOrWhiteSpace(page.Text))
            {
                builder.AppendLine(page.Text);
            }
        }

        return builder.ToString();
    }

    private static string ExtractDocx(Stream fileStream)
    {
        using var document = WordprocessingDocument.Open(fileStream, false);

        var body = document.MainDocumentPart?.Document?.Body;

        if (body is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            var text = paragraph.InnerText.Trim();

            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine(text);
            }
        }

        return builder.ToString();
    }

    private static async Task<string> ExtractTextAsync(
        Stream fileStream,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(
            fileStream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        return await reader.ReadToEndAsync(cancellationToken);
    }
}
