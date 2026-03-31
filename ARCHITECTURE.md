# Architektura projektu

## Ogolny opis

System do zarzadzania atrakcjami turystycznymi - od definicji (archetypu), przez katalog instancji z cenami, pule biletow, az po dobieranie atrakcji do wycieczki z walidacja relacji.

**Stack**: .NET 8, ASP.NET Core, REST API, Swagger, InMemory repositories (gotowe do podmiany na baze danych)

---

## Moduly

| Modul | Odpowiedzialnosc |
|---|---|
| **AttractionDefinition** | Archetyp atrakcji - definicja, warianty, tagi, constrainty, pakiety |
| **Catalog** | Instancje atrakcji dostepne w konkretnym czasie z dynamicznym cennikiem |
| **Availability** | Pule biletow organizatora, rezerwacje (soft/hard), sprawdzanie dostepnosci |
| **TripSelection** | Sesja doboru atrakcji - relacje (wyklucza/sugeruje/wymaga), walidacja kompatybilnosci, dwie listy (must-have + optional) |

---

## Architektura warstwowa (Ports & Adapters)

Kazdy modul ma 4 warstwy:

```
Api  ->  Application  ->  Domain  <-  Infrastructure
```

- **Domain** - zero zewnetrznych zaleznosci (tylko PB.Shared). Agregaty, encje, value objects, enumy, porty (interfejsy repozytoriow), serwisy domenowe.
- **Application** - orchestrator. Serwisy aplikacyjne, DTOs, mapowanie. Definiuje porty cross-module. Referencja do Domain.
- **Api** - kontrolery REST, marker class modulu. Referencja do Application.
- **Infrastructure** - implementacje repozytoriow (InMemory), adaptery cross-module, rejestracja DI. Referencja do Application.

**Zasada**: Domain nie importuje klas z zadnego innego modulu. Cross-module komunikacja tylko przez interfejsy definiowane w Application layer modulu konsumujacego.

---

## Shared Layer (PB.Shared)

Bazowe klasy wspolne dla wszystkich modulow:

| Klasa | Opis |
|---|---|
| `Entity` | Bazowa klasa z `Id: Guid`, rownosc po Id |
| `AggregateRoot` | Rozszerza Entity (marker) |
| `ValueObject` | Abstrakcyjna klasa z `GetEqualityComponents()`, rownosc po komponentach |
| `DomainException` | Wyjatek domenowy |
| `Tag` | Value Object - `Name` + `Group?`, normalizacja lowercase/trim, ToString() = "group:name" |
| `Money` | Value Object - `Amount: decimal` + `Currency: string`, static `Free`, walidacja Amount >= 0 |

---

## MODULE 1: AttractionDefinition

### Cel

Archetyp produktu (atrakcji). Definicja to "przepis" - **nie ma stanow** (brak Draft/Published/Archived). Gotowoscia steruje flaga `IsComplete`. Definicja moze miec wiele **wariantow** (np. Wawel -> Zbrojownia, Podziemia, Pietra).

### Composite Pattern

`AttractionDefinition` i `AttractionPackage` dziedzicza po wspolnej abstrakcji `AttractionComponent`. Dzieki temu reszta systemu (Catalog, TripSelection) moze traktowac je jednolicie - nie musi wiedziec czy ma do czynienia z pojedyncza atrakcja czy pakietem. Przygotowanie pod budowanie planu wycieczki w przyszlosci.

```
AttractionComponent (abstract AggregateRoot)
  - Name, Description, Tags
  - AddTag(), RemoveTag()
       |                  |
       v                  v
AttractionDefinition  AttractionPackage
  - Location?           - SelectionRule
  - OpeningHours?       - ComponentIds
  - SeasonalAvailability?
  - Variants
  - IsComplete
```

### Domain

**Agregaty:**

| Klasa | Opis |
|---|---|
| `AttractionComponent` | Abstrakcyjna baza - Name, Description, Tags (HashSet\<Tag\>), AddTag/RemoveTag |
| `AttractionDefinition` | Rozszerza AttractionComponent - Location?, OpeningHours?, SeasonalAvailability?, Variants |
| `AttractionPackage` | Rozszerza AttractionComponent - SelectionRule, ComponentIds (List\<Guid\>) |

