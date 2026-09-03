# Raport – Zestawienie na posiedzenie Komisji Socjalnej

Wzorzec wydruku Enova (`Raporty/ZestawienieKomisjaSocjalna.repx`, format
DevExpress XtraReports, orientacja pozioma), zasilany snippetem
`Raporty/ZestawienieKomisjaSocjalnaSnippet`.

## Kontekst uruchomienia

Wydruk wywoływany jest z **Listy pracowników** (ew. **Pulpitu kierownika**)
dla zaznaczonych pracowników. Jeśli zaznaczone są wiersze historii
zatrudnienia (`PracHistoria`), snippet rozpoznaje pracownika, do którego
należą. Pracownicy sortowani po „Nazwisko i Imię”.

## Parametr wydruku

| Parametr | Pole | Domyślnie | Rola |
|---|---|---|---|
| Data posiedzenia komisji | `PrnParams.DataPosiedzenia` (`Date`) | dziś | Wyznacza rok odniesienia `R` (= rok tej daty) oraz służy jako data graniczna wniosku ZFŚS |

Okna zapomóg to **całe lata kalendarzowe**, rok `R` (rok daty posiedzenia)
jest **pominięty**:

- **„z 2 lat”**: lata `R-2` i `R-1` → `[1.1.(R-2), 31.12.(R-1)]`
- **„z poprzednich 2 lat”**: lata `R-4` i `R-3` → `[1.1.(R-4), 31.12.(R-3)]`

Przykład: `R = 2026` → „z 2 lat” = 2024 + 2025; „z poprzednich 2 lat” =
2022 + 2023.

## Układ: master-detail, jeden wiersz = jedna zapomoga

**Jeden wiersz wydruku = jedna wypłacona zapomoga.** Pracownik z kilkoma
zapomogami zajmuje kilka kolejnych wierszy. Kolumny „poziomu pracownika”
(Lp, Nr ewid., Nazwisko, Data urodzenia, Jednostka obsługująca, Dochód na
członka rodziny) oraz obie sumy wypełniane są **tylko w pierwszym wierszu
grupy** pracownika; w kolejnych wierszach te komórki są puste. Dwie listy
zapomóg (okno bieżące / poprzednie) są wyrównane wg indeksu wiersza –
dłuższa wyznacza liczbę wierszy grupy, krótsza ma puste komórki w
nadmiarowych wierszach. **Pracownik bez żadnej zapomogi w obu oknach jest
pomijany** – nie trafia na wydruk i nie zużywa numeru `Lp` (numeracja
pozostaje ciągła dla osób z zapomogami).

Przykład ze wzoru papierowego (nr ewid. 82652, 2 wiersze detail):
`Kwota zapomogi z 2 lat` = 3 000,00 = suma kolumny `Kwota` z obu wierszy
(1 000 + 2 000); `Kwota zapomóg z poprzednich 2 lat` = 2 000,00 = suma
kolumny `Kwota` (poprzednia) z obu wierszy (1 000 + 1 000).

## Kolumny

| Nagłówek | Pole źródła | Zawartość |
|---|---|---|
| Lp. | `Lp` | Liczba porządkowa – tylko w 1. wierszu grupy pracownika |
| Nr ewid. | `NrEwid` | `Pracownik.Kod` – tylko 1. wiersz grupy |
| Nazwisko i Imię | `NazwiskoImie` | `Pracownik.NazwiskoImię` – tylko 1. wiersz grupy |
| Data urodzenia | `DataUrodzenia` | `Pracownik.Historia[Date.Today].Urodzony.Data` – tylko 1. wiersz grupy |
| Jednostka obsługująca | `JednostkaObslugujaca` | Cecha „Jednostka obsługująca” z Wydziału bieżącego etatu pracownika. Cecha typu „element słownika” → `(ElemSlownika) Wydzial.Features["Jednostka obsługująca"]`, wyświetlane `.Nazwa` (fallback `"brak"`). Tylko 1. wiersz grupy |
| Dochód na członka rodziny | `DochodNaCzlonkaRodziny` | Z aktualnego, zatwierdzonego wniosku ZFŚS pracownika (krotka `A1_ZFSS`, dodatek `AltOne.Skanska.Workflow`). Odwzorowuje worker `WniosekZFFSPracownikaWorker.GetProgDochodu`: wniosek przez `PracownikExt.GetAktualnyWniosekZFSS(pracownik, data posiedzenia, rok posiedzenia)`; dla `RodzajWskazywaniaDochodu == "Kwota"` → kwota dochodu na członka rodziny, w przeciwnym razie nazwa progu dochodowego (element słownika). Brak roli widoczności kwot u operatora → `(Brak praw do widoku)`. Puste = brak zatwierdzonego wniosku. Tylko 1. wiersz grupy |
| Kwota zapomogi z 2 lat | `KwotaZapomogiZ2Lat` | Suma `WypElement.Wartosc` zapomóg z okna bieżącego (= suma kolumny `Kwota` wszystkich wierszy grupy). Puste = 0. Tylko 1. wiersz grupy |
| Data / Kwota | `DataWyplaty` / `KwotaWyplaty` | i-ta zapomoga z okna bieżącego: `Wyplata.Data` / `WypElement.Wartosc` (`N2`). Puste, gdy w tym wierszu nie ma już zapomogi z tego okna |
| Kwota zapomóg z poprzednich 2 lat | `KwotaZapomogPoprzednich2Lat` | Suma zapomóg z okna poprzedniego (= suma kolumny `Kwota` poprzednia wszystkich wierszy grupy). Puste = 0. Tylko 1. wiersz grupy |
| Data / Kwota | `DataWyplatyPoprzedniej` / `KwotaWyplatyPoprzedniej` | i-ta zapomoga z okna poprzedniego. Puste, gdy w tym wierszu nie ma już zapomogi z tego okna |

