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

    public async Task<PagedResult<BookDto>> GetPagedBooksAsync1(int page, int pageSize, CancellationToken cancellationToken)
    {
        var books = await _libraryDbContext.Books
            .AsNoTracking()
            .GroupBy(book => new
            {
                Name = book.Name.Trim().ToLower(),
                Author = book.Author.Trim().ToLower(),
                Genre = book.Genre.Trim().ToLower()
            })
            .Select(group => group
                .OrderByDescending(book => book.ImportDate)
                .ThenByDescending(book => book.Id)
                .First())
            .OrderByDescending(book => book.ImportDate)
            .ThenByDescending(book => book.Id)
            .ToListAsync(cancellationToken);

        var totalCount = books.Count;

        var pagedBooks = books
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(book => new BookDto(
                book.Id,
                book.Name,
                book.Author,
                book.Genre,
                book.ImportDate))
            .ToList();

        return new PagedResult<BookDto>(pagedBooks, totalCount, page, pageSize);
    }

    public async Task<PagedResult<BookDto>> GetPagedBooksAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var books = _libraryDbContext.Books.AsNoTracking();

        var uniqueBooks = books.Where(book =>
            !books.Any(other =>
                other.Name.Trim().ToLower() == book.Name.Trim().ToLower() &&
                other.Author.Trim().ToLower() == book.Author.Trim().ToLower() &&
                other.Genre.Trim().ToLower() == book.Genre.Trim().ToLower() &&
                (
                    other.ImportDate > book.ImportDate || (other.ImportDate == book.ImportDate && other.Id > book.Id)
                )
            )
        );

        var totalCount = await uniqueBooks.CountAsync(cancellationToken);
        var pagedBooks = await uniqueBooks
            .OrderByDescending(book => book.ImportDate)
            .ThenByDescending(book => book.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(book => new BookDto(
                book.Id,
                book.Name,
                book.Author,
                book.Genre,
                book.ImportDate))
            .ToListAsync(cancellationToken);

        return new PagedResult<BookDto>(pagedBooks, totalCount, page, pageSize);
    }
}