`AttractionDefinition` - metody:
- `Update(name, description)`, `SetLocation(location)`, `SetOpeningHours(hours)`, `SetSeasonalAvailability(availability)`
- `AddVariant(name, desc, duration?)` -> AttractionVariant
- `RemoveVariant(variantId)`, `GetVariant(variantId)`
- `IsComplete` = Name + min 1 Tag + Location ustawione

`AttractionPackage` - metody:
- `Update(name, desc, rule)`, `AddComponent(id)`, `RemoveComponent(id)`, `SetSelectionRule(rule)`

**Encja:**

| Klasa | Opis |
|---|---|
| `AttractionVariant` | Encja wewnatrz AttractionDefinition - Name, Description, DurationMinutes?, AdditionalTags, Constraints |

`AttractionVariant` - metody: `AddTag`, `RemoveTag`, `AddConstraint`, `RemoveConstraint`, `Update`

**Value Objects:**

| Klasa | Pola | Walidacja |
|---|---|---|
| `Location` | City, Address?, Latitude?, Longitude? | City wymagane |
| `OpeningHours` | Open: TimeOnly, Close: TimeOnly | Open < Close |
| `SeasonalAvailability` | Spring, Summer, Autumn, Winter (bool) | Min 1 sezon true |
| `Constraint` | Type, Key, MinValue?, MaxValue?, AllowedValues | Factory methods: Range, Min, Max, OneOf, RequiredDaysAhead |
| `SelectionRule` | Type, Count? | Factory methods: All(), PickN(n) |

**Enumy:** `ConstraintType` { Range, Min, Max, OneOf, RequiredDaysAhead }, `SelectionRuleType` { All, PickN }, `Season` { Spring, Summer, Autumn, Winter }

**Port:** `IAttractionComponentRepository` - jeden wspolny dla obu typow
- `GetByIdAsync` -> `AttractionComponent?`
- `GetAllDefinitionsAsync`, `GetDefinitionsByTagAsync` -> przefiltrowane po typie
- `GetAllPackagesAsync`
- `AddAsync/UpdateAsync/DeleteAsync(AttractionComponent)`

### Przyklad: Wawel

```
AttractionDefinition "Wawel"
  Tags: [typ:landmark, tematyka:historia, tematyka:kultura, loc:krakow]
  Location: Krakow, Wawel 5
  Variants:
    "Zbrojownia"     AdditionalTags:[indoor]  Constraints:[group_size Range 1-15]         Duration:45min
    "Podziemia"      AdditionalTags:[indoor]  Constraints:[group_size Range 1-10, RequiredDaysAhead:3]  Duration:60min
    "Pietra I i II"  AdditionalTags:[indoor]  Constraints:[group_size Range 1-20, jezyk OneOf:[pl,en]]  Duration:90min

AttractionPackage "Wawel - 2 z 3"
  SelectionRule: PickN(2)
  ComponentIds: [zbrojownia_id, podziemia_id, pietra_id]
```

### REST API

```
POST   /api/attraction-definitions                              Stworz definicje
GET    /api/attraction-definitions?tag=&city=&isComplete=        Lista z filtrami
GET    /api/attraction-definitions/{id}                          Pobierz
PUT    /api/attraction-definitions/{id}                          Update
DELETE /api/attraction-definitions/{id}                          Usun
POST   /api/attraction-definitions/{id}/variants                 Dodaj wariant
PUT    /api/attraction-definitions/{id}/variants/{vid}           Update wariantu
DELETE /api/attraction-definitions/{id}/variants/{vid}           Usun wariant
POST   /api/attraction-definitions/{id}/tags                     Dodaj tag
DELETE /api/attraction-definitions/{id}/tags                     Usun tag

POST   /api/attraction-packages                                  Stworz pakiet
GET    /api/attraction-packages                                  Lista
GET    /api/attraction-packages/{id}                             Pobierz
PUT    /api/attraction-packages/{id}                             Update
DELETE /api/attraction-packages/{id}                             Usun
POST   /api/attraction-packages/{id}/components/{componentId}    Dodaj komponent
DELETE /api/attraction-packages/{id}/components/{componentId}    Usun komponent
```

