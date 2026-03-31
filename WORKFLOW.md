# Jak dzialaja moduly - opis i workflow

## Ogolna idea

System pozwala na pelny cykl zycia atrakcji turystycznej: od stworzenia jej "przepisu" (definicji), przez wystawienie w katalogu z cenami i datami, zarzadzanie biletami, az po dobieranie atrakcji do wycieczki z walidacja czy sie nie wykluczaja.

---

## Modul 1: AttractionDefinition

### Cel

Stworzenie **archetypu** atrakcji - definicji, ktora jest "przepisem" na produkt. To nie jest konkretna oferta w sklepie, to szablon z ktorego pozniej powstaja oferty (CatalogEntry).

### Jak dziala

Admin tworzy definicje atrakcji podajac:
- **Nazwe i opis** (np. "Wawel")
- **Tagi** - dowolne etykiety kategoryzujace atrakcje (np. typ:landmark, tematyka:historia, loc:krakow). Tagow moze byc duzo - to glowny mechanizm kategoryzacji zamiast sztywnych enumow
- **Lokalizacje** - miasto, adres, opcjonalnie wspolrzedne GPS
- **Godziny otwarcia** i **dostepnosc sezonowa** (np. tylko lato)

### Warianty

Jedna atrakcja moze miec wiele **wariantow**. Przyklad Wawelu:
- Zbrojownia (45 min, grupy 1-15 osob)
- Podziemia (60 min, grupy 1-10, trzeba rezerwowac 3 dni wczesniej)
- Pietra I i II (90 min, grupy 1-20, dostepne po polsku i angielsku)

Kazdy wariant ma wlasne:
- Dodatkowe tagi
- Constrainty (ograniczenia) - np. rozmiar grupy, ile dni wczesniej trzeba zarezerwowac, w jakich jezykach dostepne
- Czas trwania

### Pakiety

`AttractionPackage` pozwala grupowac atrakcje/warianty. Np. "Wawel kompletny - wybierz 2 z 3 wariantow". Pakiet ma `SelectionRule`:
- **All** - trzeba wziac wszystko z pakietu
- **PickN(n)** - wybierasz n elementow z dostepnych

### Composite Pattern

Definicja i Pakiet dziedzicza po wspolnej abstrakcji `AttractionComponent`. Dzieki temu reszta systemu (i przyszly modul planowania trasy) moze traktowac je jednolicie - nie musi wiedziec czy operuje na pojedynczej atrakcji czy na pakiecie.

### Flaga IsComplete

Definicja nie ma stanow (Draft/Published/Archived). Zamiast tego ma flage `IsComplete` - jest ustawiona kiedy definicja ma nazwe, przynajmniej jeden tag i lokalizacje. To informacja dla admina "ta definicja jest gotowa do uzycia w katalogu".

### Przyklad uzycia

```
1. Admin tworzy AttractionDefinition "Wawel"
2. Dodaje tagi: typ:landmark, tematyka:historia, tematyka:kultura
3. Ustawia lokalizacje: Krakow, Wawel 5
4. Dodaje wariant "Zbrojownia" z constraintem group_size Range 1-15
5. Dodaje wariant "Podziemia" z constraintem RequiredDaysAhead(3)
6. Tworzy AttractionPackage "Wawel 2 z 3" z regula PickN(2)
7. Dodaje do pakietu ID wariantow: zbrojownia, podziemia, pietra
-> Definicja jest kompletna (IsComplete = true), gotowa do katalogu
```

---

## Modul 2: Catalog

### Cel

Stworzenie **konkretnej instancji** atrakcji dostepnej w okreslonym czasie z konkretnymi cenami. O ile definicja to "przepis" (Wawel jako atrakcja), to CatalogEntry to "oferta" (Wawel - Zbrojownia, lato 2025, bilet 30 PLN).

### Jak dziala

Admin tworzy CatalogEntry podajac:
- **Referencje do definicji** (AttractionDefinitionId) i opcjonalnie do wariantu (VariantId)
- **Nazwe i opis** konkretnej oferty
- **Tagi** - kopiowane z definicji + ewentualnie dodatkowe
- **Lokalizacje**
- **Zakres dat** - kiedy ta oferta obowiazuje (np. 2025-06-01 do 2025-08-31)
- **Czy to event** (jednorazowy, np. koncert) czy stala atrakcja (np. muzeum)
- **Godziny otwarcia**

