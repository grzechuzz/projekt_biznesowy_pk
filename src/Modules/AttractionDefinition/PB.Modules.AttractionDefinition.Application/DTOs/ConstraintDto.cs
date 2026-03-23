namespace PB.Modules.AttractionDefinition.Application.DTOs;

public record ConstraintDto(string Type, string Key, decimal? MinValue, decimal? MaxValue, List<string>? AllowedValues);
