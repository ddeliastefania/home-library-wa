using HomeLibrary.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HomeLibrary.API.Controllers;

[ApiController]
[Route("api/imports")]
public class ImportsController : ControllerBase
{
    private readonly BookImportService _bookImportService;

    public ImportsController(BookImportService bookImportService)
    {
        _bookImportService = bookImportService;
    }


    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Import(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("A non-empty CSV file is required.");
        }

        try
        {
            await using var stream = file.OpenReadStream();

            var acceptedRows = await _bookImportService.ImportAsync(stream, file.FileName, cancellationToken);

            return Accepted(new { AcceptedRows = acceptedRows });
        }
        catch (InvalidDataException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
