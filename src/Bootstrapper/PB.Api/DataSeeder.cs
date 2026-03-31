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
        var catalogFrom = new DateOnly(2026, 4, 1);
        var catalogTo   = new DateOnly(2026, 6, 30);

        // ================================================================
        // 1. ATTRACTION DEFINITIONS
        //
        // Zasada podzialu:
        //   - Osobna definicja = odrebne doswiadczenie (inny temat, inne
        //     miejsce wejscia, osobny bilet)
        //   - Wariant = to samo doswiadczenie, inny format/jezyk/trudnosc
        //
        // Przyklady wariantow ponizej:
        //   St. Mary's:  Altar Visit  vs  Tower Climb
        //   Kazimierz:   Standard Group Tour  vs  Private Tour
        //   Wieliczka:   Tourist Route  vs  Miner's Route
        // ================================================================

        // ----------------------------------------------------------------
        // Wawel Castle - TRZY OSOBNE DEFINICJE (nie warianty)
        // Kazda to inny temat, inne wejscie, osobny bilet.
        // Laczone w pakiet "Wawel Full Experience" z PickN(2).
        // ----------------------------------------------------------------

        var wawelStateRooms = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Wawel Castle - State Rooms",
            "Tour of the Royal State Rooms with Renaissance furnishings and Flemish tapestries. The heart of the Polish royal residence.",
            new List<DefTagDto>
            {
                new("castle", "type"), new("guided-tour", "type"),
                new("history", "category"), new("renaissance", "style"),
                new("landmark", "category"), new("krakow", "loc"), new("unesco", "category")
            },
            new LocationDto("Kraków", "Wawel 5, 31-001 Kraków", 50.0540, 19.9354),
            new OpeningHoursDto(new TimeOnly(9, 30), new TimeOnly(17, 0)),
            new SeasonalAvailabilityDto(true, true, true, false)));

        var wawelArmoury = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Wawel Castle - Armoury",
            "Collection of historical weapons, armour, and military artifacts from the 15th-18th century.",
            new List<DefTagDto>
            {
                new("castle", "type"), new("museum", "type"),
                new("history", "category"), new("military", "category"),
                new("krakow", "loc"), new("unesco", "category")
            },
            new LocationDto("Kraków", "Wawel 5, 31-001 Kraków", 50.0540, 19.9354),
            new OpeningHoursDto(new TimeOnly(9, 30), new TimeOnly(17, 0)),
            new SeasonalAvailabilityDto(true, true, true, false)));

        var wawelDragonsDen = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Wawel Castle - Dragon's Den",
            "The legendary Dragon's Den cave beneath Wawel Hill. A short, self-guided walk through the cave ending at a fire-breathing dragon statue.",
            new List<DefTagDto>
            {
                new("cave", "type"), new("legend", "category"),
                new("family-friendly", "audience"), new("krakow", "loc")
            },
            new LocationDto("Kraków", "Wawel 5 (cave entrance)", 50.0533, 19.9344),
            new OpeningHoursDto(new TimeOnly(10, 0), new TimeOnly(17, 0)),
            new SeasonalAvailabilityDto(true, true, true, false)));

        // ----------------------------------------------------------------
        // St. Mary's Basilica - z 2 wariantami
        //
        // PRZYKLAD WARIANTU: ta sama bazylika, dwa odrebne doswiadczenia:
        //   - Altar Visit: zwiedzanie wnetrza i oltarza Stwosza (max 20 osob)
        //   - Tower Climb: wspinaczka na wieze dzwonnicza (max 5 osob - wasne krety schody)
        // Inne constrainty, inny czas, ten sam budynek -> wariant, nie osobna definicja.
        // ----------------------------------------------------------------

        var stMarys = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "St. Mary's Basilica",
            "Gothic church on the Main Market Square, famous for the Veit Stoss altarpiece and the hourly trumpet signal.",
            new List<DefTagDto>
            {
                new("church", "type"), new("gothic", "style"),
                new("history", "category"), new("art", "category"), new("krakow", "loc")
            },
            new LocationDto("Kraków", "Plac Mariacki 5, 31-042 Kraków", 50.0616, 19.9394),
            new OpeningHoursDto(new TimeOnly(11, 30), new TimeOnly(18, 0)),
            new SeasonalAvailabilityDto(true, true, true, true)));

        // Wariant 1: Altar Visit
        var stMarysV1 = await _definitionService.AddVariantAsync(stMarys.Id, new AddVariantDto(
            "Altar Visit",
            "Close-up viewing of the Veit Stoss altarpiece - the largest Gothic polyptych in the world. Guided or self-guided.",
            new List<DefTagDto> { new("indoor", "type") },
            new List<DefConstraintDto> { new("Max", "group_size", null, 20, null) },
            45));
        var altarVariantId = stMarysV1.Variants[0].Id;

        // Wariant 2: Tower Climb - te same wejscie bazyliki, ale wiezyczka
        // Wasne, krete schody -> max 5 osob jednoczesnie
        var stMarysV2 = await _definitionService.AddVariantAsync(stMarys.Id, new AddVariantDto(
            "Tower Climb",
            "Climb 239 steps of the north bell tower for panoramic views over the Main Market Square and Kraków skyline.",
            new List<DefTagDto> { new("viewpoint", "category"), new("outdoor", "type") },
            new List<DefConstraintDto> { new("Max", "group_size", null, 5, null) },
            30));
        var towerVariantId = stMarysV2.Variants[1].Id;

        // ----------------------------------------------------------------
        // Sukiennice - proste, bez wariantow
        // ----------------------------------------------------------------

        var sukiennice = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Sukiennice - Cloth Hall",
            "Renaissance trading hall in the center of Main Market Square. Gallery of 19th-Century Polish Art upstairs, market stalls below.",
            new List<DefTagDto>
            {
                new("market", "type"), new("museum", "type"),
                new("renaissance", "style"), new("shopping", "category"), new("krakow", "loc")
            },
            new LocationDto("Kraków", "Rynek Główny 1/3, 31-042 Kraków", 50.0614, 19.9372),
            new OpeningHoursDto(new TimeOnly(10, 0), new TimeOnly(18, 0)),
            new SeasonalAvailabilityDto(true, true, true, true)));

        // ----------------------------------------------------------------
        // Tauron Arena Concert - event jednorazowy
        // ----------------------------------------------------------------

        var tauron = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Concert at Tauron Arena",
            "Live music event at Tauron Arena Kraków, the largest entertainment venue in southern Poland.",
            new List<DefTagDto>
            {
                new("concert", "type"), new("music", "category"),
                new("event", "category"), new("krakow", "loc")
            },
            new LocationDto("Kraków", "ul. Stanisława Lema 7, 31-571 Kraków", 50.0694, 20.0106),
            null,
            new SeasonalAvailabilityDto(true, true, true, true)));

        // ----------------------------------------------------------------
        // Kazimierz District Walking Tour - z 2 wariantami
        //
        // PRZYKLAD WARIANTU: ta sama trasa przez Kazimierz, ale inny format:
        //   - Standard Group Tour: grupa 2-12 osob, wybor jezyka (OneOf), 150 min
        //   - Private Tour: 1-6 osob, przewodnik mowi twoim jezykiem,
        //     bez constraintu OneOf (elastyczny jezyk), inny cennik
        // Ten sam Kazimierz, inny sposob zwiedzania -> wariant.
        // ----------------------------------------------------------------

        var kazimierz = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Kazimierz District Walking Tour",
            "Walking tour through the historic Jewish quarter of Kraków. Synagogues, street art, cafes and vibrant atmosphere.",
            new List<DefTagDto>
            {
                new("walking-tour", "type"), new("history", "category"),
                new("culture", "category"), new("jewish-heritage", "category"), new("krakow", "loc")
            },
            new LocationDto("Kraków", "ul. Szeroka 24, 31-053 Kraków", 50.0512, 19.9459),
            new OpeningHoursDto(new TimeOnly(10, 0), new TimeOnly(16, 0)),
            new SeasonalAvailabilityDto(true, true, true, true)));

        // Wariant 1: Standard Group Tour
        var kazV1 = await _definitionService.AddVariantAsync(kazimierz.Id, new AddVariantDto(
            "Standard Group Tour",
            "2.5 hour guided walking tour of the main Kazimierz landmarks. Joined group, choose your language.",
            new List<DefTagDto> { new("guided-tour", "type") },
            new List<DefConstraintDto>
            {
                new("Range", "group_size", 2, 12, null),
                new("OneOf", "language", null, null, new List<string> { "polish", "english" })
            },
            150));
        var kazStandardVariantId = kazV1.Variants[0].Id;

        // Wariant 2: Private Tour - ten sam szlak, ale prywatnie
        // Brak OneOf language (przewodnik mowi twoim jezykiem), mniejsza grupa
        var kazV2 = await _definitionService.AddVariantAsync(kazimierz.Id, new AddVariantDto(
            "Private Tour",
            "Same 2.5 hour route, but exclusively for your group. Guide speaks your language - no language selection needed.",
            new List<DefTagDto> { new("guided-tour", "type"), new("private", "type") },
            new List<DefConstraintDto>
            {
                new("Range", "group_size", 1, 6, null)
            },
            150));
        var kazPrivateVariantId = kazV2.Variants[1].Id;

        // ----------------------------------------------------------------
        // Pijalnia Wodki i Piwa - proste, bez wariantow
        // ----------------------------------------------------------------

        var pijalnia = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Pijalnia Wódki i Piwa",
            "Popular bar chain in Kraków serving cheap shots of vodka and beers. Classic Kraków nightlife.",
            new List<DefTagDto>
            {
                new("bar", "type"), new("nightlife", "category"),
                new("beer", "category"), new("vodka", "category"), new("krakow", "loc")
            },
            new LocationDto("Kraków", "ul. Mikołajska 5, 31-027 Kraków", 50.0608, 19.9410),
            new OpeningHoursDto(new TimeOnly(12, 0), new TimeOnly(23, 59)),
            new SeasonalAvailabilityDto(true, true, true, true)));

        // ----------------------------------------------------------------
        // Wieliczka Salt Mine - z 2 wariantami
        //
        // PRZYKLAD WARIANTU: ta sama kopalnia, dwa poziomy trudnosci trasy:
        //   - Tourist Route: standardowa (3.5km, 180min, do 35 osob, dla wszystkich)
        //   - Miner's Route: wymagajaca (wassze przejscia, do 10 osob, fit adults only,
        //     booking 5 dni wczesniej)
        // Ta sama kopalnia, inny typ trasy -> wariant.
        //
        // RELACJA REQUIRES: Miner's Route wymaga Tourist Route w sesji
        // (orientacja w kopalni przed trudniejsza trasa).
        // ----------------------------------------------------------------

        var wieliczka = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Wieliczka Salt Mine",
            "UNESCO World Heritage Site. Underground salt mine with chapels, sculptures, and an underground lake - all carved from salt.",
            new List<DefTagDto>
            {
                new("mine", "type"), new("unesco", "category"),
                new("history", "category"), new("underground", "category"), new("wieliczka", "loc")
            },
            new LocationDto("Wieliczka", "ul. Daniłowicza 10, 32-020 Wieliczka", 49.9833, 20.0553),
            new OpeningHoursDto(new TimeOnly(8, 0), new TimeOnly(17, 0)),
            new SeasonalAvailabilityDto(true, true, true, true)));

        // Wariant 1: Tourist Route - standardowa, dla wszystkich
        var wieliczkaV1 = await _definitionService.AddVariantAsync(wieliczka.Id, new AddVariantDto(
            "Tourist Route",
            "Classic 3.5km underground route through 20+ chambers including the Chapel of St. Kinga. Suitable for all fitness levels.",
            new List<DefTagDto> { new("guided-tour", "type") },
            new List<DefConstraintDto>
            {
                new("Range", "group_size", 1, 35, null),
                new("RequiredDaysAhead", "booking_days_ahead", 3, null, null),
                new("OneOf", "language", null, null, new List<string> { "polish", "english", "german", "french", "italian", "spanish" })
            },
            180));
        var touristRouteVariantId = wieliczkaV1.Variants[0].Id;

        // Wariant 2: Miner's Route - wymagajaca, dla osob sprawnych fizycznie
        var wieliczkaV2 = await _definitionService.AddVariantAsync(wieliczka.Id, new AddVariantDto(
            "Miner's Route",
            "Challenging 3.5km route through narrow passages and original miner's tunnels. Requires crawling in places. Fit adults only.",
            new List<DefTagDto> { new("adventure", "category") },
            new List<DefConstraintDto>
            {
                new("Range", "group_size", 1, 10, null),
                new("RequiredDaysAhead", "booking_days_ahead", 5, null, null),
                new("OneOf", "language", null, null, new List<string> { "polish", "english" })
            },
            210));
        var minersRouteVariantId = wieliczkaV2.Variants[1].Id;

        // ----------------------------------------------------------------
        // Tatra Mountains - Morskie Oko Day Trip (NEW)
        //
        // Caly dzien wycieczki z Krakowa (ok. 3h drogi w kazda strone + 2h spacer).
        // WYKLUCZA inne pelne wycieczki (Wieliczka) i wieczorny koncert (za zmeczony).
        // Tylko wiosna/lato/jesien - zima szlak niebezpieczny.
        // ----------------------------------------------------------------

        var tatry = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Tatra Mountains - Morskie Oko Hike",
            "Full day trip from Kraków to the most famous mountain lake in Poland. 9km round-trip hike through stunning alpine scenery. Shuttle bus from Palenica Białczańska to the trailhead.",
            new List<DefTagDto>
            {
                new("hiking", "type"), new("nature", "category"),
                new("mountains", "category"), new("lake", "category"),
                new("full-day", "category"), new("zakopane", "loc")
            },
            new LocationDto("Zakopane", "Palenica Białczańska (trailhead)", 49.2320, 20.0810),
            new OpeningHoursDto(new TimeOnly(7, 0), new TimeOnly(18, 0)),
            new SeasonalAvailabilityDto(true, true, true, false)));

        // ----------------------------------------------------------------
        // Zakrzówek Reservoir (NEW)
        //
        // Darmowe kąpielisko w zatopionym kamieniolomie w Krakowie.
        // Popularne latem, wstep wolny, bez rezerwacji.
        // Dobry przyklad prostej, bezplatnej atrakcji bez constraintow.
        // ----------------------------------------------------------------

        var zakrzowek = await _definitionService.CreateAsync(new CreateAttractionDefinitionDto(
            "Zakrzówek Reservoir",
            "Former limestone quarry turned open-air swimming and snorkelling spot in Kraków. Crystal clear turquoise water. Free entry, no reservation needed.",
            new List<DefTagDto>
            {
                new("swimming", "type"), new("outdoor", "type"),
                new("lake", "category"), new("free", "category"),
                new("nature", "category"), new("krakow", "loc")
            },
            new LocationDto("Kraków", "ul. Twardowskiego, 30-213 Kraków", 50.0369, 19.9005),
            new OpeningHoursDto(new TimeOnly(8, 0), new TimeOnly(20, 0)),
            new SeasonalAvailabilityDto(false, true, false, false)));

        // ================================================================
        // 2. ATTRACTION PACKAGE
        //
        // Wawel Full Experience: wybierz 2 z 3 osobnych definicji Wawelu.
        // ComponentIds trzyma ID definicji (AttractionComponent) - zgodnie
        // z Composite Pattern. Nie warianty, nie losowe Guidy.
        // ================================================================

        var wawelPackage = await _packageService.CreateAsync(new CreatePackageDto(
            "Wawel Full Experience",
            "Pick any 2 of the 3 Wawel attractions: State Rooms, Armoury, Dragon's Den. Combine them for a full castle experience.",
            new SelectionRuleDto("PickN", 2),
            new List<Guid> { wawelStateRooms.Id, wawelArmoury.Id, wawelDragonsDen.Id }));

        // ================================================================
        // 3. CATALOG ENTRIES
        //
        // Wawel - osobne definicje, wiec variantId = null (brak wariantow)
        // Pozostale - variantId wskazuje na konkretny wariant definicji
        // ================================================================

        // --- Wawel State Rooms (osobna definicja, brak wariantu)
        var wawelStateRoomsCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            wawelStateRooms.Id, null,
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

        // --- Wawel Armoury (osobna definicja, brak wariantu)
        var wawelArmouryCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            wawelArmoury.Id, null,
            "Wawel Castle - Armoury",
            "Historical weapons and armour collection. Walk-in friendly, short queue times.",
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

        // --- Dragon's Den (osobna definicja, brak wariantu, walk-in)
        var dragonsDenCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            wawelDragonsDen.Id, null,
            "Wawel Castle - Dragon's Den",
            "Self-guided walk through the cave. No booking required - just show up.",
            new List<CatTagDto>
            {
                new("cave", "type"), new("family-friendly", "audience"),
                new("legend", "category"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "Wawel 5 (cave entrance)"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(10, 0), new TimeOnly(17, 0)),
            false, null));

        // --- St. Mary's - Altar Visit (wariant 1)
        var stMarysAltarCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
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

        // --- St. Mary's - Tower Climb (wariant 2)
        // Wasne krety schody = max 5 osob jednoczesnie
        var stMarysTowerCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            stMarys.Id, towerVariantId,
            "St. Mary's Basilica - Tower Climb",
            "Climb 239 steps for panoramic views over the Main Market Square. Narrow spiral staircase.",
            new List<CatTagDto>
            {
                new("church", "type"), new("viewpoint", "category"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "Plac Mariacki 5"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(11, 30), new TimeOnly(17, 30)),
            false,
            new List<BookingConstraintDto>
            {
                new("Max", "group_size", null, 5, null)
            }));

        // --- Sukiennice
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

        // --- Tauron Concert (event jednorazowy)
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
            null, true, null));

        // --- Kazimierz Standard Group Tour (wariant 1)
        var kazimierzStandardCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            kazimierz.Id, kazStandardVariantId,
            "Kazimierz Walking Tour - Standard Group",
            "Joined group tour of Kazimierz. Choose Polish or English.",
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

        // --- Kazimierz Private Tour (wariant 2)
        // Bez OneOf language - przewodnik mowi twoim jezykiem
        var kazimierzPrivateCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            kazimierz.Id, kazPrivateVariantId,
            "Kazimierz Walking Tour - Private",
            "Same route, exclusively for your group. Guide adapts to your language and pace. Higher price, more personal.",
            new List<CatTagDto>
            {
                new("walking-tour", "type"), new("private", "type"),
                new("history", "category"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "ul. Szeroka 24"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(10, 0), new TimeOnly(16, 0)),
            false,
            new List<BookingConstraintDto>
            {
                new("Range", "group_size", 1, 6, null)
            }));

        // --- Pijalnia
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

        // --- Wieliczka Tourist Route (wariant 1)
        var wieliczkaTouristCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            wieliczka.Id, touristRouteVariantId,
            "Wieliczka Salt Mine - Tourist Route",
            "Classic 3.5km underground tour including the Chapel of St. Kinga. Suitable for all.",
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

        // --- Wieliczka Miner's Route (wariant 2)
        // Trudniejsza, mniejsze grupy, booking 5 dni wczesniej
        // REQUIRES Tourist Route (patrz relacje ponizej)
        var wieliczkaМinersCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            wieliczka.Id, minersRouteVariantId,
            "Wieliczka Salt Mine - Miner's Route",
            "Challenging route through narrow original tunnels. Requires crawling in places. Fit adults only.",
            new List<CatTagDto>
            {
                new("mine", "type"), new("adventure", "category"),
                new("guided-tour", "type"), new("wieliczka", "loc")
            },
            new CatalogLocationDto("Wieliczka", "ul. Daniłowicza 10"),
            new DateRangeDto(catalogFrom, catalogTo),
            new CatalogOpeningHoursDto(new TimeOnly(9, 0), new TimeOnly(15, 0)),
            false,
            new List<BookingConstraintDto>
            {
                new("Range", "group_size", 1, 10, null),
                new("RequiredDaysAhead", "booking_days_ahead", 5, null, null),
                new("OneOf", "language", null, null, new List<string> { "polish", "english" })
            }));

        // --- Tatra Mountains (full day trip)
        var tatryCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            tatry.Id, null,
            "Tatra Mountains - Morskie Oko Day Trip",
            "Full day excursion from Kraków. Bus to Zakopane, shuttle to trailhead, 9km hike to the lake and back.",
            new List<CatTagDto>
            {
                new("hiking", "type"), new("nature", "category"),
                new("mountains", "category"), new("full-day", "category"), new("zakopane", "loc")
            },
            new CatalogLocationDto("Zakopane", "Palenica Białczańska (trailhead)"),
            new DateRangeDto(catalogFrom, catalogTo),
            null, false,
            new List<BookingConstraintDto>
            {
                new("Range", "group_size", 1, 20, null),
                new("RequiredDaysAhead", "booking_days_ahead", 2, null, null)
            }));

        // --- Zakrzówek (darmowe, bez rezerwacji)
        var zakrzowekCatalog = await _catalogService.CreateAsync(new CreateCatalogEntryDto(
            zakrzowek.Id, null,
            "Zakrzówek Reservoir - Open Water Swimming",
            "Turquoise quarry lake in Kraków. Free entry, no booking. Bring your own towel.",
            new List<CatTagDto>
            {
                new("swimming", "type"), new("outdoor", "type"),
                new("free", "category"), new("krakow", "loc")
            },
            new CatalogLocationDto("Kraków", "ul. Twardowskiego"),
            new DateRangeDto(new DateOnly(2026, 5, 1), new DateOnly(2026, 8, 31)),
            new CatalogOpeningHoursDto(new TimeOnly(8, 0), new TimeOnly(20, 0)),
            false, null));

        // ================================================================
        // 4. PRICING PERIODS
        // ================================================================

        // Wawel State Rooms: kwiecien tanszy, maj-czerwiec sezon
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
            new List<DiscountDto> { new("Student discount", 40, null, "Valid student ID required") }));

        // Wawel Armoury
        await _catalogService.AddPricingPeriodAsync(wawelArmouryCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(20, "PLN"),
            new List<DiscountDto> { new("Student discount", 50, null, null) }));

        // Dragon's Den - tanio, flat price
        await _catalogService.AddPricingPeriodAsync(dragonsDenCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(5, "PLN"), null));

        // St. Mary's - Altar Visit
        await _catalogService.AddPricingPeriodAsync(stMarysAltarCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(15, "PLN"), null));

        // St. Mary's - Tower Climb (tanszy niz oltarz)
        await _catalogService.AddPricingPeriodAsync(stMarysTowerCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(10, "PLN"), null));

        // Sukiennice
        await _catalogService.AddPricingPeriodAsync(sukienniceCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(12, "PLN"),
            new List<DiscountDto> { new("Free on Tuesdays", 100, null, "Permanent exhibitions free on Tuesdays") }));

        // Tauron concert
        await _catalogService.AddPricingPeriodAsync(tauronConcertCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 12), new DateOnly(2026, 4, 12),
            new MoneyDto(120, "PLN"),
            new List<DiscountDto> { new("Early bird (sold out)", null, new MoneyDto(30, "PLN"), "Early bird no longer available") }));

        // Kazimierz Standard
        await _catalogService.AddPricingPeriodAsync(kazimierzStandardCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(60, "PLN"), null));

        // Kazimierz Private (drozsze - tour prywatny)
        await _catalogService.AddPricingPeriodAsync(kazimierzPrivateCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(280, "PLN"),
            new List<DiscountDto> { new("Off-peak (Mon-Thu)", 15, null, "15% discount on weekdays") }));

        // Pijalnia - darmowe wejscie
        await _catalogService.AddPricingPeriodAsync(pijalniaCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(0, "PLN"), null));

        // Wieliczka Tourist Route
        await _catalogService.AddPricingPeriodAsync(wieliczkaTouristCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30),
            new MoneyDto(89, "PLN"),
            new List<DiscountDto>
            {
                new("Student discount", 30, null, "Valid student ID"),
                new("Family pack (2+2)", null, new MoneyDto(50, "PLN"), "For families of 4")
            }));
        await _catalogService.AddPricingPeriodAsync(wieliczkaTouristCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 5, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(109, "PLN"),
            new List<DiscountDto> { new("Student discount", 25, null, "Valid student ID") }));

        // Wieliczka Miner's Route (drozsze - specjalistyczna)
        await _catalogService.AddPricingPeriodAsync(wieliczkaМinersCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(149, "PLN"), null));

        // Tatra Mountains
        await _catalogService.AddPricingPeriodAsync(tatryCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30),
            new MoneyDto(75, "PLN"),
            new List<DiscountDto> { new("Student discount", 20, null, null) }));
        await _catalogService.AddPricingPeriodAsync(tatryCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 5, 1), new DateOnly(2026, 6, 30),
            new MoneyDto(90, "PLN"),
            new List<DiscountDto> { new("Student discount", 20, null, null) }));

        // Zakrzowek - darmowe
        await _catalogService.AddPricingPeriodAsync(zakrzowekCatalog.Id, new AddPricingPeriodDto(
            new DateOnly(2026, 5, 1), new DateOnly(2026, 8, 31),
            new MoneyDto(0, "PLN"), null));

        // ================================================================
        // 5. AVAILABILITY (ticket pools)
        // ================================================================

        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(wawelStateRoomsCatalog.Id, null, 30));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(wawelArmouryCatalog.Id, null, 50));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(dragonsDenCatalog.Id, null, 200));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(stMarysAltarCatalog.Id, altarVariantId, 40));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(stMarysTowerCatalog.Id, towerVariantId, 15));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(sukienniceCatalog.Id, null, 100));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(tauronConcertCatalog.Id, null, 18000));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(kazimierzStandardCatalog.Id, kazStandardVariantId, 12));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(kazimierzPrivateCatalog.Id, kazPrivateVariantId, 5));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(pijalniaCatalog.Id, null, 999));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(wieliczkaTouristCatalog.Id, touristRouteVariantId, 70));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(wieliczkaМinersCatalog.Id, minersRouteVariantId, 20));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(tatryCatalog.Id, null, 40));
        await _availabilityService.CreatePoolAsync(new CreateTicketPoolDto(zakrzowekCatalog.Id, null, 999));

        // ================================================================
        // 6. ATTRACTION RELATIONS
        //
        // Uzywamy ID definicji gdy relacja dotyczy calej atrakcji
        // Uzywamy ID wariantu gdy relacja dotyczy konkretnego wariantu
        //
        // SUGGESTS:  A -> sugeruje -> B (soft, wypelnia OptionalSuggestions)
        // EXCLUDES:  A -> wyklucza -> B (soft issue Conflict w sesji)
        // REQUIRES:  A -> wymaga -> B   (soft issue RequirementMissing w sesji)
        // ================================================================

        // -- SUGGESTS: Wawel State Rooms sugeruje Armoury i Dragon's Den (to samo miejsce)
        await _relationService.CreateAsync(new CreateRelationDto(
            wawelStateRooms.Id, wawelArmoury.Id, "Suggests", "same_location",
            "Both in Wawel Castle - natural to combine State Rooms with Armoury"));
        await _relationService.CreateAsync(new CreateRelationDto(
            wawelStateRooms.Id, wawelDragonsDen.Id, "Suggests", "same_location",
            "Dragon's Den is right below the castle - easy add-on after State Rooms"));
        await _relationService.CreateAsync(new CreateRelationDto(
            wawelArmoury.Id, wawelDragonsDen.Id, "Suggests", "same_location",
            "Dragon's Den is right below the Armoury - 5 minutes walk"));

        // -- SUGGESTS: St. Mary's sugeruje Sukiennice (oba na Rynku Glownym)
        await _relationService.CreateAsync(new CreateRelationDto(
            stMarys.Id, sukiennice.Id, "Suggests", "same_location",
            "Both on the Main Market Square - visit together in one go"));

        // -- SUGGESTS: Kazimierz sugeruje Pijalnie (blisko, dobry duet)
        await _relationService.CreateAsync(new CreateRelationDto(
            kazimierz.Id, pijalnia.Id, "Suggests", "nearby",
            "Pijalnia Wódki i Piwa is in Kazimierz - perfect ending after the walking tour"));

        // -- SUGGESTS: Tatra Mountains sugeruje Pijalnie (zimne piwko po gorach!)
        await _relationService.CreateAsync(new CreateRelationDto(
            tatry.Id, pijalnia.Id, "Suggests", "reward",
            "Cold beer at Pijalnia is a perfect reward after a full day in the mountains"));

        // -- SUGGESTS: Zakrzowek sugeruje Pijalnie (kąpiel -> piwko)
        await _relationService.CreateAsync(new CreateRelationDto(
            zakrzowek.Id, pijalnia.Id, "Suggests", "nearby",
            "Swimming at Zakrzówek pairs perfectly with an evening drink at Pijalnia"));

        // -- EXCLUDES: Wieliczka wyklucza Tauron Concert
        // Kopalnia to ok. 4h + dojazd -> wraca sie zmeczony, nie nadaje sie na wieczorny koncert
        await _relationService.CreateAsync(new CreateRelationDto(
            wieliczka.Id, tauron.Id, "Excludes", "time_conflict",
            "Wieliczka takes 4+ hours including travel - too exhausting to attend an evening concert the same day"));

        // -- EXCLUDES: Tatra Mountains wyklucza Wieliczke i Tauron
        // Caly dzien wycieczki -> nie ma czasu/sily na inne pelne atrakcje tego samego dnia
        await _relationService.CreateAsync(new CreateRelationDto(
            tatry.Id, wieliczka.Id, "Excludes", "time_conflict",
            "Tatra Mountains is a full-day trip from Kraków - no time for Wieliczka the same day"));
        await _relationService.CreateAsync(new CreateRelationDto(
            tatry.Id, tauron.Id, "Excludes", "exhaustion",
            "After a full mountain day you will be too tired for an evening concert"));
        await _relationService.CreateAsync(new CreateRelationDto(
            tatry.Id, wawelStateRooms.Id, "Excludes", "exhaustion",
            "After a full mountain day, guided castle tours are too much - save Wawel for another day"));

        // -- REQUIRES: Wieliczka Miner's Route wymaga Tourist Route
        // Trudniejsza trasa wymaga znajomosci kopalni z podstawowej trasy
        // (albo: obie sa planowane na ten sam wyjazd do Wieliczki)
        await _relationService.CreateAsync(new CreateRelationDto(
            minersRouteVariantId, touristRouteVariantId, "Requires", "prerequisite",
            "Miner's Route requires prior orientation in the mine - plan Tourist Route in the same trip"));

        // ================================================================
        // SUMMARY
        // ================================================================

        Console.WriteLine("=== DATA SEEDER ===");
        Console.WriteLine($"Definitions:  11 (w tym 3 osobne definicje Wawelu, 2 nowe: Tatry, Zakrzowek)");
        Console.WriteLine($"Packages:     1  (Wawel Full Experience - PickN(2) z 3 definicji)");
        Console.WriteLine($"Catalog:      14 entries (w tym warianty: St.Mary's x2, Kazimierz x2, Wieliczka x2)");
        Console.WriteLine($"Pools:        14");
        Console.WriteLine($"Relations:    11 (3x Suggests Wawel, 2x Suggests inne, 2x Suggests +piwo/gory, 4x Excludes, 1x Requires)");
        Console.WriteLine("===================");
        Console.WriteLine();
        Console.WriteLine("Key catalog IDs for testing:");
        Console.WriteLine($"  Wawel State Rooms:          {wawelStateRoomsCatalog.Id}");
        Console.WriteLine($"  Wawel Armoury:              {wawelArmouryCatalog.Id}");
        Console.WriteLine($"  Wawel Dragon's Den:         {dragonsDenCatalog.Id}");
        Console.WriteLine($"  St. Mary's - Altar Visit:   {stMarysAltarCatalog.Id}");
        Console.WriteLine($"  St. Mary's - Tower Climb:   {stMarysTowerCatalog.Id}    [wariant, max 5 osob]");
        Console.WriteLine($"  Sukiennice:                 {sukienniceCatalog.Id}");
        Console.WriteLine($"  Tauron Concert:             {tauronConcertCatalog.Id}");
        Console.WriteLine($"  Kazimierz Standard:         {kazimierzStandardCatalog.Id}");
        Console.WriteLine($"  Kazimierz Private:          {kazimierzPrivateCatalog.Id}  [wariant, grupa 1-6]");
        Console.WriteLine($"  Pijalnia:                   {pijalniaCatalog.Id}");
        Console.WriteLine($"  Wieliczka Tourist Route:    {wieliczkaTouristCatalog.Id}");
        Console.WriteLine($"  Wieliczka Miner's Route:    {wieliczkaМinersCatalog.Id}   [wariant, wymaga Tourist Route]");
        Console.WriteLine($"  Tatra Mountains:            {tatryCatalog.Id}             [wyklucza Wieliczke i Tauron]");
        Console.WriteLine($"  Zakrzowek:                  {zakrzowekCatalog.Id}         [darmowe, sezon letni]");
        Console.WriteLine("===================");
    }
}
