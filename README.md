# Projek Biznesowy

System zarzadzania atrakcjami turystycznymi z podzialem na cztery moduly:
- `AttractionDefinition`: model komponentow atrakcji i pakietow
- `Catalog`: konkretne oferty dostepne w czasie z cenami i ograniczeniami
- `Availability`: pule biletow i rezerwacje
- `TripSelection`: budowanie sesji doboru atrakcji z relacjami i walidacja

Projekt jest napisany w `.NET 8`, wystawia REST API przez `ASP.NET Core` i domyslnie dziala na repozytoriach in-memory. Przy starcie aplikacji ladowane sa dane demo przez `DataSeeder`.

## Model domenowy

### Attraction components

Podstawowym bytem jest `AttractionComponent`.

Sa dwa typy komponentow:
- `attraction`: pojedyncze experience, np. `Wawel Castle - State Rooms`
- `package`: kompozyt grupujacy inne komponenty, np. pakiet Wawelu z regula `PickN(2)`

Kazdy komponent ma:
- `Id`
- `Name`
- `Description`
- `Tags`

Komponent typu `attraction` ma dodatkowo:
- `Location`
- `OpeningHours`
- `IsComplete`

Komponent typu `package` ma dodatkowo:
- `SelectionRule`
- `ComponentIds`

System nie uzywa obecnie publicznego modelu wariantow. Tozsamosc planowania, relacji i katalogu opiera sie na `AttractionComponentId`.

## Dodatkowa sciaga: dawny wariant vs obecny model

Ta sekcja jest tylko praktyczna sciaga decyzyjna. Nie opisuje calej architektury, tylko pokazuje jak teraz rozrozniac przypadki, ktore wczesniej byly wrzucane do worka `variant`.

| Sytuacja | Wczesniej | Teraz |
|---|---|---|
| Ten sam produkt, rozny jezyk | `Variant` albo czasem constraint | `CatalogEntry` + `OneOf language` w constraintach |
| Ten sam program, ale jezyk wybierany przy bookingu | `Variant` | `OneOf language` |
| Ten sam obiekt, ale inna trasa / inna czesc zwiedzania | raz `Variant`, raz osobna atrakcja | osobny `attraction` component |
| Inne prerequisite albo inne relacje planowania | czasem `Variant` | osobny `attraction` component |
| Realnie inne experience, nawet w tym samym miejscu | bywalo mieszane | osobny `attraction` component |
| Zestaw kilku atrakcji | `AttractionPackage` | `package` component |
| Relacje i logika planowania | mieszanka `definitionId` i `variantId` | tylko `AttractionComponentId` |

Najkrotsza regula:
- parametr wyboru przy rezerwacji, np. jezyk, to constraint
- osobne experience to osobny `attraction` component
- grupa atrakcji to `package`

### Catalog entries

`CatalogEntry` to konkretna oferta sprzedazowa powiazana z komponentem:
- wskazuje `AttractionComponentId`
- ma wlasna nazwe, opis, tagi i lokalizacje
- ma `DateRange`
- moze miec `OpeningHours`
- ma dynamiczny cennik przez `PricingPeriod`
- moze miec `BookingConstraint`
- ma status `Available`, `SoldOut` albo `Cancelled`

To oznacza, ze komponent opisuje czym jest atrakcja, a katalog opisuje kiedy i na jakich warunkach da sie ja kupic.

### Availability

`TicketPool` zarzadza pula miejsc dla konkretnego `CatalogEntry`.

Pula trzyma:
- laczna pojemnosc
- liste rezerwacji
- liczbe miejsc pending, confirmed i available

Rezerwacja przechodzi przez statusy:
- `Pending`
- `Confirmed`
- `Cancelled`
- `Expired`

### Trip selection

`SelectionSession` sluzy do skladania planu wyjazdu.

Sesja przechowuje:
- destination city
- date range wyjazdu
- group size
- liste `MustHaveItems`
- liste `OptionalSuggestions`
- `ExcludedIds`
- `Issues`

Relacje dzialaja na `AttractionComponentId` i moga miec typ:
- `Suggests`
- `Excludes`
- `Requires`
- `Replaces`

Przy dodaniu oferty do sesji system:
- sprawdza dostepnosc biletow
- waliduje constrainty z katalogu
- sprawdza relacje z innymi komponentami
- buduje sugestie
- buduje liste wykluczen

## Moduly i odpowiedzialnosci

### 1. AttractionDefinition

Modul trzyma model komponentow.

API:
- `POST /api/attraction-components`
- `GET /api/attraction-components`
- `GET /api/attraction-components/{id}`
- `PUT /api/attraction-components/{id}`
- `DELETE /api/attraction-components/{id}`
- `POST /api/attraction-components/{id}/tags`
- `DELETE /api/attraction-components/{id}/tags`
- `POST /api/attraction-components/{id}/components/{componentId}`
- `DELETE /api/attraction-components/{id}/components/{componentId}`

