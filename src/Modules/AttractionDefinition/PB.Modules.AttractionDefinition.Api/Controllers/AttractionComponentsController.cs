using Microsoft.AspNetCore.Mvc;
using PB.Modules.AttractionDefinition.Application.DTOs;
using PB.Modules.AttractionDefinition.Application.Services;

namespace PB.Modules.AttractionDefinition.Api.Controllers;

[ApiController]
[Route("api/attraction-components")]
public class AttractionComponentsController : ControllerBase
{
    private readonly IAttractionComponentService _service;

    public AttractionComponentsController(IAttractionComponentService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAttractionComponentDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? type,
        [FromQuery] string? tag,
        [FromQuery] string? city,
        [FromQuery] bool? isComplete)
    {
        var result = await _service.GetAllAsync(type, tag, city, isComplete);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttractionComponentDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/tags")]
    public async Task<IActionResult> AddTag(Guid id, [FromBody] TagDto tag)
    {
        var result = await _service.AddTagAsync(id, tag);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/tags")]
    public async Task<IActionResult> RemoveTag(Guid id, [FromBody] TagDto tag)
    {
        var result = await _service.RemoveTagAsync(id, tag);
        return Ok(result);
    }

    [HttpPost("{id:guid}/components/{componentId:guid}")]
    public async Task<IActionResult> AddComponent(Guid id, Guid componentId)
    {
        var result = await _service.AddComponentAsync(id, componentId);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/components/{componentId:guid}")]
    public async Task<IActionResult> RemoveComponent(Guid id, Guid componentId)
    {
        var result = await _service.RemoveComponentAsync(id, componentId);
        return Ok(result);
    }
}
