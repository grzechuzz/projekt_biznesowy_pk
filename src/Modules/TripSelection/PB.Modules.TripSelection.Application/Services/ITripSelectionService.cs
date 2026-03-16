namespace PB.Modules.TripSelection.Application.Services;

using PB.Modules.TripSelection.Application.DTOs;

public interface ITripSelectionService
{
    Task<TripSelectionResultDto> GenerateAsync(GenerateSelectionDto dto);
    Task<TripSelectionResultDto?> GetByIdAsync(Guid id);
}
