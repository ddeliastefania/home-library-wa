using HomeLibrary.Contracts.Messages;
using HomeLibrary.Persistence.Data;
using HomeLibrary.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeLibrary.Worker.Services;

public class BookPersistenceService
{
    private readonly IDbContextFactory<LibraryDbContext> _libraryDbContextFactory;

    public BookPersistenceService(IDbContextFactory<LibraryDbContext> libraryDbContextFactory)
    {
        _libraryDbContextFactory = libraryDbContextFactory;
    }

    public async Task SaveAsync(BookImportMessage message, CancellationToken cancellationToken)
    {
        await using var dbContext = await _libraryDbContextFactory.CreateDbContextAsync(cancellationToken);

        var book = new Book
        {
            Name = message.Name,
            Author = message.Author,
            Genre = message.Genre,
            ImportDate = DateTime.UtcNow
        };

        dbContext.Books.Add(book);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