---

## MODULE 2: Catalog

### Cel

Instancje atrakcji dostepne w konkretnym czasie. CatalogEntry **MA stany** (Available, SoldOut, Cancelled, Upcoming). Ceny sa dynamiczne - `PricingPeriod` z roznymi cenami w roznych okresach.

### Domain

**Agregat:**

| Klasa | Opis |
|---|---|
| `CatalogEntry` | AttractionDefinitionId, VariantId?, Name, Description, Tags, Location, DateRange, OpeningHours?, IsEvent, Status, PricingPeriods, **Constraints** |

Metody:
- `Update(name, desc, location, dateRange, isEvent)` - nie mozna updatowac cancelled entry
- `AddTag(tag)`, `RemoveTag(tag)`, `SetOpeningHours(hours)`
- `AddPricingPeriod(period)` - walidacja: brak nakladek
- `RemovePricingPeriodAt(index)`, `GetPriceForDate(date) -> Money?`
- `Cancel()`, `MarkAsSoldOut()`, `MarkAsAvailable()`

**Value Objects:**

| Klasa | Pola | Walidacja |
|---|---|---|
| `CatalogLocation` | City, Address? | City wymagane |
| `CatalogOpeningHours` | Open, Close (TimeOnly) | Open < Close |
| `DateRange` | From, To (DateOnly) | From <= To. Metody: Contains(date), Overlaps(other) |
| `PricingPeriod` | DateRange, Price (Money), Discounts (List\<Discount\>) | DateRange i Price wymagane |
| `Discount` | Description, PercentOff?, AmountOff? (Money), Condition? | Min 1 z PercentOff/AmountOff, PercentOff 0-100 |
| `BookingConstraint` | Type, Key, MinValue?, MaxValue?, AllowedValues | Type i Key wymagane. Nosi constrainty z AttractionVariant - walidowane w TripSelection |

**Enum:** `CatalogEntryStatus` { Available, SoldOut, Cancelled, Upcoming }

**Port:** `ICatalogEntryRepository` - CRUD + SearchAsync(city?, from?, to?, tags?, status?) + GetByAttractionDefinitionIdAsync

### REST API

```
POST   /api/catalog/entries                          Stworz entry
GET    /api/catalog/entries?city=&from=&to=&tags=&status=  Szukaj z filtrami
GET    /api/catalog/entries/{id}                     Pobierz
PUT    /api/catalog/entries/{id}                     Update
DELETE /api/catalog/entries/{id}                     Usun
POST   /api/catalog/entries/{id}/pricing             Dodaj PricingPeriod
DELETE /api/catalog/entries/{id}/pricing/{index}     Usun PricingPeriod
POST   /api/catalog/entries/{id}/cancel              Anuluj
POST   /api/catalog/entries/{id}/sold-out            Oznacz sold out
POST   /api/catalog/entries/{id}/available           Przywroc dostepnosc
POST   /api/catalog/entries/{id}/tags                Dodaj tag
DELETE /api/catalog/entries/{id}/tags                Usun tag
```

---

## MODULE 3: Availability

### Cel

Organizator ma kupione **pule biletow** na CatalogEntry. Modul ogarnia pojemnosc, rezerwacje (soft z wygasaniem i hard), sprawdzanie dostepnosci.

### Domain

**Agregat:**

| Klasa | Opis |
|---|---|
| `TicketPool` | CatalogEntryId, VariantId?, TotalCapacity, Reservations (List\<Reservation\>) |

Computed: `PendingCount`, `ConfirmedCount`, `AvailableCount`, `IsAvailable`

Metody:
- `Reserve(quantity, expiresAt?, notes?) -> Reservation` - rzuca jesli brak miejsca
- `ConfirmReservation(id)`, `CancelReservation(id)`
- `ExpireOutdated(now) -> int` - wygasza przedawnione pending rezerwacje
- `IncreaseCapacity(amount)`, `ReduceCapacity(amount)` - rzuca jesli ponizej aktywnych rezerwacji