### Dynamiczny cennik

Kluczowa funkcja katalogu - ceny moga sie zmieniac w czasie. CatalogEntry ma liste `PricingPeriod`, kazdy z:
- Zakres dat (np. 01.06-30.06)
- Cena (np. 30 PLN)
- Znizki (np. studenci -50%, dzieci za darmo)

Przyklad:
```
Wawel - Zbrojownia, lato 2025:
  01.06 - 30.06:  30 PLN  (student: -50%)
  01.07 - 31.07:  35 PLN
  01.08 - 31.08:  35 PLN  (dziecko: za darmo)
```

Okresy nie moga na siebie nachodzic. Mozna sprawdzic cene na konkretny dzien metoda `GetPriceForDate(date)`.

### Stany instancji

CatalogEntry MA stany (w przeciwienstwie do definicji):
- **Available** - dostepna, mozna rezerwowac
- **SoldOut** - wyprzedana
- **Cancelled** - anulowana (nie mozna juz updatowac)
- **Upcoming** - zapowiedziana, jeszcze nie dostepna

### Wyszukiwanie

Catalog pozwala szukac po: miescie, zakresie dat, tagach, statusie. Dzieki temu mozna latwo znalezc "co jest dostepne w Krakowie w marcu z tagiem historia".

### Przyklad uzycia

```
1. Admin tworzy CatalogEntry z AttractionDefinitionId = wawel_id, VariantId = zbrojownia_id
2. Ustawia nazwe "Wawel - Zbrojownia (lato 2025)"
3. Ustawia zakres dat: 2025-06-01 do 2025-08-31
4. Dodaje PricingPeriod: 2025-06-01..2025-06-30, cena 30 PLN, znizka student 50%
5. Dodaje PricingPeriod: 2025-07-01..2025-08-31, cena 35 PLN
6. Status: Available
-> Oferta widoczna w katalogu, mozna na nia rezerwowac bilety
```

---

## Modul 3: Availability

### Cel

Zarzadzanie **pulami biletow** organizatora. Organizator kupuje pewna ilosc biletow na dana atrakcje (CatalogEntry) i chce zarzadzac ich dostepnoscia - ile zostalo, kto zarezerwowl, czy rezerwacja jest potwierdzona czy jeszcze miekka.

### Jak dziala

Admin tworzy `TicketPool` powiazany z CatalogEntry:
- **Pojemnosc** (TotalCapacity) - ile biletow lacznie
- Opcjonalnie powiazanie z wariantem (VariantId)

### Rezerwacje

System obsluguje dwa typy rezerwacji:
- **Pending (miekka)** - wstepna rezerwacja z czasem wygasniecia. Jesli nie zostanie potwierdzona przed ExpiresAt, automatycznie wygasa
- **Confirmed (twarda)** - potwierdzona, bilety naleza do rezerwujacego

Cykl zycia rezerwacji:
```
Reserve(ilosc, expiresAt)  ->  Pending
                                 |
                    +------------+------------+
                    |            |            |
              Confirm()     Cancel()    ExpireOutdated()
                    |            |            |
                    v            v            v
               Confirmed    Cancelled     Expired
```

### Pojemnosc

TicketPool dynamicznie oblicza:
- **PendingCount** - ile biletow w miekkich rezerwacjach
- **ConfirmedCount** - ile potwierdzonych
- **AvailableCount** = TotalCapacity - PendingCount - ConfirmedCount
- **IsAvailable** - czy sa wolne bilety

Admin moze zwiekszyc lub zmniejszyc pojemnosc (ale nie ponizej aktywnych rezerwacji).

### Przyklad uzycia

```
1. Admin tworzy TicketPool: CatalogEntryId = zbrojownia_lato_id, TotalCapacity = 100
2. Klient rezerwuje 5 biletow (Pending, wygasa za 30 min)
   -> AvailableCount: 95, PendingCount: 5
3. Klient potwierdza rezerwacje
   -> AvailableCount: 95, ConfirmedCount: 5
4. Inny klient rezerwuje 3 bilety ale nie potwierdza w czasie
   -> system wywoluje ExpireOutdated() -> rezerwacja wygasa
   -> AvailableCount wraca do 95
5. Admin zwieksza pojemnosc o 50
   -> TotalCapacity: 150, AvailableCount: 145
```

