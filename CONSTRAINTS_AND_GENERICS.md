# Constrainty, walidacja i generyczne traktowanie atrakcji

## Problem: jak traktowac generycznie rzeczy, ktore sa inne

Mecz pilkarski, wycieczka po Wawelu, koncert w Tauron Arenie, wieczor w barze - kazda z tych atrakcji ma inne reguly. Zamiast robic osobne klasy dla kazdego typu, system uzywa dwoch mechanizmow:

1. **Tagi** - do kategoryzacji i dopasowywania (zamiast sztywnych enumow typow)
2. **Constrainty** - do opisu ograniczen logistycznych (zamiast osobnych pol per typ atrakcji)

Dzieki temu `AttractionDefinition`, `CatalogEntry`, `SelectionItem` sa zawsze tymi samymi klasami - niezaleznie czy opisuja zamek, koncert czy pub.

---

## Tagi - generyczna kategoryzacja

Tag to para `(name, group?)`, np. `typ:landmark`, `tematyka:historia`, `loc:krakow`.

- Nie ma enuma typow atrakcji - zamiast tego tag `typ:event`, `typ:landmark`, `typ:bar` itp.
- Tagi sa na definicji, dziedziczone przez CatalogEntry, kopiowane do SelectionItem
- Moze ich byc dowolnie duzo - to glowny mechanizm filtrowania ("znajdz mi atrakcje z tagiem `tematyka:historia` w Krakowie")
- Warianty moga miec dodatkowe tagi ponad te z definicji (np. wariant "Zbrojownia" dodaje `indoor`)

```
AttractionDefinition "Wawel"
  Tags: [typ:landmark, tematyka:historia, loc:krakow]
  Variant "Zbrojownia"
    AdditionalTags: [indoor]
    -> efektywne tagi: typ:landmark + tematyka:historia + loc:krakow + indoor
```

---

## Constrainty - generyczne ograniczenia logistyczne

Zamiast osobnych pol `maxGroupSize`, `minGroupSize`, `requiredLanguage`, `daysAheadRequired` na kazdym typie atrakcji - wszystko to jest jednym lista `Constraint` z kluczem i wartoscia.

### Gdzie zyja constrainty

```
AttractionVariant.Constraints  (typ: Constraint - value object z domain AttractionDefinition)
        |
        | kopiowane przy tworzeniu CatalogEntry
        v
CatalogEntry.Constraints       (typ: BookingConstraint - value object z domain Catalog)
        |
        | przekazywane przez ICatalogEntryQuery jako ConstraintSnapshot
        v
SelectionSessionService        (tu sa walidowane przy AddItemAsync)
```

`BookingConstraint` to celowo oddzielna klasa od `Constraint` - Catalog nie zalezy od modulu AttractionDefinition, tylko przechowuje dane jako string type + key.

### Typy constraintow

| Typ | Przyklad uzycia | Pola |
|---|---|---|
| `Range` | group_size od 2 do 15 | Key, MinValue, MaxValue |
| `Min` | group_size min 4 | Key, MinValue |
| `Max` | group_size max 20 | Key, MaxValue |
| `RequiredDaysAhead` | trzeba zarezerwowac min 3 dni wczesniej | Key="booking_days_ahead", MinValue=3 |
| `OneOf` | jezyk: polish / english / german | Key="language", AllowedValues=[...] |

`Constraint` ma factory methods zamiast publicznego konstruktora - nie mozna stworzyc Constraint w zlym stanie:

```csharp
Constraint.Range("group_size", 1, 15)       // min 1, max 15
Constraint.RequiredDaysAhead(3)             // klucz ustawiony automatycznie
Constraint.OneOf("language", ["pl","en"])   // musi miec przynajmniej 1 wartosc
```

---

## SelectionRule - generyczne pakiety

`AttractionPackage` grupuje kilka atrakcji/wariantow z regula ile trzeba wziac:

| Typ | Znaczenie | Przyklad |
|---|---|---|
| `All` | weź wszystkie komponenty z pakietu | "Pelny pakiet Wawelu - wszystkie 3 warianty" |
| `PickN(n)` | wybierz dokladnie n z dostepnych | "Wawel 2 z 3 - wybierz 2 warianty" |

`SelectionRule` to osobny value object - pakiet nie musi wiedziec co jest w srodku, tylko ile elementow ma byc wybranych.

**To NIE jest to samo co `OneOf` w constraintach.** `SelectionRule` dotyczy pakietow (ile atrakcji brac). `OneOf` w constraintach dotyczy wartosci pola (np. jaki jezyk wybrać).

---

## Composite Pattern - jednolite traktowanie atrakcji i pakietow

`AttractionDefinition` i `AttractionPackage` dziedzicza po `AttractionComponent`:

```
AttractionComponent (abstract)
  - Id, Name, Description, Tags
  - AddTag(), RemoveTag()
       |                    |
       v                    v
AttractionDefinition    AttractionPackage
  - Location?             - SelectionRule
  - OpeningHours?         - ComponentIds (List<Guid>)
  - SeasonalAvailability?
  - Variants
  - IsComplete
```

