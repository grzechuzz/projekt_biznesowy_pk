namespace PB.Modules.TripSelection.Application.Services;

using PB.Modules.Catalog.Application.Services;
using PB.Modules.Preference.Application.Services;
using PB.Modules.TripSelection.Application.DTOs;
using PB.Modules.TripSelection.Domain.Entities;
using PB.Modules.TripSelection.Domain.Repositories;
using PB.Modules.TripSelection.Domain.Services;

public class TripSelectionService : ITripSelectionService
{
    private readonly ITripSelectionResultRepository _repository;
    private readonly ICatalogService _catalogService;
    private readonly IPreferenceService _preferenceService;
    private readonly ISelectionStrategy _selectionStrategy;

    public TripSelectionService(
        ITripSelectionResultRepository repository,
        ICatalogService catalogService,
        IPreferenceService preferenceService,
        ISelectionStrategy selectionStrategy)
    {
        _repository = repository;
        _catalogService = catalogService;
        _preferenceService = preferenceService;
        _selectionStrategy = selectionStrategy;
    }

    public async Task<TripSelectionResultDto> GenerateAsync(GenerateSelectionDto dto)
    {
        var preference = await _preferenceService.GetByIdAsync(dto.PreferenceId)
            ?? throw new InvalidOperationException($"Preference {dto.PreferenceId} not found.");

        var catalogEntries = await _catalogService.SearchAsync(
            preference.DestinationCity,
            preference.StartDate,
            preference.EndDate,
            null);

        var result = new TripSelectionResult(
            Guid.NewGuid(),
            preference.Id,
            preference.DestinationCity,
            preference.StartDate,
            preference.EndDate);

        foreach (var entry in catalogEntries)
        {
            var score = _selectionStrategy.CalculateMatchScore(
                entry.Category,
                entry.IsEvent,
                preference.PreferredCategories);

            if (score <= 0) continue;

            var selected = new SelectedAttraction(
                Guid.NewGuid(),
                entry.Id,
                entry.Name,
                entry.Description,
                entry.Category,
                entry.City,
                entry.Address,
                entry.IsEvent,
                score);

            if (_selectionStrategy.IsMustHave(score))
                result.AddMustHave(selected);
            else
                result.AddOptional(selected);
        }

        await _repository.AddAsync(result);
        return MapToDto(result);
    }

    public async Task<TripSelectionResultDto?> GetByIdAsync(Guid id)
    {
        var result = await _repository.GetByIdAsync(id);
        return result is null ? null : MapToDto(result);
    }

    private static TripSelectionResultDto MapToDto(TripSelectionResult r)
    {
        return new TripSelectionResultDto(
            r.Id,
            r.PreferenceId,
            r.DestinationCity,
            r.StartDate,
            r.EndDate,
            r.MustHave.Select(MapAttractionDto).ToList(),
            r.Optional.Select(MapAttractionDto).ToList());
    }

    private static SelectedAttractionDto MapAttractionDto(SelectedAttraction a)
    {
        return new SelectedAttractionDto(
            a.Id,
            a.CatalogEntryId,
            a.Name,
            a.Description,
            a.Category,
            a.City,
            a.Address,
            a.IsEvent,
            a.MatchScore);
    }
}
