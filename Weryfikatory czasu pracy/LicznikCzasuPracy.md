# Licznik czasu pracy (CodeFile `LicznikManager`)

CodeFile enova365 (`Soneta.Runtime.Database.Business.TblCodeFiles.LicznikManager`),
nie weryfikator w ścisłym sensie enova (nie jest podpięty jako `DefinicjaWeryfikatora`),
ale obsługuje logikę RCP „Rozpocznij/Zakończ pracę” (przyciski na kartotece
pracownika) — stąd w tym folderze, bo dotyczy domeny czasu pracy.

- **Lokalizacja w bazie:** `CodeFiles.ID = 14`, `RuntimeInfoFileName = LicznikCzasuPracy`.
- **Zweryfikowano na żywo:** **Nie** — środowisko robocze nie ma buscall/GUI, tylko
  SQL bezpośredni na `localhost\SQLEXPRESS`. Weryfikacja = code review + sprawdzenie
  konfiguracji cech w bazie testowej „Claude”.

## Cel

Sterowanie widocznością/blokadą przycisków „Rozpocznij/Zakończ pracę” oraz — dla
pracowników na **kalendarzu ruchomym** — automatyczne domknięcie dnia: gdy pracownik
kliknie tylko „Rozpocznij pracę”, system sam dolicza normę dnia (np. 8h) i wstawia
wpis „Wyjście”, przycięty do maksymalnej godziny z cechy `LCzPGodzinaKoncaDnia`
(bezpiecznik przeciw zbyt późnemu/przekręcającemu dobę wyliczeniu).

## Cechy globalne (`FeatureDefs`, `TableName = CfgNodes`)

| Cecha | Typ | Rola |
|---|---|---|
| `LCzPStartNormaKalend` | Bool | włącznik główny — bez niej `DodajWyjsciePodczasWejscia` nic nie robi |
| `LCzPKoniecDnia` | Bool | włącznik przycinania do stałej godziny końca dnia |
| `LCzPGodzinaKoncaDnia` | Time | godzina graniczna (np. `18:30`) — maksymalna dozwolona godzina wyjścia |
| `LCzPKoniecNorma` | Bool | gdy `True`, finalny wpis liczony **od nowa** z `OdGodziny planu + Czas planu`, **ignorując** wcześniejsze przycięcie do `LCzPGodzinaKoncaDnia` (patrz Ryzyko niżej) |
| `LCzPStartNorma`, `LCzPStartPrzedzial`, `LCzPStartPPrzed`, `LCzPStartPPo`, `LCzPTolernacja`, `TolerancjaSpoznienia` | Bool/Time/Int | dotyczą kontroli godziny **wejścia** (`IsReadOnlyRozpoczeciePracy`, `DodajWejscieWyjscieAfterEdit`), nie „końca dnia” |
| `LCZPStart7` | Bool | domyślna godzina rozpoczęcia nie wcześniej niż 7:00 (`DomyslnaGodzinaRozpoczecia`) |
| `LCZBlokujWidokZakonczenia` | Bool | ukrywa przycisk „Zakończ pracę” |
| `LCzPBlokadaWdniWolne` | Bool | blokuje wejście w dniu wolnym |

## Warunek dostępu do przycięcia `LCzPGodzinaKoncaDnia`

Przycięcie działa **tylko** gdy `IsStandGroupCalend(pracownik, data) == false`, czyli
kalendarz wzorcowy pracownika (`Etat.Kalendarz.SystemCzasuPracy`, kalendarz **wzorcowy**,
nie indywidualny — patrz pamięć repo o kalendarzu wzorcowym vs indywidualnym) **nie**
jest Standardowy ani Równoważny — w praktyce: **kalendarz ruchomy**. To jest zamierzone:
mechanizm dotyczy wyłącznie grup na czasie ruchomym, gdzie `Dzien.OdGodziny` jest
ustawiane przez silnik enova na rzeczywisty czas kliknięcia „Rozpocznij pracę”.

## Błąd znaleziony i naprawiony (2026-09-16)

W `DodajWyjsciePodczasWejscia`, przy budowie granicy `godzdomk`:

```csharp
// PRZED (błąd):
DateTime godzdomk = new DateTime(godzinaWyjscia.Year, godzinaWyjscia.Month, godzinaWyjscia.Day,
                                  godzinadomk.Hours, godzinadomk.Minutes, 0);
```

`godzinaWyjscia` to już wyliczone „start + norma dnia”. Gdy start jest późny
(np. 16:00 + 8h planu = 24:00), `DateTime` samo przewija się na **kolejny dzień
kalendarzowy** (jutro 00:00). Granica `godzdomk` była wtedy liczona z tej *już
przewiniętej* daty → wychodziła jako „jutro 18:30” zamiast „dziś 18:30”, więc
porównanie `godzinaWyjscia > godzdomk` (jutro 00:00 > jutro 18:30) dawało **fałsz**
i limit się nie włączał — pracownikowi zapisywało się wyjście o północy zamiast
o godzinie z cechy. To dokładnie scenariusz, przed którym cały mechanizm ma chronić.

