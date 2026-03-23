using Microsoft.AspNetCore.Mvc;
using PB.Modules.AttractionDefinition.Application.DTOs;
using PB.Modules.AttractionDefinition.Application.Services;

namespace PB.Modules.AttractionDefinition.Api.Controllers;

[ApiController]
[Route("api/attraction-packages")]
public class AttractionPackagesController : ControllerBase
{
    private readonly IAttractionPackageService _service;

    public AttractionPackagesController(IAttractionPackageService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePackageDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePackageDto dto)
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
