namespace PB.Modules.Catalog.Application.DTOs;

public record TagDto(string Name, string? Group);

public record MoneyDto(decimal Amount, string Currency);

public record DiscountDto(string Description, decimal? PercentOff, MoneyDto? AmountOff, string? Condition);

public record PricingPeriodDto(DateOnly From, DateOnly To, MoneyDto Price, List<DiscountDto>? Discounts);

public record BookingConstraintDto(string Type, string Key, decimal? MinValue, decimal? MaxValue, List<string>? AllowedValues);

public record CatalogLocationDto(string City, string? Address);

public record CatalogOpeningHoursDto(TimeOnly Open, TimeOnly Close);

public record DateRangeDto(DateOnly From, DateOnly To);

public record CatalogEntryDto(
    Guid Id,
    Guid AttractionComponentId,
    string Name,
    string Description,
    List<TagDto> Tags,
    CatalogLocationDto Location,
    DateRangeDto DateRange,
    CatalogOpeningHoursDto? OpeningHours,
    bool IsEvent,
    string Status,
    List<PricingPeriodDto> PricingPeriods,
    List<BookingConstraintDto> Constraints);

public record CreateCatalogEntryDto(
    Guid AttractionComponentId,
    string Name,
    string Description,
    List<TagDto>? Tags,
    CatalogLocationDto Location,
    DateRangeDto DateRange,
    CatalogOpeningHoursDto? OpeningHours,
    bool IsEvent,
    List<BookingConstraintDto>? Constraints);

public record UpdateCatalogEntryDto(
    string Name,
    string Description,
    CatalogLocationDto Location,
    DateRangeDto DateRange,
    CatalogOpeningHoursDto? OpeningHours,
    bool IsEvent);

public record AddPricingPeriodDto(DateOnly From, DateOnly To, MoneyDto Price, List<DiscountDto>? Discounts);
