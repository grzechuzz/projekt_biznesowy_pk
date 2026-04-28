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
        var fullYear = (From: new DateOnly(2025, 1, 1), To: new DateOnly(2026, 12, 31));
        var summerSeason = (From: new DateOnly(2025, 5, 1), To: new DateOnly(2025, 8, 31));

        var discDziecko = new Discount("Dzieci do 15 lat (w towarzystwie płacącego dorosłego)", 25m, null, "dziecko");
        var discGrupowy = new Discount("Zniżka grupowa", 20m, null, "grupowa");

        Location PWLocation() => new("Kraków", "ul. Dobrego Pasterza 126, 31-416 Kraków", 50.0886, 19.9839);
        OpeningHours PWHours() => Hours(10, 0, 22, 0);

        // ── 1. Attractions ───────────────────────────────────────────────────────
        var attractionSeeds = new[]
        {
            // Aqua Park Zone
            new AttractionSeed("pw-basen-rekreacyjny", "Basen Rekreacyjny", "25-metrowy basen z falą morską i jacuzzi (temp. 29°C)",
                [Tag("basen"), Tag("aqua-park")], PWLocation(), PWHours()),
            new AttractionSeed("pw-basen-sportowy", "Basen Sportowy", "6-torowy basen 25 m do pływania (temp. 27°C)",
                [Tag("basen"), Tag("sport")], PWLocation(), PWHours()),
            new AttractionSeed("pw-zjezdzalnie", "Zjeżdżalnie Wodne", "4 tory: rura, wąż, kamikaze, rodzinna (wzrost min. 120 cm)",
                [Tag("rozrywka"), Tag("zjezdzalnia")], PWLocation(), PWHours()),
            new AttractionSeed("pw-aqua-kids", "Aqua Kids — Strefa dla Dzieci", "Brodzik, fontanny, zjeżdżalnie (gł. 30 cm)",
                [Tag("dzieci"), Tag("rodzinne")], PWLocation(), PWHours()),
            new AttractionSeed("pw-rzeka", "Rzeka Leniwca i Jacuzzi", "60-metrowa rzeka leniwca + jacuzzi zewnętrzne",
                [Tag("relaks")], PWLocation(), PWHours()),
            new AttractionSeed("pw-aqua-aerobik", "Aqua Aerobik", "Zajęcia grupowe 45 min",
                [Tag("fitness"), Tag("sport")], PWLocation(), PWHours()),
            new AttractionSeed("pw-nauka-dzieci", "Nauka Pływania — Dzieci", "Lekcje z instruktorem PZP",
                [Tag("edukacja"), Tag("dzieci")], PWLocation(), Hours(15, 0, 20, 0)),
            new AttractionSeed("pw-wyp-sprzetu", "Wypożyczalnia Sprzętu Wodnego", "Okulary, czepek, deska, itp.",
                [Tag("usluga")], PWLocation(), PWHours()),
            new AttractionSeed("pw-szatnia", "Szatnia i Przechowalnia", "Szafki z opaską",
                [Tag("infrastruktura")], PWLocation(), PWHours()),
                
            // SPA Zone
            new AttractionSeed("pw-strefa-saun", "Strefa Saun", "Sauna fińska, parowa, solna, infrared",
                [Tag("spa"), Tag("relaks")], PWLocation(), Hours(12, 0, 22, 0)),
            new AttractionSeed("pw-masaz-klas", "Masaż Klasyczny", "Masaż całego ciała, 50 min",
                [Tag("spa"), Tag("masaz")], PWLocation(), Hours(12, 0, 21, 0)),
            new AttractionSeed("pw-masaz-kamienie", "Masaż Gorącymi Kamieniami", "Masaż bazaltowymi kamieniami, 60 min",
                [Tag("spa"), Tag("masaz")], PWLocation(), Hours(12, 0, 21, 0)),
            new AttractionSeed("pw-nauka-dorosli", "Nauka Pływania — Dorośli", "Indywidualne z certyfikowanym instruktorem",
                [Tag("edukacja"), Tag("sport")], PWLocation(), Hours(18, 0, 21, 0)),
                
            // Outdoor Zone
            new AttractionSeed("pw-basen-zewnetrzny", "Basen Zewnętrzny", "Odkryty podgrzewany basen z leżakami",
                [Tag("basen"), Tag("lato")], PWLocation(), Hours(10, 0, 20, 0)),
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
        
        // ── 2. Services (modeled as abstract components) ────────────────────────
        var serviceSeeds = new[]
        {
            new AttractionSeed("pw-svc-recznik", "Wypożyczenie ręcznika", "Duży ręcznik kąpielowy",
                [Tag("usluga")], PWLocation(), PWHours()),
            new AttractionSeed("pw-svc-szafka", "Dodatkowa szafka bagażowa", "Szafka mała/duża",
                [Tag("infrastruktura")], PWLocation(), PWHours()),
            new AttractionSeed("pw-svc-parking", "Parking", "Parking przy obiekcie (200 miejsc)",
                [Tag("parking")], PWLocation(), Hours(9, 0, 23, 0)),
            new AttractionSeed("pw-svc-instruktor", "Instruktor pływania", "Indywidualna lekcja (45 min)",
                [Tag("edukacja")], PWLocation(), PWHours()),
            new AttractionSeed("pw-svc-opaska", "Elektroniczna opaska wodoszczelna", "Klucz do szafki",
                [Tag("akcesoria")], PWLocation(), PWHours()),
        };

        foreach (var seed in serviceSeeds)
        {
            var attraction = new AttractionDefinition(seed.Name, seed.Description);
            foreach (var tag in seed.Tags) attraction.AddTag(tag);
            attraction.SetLocation(seed.Location);
            if (seed.OpeningHours != null) attraction.SetOpeningHours(seed.OpeningHours);
            componentsByKey[seed.Key] = attraction;
        }

        // ── 3. Packages ─────────────────────────────────────────────────────────
        var packageSeeds = new[]
        {
            new PackageSeed("pw-pkg-aqua", "Bilet Aqua Park", "Dostęp do wszystkich stref basenów i zjeżdżalni", SelectionRule.All(),
                ["pw-basen-rekreacyjny", "pw-basen-sportowy", "pw-zjezdzalnie", "pw-aqua-kids", "pw-rzeka", "pw-szatnia", "pw-svc-opaska"]),
            new PackageSeed("pw-pkg-aqua-spa", "Bilet Aqua Park + SPA", "Dostęp do basenów i strefy saun", SelectionRule.All(),
                ["pw-pkg-aqua", "pw-strefa-saun"]),
            new PackageSeed("pw-pkg-rodzina", "Bilet Rodzinny (2+2)", "Pakiet da rodzin z dziećmi", SelectionRule.All(),
                ["pw-pkg-aqua"]),
            new PackageSeed("pw-pkg-grupa", "Bilet Grupowy", "Dla min. 10 osób", SelectionRule.All(),
                ["pw-pkg-aqua"]),
            new PackageSeed("pw-pkg-szkola", "Wycieczka Edukacyjna", "Dla szkół", SelectionRule.All(),
                ["pw-pkg-aqua"]),
            new PackageSeed("pw-pkg-karnet", "Karnet Miesięczny — Aqua Park", "Dostęp bez limitu przez 30 dni", SelectionRule.All(),
                ["pw-pkg-aqua"]),
            new PackageSeed("pw-pkg-karnet-spa", "Karnet Miesięczny — Aqua Park + SPA", "Sauny + Baseny (30 dni)", SelectionRule.All(),
                ["pw-pkg-aqua-spa"]),
            new PackageSeed("pw-pkg-wellness", "Day SPA", "Sauny + masaż kamieniami + ręcznik", SelectionRule.All(),
                ["pw-strefa-saun", "pw-masaz-kamienie", "pw-svc-recznik", "pw-szatnia", "pw-svc-opaska"]),
        };

        foreach (var seed in packageSeeds)
        {
            var package = new AttractionPackage(seed.Name, seed.Description, seed.SelectionRule);
            foreach (var key in seed.ComponentKeys)
            {
                if (componentsByKey.TryGetValue(key, out var comp))
                    package.AddComponent(comp.Id);
            }
            componentsByKey[seed.Key] = package;
        }

        foreach (var component in componentsByKey.Values)
            await _componentRepository.AddAsync(component);

        // ── 4. Catalog Entries (22) ─────────────────────────────────────────────
        var catalogSeeds = new[]
        {
            // Packages
            new CatalogSeed("pw-cat-aqua-2h", "pw-pkg-aqua", "Bilet Aqua Park 2h", "Dostęp na 2 godziny do wszystkich stref wodnych.",
                [Tag("hit-sezonu")], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, [], [Pricing(fullYear.From, fullYear.To, 35)], 1500),
            new CatalogSeed("pw-cat-aqua-3h", "pw-pkg-aqua", "Bilet Aqua Park 3h", "Dostęp na 3 godziny.",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, [], [Pricing(fullYear.From, fullYear.To, 43)], 1500),
            new CatalogSeed("pw-cat-aqua-allday", "pw-pkg-aqua", "Bilet Aqua Park Całodniowy", "Bez limitu czasu.",
                [Tag("bestseller")], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, [], [Pricing(fullYear.From, fullYear.To, 55)], 1000),

            new CatalogSeed("pw-cat-aqua-spa", "pw-pkg-aqua-spa", "Bilet Aqua Park + SPA 2h", "Z dostępem do strefy saun. Wymagane min. 16 lat.",
                [Tag("premium")], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Min", "visitor_age", 16, null)], [Pricing(fullYear.From, fullYear.To, 75)], 400),

            new CatalogSeed("pw-cat-rodzina", "pw-pkg-rodzina", "Bilet Rodzinny (2+2)", "Pakiet rodzinny cały dzień.",
                [Tag("polecane-rodzinom")], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Range", "group_size", 4, 4)], [Pricing(fullYear.From, fullYear.To, 129)], 800),

            new CatalogSeed("pw-cat-grupa", "pw-pkg-grupa", "Bilet Grupowy", "Zniżka dla grup pow. 10 os.",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Min", "group_size", 10, null), Constraint("RequiredDaysAhead", "booking_days_ahead", 3, null)], [Pricing(fullYear.From, fullYear.To, 44, discGrupowy)], 500),

            new CatalogSeed("pw-cat-szkola", "pw-pkg-szkola", "Bilet Szkolny EDU", "Dla grup min. 10 osób, opcjonalna darmowa lekcja instruktora (rezerwacja tydzień wcześniej).",
                [Tag("edukacyjne")], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Min", "group_size", 10, null), Constraint("RequiredDaysAhead", "booking_days_ahead", 7, null), Constraint("OneOf", "client_type", null, null, "szkoła")], [Pricing(fullYear.From, fullYear.To, 28)], 300),

            new CatalogSeed("pw-cat-karnet", "pw-pkg-karnet", "Karnet Miesięczny — Aqua Park", "Abonament na 30 dni wejść, przypisany do osoby.",
                [Tag("karnet")], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [], [Pricing(fullYear.From, fullYear.To, 199)], 1000),

            new CatalogSeed("pw-cat-karnet-spa", "pw-pkg-karnet-spa", "Karnet Miesięczny — Aqua + SPA", "Abonament z saunami.",
                [Tag("karnet"), Tag("premium")], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Min", "visitor_age", 16, null)], [Pricing(fullYear.From, fullYear.To, 299)], 500),

            new CatalogSeed("pw-cat-wellness", "pw-pkg-wellness", "Day SPA", "Sauny, masaż gorącymi kamieniami i ręcznik (Day SPA)",
                [Tag("wellness")], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Min", "visitor_age", 16, null), Constraint("RequiredDaysAhead", "booking_days_ahead", 1, null)], [Pricing(fullYear.From, fullYear.To, 185)], 80),

            // Base Services & standalone attractions
            new CatalogSeed("pw-cat-basen-rek", "pw-basen-rekreacyjny", "Tor - Basen Rekreacyjny", "Wejście na sam basen (1h)",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, [], [Pricing(fullYear.From, fullYear.To, 22)], 200),
            new CatalogSeed("pw-cat-basen-spt", "pw-basen-sportowy", "Tor - Basen Sportowy", "Wejście sportowe (1h)",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, [], [Pricing(fullYear.From, fullYear.To, 22)], 150),
            new CatalogSeed("pw-cat-zjazd", "pw-zjezdzalnie", "Tor - Zjeżdżalnie", "Pojedyncza strefa zjeżdżalni",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, [], [Pricing(fullYear.From, fullYear.To, 20)], 100),
            new CatalogSeed("pw-cat-kids", "pw-aqua-kids", "Aquad Kids", "Strefa malucha",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, [], [Pricing(fullYear.From, fullYear.To, 15)], 80),
            
            new CatalogSeed("pw-cat-aerobik", "pw-aqua-aerobik", "Aqua Aerobik", "Zajęcia 45 min",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Range", "group_size", 5, 20)], [Pricing(fullYear.From, fullYear.To, 30)], 60),

            new CatalogSeed("pw-cat-nauka-d", "pw-nauka-dzieci", "Nauka Pływania - Dzieci", "Jedna lekcja",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Range", "group_size", 1, 6)], [Pricing(fullYear.From, fullYear.To, 80)], 40),

            new CatalogSeed("pw-cat-nauka-dor", "pw-nauka-dorosli", "Nauka Pływania - Dorośli", "Jedna lekcja indywidualna",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Min", "visitor_age", 16, null)], [Pricing(fullYear.From, fullYear.To, 100)], 20),

            new CatalogSeed("pw-cat-sauny", "pw-strefa-saun", "Bilet Strefa Saun 1h", "Pojedynczy bilet do saun",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Min", "visitor_age", 16, null)], [Pricing(fullYear.From, fullYear.To, 45)], 200),

            new CatalogSeed("pw-cat-masaz-klas", "pw-masaz-klas", "Masaż Klasyczny", "Pojedynczy masaż 50 min",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Min", "visitor_age", 16, null), Constraint("RequiredDaysAhead", "booking_days_ahead", 1, null)], [Pricing(fullYear.From, fullYear.To, 150)], 20),

            new CatalogSeed("pw-cat-masaz-kam", "pw-masaz-kamienie", "Masaż Gorącymi Kamieniami", "Masaż z kamieniami",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [Constraint("Min", "visitor_age", 16, null), Constraint("RequiredDaysAhead", "booking_days_ahead", 1, null)], [Pricing(fullYear.From, fullYear.To, 190)], 10),

            new CatalogSeed("pw-cat-basen-zew", "pw-basen-zewnetrzny", "Basen Zewnętrzny", "Bilet plener",
                [Tag("lato")], "Kraków", "Dobrego Pasterza 126", summerSeason.From, summerSeason.To, null, false, 
                [], [Pricing(summerSeason.From, summerSeason.To, 25)], 300),

            new CatalogSeed("pw-cat-parking", "pw-svc-parking", "Bilet Parkingowy", "Jednorazowy",
                [], "Kraków", "Dobrego Pasterza 126", fullYear.From, fullYear.To, null, false, 
                [], [Pricing(fullYear.From, fullYear.To, 15)], 200),
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

            if (seed.OpeningHours != null) entry.SetOpeningHours(seed.OpeningHours);
            foreach (var period in seed.PricingPeriods) entry.AddPricingPeriod(period);

            catalogEntriesByKey[seed.CatalogKey] = entry;
            await _catalogRepository.AddAsync(entry);
            await _ticketPoolRepository.AddAsync(new TicketPool(entry.Id, seed.Capacity));
        }

        // ── 5. Relations ────────────────────────────────────────────────────────
        var relationSeeds = new[]
        {
            // ComplementedBy
            new RelationSeed("pw-basen-rekreacyjny", "pw-rzeka", RelationType.Suggests, "Pełen pakiet relaksu", "Rzeka urozmaici relaks"),
            new RelationSeed("pw-zjezdzalnie", "pw-basen-rekreacyjny", RelationType.Suggests, "Naturalne uzupełnienie", "Klasyka parków wodnych"),
            new RelationSeed("pw-basen-zewnetrzny", "pw-strefa-saun", RelationType.Suggests, "Kontrast", "Szok termiczny"),
            new RelationSeed("pw-strefa-saun", "pw-masaz-klas", RelationType.Suggests, "Masaż", "SPA po całości"),
            new RelationSeed("pw-strefa-saun", "pw-svc-recznik", RelationType.Suggests, "Praktyczne", "Potrzebne w saunie"),
            
            // CompatibleWith
            new RelationSeed("pw-aqua-kids", "pw-basen-rekreacyjny", RelationType.Suggests, "Rodzinne", "-"),
            new RelationSeed("pw-aqua-kids", "pw-basen-sportowy", RelationType.Suggests, "Rodzinne", "-"),
            new RelationSeed("pw-basen-sportowy", "pw-aqua-aerobik", RelationType.Suggests, "Aktywne", "-"),
            new RelationSeed("pw-nauka-dzieci", "pw-aqua-kids", RelationType.Suggests, "Dla dzieci", "-"),

            // SubstitutedBy (represented by Excludes/Suggests in our new model schema)
            new RelationSeed("pw-masaz-klas", "pw-masaz-kamienie", RelationType.Suggests, "Zamiennik", "Oba to masaże"),
            new RelationSeed("pw-masaz-kamienie", "pw-masaz-klas", RelationType.Suggests, "Zamiennik", "Oba to masaże"),
            new RelationSeed("pw-aqua-aerobik", "pw-nauka-dorosli", RelationType.Suggests, "Aktywność", "Sposoby na ruch"),
            new RelationSeed("pw-pkg-grupa", "pw-pkg-rodzina", RelationType.Suggests, "Zamiennik", "Bilet rodzinny może być alternatywą"),

            // UpgradableTo (modeled as suggests to upgrade package)
            new RelationSeed("pw-pkg-aqua", "pw-pkg-wellness", RelationType.Suggests, "Upgrade Day Spa", "Pełny relaks w ramach Day SPA"),
            new RelationSeed("pw-pkg-aqua", "pw-pkg-karnet", RelationType.Suggests, "Upgrade abonament", "Wybierz abonament na miesiąc"),
            new RelationSeed("pw-pkg-aqua-spa", "pw-pkg-karnet-spa", RelationType.Suggests, "Upgrade abonament", "Oszczędność"),
            new RelationSeed("pw-pkg-aqua-spa", "pw-pkg-wellness", RelationType.Suggests, "Upgrade wellness", "Doznania"),
            new RelationSeed("pw-pkg-karnet", "pw-pkg-karnet-spa", RelationType.Suggests, "Upgrade karnetu", "Więcej saunury")
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
        
        Console.WriteLine("=== PARK WODNY KRAKÓW — DATA SEEDER ===");
        Console.WriteLine($"AttractionComponents: {componentsByKey.Count}");
        Console.WriteLine($"CatalogEntries:       {catalogEntriesByKey.Count}");
        Console.WriteLine($"Relations:            {relationSeeds.Length}");
        Console.WriteLine("=============================================");
    }

    private static Tag Tag(string name, string? group = null) => new(name, group);
    private static OpeningHours Hours(int oh, int om, int ch, int cm) => new(new TimeOnly(oh, om), new TimeOnly(ch, cm));
    private static CatalogOpeningHours CatalogHours(int oh, int om, int ch, int cm) => new(new TimeOnly(oh, om), new TimeOnly(ch, cm));
    private static BookingConstraint Constraint(string type, string key, decimal? min, decimal? max, params string[] allowed) =>
        new(type, key, min, max, allowed.Length == 0 ? Array.Empty<string>() : allowed);
    private static PricingPeriod Pricing(DateOnly from, DateOnly to, decimal amount, params Discount[] discounts) =>
        new(new PB.Modules.Catalog.Domain.ValueObjects.DateRange(from, to), new Money(amount, "PLN"), discounts.Length == 0 ? null : discounts);

    private sealed record AttractionSeed(string Key, string Name, string Description, IReadOnlyList<Tag> Tags, Location Location, OpeningHours? OpeningHours);
    private sealed record PackageSeed(string Key, string Name, string Description, SelectionRule SelectionRule, IReadOnlyList<string> ComponentKeys);
    private sealed record CatalogSeed(string CatalogKey, string ComponentKey, string Name, string Description, IReadOnlyList<Tag> Tags, string City, string? Address, DateOnly From, DateOnly To, CatalogOpeningHours? OpeningHours, bool IsEvent, IReadOnlyList<BookingConstraint> Constraints, IReadOnlyList<PricingPeriod> PricingPeriods, int Capacity);
    private sealed record RelationSeed(string SourceKey, string TargetKey, RelationType Type, string? Context, string? Description);
}
