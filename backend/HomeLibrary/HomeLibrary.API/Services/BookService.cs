using HomeLibrary.API.Interfaces;
using HomeLibrary.Contracts.DTOs;
using HomeLibrary.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeLibrary.API.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext _libraryDbContext;

    public BookService(LibraryDbContext libraryDbContext)
    {
        _libraryDbContext = libraryDbContext;
    }
    public async Task<IReadOnlyList<BookDto>> GetAllBooksAsync(CancellationToken cancellationToken)
    {
        return await _libraryDbContext.Books
                    .AsNoTracking()
                    .OrderByDescending(book => book.ImportDate)
                    .ThenByDescending(book => book.Id)
                    .Select(book => new BookDto(
                        book.Id,
                        book.Name,
                        book.Author,
                        book.Genre,
                        book.ImportDate))
                    .ToListAsync(cancellationToken);
    }
}