W kodzie była już zakomentowana próba tej poprawki (`//DateTime godzdomk = new
DateTime(today.Year, ...)`), widocznie nigdy nie odkomentowana.

**Poprawka:** użycie `today` (parametr metody = doba pracownicza) zamiast
`godzinaWyjscia.Year/Month/Day`:

```csharp
DateTime godzdomk = new DateTime(today.Year, today.Month, today.Day,
                                  godzinadomk.Hours, godzinadomk.Minutes, 0);
```

Naniesiona bezpośrednio w `CodeFiles.ID=14` w bazie Claude (ADO.NET/`SqlParameter`,
nie `sqlcmd -Q`, żeby nie zepsuć polskich znaków — patrz pamięć repo o mangle
`sqlcmd -f 65001`) oraz w tym pliku repo.

## Ryzyko do potwierdzenia: `LCzPKoniecNorma`

Jeśli poza `LCzPKoniecDnia` włączona jest **równocześnie** `LCzPKoniecNorma`, kod
niżej w tej samej metodzie nadpisuje już przycięte `godzinaWyjscia` świeżo policzonym
`normatywneDo` (`OdGodziny planu + Czas planu`, **bez** odniesienia do
`LCzPGodzinaKoncaDnia`) i to on trafia do `AddEmployeeRCPEntry`. W bazie Claude
(konfiguracja „jak u klienta”) `LCzPKoniecNorma` **nie jest ustawiona** (efektywnie
`False`), więc na razie to nie problem — ale przy ewentualnym włączeniu tej cechy
przycięcie zostałoby po cichu zignorowane. Nie naprawiane (poza zakresem zgłoszenia),
tylko udokumentowane.

## Zakres zmiany godziny granicznej

`LCzPGodzinaKoncaDnia` to **jedna wartość globalna** (`session.Global.Features`),
wspólna dla **wszystkich** grup pracowników na kalendarzu ruchomym — nie ma
możliwości ustawienia różnej godziny granicznej per kalendarz/grupa bez zmiany
kodu. Potwierdzone z użytkownikiem: podniesienie z 17:00 na 18:30 ma dotyczyć
**wszystkich** grup na czasie ruchomym (nie tylko nowo tworzonej), więc sama zmiana
wartości cechy jest wystarczająca — nie wymaga rozbudowy mechanizmu o
zróżnicowanie per kalendarz.

## Nowa grupa (czas ruchomy, w przygotowaniu)

Założenia klienta: start 7:00–10:00, koniec 15:00–18:00, norma 8h. Naliczony koniec
(start + 8h) mieści się naturalnie w oknie 15:00–18:00 przy starcie w oknie 7:00–10:00
— cecha `LCzPGodzinaKoncaDnia = 18:30` działa więc jako **siatka bezpieczeństwa** dla
anomalii (spóźnione/błędne kliknięcie „Rozpocznij pracę”, np. dopiero o 16:00), nie
jako normalna ścieżka. Sam kalendarz tworzy klient — poza zakresem tego pliku.

## Stan konfiguracji w bazie testowej „Claude” (2026-09-16)

Wartości cech (`Features`, `ParentType='CfgNodes', Parent=1`):

```
LCzPStartNormaKalend = True
LCzPKoniecDnia       = True
LCzPGodzinaKoncaDnia = 18:30
LCzPKoniecNorma      = (nieustawiona → False)
```

Żaden pracownik w bazie „Claude” nie ma jeszcze przypisanego kalendarza ruchomego
(kalendarz „Ruchomy”, ID 26, istnieje jako wzorzec, ale bez przypisania) — do
faktycznego przetestowania scenariusza (16:00 + 8h → przycięcie do 18:30) potrzebny
pracownik testowy na takim kalendarzu.

## Pracownik testowy `RUCH-01` (2026-09-16)

Zatrudniony pod scenariusz nowej grupy (czas ruchomy 7:00–10:00 start / 15:00–18:00
koniec, norma 8h):

- Import: [ImportyXML/Licznik czasu pracy ruchomy - test 01 pracownicy.xml](../ImportyXML/Licznik%20czasu%20pracy%20ruchomy%20-%20test%2001%20pracownicy.xml)
  (`dbmgr importxml Claude "<plik>"`, tryb rekordowy — struktura wg
  `Demo/100.Kadry.gold.xml`).
- `Pracownicy.ID = 41`, `Kod = RUCH-01`.
- `Etat.Kalendarz` (wzorcowy) = **Ruchomy** (`Kalendarze.ID=26`,
  `RuchomyCzasPracy=1`) → `IsStandGroupCalend` zwraca `false` dla tego pracownika,
  czyli mechanizm cappingu `LCzPGodzinaKoncaDnia` się dla niego uruchamia.
  Indywidualny kalendarz (`Kalendarze.ID=52`, `Typ=2`) utworzony automatycznie
  przy imporcie.
