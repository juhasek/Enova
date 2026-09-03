# Raport – Zestawienie na posiedzenie Komisji Socjalnej

Wzorzec wydruku Enova (`Raporty/ZestawienieKomisjaSocjalna.repx`, format
DevExpress XtraReports, orientacja pozioma), zasilany snippetem
`Raporty/ZestawienieKomisjaSocjalnaSnippet`.

## Kontekst uruchomienia

Wydruk wywoływany jest z **Listy pracowników** (ew. **Pulpitu kierownika**)
dla zaznaczonych pracowników. Jeśli zaznaczone są wiersze historii
zatrudnienia (`PracHistoria`), snippet rozpoznaje pracownika, do którego
należą. Jeden wiersz wydruku = jeden pracownik (deduplikacja), posortowane
po „Nazwisko i Imię”.

## Kolumny

| Nagłówek | Pole źródła | Zawartość |
|---|---|---|
| Lp. | `Lp` | Liczba porządkowa wiersza |
| Nr ewid. | `NrEwid` | `Pracownik.Kod` |
| Nazwisko i Imię | `NazwiskoImie` | `Pracownik.NazwiskoImię` |
| Data urodzenia | `DataUrodzenia` | `Pracownik.Historia[Date.Today].Urodzony.Data` |
| Jednostka obsługująca | `JednostkaObslugujaca` | Cecha „Jednostka obsługująca” z Wydziału bieżącego etatu pracownika. Cecha typu „element słownika” → `(ElemSlownika) Wydzial.Features["Jednostka obsługująca"]`, wyświetlane `.Nazwa` (fallback `"brak"`) |
| Dochód na członka rodziny | `DochodNaCzlonkaRodziny` | Z aktualnego, zatwierdzonego wniosku ZFŚS pracownika (krotka `A1_ZFSS`, dodatek `AltOne.Skanska.Workflow`). Odwzorowuje worker `WniosekZFFSPracownikaWorker.GetProgDochodu`: wniosek pobierany przez `PracownikExt.GetAktualnyWniosekZFSS(pracownik, dziś, rok bieżący)`; dla `RodzajWskazywaniaDochodu == "Kwota"` → kwota dochodu na członka rodziny, w przeciwnym razie nazwa progu dochodowego (element słownika). Brak roli widoczności kwot u operatora → `(Brak praw do widoku)`. Puste = brak zatwierdzonego wniosku za rok bieżący |
| Kwota zapomogi z 2 lat | `KwotaZapomogiZ2Lat` | Suma `WypElement.Wartosc` elementów wypłat pracownika, których nazwa (lub nazwa `Definicja`) zawiera „zapomog"/„zapomóg", wypłaconych w oknie `[dziś − 24 mies., dziś]` wg `Wyplata.Data`. Elementy stornowane pominięte. Puste = brak / suma 0 |
| Data / Kwota (wypłata bieżąca) | `DataWyplaty` / `KwotaWyplaty` | **na razie puste** |
| Kwota zapomóg z poprzednich 2 lat | `KwotaZapomogPoprzednich2Lat` | **na razie puste** |
| Data / Kwota (wypłata poprzednia) | `DataWyplatyPoprzedniej` / `KwotaWyplatyPoprzedniej` | **na razie puste** |

## Zależność: dodatek AltOne.Skanska.Workflow

Kolumna „Dochód na członka rodziny” korzysta z klas dodatku
`AltOne.Skanska.Workflow` (środowisko Skanska): `PracownikExt` i `A1ZfssTuple`
z przestrzeni `AltOne.Skanska.Workflow.Extensions` /
`AltOne.Skanska.Workflow.Procesy.WniosekZFSS.Tuples`. Snippet **nie skompiluje
się** w bazie, w której ten dodatek nie jest wczytany. Logika (metoda
`PobierzDochodNaCzlonkaRodziny`) jest 1:1 odwzorowaniem property
`WniosekZFFSPracownikaWorker.GetProgDochodu` – zweryfikowana przez dekompilację
DLL w wersjach `2512.7.8` i `2604.4.4`, **niesprawdzona na żywej aplikacji**
(repo nie ma dostępu do środowiska Skanska). Przed użyciem produkcyjnym:
uruchomić wydruk na bazie Skanska dla kilku pracowników z i bez wniosku ZFŚS.

Filtrowanie wniosku jest ustawione na stałe: stan na dzień dzisiejszy, rok
oświadczenia = rok bieżący (`Date.Today.Year`) – tak jak domyślny widok
wniosków ZFŚS. Jeśli komisja potrzebuje danych za wskazany rok, trzeba dodać
parametr wydruku `RokOswiadczenia` (klasa `PrnParams` jest dziś pusta – wymaga
też pola parametru w `.repx`).

## TODO

- **Pozostałe kolumny finansowe** (kwoty zapomóg poza „z 2 lat”, daty i
  kwoty wypłat) – do uzupełnienia. Źródłem będą świadczenia socjalne
  pracownika (`Soneta.Kadry.SwiadczSocjalne`: `Data` = data przyznania,
  `Definicja`, `Rozliczenie.Kwota`, `Rozliczenie.Data`, `Rozliczenie.Okres`,
  `Elementy`).
- **Cecha „Jednostka obsługująca”** na Wydziale jest typu **element
  słownika** (`Soneta.Ksiega.ElemSlownika`). Odczyt: nietypowany indeksator
  `Wydzial.Features["Jednostka obsługująca"]` → rzut na `ElemSlownika` →
  `.Nazwa`; brak wartości → `"brak"`. Nazwa cechy w konfiguracji to dokładnie
  „Jednostka obsługująca” (ze spacją). Wymaga `using Soneta.Ksiega;`.

## Jak podpiąć snippet w Enova

1. Otwórz `ZestawienieKomisjaSocjalna` w projektancie wydruków Enova.
2. W „Kod źródłowy” wklej całą zawartość `Raporty/ZestawienieKomisjaSocjalnaSnippet`.
3. Zapisz – Enova skompiluje kod i podepnie klasę
   `ZestawienieKomisjaSocjalnaSnippet` pod wydruk.
4. Uruchom wydruk dla zaznaczonych świadczeń socjalnych i sprawdź wynik.

`.repx` zawiera już w `ComponentStorage` komponenty `BusinessContext`,
`BusinessSource` (`DataKind="Empty"`) i `BusinessSourceContext`
(`DataKind="Context"`) oraz atrybut `DataSource="#Ref-49"` na korzeniu –
analogicznie do `Raporty/SzablonTabela6Kolumn` (patrz tam „Historia usterki:
brak źródła danych w `.repx`”). Komórki wiersza danych są już powiązane z
polami `[Lp]`, `[NrEwid]`, `[NazwiskoImie]`, `[DataUrodzenia]`,
`[JednostkaObslugujaca]` itd. – layout nie wymaga zmian.

## Odporność na błędy

Kolumny liczone (Nr ewid., Nazwisko, Data urodzenia, Jednostka obsługująca,
Dochód na członka rodziny, Kwota zapomogi z 2 lat) liczone są przez
`Bezpiecznie(...)` – błąd dla pojedynczego wiersza daje `[BŁĄD: ...]` w tej
komórce zamiast wywalenia wydruku. Cała
`BeforePrint` jest w try/catch – błąd poza pętlą daje jeden wiersz „BŁĄD” z
etapem i treścią wyjątku. Jeśli zobaczysz `[BŁĄD: ...]`, wklej treść –
pozwoli poprawić logikę lub dane.
