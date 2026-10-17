using LibraryLab.Api.Contracts;
using LibraryLab.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryLab.Api.Controllers;

[ApiController]
[Route("api/books")]
public sealed class BooksController(LibraryApplicationService library) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookView>>> List(CancellationToken cancellationToken) =>
        Ok(await library.ListBooksAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookView>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await library.GetBookAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<BookView>> Create(CreateBookRequest request, CancellationToken cancellationToken)
    {
        var result = await library.CreateBookAsync(new(request.Isbn, request.Title, request.Author), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}
