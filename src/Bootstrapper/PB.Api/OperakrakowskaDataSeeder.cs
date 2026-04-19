using PB.Modules.AttractionDefinition.Domain.Aggregates;
using PB.Modules.AttractionDefinition.Domain.Enums;
using PB.Modules.AttractionDefinition.Domain.Ports;
using PB.Modules.AttractionDefinition.Domain.ValueObjects;
using PB.Modules.Availability.Domain.Aggregates;
using PB.Modules.Availability.Domain.Ports;
using PB.Modules.Catalog.Domain.Aggregates;
using PB.Modules.Catalog.Domain.Ports;
using PB.Modules.Catalog.Domain.ValueObjects;
using PB.Modules.TripSelection.Domain.Aggregates;
using PB.Modules.TripSelection.Domain.Enums;
using PB.Modules.TripSelection.Domain.Ports;
using PB.Shared.Domain;

namespace PB.Api;

public class OperakrakowskaDataSeeder
{
    private readonly IAttractionComponentRepository _componentRepository;
    private readonly ICatalogEntryRepository _catalogRepository;
    private readonly ITicketPoolRepository _ticketPoolRepository;
    private readonly IAttractionRelationRepository _relationRepository;

    public OperakrakowskaDataSeeder(
        IAttractionComponentRepository componentRepository,
        ICatalogEntryRepository catalogRepository,
        ITicketPoolRepository ticketPoolRepository,
        IAttractionRelationRepository relationRepository)
    {
        _componentRepository = componentRepository;
        _catalogRepository = catalogRepository;
        _ticketPoolRepository = ticketPoolRepository;
        _relationRepository = relationRepository;
    }

