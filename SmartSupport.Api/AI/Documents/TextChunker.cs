
using SmartSupport.Api.AI.Documents.Dtos;
using SmartSupport.Api.AI.Tokenizer;

namespace SmartSupport.Api.AI.Documents;

public class TextChunker
{
    private const int ChunkSize = 500;
    private const int OverlapSize = 80;
    private const int ContentSize = ChunkSize - OverlapSize;

    private static readonly string[] Separators =
    [
        "\n\n",
        "\n",
        ". ",
        "? ",
        "! ",
        " "
    ];

    private readonly TokenizerService _tokenizer;

    public TextChunker(TokenizerService tokenizer)
    {
        _tokenizer = tokenizer;
    }

    public List<DocumentChunk> ChunkPages(
        List<ExtractedPdfPage> pages)
    {
        var chunks = new List<DocumentChunk>();

        foreach (var page in pages)
        {
            var text = page.Text
                .Replace("\r\n", "\n")
                .Trim();

            if (string.IsNullOrWhiteSpace(text))
                continue;

            var pageChunks = SplitRecursively(text, 0);

            for (var i = 0; i < pageChunks.Count; i++)
            {
                var chunkText = pageChunks[i];

                if (i > 0)
                {
                    var overlap = GetOverlapText(
                        pageChunks[i - 1]);

                    chunkText = CombineWithOverlap(
                        overlap,
                        chunkText);
                }

                chunks.Add(new DocumentChunk
                {
                    ChunkIndex = chunks.Count,
                    PageNumber = page.PageNumber,
                    Text = chunkText
                });
            }
        }

        return chunks;
    }

    private List<string> SplitRecursively(
        string text,
        int separatorIndex)
    {
        if (_tokenizer.CountTokens(text) <= ContentSize)
            return [text];

        // No more separators: use word-level splitting.
        if (separatorIndex >= Separators.Length)
            return SplitByWords(text);

        var separator = Separators[separatorIndex];

        var parts = text.Split(
            separator,
            StringSplitOptions.None);

        if (parts.Length == 1)
        {
            return SplitRecursively(
                text,
                separatorIndex + 1);
        }

        var result = new List<string>();
        var current = string.Empty;

        for (var i = 0; i < parts.Length; i++)
        {
            var segment = parts[i];

            if (i < parts.Length - 1)
                segment += separator;

            if (string.IsNullOrWhiteSpace(segment))
                continue;

            var candidate = current + segment;

            if (_tokenizer.CountTokens(candidate) <= ContentSize)
            {
                current = candidate;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(current))
            {
                result.Add(current.Trim());
                current = string.Empty;
            }

            if (_tokenizer.CountTokens(segment) > ContentSize)
            {
                result.AddRange(
                    SplitRecursively(
                        segment.Trim(),
                        separatorIndex + 1));
            }
            else
            {
                current = segment;
            }
        }

        if (!string.IsNullOrWhiteSpace(current))
            result.Add(current.Trim());

        return result;
    }

    private List<string> SplitByWords(string text)
    {
        var words = text.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries);

        var result = new List<string>();
        var current = string.Empty;

        foreach (var word in words)
        {
            // Handle an unusually long word with no spaces.
            if (_tokenizer.CountTokens(word) > ContentSize)
            {
                if (!string.IsNullOrWhiteSpace(current))
                {
                    result.Add(current);
                    current = string.Empty;
                }

                result.AddRange(SplitByCharacters(word));
                continue;
            }

            var candidate = string.IsNullOrEmpty(current)
                ? word
                : $"{current} {word}";

            if (_tokenizer.CountTokens(candidate) <= ContentSize)
            {
                current = candidate;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(current))
                    result.Add(current);

                current = word;
            }
        }

        if (!string.IsNullOrWhiteSpace(current))
            result.Add(current);

        return result;
    }

    private List<string> SplitByCharacters(string text)
    {
        if (_tokenizer.CountTokens(text) <= ContentSize)
            return [text];

        if (text.Length <= 1)
        {
            throw new InvalidOperationException(
                "A single character exceeds the configured token limit.");
        }

        var middle = text.Length / 2;

        var left = text[..middle];
        var right = text[middle..];

        return
        [
            .. SplitByCharacters(left),
            .. SplitByCharacters(right)
        ];
    }

    private string GetOverlapText(string previousChunk)
    {
        var words = previousChunk.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries);

        var overlapWords = new List<string>();

        for (var i = words.Length - 1; i >= 0; i--)
        {
            var candidate = string.Join(
                " ",
                new[] { words[i] }.Concat(overlapWords));

            if (_tokenizer.CountTokens(candidate) > OverlapSize)
                break;

            overlapWords.Insert(0, words[i]);
        }

        return string.Join(" ", overlapWords);
    }

    private string CombineWithOverlap(
        string overlap,
        string currentChunk)
    {
        if (string.IsNullOrWhiteSpace(overlap))
            return currentChunk;

        var combined = $"{overlap}\n{currentChunk}";

        while (_tokenizer.CountTokens(combined) > ChunkSize)
        {
            overlap = RemoveFirstWord(overlap);

            if (string.IsNullOrWhiteSpace(overlap))
                return currentChunk;

            combined = $"{overlap}\n{currentChunk}";
        }

        return combined;
    }

    private static string RemoveFirstWord(string text)
    {
        var firstSpaceIndex = text.IndexOf(' ');

        if (firstSpaceIndex < 0)
            return string.Empty;

        return text[(firstSpaceIndex + 1)..].Trim();
    }
}
