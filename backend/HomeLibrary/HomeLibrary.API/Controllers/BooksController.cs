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
}
