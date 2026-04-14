# Workflow projektu

Ten dokument opisuje, jak system jest uzywany od poczatku do konca na poziomie biznesowym i API.

## 1. Utworzenie komponentu atrakcji

Pierwszy krok to stworzenie `AttractionComponent` typu `attraction`.

Przyklad:
- admin dodaje pojedyncza atrakcje, np. `Wieliczka Salt Mine - Tourist Route`
- ustawia nazwe i opis
- dodaje tagi
- ustawia lokalizacje
- ustawia godziny otwarcia

Przyklad payload:

```json
{
  "type": "attraction",
  "name": "Wieliczka Salt Mine - Tourist Route",
  "description": "Classic underground route through the mine.",
  "tags": [
    { "name": "mine", "group": "type" },
    { "name": "wieliczka", "group": "loc" }
  ],
  "location": {
    "city": "Wieliczka",
    "address": "ul. Daniłowicza 10",
    "latitude": 49.9833,
    "longitude": 20.0553
  },
  "openingHours": {
    "open": "08:00:00",
    "close": "17:00:00"
  },
  "selectionRule": null,
  "componentIds": null
}
```

Endpoint:

```text
POST /api/attraction-components
```

## 2. Utworzenie pakietu

Jesli kilka atrakcji ma byc traktowanych jako zestaw, tworzy sie komponent typu `package`.

Pakiet:
- ma nazwe i opis
- ma `SelectionRule`
- przechowuje liste `ComponentIds`

Przyklad:
- pakiet Wawelu grupuje `State Rooms`, `Armoury` i `Dragon's Den`
- regula `PickN(2)` oznacza, ze pakiet wymaga wybrania 2 z 3 elementow

Przyklad payload:

```json
{
  "type": "package",
  "name": "Wawel Full Experience",
  "description": "Pick any 2 of the 3 Wawel attractions.",
  "tags": [],
  "location": null,
  "openingHours": null,
  "selectionRule": {
    "type": "PickN",
    "count": 2
  },
  "componentIds": [
    "component-id-1",
    "component-id-2",
    "component-id-3"
  ]
}
```

Endpoint:

```text
POST /api/attraction-components
```

Mozna tez dopinac komponenty do istniejącego pakietu:

```text
POST /api/attraction-components/{id}/components/{componentId}
DELETE /api/attraction-components/{id}/components/{componentId}
```

## 3. Wystawienie oferty w katalogu

Komponent sam w sobie nie jest jeszcze sprzedawalna oferta. Zeby stal sie dostepny dla klienta, trzeba zalozyc `CatalogEntry`.

Katalogowa oferta:
- wskazuje `AttractionComponentId`
- ma wlasny `DateRange`
- ma status
- ma opcjonalne `OpeningHours`
- moze miec ograniczenia rezerwacji

Przyklad payload:

```json
{
  "attractionComponentId": "component-id",
  "name": "Wieliczka Salt Mine - Tourist Route",
  "description": "Classic 3.5km underground tour.",
  "tags": [
    { "name": "mine", "group": "type" },
    { "name": "guided-tour", "group": "type" }
  ],
  "location": {
    "city": "Wieliczka",
    "address": "ul. Daniłowicza 10"
  },
  "dateRange": {
    "from": "2026-04-01",
    "to": "2026-06-30"
  },
  "openingHours": {
    "open": "08:00:00",
    "close": "17:00:00"
  },
  "isEvent": false,
  "constraints": [
    {
      "type": "Range",
      "key": "group_size",
      "minValue": 1,
      "maxValue": 35,
      "allowedValues": []
    }
  ]
}
```

Endpoint:

```text
POST /api/catalog/entries
```

## 4. Dodanie cennika

Po utworzeniu wpisu katalogowego trzeba dodac przynajmniej jeden `PricingPeriod`, jesli oferta ma miec cene.

Przyklad payload:

```json
{
  "from": "2026-04-01",
  "to": "2026-06-30",
  "price": {
    "amount": 119,
    "currency": "PLN"
  },
  "discounts": []
}
```

Endpoint:

```text
POST /api/catalog/entries/{id}/pricing
```

W praktyce wpis katalogowy moze miec kilka okresow cenowych. System pilnuje, zeby sie nie nachodzily.

## 5. Utworzenie puli biletow

Gdy oferta ma byc rezerwowalna, zaklada sie `TicketPool`.

