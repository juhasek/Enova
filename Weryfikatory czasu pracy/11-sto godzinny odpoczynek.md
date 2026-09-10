# 11-sto godzinny odpoczynek (weryfikator dnia planu)

Weryfikator kalendarza rodzaju **DzienPlanu** (`DefinicjaWeryfikatoraKalendarza+DzienPlanu`).
Uruchamia się przy każdej zmianie dnia planu pracy (`KontrolaDniaVerifier` w
`DzienPlanu.OnVerify()` – niezależnie od ustawienia „Kontrola planu”).

- **GUID definicji:** `1eeff9e2-7888-4309-ad2b-3064a1729809`
- **Nazwa w enova:** `11-sto godzinny odpoczynek`
- **Poziom:** `Error` (blokuje zapis planu) – ustawiany na powiązaniu kalendarza
  z weryfikatorem (`WeryfikatorKalendarza.Typ`), **nie** w kodzie. Ustalenie z klientem.
- Zweryfikowano na żywo: **Nie** – środowisko robocze repo nie ma dostępu do
  bazy/DLL klienta. Odbiór po teście w GUI (scenariusze niżej).

## Po co, skoro Soneta ma własny weryfikator 11 h

Wbudowany `WymaganaPrzerwaVerifier` Sonety **nie uruchamia się dla kalendarzy
równoważnych** (`DzienPlanu.OnVerify` dodaje go tylko, gdy `!RównoważnyCzasPracy`).
Kalendarz klienta „4msc” jest równoważny, więc ten customowy weryfikator jest
**jedyną** kontrolą 11 h odpoczynku dla planu.

Dodatkowo klient wymaga **innej semantyki niż standard Sonety** (patrz niżej).

## Semantyka: jedna doba pracownicza (24 h)

Standardowy weryfikator Sonety liczy odpoczynek **względem realnego początku pracy
w kolejnej dobie** (patrzy na dzień następny). Tutaj odpoczynek jest analizowany
**w ramach jednej doby pracowniczej = sztywne 24 h**:

```
okno doby = [ D0 ; D0 + 24h ]
```

### Kotwica doby `D0`

| Sytuacja w dniu | `D0` |
|---|---|
| Jest „praca w normie” (strefa `Wchodzi && Typ == Zwiększa`) | początek najwcześniejszej takiej strefy |
| Brak pracy w normie – dzień wolny / świąteczny / sobota / niedziela / wolny za święto, jedyną strefą bywa **Dyżur domowy** | początek najwcześniejszej strefy aktywności (Dyżuru) |
| Brak jakiejkolwiek strefy aktywności | `OdGodziny` dnia; a gdy i to puste – doba pomijana (nie ma czego sprawdzać) |

### Strefy aktywności (co „zajmuje” dobę)

- strefy wliczane do doby wg Sonety: `Definicja.Wchodzi && Definicja.Typ == TypStrefy.Zwieksza`
  (np. „praca w normie”, „praca poza normą”),
- **oraz** strefy oznaczone cechą **`Weryfikator11h`** (u klienta: „Dyżur domowy”,
  która ma `Typ == NieWpływa`, więc Sonecie „nie liczy się” – cecha dodaje ją z powrotem),
- pomijane: strefy z `OdGodziny` puste lub `Czas == 0`.

### Naruszenie

W oknie `[D0 ; D0 + 24h]` liczona jest **największa ciągła przerwa** (wolna od stref
aktywności): przerwa wiodąca (`D0` → 1. strefa), przerwy między strefami oraz przerwa
końcowa (ostatnia strefa → `D0 + 24h`). Jeśli **największa** z nich `< 11 h` → komunikat.

Strefy są przycinane do okna doby (fragment sprzed `D0` lub po `D0 + 24h` nie liczy się).

## Kontrola dnia poprzedniego

Weryfikator, uruchomiony dla dnia `dp.Data`, sprawdza **dwie doby**:

1. dobę rozpoczętą w dniu bieżącym (`dp.Data`),
2. dobę rozpoczętą w dniu poprzednim (`dp.Data - 1`).

Dzięki temu komunikat o niezachowaniu 11 h w dobie np. **soboty** pojawia się także
podczas **wprowadzania zapisów na niedzielę** (tak działała wcześniejsza, pełniejsza
wersja `…_old`; uproszczona wersja produkcyjna to gubiła – to była regresja).

## Scenariusze (założenia klienta)

Doba w komentarzach zapisana jako `D0–(D0+24h)`.

