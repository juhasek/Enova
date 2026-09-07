# A1PelnaListaPlacWorker — pełna lista płac prosto do XLSX

**Cel (życzenie użytkownika):** raport listy płac ma **od razu generować się do
Excela z prawidłowym formatowaniem** — bez ręcznego eksportu z podglądu wydruku,
bez rozjeżdżających się / jednoznakowych kolumn.

**Status: kod napisany i zweryfikowany względem DLL-i serwera (enova 2512.5.6),
NIEPRZETESTOWANY na żywej bazie.** Wgrać jako „Projekt kodu w bazie" (patrz „Jak
wgrać") i sprawdzić punkty z „Do potwierdzenia".

## Dlaczego całkiem nowe podejście (nie eksport wydruku)

Zrzut z Excela (2026-09-07) pokazał, że eksport wzorca `A1PelnaListaPlac.repx`
(snippet `A1PelnaListaPlacSnippet`) z podglądu wydruku daje **nieczytelny plik**:
kolumny tekstowe (Imię i Nazwisko, Wydział, pierwsze składniki) ściśnięte do
1 znaku, nagłówki łamane litera-po-literze, arkusz rozbity na dziesiątki wąskich
kolumn Excela (`B D F I J L …`). Przyczyna jest **w samym mechanizmie eksportu
`XRTable` → Excel**: DevExpress tworzy drobnoziarnistą siatkę kolumn i scalone
komórki, gdy krawędzie komórek tabeli nagłówka i tabeli danych nie trafiają
idealnie w ten sam raster. Pseudo-autofit w snippecie (`Weight`/`WidthF` +
poszerzanie strony) tego **nie naprawia** — walczy z fragmentacją, nie z jej
źródłem.

Dlatego ten worker **nie używa wzorca `.repx` ani snippetu**. Buduje plik `.xlsx`
**od zera** biblioteką `DevExpress.Spreadsheet` — pisze czystą siatkę komórek
(1 nagłówek + N wierszy danych), więc fragmentacja nie ma prawa wystąpić, a
`Columns.AutoFit` mierzy realnie renderowany tekst.

Logika danych (kolumny dynamiczne per definicja elementu, 17 stałych kolumn
ZUS/PPK/PIT, pomijanie storna, wydział historyczny wg daty wypłaty) jest
przeniesiona z `A1PelnaListaPlacSnippet` 1:1 — ten sam kontrakt kolumn.

## Co robi

Czynność w menu **Płace → Listy płac** → „Pełna lista płac → XLSX", na
**zaznaczonych** pozycjach:

1. Zbiera wszystkie `Wyplata` z zaznaczonych `ListaPlac`.
2. Wyznacza kolumny dynamiczne — unikalne `WypElement.Definicja` (bez storna),
   alfabetycznie.
3. Buduje wiersze: `Kod` / `Imię i Nazwisko` / `Wydział` + po jednej kolumnie na
   definicję elementu (suma `WypElement.Wartosc`) + 17 stałych kolumn z
   `WypElement.Podatki.*` (ZUS 5×pracownik/pracodawca, FP, FGŚP, FEP, PPK
   pracownik/pracodawca, Zaliczka na PIT) + `Kwota do wypłaty` = `Wyplata.Wartosc`.
4. Zapisuje `.xlsx`: nagłówek pogrubiony z zawijaniem, liczby jako **prawdziwe
   liczby** z formatem `#,##0.00`, obramowania, zablokowany nagłówek (`FreezeRows`),
   autofiltr, `Columns.AutoFit` z ograniczeniem szerokości do 42 znaków
   (i min. 12 znaków dla kolumn liczbowych).
5. Zwraca `NamedStream` — operator pobiera plik.

Błąd przy jednej wypłacie (np. brak etatu/wydziału) wpada jako `[BŁĄD: …]` w tej
jednej komórce tekstowej, nie wywala całości.

## Zweryfikowane na DLL-ach serwera (ilspycmd, 2026-09-07)

- `DevExpress.Spreadsheet`: klasa `Workbook` (public ctor, `SaveDocument(Stream,
  DocumentFormat)`) → `DevExpress.Docs.v24.1.dll`; `Worksheet` / `Cell` /
  `CellRange` / `Column` / `Formatting` (`Cell : CellRange : Formatting` —
  `Font`/`Alignment`/`Borders`/`NumberFormat` bezpośrednio na komórce),
  `WorksheetCollection.ActiveWorksheet`, `Columns.AutoFit(int,int)`,
  `Column.WidthInCharacters`, `Rows[int].Height`, `Worksheet.FreezeRows`,
  `SheetAutoFilter.Apply`, `IRangeProvider.FromLTRB` →
  `DevExpress.Spreadsheet.v24.1.Core.dll`. Oba pliki w folderze serwera obok
  używanego `DevExpress.XtraReports`.
