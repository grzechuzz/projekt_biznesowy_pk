using PB.Modules.AttractionDefinition.Application.Services;
using PB.Modules.Catalog.Application.Services;
using PB.Modules.Availability.Application.DTOs;
using PB.Modules.Availability.Application.Services;
using PB.Modules.TripSelection.Application.Services;

using DefTagDto = PB.Modules.AttractionDefinition.Application.DTOs.TagDto;
using DefConstraintDto = PB.Modules.AttractionDefinition.Application.DTOs.ConstraintDto;
using CreateAttractionDefinitionDto = PB.Modules.AttractionDefinition.Application.DTOs.CreateAttractionDefinitionDto;
using AddVariantDto = PB.Modules.AttractionDefinition.Application.DTOs.AddVariantDto;
using LocationDto = PB.Modules.AttractionDefinition.Application.DTOs.LocationDto;
using OpeningHoursDto = PB.Modules.AttractionDefinition.Application.DTOs.OpeningHoursDto;
using SeasonalAvailabilityDto = PB.Modules.AttractionDefinition.Application.DTOs.SeasonalAvailabilityDto;
using SelectionRuleDto = PB.Modules.AttractionDefinition.Application.DTOs.SelectionRuleDto;
using CreatePackageDto = PB.Modules.AttractionDefinition.Application.DTOs.CreatePackageDto;
using CatTagDto = PB.Modules.Catalog.Application.DTOs.TagDto;
using CatalogLocationDto = PB.Modules.Catalog.Application.DTOs.CatalogLocationDto;
using DateRangeDto = PB.Modules.Catalog.Application.DTOs.DateRangeDto;
using CatalogOpeningHoursDto = PB.Modules.Catalog.Application.DTOs.CatalogOpeningHoursDto;
using CreateCatalogEntryDto = PB.Modules.Catalog.Application.DTOs.CreateCatalogEntryDto;
using BookingConstraintDto = PB.Modules.Catalog.Application.DTOs.BookingConstraintDto;
using AddPricingPeriodDto = PB.Modules.Catalog.Application.DTOs.AddPricingPeriodDto;
using MoneyDto = PB.Modules.Catalog.Application.DTOs.MoneyDto;
using DiscountDto = PB.Modules.Catalog.Application.DTOs.DiscountDto;
using CreateRelationDto = PB.Modules.TripSelection.Application.DTOs.CreateRelationDto;

namespace PB.Api;

public class DataSeeder
{
    private readonly IAttractionDefinitionService _definitionService;
    private readonly IAttractionPackageService _packageService;
    private readonly ICatalogService _catalogService;
    private readonly IAvailabilityService _availabilityService;
    private readonly IAttractionRelationService _relationService;

    public DataSeeder(
        IAttractionDefinitionService definitionService,
        IAttractionPackageService packageService,
        ICatalogService catalogService,
        IAvailabilityService availabilityService,
        IAttractionRelationService relationService)
    {
        _definitionService = definitionService;
        _packageService = packageService;
        _catalogService = catalogService;
        _availabilityService = availabilityService;
        _relationService = relationService;
    }

