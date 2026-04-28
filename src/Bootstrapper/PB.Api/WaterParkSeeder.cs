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

public class WaterParkSeeder
{
    private readonly IAttractionComponentRepository _componentRepository;
    private readonly ICatalogEntryRepository _catalogRepository;
    private readonly ITicketPoolRepository _ticketPoolRepository;
    private readonly IAttractionRelationRepository _relationRepository;

    public WaterParkSeeder(
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
        var catalogFrom = new DateOnly(2026, 4, 1);
        var catalogTo = new DateOnly(2026, 12, 31);
        var krakowLoc = new Location("Kraków", "ul. Dobrego Pasterza 126, 31-416 Kraków", 50.0883, 19.9822);
        var fitparkLoc = new Location("Kraków", "ul. Dobrego Pasterza 126, 31-416 Kraków", 50.0883, 19.9822);

        var attractionSeeds = new[]
        {
            // Kompleks Basenowy
            new AttractionSeed("waterpark-pool", "Hala basenowa",
                "Główna hala basenowa z torami sportowymi, sztuczną falą i hydromasażami.",
                [Tag("pool", "type"), Tag("water", "category"), Tag("indoor", "style"), Tag("krakow", "loc")],
                krakowLoc, Hours(8, 0, 22, 0)),
            new AttractionSeed("waterpark-sauna", "Sauny",
                "Strefa saun: sucha, parowa, infrared oraz pokój relaksu.",
                [Tag("sauna", "type"), Tag("wellness", "category"), Tag("indoor", "style"), Tag("krakow", "loc")],
                krakowLoc, Hours(8, 0, 22, 0)),
            new AttractionSeed("waterpark-outdoor", "Strefa parkowa",
                "Atrakcje zewnętrzne: zjeżdżalnie, jacuzzi i baseny letnie.",
                [Tag("slides", "type"), Tag("park", "category"), Tag("outdoor", "style"), Tag("family", "audience"), Tag("krakow", "loc")],
                krakowLoc, Hours(12, 0, 20, 0)),

            // Fitpark
            new AttractionSeed("fitpark-gym", "Siłownia",
                "Profesjonalna strefa cardio i siłowa z nowoczesnym sprzętem.",
                [Tag("gym", "type"), Tag("fitness", "category"), Tag("sports", "category"), Tag("krakow", "loc")],
                fitparkLoc, Hours(8, 0, 23, 0)),
            new AttractionSeed("fitpark-classes", "Zajęcia grupowe",
                "Zajęcia fitness: Zumba, Joga, Aerobik pod okiem instruktora.",
                [Tag("classes", "type"), Tag("fitness", "category"), Tag("group", "audience"), Tag("krakow", "loc")],
                fitparkLoc, Hours(8, 0, 22, 0)),

            // Hotel
            new AttractionSeed("hotel-room", "Nocleg w hotelu",
                "Zakwaterowanie w komfortowym pokoju dwuosobowym.",
                [Tag("hotel", "type"), Tag("accommodation", "category"), Tag("krakow", "loc")],
                krakowLoc, null),
            new AttractionSeed("hotel-breakfast", "Śniadanie",
                "Bogaty bufet śniadaniowy w restauracji hotelowej.",
                [Tag("breakfast", "type"), Tag("food", "category"), Tag("morning", "time")],
                krakowLoc, Hours(7, 0, 11, 0))
        };

        var componentsByKey = new Dictionary<string, AttractionComponent>();
        foreach (var seed in attractionSeeds)
        {
            var attraction = new AttractionDefinition(seed.Name, seed.Description);
            foreach (var tag in seed.Tags) attraction.AddTag(tag);
            attraction.SetLocation(seed.Location);
            attraction.SetOpeningHours(seed.OpeningHours);
            componentsByKey[seed.Key] = attraction;
        }

        var packageSeeds = new[]
        {
            // Warianty basenowe
            new PackageSeed("waterpark-pool-sauna", "Hala basenowa + Sauny", "Połączenie pływania i relaksu w saunie.", SelectionRule.All(), ["waterpark-pool", "waterpark-sauna"]),
            new PackageSeed("waterpark-pool-outdoor", "Hala basenowa + Strefa parkowa", "Dostęp do basenów wewnętrznych i zewnętrznych zjeżdżalni.", SelectionRule.All(), ["waterpark-pool", "waterpark-outdoor"]),
            new PackageSeed("waterpark-full", "Kompleks Basenowy - Full", "Pełny dostęp: Hala, Sauny i Strefa Parkowa.", SelectionRule.All(), ["waterpark-pool", "waterpark-sauna", "waterpark-outdoor"]),
            
            // Warianty Fitpark
            new PackageSeed("fitpark-full", "Fitpark Full Pass", "Siłownia oraz udział w dowolnych zajęciach grupowych.", SelectionRule.All(), ["fitpark-gym", "fitpark-classes"]),


            new PackageSeed("dzien-aktywny", "Dzień Aktywny", "Energiczny dzień: Full Water Park + Full Fitpark.", SelectionRule.All(), ["waterpark-full", "fitpark-full"]),
            new PackageSeed("dzien-wellness", "Dzień Wellness", "Relaks: Basen, Sauny oraz zajęcia Jogi.", SelectionRule.PickN(3), ["waterpark-pool", "waterpark-sauna", "fitpark-classes"]),
            new PackageSeed("weekend-relaks", "Weekend Relaks (B&B + Basen)", "Pobyt w hotelu ze śniadaniem i relaksem na basenie.", SelectionRule.All(), ["hotel-room", "hotel-breakfast", "waterpark-pool-sauna"]),
            new PackageSeed("weekend-premium", "Weekend Premium", "Flagowy pakiet: Hotel B&B, Water Park Full oraz Fitpark Full.", SelectionRule.All(), ["hotel-room", "hotel-breakfast", "waterpark-full", "fitpark-full"])
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


        var catalogSeeds = new[]
        {
            // Bilety pojedyncze
            new CatalogSeed("waterpark-pool", "Bilet: Hala basenowa", "Wejście na 2h do hali basenowej.",
                [Tag("pool", "type"), Tag("water", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Dobrego Pasterza 126", catalogFrom, catalogTo, new CatalogOpeningHours(new TimeOnly(8, 0), new TimeOnly(22, 0)), false,
                [Constraint("Range", "group_size", 1, 10)], [Pricing(catalogFrom, catalogTo, 40)], 100),
            
            new CatalogSeed("waterpark-sauna", "Bilet: Sauny", "Wejście na 2h do strefy saun (18+).",
                [Tag("sauna", "type"), Tag("wellness", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Dobrego Pasterza 126", catalogFrom, catalogTo, new CatalogOpeningHours(new TimeOnly(8, 0), new TimeOnly(22, 0)), false,
                [Constraint("Range", "group_size", 1, 4)], [Pricing(catalogFrom, catalogTo, 35)], 50),

            new CatalogSeed("waterpark-outdoor", "Bilet: Strefa parkowa (LATO)", "Całodniowy bilet do strefy zewnętrznej. Dostępny w sezonie letnim.",
                [Tag("slides", "type"), Tag("park", "category"), Tag("outdoor", "style"), Tag("krakow", "loc")],
                "Kraków", "ul. Dobrego Pasterza 126", new DateOnly(2026, 6, 1), new DateOnly(2026, 8, 31), new CatalogOpeningHours(new TimeOnly(12, 0), new TimeOnly(20, 0)), false,
                [Constraint("Range", "group_size", 1, 15)], [Pricing(new DateOnly(2026, 6, 1), new DateOnly(2026, 8, 31), 60)], 250),

            new CatalogSeed("fitpark-gym", "Karnet: Siłownia", "Jednorazowe wejście bez limitu czasu.",
                [Tag("gym", "type"), Tag("fitness", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Dobrego Pasterza 126", catalogFrom, catalogTo, new CatalogOpeningHours(new TimeOnly(8, 0), new TimeOnly(23, 0)), false,
                [], [Pricing(catalogFrom, catalogTo, 25)], 80),

            new CatalogSeed("fitpark-classes", "Zajęcia: Fitness/Joga", "Wejście na wybrane zajęcia grupowe (1h).",
                [Tag("fitness", "category"), Tag("classes", "type"), Tag("krakow", "loc")],
                "Kraków", "ul. Dobrego Pasterza 126", catalogFrom, catalogTo, new CatalogOpeningHours(new TimeOnly(8, 0), new TimeOnly(22, 0)), false,
                [Constraint("Range", "group_size", 1, 1)], [Pricing(catalogFrom, catalogTo, 35)], 20),

            new CatalogSeed("hotel-room", "Pobyt: Pokój Hotelowy", "Doba hotelowa (14:00 - 11:00).",
                [Tag("hotel", "type"), Tag("accommodation", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Dobrego Pasterza 126", catalogFrom, catalogTo, null, false,
                [Constraint("Range", "group_size", 1, 2)], [Pricing(catalogFrom, catalogTo, 350)], 40),

            // Pakiety w katalogu
            new CatalogSeed("waterpark-full", "Bilet: Kompleks Basenowy (OPEN)", "Dostęp do wszystkich stref Water Parku (w sezonie letnim również strefa zewnętrzna).",
                [Tag("waterpark", "category"), Tag("all-in", "type"), Tag("krakow", "loc")],
                "Kraków", "ul. Dobrego Pasterza 126", catalogFrom, catalogTo, new CatalogOpeningHours(new TimeOnly(12, 0), new TimeOnly(20, 0)), false,
                [Constraint("Range", "group_size", 1, 8)], [Pricing(catalogFrom, catalogTo, 110)], 300)
        };

        var catalogEntriesByKey = new Dictionary<string, CatalogEntry>();
        foreach (var seed in catalogSeeds)
        {
            var entry = new CatalogEntry(componentsByKey[seed.ComponentKey].Id, seed.Name, seed.Description, new CatalogLocation(seed.City, seed.Address),
                new PB.Modules.Catalog.Domain.ValueObjects.DateRange(seed.From, seed.To), seed.IsEvent, seed.Tags, seed.Constraints);
            entry.SetOpeningHours(seed.OpeningHours);
            foreach (var pricing in seed.PricingPeriods) entry.AddPricingPeriod(pricing);
            catalogEntriesByKey[seed.ComponentKey] = entry;
            await _catalogRepository.AddAsync(entry);
            await _ticketPoolRepository.AddAsync(new TicketPool(entry.Id, seed.Capacity));
        }


        var relationSeeds = new[]
        {
            new RelationSeed("waterpark-pool", "waterpark-sauna", RelationType.Suggests, "recovery", "Po pływaniu polecamy relaks w saunie."),
            new RelationSeed("waterpark-pool", "waterpark-outdoor", RelationType.Suggests, "seasonal", "W słoneczne dni skorzystaj ze strefy zewnętrznej."),
            new RelationSeed("fitpark-gym", "waterpark-pool", RelationType.Suggests, "recovery", "Basen to idealna forma regeneracji po treningu."),
            new RelationSeed("hotel-room", "hotel-breakfast", RelationType.Requires, "morning", "Nocleg bez śniadania? Dodaj je do swojego pobytu."),
            new RelationSeed("hotel-room", "waterpark-full", RelationType.Suggests, "amenity", "Dla gości hotelowych specjalna cena na Water Park.")
        };

        foreach (var seed in relationSeeds)
        {
            var relation = new AttractionRelation(componentsByKey[seed.SourceKey].Id, componentsByKey[seed.TargetKey].Id, seed.Type, seed.Context, seed.Description);
            await _relationRepository.AddAsync(relation);
        }

        Console.WriteLine("WATER PARK SEEDER");
        Console.WriteLine($"Atrakcje:      {attractionSeeds.Length}");
        Console.WriteLine($"Pakiety:       {packageSeeds.Length}");
        Console.WriteLine($"Katalog:       {catalogSeeds.Length} wpisów");
        Console.WriteLine($"Relacje:       {relationSeeds.Length}");
    }

    private static Tag Tag(string name, string? group = null) => new(name, group);
    private static OpeningHours Hours(int openHour, int openMinute, int closeHour, int closeMinute) => new(new TimeOnly(openHour, openMinute), new TimeOnly(closeHour, closeMinute));
    private static BookingConstraint Constraint(string type, string key, decimal? minValue, decimal? maxValue, params string[] allowedValues) => new(type, key, minValue, maxValue, allowedValues.Length == 0 ? Array.Empty<string>() : allowedValues);
    private static PricingPeriod Pricing(DateOnly from, DateOnly to, decimal amount) => new(new DateRange(from, to), new Money(amount, "PLN"));

    private record AttractionSeed(string Key, string Name, string Description, IReadOnlyList<Tag> Tags, Location Location, OpeningHours? OpeningHours);
    private record PackageSeed(string Key, string Name, string Description, SelectionRule SelectionRule, IReadOnlyList<string> ComponentKeys);
    private record CatalogSeed(string ComponentKey, string Name, string Description, IReadOnlyList<Tag> Tags, string City, string? Address, DateOnly From, DateOnly To, CatalogOpeningHours? OpeningHours, bool IsEvent, IReadOnlyList<BookingConstraint> Constraints, IReadOnlyList<PricingPeriod> PricingPeriods, int Capacity);
    private record RelationSeed(string SourceKey, string TargetKey, RelationType Type, string? Context, string? Description);
}