Pula:
- jest przypieta do `CatalogEntryId`
- okresla laczna liczbe miejsc
- przechowuje rezerwacje

Przyklad payload:

```json
{
  "catalogEntryId": "catalog-entry-id",
  "totalCapacity": 70
}
```

Endpoint:

```text
POST /api/availability/pools
```

Od tego momentu mozna:
- sprawdzic dostepnosc
- robic rezerwacje
- potwierdzac rezerwacje
- zwiekszac lub zmniejszac pojemnosc

Najwazniejsze endpointy:

```text
GET  /api/availability/check/{catalogEntryId}
POST /api/availability/pools/{id}/reserve
POST /api/availability/pools/{id}/confirm/{reservationId}
POST /api/availability/pools/{id}/cancel/{reservationId}
POST /api/availability/pools/{id}/expire
```

## 6. Definiowanie relacji miedzy komponentami

Przed skladaniem planu mozna zdefiniowac relacje biznesowe pomiedzy komponentami.

Przyklad typow:
- `Suggests`: komponent sugeruje inny
- `Excludes`: komponent wyklucza inny
- `Requires`: komponent wymaga innego

Przyklad payload:

```json
{
  "sourceComponentId": "component-id-a",
  "targetComponentId": "component-id-b",
  "type": "Requires",
  "context": "prerequisite",
  "description": "Miner's Route requires Tourist Route in the same trip."
}
```

Endpoint:

```text
POST /api/trip-selections/relations
```

## 7. Zalozenie sesji planowania

Uzytkownik zaczyna od utworzenia `SelectionSession`.

Payload:

```json
{
  "destinationCity": "Krakow",
  "travelFrom": "2026-04-10",
  "travelTo": "2026-04-14",
  "groupSize": 4
}
```

Endpoint:

```text
POST /api/trip-selections/sessions
```

## 8. Dodawanie oferty do sesji

Do sesji dodaje sie konkretny `CatalogEntryId`, nie sam komponent.

Payload:

```json
{
  "catalogEntryId": "catalog-entry-id"
}
```

Endpoint:

```text
POST /api/trip-selections/sessions/{id}/items
```

Przy dodaniu system wykonuje nastepujacy flow:

1. Pobiera wpis katalogowy.
2. Sprawdza dostepnosc biletow przez Availability.
3. Waliduje constrainty z katalogu.
4. Pobiera relacje dla `AttractionComponentId`.
5. Sprawdza konflikty i wymagania wzgledem juz dodanych pozycji.
6. Dodaje nowa pozycje do `MustHaveItems`.
7. Buduje `OptionalSuggestions` przez relacje `Suggests`.
8. Ustawia `ExcludedIds` przez relacje `Excludes`.
9. Zwraca aktualny stan sesji.

W odpowiedzi sesja zawiera:
- `MustHaveItems`
- `OptionalSuggestions`
- `ExcludedIds`
- `Issues`

## 9. Usuwanie oferty z sesji

Usuniecie odbywa sie po `CatalogEntryId`.

Endpoint:

```text
DELETE /api/trip-selections/sessions/{id}/items/{catalogEntryId}
```

## 10. Typowy scenariusz end-to-end

Przyklad:

1. Admin tworzy atrakcje `Wieliczka Tourist Route`.
2. Admin tworzy atrakcje `Wieliczka Miner's Route`.
3. Admin zaklada relacje `Requires` od `Miner's Route` do `Tourist Route`.
4. Admin wystawia obie atrakcje w katalogu na kwiecien-czerwiec.
5. Admin zaklada pule biletow dla obu ofert.
6. Uzytkownik tworzy sesje wyjazdu do Krakowa.
7. Uzytkownik dodaje `Miner's Route`.
8. System sprawdza bilety i constrainty.
9. System wykrywa brak wymaganego komponentu `Tourist Route` i zwraca issue `RequirementMissing`.
10. Uzytkownik dodaje tez `Tourist Route`.
11. Sesja przechodzi do stanu spojnego.

## 11. Co jest zrodlem prawdy w systemie

Przy czytaniu kodu warto trzymac ten podzial:
- `AttractionComponent`: czym jest atrakcja lub pakiet
- `CatalogEntry`: kiedy i na jakich warunkach da sie to kupic
- `TicketPool`: ile miejsc realnie zostalo
- `SelectionSession`: co uzytkownik probuje ulozyc na wyjazd

Ten podzial jest glownym flow calej aplikacji.