    public async Task SeedAsync()
    {
        // ============================================================
        // 1. ATTRACTION DEFINITIONS (archetypes/drafts)
        // ============================================================

        // --- Wawel Castle ---
        var wawel = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Wawel Castle",
            "Royal castle and cathedral on Wawel Hill in Krakow. One of the most important historical sites in Poland.",
            new List<DefTagDto>
            {
                new("castle", "type"),
                new("history", "category"),
                new("landmark", "category"),
                new("krakow", "loc"),
                new("unesco", "category")
            },
            new LocationDto("Kraków", "Wawel 5, 31-001 Kraków", 50.0540, 19.9354),
            new OpeningHoursDto(new TimeOnly(9, 30), new TimeOnly(17, 0)),
            new SeasonalAvailabilityDto(true, true, true, false)));

        // Wawel variants
        var wawelWithVariants = await _definitionService.AddVariantAsync(wawel.Id, new AddVariantDto(
            "State Rooms",
            "Tour of the Royal State Rooms with Renaissance furnishings and Flemish tapestries",
            new List<DefTagDto>
            {
                new("guided-tour", "type"),
                new("renaissance", "style")
            },
            new List<DefConstraintDto>
            {
                new("Range", "group_size", 1, 15, null),
                new("RequiredDaysAhead", "booking_days_ahead", 2, null, null),
                new("OneOf", "language", null, null, new List<string> { "polish", "english", "german" })
            },
            90));

        var stateRoomsVariantId = wawelWithVariants.Variants[0].Id;

        wawelWithVariants = await _definitionService.AddVariantAsync(wawel.Id, new AddVariantDto(
            "Armoury",
            "Collection of historical weapons and armour from the 15th-18th century",
            new List<DefTagDto>
            {
                new("museum", "type"),
                new("military", "category")
            },
            new List<DefConstraintDto>
            {
                new("Range", "group_size", 1, 25, null),
                new("RequiredDaysAhead", "booking_days_ahead", 1, null, null)
            },
            60));

        var armouryVariantId = wawelWithVariants.Variants[1].Id;

        wawelWithVariants = await _definitionService.AddVariantAsync(wawel.Id, new AddVariantDto(
            "Dragon's Den",
            "The legendary Dragon's Den cave beneath Wawel Hill",
            new List<DefTagDto>
            {
                new("cave", "type"),
                new("legend", "category"),
                new("family-friendly", "audience")
            },
            null,
            30));

        var dragonsDenVariantId = wawelWithVariants.Variants[2].Id;

        // --- St. Mary's Basilica ---
        var stMarys = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "St. Mary's Basilica",
            "Gothic church on the Main Market Square, famous for the wooden altarpiece by Veit Stoss and the trumpet signal played every hour.",
            new List<DefTagDto>
            {
                new("church", "type"),
                new("gothic", "style"),
                new("history", "category"),
                new("krakow", "loc")
            },
            new LocationDto("Kraków", "Plac Mariacki 5, 31-042 Kraków", 50.0616, 19.9394),
            new OpeningHoursDto(new TimeOnly(11, 30), new TimeOnly(18, 0)),
            new SeasonalAvailabilityDto(true, true, true, true)));

        await _definitionService.AddVariantAsync(stMarys.Id, new AddVariantDto(
            "Altar Visit",
            "Close-up viewing of the Veit Stoss altarpiece, the largest Gothic altarpiece in the world",
            new List<DefTagDto>
            {
                new("art", "category")
            },
            new List<DefConstraintDto>
            {
                new("Max", "group_size", null, 20, null)
            },
            45));

        var stMarysUpdated = await _definitionService.GetByIdAsync(stMarys.Id);
        var altarVariantId = stMarysUpdated!.Variants[0].Id;

        // --- Sukiennice (Cloth Hall) ---
        var sukiennice = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Sukiennice - Cloth Hall",
            "Renaissance trading hall in the center of Main Market Square. Houses the Gallery of 19th-Century Polish Art upstairs and market stalls on the ground floor.",
            new List<DefTagDto>
            {
                new("market", "type"),
                new("museum", "type"),
                new("renaissance", "style"),
                new("shopping", "category"),
                new("krakow", "loc")
            },
            new LocationDto("Kraków", "Rynek Główny 1/3, 31-042 Kraków", 50.0614, 19.9372),
            new OpeningHoursDto(new TimeOnly(10, 0), new TimeOnly(18, 0)),
            new SeasonalAvailabilityDto(true, true, true, true)));

        // --- Tauron Arena Concert ---
        var tauron = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Concert at Tauron Arena",
            "Live music event at Tauron Arena Kraków, the largest entertainment venue in southern Poland.",
            new List<DefTagDto>
            {
                new("concert", "type"),
                new("music", "category"),
                new("event", "category"),
                new("krakow", "loc")
            },
            new LocationDto("Kraków", "ul. Stanisława Lema 7, 31-571 Kraków", 50.0694, 20.0106),
            null,
            new SeasonalAvailabilityDto(true, true, true, true)));

        // --- Kazimierz District Tour ---
        var kazimierz = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Kazimierz District Walking Tour",
            "Walking tour through the historic Jewish quarter of Kraków. Explore synagogues, street art, cafes, and the vibrant atmosphere of Kazimierz.",
            new List<DefTagDto>
            {
                new("walking-tour", "type"),
                new("history", "category"),
                new("culture", "category"),
                new("jewish-heritage", "category"),
                new("krakow", "loc")
            },
            new LocationDto("Kraków", "ul. Szeroka 24, 31-053 Kraków", 50.0512, 19.9459),
            new OpeningHoursDto(new TimeOnly(10, 0), new TimeOnly(16, 0)),
            new SeasonalAvailabilityDto(true, true, true, true)));

        await _definitionService.AddVariantAsync(kazimierz.Id, new AddVariantDto(
            "Standard Tour",
            "2.5 hour guided walking tour of the main Kazimierz landmarks",
            new List<DefTagDto>
            {
                new("guided-tour", "type")
            },
            new List<DefConstraintDto>
            {
                new("Range", "group_size", 2, 12, null),
                new("OneOf", "language", null, null, new List<string> { "polish", "english" })
            },
            150));

        var kazimierzUpdated = await _definitionService.GetByIdAsync(kazimierz.Id);
        var kazimierzTourVariantId = kazimierzUpdated!.Variants[0].Id;

        // --- Pijalnia Wodki i Piwa ---
        var pijalnia = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Pijalnia Wódki i Piwa",
            "Popular bar chain in Kraków serving cheap shots of vodka and beers. A must-visit Kraków nightlife experience.",
            new List<DefTagDto>
            {
                new("bar", "type"),
                new("nightlife", "category"),
                new("beer", "category"),
                new("vodka", "category"),
                new("krakow", "loc")
            },
            new LocationDto("Kraków", "ul. Mikołajska 5, 31-027 Kraków", 50.0608, 19.9410),
            new OpeningHoursDto(new TimeOnly(12, 0), new TimeOnly(23, 59)),
            new SeasonalAvailabilityDto(true, true, true, true)));

        // --- Wieliczka Salt Mine ---
        var wieliczka = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Wieliczka Salt Mine",
            "UNESCO World Heritage Site. Underground salt mine with chapels, sculptures, and an underground lake, all carved from salt.",
            new List<DefTagDto>
            {
                new("mine", "type"),
                new("unesco", "category"),
                new("history", "category"),
                new("underground", "category"),
                new("wieliczka", "loc")
            },
            new LocationDto("Wieliczka", "ul. Daniłowicza 10, 32-020 Wieliczka", 49.9833, 20.0553),
            new OpeningHoursDto(new TimeOnly(8, 0), new TimeOnly(17, 0)),
            new SeasonalAvailabilityDto(true, true, true, true)));

        await _definitionService.AddVariantAsync(wieliczka.Id, new AddVariantDto(
            "Tourist Route",
            "3.5 km underground route with 20+ chambers including the Chapel of St. Kinga",
            new List<DefTagDto>
            {
                new("guided-tour", "type")
            },
            new List<DefConstraintDto>
            {
                new("Range", "group_size", 1, 35, null),
                new("RequiredDaysAhead", "booking_days_ahead", 3, null, null),
                new("OneOf", "language", null, null, new List<string> { "polish", "english", "german", "french", "italian", "spanish" })
            },
            180));

        var wieliczkaUpdated = await _definitionService.GetByIdAsync(wieliczka.Id);
        var wieliczkaTourVariantId = wieliczkaUpdated!.Variants[0].Id;

        // ============================================================
        // 2. ATTRACTION PACKAGE (Wawel Full Experience)
        // ============================================================

        var wawelPackage = await _packageService.CreateAsync(new CreatePackageDto(
            "Wawel Full Experience",
            "Visit all three Wawel attractions: State Rooms, Armoury, and the Dragon's Den. Choose at least 2 out of 3.",
            new SelectionRuleDto("PickN", 2),
            new List<Guid> { wawel.Id }));

        // ============================================================
        // 3. CATALOG ENTRIES (available instances for specific dates)
        // ============================================================

        var catalogFrom = new DateOnly(2026, 4, 1);
        var catalogTo = new DateOnly(2026, 6, 30);

        // Wawel State Rooms - catalog entry
        var wawelStateRoomsCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            wawel.Id, stateRoomsVariantId,
            "Wawel Castle - State Rooms Tour",
            "Daily guided tours of the Royal State Rooms. Available April-June 2026.",
            new List<CatTagDto>
            {
                new("castle", "type"), new("guided-tour", "type"),
                new("history", "category"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "Wawel 5"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(9, 30), new TimeOnly(17, 0)),
            false,
            new List<BookingConstraintDto>
            {
                new("Range", "group_size", 1, 15, null),
                new("RequiredDaysAhead", "booking_days_ahead", 2, null, null),
                new("OneOf", "language", null, null, new List<string> { "polish", "english", "german" })
            }));

        // Wawel Armoury - catalog entry
        var wawelArmouryCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            wawel.Id, armouryVariantId,
            "Wawel Castle - Armoury",
            "Visit the historical weapons collection at Wawel Castle.",
            new List<CatTagDto>
            {
                new("castle", "type"), new("museum", "type"),
                new("history", "category"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "Wawel 5"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(9, 30), new TimeOnly(17, 0)),
            false,
            new List<BookingConstraintDto>
            {
                new("Range", "group_size", 1, 25, null),
                new("RequiredDaysAhead", "booking_days_ahead", 1, null, null)
            }));

        // Dragon's Den - catalog entry (no booking constraints - walk-in)
        var dragonsDenCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            wawel.Id, dragonsDenVariantId,
            "Wawel Castle - Dragon's Den",
            "Visit the legendary Dragon's Den cave.",
            new List<CatTagDto>
            {
                new("cave", "type"), new("family-friendly", "audience"),
                new("legend", "category"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "Wawel 5"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(10, 0), new TimeOnly(17, 0)),
            false, null));

        // St. Mary's Basilica - catalog entry
        var stMarysCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            stMarys.Id, altarVariantId,
            "St. Mary's Basilica - Altar Visit",
            "See the world-famous Veit Stoss altarpiece up close.",
            new List<CatTagDto>
            {
                new("church", "type"), new("art", "category"),
                new("gothic", "style"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "Plac Mariacki 5"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(11, 30), new TimeOnly(18, 0)),
            false,
            new List<BookingConstraintDto>
            {
                new("Max", "group_size", null, 20, null)
            }));

        // Sukiennice - catalog entry
        var sukienniceCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            sukiennice.Id, null,
            "Sukiennice - Cloth Hall & Gallery",
            "Visit the Cloth Hall market and the Gallery of 19th-Century Polish Art.",
            new List<CatTagDto>
            {
                new("market", "type"), new("museum", "type"),
                new("shopping", "category"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "Rynek Główny 1/3"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(10, 0), new TimeOnly(18, 0)),
            false, null));

        // Tauron Arena concert - EVENT
        var tauronConcertCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            tauron.Id, null,
            "Rock Festival at Tauron Arena",
            "Evening rock concert at Tauron Arena Kraków. Gates open at 18:00.",
            new List<CatTagDto>
            {
                new("concert", "type"), new("music", "category"),
                new("event", "category"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "ul. Stanisława Lema 7"),
            new DateRangeDto(new DateOnly(2026, 4, 12), new DateOnly(2026, 4, 12)),
            null,
            true, null));

        // Kazimierz tour - catalog entry
        var kazimierzCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            kazimierz.Id, kazimierzTourVariantId,
            "Kazimierz Walking Tour - Guided",
            "Discover the Jewish heritage and vibrant culture of Kazimierz district.",
            new List<CatTagDto>
            {
                new("walking-tour", "type"), new("history", "category"),
                new("culture", "category"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "ul. Szeroka 24"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(10, 0), new TimeOnly(16, 0)),
            false,
            new List<BookingConstraintDto>
            {
                new("Range", "group_size", 2, 12, null),
                new("OneOf", "language", null, null, new List<string> { "polish", "english" })
            }));

        // Pijalnia - catalog entry
        var pijalniaCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            pijalnia.Id, null,
            "Pijalnia Wódki i Piwa - Evening Out",
            "Classic Kraków bar experience. No reservation needed.",
            new List<CatTagDto>
            {
                new("bar", "type"), new("nightlife", "category"),
                new("beer", "category"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "ul. Mikołajska 5"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(12, 0), new TimeOnly(23, 59)),
            false, null));

        // Wieliczka Salt Mine - catalog entry
        var wieliczkaCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            wieliczka.Id, wieliczkaTourVariantId,
            "Wieliczka Salt Mine - Tourist Route",
            "3.5km underground tour including the Chapel of St. Kinga.",
            new List<CatTagDto>
            {
                new("mine", "type"), new("unesco", "category"),
                new("guided-tour", "type"), new("wieliczka", "loc")
            },
            new CatalogLocationDto("Wieliczka", "ul. Daniłowicza 10"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(8, 0), new TimeOnly(17, 0)),
            false,
            new List<BookingConstraintDto>
            {
                new("Range", "group_size", 1, 35, null),
                new("RequiredDaysAhead", "booking_days_ahead", 3, null, null),
                new("OneOf", "language", null, null, new List<string> { "polish", "english", "german", "french", "italian", "spanish" })
            }));

        // ============================================================
        // 4. PRICING PERIODS
        // ============================================================

        // Wawel State Rooms: April (lower) vs May-June (high season)
        await _catalogService.AddPricingPeriodAsync(wawelStateRoomsCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30),
            new MoneyDto(25, "PLN"),
            new List<DiscountDto>
            {
                new("Student discount", 50, null, "Valid student ID required"),
                new("Child under 7", 100, null, "Free for children under 7")
            }));

        await _catalogService.AddPricingPeriodAsync(wawelStateRoomsCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 5, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(35, "PLN"),
            new List<DiscountDto>
            {
                new("Student discount", 40, null, "Valid student ID required")
            }));

        // Wawel Armoury
        await _catalogService.AddPricingPeriodAsync(wawelArmouryCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(20, "PLN"),
            new List<DiscountDto>
            {
                new("Student discount", 50, null, null)
            }));

        // Dragon's Den - cheap, flat price
        await _catalogService.AddPricingPeriodAsync(dragonsDenCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(5, "PLN"), null));

        // St. Mary's
        await _catalogService.AddPricingPeriodAsync(stMarysCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(15, "PLN"), null));

        // Sukiennice Gallery
        await _catalogService.AddPricingPeriodAsync(sukienniceCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(12, "PLN"),
            new List<DiscountDto>
            {
                new("Free on Tuesdays", 100, null, "Permanent exhibitions free on Tuesdays")
            }));

        // Tauron concert
        await _catalogService.AddPricingPeriodAsync(tauronConcertCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 12), new DateOnly(2026, 4, 12),
            new MoneyDto(120, "PLN"),
            new List<DiscountDto>
            {
                new("Early bird (sold out)", null, new MoneyDto(30, "PLN"), "Early bird tickets no longer available")
            }));

        // Kazimierz tour
        await _catalogService.AddPricingPeriodAsync(kazimierzCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(60, "PLN"), null));

        // Pijalnia - free entry
        await _catalogService.AddPricingPeriodAsync(pijalniaCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(0, "PLN"), null));

        // Wieliczka
        await _catalogService.AddPricingPeriodAsync(wieliczkaCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30),
            new MoneyDto(89, "PLN"),
            new List<DiscountDto>
            {
                new("Student discount", 30, null, "Valid student ID"),
                new("Family pack (2+2)", null, new MoneyDto(50, "PLN"), "For families of 4")
            }));

        await _catalogService.AddPricingPeriodAsync(wieliczkaCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 5, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(109, "PLN"),
            new List<DiscountDto>
            {
                new("Student discount", 25, null, "Valid student ID")
            }));

        // ============================================================
        // 5. AVAILABILITY (ticket pools)
        // ============================================================

        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(wawelStateRoomsCatalog.Id, stateRoomsVariantId, 30));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(wawelArmouryCatalog.Id, armouryVariantId, 50));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(dragonsDenCatalog.Id, dragonsDenVariantId, 200));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(stMarysCatalog.Id, altarVariantId, 40));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(sukienniceCatalog.Id, null, 100));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(tauronConcertCatalog.Id, null, 18000));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(kazimierzCatalog.Id, kazimierzTourVariantId, 12));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(pijalniaCatalog.Id, null, 999));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(wieliczkaCatalog.Id, wieliczkaTourVariantId, 70));

        // ============================================================
        // 6. ATTRACTION RELATIONS
        // ============================================================

        // Wawel State Rooms variant suggests Armoury variant
        await _relationService.CreateAsync(new CreateRelationDto(
            stateRoomsVariantId, armouryVariantId,
            "Suggests",
            "same_location",
            "Combine Wawel State Rooms with Armoury for a full Wawel experience"));

        // Armoury variant suggests Dragon's Den
        await _relationService.CreateAsync(new CreateRelationDto(
            armouryVariantId, dragonsDenVariantId,
            "Suggests",
            "same_location",
            "Dragon's Den is right below the castle - easy to visit after Armoury"));

        // Wieliczka EXCLUDES Tauron concert (both take a full day)
        await _relationService.CreateAsync(new CreateRelationDto(
            wieliczka.Id, tauron.Id,
            "Excludes",
            "time_conflict",
            "Wieliczka Salt Mine is a full-day trip - cannot attend the Tauron concert the same day"));

        // Kazimierz tour suggests Pijalnia (nearby, good combo)
        await _relationService.CreateAsync(new CreateRelationDto(
            kazimierz.Id, pijalnia.Id,
            "Suggests",
            "nearby",
            "Pijalnia Wódki i Piwa is in Kazimierz - perfect after the walking tour"));

        // St. Mary's suggests Sukiennice (both on Main Market Square)
        await _relationService.CreateAsync(new CreateRelationDto(
            stMarys.Id, sukiennice.Id,
            "Suggests",
            "same_location",
            "Both on the Main Market Square - visit together"));

        Console.WriteLine("=== DATA SEEDER ===");
        Console.WriteLine($"Created 8 attraction definitions with variants");
        Console.WriteLine($"Created 1 attraction package");
        Console.WriteLine($"Created 9 catalog entries with pricing");
        Console.WriteLine($"Created 9 ticket pools");
        Console.WriteLine($"Created 5 attraction relations");
        Console.WriteLine("===================");
        Console.WriteLine();
        Console.WriteLine("Key IDs for testing:");
        Console.WriteLine($"  Wawel State Rooms (catalog): {wawelStateRoomsCatalog.Id}");
        Console.WriteLine($"  Wawel Armoury (catalog):     {wawelArmouryCatalog.Id}");
        Console.WriteLine($"  Dragon's Den (catalog):      {dragonsDenCatalog.Id}");
        Console.WriteLine($"  St. Mary's (catalog):        {stMarysCatalog.Id}");
        Console.WriteLine($"  Sukiennice (catalog):        {sukienniceCatalog.Id}");
        Console.WriteLine($"  Tauron concert (catalog):    {tauronConcertCatalog.Id}");
        Console.WriteLine($"  Kazimierz tour (catalog):    {kazimierzCatalog.Id}");
        Console.WriteLine($"  Pijalnia (catalog):          {pijalniaCatalog.Id}");
        Console.WriteLine($"  Wieliczka (catalog):         {wieliczkaCatalog.Id}");
        Console.WriteLine("===================");
    }
}