Dzieki temu repozytorium (`IAttractionComponentRepository`) przechowuje oba typy jednolicie. Przyszly modul planowania trasy moze iterowac po komponentach nie wiedzac czy to pojedyncza atrakcja czy pakiet.

---

## Walidacja constraintow - gdzie i jak

### Walidacja przy tworzeniu obiektow (domain)

Wszystkie obiekty domenowe waliduja sie same w konstruktorze / factory methods - nie mozna stworzyc obiektu w zlym stanie:

| Obiekt | Co waliduje |
|---|---|
| `Constraint.Range(min, max)` | min <= max |
| `Constraint.OneOf(values)` | lista niepusta |
| `Constraint.RequiredDaysAhead(days)` | days >= 0 |
| `SelectionRule.PickN(n)` | n > 0 |
| `OpeningHours(open, close)` | open < close |
| `DateRange(from, to)` | from <= to |
| `PricingPeriod` | brak nakladek z innymi okresami (walidacja na agregacie CatalogEntry) |
| `SeasonalAvailability` | przynajmniej 1 sezon = true |
| `AttractionDefinition.AddVariant(name)` | nazwa unikalna wsrod wariantow |
| `SelectionSession(city, groupSize)` | city niepuste, groupSize >= 1 |
| `SelectionSession.AddMustHaveItem(item)` | item nie jest juz na liscie |

### Walidacja przy dodaniu atrakcji do sesji (aplikacja)

`SelectionSessionService.AddItemAsync` - dwie kategorie:

**Hard constraints - blokuja dodanie (DomainException):**

| Co sprawdza | Skad dane | Przyklad bledu |
|---|---|---|
| Dostepnosc biletow | `IAvailabilityQuery` | "No tickets available for 'Wawel - Zbrojownia'" |
| `RequiredDaysAhead` | `session.TravelDateRange.From` vs `DateTime.UtcNow` | "requires booking at least 3 days ahead (you have 1 day)" |
| `Range` group_size | `session.GroupSize` | "allows maximum group size of 15 (your group: 20)" |
| `Min` group_size | `session.GroupSize` | "requires minimum group size of 2 (your group: 1)" |
| `Max` group_size | `session.GroupSize` | "allows maximum group size of 20 (your group: 25)" |

**Soft constraints - ostrzezenie w odpowiedzi, atrakcja dodana:**

| Co sprawdza | Dlaczego nie blokuje |
|---|---|
| `OneOf` (np. jezyk) | Uzytkownik wybiera konkretna opcje przy bookingu, nie przy doborze atrakcji do listy |

**Relacje miedzy atrakcjami - soft issues (RelationValidationService):**

| Typ relacji | Co sprawdza | Issue type |
|---|---|---|
| `Excludes` | nowa atrakcja wyklucza cos juz w sesji (i odwrotnie) | `Conflict` |
| `Requires` | wymagana zaleznosc nie jest jeszcze w sesji | `RequirementMissing` |
| `Suggests` | (nie generuje issue - wypelnia OptionalSuggestions) | - |
| `Replaces` | (zdefiniowany, niezaimplementowany w walidacji) | - |

---

## Pelny przyklad: dodanie Wawel State Rooms do sesji

```
Sesja: Krakow, 10-14.04.2026, groupSize=4
CatalogEntry "Wawel State Rooms":
  Constraints: [Range group_size 1-15, RequiredDaysAhead 2, OneOf language [pl,en,de]]
  Availability: 50 wolnych biletow

1. IsAvailable? -> true (OK, nie blokuje)
2. RequiredDaysAhead 2 -> dzis 31.03, podróz 10.04 -> 10 dni -> OK
3. Range group_size 1-15 -> groupSize=4 -> OK
4. OneOf language -> soft issue: "requires choosing: polish, english, german"
5. Relacje: State Rooms SUGGESTS Armoury -> sugestia
6. -> DODANO. MustHave: [State Rooms], Optional: [Armoury], Issues: [OneOf language warning]

Gdyby groupSize=20:
3. Range group_size 1-15 -> groupSize=20 -> BLOKUJE
-> DomainException: "allows maximum group size of 15 (your group: 20)"
-> State Rooms NIE jest dodany do sesji
```

---

## Dlaczego taka architektura daje generycznosc

- Mecz pilkarski moze miec constraint `Range group_size 100-50000` i `OneOf sector [north,south,vip]`
- Pub nie ma zadnych constraintow - lista pusta, walidacja przechodzi bez problemu
- Wycieczka jezykowa ma `OneOf language [pl,en]` i `Range group_size 1-20`
- Wszystkie sa instancjami tej samej klasy `CatalogEntry` z ta sama lista `BookingConstraint`

Nowy typ ograniczenia = nowy case w switchu w `SelectionSessionService`. Nowy typ atrakcji = nowe tagi i constrainty na istniejacych klasach. Zero nowych klas domenowych.
