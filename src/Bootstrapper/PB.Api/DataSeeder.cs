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

/// <summary>
/// Seeds a realistic dataset for the Wieliczka Salt Mine (Kopalnia Soli "Wieliczka").
/// Covers all main routes, their variants (individual / group / school / family / pilgrimage),
/// seasonal pricing with discounts, booking constraints, packages, and inter-route relations.
/// </summary>
public class DataSeeder
{
    private readonly IAttractionComponentRepository _componentRepository;
    private readonly ICatalogEntryRepository _catalogRepository;
    private readonly ITicketPoolRepository _ticketPoolRepository;
    private readonly IAttractionRelationRepository _relationRepository;

    public DataSeeder(
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
        // ── Season boundaries ─────────────────────────────────────────────────
        // Low season: Jan–Mar and Oct–Dec (quieter, lower prices)
        // High season: Apr–Sep (peak tourist traffic, full schedule)
        var lowFrom  = new DateOnly(2026, 1, 1);
        var lowTo    = new DateOnly(2026, 3, 31);
        var highFrom = new DateOnly(2026, 4, 1);
        var highTo   = new DateOnly(2026, 9, 30);
        var offFrom  = new DateOnly(2026, 10, 1);
        var offTo    = new DateOnly(2026, 12, 31);
        var fullYear = (From: new DateOnly(2026, 1, 1), To: new DateOnly(2026, 12, 31));
        // Brine tower is outdoor and strictly seasonal
        var brineSeason = (From: new DateOnly(2026, 5, 1), To: new DateOnly(2026, 9, 30));

        // ── Shared discount definitions ───────────────────────────────────────
        // MODEL NOTE: PricingPeriod holds a single base price (normal adult).
        // Discounts represent reductions for specific groups. Absolute price for
        // a reduced ticket = base – AmountOff, or base × (1 – PercentOff/100).
        var discUlgowy = new Discount(
            "Bilet ulgowy — studenci, uczniowie szkół wyższych, seniorzy 65+",
            null, new Money(20, "PLN"), "ulgowy");

        var discDziecko416 = new Discount(
            "Dzieci 4–16 lat (w towarzystwie płacącego dorosłego)",
            50m, null, "wiek:4-16");

        var discDzieckoFree = new Discount(
            "Dzieci do lat 4 — wstęp bezpłatny (obowiązkowo z dorosłym)",
            100m, null, "wiek:<4");

        var discUlgowyGrupa = new Discount(
            "Bilet ulgowy grupowy — studenci, seniorzy 65+",
            null, new Money(10, "PLN"), "ulgowy");

        // ── 1. AttractionDefinitions (routes as abstract components) ──────────
        var attractionSeeds = new[]
        {
            // ─ Tourist Route ──────────────────────────────────────────────────
            // Entry via Szyb Daniłowicza (Daniłowicz Shaft)
            // 3.5 km, depth 135 m, ~2–3 h, 800 stairs, 20+ chambers
            // No age or fitness restrictions; wheelchair inaccessible
            new AttractionSeed(
                "wieliczka-tourist-route",
                "Trasa Turystyczna — Kopalnia Soli Wieliczka",
                "Klasyczna trasa przez ponad 20 komór solnych, w tym Kaplicę św. Kingi. " +
                "Długość 3,5 km, głębokość do 135 m, ok. 800 schodów. Wejście przez Szyb Daniłowicza. " +
                "Przeznaczona dla wszystkich, bez ograniczeń wiekowych. Czas zwiedzania ok. 2–3 h.",
                [Tag("trasa-turystyczna", "trasa"), Tag("unesco", "category"),
                 Tag("przewodnik-obowiązkowy", "type"), Tag("historia", "category"),
                 Tag("wieliczka", "loc"), Tag("family-friendly", "audience"),
                 Tag("dostępna-dla-dzieci", "audience")],
                new Location("Wieliczka", "ul. Daniłowicza 10, 32-020 Wieliczka (Szyb Daniłowicza)",
                    49.9833, 20.0553),
                Hours(7, 30, 19, 30)),

            // ─ Mining Route ───────────────────────────────────────────────────
            // Entry via Szyb Regis (Regis Shaft) — DIFFERENT starting location
            // 1.9 km, depth 101 m, ~2 h, helmet + headlamp provided
            // Min. age 12, physical fitness required, crawling possible
            new AttractionSeed(
                "wieliczka-miner-route",
                "Trasa Górnicza — Kopalnia Soli Wieliczka",
                "Autentyczna trasa górnicza przez oryginalne wyrobiska niedostępne dla ruchu turystycznego. " +
                "1,9 km, głębokość do 101 m, ok. 2 h. Start przez Szyb Regis. " +
                "Uczestnicy wykonują zadania górnicze: szukanie soli, badanie atmosfery kopalni, " +
                "obsługa narzędzi. Kask i lampa górnicza w zestawie. Wymagana sprawność fizyczna, min. wiek 12 lat.",
                [Tag("trasa-górnicza", "trasa"), Tag("przygoda", "category"),
                 Tag("przewodnik-obowiązkowy", "type"), Tag("aktywny", "category"),
                 Tag("wieliczka", "loc"), Tag("dorośli-i-młodzież", "audience")],
                new Location("Wieliczka", "ul. Daniłowicza 10, 32-020 Wieliczka (Szyb Regis)",
                    49.9831, 20.0550),
                Hours(8, 0, 17, 0)),

            // ─ Brine Graduation Tower (Tężnia Solankowa) ─────────────────────
            // Above-ground, outdoor installation, seasonal May–Sep
            // No depth, no fitness restriction, wheelchair accessible
            new AttractionSeed(
                "wieliczka-brine-tower",
                "Tężnia Solankowa — Kopalnia Soli Wieliczka",
                "Sezonowa tężnia solankowa na terenie naziemnym kopalni. Naturalna solanka rozpylana " +
                "przez drewnianą konstrukcję tworzy mgłę korzystną dla dróg oddechowych. " +
                "Brak ograniczeń wiekowych, dostępna dla wózków. Czynna maj–wrzesień.",
                [Tag("tężnia", "trasa"), Tag("zdrowie", "category"), Tag("outdoor", "type"),
                 Tag("family-friendly", "audience"), Tag("wieliczka", "loc"),
                 Tag("sezonowe", "category"), Tag("dostępne-dla-wózków", "type")],
                new Location("Wieliczka", "ul. Daniłowicza 10, 32-020 Wieliczka (teren naziemny)",
                    49.9832, 20.0555),
                Hours(9, 0, 18, 0)),

            // ─ Family/Legends Trail (Śladami Legend) ─────────────────────────
            // Themed trail for children 5–12, depth only 64 m, ~1.5 h
            // Guardian required, smaller groups, story-driven guide in costume
            new AttractionSeed(
                "wieliczka-legends-trail",
                "Śladami Legend — Rodzinna Trasa Tematyczna",
                "Interaktywna, fabularna trasa dla rodzin z dziećmi w wieku 5–12 lat. " +
                "Przewodnik w kostiumie górnika prowadzi przez zagadki i legendy kopalni. " +
                "Głębokość 64 m, czas ok. 1,5 h. Obowiązkowy dorosły opiekun dla każdej grupy dzieci.",
                [Tag("śladami-legend", "trasa"), Tag("rodzinny", "audience"),
                 Tag("dzieci", "audience"), Tag("interaktywna", "category"),
                 Tag("przewodnik-obowiązkowy", "type"), Tag("wieliczka", "loc")],
                new Location("Wieliczka", "ul. Daniłowicza 10, 32-020 Wieliczka (Szyb Daniłowicza)",
                    49.9833, 20.0553),
                Hours(9, 0, 16, 0)),

            // ─ Pilgrimage Route (Szlak Pielgrzymkowy) ────────────────────────
            // Focus on Chapel of St. Kinga, underground Mass, salt altar
            // Group-only, min 15 people, advance booking 14 days
            new AttractionSeed(
                "wieliczka-pilgrimage-route",
                "Szlak Pielgrzymkowy — Kopalnia Soli Wieliczka",
                "Trasa duchowa skoncentrowana na Kaplicy Błogosławionej Kingi — perle podziemnej architektury sakralnej. " +
                "Możliwość odprawienia Mszy Świętej pod ziemią (wymagana oddzielna rezerwacja). " +
                "Modlitwa przy ołtarzu solnym, historia kultu solnego. Wyłącznie dla grup pielgrzymkowych.",
                [Tag("szlak-pielgrzymkowy", "trasa"), Tag("religia", "category"),
                 Tag("kaplica", "category"), Tag("przewodnik-obowiązkowy", "type"),
                 Tag("grupy", "audience"), Tag("wieliczka", "loc")],
                new Location("Wieliczka", "ul. Daniłowicza 10, 32-020 Wieliczka (Szyb Daniłowicza)",
                    49.9833, 20.0553),
                Hours(8, 0, 16, 0)),

            // ─ Hotel Stay (used only as package component) ───────────────────
            // Modeled as an AttractionDefinition so it can be part of a package.
            // Not sold individually — only appears inside the Mine+Hotel package.
            new AttractionSeed(
                "wieliczka-hotel-stay",
                "Nocleg w hotelu przy Kopalni Soli Wieliczka",
                "Pobyt z noclegiem w hotelu przy kopalni. Pakiet ze wczesnym wejściem do kopalni " +
                "(przed otwarciem dla turystów indywidualnych). Śniadanie w cenie.",
                [Tag("hotel", "type"), Tag("nocleg", "category"), Tag("wieliczka", "loc"),
                 Tag("pakiet", "category")],
                new Location("Wieliczka", "ul. Daniłowicza 10, 32-020 Wieliczka", 49.9833, 20.0553),
                null),
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

        // ── 2. AttractionPackages ─────────────────────────────────────────────
        // MODEL NOTE: An AttractionPackage groups component IDs under a SelectionRule.
        // A CatalogEntry can be created pointing to a package's Id — this is how
        // a package becomes bookable as a single offer.
        var packageSeeds = new[]
        {
            // Tourist + Miner Routes — "complete mine" offer
            // SelectionRule.All() means both must be taken
            new PackageSeed(
                "wieliczka-combo-tourist-miner",
                "Kombo: Trasa Turystyczna + Górnicza",
                "Pełne doświadczenie kopalni: klasyczna trasa turystyczna i autentyczna trasa górnicza. " +
                "Można zwiedzić obie w ciągu jednego dnia lub w dwa kolejne dni. " +
                "Bilet łączony w cenie korzystniejszej niż dwa osobne wejścia. Min. wiek 12 lat.",
                SelectionRule.All(),
                ["wieliczka-tourist-route", "wieliczka-miner-route"]),

            // Tourist Route + Legends Trail — family package
            // Children do Legends while parents do the full Tourist Route
            new PackageSeed(
                "wieliczka-family-combo",
                "Pakiet Rodzinny: Trasa Turystyczna + Śladami Legend",
                "Dla rodzin z dziećmi 5–12 lat: dorośli zwiedzają pełną Trasę Turystyczną, " +
                "dzieci odkrywają kopalnię Śladami Legend. Wychodzą z kopalni razem.",
                SelectionRule.All(),
                ["wieliczka-tourist-route", "wieliczka-legends-trail"]),

            // Tourist Route + Hotel Stay
            new PackageSeed(
                "wieliczka-mine-hotel-pkg",
                "Pakiet: Zwiedzanie Kopalni + Nocleg",
                "Wejście na Trasę Turystyczną połączone z noclegiem w hotelu przy kopalni. " +
                "Wczesny dostęp do kopalni przed oficjalnym otwarciem. Śniadanie w cenie.",
                SelectionRule.All(),
                ["wieliczka-tourist-route", "wieliczka-hotel-stay"]),
        };

        foreach (var seed in packageSeeds)
        {
            var package = new AttractionPackage(seed.Name, seed.Description, seed.SelectionRule);
            foreach (var key in seed.ComponentKeys)
                package.AddComponent(componentsByKey[key].Id);
            componentsByKey[seed.Key] = package;
        }

        foreach (var component in componentsByKey.Values)
            await _componentRepository.AddAsync(component);

        // ── 3. CatalogEntries ─────────────────────────────────────────────────
        // KEY DESIGN: Multiple CatalogEntries share the same AttractionComponent.
        // Each entry is a distinct, bookable variant with its own constraints,
        // pricing, capacity, and target audience.

        // ─ Shared pricing builders ────────────────────────────────────────────
        PricingPeriod[] TouristIndivPricing() => [
            Pricing(lowFrom,  lowTo,  109, discUlgowy, discDziecko416, discDzieckoFree),
            Pricing(highFrom, highTo, 119, discUlgowy, discDziecko416, discDzieckoFree),
            Pricing(offFrom,  offTo,  109, discUlgowy, discDziecko416, discDzieckoFree),
        ];
        PricingPeriod[] TouristGroupPricing() => [
            Pricing(lowFrom,  lowTo,  79,  discUlgowyGrupa),
            Pricing(highFrom, highTo, 89,  discUlgowyGrupa),
            Pricing(offFrom,  offTo,  79,  discUlgowyGrupa),
        ];
        PricingPeriod[] TouristSchoolPricing() => [
            Pricing(lowFrom,  lowTo,  55),
            Pricing(highFrom, highTo, 65),
            Pricing(offFrom,  offTo,  55),
        ];

        var catalogSeeds = new[]
        {
            // ═════════════════════════════════════════════════════════════════
            // TOURIST ROUTE — 4 variants
            // ═════════════════════════════════════════════════════════════════

            // ─ Tourist / Individual ───────────────────────────────────────────
            // 1–14 people, 8 languages, slots every 30 min, all ages
            // MODEL NOTE: "slot_interval_minutes" stored as OneOf constraint —
            // the model has no dedicated slot entity, so slot logic is in data.
            new CatalogSeed(
                CatalogKey:    "wieliczka-tourist-individual",
                ComponentKey:  "wieliczka-tourist-route",
                Name:          "Trasa Turystyczna — bilet indywidualny",
                Description:   "Wejście indywidualne lub mała grupa (do 14 osób). " +
                               "Wejścia co 30 minut od 7:30. Wybór spośród 8 języków przy rezerwacji. " +
                               "Trasa 3,5 km, ok. 2–3 h, głębokość 135 m.",
                Tags:          [Tag("turystyczna", "trasa"), Tag("indywidualny", "typ"),
                                Tag("wielojęzyczny", "kategoria"), Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10 (Szyb Daniłowicza)",
                From:          fullYear.From,
                To:            fullYear.To,
                OpeningHours:  CatalogHours(7, 30, 19, 30),
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range",            "group_size",             1,    14),
                    Constraint("RequiredDaysAhead","booking_days_ahead",     1,    null),
                    Constraint("OneOf",            "language",               null, null,
                        "polski", "english", "deutsch", "français",
                        "italiano", "español", "русский", "中文"),
                    Constraint("OneOf",            "guide_mandatory",        null, null, "true"),
                    Constraint("OneOf",            "slot_interval_minutes",  null, null, "30"),
                    Constraint("OneOf",            "accessibility",          null, null,
                        "brak_wózków_inwalidzkich", "ok_800_schodów"),
                    Constraint("OneOf",            "children_under4_policy", null, null,
                        "bezpłatny_z_opiekunem"),
                ],
                PricingPeriods: TouristIndivPricing(),
                Capacity:      800),

            // ─ Tourist / Organized Group ──────────────────────────────────────
            // 15–35 people, group price, mandatory advance booking 7 days
            new CatalogSeed(
                CatalogKey:    "wieliczka-tourist-group",
                ComponentKey:  "wieliczka-tourist-route",
                Name:          "Trasa Turystyczna — bilet grupowy (min. 15 osób)",
                Description:   "Dla zorganizowanych grup turystycznych: biura podróży, wyjazdy integracyjne. " +
                               "Min. 15, maks. 35 osób na jednego przewodnika. Obowiązkowa rezerwacja z 7-dniowym wyprzedzeniem.",
                Tags:          [Tag("turystyczna", "trasa"), Tag("grupowy", "typ"), Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10 (Szyb Daniłowicza)",
                From:          fullYear.From,
                To:            fullYear.To,
                OpeningHours:  CatalogHours(8, 0, 17, 0),
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range",            "group_size",            15,   35),
                    Constraint("RequiredDaysAhead","booking_days_ahead",    7,    null),
                    Constraint("OneOf",            "language",              null, null,
                        "polski", "english", "deutsch", "français", "italiano", "español"),
                    Constraint("OneOf",            "guide_mandatory",       null, null, "true"),
                    Constraint("OneOf",            "slot_interval_minutes", null, null, "30"),
                    Constraint("OneOf",            "accessibility",         null, null,
                        "brak_wózków_inwalidzkich"),
                ],
                PricingPeriods: TouristGroupPricing(),
                Capacity:      350),

            // ─ Tourist / School Group ─────────────────────────────────────────
            // 10–50 pupils + supervisors, dedicated educational guide, 14-day booking
            // MODEL NOTE: "supervisor_count" min constraint models the 1-adult-per-15-children rule
            new CatalogSeed(
                CatalogKey:    "wieliczka-tourist-school",
                ComponentKey:  "wieliczka-tourist-route",
                Name:          "Trasa Turystyczna — bilet szkolny (min. 10 uczniów)",
                Description:   "Program edukacyjny dla szkół podstawowych i ponadpodstawowych. " +
                               "Przewodnik z gotowymi scenariuszami dydaktycznymi. " +
                               "Min. 10 uczniów. Obowiązkowy co najmniej 1 opiekun na 15 uczniów.",
                Tags:          [Tag("turystyczna", "trasa"), Tag("szkolny", "typ"),
                                Tag("edukacja", "kategoria"), Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10 (Szyb Daniłowicza)",
                From:          fullYear.From,
                To:            fullYear.To,
                OpeningHours:  CatalogHours(8, 0, 16, 0),
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range",            "group_size",            10,   50),
                    Constraint("RequiredDaysAhead","booking_days_ahead",    14,   null),
                    Constraint("OneOf",            "language",              null, null, "polski", "english"),
                    Constraint("OneOf",            "guide_mandatory",       null, null, "true"),
                    Constraint("OneOf",            "client_type",           null, null, "szkoła"),
                    // ratio rule: at least 1 supervisor per 15 pupils
                    Constraint("Min",              "supervisor_count",      1,    null),
                    Constraint("OneOf",            "accessibility",         null, null,
                        "brak_wózków_inwalidzkich"),
                ],
                PricingPeriods: TouristSchoolPricing(),
                Capacity:      200),

            // ─ Tourist / Family Ticket ────────────────────────────────────────
            // 2 adults + 2–3 children (4–16), all-in per-person rate
            // Separate from individual so pricing and constraints can differ
            new CatalogSeed(
                CatalogKey:    "wieliczka-tourist-family",
                ComponentKey:  "wieliczka-tourist-route",
                Name:          "Trasa Turystyczna — bilet rodzinny (2 dorosłych + maks. 3 dzieci 4–16 lat)",
                Description:   "Pakiet rodzinny: 2 osoby dorosłe + do 3 dzieci w wieku 4–16 lat. " +
                               "Cena podana per osoba (bilet łączony, oddzielne od biletów indywidualnych). " +
                               "Dzieci poniżej 4 lat wchodzą bezpłatnie.",
                Tags:          [Tag("turystyczna", "trasa"), Tag("rodzinny", "typ"),
                                Tag("family-friendly", "audience"), Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10 (Szyb Daniłowicza)",
                From:          fullYear.From,
                To:            fullYear.To,
                OpeningHours:  CatalogHours(7, 30, 18, 0),
                IsEvent:       false,
                Constraints:   [
                    // 2 adults + 2–3 children = 4–5 people total
                    Constraint("Range", "group_size",            4,    5),
                    Constraint("Range", "adult_count",           2,    2),
                    Constraint("Range", "children_count",        2,    3),
                    Constraint("Min",   "min_child_age",         4,    null),
                    Constraint("Max",   "max_child_age",         16,   null),
                    Constraint("RequiredDaysAhead","booking_days_ahead", 1, null),
                    Constraint("OneOf", "language",              null, null,
                        "polski", "english", "deutsch"),
                    Constraint("OneOf", "guide_mandatory",       null, null, "true"),
                    Constraint("OneOf", "slot_interval_minutes", null, null, "30"),
                    Constraint("OneOf", "children_under4_policy",null, null,
                        "bezpłatny_z_opiekunem"),
                ],
                PricingPeriods: [
                    Pricing(lowFrom,  lowTo,  99),
                    Pricing(highFrom, highTo, 109),
                    Pricing(offFrom,  offTo,  99),
                ],
                Capacity: 150),

            // ═════════════════════════════════════════════════════════════════
            // MINING ROUTE — 2 variants
            // ═════════════════════════════════════════════════════════════════

            // ─ Mining / Individual + Small Group ──────────────────────────────
            // 4–15 people, min age 12, fitness required, helmet & lamp included
            // Slots every 60 min (more time needed between groups — narrower passages)
            new CatalogSeed(
                CatalogKey:    "wieliczka-miner-individual",
                ComponentKey:  "wieliczka-miner-route",
                Name:          "Trasa Górnicza — bilet indywidualny / mała grupa (4–15 osób)",
                Description:   "Aktywna trasa górnicza dla małych grup. Start przez Szyb Regis. " +
                               "Kask i lampa górnicza wliczone w cenę. Uczestnicy uczą się górnictwa przez działanie. " +
                               "Wymagana sprawność fizyczna i min. 12 lat.",
                Tags:          [Tag("górnicza", "trasa"), Tag("indywidualny", "typ"),
                                Tag("przygoda", "kategoria"), Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10 (Szyb Regis)",
                From:          fullYear.From,
                To:            fullYear.To,
                OpeningHours:  CatalogHours(8, 0, 17, 0),
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range",            "group_size",            4,    15),
                    Constraint("RequiredDaysAhead","booking_days_ahead",    5,    null),
                    Constraint("OneOf",            "language",              null, null, "polski", "english"),
                    Constraint("OneOf",            "guide_mandatory",       null, null, "true"),
                    Constraint("Min",              "min_age",               12,   null),
                    Constraint("OneOf",            "fitness_required",      null, null,
                        "dobra_kondycja_fizyczna"),
                    Constraint("OneOf",            "equipment_provided",    null, null,
                        "kask", "lampa_górnicza"),
                    Constraint("OneOf",            "accessibility",         null, null,
                        "brak_wózków_inwalidzkich", "wąskie_chodniki",
                        "możliwe_czołganie"),
                    Constraint("OneOf",            "slot_interval_minutes", null, null, "60"),
                ],
                PricingPeriods: [
                    Pricing(lowFrom,  lowTo,  139, discUlgowy),
                    Pricing(highFrom, highTo, 149, discUlgowy),
                    Pricing(offFrom,  offTo,  139, discUlgowy),
                ],
                Capacity: 60),

            // ─ Mining / School & Youth Group ──────────────────────────────────
            // Secondary schools and universities, pedagogical guide, min age 16
            new CatalogSeed(
                CatalogKey:    "wieliczka-miner-school",
                ComponentKey:  "wieliczka-miner-route",
                Name:          "Trasa Górnicza — program edukacyjny dla szkół (młodzież 16+)",
                Description:   "Edukacyjna wersja trasy górniczej dla uczniów szkół ponadpodstawowych i studentów. " +
                               "Przewodnik z przygotowaniem pedagogicznym. Ograniczenia sprawnościowe i wiekowe (min. 16 lat) obowiązują.",
                Tags:          [Tag("górnicza", "trasa"), Tag("szkolny", "typ"),
                                Tag("edukacja", "kategoria"), Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10 (Szyb Regis)",
                From:          fullYear.From,
                To:            fullYear.To,
                OpeningHours:  CatalogHours(9, 0, 15, 0),
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range",            "group_size",         8,    20),
                    Constraint("RequiredDaysAhead","booking_days_ahead", 14,   null),
                    Constraint("OneOf",            "language",           null, null, "polski", "english"),
                    Constraint("OneOf",            "guide_mandatory",    null, null, "true"),
                    Constraint("OneOf",            "client_type",        null, null,
                        "szkoła_ponadpodstawowa", "uczelnia_wyższa"),
                    Constraint("Min",              "min_age",            16,   null),
                    Constraint("OneOf",            "fitness_required",   null, null,
                        "dobra_kondycja_fizyczna"),
                    Constraint("OneOf",            "equipment_provided", null, null,
                        "kask", "lampa_górnicza"),
                ],
                PricingPeriods: [
                    Pricing(lowFrom,  lowTo,  99),
                    Pricing(highFrom, highTo, 109),
                    Pricing(offFrom,  offTo,  99),
                ],
                Capacity: 40),

            // ═════════════════════════════════════════════════════════════════
            // BRINE TOWER — 2 variants (seasonal: May–Sep only)
            // ═════════════════════════════════════════════════════════════════

            // ─ Brine Tower / Standard ─────────────────────────────────────────
            new CatalogSeed(
                CatalogKey:    "wieliczka-brine-standard",
                ComponentKey:  "wieliczka-brine-tower",
                Name:          "Tężnia Solankowa — wstęp standardowy",
                Description:   "Wstęp na teren tężni solankowej. Bez przewodnika, swobodne przebywanie. " +
                               "Dostępna dla wózków inwalidzkich i dziecięcych. Korzystna dla alergików i astmatyków.",
                Tags:          [Tag("tężnia", "trasa"), Tag("standardowy", "typ"),
                                Tag("zdrowie", "kategoria"), Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10 (teren naziemny)",
                From:          brineSeason.From,
                To:            brineSeason.To,
                OpeningHours:  CatalogHours(9, 0, 18, 0),
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range", "group_size",       1, 100),
                    Constraint("OneOf", "guide_mandatory",  null, null, "false"),
                    Constraint("OneOf", "accessibility",    null, null,
                        "dostępne_dla_wózków", "brak_ograniczeń_wiekowych"),
                ],
                PricingPeriods: [
                    new PricingPeriod(
                        new PB.Modules.Catalog.Domain.ValueObjects.DateRange(brineSeason.From, brineSeason.To),
                        new Money(40, "PLN"),
                        [
                            new Discount("Bilet ulgowy — studenci, seniorzy 65+",
                                null, new Money(10, "PLN"), "ulgowy"),
                            new Discount("Dzieci 4–16 lat",
                                null, new Money(10, "PLN"), "wiek:4-16"),
                            new Discount("Dzieci do lat 4 bezpłatnie",
                                100m, null, "wiek:<4"),
                        ]),
                ],
                Capacity: 300),

            // ─ Brine Tower / School ──────────────────────────────────────────
            new CatalogSeed(
                CatalogKey:    "wieliczka-brine-school",
                ComponentKey:  "wieliczka-brine-tower",
                Name:          "Tężnia Solankowa — wstęp szkolny z pokazem edukacyjnym",
                Description:   "Grupowe wejście szkolne połączone z krótkim pokazem o właściwościach solanki " +
                               "i historii tężni solankowych w Polsce. Dla klas wszystkich poziomów.",
                Tags:          [Tag("tężnia", "trasa"), Tag("szkolny", "typ"),
                                Tag("edukacja", "kategoria"), Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10 (teren naziemny)",
                From:          brineSeason.From,
                To:            brineSeason.To,
                OpeningHours:  CatalogHours(9, 0, 16, 0),
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range",            "group_size",         10,   60),
                    Constraint("RequiredDaysAhead","booking_days_ahead", 7,    null),
                    Constraint("OneOf",            "client_type",        null, null, "szkoła"),
                    Constraint("OneOf",            "accessibility",      null, null,
                        "dostępne_dla_wózków"),
                ],
                PricingPeriods: [
                    new PricingPeriod(
                        new PB.Modules.Catalog.Domain.ValueObjects.DateRange(brineSeason.From, brineSeason.To),
                        new Money(25, "PLN")),
                ],
                Capacity: 200),

            // ═════════════════════════════════════════════════════════════════
            // LEGENDS TRAIL (Śladami Legend) — 1 variant
            // ═════════════════════════════════════════════════════════════════

            // ─ Legends Trail / Family ─────────────────────────────────────────
            // Children 5–12, adult guardian required, story-driven
            // Separate pricing for children vs adults within same entry via discounts
            new CatalogSeed(
                CatalogKey:    "wieliczka-legends-family",
                ComponentKey:  "wieliczka-legends-trail",
                Name:          "Śladami Legend — trasa rodzinna (dzieci 5–12 lat)",
                Description:   "Interaktywna trasa tematyczna z przewodnikiem w kostiumie górnika. " +
                               "Przeznaczona dla dzieci 5–12 lat z rodzicami/opiekunami. Czas: ok. 1,5 h, głębokość 64 m. " +
                               "Cena od 105 PLN/osobę. Wymagany co najmniej 1 dorosły opiekun.",
                Tags:          [Tag("śladami-legend", "trasa"), Tag("rodzinny", "typ"),
                                Tag("dzieci", "audience"), Tag("interaktywna", "kategoria"),
                                Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10 (Szyb Daniłowicza)",
                From:          fullYear.From,
                To:            fullYear.To,
                OpeningHours:  CatalogHours(9, 0, 16, 0),
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range", "group_size",              2,    20),
                    Constraint("RequiredDaysAhead","booking_days_ahead", 3, null),
                    Constraint("OneOf", "language",                null, null, "polski", "english"),
                    Constraint("OneOf", "guide_mandatory",         null, null, "true"),
                    Constraint("Min",   "min_child_age",           5,    null),
                    Constraint("Max",   "max_child_age",           12,   null),
                    Constraint("OneOf", "adult_guardian_required", null, null, "true"),
                    Constraint("OneOf", "slot_interval_minutes",   null, null, "60"),
                ],
                PricingPeriods: [
                    // Adult baseline; child discount modeled via Discount
                    Pricing(lowFrom, lowTo, 95,
                        new Discount("Dzieci 5–12 lat", null, new Money(30, "PLN"), "dziecko:5-12")),
                    Pricing(highFrom, highTo, 105,
                        new Discount("Dzieci 5–12 lat", null, new Money(35, "PLN"), "dziecko:5-12")),
                    Pricing(offFrom, offTo, 95,
                        new Discount("Dzieci 5–12 lat", null, new Money(30, "PLN"), "dziecko:5-12")),
                ],
                Capacity: 100),

            // ═════════════════════════════════════════════════════════════════
            // PILGRIMAGE ROUTE — 1 variant
            // ═════════════════════════════════════════════════════════════════

            new CatalogSeed(
                CatalogKey:    "wieliczka-pilgrimage-standard",
                ComponentKey:  "wieliczka-pilgrimage-route",
                Name:          "Szlak Pielgrzymkowy — bilet grupowy (min. 15 osób)",
                Description:   "Duchowa trasa przez Kaplicę Błogosławionej Kingi. Możliwość Mszy Świętej pod ziemią " +
                               "(oddzielna rezerwacja z duszpasterzem). Wyłącznie grupy pielgrzymkowe, min. 15 osób. " +
                               "Rezerwacja min. 14 dni naprzód.",
                Tags:          [Tag("pielgrzymka", "trasa"), Tag("grupowy", "typ"),
                                Tag("religia", "kategoria"), Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10 (Szyb Daniłowicza)",
                From:          fullYear.From,
                To:            fullYear.To,
                OpeningHours:  CatalogHours(8, 0, 16, 0),
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range",            "group_size",         15,   60),
                    Constraint("RequiredDaysAhead","booking_days_ahead", 14,   null),
                    Constraint("OneOf",            "language",           null, null,
                        "polski", "latin", "english"),
                    Constraint("OneOf",            "guide_mandatory",    null, null, "true"),
                    Constraint("OneOf",            "client_type",        null, null, "pielgrzymka"),
                ],
                PricingPeriods: [
                    Pricing(lowFrom,  lowTo,  79),
                    Pricing(highFrom, highTo, 89),
                    Pricing(offFrom,  offTo,  79),
                ],
                Capacity: 120),

            // ═════════════════════════════════════════════════════════════════
            // PACKAGES — 3 catalog entries for the packages defined above
            // MODEL NOTE: The CatalogEntry for a package uses the package's
            // component Id so it is bookable as a single offering.
            // ═════════════════════════════════════════════════════════════════

            // ─ Combo: Tourist + Miner ─────────────────────────────────────────
            // Saving vs. buying both separately (119+149=268 vs. 249 in high season)
            new CatalogSeed(
                CatalogKey:    "wieliczka-combo-tourist-miner",
                ComponentKey:  "wieliczka-combo-tourist-miner",
                Name:          "Kombo: Trasa Turystyczna + Górnicza — bilet łączony",
                Description:   "Bilet łączony na obie trasy kopalni w korzystniejszej cenie. " +
                               "Trasy można zwiedzić jednego dnia lub w dwa kolejne dni. " +
                               "Min. wiek 12 lat, sprawność fizyczna wymagana (warunek Trasy Górniczej).",
                Tags:          [Tag("kombo", "typ"), Tag("oszczędność", "kategoria"),
                                Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10",
                From:          fullYear.From,
                To:            fullYear.To,
                OpeningHours:  CatalogHours(8, 0, 17, 0),
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range",            "group_size",         4,    15),
                    Constraint("RequiredDaysAhead","booking_days_ahead", 7,    null),
                    Constraint("OneOf",            "language",           null, null, "polski", "english"),
                    Constraint("OneOf",            "guide_mandatory",    null, null, "true"),
                    Constraint("Min",              "min_age",            12,   null),
                    Constraint("OneOf",            "fitness_required",   null, null,
                        "dobra_kondycja_fizyczna"),
                    Constraint("OneOf",            "equipment_provided", null, null,
                        "kask", "lampa_górnicza"),
                ],
                PricingPeriods: [
                    Pricing(lowFrom, lowTo, 229,
                        new Discount("Oszczędność kombo vs. zakup osobny (139+109→229)", null, new Money(19, "PLN"), "kombo")),
                    Pricing(highFrom, highTo, 249,
                        new Discount("Oszczędność kombo vs. zakup osobny (149+119→249)", null, new Money(19, "PLN"), "kombo")),
                    Pricing(offFrom, offTo, 229,
                        new Discount("Oszczędność kombo vs. zakup osobny", null, new Money(19, "PLN"), "kombo")),
                ],
                Capacity: 60),

            // ─ Family Combo: Tourist + Legends ───────────────────────────────
            new CatalogSeed(
                CatalogKey:    "wieliczka-family-combo",
                ComponentKey:  "wieliczka-family-combo",
                Name:          "Pakiet Rodzinny: Trasa Turystyczna + Śladami Legend",
                Description:   "Pakiet dla rodzin z dziećmi 5–12 lat. Dorośli zwiedzają pełną Trasę Turystyczną, " +
                               "dzieci odkrywają kopalnię Śladami Legend z osobnym przewodnikiem. " +
                               "Wyjście razem. Cena per osoba (mieszana stawka dorosły/dziecko).",
                Tags:          [Tag("pakiet-rodzinny", "typ"), Tag("family-friendly", "audience"),
                                Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10",
                From:          fullYear.From,
                To:            fullYear.To,
                OpeningHours:  CatalogHours(9, 0, 16, 0),
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range", "group_size",              2,    10),
                    Constraint("RequiredDaysAhead","booking_days_ahead", 3, null),
                    Constraint("OneOf", "language",                null, null, "polski", "english"),
                    Constraint("OneOf", "guide_mandatory",         null, null, "true"),
                    Constraint("Min",   "min_child_age",           5,    null),
                    Constraint("Max",   "max_child_age",           12,   null),
                    Constraint("OneOf", "adult_guardian_required", null, null, "true"),
                ],
                PricingPeriods: [
                    // Bundled per-person rate (adults + children averaged)
                    Pricing(lowFrom,  lowTo,  185,
                        new Discount("Dzieci 5–12 lat w pakiecie", null, new Money(60, "PLN"), "dziecko:5-12")),
                    Pricing(highFrom, highTo, 199,
                        new Discount("Dzieci 5–12 lat w pakiecie", null, new Money(70, "PLN"), "dziecko:5-12")),
                    Pricing(offFrom,  offTo,  185,
                        new Discount("Dzieci 5–12 lat w pakiecie", null, new Money(60, "PLN"), "dziecko:5-12")),
                ],
                Capacity: 80),

            // ─ Mine + Hotel Package ───────────────────────────────────────────
            // Price matches the website: "od 529 PLN/osobę"
            new CatalogSeed(
                CatalogKey:    "wieliczka-mine-hotel",
                ComponentKey:  "wieliczka-mine-hotel-pkg",
                Name:          "Pakiet: Zwiedzanie Kopalni + Nocleg w hotelu",
                Description:   "Pobyt z noclegiem bezpośrednio przy kopalni i wejściem na Trasę Turystyczną " +
                               "w specjalnej cenie. Wczesny dostęp do kopalni przed otwarciem dla turystów indywidualnych. " +
                               "Śniadanie w cenie. Cena od 529 PLN/osobę (wysokie sezony).",
                Tags:          [Tag("pakiet-noclegowy", "typ"), Tag("nocleg", "kategoria"),
                                Tag("wieliczka", "loc")],
                City:          "Wieliczka",
                Address:       "ul. Daniłowicza 10",
                From:          fullYear.From,
                To:            fullYear.To,
                OpeningHours:  null,
                IsEvent:       false,
                Constraints:   [
                    Constraint("Range",            "group_size",         1,    4),
                    Constraint("RequiredDaysAhead","booking_days_ahead", 7,    null),
                    Constraint("OneOf",            "language",           null, null, "polski", "english"),
                    Constraint("Min",              "min_nights",         1,    null),
                    Constraint("OneOf",            "guide_mandatory",    null, null, "true"),
                ],
                PricingPeriods: [
                    Pricing(lowFrom,  lowTo,  499),
                    Pricing(highFrom, highTo, 529),
                    Pricing(offFrom,  offTo,  499),
                ],
                Capacity: 40),
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
            foreach (var period in seed.PricingPeriods)
                entry.AddPricingPeriod(period);

            catalogEntriesByKey[seed.CatalogKey] = entry;
            await _catalogRepository.AddAsync(entry);
            await _ticketPoolRepository.AddAsync(new TicketPool(entry.Id, seed.Capacity));
        }

        // ── 4. Attraction Relations ───────────────────────────────────────────
        var relationSeeds = new[]
        {
            // Tourist → Miner: natural deepening of the mine experience
            new RelationSeed("wieliczka-tourist-route", "wieliczka-miner-route",
                RelationType.Suggests, "pogłębienie_doświadczenia",
                "Po Trasie Turystycznej naturalnym uzupełnieniem jest Trasa Górnicza — autentyczna praca górnika."),

            // Tourist → Brine Tower: recovery above ground after 3 h underground
            new RelationSeed("wieliczka-tourist-route", "wieliczka-brine-tower",
                RelationType.Suggests, "relaks_po_trasie",
                "Tężnia Solankowa to doskonały relaks na świeżym powietrzu po kilku godzinach pod ziemią."),

            // Tourist → Legends: same entry shaft, recommended for families with kids 5–12
            new RelationSeed("wieliczka-tourist-route", "wieliczka-legends-trail",
                RelationType.Suggests, "oferta_dla_rodzin",
                "Rodziny z dziećmi 5–12 lat powinny rozważyć równoległą rezerwację Śladami Legend."),

            // Tourist → Pilgrimage: alternative framing for religious groups
            new RelationSeed("wieliczka-tourist-route", "wieliczka-pilgrimage-route",
                RelationType.Suggests, "alternatywa_dla_pielgrzymek",
                "Grupy pielgrzymkowe mogą wybrać Szlak Pielgrzymkowy jako uzupełnienie lub zamiennik Trasy Turystycznej."),

            // Miner REQUIRES Tourist: orientation prerequisite
            // (described on the real site: prior visit to tourist route recommended)
            new RelationSeed("wieliczka-miner-route", "wieliczka-tourist-route",
                RelationType.Requires, "wymagany_wstęp",
                "Trasa Górnicza wymaga wcześniejszej orientacji w kopalni — zaleca się uprzednie zwiedzenie Trasy Turystycznej."),

            // Miner EXCLUDES Legends: age conflict (Miner min 12, Legends for 5–12 children)
            new RelationSeed("wieliczka-miner-route", "wieliczka-legends-trail",
                RelationType.Excludes, "konflikt_wiekowy",
                "Trasa Górnicza (min. wiek 12 lat, sprawność fizyczna) jest niezgodna z trasą Śladami Legend (dzieci 5–12 lat)."),

            // Miner → Brine Tower: recovery after strenuous physical route
            new RelationSeed("wieliczka-miner-route", "wieliczka-brine-tower",
                RelationType.Suggests, "regeneracja_po_trasie",
                "Po wyczerpującej Trasie Górniczej warto odpocząć przy tężni solankowej."),

            // Legends → Brine Tower: both family-friendly, complement each other
            new RelationSeed("wieliczka-legends-trail", "wieliczka-brine-tower",
                RelationType.Suggests, "oferta_dla_rodzin",
                "Tężnia Solankowa świetnie uzupełnia trasę dziecięcą — rodziny mogą spędzić czas na powietrzu przed lub po wejściu."),

            // Pilgrimage → Tourist: see the full mine after the spiritual route
            new RelationSeed("wieliczka-pilgrimage-route", "wieliczka-tourist-route",
                RelationType.Suggests, "pełne_doświadczenie",
                "Po Szlaku Pielgrzymkowym warto uzupełnić wizytę o pełną Trasę Turystyczną, by poznać historię kopalni."),

            // Combo Tourist+Miner EXCLUDES Family Combo: audience/age incompatibility
            new RelationSeed("wieliczka-combo-tourist-miner", "wieliczka-family-combo",
                RelationType.Excludes, "konflikt_grupy_docelowej",
                "Kombo Turystyczna+Górnicza (min. 12 lat, sprawność fizyczna) jest niezgodne z Pakietem Rodzinnym (dzieci 5–12 lat)."),
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

        // ── Summary ───────────────────────────────────────────────────────────
        Console.WriteLine("=== KOPALNIA SOLI WIELICZKA — DATA SEEDER ===");
        Console.WriteLine($"AttractionComponents: {componentsByKey.Count}  " +
                          $"({attractionSeeds.Length} trasy + {packageSeeds.Length} pakiety)");
        Console.WriteLine($"CatalogEntries:       {catalogEntriesByKey.Count}");
        Console.WriteLine($"TicketPools:          {catalogEntriesByKey.Count}");
        Console.WriteLine($"Relations:            {relationSeeds.Length}");
        Console.WriteLine("=============================================");
        Console.WriteLine();
        Console.WriteLine("Catalog IDs:");
        foreach (var (key, entry) in catalogEntriesByKey)
            Console.WriteLine($"  [{key,-38}] {entry.Id}");
        Console.WriteLine("=============================================");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Tag Tag(string name, string? group = null) => new(name, group);

    private static OpeningHours Hours(int oh, int om, int ch, int cm) =>
        new(new TimeOnly(oh, om), new TimeOnly(ch, cm));

    private static CatalogOpeningHours CatalogHours(int oh, int om, int ch, int cm) =>
        new(new TimeOnly(oh, om), new TimeOnly(ch, cm));

    private static BookingConstraint Constraint(
        string type, string key, decimal? min, decimal? max, params string[] allowed) =>
        new(type, key, min, max, allowed.Length == 0 ? Array.Empty<string>() : allowed);

    private static PricingPeriod Pricing(
        DateOnly from, DateOnly to, decimal amount, params Discount[] discounts) =>
        new(new PB.Modules.Catalog.Domain.ValueObjects.DateRange(from, to),
            new Money(amount, "PLN"),
            discounts.Length == 0 ? null : discounts);

    // ── Seed record types ─────────────────────────────────────────────────────

    private sealed record AttractionSeed(
        string Key, string Name, string Description,
        IReadOnlyList<Tag> Tags, Location Location, OpeningHours? OpeningHours);

    private sealed record PackageSeed(
        string Key, string Name, string Description,
        SelectionRule SelectionRule, IReadOnlyList<string> ComponentKeys);

    private sealed record CatalogSeed(
        string CatalogKey,
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
        string SourceKey, string TargetKey,
        RelationType Type, string? Context, string? Description);
}
