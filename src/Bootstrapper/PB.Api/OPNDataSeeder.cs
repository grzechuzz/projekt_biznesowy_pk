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

public class OpnDataSeeder
{
    private readonly IAttractionComponentRepository _componentRepository;
    private readonly ICatalogEntryRepository _catalogRepository;
    private readonly ITicketPoolRepository _ticketPoolRepository;
    private readonly IAttractionRelationRepository _relationRepository;

    public OpnDataSeeder(
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
        var catalogFrom = new DateOnly(2026, 4, 20);
        var catalogTo = new DateOnly(2026, 10, 31);

        var attractionSeeds = new[]
        {
            new AttractionSeed("opn-zamek-ojcow", "Zamek Kazimierzowski w Ojcowie",
                "Ruins of a 14th-century fortress with a neo-Gothic gate tower and an exhibition. Observation deck overlooking Dolina Prądnika.",
                [Tag("castle", "type"), Tag("ruins", "type"), Tag("history", "category"), Tag("viewpoint", "category"), Tag("ojcow", "loc")],
                new Location("Ojców", "Ojców 9", 50.2113, 19.8315),
                Hours(9, 0, 16, 30)),

            new AttractionSeed("opn-jaskinia-lokietka", "Jaskinia Łokietka",
                "The largest cave in the park (320m). According to legend, a royal shelter. Constant temperature of 7-8°C.",
                [Tag("cave", "type"), Tag("legend", "category"), Tag("nature", "category"), Tag("guided-tour", "type"), Tag("ojcow", "loc")],
                new Location("Ojców", "Góra Chełmowa", 50.2015, 19.8277),
                Hours(9, 0, 18, 30)),

            new AttractionSeed("opn-jaskinia-ciemna", "Jaskinia Ciemna",
                "Valuable archaeological site with a Neanderthal campsite. No artificial lighting.",
                [Tag("cave", "type"), Tag("archaeology", "category"), Tag("nature", "category"), Tag("guided-tour", "type"), Tag("ojcow", "loc")],
                new Location("Ojców", "Góra Koronna", 50.1972, 19.8242),
                Hours(9, 0, 16, 0)),

            new AttractionSeed("opn-ekspozycja", "Ekspozycja Przyrodnicza OPN",
                "Modern natural history museum featuring a 3D film about the park's geological history and a diorama.",
                [Tag("museum", "type"), Tag("nature", "category"), Tag("indoor", "type"), Tag("family-friendly", "audience"), Tag("ojcow", "loc")],
                new Location("Ojców", "Ojców 9", 50.2110, 19.8300),
                Hours(9, 0, 15, 0)),

            new AttractionSeed("opn-pieskowa-skala", "Zamek w Pieskowej Skale",
                "Renaissance magnate residence (Branch of the Wawel Royal Castle).",
                [Tag("castle", "type"), Tag("renaissance", "style"), Tag("museum", "type"), Tag("history", "category"), Tag("suloszowa", "loc")],
                new Location("Sułoszowa", "Sułoszowa 5", 50.2443, 19.7801),
                Hours(9, 0, 17, 0)),

            new AttractionSeed("opn-brama-krakowska", "Brama Krakowska",
                "A 15-meter rock gate, a classic landmark of the valley.",
                [Tag("rock-formation", "type"), Tag("landmark", "category"), Tag("outdoor", "type"), Tag("ojcow", "loc")],
                new Location("Ojców", "Dolina Prądnika", 50.1975, 19.8261),
                null),

            new AttractionSeed("opn-pstrag", "Pstrąg Ojcowski",
                "Cult local gastronomy. Traditionally smoked brown trout served outdoors.",
                [Tag("restaurant", "type"), Tag("food", "category"), Tag("local-specialty", "category"), Tag("outdoor", "type"), Tag("ojcow", "loc")],
                new Location("Ojców", "Dolina Prądnika (przy Jonaszówce)", 50.2050, 19.8290),
                Hours(10, 0, 19, 0)),

            new AttractionSeed("opn-szlak-zielony", "Szlak Zielony",
                "A demanding trail section leading along the slope of Góra Koronna.",
                [Tag("trail", "type"), Tag("hiking", "type"), Tag("nature", "category"), Tag("outdoor", "type")],
                new Location("Ojców", "Góra Koronna", 50.1980, 19.8250),
                null)
        };

        var componentsByKey = new Dictionary<string, AttractionComponent>();

        foreach (var seed in attractionSeeds)
        {
            var attraction = new AttractionDefinition(seed.Name, seed.Description);
            foreach (var tag in seed.Tags) attraction.AddTag(tag);
            attraction.SetLocation(seed.Location);
            if (seed.OpeningHours != null) attraction.SetOpeningHours(seed.OpeningHours);
            componentsByKey[seed.Key] = attraction;
        }

        var packageSeeds = new[]
        {
            new PackageSeed("opn-caves-explorer", "Ojców Explorer",
                "A set for active visitors: entry to Jaskinia Łokietka and Jaskinia Ciemna.",
                SelectionRule.PickN(2), ["opn-jaskinia-lokietka", "opn-jaskinia-ciemna"])
        };

        foreach (var seed in packageSeeds)
        {
            var package = new AttractionPackage(seed.Name, seed.Description, seed.SelectionRule);
            foreach (var key in seed.ComponentKeys) package.AddComponent(componentsByKey[key].Id);
            componentsByKey[seed.Key] = package;
        }

        foreach (var component in componentsByKey.Values)
            await _componentRepository.AddAsync(component);

        var catalogSeeds = new[]
        {
            new CatalogSeed("opn-zamek-ojcow", "Zamek w Ojcowie - Sightseeing", "Self-guided tour of the castle ruins. Cash payments only on site.",
                [Tag("castle", "type"), Tag("history", "category")], "Ojców", "Ojców 9", catalogFrom, catalogTo, 
                new CatalogOpeningHours(new TimeOnly(9, 0), new TimeOnly(16, 30)), false,
                [], [Pricing(catalogFrom, catalogTo, 22)], 200),

            new CatalogSeed("opn-jaskinia-lokietka", "Jaskinia Łokietka - Guided Tour", "Entry in groups with a duty guide every 30 minutes. Long-sleeved clothing required.",
                [Tag("cave", "type"), Tag("guided-tour", "type")], "Ojców", "Góra Chełmowa", catalogFrom, catalogTo, 
                new CatalogOpeningHours(new TimeOnly(9, 0), new TimeOnly(18, 30)), false,
                [Constraint("Range", "group_size", 2, 60), Constraint("OneOf", "language", null, null, "polish", "english")], 
                [Pricing(catalogFrom, catalogTo, 30)], 60),

            new CatalogSeed("opn-jaskinia-ciemna", "Jaskinia Ciemna - Archaeological Route", "Exploring a dark cave. Bringing your own flashlight is highly recommended. Requires good physical condition.",
                [Tag("cave", "type"), Tag("guided-tour", "type"), Tag("adventure", "category")], "Ojców", "Góra Koronna", catalogFrom, new DateOnly(2026, 9, 14), 
                new CatalogOpeningHours(new TimeOnly(9, 0), new TimeOnly(16, 0)), false,
                [Constraint("Range", "group_size", 2, 60), Constraint("RequiredDaysAhead", "booking_days_ahead", 1, null)], 
                [Pricing(catalogFrom, new DateOnly(2026, 9, 14), 20)], 60),

            new CatalogSeed("opn-ekspozycja", "Ekspozycja Przyrodnicza & 3D Movie", "3D movie screening and museum tour. Minimum group size: 3 people.",
                [Tag("museum", "type"), Tag("indoor", "type")], "Ojców", "Ojców 9", catalogFrom, catalogTo, 
                new CatalogOpeningHours(new TimeOnly(9, 0), new TimeOnly(15, 0)), false,
                [Constraint("Range", "group_size", 3, 50), Constraint("RequiredDaysAhead", "booking_days_ahead", 1, null)], 
                [Pricing(catalogFrom, catalogTo, 20)], 50),

            new CatalogSeed("opn-pieskowa-skala", "Zamek w Pieskowej Skale - Entry", "Entry to the courtyard and museum exhibitions.",
                [Tag("castle", "type"), Tag("museum", "type")], "Sułoszowa", "Sułoszowa 5", catalogFrom, catalogTo, 
                new CatalogOpeningHours(new TimeOnly(9, 0), new TimeOnly(17, 0)), false,
                [], [Pricing(catalogFrom, catalogTo, 30)], 300),

            new CatalogSeed("opn-brama-krakowska", "Brama Krakowska - Attraction", "Viewpoint at the bottom of the valley.",
                [Tag("outdoor", "type"), Tag("free", "category")], "Ojców", "Dolina Prądnika", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), null, false,
                [], [Pricing(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 0)], 999),

            new CatalogSeed("opn-pstrag", "Lunch: Pstrąg Ojcowski", "A portion of traditional trout. No reservation required.",
                [Tag("food", "category")], "Ojców", "Dolina Prądnika", catalogFrom, catalogTo, 
                new CatalogOpeningHours(new TimeOnly(10, 0), new TimeOnly(19, 0)), false,
                [], [Pricing(catalogFrom, catalogTo, 45)], 200),
                
            new CatalogSeed("opn-szlak-zielony", "Szlak Zielony - Entrance", "Pedestrian tourist trail.",
                [Tag("trail", "type"), Tag("free", "category")], "Ojców", "Góra Koronna", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), null, false,
                [], [Pricing(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 0)], 9999)
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
                seed.IsEvent, seed.Tags, seed.Constraints);

            if (seed.OpeningHours != null) entry.SetOpeningHours(seed.OpeningHours);
            foreach (var pricing in seed.PricingPeriods) entry.AddPricingPeriod(pricing);

            catalogEntriesByKey[seed.ComponentKey] = entry;
            await _catalogRepository.AddAsync(entry);
            await _ticketPoolRepository.AddAsync(new TicketPool(entry.Id, seed.Capacity));
        }

        var relationSeeds = new[]
        {
            new RelationSeed("opn-zamek-ojcow", "opn-ekspozycja", RelationType.Suggests, "same_location", "Ekspozycja Przyrodnicza OPN and Zamek Kazimierzowski w Ojcowie are located on the same hill. It is highly recommended to combine them."),
            new RelationSeed("opn-szlak-zielony", "opn-jaskinia-ciemna", RelationType.Requires, "prerequisite", "Entering Jaskinia Ciemna REQUIRES prior completion of the steep Szlak Zielony."),
            new RelationSeed("opn-jaskinia-ciemna", "opn-pstrag", RelationType.Suggests, "reward", "After an exhausting descent from Jaskinia Ciemna, a hot Pstrąg Ojcowski is a perfect reward and regeneration."),
            new RelationSeed("opn-brama-krakowska", "opn-pstrag", RelationType.Suggests, "nearby", "The trout ponds are literally a 5-minute walk from Brama Krakowska."),
            new RelationSeed("opn-jaskinia-lokietka", "opn-jaskinia-ciemna", RelationType.Excludes, "exhaustion", "Exploring both caves in one day requires two very steep ascents. Not recommended for beginners."),
            new RelationSeed("opn-jaskinia-lokietka", "opn-pieskowa-skala", RelationType.Excludes, "time_conflict", "Lines to the cave (even up to 1.5h wait) and the commute (7 km) make visiting these two places back-to-back risky in terms of time.")
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

        Console.WriteLine("=== OPN DATA SEEDER ===");
        Console.WriteLine($"Components:   {componentsByKey.Count} (attractions + packages)");
        Console.WriteLine($"Catalog:      {catalogEntriesByKey.Count} entries");
        Console.WriteLine($"Relations:    {relationSeeds.Length}");
        Console.WriteLine("=======================");
    }

    private static Tag Tag(string name, string? group = null) => new(name, group);

    private static OpeningHours Hours(int openHour, int openMinute, int closeHour, int closeMinute) =>
        new(new TimeOnly(openHour, openMinute), new TimeOnly(closeHour, closeMinute));

    private static BookingConstraint Constraint(string type, string key, decimal? minValue, decimal? maxValue, params string[] allowedValues) =>
        new(type, key, minValue, maxValue, allowedValues.Length == 0 ? Array.Empty<string>() : allowedValues);

    private static PricingPeriod Pricing(DateOnly from, DateOnly to, decimal amount) =>
        new(new PB.Modules.Catalog.Domain.ValueObjects.DateRange(from, to), new Money(amount, "PLN"));

    private sealed record AttractionSeed(string Key, string Name, string Description, IReadOnlyList<Tag> Tags, Location Location, OpeningHours? OpeningHours);
    private sealed record PackageSeed(string Key, string Name, string Description, SelectionRule SelectionRule, IReadOnlyList<string> ComponentKeys);
    private sealed record CatalogSeed(string ComponentKey, string Name, string Description, IReadOnlyList<Tag> Tags, string City, string? Address, DateOnly From, DateOnly To, CatalogOpeningHours? OpeningHours, bool IsEvent, IReadOnlyList<BookingConstraint> Constraints, IReadOnlyList<PricingPeriod> PricingPeriods, int Capacity);
    private sealed record RelationSeed(string SourceKey, string TargetKey, RelationType Type, string? Context, string? Description);
}