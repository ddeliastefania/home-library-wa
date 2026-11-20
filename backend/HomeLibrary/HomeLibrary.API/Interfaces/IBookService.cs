using HomeLibrary.Contracts.DTOs;

namespace HomeLibrary.API.Interfaces;

public interface IBookService
{
    Task<IReadOnlyList<BookDto>> GetAllBooksAsync(CancellationToken cancellationToken);
}