- `CellValue` ma `implicit operator` z `decimal` / `string` / `DateTime`.
- `Borders.SetAllBorders(System.Drawing.Color, BorderLineStyle)`.
- Pola `Wyplata` / `WypElement` / `Podatki` — potwierdzone w `data/props/Place/*`
  skilla soneta-programming i zgodne z **działającym** `A1PelnaListaPlacSnippet`.

## Jak wgrać — SystemFile typu „Snippet" (NIE własne Rozwiązanie)

Worker to **rozszerzenie globalne** (`[assembly: Worker<…>]`). Kluczowe ustalenie
z dekompilacji `BusinessCompilerRoslyn` (2026-09-07): **kompilator wycina
referencje `DevExpress.*` dla każdego projektu poza wbudowanym
`Soneta.Runtime.Reports`** (filtr `project.Guid == BusinessModule.ReportsBusinessGuid
|| !FullName.StartsWith("DevExpress.")`). Własne „Rozwiązanie"/projekt (np.
`A1.Testy`) **nie skompiluje** `using DevExpress.Spreadsheet`.

Za to **każdy `SystemFile` domyślnie kompiluje się do projektu
`Soneta.Runtime.Reports`** (`BusinessModule.GetDefaultProject`: `row is SystemFile
→ ReportsBusinessGuid`) — tam DevExpress JEST. Dlatego worker wchodzi jako
`SystemFile` typu **`Snippet`** (`SystemFileTypes.Snippet` = 2, „kod C#" — nie
`DxSnippet`=1 „wydruk").

**Zrobione 2026-09-07:** wiersz wstawiony SQL-em do `SystemFiles` w bazie `Claude`
(`ID 5`, `FileType=2`, `Name='A1PelnaListaPlacWorker'`,
`RuntimeInfoIdentifier='x.x.Reports.A1PelnaListaPlacWorker'`, `Code` = zawartość
`Raporty/A1PelnaListaPlacWorker`, polskie znaki potwierdzone punktami kodowymi).
Przepis: [[reference_systemfiles_sql_insert]].

**Pozostało (po stronie użytkownika, na żywej enovie):**
1. **Narzędzia → Opcje → Systemowe → Kompilator → „Wymuś rekompilację kodu"**.
2. Restart usług enova (rozszerzenia globalne `[assembly: Worker]` ładują się przy
   starcie procesu).
3. **Płace → Listy płac** → zaznacz 1+ pozycji → Czynności →
   **„Pełna lista płac → XLSX"**.

Aktualizacja kodu w przyszłości: podmień `Code` w `SystemFiles.ID=5` (SQL wg
przepisu) albo wklej nową treść w edytorze snippetu w GUI; potem znów „Wymuś
rekompilację" + restart.

Wycofanie: `DELETE FROM SystemFiles WHERE Name='A1PelnaListaPlacWorker'` +
rekompilacja + restart.

## Do potwierdzenia na żywej bazie (w tej kolejności)

1. **Kompilacja** — czy `SystemFile` „Snippet" w projekcie `Soneta.Runtime.Reports`
   widzi `DevExpress.Docs`/`DevExpress.Spreadsheet` oraz pola `Wyplata`/`WypElement`.
   Błąd kompilacji pojawi się przy „Wymuś rekompilację" / pierwszym użyciu.
2. **Rejestracja czynności** — czy `[assembly: Worker<…>]` z runtime-`SystemFile`
   faktycznie dokłada pozycję do menu Czynności listy „Listy płac" (workery z kodu
   w bazie są wspierane, ale to pierwszy taki w tym wdrożeniu).
3. **Zaznaczenie** — czy `[Context] ListaPlac[]` dostaje zaznaczone pozycje.
4. **Zgodność liczb** — sumy `WypElement.Podatki.*` w kolumnach ZUS/PPK/PIT
   zgadzają się z paskiem wypłaty (na realnie przeliczonej liście).
5. **Czytelność** — kolumny dopasowane do treści, liczby sumowalne w Excelu
   (`=SUMA(...)` działa), nagłówek zablokowany, autofiltr aktywny.

## Wariant awaryjny

- **Nie kompiluje się `DevExpress.Spreadsheet` nawet jako `SystemFile`** (mimo że
  trafia do projektu `Soneta.Runtime.Reports`) → skompilowany dodatek `.csproj`
  (`dotnet new soneta-addon`, DLL do folderu serwera, restart) hostujący ten sam
  kod — wymaga osobnej zgody.
- **`[assembly: Worker]` z runtime-`SystemFile` nie rejestruje czynności** →
  jw. (dodatek `.csproj`).
- W ostateczności: zostaje sam `A1PelnaListaPlacSnippet` + ręczny eksport z
  podglądu (mniej czytelny — patrz „Dlaczego całkiem nowe podejście").
