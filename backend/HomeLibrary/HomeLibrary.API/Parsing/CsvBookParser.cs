using HomeLibrary.Contracts.Messages;

namespace HomeLibrary.API.Parsing;

public class CsvBookParser
{
    public async Task<List<BookImportMessage>> ParseAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);

        await ValidateHeaderAsync(reader, cancellationToken);

        return await ParseBooksAsync(reader, cancellationToken);
    }

    private static async Task ValidateHeaderAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        var header = await reader.ReadLineAsync(cancellationToken);

        if (header is null)
        {
            throw new InvalidDataException(
                "The CSV file is empty.");
        }

        var columns = header.TrimStart('\uFEFF')
            .Split(',')
            .Select(column => column.Trim())
            .ToArray();

        var expectedColumns = new[] { "name", "author", "genre" };

        if (!columns.SequenceEqual(
            expectedColumns,
            StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "Expected CSV header: name,author,genre.");
        }
    }

    private static async Task<List<BookImportMessage>> ParseBooksAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        var books = new List<BookImportMessage>();

        while (await reader.ReadLineAsync(cancellationToken)
               is { } line)
        {
            if (TryParseBook(line, out var book))
            {
                books.Add(book!);
            }
        }

        return books;
    }

    private static bool TryParseBook(string line, out BookImportMessage? book)
    {
        book = null;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var values = line.Split(',');
        if (values.Length != 3)
        {
            return false;
        }

        var name = values[0].Trim();
        var author = values[1].Trim();
        var genre = values[2].Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(genre))
        {
            return false;
        }

        book = new BookImportMessage(name, author, genre);

        return true;
    }
}
