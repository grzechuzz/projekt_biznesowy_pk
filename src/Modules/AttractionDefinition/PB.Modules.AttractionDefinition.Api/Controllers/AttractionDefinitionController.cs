namespace PB.Modules.AttractionDefinition.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using PB.Modules.AttractionDefinition.Application.DTOs;
using PB.Modules.AttractionDefinition.Application.Services;

[ApiController]
[Route("api/attraction-definitions")]
public class AttractionDefinitionController : ControllerBase
{
    private readonly IAttractionDefinitionService _service;

    public AttractionDefinitionController(IAttractionDefinitionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSingleAttractionDto dto)
    {
        var result = await _service.CreateSingleAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("groups")]
    public async Task<IActionResult> CreateGroup([FromBody] CreateAttractionGroupDto dto)
    {
        var result = await _service.CreateGroupAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        var result = await _service.GetAllAsync(status);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSingleAttractionDto dto)
    {
        var result = await _service.UpdateSingleAsync(id, dto);
        return Ok(result);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var result = await _service.PublishAsync(id);
        return Ok(result);
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id)
    {
        var result = await _service.ArchiveAsync(id);
        return Ok(result);
    }
}
