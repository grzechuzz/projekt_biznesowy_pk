namespace PB.Modules.TripSelection.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using PB.Modules.TripSelection.Application.DTOs;
using PB.Modules.TripSelection.Application.Services;

[ApiController]
[Route("api/trip-selections")]
public class TripSelectionController : ControllerBase
{
    private readonly ITripSelectionService _service;

    public TripSelectionController(ITripSelectionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] GenerateSelectionDto dto)
    {
        var result = await _service.GenerateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}
