# Raport: Seedowanie danych — Kopalnia Soli „Wieliczka"

## Co zaseedowano

### AttractionComponents (9)
| Klucz | Typ | Opis |
|---|---|---|
| `wieliczka-tourist-route` | AttractionDefinition | Trasa Turystyczna — Szyb Daniłowicza |
| `wieliczka-miner-route` | AttractionDefinition | Trasa Górnicza — Szyb Regis |
| `wieliczka-brine-tower` | AttractionDefinition | Tężnia Solankowa (naziemna, sezonowa) |
| `wieliczka-legends-trail` | AttractionDefinition | Śladami Legend (rodzinna) |
| `wieliczka-pilgrimage-route` | AttractionDefinition | Szlak Pielgrzymkowy |
| `wieliczka-hotel-stay` | AttractionDefinition | Nocleg (komponent pakietu) |
| `wieliczka-combo-tourist-miner` | AttractionPackage | Kombo Turystyczna + Górnicza |
| `wieliczka-family-combo` | AttractionPackage | Pakiet Rodzinny (Turystyczna + Śladami Legend) |
| `wieliczka-mine-hotel-pkg` | AttractionPackage | Pakiet Noclegowy (Turystyczna + Hotel) |

### CatalogEntries (13) — warianty tras
| Klucz | Komponent | Wariant |
|---|---|---|
| `wieliczka-tourist-individual` | tourist-route | Indywidualny, 1–14 os., 8 języków |
| `wieliczka-tourist-group` | tourist-route | Grupowy, 15–35 os. |
| `wieliczka-tourist-school` | tourist-route | Szkolny, 10–50 uczniów |
| `wieliczka-tourist-family` | tourist-route | Rodzinny (2+2–3) |
| `wieliczka-miner-individual` | miner-route | Indywidualny/mała grupa 4–15 os. |
| `wieliczka-miner-school` | miner-route | Edukacyjny szkolny (16+) |
| `wieliczka-brine-standard` | brine-tower | Standardowy (maj–wrz) |
| `wieliczka-brine-school` | brine-tower | Szkolny z pokazem edukacyjnym |
| `wieliczka-legends-family` | legends-trail | Rodzinny (dzieci 5–12 lat) |
| `wieliczka-pilgrimage-standard` | pilgrimage-route | Pielgrzymkowy (min. 15 os.) |
| `wieliczka-combo-tourist-miner` | pakiet kombo | Bilet łączony Turystyczna + Górnicza |
| `wieliczka-family-combo` | pakiet rodzinny | Turystyczna + Śladami Legend |
| `wieliczka-mine-hotel` | pakiet noclegowy | Zwiedzanie + nocleg |

### Cenniki — 3 okresy dla każdej trasy całorocznej
- **Niski sezon** (I–III 2026): ceny niższe (~10 PLN taniej)
- **Wysoki sezon** (IV–IX 2026): ceny pełne (np. 119 PLN indywidualny)
- **Poza sezonem** (X–XII 2026): jak niski sezon
- **Tężnia**: jeden okres (V–IX) — trasa sezonowa

### Zniżki w każdym PricingPeriod
- Bilet ulgowy (studenci, seniorzy 65+): −20 PLN
- Dzieci 4–16 lat: −50%
- Dzieci do lat 4: −100% (bezpłatny)
- Warianty grupowe i szkolne mają własne niższe ceny bazowe

### Ograniczenia (BookingConstraint) — przykłady modelowanych reguł
| Klucz | Typ | Wartość | Trasa |
|---|---|---|---|
| `min_age` | Min | 12 | Górnicza |
| `min_age` | Min | 16 | Górnicza szkolna |
| `fitness_required` | OneOf | "dobra_kondycja_fizyczna" | Górnicza |
| `equipment_provided` | OneOf | "kask", "lampa_górnicza" | Górnicza |
| `adult_guardian_required` | OneOf | "true" | Śladami Legend |
| `min_child_age` / `max_child_age` | Min / Max | 5 / 12 | Śladami Legend |
| `slot_interval_minutes` | OneOf | "30" / "60" | Turystyczna / Górnicza |
| `accessibility` | OneOf | "brak_wózków_inwalidzkich" | Turystyczna, Górnicza |
| `client_type` | OneOf | "szkoła", "pielgrzymka" | Szkolna, Pielgrzymkowa |
| `supervisor_count` | Min | 1 | Szkolna |
| `booking_days_ahead` | RequiredDaysAhead | 1–14 | Wszystkie |
| `adult_count` / `children_count` | Range | 2/2–3 | Rodzinny |
| `min_nights` | Min | 1 | Pakiet noclegowy |
| `guide_mandatory` | OneOf | "true" / "false" | Wszystkie tras |

### Relacje (10)
- Tourist → Miner: `Suggests`
- Tourist → Brine Tower: `Suggests`
- Tourist → Legends: `Suggests`
- Tourist → Pilgrimage: `Suggests`
- Miner → Tourist: `Requires` (prerequisite)
- Miner → Legends: `Excludes` (conflict wiekowy)
- Miner → Brine Tower: `Suggests`
- Legends → Brine Tower: `Suggests`
- Pilgrimage → Tourist: `Suggests`
- Combo (Turystyczna+Górnicza) → Pakiet Rodzinny: `Excludes`

