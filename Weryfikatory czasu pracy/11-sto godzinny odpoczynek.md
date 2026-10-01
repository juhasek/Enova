# 11-sto godzinny odpoczynek (weryfikator dnia planu)

Weryfikator kalendarza rodzaju **DzienPlanu** (`DefinicjaWeryfikatoraKalendarza+DzienPlanu`).
Uruchamia się przy każdej zmianie dnia planu pracy (`KontrolaDniaVerifier` w
`DzienPlanu.OnVerify()` – niezależnie od ustawienia „Kontrola planu”).

- **GUID definicji:** `1eeff9e2-7888-4309-ad2b-3064a1729809`
- **Nazwa w enova:** `11-sto godzinny odpoczynek`
- **Poziom:** `Error` (blokuje zapis planu) – ustawiany na powiązaniu kalendarza
  z weryfikatorem (`WeryfikatorKalendarza.Typ`), **nie** w kodzie. Ustalenie z klientem.
- Zweryfikowano na żywo: **Tak** (2026-09-11, test w GUI klienta, przed dodaniem zasady
  "dni modyfikowane" opisanej niżej) – scenariusz sobota 8:00–21:00 / niedziela 7:00–20:00
  poprawnie zgłasza brak 11h odpoczynku.
- **Zasada "dni modyfikowane" (2026-09-22): NIEZWERYFIKOWANA na żywo w GUI** – wymaga
  potwierdzenia u klienta.
- **Zgłoszona regresja (2026-09-22) i poprawka tego samego dnia**, patrz sekcja
  „Naprawiona luka" niżej. Nadal niezweryfikowana na żywo w GUI po poprawce.

## Nowe wymaganie: weryfikator dotyczy tylko dni modyfikowanych (kolor żółty)

Zgłoszenie klienta (2026-09-22): weryfikator ma reagować wyłącznie na dni, które użytkownik
**faktycznie zmodyfikował** (jawny wyjątek w kalendarzu pracownika – w GUI pokazywany jako
dzień w **kolorze żółtym**). Dzień, który wciąż dziedziczy godziny z kalendarza wzorcowego
(brak własnego wiersza w `DniPlanu`), nie jest sprawdzany i nie ma wpływać na wynik
weryfikacji dnia sąsiedniego.

Przykład z ustaleń: 16.09 wprowadzam pracę 8:00–16:00 + dyżur 16:00–21:00, **nie modyfikując**
17.09 (pozostaje domyślne 7:00–15:00) → **brak błędu**, mimo że realny odstęp
21:00→7:00 = 10h < 11h. Jeśli **17.09 również zostanie zmodyfikowany** (dowolnie) i odstęp
nadal będzie < 11h → **błąd**. Dotyczy to wszystkich dni (zarówno kontroli doby dnia
poprzedniego, jak i dociągania wczesnorannych stref dnia następnego).

**Implementacja:** `Pracownik.DniPlanu[data] != null` (właściwość `DateSubTable`, zwraca
`null` gdy dla danej daty nie ma jawnego wiersza w kalendarzu indywidualnym pracownika –
czyli dzień jest niemodyfikowany/dziedziczony z kalendarza wzorcowego). Sprawdzenie dodane:
- na wejściu każdej iteracji pętli (dzień bieżący / dzień poprzedni) – dzień niemodyfikowany
  pomija całą dobę rozpoczętą w tym dniu,
- przed dociągnięciem wczesnorannych stref dnia następnego do okna doby – pomijane, gdy
  dzień następny jest niemodyfikowany.

API zweryfikowane dekompilacją `Soneta.KadryPlace.dll` (2026-09-22): `Soneta.Kadry.Pracownik`
ma publiczną właściwość `DateSubTable DniPlanu => Kalendarz.Dni;` (to samo źródło, którego
używa `IZrodloPlanu.GetDzienPlanu(Date)` w jawnej implementacji interfejsu:
`(DzienKalendarzaBase)DniPlanu[data]`). `DateSubTable.this[Date]` zwraca `null`, gdy nie ma
wiersza dla danej daty (`Soneta.Business.DateSubTable`).

## Naprawiona luka: dzień właśnie zapisywany (`dp.Data`) traktowany jako niemodyfikowany

