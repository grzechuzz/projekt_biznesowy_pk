using Microsoft.AspNetCore.Mvc;
using PB.Modules.Catalog.Application.DTOs;
using PB.Modules.Catalog.Application.Services;

namespace PB.Modules.Catalog.Api.Controllers;

[ApiController]
[Route("api/catalog/entries")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _service;

    public CatalogController(ICatalogService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCatalogEntryDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? city,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? tags,
        [FromQuery] string? status)
    {
        var tagList = tags?.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t));
        var result = await _service.SearchAsync(city, from, to, tagList, status);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCatalogEntryDto dto)
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

    [HttpPost("{id:guid}/pricing")]
    public async Task<IActionResult> AddPricing(Guid id, [FromBody] AddPricingPeriodDto dto)
    {
        var result = await _service.AddPricingPeriodAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/pricing/{index:int}")]
    public async Task<IActionResult> RemovePricing(Guid id, int index)
    {
        var result = await _service.RemovePricingPeriodAsync(id, index);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _service.CancelAsync(id);
        return Ok(result);
    }

    [HttpPost("{id:guid}/sold-out")]
    public async Task<IActionResult> SoldOut(Guid id)
    {
        var result = await _service.MarkAsSoldOutAsync(id);
        return Ok(result);
    }

    [HttpPost("{id:guid}/available")]
    public async Task<IActionResult> Available(Guid id)
    {
        var result = await _service.MarkAsAvailableAsync(id);
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
