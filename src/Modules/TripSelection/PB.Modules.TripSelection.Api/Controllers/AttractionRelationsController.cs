using Microsoft.AspNetCore.Mvc;
using PB.Modules.TripSelection.Application.DTOs;
using PB.Modules.TripSelection.Application.Services;

namespace PB.Modules.TripSelection.Api.Controllers;

[ApiController]
[Route("api/trip-selections/relations")]
public class AttractionRelationsController : ControllerBase
{
    private readonly IAttractionRelationService _service;

    public AttractionRelationsController(IAttractionRelationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRelationDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