**Encja:**

| Klasa | Opis |
|---|---|
| `Reservation` | Quantity, Status, CreatedAt, ExpiresAt?, Notes? |

Status: Pending -> Confirmed / Cancelled / Expired. Metody: `Confirm()`, `Cancel()`, `Expire()`, `IsActive`

**Enum:** `ReservationStatus` { Pending, Confirmed, Cancelled, Expired }

**Port:** `ITicketPoolRepository` - CRUD + GetByCatalogEntryIdAsync

### REST API

```
POST   /api/availability/pools                               Stworz pule
GET    /api/availability/pools                               Lista pul
GET    /api/availability/pools/{id}                          Pobierz pule
DELETE /api/availability/pools/{id}                          Usun pule
GET    /api/availability/pools/by-entry/{catalogEntryId}     Pula dla entry
POST   /api/availability/pools/{id}/reserve                  Zarezerwuj bilety
POST   /api/availability/pools/{id}/confirm/{reservationId}  Potwierdz rezerwacje
POST   /api/availability/pools/{id}/cancel/{reservationId}   Anuluj rezerwacje
POST   /api/availability/pools/{id}/expire                   Wygas przedawnione
POST   /api/availability/pools/{id}/capacity/increase        Zwieksz pojemnosc
POST   /api/availability/pools/{id}/capacity/reduce          Zmniejsz pojemnosc
GET    /api/availability/check/{catalogEntryId}              Sprawdz dostepnosc
```

---

## MODULE 4: TripSelection

### Cel

Sesja doboru atrakcji do wycieczki. Uzytkownik dodaje atrakcje z katalogu - modul waliduje kompatybilnosc (relacje), sprawdza dostepnosc biletow, generuje sugestie i wykluczenia. Zwraca dwie listy: **must-have** (explicite dodane + wymagane) i **optional** (sugestie).

### Typy relacji

| Typ | Znaczenie | Przyklad |
|---|---|---|
| **Excludes** | Jesli A to nie B | Audiobook PL wyklucza audiobook EN |
| **Suggests** | Jesli A to moze B | Zwiedzanie Wawelu sugeruje Sukiennice |
| **Requires** | Jesli A to musi B | Pietra II Wawelu wymaga biletu na Pietra I |
| **Replaces** | A jest alternatywa dla B | Pelny pakiet Wawelu zastepuje pojedyncze |

### Domain

**Agregaty:**

| Klasa | Opis |
|---|---|
| `SelectionSession` | DestinationCity, TravelDateRange, **GroupSize**, MustHaveItems, OptionalSuggestions, ExcludedIds, Issues, CreatedAt |
| `AttractionRelation` | SourceId, TargetId, Type (RelationType), Context?, Description? |

`SelectionSession` - metody:
- `AddMustHaveItem(item)` - automatycznie usunie z sugestii jesli tam byl
- `RemoveMustHaveItem(catalogEntryId)`
- `SetSuggestions(items)` - odfiltrowuje juz dodane i wykluczone
- `SetExcludedIds(ids)` - odfiltrowuje juz dodane do must-have
- `SetIssues(issues)`

**Encja:**

| Klasa | Opis |
|---|---|
| `SelectionItem` | CatalogEntryId, Name, Tags, AttractionDefinitionId, VariantId?, AddedAt |

**Value Objects:**

| Klasa | Opis |
|---|---|
| `SelectionIssue` | Type (IssueType), Message, RelatedItemId? |
| `DateRange` | From, To (DateOnly) - lokalna kopia |

**Enumy:** `RelationType` { Excludes, Suggests, Requires, Replaces }, `IssueType` { ConstraintViolation, Conflict, RequirementMissing }

### Serwis domenowy: RelationValidationService

- `ValidateNewItem(item, existingItems, relations)` -> lista Issues
  - Sprawdza Excludes w obie strony (nowy wyklucza istniejacy / istniejacy wyklucza nowy)
  - Sprawdza Requires (czy wymagane zaleznosci sa juz w sesji)
