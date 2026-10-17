using LibraryLab.Api.Contracts;
using LibraryLab.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryLab.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController(LibraryApplicationService library) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserView>>> List(CancellationToken cancellationToken) =>
        Ok(await library.ListUsersAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<UserView>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await library.CreateUserAsync(new(request.Name, request.Email), cancellationToken);
        return CreatedAtAction(nameof(List), result);
    }
}