---

## Co udało się odwzorować bez problemu

### 1. Różne lokalizacje startowe
`AttractionDefinition.Location` i `CatalogEntry.CatalogLocation` są niezależne.  
Trasa Turystyczna ma lokalizację `Szyb Daniłowicza`, a Trasa Górnicza `Szyb Regis` — mimo że obie są przy ul. Daniłowicza 10. Działa bez kompromisów.

### 2. Wiele wariantów tej samej trasy
`CatalogEntry` jest powiązany z `AttractionComponent` przez `AttractionComponentId` (Guid).  
Nic w modelu nie ogranicza liczby `CatalogEntry` do jednego komponentu.  
Trasa Turystyczna ma **4 osobne wpisy katalogowe** (indywidualny, grupowy, szkolny, rodzinny), każdy z własną ceną, godzinami, limitami i językami.

### 3. Sezonowe cenniki
`PricingPeriod` z nieaktualnymi zakresami dat — walidacja `Overlaps()` pilnuje spójności.  
Trzy okresy (I–III, IV–IX, X–XII) dla tras całorocznych działają poprawnie.

### 4. Zniżki w cennikach
`Discount` z `PercentOff` lub `AmountOff` + pole `Condition` (string) odwzorowuje bilety ulgowe, dziecięce i rodzinne. Model poprawnie obsługuje zarówno procentowe, jak i kwotowe zniżki.

### 5. Złożone ograniczenia na grupę
`BookingConstraint` z typem `Range`/`Min`/`Max`/`OneOf`/`RequiredDaysAhead` pozwala opisać:
- min/max liczebność grupy
- minimalny wiek
- wymagany opiekun
- wymagana sprawność fizyczna
- języki dostępne
- typ klienta (szkoła, pielgrzymka)
- wyposażenie dostarczone (kask, lampa)
- dostępność (bez wózków, schody)

Wiele ograniczeń na jedną ofertę — działa.

### 6. Pakiety jako bookable offer
`AttractionPackage` dziedziczny po `AttractionComponent` (i ma swoje `Id`).  
`CatalogEntry` może być stworzony z `AttractionComponentId = package.Id` — pakiet staje się osobną, zamawiową ofertą z własną ceną i ograniczeniami.  
Trzy pakiety (kombo, rodzinny, noclegowy) mają własne wpisy katalogowe i pule biletów.

### 7. Różne godziny i pojemności na wariant
Każdy `CatalogEntry` ma własne `OpeningHours` i `TicketPool.TotalCapacity`.  
Wariant szkolny ma inne godziny (8:00–16:00) niż indywidualny (7:30–19:30).

---

## Co wymagało workaroundów

### 1. Typy biletów (normalny / ulgowy / dziecko) w jednej ofercie
**Problem:** `PricingPeriod` ma jeden `Price` — cenę bazową (bilet normalny dla dorosłego).  
**Workaround:** Zniżki (`Discount`) z polem `Condition` (string) reprezentują ulgowy/dziecko.  
**Konsekwencja:** Absolutna cena biletu ulgowego nie jest przechowywana explicite — musi być wyliczona przez klienta. Pole `Condition` to dowolny string bez walidacji — "ulgowy", "wiek:4-16" są konwencją, nie kontraktem. Różne oferty mogą używać różnych konwencji i system nie wykryje niespójności.

### 2. Składy rodzinne (2+2, 2+3)
**Problem:** Model nie ma pojęcia "koszyk biletowy" z różnymi typami.  
**Workaround:** Osobna `CatalogEntry` rodzinna z ograniczeniami `adult_count` i `children_count` jako `Range` constraints. Cena jest jedną stawką per osoba (uśredniona). Wariant rodzinny nie może wyrazić "2 × cena dorosłego + 2 × cena dziecka = X PLN".

### 3. Wymagany opiekun — stosunek opiekun:dzieci
**Problem:** Reguła "1 opiekun na 15 uczniów" to relacja dwóch wartości.  
**Workaround:** `Constraint("Min", "supervisor_count", 1, null)` — tylko minimalna liczba opiekunów. Ratio (1:15) nie jest możliwy do wyrażenia bez pola obliczeniowego lub osobnej reguły.

### 4. Sezonowość godzin (lato vs. zima)
**Problem:** `CatalogOpeningHours` to jedno pole (open, close) na cały zakres katalogu.  
**Workaround:** Dwa osobne `CatalogEntry` dla różnych sezonów (nie zastosowano — zbyt rozwlekłe) lub przyjęcie stałych godzin szczytowych z komentarzem w opisie. Użyto drugiego podejścia.  
W rzeczywistości Wieliczka ma 7:30–19:30 (lato) vs. 8:00–18:00 (zima) — tego nie da się wyrazić jednym wpisem.

### 5. Dostępność wyposażenia i safety check jako warunek blokujący
**Problem:** `BookingConstraint` to dane — nie ma mechanizmu walidacji po stronie domeny.  
**Workaround:** `Constraint("OneOf", "fitness_required", null, null, "dobra_kondycja_fizyczna")` sygnalizuje wymaganie, ale system nie blokuje rezerwacji dla klienta niespełniającego warunku. Ograniczenia są odczytywane przez aplikację, nie egzekwowane przez domenę.