- `GetSuggestions(item, relations)` -> lista Guid (TargetId relacji Suggests)
- `GetExclusions(item, relations)` -> lista Guid (TargetId relacji Excludes)

### Flow: dodanie atrakcji do sesji (AddItemAsync)

```
1. Pobierz sesje z repo
2. Pobierz info o CatalogEntry przez ICatalogEntryQuery (cross-module)
   -> CatalogEntrySnapshot zawiera Constraints
3. Sprawdz dostepnosc przez IAvailabilityQuery (cross-module)
   -> brak biletow = Issue(ConstraintViolation)
4. Waliduj BookingConstraints z CatalogEntry:
   -> RequiredDaysAhead: (TravelFrom - dzis) >= minValue?
   -> Range/Min/Max group_size: session.GroupSize w dozwolonym zakresie?
   -> OneOf: informuje o wymaganych wyborach (np. jezyk)
5. Pobierz relacje dla AttractionDefinitionId (+ VariantId) z repo
   -> takze dla istniejacych items (by sprawdzic "existing excludes new")
6. RelationValidationService.ValidateNewItem(...) -> issues
7. Stworz SelectionItem, dodaj do MustHaveItems
8. GetSuggestions -> szukaj CatalogEntry po AttractionDefinitionId, sprawdz dostepnosc -> OptionalSuggestions
9. GetExclusions -> ExcludedIds
10. Zapisz sesje
```

### Cross-module porty (Application layer)

| Port | Implementacja (Infrastructure) | Deleguje do |
|---|---|---|
| `ICatalogEntryQuery` | `CatalogEntryQueryAdapter` | `ICatalogService` z Catalog |
| `IAvailabilityQuery` | `AvailabilityQueryAdapter` | `IAvailabilityService` z Availability |

`CatalogEntrySnapshot` - record w TripSelection.Application mapowany z CatalogEntryDto:
Id, Name, Description, AttractionDefinitionId, VariantId?, Tags, City, IsEvent, Status, **Constraints**

`ConstraintSnapshot` - record: Type, Key, MinValue?, MaxValue?, AllowedValues

### REST API

```
POST   /api/trip-selections/sessions                            Stworz sesje
GET    /api/trip-selections/sessions/{id}                       Pobierz sesje
POST   /api/trip-selections/sessions/{id}/items                 Dodaj atrakcje (body: catalogEntryId)
DELETE /api/trip-selections/sessions/{id}/items/{catalogEntryId} Usun atrakcje

POST   /api/trip-selections/relations                           Stworz relacje
GET    /api/trip-selections/relations                           Lista relacji
GET    /api/trip-selections/relations/{id}                      Pobierz relacje
DELETE /api/trip-selections/relations/{id}                      Usun relacje
```

---

## Zaleznosci miedzy modulami

```
AttractionDefinition  (niezalezny)
       |
       v  (AttractionDefinitionId - referencja po Guid)
    Catalog  (niezalezny, linkuje do AD przez ID)
       |
       v  (CatalogEntryId - referencja po Guid)
  Availability  (niezalezny, linkuje do Catalog przez ID)

TripSelection  (zna Catalog i Availability przez porty w Application layer)
  -> ICatalogEntryQuery   (impl: CatalogEntryQueryAdapter -> ICatalogService)
  -> IAvailabilityQuery   (impl: AvailabilityQueryAdapter -> IAvailabilityService)
```

Kompilacyjnie: TripSelection.Infrastructure referencje -> Catalog.Application, Availability.Application

---

## Przeplywy danych

### 1. Tworzenie atrakcji (admin)

```
Admin tworzy AttractionDefinition "Wawel"
  -> dodaje tagi, lokalizacje, warianty
  -> IsComplete = true

Admin tworzy CatalogEntry z definicji
  -> podaje daty (lato 2025), cennik (PricingPeriods)
  -> status: Available

Admin tworzy TicketPool w Availability
  -> CatalogEntryId, TotalCapacity: 100
```

### 2. Dobieranie atrakcji (uzytkownik)