    public async Task SeedAsync()
    {
        var catalogFrom = new DateOnly(2026, 9, 1);
        var catalogTo = new DateOnly(2027, 6, 30);

        var operaLoc = new Location("Kraków", "ul. Lubicz 48, 31-512 Kraków", 50.0647, 19.9502);

        var attractionSeeds = new[]
        {
            new AttractionSeed("opera-main-stage",
                "Opera Krakowska - Main Stage Performance",
                "Evening opera performance on the main stage of Opera Krakowska. Repertoire includes classic Italian and Polish opera.",
                [Tag("opera", "type"), Tag("music", "category"), Tag("evening", "time"), Tag("performance", "category"), Tag("krakow", "loc")],
                operaLoc, Hours(18, 0, 23, 0)),

            new AttractionSeed("opera-ballet",
                "Opera Krakowska - Ballet Performance",
                "Ballet performance by the Opera Krakowska Ballet Company. Classical and contemporary repertoire.",
                [Tag("ballet", "type"), Tag("dance", "category"), Tag("evening", "time"), Tag("performance", "category"), Tag("krakow", "loc")],
                operaLoc, Hours(18, 0, 22, 30)),

            new AttractionSeed("opera-operetta",
                "Opera Krakowska - Operetta",
                "Light operetta performances suitable for families and first-time opera visitors.",
                [Tag("operetta", "type"), Tag("music", "category"), Tag("family-friendly", "audience"), Tag("performance", "category"), Tag("krakow", "loc")],
                operaLoc, Hours(17, 0, 21, 0)),

            new AttractionSeed("opera-backstage-tour",
                "Opera Krakowska - Backstage Tour",
                "Guided tour of the Opera backstage: costume workshops, stage machinery, and rehearsal halls. Approx. 90 minutes.",
                [Tag("guided-tour", "type"), Tag("backstage", "category"), Tag("educational", "style"), Tag("indoor", "type"), Tag("krakow", "loc")],
                operaLoc, Hours(10, 0, 14, 0)),

            new AttractionSeed("opera-vocal-workshop",
                "Opera Krakowska - Vocal Workshop",
                "One-hour vocal workshop led by an Opera Krakowska soloist. Open to adults with no prior singing experience required.",
                [Tag("workshop", "type"), Tag("educational", "style"), Tag("music", "category"), Tag("indoor", "type"), Tag("krakow", "loc")],
                operaLoc, Hours(10, 0, 16, 0)),

            new AttractionSeed("opera-foyer-bar",
                "Opera Krakowska - Foyer Bar",
                "Elegant foyer bar serving champagne, wine and canapés during interval. Open on performance evenings only.",
                [Tag("bar", "type"), Tag("food", "category"), Tag("evening", "time"), Tag("indoor", "type"), Tag("krakow", "loc")],
                operaLoc, Hours(17, 30, 23, 0)),

            new AttractionSeed("opera-pre-show-dinner",
                "Opera Krakowska - Pre-Show Dinner",
                "Three-course dinner at the Opera's partner restaurant, timed before the evening performance (17:00-19:00).",
                [Tag("dinner", "type"), Tag("food", "category"), Tag("evening", "time"), Tag("indoor", "type"), Tag("krakow", "loc")],
                operaLoc, Hours(17, 0, 19, 0)),
        };

        var componentsByKey = new Dictionary<string, AttractionComponent>();

        foreach (var seed in attractionSeeds)
        {
            var attraction = new AttractionDefinition(seed.Name, seed.Description);
            foreach (var tag in seed.Tags)
                attraction.AddTag(tag);
            attraction.SetLocation(seed.Location);
            attraction.SetOpeningHours(seed.OpeningHours);
            componentsByKey[seed.Key] = attraction;
        }

        var packageSeeds = new[]
        {
            new PackageSeed(
                "opera-evening-standard",
                "Opera Evening - Standard",
                "Pick any one performance (opera, ballet or operetta) and enjoy the foyer bar during interval.",
                SelectionRule.PickN(1),
                ["opera-main-stage", "opera-ballet", "opera-operetta"]),

            new PackageSeed(
                "opera-evening-premium",
                "Opera Evening - Premium",
                "Pre-show dinner, main stage opera performance, and foyer bar included.",
                SelectionRule.All(),
                ["opera-pre-show-dinner", "opera-main-stage", "opera-foyer-bar"]),

            new PackageSeed(
                "opera-discovery-day",
                "Opera Discovery Day",
                "Full immersion: vocal workshop in the morning, backstage tour, and an evening performance.",
                SelectionRule.All(),
                ["opera-vocal-workshop", "opera-backstage-tour", "opera-main-stage"]),

            new PackageSeed(
                "opera-family",
                "Opera Family Package",
                "Operetta performance with a backstage tour - great introduction for children and first-time visitors.",
                SelectionRule.All(),
                ["opera-operetta", "opera-backstage-tour"]),
        };

        foreach (var seed in packageSeeds)
        {
            var package = new AttractionPackage(seed.Name, seed.Description, seed.SelectionRule);
            foreach (var componentKey in seed.ComponentKeys)
                package.AddComponent(componentsByKey[componentKey].Id);
            componentsByKey[seed.Key] = package;
        }

        foreach (var component in componentsByKey.Values)
            await _componentRepository.AddAsync(component);


        var summerOutdoorFrom = new DateOnly(2026, 7, 1);
        var summerOutdoorTo = new DateOnly(2026, 8, 31);

        var catalogSeeds = new[]
        {
            new CatalogSeed("opera-main-stage",
                "Opera Krakowska - Opera Performance",
                "Evening opera on the main stage. Booking required. Dress code: smart casual.",
                [Tag("opera", "type"), Tag("music", "category"), Tag("performance", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Lubicz 48",
                catalogFrom, catalogTo,
                new CatalogOpeningHours(new TimeOnly(18, 0), new TimeOnly(23, 0)),
                IsEvent: true,
                [Constraint("Range", "group_size", 1, 8), Constraint("RequiredDaysAhead", "booking_days_ahead", 3, null)],
                [Pricing(catalogFrom, catalogTo, 180)],
                Capacity: 700),

            new CatalogSeed("opera-ballet",
                "Opera Krakowska - Ballet Performance",
                "Ballet on the main stage. Booking required.",
                [Tag("ballet", "type"), Tag("dance", "category"), Tag("performance", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Lubicz 48",
                catalogFrom, catalogTo,
                new CatalogOpeningHours(new TimeOnly(18, 0), new TimeOnly(22, 30)),
                IsEvent: true,
                [Constraint("Range", "group_size", 1, 8), Constraint("RequiredDaysAhead", "booking_days_ahead", 3, null)],
                [Pricing(catalogFrom, catalogTo, 160)],
                Capacity: 700),

            new CatalogSeed("opera-operetta",
                "Opera Krakowska - Operetta Performance",
                "Light operetta repertoire. Family-friendly. Book at least 1 day ahead.",
                [Tag("operetta", "type"), Tag("family-friendly", "audience"), Tag("performance", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Lubicz 48",
                catalogFrom, catalogTo,
                new CatalogOpeningHours(new TimeOnly(17, 0), new TimeOnly(21, 0)),
                IsEvent: true,
                [Constraint("Range", "group_size", 1, 10), Constraint("RequiredDaysAhead", "booking_days_ahead", 1, null)],
                [Pricing(catalogFrom, catalogTo, 120)],
                Capacity: 700),

            new CatalogSeed("opera-backstage-tour",
                "Opera Krakowska - Backstage Tour",
                "Guided 90-min backstage tour. Groups of max 15. Available on non-performance mornings.",
                [Tag("guided-tour", "type"), Tag("backstage", "category"), Tag("educational", "style"), Tag("krakow", "loc")],
                "Kraków", "ul. Lubicz 48",
                catalogFrom, catalogTo,
                new CatalogOpeningHours(new TimeOnly(10, 0), new TimeOnly(14, 0)),
                IsEvent: false,
                [Constraint("Range", "group_size", 1, 15), Constraint("RequiredDaysAhead", "booking_days_ahead", 2, null)],
                [Pricing(catalogFrom, catalogTo, 45)],
                Capacity: 15),

            new CatalogSeed("opera-vocal-workshop",
                "Opera Krakowska - Vocal Workshop",
                "One-hour workshop with a professional soloist. Max 10 participants. No experience needed.",
                [Tag("workshop", "type"), Tag("educational", "style"), Tag("music", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Lubicz 48",
                catalogFrom, catalogTo,
                new CatalogOpeningHours(new TimeOnly(10, 0), new TimeOnly(16, 0)),
                IsEvent: false,
                [Constraint("Range", "group_size", 1, 10), Constraint("RequiredDaysAhead", "booking_days_ahead", 3, null)],
                [Pricing(catalogFrom, catalogTo, 90)],
                Capacity: 10),

            new CatalogSeed("opera-foyer-bar",
                "Opera Krakowska - Foyer Bar (interval)",
                "Champagne or wine with canapés during the performance interval. Added per person.",
                [Tag("bar", "type"), Tag("food", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Lubicz 48",
                catalogFrom, catalogTo,
                new CatalogOpeningHours(new TimeOnly(17, 30), new TimeOnly(23, 0)),
                IsEvent: false,
                [Constraint("Range", "group_size", 1, 8)],
                [Pricing(catalogFrom, catalogTo, 55)],
                Capacity: 120),

            new CatalogSeed("opera-pre-show-dinner",
                "Opera Krakowska - Pre-Show Dinner",
                "Three-course dinner 17:00-19:00 at partner restaurant. Available on performance days only.",
                [Tag("dinner", "type"), Tag("food", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Lubicz 48",
                catalogFrom, catalogTo,
                new CatalogOpeningHours(new TimeOnly(17, 0), new TimeOnly(19, 0)),
                IsEvent: false,
                [Constraint("Range", "group_size", 1, 6), Constraint("RequiredDaysAhead", "booking_days_ahead", 2, null)],
                [Pricing(catalogFrom, catalogTo, 150)],
                Capacity: 30),

            new CatalogSeed("opera-evening-premium",
                "Opera Krakowska - Premium Evening Package",
                "Pre-show dinner + main stage opera + foyer bar. Complete opera experience in one booking.",
                [Tag("opera", "type"), Tag("premium", "tier"), Tag("krakow", "loc")],
                "Kraków", "ul. Lubicz 48",
                catalogFrom, catalogTo,
                new CatalogOpeningHours(new TimeOnly(17, 0), new TimeOnly(23, 0)),
                IsEvent: false,
                [Constraint("Range", "group_size", 1, 6), Constraint("RequiredDaysAhead", "booking_days_ahead", 3, null)],
                [Pricing(catalogFrom, catalogTo, 370)],
                Capacity: 30),

            new CatalogSeed("opera-discovery-day",
                "Opera Krakowska - Discovery Day Package",
                "Vocal workshop + backstage tour + evening opera. Full day immersion.",
                [Tag("educational", "style"), Tag("opera", "type"), Tag("krakow", "loc")],
                "Kraków", "ul. Lubicz 48",
                catalogFrom, catalogTo,
                new CatalogOpeningHours(new TimeOnly(10, 0), new TimeOnly(23, 0)),
                IsEvent: false,
                [Constraint("Range", "group_size", 1, 8), Constraint("RequiredDaysAhead", "booking_days_ahead", 5, null)],
                [Pricing(catalogFrom, catalogTo, 290)],
                Capacity: 10),
        };

        var catalogEntriesByKey = new Dictionary<string, CatalogEntry>();

        foreach (var seed in catalogSeeds)
        {
            var entry = new CatalogEntry(
                componentsByKey[seed.ComponentKey].Id,
                seed.Name,
                seed.Description,
                new CatalogLocation(seed.City, seed.Address),
                new PB.Modules.Catalog.Domain.ValueObjects.DateRange(seed.From, seed.To),
                seed.IsEvent,
                seed.Tags,
                seed.Constraints);

            entry.SetOpeningHours(seed.OpeningHours);
            foreach (var pricing in seed.PricingPeriods)
                entry.AddPricingPeriod(pricing);

            catalogEntriesByKey[seed.ComponentKey] = entry;
            await _catalogRepository.AddAsync(entry);
            await _ticketPoolRepository.AddAsync(new TicketPool(entry.Id, seed.Capacity));
        }


        var relationSeeds = new[]
        {

            new RelationSeed("opera-main-stage",    "opera-foyer-bar",
                RelationType.Suggests, "intermission",
                "Foyer bar is open during the interval - natural add-on for any main stage performance."),

            new RelationSeed("opera-ballet",        "opera-foyer-bar",
                RelationType.Suggests, "intermission",
                "Foyer bar is open during ballet intervals."),

            new RelationSeed("opera-operetta",      "opera-foyer-bar",
                RelationType.Suggests, "intermission",
                "Foyer bar is available during operetta intermissions too."),

            new RelationSeed("opera-main-stage",    "opera-pre-show-dinner",
                RelationType.Suggests, "pre-show",
                "Pre-show dinner at 17:00 pairs well with an 18:00 curtain."),

            new RelationSeed("opera-backstage-tour", "opera-main-stage",
                RelationType.Suggests, "same-day",
                "Seeing the backstage in the morning makes the evening performance even more engaging."),

            new RelationSeed("opera-vocal-workshop", "opera-backstage-tour",
                RelationType.Suggests, "educational",
                "Combine workshop and backstage tour for a full educational day at the Opera."),

            new RelationSeed("opera-operetta",      "opera-backstage-tour",
                RelationType.Suggests, "family",
                "Backstage tour is a great complement to the operetta for families with children."),


            new RelationSeed("opera-pre-show-dinner", "opera-main-stage",
                RelationType.Requires, "pre-show",
                "Pre-show dinner is only available on opera performance days."),

            new RelationSeed("opera-main-stage",    "opera-ballet",
                RelationType.Excludes, "time_conflict",
                "Both performances run in the evening on the same stage - cannot attend both on the same night."),

            new RelationSeed("opera-main-stage",    "opera-operetta",
                RelationType.Excludes, "time_conflict",
                "Opera and operetta share the main stage - only one performance per evening."),

            new RelationSeed("opera-ballet",        "opera-operetta",
                RelationType.Excludes, "time_conflict",
                "Ballet and operetta share the main stage - only one performance per evening."),

            new RelationSeed("opera-backstage-tour", "opera-main-stage",
                RelationType.Excludes, "schedule_conflict",
                "Backstage tour runs 10:00-14:00 but the crew needs the stage from 14:00 for performance prep - do not book both unless the tour ends before 13:00."),
        };

        foreach (var seed in relationSeeds)
        {
            var relation = new AttractionRelation(
                componentsByKey[seed.SourceKey].Id,
                componentsByKey[seed.TargetKey].Id,
                seed.Type,
                seed.Context,
                seed.Description);
            await _relationRepository.AddAsync(relation);
        }

        Console.WriteLine("=== OPERA KRAKOWSKA SEEDER ===");
        Console.WriteLine($"Components:   {componentsByKey.Count} (attractions + packages)");
        Console.WriteLine($"Catalog:      {catalogEntriesByKey.Count} entries");
        Console.WriteLine($"Pools:        {catalogEntriesByKey.Count}");
        Console.WriteLine($"Relations:    {relationSeeds.Length}");
        Console.WriteLine("==============================");
        Console.WriteLine();
        Console.WriteLine("Key catalog IDs for testing:");
        foreach (var key in new[] { "opera-main-stage", "opera-ballet", "opera-operetta", "opera-backstage-tour", "opera-vocal-workshop", "opera-evening-premium", "opera-discovery-day" })
            Console.WriteLine($"  {catalogEntriesByKey[key].Name}: {catalogEntriesByKey[key].Id}");
        Console.WriteLine("==============================");
    }

    private static Tag Tag(string name, string? group = null) => new(name, group);

    private static OpeningHours Hours(int openHour, int openMinute, int closeHour, int closeMinute) =>
        new(new TimeOnly(openHour, openMinute), new TimeOnly(closeHour, closeMinute));

    private static BookingConstraint Constraint(string type, string key, decimal? minValue, decimal? maxValue, params string[] allowedValues) =>
        new(type, key, minValue, maxValue, allowedValues.Length == 0 ? Array.Empty<string>() : allowedValues);

    private static PricingPeriod Pricing(DateOnly from, DateOnly to, decimal amount) =>
        new(new PB.Modules.Catalog.Domain.ValueObjects.DateRange(from, to), new Money(amount, "PLN"));

    private sealed record AttractionSeed(
        string Key,
        string Name,
        string Description,
        IReadOnlyList<Tag> Tags,
        Location Location,
        OpeningHours? OpeningHours);

    private sealed record PackageSeed(
        string Key,
        string Name,
        string Description,
        SelectionRule SelectionRule,
        IReadOnlyList<string> ComponentKeys);

    private sealed record CatalogSeed(
        string ComponentKey,
        string Name,
        string Description,
        IReadOnlyList<Tag> Tags,
        string City,
        string? Address,
        DateOnly From,
        DateOnly To,
        CatalogOpeningHours? OpeningHours,
        bool IsEvent,
        IReadOnlyList<BookingConstraint> Constraints,
        IReadOnlyList<PricingPeriod> PricingPeriods,
        int Capacity);

    private sealed record RelationSeed(
        string SourceKey,
        string TargetKey,
        RelationType Type,
        string? Context,
        string? Description);
}