using HomeLibrary.API.Interfaces;
using HomeLibrary.Contracts.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HomeLibrary.API.Controllers;

[ApiController]
[Route("api/books")]
public class BooksController : Controller
{
    private readonly IBookService _bookService;
    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetBooks(CancellationToken cancellationToken)
    {
        var books = await _bookService.GetAllBooksAsync(cancellationToken);

        return Ok(books);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<BookDto>>> GetPagedBooks([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Invalid pagination parameters.");
        }

        var result = await _bookService.GetPagedBooksAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }
}