Zapomoga = element wypłaty (`WypElement`), którego nazwa (lub nazwa
`Definicja`) zawiera „zapomog"/„zapomóg", niewystornowany
(`RozliczenieStorna == false`), z datą wypłaty w oknie. Data pozycji =
`Wyplata.Data`. Pozycje w oknie sortowane rosnąco po dacie.

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

Rok oświadczenia = rok z „Daty posiedzenia komisji”; data graniczna wniosku
(„nie później niż”) = data posiedzenia.

## TODO / do weryfikacji na żywej bazie

- **Cały snippet niesprawdzony na środowisku Skanska** – kompilacja
  zweryfikowana lokalnie (enova 2604.4.4 + `AltOne.Skanska.Workflow` +
  DevExpress), ale bez uruchomienia. Sprawdzić:
  - liczba wierszy grupy = `max(zapomogi w oknie bieżącym, w poprzednim)`,
    puste kolumny poziomu pracownika w wierszach 2..N;
  - sumy zgadzają się z sumą widocznych pozycji (jak we wzorze papierowym);
  - zmiana roku w „Dacie posiedzenia” przesuwa oba okna o rok kalendarzowy.
- **Definicja „zapomogi”** – obecnie dopasowanie po nazwie elementu wypłaty
  (`"zapomog"`). Potwierdzić, że wszystkie definicje zapomóg w bazie Skanska
  mają „zapomog” w nazwie i że nie łapie fałszywych trafień.
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
4. Uruchom wydruk dla zaznaczonych pracowników; w oknie parametrów podaj
   „Datę posiedzenia komisji” (domyślnie dziś) i sprawdź wynik.

Zmiana nie wymaga modyfikacji `.repx` – pasmo `Detail` jest płaskie, a
„wygaszanie” powtórzonych kolumn i wyrównanie dwóch list zapomóg realizuje
snippet (puste stringi w wierszach 2..N grupy).

`.repx` zawiera już w `ComponentStorage` komponenty `BusinessContext`,
`BusinessSource` (`DataKind="Empty"`) i `BusinessSourceContext`
(`DataKind="Context"`) oraz atrybut `DataSource="#Ref-49"` na korzeniu –
analogicznie do `Raporty/SzablonTabela6Kolumn` (patrz tam „Historia usterki:
brak źródła danych w `.repx`”). Komórki wiersza danych są już powiązane z
polami `[Lp]`, `[NrEwid]`, `[NazwiskoImie]`, `[DataUrodzenia]`,
`[JednostkaObslugujaca]` itd. – layout nie wymaga zmian.

## Odporność na błędy

- Poszczególne komórki poziomu pracownika liczone są przez `Bezpiecznie(...)`
  – błąd w jednej daje `[BŁĄD: ...]` w tej komórce zamiast wywalenia wydruku.
- Grupa **każdego pracownika** budowana jest w osobnym `try/catch` – błąd
  (np. w pobraniu wypłat / wniosku ZFŚS) daje jeden wiersz `[BŁĄD: ...]` dla
  tego pracownika, reszta zestawienia się drukuje.
- Cała `BeforePrint` jest w `try/catch` – błąd poza pętlą daje jeden wiersz
  „BŁĄD” z etapem i treścią wyjątku.

Jeśli zobaczysz `[BŁĄD: ...]`, wklej treść – pozwoli poprawić logikę lub dane.
