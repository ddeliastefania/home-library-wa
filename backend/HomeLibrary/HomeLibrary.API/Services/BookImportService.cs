using HomeLibrary.API.Interfaces;
using HomeLibrary.API.Parsing;

namespace HomeLibrary.API.Services;

public class BookImportService
{
    private readonly CsvBookParser _csvBookParser;
    private readonly IBookMessagePublisher _bookMessagePublisher;

    public BookImportService(CsvBookParser csvBookParser, IBookMessagePublisher bookMessagePublisher)
    {
        _csvBookParser = csvBookParser;
        _bookMessagePublisher = bookMessagePublisher;
    }

    public async Task<int> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken)
    {
        ValidateFile(fileName);

        var books = await _csvBookParser.ParseAsync(stream, cancellationToken);

        if (books.Count == 0)
        {
            throw new InvalidDataException("The CSV contains no valid book rows.");
        }

        await _bookMessagePublisher.PublishAsync(books, cancellationToken);

        return books.Count;
    }

    private static void ValidateFile(string fileName)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Only CSV files are accepted.");
        }
    }

}