| Lp | Dzień / strefy | `D0` | Największa przerwa | Wynik |
|---|---|---|---|---|
| 1 | Niedziela, Dyżur domowy 8:00–21:00 (jedyna strefa) | 8:00 | 21:00 → 8:00 = **11:00** | brak błędu |
| 2 | Dzień roboczy: Praca w normie 8:00–16:00 + Dyżur 20:00–21:00 | 8:00 | trailing 21:00 → 8:00 = **11:00** (środek 16→20 = 4 h) | brak błędu |
| 3 | Dzień roboczy: Praca w normie 7:00–16:00 + Dyżur 21:00–23:00 | 7:00 | trailing 23:00 → 7:00 = **8:00** | **błąd** – „…nie zachowano … 11-godzinnego odpoczynku …” |
| 4 | Święto / sobota / niedziela / wolny za św.: Dyżur domowy jedyną strefą | początek Dyżuru | wg długości Dyżuru | błąd, gdy Dyżur zostawia < 11 h wolnego w dobie 24 h |
| 5 | Sobota z Dyżurem kończącym się za późno; komunikat ma się pokazać przy edycji **niedzieli** | wg soboty | wg soboty | **błąd** wyświetlany także przy zapisie niedzieli (kontrola dnia poprzedniego) |

Pełna lista wraz z kolumnami „Wynik testu / Uwagi” →
[Scenariusze testowe weryfikatorow czasu pracy.xlsx](Scenariusze%20testowe%20weryfikatorow%20czasu%20pracy.xlsx), arkusz „Odpoczynek dobowy 11h”.

## Wymagana konfiguracja

1. **Cecha `Weryfikator11h`** na tabeli `DefinicjeStref` (typ Bool) –
   `FeatureDefinition` guid `aa96da4b-0e5d-4504-9bad-89eac460fc1c`.
   Bez tej definicji `Features.GetBool("Weryfikator11h")` rzuca wyjątek przy każdym
   zapisie dnia planu.
2. **Ustawienie cechy `= True`** na definicjach stref, które mają być traktowane jak
   aktywność mimo `Typ == NieWpływa` (u klienta: „Dyżur domowy”).
3. **Powiązanie definicji weryfikatora z kalendarzem** (`Kalendarz → Weryfikatory`),
   z `Typ = Error`. Sam import definicji nie wystarczy – musi być podpięta do każdego
   kalendarza, który ma ją egzekwować. `Blokada` definicji = `False`.

Import: [ImportyXML/Weryfikator 11h odpoczynek dobowy.xml](../ImportyXML/Weryfikator%2011h%20odpoczynek%20dobowy.xml)
(aktualizuje `<Code>` istniejącej definicji po GUID – zachowuje powiązania i poziom).

## API użyte w skrypcie

- `dp.Pracownik` (== `dp.Kalendarz.Pracownik`), `dp.Data`
- `new KalkulatorPlanu(pracownik)`, indekser `kp[Date]` → `Dzien` (auto-`LoadOkres`; może zwrócić `null`)
- `Dzien : IEnumerable<IStrefaExt>` – iteracja po strefach doby; `Dzien.OdGodziny`
- `IStrefaExt.Definicja` (`DefinicjaStrefy`): `.Wchodzi`, `.Typ` (`TypStrefy.Zwieksza`), `.Features.GetBool(...)`
- `IStrefaExt.OdGodziny`, `.Czas`
- `FromTimes` / `FromTime`: `.Add(FromTime)`, `.ToFlat()`, `FromTime(Time from, Time czas)`, `.From`, `.To`
- `Time`: `new Time(11,0)`, `new Time(24,0)`, `Time.Empty`, `Time.Zero`, operatory `+ - < > >=`
  (uwaga: `t - Time.Empty` zwraca `t`; `Time.Empty + t` zwraca `t` – dlatego jawne guardy)
- `Date.Day/Month/Year`, `.ToString("00")`, `"...".TranslateFormat(...)`

## Ograniczenia edytora skryptów (patrz pamięć repo)

Cała logika w jednej metodzie, bez metod pomocniczych i bez `Nullable<T>` –
wbudowany edytor skryptów enova bywa zawodny przy takich konstrukcjach.

## Znane ograniczenia / do potwierdzenia w GUI

- **Strefy wczesnoranne dnia następnego** wpadające w okno doby bieżącej **nie są**
  wliczane (każda doba liczona z własnych stref). Jeśli np. sobotni Dyżur kończy się
  o 20:00 (odpoczynek do 8:00 niedz. = 12 h, OK), a w niedzielę dodano Dyżur 6:00–8:00,
  faktyczny odpoczynek soboty spada do 10 h – **ta sytuacja nie zostanie wykryta**.
  Do decyzji, czy rozszerzać (drobna zmiana).
- Zachowanie przy **nieobecności części dnia** + Dyżur – nie objęte scenariuszami klienta.
- `kp[dp.Data - 1]` dla dni sprzed zatrudnienia zwraca `null` → doba poprzednia pomijana
  (bez błędu).