---

## Modul 4: TripSelection

### Cel

Budowanie **sesji doboru atrakcji** do wycieczki. Uzytkownik dodaje atrakcje z katalogu do sesji, a modul pilnuje zeby:
1. Wybrane atrakcje nie wykluczaly sie nawzajem
2. Wymagane zaleznosci byly spelnione
3. Bilety byly dostepne
4. Proponowal powiazane atrakcje

Wynik to dwie listy: **must-have** (to co uzytkownik explicite dodal + wymagane) i **optional** (sugestie).

### Relacje miedzy atrakcjami

Admin definiuje relacje miedzy atrakcjami/wariantami:

| Typ | Co robi | Przyklad |
|---|---|---|
| **Excludes** | Dodanie A wyklucza B | Audiobook po polsku wyklucza audiobook po angielsku (ta sama tresc, inny jezyk) |
| **Suggests** | Dodanie A sugeruje B | Zwiedzanie Wawelu sugeruje Sukiennice (blisko, te same tagi) |
| **Requires** | Dodanie A wymaga B | Wawel Pietra II wymaga biletu na Pietra I (trzeba isc kolejno) |
| **Replaces** | A jest alternatywa dla B | Pelny pakiet Wawelu zastepuje pojedyncze warianty |

### Sesja doboru

Uzytkownik tworzy `SelectionSession` podajac:
- Miasto docelowe
- Daty podrozy (od-do)
- **Wielkosc grupy** (GroupSize) - uzywana do walidacji constraintow

Potem dodaje atrakcje z katalogu. Przy kazdym dodaniu system:

1. **Sprawdza dostepnosc biletow** - pyta modul Availability czy sa wolne bilety; brak biletow **blokuje** dodanie
2. **Waliduje constrainty** z CatalogEntry - hard constraints (group_size, booking_days_ahead) **blokuja** dodanie rzucajac DomainException; OneOf (np. jezyk) zwraca soft issue - atrakcja dodana, uzytkownik wybiera przy bookingu
3. **Waliduje relacje** - czy nowa atrakcja nie wyklucza istniejacych (i na odwrot), czy wymagane zaleznosci sa spelnione
4. **Generuje sugestie** - na podstawie relacji SUGGESTS, szuka odpowiednich CatalogEntry
5. **Generuje wykluczenia** - co trzeba wykluczyc z przyszlych propozycji

### Dwie listy

- **MustHaveItems** - atrakcje ktore uzytkownik explicite dodal + te wymagane przez relacje REQUIRES
- **OptionalSuggestions** - propozycje z relacji SUGGESTS (jesli sa dostepne w katalogu i maja bilety)
- **ExcludedIds** - co jest wykluczone i nie powinno byc proponowane
- **Issues** - lista miekkich problemow (konflikty relacji, OneOf do wyboru przy bookingu) - NIE zawiera hard constraint violations (te blokuja dodanie)

### Cross-module komunikacja

TripSelection nie importuje bezposrednio klas z Catalog i Availability. Zamiast tego definiuje wlasne interfejsy (porty):
- `ICatalogEntryQuery` - "daj mi info o tej atrakcji z katalogu"
- `IAvailabilityQuery` - "czy sa bilety na ta atrakcje?"

W warstwie Infrastructure sa **adaptery** ktore implementuja te porty delegujac do prawdziwych serwisow Catalog i Availability. Dzieki temu domena TripSelection jest niezalezna.

### Przyklad uzycia (z seedowanych danych)