---

## Czego nie dało się odwzorować w obecnym modelu

### 1. Sloty czasowe (KRYTYCZNE)
**Rzeczywistość:** Wejścia na Trasę Turystyczną odbywają się co 30 minut. Każdy slot (np. 9:00, 9:30, 10:00...) ma własną pojemność (ok. 35 osób). Łącznie ~24 sloty dziennie.  
**Model:** `CatalogEntry.OpeningHours` to para (open, close) — jedno okno dzienne. `TicketPool` ma jedną pulę na cały dzień bez podziału na sloty.  
**Konsekwencja:** Nie można zarezerwować konkretnej godziny wejścia. Nie można pilnować limitu per slot. System traktuje całą dobę jako jedną dostępność — po 35 rezerwacjach "slot 9:00 jest pełny" model nie wie, że sloty 9:30, 10:00 wciąż wolne.  
**Obejście (niepraktyczne):** Jeden `CatalogEntry` + `TicketPool` per slot (25 dziennie × 365 dni = tysiące wpisów). Model tego nie przewiduje.

### 2. Pojemność per slot vs. pojemność dzienna
Wynikowe z punktu 1. Zbiorcza pojemność dzienna (np. 800 biletów) i pojemność slotowa (35/slot) to dwa różne poziomy granulacji. Model ma tylko jeden `TotalCapacity` per `TicketPool`, co nie oddaje rzeczywistości.

### 3. Ceny absolutne dla każdego typu biletu
**Rzeczywistość:** Kasa wystawia bilety z ceną: normalny 119 PLN, ulgowy 99 PLN, dziecko 59 PLN — każdy z osobną, wydrukowaną kwotą.  
**Model:** Jedna cena bazowa + lista zniżek (procent lub kwota od bazy). Nie można przechowywać "bilet ulgowy = 99 PLN" bez wyliczania. Model nie ma `TicketType` jako osobnego bytu.

### 4. Różne języki mają różną dostępność slotów
**Rzeczywistość:** Trasa po angielsku odbywa się 4×/dzień, po polsku 12×/dzień, po chińsku 1×/dzień (na życzenie).  
**Model:** Język to `OneOf` constraint — lista dozwolonych wartości. Nie ma powiązania język → dostępne godziny → pojemność. Nie można modelować "język X dostępny tylko o 10:00 i 14:00".

### 5. Zmienne harmonogramy w zależności od dnia tygodnia
**Rzeczywistość:** W poniedziałki Trasa Górnicza może być niedostępna. W weekendy więcej slotów na Trasę Turystyczną.  
**Model:** `OpeningHours` jest jeden dla całego `DateRange`. Brak pola `DayOfWeek` ani harmonogramu tygodniowego.

### 6. Odpust i Msza pod ziemią jako osobna rezerwacja w ramach Szlaku Pielgrzymkowego
**Rzeczywistość:** Zamówienie Mszy wymaga osobnej rezerwacji z duszpasterzem, oddzielnej opłaty lub braku opłaty.  
**Model:** Nie ma sub-rezerwacji ani warunkowo dołączanych usług w ramach jednej oferty. Zasymulowano to przez opis w polu `Description`.

### 7. Reguła "Trasa Górnicza wymaga wcześniejszego zapoznania z kopalnią" jako twarda blokada
**Rzeczywistość:** Kasjer odmawia sprzedaży biletu na Trasę Górniczą bez wcześniejszej Trasy Turystycznej.  
**Model:** `RelationType.Requires` w `AttractionRelation` — to sugestia/informacja w module TripSelection, nie twardy warunek blokujący rezerwację w module Availability. `TicketPool.Reserve()` nie sprawdza relacji między wpisami katalogowymi.

---

## Podsumowanie oceny modelu

| Wymiar | Ocena |
|---|---|
| Wiele wariantów jednej trasy | ✅ Działa natywnie |
| Różne lokalizacje startowe | ✅ Działa natywnie |
| Sezonowe cenniki | ✅ Działa natywnie |
| Zniżki (ulgowe, dziecięce) | ⚠️ Workaround przez Discount.Condition (string) |
| Min/max grupy, ograniczenia wiekowe | ✅ Działa przez BookingConstraint |
| Pakiety bookable jako jedna oferta | ✅ Działa przez CatalogEntry → AttractionPackage.Id |
| Różne pojemności per wariant | ✅ Osobny TicketPool per CatalogEntry |
| Sloty czasowe (30-min wejścia) | ❌ Brak pojęcia slotu — krytyczny gap |
| Ceny absolutne per typ biletu | ❌ Brak TicketType — tylko baza + procent |
| Harmonogram dzienny (godziny per dzień tygodnia) | ❌ Jedno okno czasowe na cały DateRange |
| Blokada twarda między ofertami (Requires egzekwowane) | ❌ Requires to tylko dane, nie logika |
| Dostępność per język | ❌ Brak powiązania język → sloty |
