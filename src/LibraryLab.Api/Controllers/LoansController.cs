using LibraryLab.Api.Contracts;
using LibraryLab.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryLab.Api.Controllers;

[ApiController]
[Route("api/loans")]
public sealed class LoansController(LibraryApplicationService library) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LoanView>>> List(CancellationToken cancellationToken) =>
        Ok(await library.ListLoansAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LoanView>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await library.GetLoanAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<LoanView>> Create(CreateLoanRequest request, CancellationToken cancellationToken)
    {
        var result = await library.CreateLoanAsync(
            new(request.UserId, request.ResourceId, request.DueAt), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<LoanView>> Return(Guid id, CancellationToken cancellationToken) =>
        Ok(await library.ReturnLoanAsync(id, cancellationToken));
}