```
1. Admin zdefiniowl relacje:
   - Wawel State Rooms  SUGGESTS  Wawel Armoury
   - Wawel Armoury      SUGGESTS  Dragon's Den
   - Wieliczka           EXCLUDES  Tauron Concert  (caly dzien, nie da sie obu)
   - Kazimierz Tour      SUGGESTS  Pijalnia Wodki i Piwa
   - St. Mary's          SUGGESTS  Sukiennice

2. Uzytkownik tworzy sesje: Krakow, 10-14.04.2026, grupa 4 osoby

3. Dodaje "Kazimierz Walking Tour" do sesji:
   -> Sprawdza bilety: dostepne (OK)
   -> Sprawdza constrainty: group_size Range(2,12) - grupa 4 OK
   -> Constraint OneOf language -> issue informacyjny
   -> Relacja SUGGESTS -> sugeruje Pijalnie

   Wynik:
     MustHave: [Kazimierz Tour]
     Optional: [Pijalnia Wodki i Piwa]
     Issues: ["requires choosing: polish, english (for 'language')"]

4. Dodaje "Wieliczka Salt Mine" do sesji:
   -> Constrainty: group_size Range(1,35) OK, booking_days_ahead 3 -> OK (10 dni)
   -> Relacja EXCLUDES Tauron Concert -> dodaje do ExcludedIds

5. Dodaje "Tauron Concert" do sesji:
   -> System wykrywa CONFLICT: Wieliczka juz w sesji wyklucza Tauron
   -> Issue: [Conflict] "Wieliczka in your selection conflicts with Tauron"
   -> Atrakcja JEST dodana do must-have ale z ostrzezeniem (conflict to soft issue)

6. Scenariusz z naruszeniem constraintow (grupa 20):
   -> Dodaje Wawel State Rooms (group_size max 15):
   -> DomainException: "allows max group size 15 (your group: 20)" -> atrakcja NIE jest dodana

7. Scenariusz z OneOf (grupa 4, poprawna):
   -> Dodaje Wawel State Rooms (group_size OK, booking OK, language OneOf):
   -> Atrakcja DODANA, issues: ["requires choosing: polish, english, german (for 'language')"]
   -> Uzytkownik wybierze jezyk przy faktycznym bookingu
```

---

## Pelny workflow end-to-end

```
FAZA 1: DEFINICJA (admin, jednorazowo)
=========================================
AttractionDefinition
  |
  |-- Tworzy definicje "Wawel"
  |-- Dodaje tagi, lokalizacje
  |-- Dodaje warianty: Zbrojownia, Podziemia, Pietra
  |-- Tworzy pakiet "Wawel 2 z 3" (PickN)
  |-- IsComplete = true


FAZA 2: KATALOG (admin, per sezon/event)
=========================================
Catalog
  |
  |-- Tworzy CatalogEntry "Wawel Zbrojownia lato 2025"
  |     z AttractionDefinitionId + VariantId
  |-- Ustawia daty: 01.06-31.08
  |-- Dodaje cennik: czerwiec 30 PLN, lipiec-sierpien 35 PLN
  |-- Status: Available
  |
  |-- To samo dla kazdego wariantu/eventu


FAZA 3: BILETY (admin/organizator)
=========================================
Availability
  |
  |-- Tworzy TicketPool dla kazdego CatalogEntry
  |-- Ustawia pojemnosc (np. 100 biletow)
  |-- Gotowe do rezerwacji


FAZA 4: RELACJE (admin, jednorazowo)
=========================================
TripSelection (czesc administracyjna)
  |
  |-- Definiuje relacje:
  |     Zbrojownia PL EXCLUDES Zbrojownia EN
  |     Zbrojownia SUGGESTS Podziemia
  |     Pietra II REQUIRES Pietra I


FAZA 5: DOBOR ATRAKCJI (uzytkownik)
=========================================
TripSelection (czesc uzytkownika)
  |
  |-- Tworzy sesje: Krakow, 10-14.04, grupa 4 osoby
  |-- Dodaje atrakcje z katalogu
  |     -> system waliduje constrainty (group_size, booking_days_ahead, language)
  |     -> waliduje relacje (excludes, requires)
  |     -> generuje sugestie i wykluczenia
  |-- Dostaje dwie listy:
  |     MustHave: [to co dodal + wymagane]
  |     Optional: [sugestie]
  |-- Issues: lista problemow (constraint violations, konflikty)
  |-- Moze iterowac: dodawac, usuwac, dopoki jest zadowolony


(PRZYSZLOSC: Modul planowania trasy)
=========================================
  |-- Bierze MustHave + wybrane Optional
  |-- Planuje optymalna trase (kolejnosc, godziny, transport)
  |-- To jest poza obecnym zakresem
```
