using Microsoft.AspNetCore.Mvc;
using PB.Modules.TripSelection.Application.DTOs;
using PB.Modules.TripSelection.Application.Services;

namespace PB.Modules.TripSelection.Api.Controllers;

[ApiController]
[Route("api/trip-selections/sessions")]
public class SelectionSessionsController : ControllerBase
{
    private readonly ISelectionSessionService _service;

    public SelectionSessionsController(ISelectionSessionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSessionDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddItemToSessionDto dto)
    {
        var result = await _service.AddItemAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/items/{catalogEntryId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid id, Guid catalogEntryId)
    {
        var result = await _service.RemoveItemAsync(id, catalogEntryId);
        return Ok(result);
    }
}