```
Uzytkownik tworzy SelectionSession (Krakow, 17-19.03)

Dodaje "Wawel Zbrojownia" do sesji:
  -> TripSelection sprawdza dostepnosc biletow (Availability)
  -> sprawdza relacje: Zbrojownia PL EXCLUDES Zbrojownia EN
  -> sugeruje: Podziemia (SUGGESTS), Sukiennice (SUGGESTS)
  -> wynik: MustHave: [Zbrojownia PL], Optional: [Podziemia, Sukiennice], Excluded: [Zbrojownia EN]
```

---

## Struktura plikow

```
src/
├── Bootstrapper/PB.Api/
│   ├── Program.cs                    (rejestracja modulow, Swagger, MapControllers, DataSeeder)
│   ├── DataSeeder.cs                 (seedowanie danych demo: atrakcje Krakowa z cenami, biletami, relacjami)
│   ├── ExceptionMiddleware.cs        (DomainException -> 400 Bad Request z JSON)
│   └── PB.Api.csproj                 (ref: wszystkie Api + Infrastructure)
│
├── Shared/PB.Shared/
│   └── Domain/
│       ├── Entity.cs, AggregateRoot.cs, ValueObject.cs, DomainException.cs
│       ├── Tag.cs                    (Name + Group?, lowercase/trim)
│       └── Money.cs                  (Amount + Currency, static Free)
│
├── Modules/
│   ├── AttractionDefinition/
│   │   ├── Domain/
│   │   │   ├── Aggregates/           AttractionDefinition.cs, AttractionPackage.cs
│   │   │   ├── Entities/             AttractionVariant.cs
│   │   │   ├── Enums/                ConstraintType, SelectionRuleType, Season
│   │   │   ├── Ports/                IAttractionDefinitionRepository, IAttractionPackageRepository
│   │   │   └── ValueObjects/         Location, OpeningHours, SeasonalAvailability, Constraint, SelectionRule
│   │   ├── Application/
│   │   │   ├── DTOs/                 AttractionDefinitionDto, AttractionPackageDto, AttractionVariantDto,
│   │   │   │                         ConstraintDto, LocationDto, OpeningHoursDto, SeasonalAvailabilityDto,
│   │   │   │                         SelectionRuleDto, TagDto, RequestDtos
│   │   │   └── Services/             IAttractionDefinitionService + impl, IAttractionPackageService + impl
│   │   ├── Api/Controllers/          AttractionDefinitionsController, AttractionPackagesController
│   │   └── Infrastructure/
│   │       ├── Repositories/         InMemoryAttractionComponentRepository (przechowuje oba typy)
│   │       └── Extensions.cs         AddAttractionDefinitionModule()
│   │
│   ├── Catalog/
│   │   ├── Domain/
│   │   │   ├── Aggregates/           CatalogEntry.cs
│   │   │   ├── Enums/                CatalogEntryStatus
│   │   │   ├── Ports/                ICatalogEntryRepository
│   │   │   └── ValueObjects/         CatalogLocation, CatalogOpeningHours, DateRange, PricingPeriod, Discount, BookingConstraint
│   │   ├── Application/
│   │   │   ├── DTOs/                 CatalogDtos.cs (wszystkie DTOs w jednym pliku)
│   │   │   └── Services/             ICatalogService + impl
│   │   ├── Api/Controllers/          CatalogController
│   │   └── Infrastructure/
│   │       ├── Repositories/         InMemoryCatalogEntryRepository
│   │       └── Extensions.cs         AddCatalogModule()
│   │
│   ├── Availability/
│   │   ├── Domain/
│   │   │   ├── Aggregates/           TicketPool.cs
│   │   │   ├── Entities/             Reservation.cs
│   │   │   ├── Enums/                ReservationStatus
│   │   │   └── Ports/                ITicketPoolRepository
│   │   ├── Application/
│   │   │   ├── DTOs/                 AvailabilityDtos.cs
│   │   │   └── Services/             IAvailabilityService + impl
│   │   ├── Api/Controllers/          AvailabilityController
│   │   └── Infrastructure/
│   │       ├── Repositories/         InMemoryTicketPoolRepository
│   │       └── Extensions.cs         AddAvailabilityModule()
│   │
│   └── TripSelection/
│       ├── Domain/
│       │   ├── Aggregates/           SelectionSession.cs, AttractionRelation.cs
│       │   ├── Entities/             SelectionItem.cs
│       │   ├── Enums/                RelationType, IssueType
│       │   ├── Ports/                ISelectionSessionRepository, IAttractionRelationRepository
│       │   ├── Services/             IRelationValidationService + RelationValidationService
│       │   └── ValueObjects/         SelectionIssue, DateRange
│       ├── Application/
│       │   ├── DTOs/                 TripSelectionDtos.cs
│       │   ├── Ports/                ICatalogEntryQuery (+ CatalogEntrySnapshot), IAvailabilityQuery
│       │   └── Services/             ISelectionSessionService + impl, IAttractionRelationService + impl
│       ├── Api/Controllers/          SelectionSessionsController, AttractionRelationsController
│       └── Infrastructure/
│           ├── CrossModule/          CatalogEntryQueryAdapter, AvailabilityQueryAdapter
│           ├── Repositories/         InMemorySelectionSessionRepository, InMemoryAttractionRelationRepository
│           └── Extensions.cs         AddTripSelectionModule()
```

