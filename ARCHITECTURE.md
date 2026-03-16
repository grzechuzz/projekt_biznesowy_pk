# Architektura systemu — Wycinek: Zarządzanie atrakcjami i dobieranie listy

## Podział na 4 moduły

### 1. `AttractionDefinition` — Definicje atrakcji (Draft / Archetyp)
**Odpowiedzialność:** Tworzenie i zarządzanie *przepisami* na atrakcje — czyli ich definicjami/archetypami, zanim trafią do katalogu.

- Atrakcja w stanie **Draft** — szablon z metadanymi (nazwa, opis, kategoria, godziny otwarcia, dostępność sezonowa, lokalizacja).
- **Composite pattern** — `AttractionComponent` to abstrakcja, pod którą kryje się zarówno pojedyncza atrakcja (`SingleAttraction`), jak i grupa atrakcji (`AttractionGroup`). Dzięki temu reszta systemu nie musi wiedzieć, czy operuje na jednej atrakcji czy na pakiecie.
- Stany: `Draft` → `Published` → `Archived`.
- Tylko atrakcja w stanie `Published` może trafić do katalogu.

**Uzasadnienie:** Prowadzący podkreślał: *"nie mieszać definicji z instancją"* oraz *"draft to przepis na elementy z kategorii"*. Ten moduł to właśnie warstwa definicji.

---

### 2. `Catalog` — Katalog ofert
**Odpowiedzialność:** Przechowywanie *instancji* atrakcji dostępnych w konkretnym czasie — to, co użytkownik faktycznie może zobaczyć/odwiedzić.

- Na podstawie opublikowanej definicji tworzona jest `CatalogEntry` — konkretna instancja z datami obowiązywania (np. "Wawel — sezon zimowy 2025/26", "Mecz Wisła vs Cracovia 18.03.2026").
- Filtrowanie po: lokalizacji (miasto), datach, porze roku, kategorii.
- Eventy (mecz, koncert) to instancje z jednorazową datą; zabytki to instancje z zakresem dat/sezonem.

**Uzasadnienie:** *"Po drafcie coś trafia do katalogu, katalog różni się tym że mamy instancje i jest dostępny dla uczestników"*. Katalog to żywa oferta — zależna od czasu.

---

### 3. `Preference` — Preferencje użytkownika
**Odpowiedzialność:** Zbieranie i przechowywanie preferencji podróżnika.

- Kategorie atrakcji, które lubi (zabytki, sport, piwo/gastronomia, kultura…).
- Parametry podróży: miasto docelowe, daty, ile chce chodzić (mało/średnio/dużo), transport (pieszo, rower, samochód, komunikacja miejska).
- Budżet czasowy (ile godzin dziennie chce zwiedzać).

**Uzasadnienie:** Bez preferencji nie da się dobrać atrakcji. To osobny kontekst — nie dotyczy samych atrakcji, tylko tego, czego chce użytkownik.

---

### 4. `TripSelection` — Dobieranie listy atrakcji
**Odpowiedzialność:** Na podstawie preferencji i katalogu generuje **dwie listy**:
- **Must-have** — atrakcje, które zdecydowanie pasują do preferencji i są dostępne w podanych datach.
- **Optional** — atrakcje, które częściowo pasują lub są warte rozważenia.

- Odpytuje katalog o dostępne atrakcje (lokalizacja + daty).
- Filtruje/rankinguje po zgodności z preferencjami użytkownika.
- **Nie planuje trasy** — zwraca tylko dobrane miejsca w dwóch listach.

**Uzasadnienie:** Prowadzący wielokrotnie podkreślał *"dwie listy — optional i must-have"*. Ten moduł to serce wycinka — łączy preferencje z katalogiem i produkuje wynik.

---

## Architektura — Ports & Adapters

Każdy moduł ma 4 warstwy:

```
┌─────────────────────────────────────────────────────────┐
│                        Api                              │  ← Kontrolery REST, DTOs request/response
├─────────────────────────────────────────────────────────┤
│                    Application                          │  ← Serwisy aplikacyjne (orchestrator)
├─────────────────────────────────────────────────────────┤
│                      Domain                             │  ← Encje, Value Objects, porty (interfejsy repo)
├─────────────────────────────────────────────────────────┤
│                   Infrastructure                        │  ← Implementacje portów (repozytoria in-memory / DB)
└─────────────────────────────────────────────────────────┘
```

**Zasady zależności:**
- `Domain` → nie zależy od niczego (zero referencji do innych warstw i zewnętrznych bibliotek)
- `Application` → zależy od `Domain`
- `Infrastructure` → zależy od `Domain` (implementuje porty)
- `Api` → zależy od `Application` (i pośrednio `Domain`)
- `Infrastructure` jest rejestrowana przez DI w `Api` — tak żeby po podpięciu bazy wystarczyło podmienić implementację

