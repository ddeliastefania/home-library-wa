using HomeLibrary.Contracts.Messages;
using System.Text.Json;

namespace HomeLibrary.Worker.Services;

public class BookMessageProcessor
{
    private readonly BookPersistenceService _bookPersistenceService;

    public BookMessageProcessor(BookPersistenceService bookPersistenceService)
    {
        _bookPersistenceService = bookPersistenceService;
    }

    public async Task<BookImportMessage> ProcessAsync(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<BookImportMessage>(body.Span)
            ?? throw new JsonException("Invalid book message.");

        await _bookPersistenceService.SaveAsync(message, cancellationToken);
        return message;
    }
}
