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
| Dochód na członka rodziny | `DochodNaCzlonkaRodziny` | **na razie puste** |
| Kwota zapomogi z 2 lat | `KwotaZapomogiZ2Lat` | Suma `WypElement.Wartosc` elementów wypłat pracownika, których nazwa (lub nazwa `Definicja`) zawiera „zapomog"/„zapomóg", wypłaconych w oknie `[dziś − 24 mies., dziś]` wg `Wyplata.Data`. Elementy stornowane pominięte. Puste = brak / suma 0 |
| Data / Kwota (wypłata bieżąca) | `DataWyplaty` / `KwotaWyplaty` | **na razie puste** |
| Kwota zapomóg z poprzednich 2 lat | `KwotaZapomogPoprzednich2Lat` | **na razie puste** |
| Data / Kwota (wypłata poprzednia) | `DataWyplatyPoprzedniej` / `KwotaWyplatyPoprzedniej` | **na razie puste** |

## TODO

- **Kolumny finansowe** (dochód na członka rodziny, kwoty zapomóg, daty i
  kwoty wypłat) – do uzupełnienia. Źródłem będą świadczenia socjalne
  pracownika (`Soneta.Kadry.SwiadczSocjalne`: `Data` = data przyznania,
  `Definicja`, `Rozliczenie.Kwota`, `Rozliczenie.Data`, `Rozliczenie.Okres`,
  `Elementy`); „dochód na członka rodziny” prawdopodobnie z oświadczenia o
  dochodach ZFŚS – do ustalenia źródło i sposób filtrowania po latach.
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

Kolumny 2–5 liczone są przez `Bezpiecznie(...)` – błąd dla pojedynczego
wiersza daje `[BŁĄD: ...]` w tej komórce zamiast wywalenia wydruku. Cała
`BeforePrint` jest w try/catch – błąd poza pętlą daje jeden wiersz „BŁĄD” z
etapem i treścią wyjątku. Jeśli zobaczysz `[BŁĄD: ...]`, wklej treść –
pozwoli poprawić logikę lub dane.