- Cechy na `Pracownicy` (`Features`, `ParentType='Pracownicy', Parent=41`):
  `LicznikCzasuPracy = True`, `PowLicznikaCzasuPracy = True`.
  **Niepewne, która z tych dwóch faktycznie steruje widocznością widgetu na
  pulpicie** — obie zdefiniowane w `FeatureDefs` (ID 35, 36), ale żadna nie jest
  użyta w dostępnym w repo kodzie (`LicznikManager` czyta tylko cechy globalne
  `LCzP*`); logika pulpitu żyje w skompilowanym dodatku `AltOne.LicznikCzasuPracy`,
  do którego nie mamy tu źródła. Ustawiono obie na `True` jako bezpieczny wybór —
  **do potwierdzenia w GUI klienta**, która faktycznie odpowiada za widoczność.
- Trzecia cecha na `Pracownicy` — `GodzinaZamknieciaLicznika` (`FeatureDefs.ID=34`,
  `TypeNumber=17`, format nieznany, brak przykładu użycia w bazie) — **pozostawiona
  nieustawiona**, bo `LicznikManager` czyta wyłącznie globalną
  `LCzPGodzinaKoncaDnia`; zgodnie z ustaleniem limit ma być wspólny dla wszystkich
  grup ruchomych, więc override per pracownik nie powinien być potrzebny — do
  potwierdzenia, czy dodatek w ogóle z niego korzysta.

## Próba symulacji „na żywo" (2026-09-16) — ograniczenie środowiska

Poproszony o zalogowanie się na pulpit pracowniczy (`tl`/`1`, `http://localhost:5000/Login/claude`)
i klikniecie „Rozpocznij pracę" — **niewykonalne z tego środowiska**, sprawdzone i
udokumentowane:

- `buscall` (sterowanie żywą aplikacją z CLI) — brak w instalacji (sprawdzone
  `dbmgr`'s katalog + `dotnet tool list -g`), zgodnie z [[project_srodowisko_lokalne]].
- `/Login/claude` to czysta powłoka SPA (JS renderuje całość po stronie klienta) —
  `curl`/`WebFetch` nie mają czego wypełnić ani obserwować; `WebFetch` w ogóle nie
  obsługuje `localhost`.
- Import XML w trybie rekordowym (jedyny działający tryb `dbmgr importxml` — tryb
  `business="true"` wywala dbmgr, patrz [[reference_import_pracownika_xml]]) **nie
  uruchamia logiki biznesowej/workerów** — wpis „Wejście" wstawiony tą drogą **nie**
  wywoła automatycznie `LicznikManager.DodajWyjsciePodczasWejscia` (ten trigger żyje
  w skompilowanym dodatku spiętym z akcją UI „Rozpocznij pracę").
- `TestBase` (testy integracyjne) zarządza własnymi, izolowanymi bazami
  `nunit_default`/`nunit_ui`/`nunit_premiumui` — nie dołącza się do istniejącej,
  ręcznie przygotowanej bazy „Claude" z naszym `RUCH-01`.
- Samodzielne otwarcie `Login`/`Session` z konsolowego `dotnet-script` wymagałoby
  odtworzenia bootstrapu `BusApplication` (rejestracja bazy, DI, licencje) — to,
  co normalnie robi `dbmgr`/`server.exe` wewnątrz własnego hosta; brak w tym
  środowisku udokumentowanego, lekkiego sposobu zrobienia tego z zewnątrz.

**Co faktycznie zweryfikowano zamiast tego:**

1. Izolowana symulacja arytmetyki dat — dosłowny fragment `godzdomk`
   z `LicznikManager` (przed i po poprawce) przepisany do małej konsoli net8,
   z realnymi wartościami tego scenariusza (`today=2026-09-16`,
   `czasWejscia=16:00`, `plan=8h`, `LCzPGodzinaKoncaDnia=18:30`):
   - **przed poprawką:** `godzinaWyjscia (2026-09-17 00:00) > godzdomk (2026-09-17 18:30)` →
     `False` → cap **nie** działa, zostaje 2026-09-17 00:00.
   - **po poprawce:** `godzinaWyjscia (2026-09-17 00:00) > godzdomk (2026-09-16 18:30)` →
     `True` → cap działa, wynik **2026-09-16 18:30**.
   - Potwierdza to dokładnie diagnozę z sekcji „Błąd znaleziony i naprawiony" wyżej.
2. Rzeczywiste dane RCP w bazie „Claude" dla `RUCH-01`:
   [ImportyXML/Licznik czasu pracy ruchomy - test 02 dane RCP.xml](../ImportyXML/Licznik%20czasu%20pracy%20ruchomy%20-%20test%2002%20dane%20RCP.xml) —
   wpis „Wejście" 2026-09-16 16:00 (realny punkt startowy scenariusza) + wpis
   „Wyjście" 2026-09-16 18:30, jawnie oznaczony w `Uwagi` jako **wyliczony ręcznie
   wg poprawionego algorytmu, nie przez żywy trigger** — bo trigera nie dało się
   tu odpalić. **Nadal wymaga potwierdzenia w GUI klienta** (przycisk „Rozpocznij
   pracę" na pulpicie `tl`), że skompilowany dodatek faktycznie wstawia ten sam
   wynik automatycznie.