```
Api ──────► Application ──────► Domain ◄────── Infrastructure
                                  ▲                   │
                                  │    implementuje    │
                                  └───── interfejsy ───┘
```

## Composite Pattern (moduł AttractionDefinition)

```
          AttractionComponent (abstract)
           /                    \
  SingleAttraction          AttractionGroup
                            (lista AttractionComponent)
```

Dzięki temu `AttractionGroup` może zawierać inne grupy lub pojedyncze atrakcje — prowadzący mówił: *"wzorzec pozwala na to żeby nie musiał wiedzieć czy mam do czynienia z grupą atrakcji czy z jedną atrakcją"*.

## Stany atrakcji (moduł AttractionDefinition)

```
  Draft ──publish()──► Published ──archive()──► Archived
```

- **Draft** — edytowalny szablon
- **Published** — gotowy do utworzenia instancji w katalogu
- **Archived** — wycofany

## Przepływ danych między modułami

```
1. Użytkownik tworzy definicję atrakcji (Draft) → AttractionDefinition
2. Publikuje ją (Published) → AttractionDefinition
3. Na podstawie opublikowanej definicji tworzy wpis w katalogu → Catalog
4. Użytkownik podaje swoje preferencje → Preference
5. System dobiera atrakcje → TripSelection odpytuje Catalog + Preference
6. Zwraca dwie listy: must-have i optional → TripSelection
```

## Struktura solucji

```
PB.sln
├── src/
│   ├── Bootstrapper/
│   │   └── PB.Api/                          ← Host ASP.NET, rejestruje wszystkie moduły
│   │
│   ├── Modules/
│   │   ├── AttractionDefinition/
│   │   │   ├── PB.Modules.AttractionDefinition.Api/
│   │   │   ├── PB.Modules.AttractionDefinition.Application/
│   │   │   ├── PB.Modules.AttractionDefinition.Domain/
│   │   │   └── PB.Modules.AttractionDefinition.Infrastructure/
│   │   │
│   │   ├── Catalog/
│   │   │   ├── PB.Modules.Catalog.Api/
│   │   │   ├── PB.Modules.Catalog.Application/
│   │   │   ├── PB.Modules.Catalog.Domain/
│   │   │   └── PB.Modules.Catalog.Infrastructure/
│   │   │
│   │   ├── Preference/
│   │   │   ├── PB.Modules.Preference.Api/
│   │   │   ├── PB.Modules.Preference.Application/
│   │   │   ├── PB.Modules.Preference.Domain/
│   │   │   └── PB.Modules.Preference.Infrastructure/
│   │   │
│   │   └── TripSelection/
│   │       ├── PB.Modules.TripSelection.Api/
│   │       ├── PB.Modules.TripSelection.Application/
│   │       ├── PB.Modules.TripSelection.Domain/
│   │       └── PB.Modules.TripSelection.Infrastructure/
│   │
│   └── Shared/
│       └── PB.Shared/                       ← Wspólne abstrakcje (Entity base, Result, itp.)
```

## Endpointy REST (skrót)

| Moduł | Metoda | Endpoint | Opis |
|---|---|---|---|
| AttractionDefinition | POST | `/api/attraction-definitions` | Utwórz draft |
| AttractionDefinition | GET | `/api/attraction-definitions/{id}` | Pobierz definicję |
| AttractionDefinition | GET | `/api/attraction-definitions` | Lista definicji (filtr po statusie) |
| AttractionDefinition | PUT | `/api/attraction-definitions/{id}` | Edytuj draft |
| AttractionDefinition | POST | `/api/attraction-definitions/{id}/publish` | Publikuj |
| AttractionDefinition | POST | `/api/attraction-definitions/{id}/archive` | Archiwizuj |
| AttractionDefinition | POST | `/api/attraction-definitions/groups` | Utwórz grupę atrakcji |
| Catalog | POST | `/api/catalog/entries` | Utwórz wpis katalogu z definicji |
| Catalog | GET | `/api/catalog/entries` | Lista (filtr: miasto, daty, kategoria) |
| Catalog | GET | `/api/catalog/entries/{id}` | Szczegóły wpisu |
| Catalog | DELETE | `/api/catalog/entries/{id}` | Usuń wpis |
| Preference | POST | `/api/preferences` | Utwórz preferencje |
| Preference | GET | `/api/preferences/{id}` | Pobierz preferencje |
| Preference | PUT | `/api/preferences/{id}` | Aktualizuj preferencje |
| TripSelection | POST | `/api/trip-selections` | Generuj dwie listy na podstawie preferencji |
| TripSelection | GET | `/api/trip-selections/{id}` | Pobierz wynik selekcji |