---

## Rejestracja w DI (Program.cs)

```csharp
builder.Services.AddAttractionDefinitionModule();  // repo + serwisy AD
builder.Services.AddCatalogModule();               // repo + serwis Catalog
builder.Services.AddAvailabilityModule();          // repo + serwis Availability
builder.Services.AddTripSelectionModule();         // repo + serwisy + adaptery cross-module + domain service
```

Kontrolery ladowane dynamicznie przez `AddApplicationPart(typeof(XModule).Assembly)`.

---

## Infrastruktura

Wszystkie repozytoria uzywaja `ConcurrentDictionary<Guid, T>` (thread-safe, in-memory). Gotowe do podmiany na implementacje z baza danych - wystarczy zamienic rejestracje w `Extensions.cs`.

---

## DataSeeder (dane demo)

Przy starcie aplikacji `DataSeeder` automatycznie seeduje dane krakowskich atrakcji:

**8 definicji**: Wawel Castle (3 warianty: State Rooms, Armoury, Dragon's Den), St. Mary's Basilica, Sukiennice, Concert at Tauron Arena, Kazimierz Walking Tour, Pijalnia Wodki i Piwa, Wieliczka Salt Mine

**1 pakiet**: Wawel Full Experience (PickN(2))

**9 catalog entries** z cennikami (rozne ceny w roznych okresach, znizki studenckie) i **constraintami** (group_size, booking_days_ahead, language)

**9 ticket pools** z roznymi pojemnosciami

**5 relacji**: State Rooms SUGGESTS Armoury, Armoury SUGGESTS Dragon's Den, Wieliczka EXCLUDES Tauron, Kazimierz SUGGESTS Pijalnia, St. Mary's SUGGESTS Sukiennice

---

## Walidacja constraintow

Constrainty sa definiowane na `AttractionVariant` (modul AttractionDefinition) i przenoszone na `CatalogEntry` jako `BookingConstraint` przy tworzeniu instancji w katalogu. Walidacja odbywa sie w `SelectionSessionService.AddItemAsync()`:

| Typ constraintu | Co waliduje | Skad dane |
|---|---|---|
| `RequiredDaysAhead` | (data podrozy - dzis) >= wymagane dni | `session.TravelDateRange.From` |
| `Range` (group_size) | min <= session.GroupSize <= max | `session.GroupSize` |
| `Min` (group_size) | session.GroupSize >= min | `session.GroupSize` |
| `Max` (group_size) | session.GroupSize <= max | `session.GroupSize` |
| `OneOf` | informuje o wymaganych wyborach | constraint.AllowedValues |

Naruszenie constraintu generuje `SelectionIssue` z typem `ConstraintViolation`. Atrakcja jest dodawana do sesji mimo naruszen - issues sa ostrzezeniami, nie blokerami.
