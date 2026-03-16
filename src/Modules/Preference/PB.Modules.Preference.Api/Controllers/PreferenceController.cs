namespace PB.Modules.Preference.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using PB.Modules.Preference.Application.DTOs;
using PB.Modules.Preference.Application.Services;

[ApiController]
[Route("api/preferences")]
public class PreferenceController : ControllerBase
{
    private readonly IPreferenceService _service;

    public PreferenceController(IPreferenceService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserPreferenceDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserPreferenceDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return Ok(result);
    }
}
