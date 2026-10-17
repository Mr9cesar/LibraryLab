using LibraryLab.Api.Contracts;
using LibraryLab.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryLab.Api.Controllers;

[ApiController]
[Route("api/equipment")]
public sealed class EquipmentController(LibraryApplicationService library) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EquipmentView>>> List(CancellationToken cancellationToken) =>
        Ok(await library.ListEquipmentAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EquipmentView>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await library.GetEquipmentAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<EquipmentView>> Create(CreateEquipmentRequest request, CancellationToken cancellationToken)
    {
        var result = await library.CreateEquipmentAsync(
            new(request.InventoryCode, request.Name, request.Type), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}