**Zgłoszenie klienta (2026-09-22, tego samego dnia co wdrożenie zasady „dni modyfikowane")):**
sobota – Dyżur domowy 8:00–21:00 (modyfikowana), niedziela – Dyżur domowy 8:00–21:00
(modyfikowana) → brak błędu, poprawnie. Następnie modyfikacja poniedziałku (praca 7:00–15:00,
też modyfikowana) → **brak błędu, mimo że powinien się pojawić** (realna przerwa
niedziela 21:00 → poniedziałek 7:00 = 10h < 11h; doba niedzieli, sprawdzana przy zapisie
poniedziałku jako kontrola dnia poprzedniego, powinna dociągnąć wczesnoranny fragment
poniedziałku i wykryć naruszenie).

**Przyczyna:** sprawdzenie „czy dzień jest modyfikowany" (`pracownik.DniPlanu[data] != null`)
zastosowane też do dnia **właśnie zapisywanego** (`dp.Data`, tu: poniedziałek) w trakcie jego
własnej weryfikacji. Nie ma gwarancji, że wiersz `dp`, jeszcze niescommitowany, jest już
widoczny przez świeże odpytanie `Pracownik.DniPlanu` (w przeciwieństwie do `kalkulatorPlanu[date]`,
którego widoczność własnych, niescommitowanych zmian była już potwierdzona na żywo
2026-09-11). Skutek: przy sprawdzaniu doby niedzieli (kontrola dnia poprzedniego dla
poniedziałku) warunek „czy poniedziałek jest modyfikowany" wychodził fałszywie negatywny,
fragment poniedziałku nie był dociągany do okna doby niedzieli i przerwa 10h nie została
wykryta.

**Poprawka:** dzień `dp.Data` jest modyfikowany **z definicji** (to właśnie ten wiersz jest
teraz zapisywany) – sprawdzane bez odpytywania `pracownik.DniPlanu`, żeby wyeliminować
zależność od jego widoczności w trakcie własnej weryfikacji:
- wejście pętli: `if (dataDoby != dp.Data && pracownik.DniPlanu[dataDoby] == null) continue;`
- dociąganie dnia następnego: `if (dataDoby + 1 == dp.Data || pracownik.DniPlanu[dataDoby + 1] != null)`

Zaimportowane do bazy `Claude` tego samego dnia (potwierdzone SQL-em). Nadal niezweryfikowane
na żywo w GUI.

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
okno doby = [ D0 ; D0 + 24h ]        (D0 w kodzie: zmienna poczatekDoby)
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
- **oraz** wczesnoranne strefy **dnia następnego**, których początek wpada jeszcze
  w okno doby bieżącej – dociągane z `kalkulatorPlanu[dataDoby + 1]`, przesunięte
  o +24h na wspólną oś czasu doby, a następnie przycinane do okna jak reszta stref,
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
| 6 | Sobota: Praca w normie 8:00–21:00 (modyfikowana); Niedziela: Praca w normie 7:00–20:00 (modyfikowana) | 8:00 (sob.) | fragment niedzieli 7:00–8:00 dociągnięty do doby sobotniej → przerwa 21:00→7:00 = **10:00** | **błąd** – realny odpoczynek 10h mimo że każda doba licząc „od siebie” dawałaby 11:00 |
| 7 | Sobota: Praca w normie 8:00–16:00 + Dyżur 16:00–21:00 (**modyfikowana**); Niedziela: godziny **domyślne** 7:00–15:00 (**niemodyfikowana** – brak wiersza w `DniPlanu`) | 8:00 (sob.) | nieliczone (niedziela wyłączona z okna, bo niemodyfikowana) → trailing 21:00→8:00 = **11:00** | **brak błędu** – mimo że realny odstęp 21:00→7:00 = 10h < 11h. Gdy niedziela zostanie zmodyfikowana (dowolnie) i odstęp nadal < 11h → błąd |
| 8 | Sobota: Dyżur domowy 8:00–21:00 (modyfikowana); Niedziela: Dyżur domowy 8:00–21:00 (modyfikowana); **następnie** modyfikacja **poniedziałku**: Praca w normie 7:00–15:00 | 8:00 (nd., kontrola dnia poprzedniego przy zapisie poniedziałku) | fragment poniedziałku 7:00–8:00 dociągnięty do doby niedzielnej → przerwa 21:00→7:00 = **10:00** | **błąd** przy zapisie poniedziałku – zgłoszona regresja (2026-09-22), naprawiona tego samego dnia (patrz „Naprawiona luka" wyżej) |

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
- `pracownik.DniPlanu` (`DateSubTable`, `Kalendarz.Dni`), indekser `DniPlanu[Date]` → `Row`,
  `null` gdy dzień niemodyfikowany (brak wyjątku w kalendarzu indywidualnym)
- `new KalkulatorPlanu(pracownik)`, indekser `kp[Date]` → `Dzien` (auto-`LoadOkres`; może zwrócić `null`)
- `Date + int` → `Date` (kolejny dzień; używane do dociągnięcia doby następnej: `kp[dataDoby + 1]`)
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

- Zachowanie przy **nieobecności części dnia** + Dyżur – nie objęte scenariuszami klienta.
- `kp[dp.Data - 1]` dla dni sprzed zatrudnienia zwraca `null` → doba poprzednia pomijana
  (bez błędu).
- **Zasada "dni modyfikowane" (2026-09-22) NIEZWERYFIKOWANA na żywo.** Założenie do
  potwierdzenia w GUI: `pracownik.DniPlanu[dp.Data]` widzi `dp` samego siebie już w trakcie
  `OnVerify` (wiersz dodawany/edytowany w tej samej sesji, jeszcze przed commitem) – ten sam
  mechanizm, na którym już wcześniej opierał się kod dla iteracji `przesuniecieDni = 0`
  (`kalkulatorPlanu[dp.Data]` musiał widzieć bieżące zmiany, żeby scenariusz z 2026-09-11
  działał poprawnie), więc ryzyko niskie, ale wymaga próby w GUI klienta.