Najwazniejsze zastosowanie:
- tworzenie pojedynczych atrakcji
- tworzenie pakietow
- zarzadzanie tagami
- przypinanie komponentow do pakietu

### 2. Catalog

Modul zarzadza ofertami.

API:
- `POST /api/catalog/entries`
- `GET /api/catalog/entries`
- `GET /api/catalog/entries/{id}`
- `PUT /api/catalog/entries/{id}`
- `DELETE /api/catalog/entries/{id}`
- `POST /api/catalog/entries/{id}/pricing`
- `DELETE /api/catalog/entries/{id}/pricing/{index}`
- `POST /api/catalog/entries/{id}/cancel`
- `POST /api/catalog/entries/{id}/sold-out`
- `POST /api/catalog/entries/{id}/available`
- `POST /api/catalog/entries/{id}/tags`
- `DELETE /api/catalog/entries/{id}/tags`

Najwazniejsze zastosowanie:
- wystawienie atrakcji do katalogu na konkretny okres
- ustawienie cennika
- filtrowanie po miescie, datach, tagach i statusie

### 3. Availability

Modul pilnuje pojemnosci i rezerwacji.

API:
- `POST /api/availability/pools`
- `GET /api/availability/pools`
- `GET /api/availability/pools/{id}`
- `DELETE /api/availability/pools/{id}`
- `GET /api/availability/pools/by-entry/{catalogEntryId}`
- `POST /api/availability/pools/{id}/reserve`
- `POST /api/availability/pools/{id}/confirm/{reservationId}`
- `POST /api/availability/pools/{id}/cancel/{reservationId}`
- `POST /api/availability/pools/{id}/expire`
- `POST /api/availability/pools/{id}/capacity/increase`
- `POST /api/availability/pools/{id}/capacity/reduce`
- `GET /api/availability/check/{catalogEntryId}`

Najwazniejsze zastosowanie:
- tworzenie puli miejsc dla oferty
- robienie rezerwacji miekkich i twardych
- sprawdzanie czy sa wolne miejsca

### 4. TripSelection

Modul sklada plan atrakcji na wyjazd.

API:
- `POST /api/trip-selections/sessions`
- `GET /api/trip-selections/sessions/{id}`
- `POST /api/trip-selections/sessions/{id}/items`
- `DELETE /api/trip-selections/sessions/{id}/items/{catalogEntryId}`
- `POST /api/trip-selections/relations`
- `GET /api/trip-selections/relations`
- `GET /api/trip-selections/relations/{id}`
- `DELETE /api/trip-selections/relations/{id}`

Najwazniejsze zastosowanie:
- tworzenie sesji planowania
- dodawanie ofert do sesji
- sprawdzanie konfliktow i wymagan
- generowanie sugestii na podstawie relacji

## Architektura

Projekt ma klasyczny podzial:
- `Api`
- `Application`
- `Domain`
- `Infrastructure`

Kazdy modul ma wlasne:
- agregaty i value objecty w `Domain`
- DTO i serwisy aplikacyjne w `Application`
- kontrolery HTTP w `Api`
- repozytoria in-memory i adaptery cross-module w `Infrastructure`

Wspolne typy trafiaja do `src/Shared/PB.Shared`.

TripSelection komunikuje sie z Catalog i Availability przez porty (`ICatalogEntryQuery`, `IAvailabilityQuery`), a nie przez bezposrednie zaleznosci domenowe.

## Dane demo

Seeder laduje zestaw demo oparty o krakowskie i malopolskie atrakcje, m.in.:
- Wawel State Rooms
- Wawel Armoury
- Wawel Dragon's Den
- St. Mary's Basilica - Altar Visit
- St. Mary's Basilica - Tower Climb
- Sukiennice
- Kazimierz Walking Tour
- Wieliczka Tourist Route
- Wieliczka Miner's Route
- Tatra Mountains - Morskie Oko
- Zakrzowek

Tworzone sa tez:
- pakiet Wawelu
- wpisy katalogowe
- pule biletow
- relacje `Suggests`, `Excludes` i `Requires`

## Uruchomienie

Minimalnie potrzebne jest dzialajace srodowisko `.NET 8 SDK`.

Przykladowe uruchomienie:

```bash
dotnet run --project src/Bootstrapper/PB.Api/PB.Api.csproj
```

Swagger jest wlaczony przez `UseSwagger()` i `UseSwaggerUI()`.

## Struktura repo

```text
src/
  Bootstrapper/PB.Api
  Modules/
    AttractionDefinition/
    Catalog/
    Availability/
    TripSelection/
  Shared/PB.Shared
```

## Obecny charakter projektu

To jest aplikacja API-first z pamieciowymi repozytoriami. Najlepiej czytac ja jako:
- model domenowy atrakcji i pakietow
- katalog ofert z cennikiem
- prosty silnik dostepnosci
- prosty silnik planowania wyjazdu

Nie ma tu jeszcze persystencji do bazy ani autoryzacji. Najmocniejsza czescia projektu jest model przeplywu od komponentu, przez katalog i dostepnosc, do sesji planowania.
