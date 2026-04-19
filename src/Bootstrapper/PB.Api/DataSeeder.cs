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
        var cityFrom = new DateOnly(2026, 4, 1);
        var cityTo = new DateOnly(2026, 6, 30);
        
        var opnFrom = new DateOnly(2026, 4, 20);
        var opnTo = new DateOnly(2026, 10, 31);

        var attractionSeeds = new[]
        {
            new AttractionSeed("wawel-state-rooms", "Wawel Castle - State Rooms",
                "Tour of the Royal State Rooms with Renaissance furnishings and Flemish tapestries. The heart of the Polish royal residence.",
                [Tag("castle", "type"), Tag("guided-tour", "type"), Tag("history", "category"), Tag("renaissance", "style"), Tag("landmark", "category"), Tag("krakow", "loc"), Tag("unesco", "category")],
                new Location("Kraków", "Wawel 5, 31-001 Kraków", 50.0540, 19.9354),
                Hours(9, 30, 17, 0)),
            new AttractionSeed("wawel-armoury", "Wawel Castle - Armoury",
                "Collection of historical weapons, armour, and military artifacts from the 15th-18th century.",
                [Tag("castle", "type"), Tag("museum", "type"), Tag("history", "category"), Tag("military", "category"), Tag("krakow", "loc"), Tag("unesco", "category")],
                new Location("Kraków", "Wawel 5, 31-001 Kraków", 50.0540, 19.9354),
                Hours(9, 30, 17, 0)),
            new AttractionSeed("wawel-dragons-den", "Wawel Castle - Dragon's Den",
                "The legendary Dragon's Den cave beneath Wawel Hill. A short, self-guided walk through the cave ending at a fire-breathing dragon statue.",
                [Tag("cave", "type"), Tag("legend", "category"), Tag("family-friendly", "audience"), Tag("krakow", "loc")],
                new Location("Kraków", "Wawel 5 (cave entrance)", 50.0533, 19.9344),
                Hours(10, 0, 17, 0)),
            new AttractionSeed("st-marys-altar", "St. Mary's Basilica - Altar Visit",
                "Close-up viewing of the Veit Stoss altarpiece inside St. Mary's Basilica.",
                [Tag("church", "type"), Tag("art", "category"), Tag("gothic", "style"), Tag("krakow", "loc"), Tag("indoor", "type")],
                new Location("Kraków", "Plac Mariacki 5, 31-042 Kraków", 50.0616, 19.9394),
                Hours(11, 30, 18, 0)),
            new AttractionSeed("st-marys-tower", "St. Mary's Basilica - Tower Climb",
                "Climb 239 steps of the north bell tower for panoramic views over the Main Market Square and Kraków skyline.",
                [Tag("church", "type"), Tag("viewpoint", "category"), Tag("outdoor", "type"), Tag("krakow", "loc")],
                new Location("Kraków", "Plac Mariacki 5, 31-042 Kraków", 50.0616, 19.9394),
                Hours(11, 30, 17, 30)),
            new AttractionSeed("sukiennice", "Sukiennice - Cloth Hall",
                "Renaissance trading hall in the center of Main Market Square. Gallery of 19th-Century Polish Art upstairs, market stalls below.",
                [Tag("market", "type"), Tag("museum", "type"), Tag("renaissance", "style"), Tag("shopping", "category"), Tag("krakow", "loc")],
                new Location("Kraków", "Rynek Główny 1/3, 31-042 Kraków", 50.0614, 19.9372),
                Hours(10, 0, 18, 0)),
            new AttractionSeed("tauron-concert", "Concert at Tauron Arena",
                "Live music event at Tauron Arena Kraków, the largest entertainment venue in southern Poland.",
                [Tag("concert", "type"), Tag("music", "category"), Tag("event", "category"), Tag("krakow", "loc")],
                new Location("Kraków", "ul. Stanisława Lema 7, 31-571 Kraków", 50.0694, 20.0106),
                null),
            new AttractionSeed("kazimierz-group", "Kazimierz Walking Tour - Standard Group",
                "Joined group tour of Kazimierz landmarks. Language chosen at booking time.",
                [Tag("walking-tour", "type"), Tag("guided-tour", "type"), Tag("history", "category"), Tag("culture", "category"), Tag("jewish-heritage", "category"), Tag("krakow", "loc")],
                new Location("Kraków", "ul. Szeroka 24, 31-053 Kraków", 50.0512, 19.9459),
                Hours(10, 0, 16, 0)),
            new AttractionSeed("kazimierz-private", "Kazimierz Walking Tour - Private",
                "Private version of the Kazimierz route for your group only.",
                [Tag("walking-tour", "type"), Tag("guided-tour", "type"), Tag("private", "type"), Tag("history", "category"), Tag("culture", "category"), Tag("krakow", "loc")],
                new Location("Kraków", "ul. Szeroka 24, 31-053 Kraków", 50.0512, 19.9459),
                Hours(10, 0, 16, 0)),
            new AttractionSeed("pijalnia", "Pijalnia Wódki i Piwa",
                "Popular bar chain in Kraków serving cheap shots of vodka and beers. Classic Kraków nightlife.",
                [Tag("bar", "type"), Tag("nightlife", "category"), Tag("beer", "category"), Tag("vodka", "category"), Tag("krakow", "loc")],
                new Location("Kraków", "ul. Mikołajska 5, 31-027 Kraków", 50.0608, 19.9410),
                Hours(12, 0, 23, 59)),
            new AttractionSeed("wieliczka-tourist", "Wieliczka Salt Mine - Tourist Route",
                "Classic 3.5km underground route through 20+ chambers including the Chapel of St. Kinga. Suitable for all fitness levels.",
                [Tag("mine", "type"), Tag("unesco", "category"), Tag("guided-tour", "type"), Tag("history", "category"), Tag("wieliczka", "loc")],
                new Location("Wieliczka", "ul. Daniłowicza 10, 32-020 Wieliczka", 49.9833, 20.0553),
                Hours(8, 0, 17, 0)),
            new AttractionSeed("wieliczka-miner", "Wieliczka Salt Mine - Miner's Route",
                "Challenging underground route through narrow original tunnels. Fit adults only.",
                [Tag("mine", "type"), Tag("adventure", "category"), Tag("guided-tour", "type"), Tag("wieliczka", "loc")],
                new Location("Wieliczka", "ul. Daniłowicza 10, 32-020 Wieliczka", 49.9833, 20.0553),
                Hours(9, 0, 15, 0)),
            new AttractionSeed("tatry", "Tatra Mountains - Morskie Oko Hike",
                "Full day trip from Kraków to the most famous mountain lake in Poland. 9km round-trip hike through alpine scenery.",
                [Tag("hiking", "type"), Tag("nature", "category"), Tag("mountains", "category"), Tag("lake", "category"), Tag("full-day", "category"), Tag("zakopane", "loc")],
                new Location("Zakopane", "Palenica Białczańska (trailhead)", 49.2320, 20.0810),
                Hours(7, 0, 18, 0)),
            new AttractionSeed("zakrzowek", "Zakrzówek Reservoir",
                "Former limestone quarry turned open-air swimming and snorkelling spot in Kraków. Free entry, no reservation needed.",
                [Tag("swimming", "type"), Tag("outdoor", "type"), Tag("lake", "category"), Tag("free", "category"), Tag("nature", "category"), Tag("krakow", "loc")],
                new Location("Kraków", "ul. Twardowskiego, 30-213 Kraków", 50.0369, 19.9005),
                Hours(8, 0, 20, 0)),
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
            foreach (var tag in seed.Tags)
                attraction.AddTag(tag);
            attraction.SetLocation(seed.Location);
            
            if (seed.OpeningHours != null) 
                attraction.SetOpeningHours(seed.OpeningHours);
                
            componentsByKey[seed.Key] = attraction;
        }

        var packageSeeds = new[]
        {
            new PackageSeed(
                "wawel-full",
                "Wawel Full Experience",
                "Pick any 2 of the 3 Wawel attractions: State Rooms, Armoury, Dragon's Den.",
                SelectionRule.PickN(2),
                ["wawel-state-rooms", "wawel-armoury", "wawel-dragons-den"]),
            new PackageSeed(
                "opn-caves-explorer",
                "Ojców Explorer",
                "A set for active visitors: entry to Jaskinia Łokietka and Jaskinia Ciemna.",
                SelectionRule.PickN(2), ["opn-jaskinia-lokietka", "opn-jaskinia-ciemna"])
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
            new CatalogSeed("wawel-state-rooms", "Wawel Castle - State Rooms Tour", "Daily guided tours of the Royal State Rooms. Available April-June 2026.",
                [Tag("castle", "type"), Tag("guided-tour", "type"), Tag("history", "category"), Tag("krakow", "loc")],
                "Kraków", "Wawel 5", cityFrom, cityTo, new CatalogOpeningHours(new TimeOnly(9, 30), new TimeOnly(17, 0)), false,
                [Constraint("Range", "group_size", 1, 15), Constraint("RequiredDaysAhead", "booking_days_ahead", 2, null), Constraint("OneOf", "language", null, null, "polish", "english", "german")],
                [Pricing(cityFrom, cityTo, 70)], 30),
            new CatalogSeed("wawel-armoury", "Wawel Castle - Armoury", "Historical weapons and armour collection. Walk-in friendly, short queue times.",
                [Tag("castle", "type"), Tag("museum", "type"), Tag("history", "category"), Tag("krakow", "loc")],
                "Kraków", "Wawel 5", cityFrom, cityTo, new CatalogOpeningHours(new TimeOnly(9, 30), new TimeOnly(17, 0)), false,
                [Constraint("Range", "group_size", 1, 25), Constraint("RequiredDaysAhead", "booking_days_ahead", 1, null)],
                [Pricing(cityFrom, cityTo, 45)], 50),
            new CatalogSeed("wawel-dragons-den", "Wawel Castle - Dragon's Den", "Self-guided walk through the cave. No booking required - just show up.",
                [Tag("cave", "type"), Tag("family-friendly", "audience"), Tag("legend", "category"), Tag("krakow", "loc")],
                "Kraków", "Wawel 5 (cave entrance)", cityFrom, cityTo, new CatalogOpeningHours(new TimeOnly(10, 0), new TimeOnly(17, 0)), false,
                [], [Pricing(cityFrom, cityTo, 20)], 200),
            new CatalogSeed("st-marys-altar", "St. Mary's Basilica - Altar Visit", "See the world-famous Veit Stoss altarpiece up close.",
                [Tag("church", "type"), Tag("art", "category"), Tag("gothic", "style"), Tag("krakow", "loc")],
                "Kraków", "Plac Mariacki 5", cityFrom, cityTo, new CatalogOpeningHours(new TimeOnly(11, 30), new TimeOnly(18, 0)), false,
                [Constraint("Max", "group_size", null, 20)], [Pricing(cityFrom, cityTo, 35)], 40),
            new CatalogSeed("st-marys-tower", "St. Mary's Basilica - Tower Climb", "Climb 239 steps for panoramic views over the Main Market Square. Narrow spiral staircase.",
                [Tag("church", "type"), Tag("viewpoint", "category"), Tag("krakow", "loc")],
                "Kraków", "Plac Mariacki 5", cityFrom, cityTo, new CatalogOpeningHours(new TimeOnly(11, 30), new TimeOnly(17, 30)), false,
                [Constraint("Max", "group_size", null, 5)], [Pricing(cityFrom, cityTo, 40)], 15),
            new CatalogSeed("sukiennice", "Sukiennice - Cloth Hall & Gallery", "Visit the Cloth Hall market and the Gallery of 19th-Century Polish Art.",
                [Tag("market", "type"), Tag("museum", "type"), Tag("shopping", "category"), Tag("krakow", "loc")],
                "Kraków", "Rynek Główny 1/3", cityFrom, cityTo, new CatalogOpeningHours(new TimeOnly(10, 0), new TimeOnly(18, 0)), false,
                [], [Pricing(cityFrom, cityTo, 30)], 100),
            new CatalogSeed("tauron-concert", "Rock Festival at Tauron Arena", "Evening rock concert at Tauron Arena Kraków. Gates open at 18:00.",
                [Tag("concert", "type"), Tag("music", "category"), Tag("event", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Stanisława Lema 7", new DateOnly(2026, 4, 12), new DateOnly(2026, 4, 12), null, true,
                [], [Pricing(new DateOnly(2026, 4, 12), new DateOnly(2026, 4, 12), 180)], 18000),
            new CatalogSeed("kazimierz-group", "Kazimierz Walking Tour - Standard Group", "Joined group tour of Kazimierz. Choose Polish or English.",
                [Tag("walking-tour", "type"), Tag("history", "category"), Tag("culture", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Szeroka 24", cityFrom, cityTo, new CatalogOpeningHours(new TimeOnly(10, 0), new TimeOnly(16, 0)), false,
                [Constraint("Range", "group_size", 2, 12), Constraint("OneOf", "language", null, null, "polish", "english")],
                [Pricing(cityFrom, cityTo, 65)], 12),
            new CatalogSeed("kazimierz-private", "Kazimierz Walking Tour - Private", "Same route, exclusively for your group. Guide adapts to your language and pace.",
                [Tag("walking-tour", "type"), Tag("private", "type"), Tag("history", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Szeroka 24", cityFrom, cityTo, new CatalogOpeningHours(new TimeOnly(10, 0), new TimeOnly(16, 0)), false,
                [Constraint("Range", "group_size", 1, 6)],
                [Pricing(cityFrom, cityTo, 220)], 5),
            new CatalogSeed("pijalnia", "Pijalnia Wódki i Piwa - Evening Out", "Classic Kraków bar experience. No reservation needed.",
                [Tag("bar", "type"), Tag("nightlife", "category"), Tag("beer", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Mikołajska 5", cityFrom, cityTo, new CatalogOpeningHours(new TimeOnly(12, 0), new TimeOnly(23, 59)), false,
                [], [Pricing(cityFrom, cityTo, 0)], 999),
            new CatalogSeed("wieliczka-tourist", "Wieliczka Salt Mine - Tourist Route", "Classic 3.5km underground tour including the Chapel of St. Kinga. Suitable for all.",
                [Tag("mine", "type"), Tag("unesco", "category"), Tag("guided-tour", "type"), Tag("wieliczka", "loc")],
                "Wieliczka", "ul. Daniłowicza 10", cityFrom, cityTo, new CatalogOpeningHours(new TimeOnly(8, 0), new TimeOnly(17, 0)), false,
                [Constraint("Range", "group_size", 1, 35), Constraint("RequiredDaysAhead", "booking_days_ahead", 3, null), Constraint("OneOf", "language", null, null, "polish", "english", "german", "french", "italian", "spanish")],
                [Pricing(cityFrom, cityTo, 119)], 70),
            new CatalogSeed("wieliczka-miner", "Wieliczka Salt Mine - Miner's Route", "Challenging route through narrow original tunnels. Requires crawling in places. Fit adults only.",
                [Tag("mine", "type"), Tag("adventure", "category"), Tag("guided-tour", "type"), Tag("wieliczka", "loc")],
                "Wieliczka", "ul. Daniłowicza 10", cityFrom, cityTo, new CatalogOpeningHours(new TimeOnly(9, 0), new TimeOnly(15, 0)), false,
                [Constraint("Range", "group_size", 1, 10), Constraint("RequiredDaysAhead", "booking_days_ahead", 5, null), Constraint("OneOf", "language", null, null, "polish", "english")],
                [Pricing(cityFrom, cityTo, 149)], 20),
            new CatalogSeed("tatry", "Tatra Mountains - Morskie Oko Day Trip", "Full day excursion from Kraków. Bus to Zakopane, shuttle to trailhead, 9km hike to the lake and back.",
                [Tag("hiking", "type"), Tag("nature", "category"), Tag("mountains", "category"), Tag("full-day", "category"), Tag("zakopane", "loc")],
                "Zakopane", "Palenica Białczańska (trailhead)", cityFrom, cityTo, null, false,
                [Constraint("Range", "group_size", 1, 20), Constraint("RequiredDaysAhead", "booking_days_ahead", 2, null)],
                [Pricing(cityFrom, cityTo, 160)], 40),
            new CatalogSeed("zakrzowek", "Zakrzówek Reservoir - Open Water Swimming", "Turquoise quarry lake in Kraków. Free entry, no booking. Bring your own towel.",
                [Tag("swimming", "type"), Tag("outdoor", "type"), Tag("free", "category"), Tag("krakow", "loc")],
                "Kraków", "ul. Twardowskiego", new DateOnly(2026, 5, 1), new DateOnly(2026, 8, 31), new CatalogOpeningHours(new TimeOnly(8, 0), new TimeOnly(20, 0)), false,
                [], [Pricing(new DateOnly(2026, 5, 1), new DateOnly(2026, 8, 31), 0)], 999),
            
            // OPN
            new CatalogSeed("opn-zamek-ojcow", "Zamek w Ojcowie - Sightseeing", "Self-guided tour of the castle ruins. Cash payments only on site.",
                [Tag("castle", "type"), Tag("history", "category")], "Ojców", "Ojców 9", opnFrom, opnTo, 
                new CatalogOpeningHours(new TimeOnly(9, 0), new TimeOnly(16, 30)), false,
                [], [Pricing(opnFrom, opnTo, 22)], 200),

            new CatalogSeed("opn-jaskinia-lokietka", "Jaskinia Łokietka - Guided Tour", "Entry in groups with a duty guide every 30 minutes. Long-sleeved clothing required.",
                [Tag("cave", "type"), Tag("guided-tour", "type")], "Ojców", "Góra Chełmowa", opnFrom, opnTo, 
                new CatalogOpeningHours(new TimeOnly(9, 0), new TimeOnly(18, 30)), false,
                [Constraint("Range", "group_size", 2, 60), Constraint("OneOf", "language", null, null, "polish", "english")], 
                [Pricing(opnFrom, opnTo, 30)], 60),
            
            new CatalogSeed("opn-jaskinia-ciemna", "Jaskinia Ciemna - Archaeological Route", "Exploring a dark cave. Bringing your own flashlight is highly recommended. Requires good physical condition.",
                [Tag("cave", "type"), Tag("guided-tour", "type"), Tag("adventure", "category")], "Ojców", "Góra Koronna", opnFrom, new DateOnly(2026, 9, 14), 
                new CatalogOpeningHours(new TimeOnly(9, 0), new TimeOnly(16, 0)), false,
                [Constraint("Range", "group_size", 2, 60), Constraint("RequiredDaysAhead", "booking_days_ahead", 1, null)], 
                [Pricing(opnFrom, new DateOnly(2026, 9, 14), 20)], 60),

            new CatalogSeed("opn-ekspozycja", "Ekspozycja Przyrodnicza & 3D Movie", "3D movie screening and museum tour. Minimum group size: 3 people.",
                [Tag("museum", "type"), Tag("indoor", "type")], "Ojców", "Ojców 9", opnFrom, opnTo, 
                new CatalogOpeningHours(new TimeOnly(9, 0), new TimeOnly(15, 0)), false,
                [Constraint("Range", "group_size", 3, 50), Constraint("RequiredDaysAhead", "booking_days_ahead", 1, null)], 
                [Pricing(opnFrom, opnTo, 20)], 50),

            new CatalogSeed("opn-pieskowa-skala", "Zamek w Pieskowej Skale - Entry", "Entry to the courtyard and museum exhibitions.",
                [Tag("castle", "type"), Tag("museum", "type")], "Sułoszowa", "Sułoszowa 5", opnFrom, opnTo, 
                new CatalogOpeningHours(new TimeOnly(9, 0), new TimeOnly(17, 0)), false,
                [], [Pricing(opnFrom, opnTo, 30)], 300),

            new CatalogSeed("opn-brama-krakowska", "Brama Krakowska - Attraction", "Viewpoint at the bottom of the valley.",
                [Tag("outdoor", "type"), Tag("free", "category")], "Ojców", "Dolina Prądnika", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), null, false,
                [], [Pricing(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), 0)], 999),

            new CatalogSeed("opn-pstrag", "Lunch: Pstrąg Ojcowski", "A portion of traditional trout. No reservation required.",
                [Tag("food", "category")], "Ojców", "Dolina Prądnika", opnFrom, opnTo, 
                new CatalogOpeningHours(new TimeOnly(10, 0), new TimeOnly(19, 0)), false,
                [], [Pricing(opnFrom, opnTo, 45)], 200),
                
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
                seed.IsEvent,
                seed.Tags,
                seed.Constraints);
            
            if (seed.OpeningHours != null) 
                entry.SetOpeningHours(seed.OpeningHours);
                
            foreach (var pricing in seed.PricingPeriods)
                entry.AddPricingPeriod(pricing);

            catalogEntriesByKey[seed.ComponentKey] = entry;
            await _catalogRepository.AddAsync(entry);
            await _ticketPoolRepository.AddAsync(new TicketPool(entry.Id, seed.Capacity));
        }

        var relationSeeds = new[]
        {
            new RelationSeed("wawel-state-rooms", "wawel-armoury", RelationType.Suggests, "same_location", "Both in Wawel Castle - natural to combine State Rooms with Armoury."),
            new RelationSeed("wawel-state-rooms", "wawel-dragons-den", RelationType.Suggests, "same_location", "Dragon's Den is right below the castle - easy add-on after State Rooms."),
            new RelationSeed("wawel-armoury", "wawel-dragons-den", RelationType.Suggests, "same_location", "Dragon's Den is right below the Armoury - 5 minutes walk."),
            new RelationSeed("st-marys-altar", "sukiennice", RelationType.Suggests, "same_location", "Both on the Main Market Square - visit together in one go."),
            new RelationSeed("st-marys-tower", "sukiennice", RelationType.Suggests, "same_location", "Tower climb and Sukiennice fit naturally into one Market Square stop."),
            new RelationSeed("kazimierz-group", "pijalnia", RelationType.Suggests, "nearby", "Pijalnia is a good ending after the walking tour."),
            new RelationSeed("kazimierz-private", "pijalnia", RelationType.Suggests, "nearby", "Pijalnia works well as a post-tour stop."),
            new RelationSeed("tatry", "pijalnia", RelationType.Suggests, "reward", "Cold beer at Pijalnia is a solid reward after a full mountain day."),
            new RelationSeed("zakrzowek", "pijalnia", RelationType.Suggests, "nearby", "Swimming at Zakrzówek pairs well with an evening drink at Pijalnia."),
            new RelationSeed("wieliczka-tourist", "tauron-concert", RelationType.Excludes, "time_conflict", "Wieliczka takes most of the day - too exhausting for an evening concert."),
            new RelationSeed("wieliczka-miner", "tauron-concert", RelationType.Excludes, "time_conflict", "Miner's Route is even more demanding and clashes with the concert plan."),
            new RelationSeed("tatry", "wieliczka-tourist", RelationType.Excludes, "time_conflict", "Tatra Mountains is a full-day trip from Kraków - no time for Wieliczka the same day."),
            new RelationSeed("tatry", "tauron-concert", RelationType.Excludes, "exhaustion", "After a full mountain day you will be too tired for an evening concert."),
            new RelationSeed("tatry", "wawel-state-rooms", RelationType.Excludes, "exhaustion", "After a full mountain day, guided castle tours are too much - save Wawel for another day."),
            new RelationSeed("wieliczka-miner", "wieliczka-tourist", RelationType.Requires, "prerequisite", "Miner's Route requires prior orientation in the mine - plan Tourist Route in the same trip."),
            
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

        Console.WriteLine("=== DATA SEEDER ===");
        Console.WriteLine($"Components:   {componentsByKey.Count} (attractions + packages in one repository)");
        Console.WriteLine($"Catalog:      {catalogEntriesByKey.Count} entries");
        Console.WriteLine($"Pools:        {catalogEntriesByKey.Count}");
        Console.WriteLine($"Relations:    {relationSeeds.Length}");
        Console.WriteLine("===================");
        Console.WriteLine();
        Console.WriteLine("Key catalog IDs for testing:");
        foreach (var key in new[] { "wawel-state-rooms", "wawel-armoury", "wawel-dragons-den", "st-marys-altar", "st-marys-tower", "wieliczka-tourist", "wieliczka-miner", "tatry" })
            Console.WriteLine($"  {catalogEntriesByKey[key].Name}: {catalogEntriesByKey[key].Id}");
        Console.WriteLine("===================");
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
