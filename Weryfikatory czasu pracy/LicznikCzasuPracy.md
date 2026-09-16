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
