using Microsoft.AspNetCore.Mvc;
using PB.Modules.AttractionDefinition.Application.DTOs;
using PB.Modules.AttractionDefinition.Application.Services;

namespace PB.Modules.AttractionDefinition.Api.Controllers;

[ApiController]
[Route("api/attraction-definitions")]
public class AttractionDefinitionsController : ControllerBase
{
    private readonly IAttractionDefinitionService _service;

    public AttractionDefinitionsController(IAttractionDefinitionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAttractionDefinitionDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? tag,
        [FromQuery] string? city,
        [FromQuery] bool? isComplete)
    {
        var result = await _service.GetAllAsync(tag, city, isComplete);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAttractionDefinitionDto dto)
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

    [HttpPost("{id:guid}/variants")]
    public async Task<IActionResult> AddVariant(Guid id, [FromBody] AddVariantDto dto)
    {
        var result = await _service.AddVariantAsync(id, dto);
        return Ok(result);
    }

    [HttpPut("{id:guid}/variants/{variantId:guid}")]
    public async Task<IActionResult> UpdateVariant(Guid id, Guid variantId, [FromBody] UpdateVariantDto dto)
    {
        var result = await _service.UpdateVariantAsync(id, variantId, dto);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/variants/{variantId:guid}")]
    public async Task<IActionResult> RemoveVariant(Guid id, Guid variantId)
    {
        var result = await _service.RemoveVariantAsync(id, variantId);
        return Ok(result);
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
}
