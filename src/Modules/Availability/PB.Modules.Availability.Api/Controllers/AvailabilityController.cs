using Microsoft.AspNetCore.Mvc;
using PB.Modules.Availability.Application.DTOs;
using PB.Modules.Availability.Application.Services;

namespace PB.Modules.Availability.Api.Controllers;

[ApiController]
[Route("api/availability")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _service;

    public AvailabilityController(IAvailabilityService service)
    {
        _service = service;
    }

    [HttpPost("pools")]
    public async Task<IActionResult> CreatePool([FromBody] CreateTicketPoolDto dto)
    {
        var result = await _service.CreatePoolAsync(dto);
        return CreatedAtAction(nameof(GetPool), new { id = result.Id }, result);
    }

    [HttpGet("pools")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllPoolsAsync();
        return Ok(result);
    }

    [HttpGet("pools/{id:guid}")]
    public async Task<IActionResult> GetPool(Guid id)
    {
        var result = await _service.GetPoolAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("pools/{id:guid}")]
    public async Task<IActionResult> DeletePool(Guid id)
    {
        await _service.DeletePoolAsync(id);
        return NoContent();
    }

    [HttpGet("pools/by-entry/{catalogEntryId:guid}")]
    public async Task<IActionResult> GetByCatalogEntry(Guid catalogEntryId)
    {
        var result = await _service.GetPoolByCatalogEntryAsync(catalogEntryId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("pools/{id:guid}/reserve")]
    public async Task<IActionResult> Reserve(Guid id, [FromBody] ReserveTicketsDto dto)
    {
        var result = await _service.ReserveAsync(id, dto);
        return Ok(result);
    }

    [HttpPost("pools/{id:guid}/confirm/{reservationId:guid}")]
    public async Task<IActionResult> Confirm(Guid id, Guid reservationId)
    {
        var result = await _service.ConfirmReservationAsync(id, reservationId);
        return Ok(result);
    }

    [HttpPost("pools/{id:guid}/cancel/{reservationId:guid}")]
    public async Task<IActionResult> Cancel(Guid id, Guid reservationId)
    {
        await _service.CancelReservationAsync(id, reservationId);
        return NoContent();
    }

    [HttpPost("pools/{id:guid}/expire")]
    public async Task<IActionResult> ExpireOutdated(Guid id)
    {
        var count = await _service.ExpireOutdatedAsync(id);
        return Ok(new { ExpiredCount = count });
    }

    [HttpPost("pools/{id:guid}/capacity/increase")]
    public async Task<IActionResult> IncreaseCapacity(Guid id, [FromBody] ChangeCapacityDto dto)
    {
        var result = await _service.IncreaseCapacityAsync(id, dto);
        return Ok(result);
    }

    [HttpPost("pools/{id:guid}/capacity/reduce")]
    public async Task<IActionResult> ReduceCapacity(Guid id, [FromBody] ChangeCapacityDto dto)
    {
        var result = await _service.ReduceCapacityAsync(id, dto);
        return Ok(result);
    }

    [HttpGet("check/{catalogEntryId:guid}")]
    public async Task<IActionResult> CheckAvailability(Guid catalogEntryId)
    {
        var result = await _service.CheckAvailabilityAsync(catalogEntryId);
        return Ok(result);
    }
}
