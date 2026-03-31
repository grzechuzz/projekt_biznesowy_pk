using PB.Modules.TripSelection.Application.DTOs;

namespace PB.Modules.TripSelection.Application.Services;

public interface ISelectionSessionService
{
    Task<SelectionSessionDto> CreateAsync(CreateSessionDto dto);
    Task<SelectionSessionDto?> GetByIdAsync(Guid id);
    Task<SelectionSessionDto> AddItemAsync(Guid sessionId, AddItemToSessionDto dto);
    Task<SelectionSessionDto> RemoveItemAsync(Guid sessionId, Guid catalogEntryId);
}
