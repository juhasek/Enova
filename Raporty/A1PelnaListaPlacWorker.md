# A1PelnaListaPlacWorker — pełna lista płac prosto do XLSX (auto-fit)

**Cel (życzenie użytkownika):** raport listy płac ma **od razu generować się do
Excela z prawidłowym formatowaniem** — bez ręcznego eksportu z podglądu wydruku i
bez rozjeżdżających się szerokości kolumn.

**Status: kod przepisany i zweryfikowany względem DLL-i serwera (enova 2512.5.6),
NIEPRZETESTOWANY na żywej bazie.** Wymaga wgrania jako „Projekt w bazie" (patrz
„Jak wgrać") i sprawdzenia 3 punktów z sekcji „Do potwierdzenia na żywej bazie".

## Co robi

Dokłada do listy **Płace → Listy płac** czynność w menu **„Pełna lista płac →
XLSX (auto-fit)"**, działającą na **zaznaczonych** pozycjach:

1. Generuje `.xlsx` **programowo** przez `IReportService.GenerateReport(...)` —
   używa **tego samego** wzorca `A1PelnaListaPlac.repx` i **tego samego** snippetu
   `A1PelnaListaPlacSnippet`, więc cała logika liczenia kolumn (dynamiczne
   elementy + 17 stałych kolumn ZUS/PPK/PIT, storno, wydział historyczny) jest
   w 100% ponownie wykorzystana — nic nie jest duplikowane.
2. Otwiera surowe bajty `.xlsx` jako `DevExpress.Spreadsheet.Workbook` i robi
   **prawdziwy** auto-fit: `Columns.AutoFit` / `Rows.AutoFit` na użytym zakresie
   każdego arkusza (mierzy realnie renderowany tekst — nie przybliżenie „długość
   znaku × stała" jak pseudo-autofit w samym snippecie).
3. Zwraca gotowy plik jako `NamedStream` — operator dostaje go do pobrania.

## Dlaczego worker, a nie sam eksport z podglądu

Standardowy eksport z podglądu wydruku Enova (DevExpress) sam zapisuje plik i nie
daje zdarzenia z dostępem do gotowych bajtów „po fakcie" — nie da się go
post-processować. Tutaj `IReportService.GenerateReport` zwraca `Stream` z surowymi
bajtami `.xlsx`, które swobodnie doformatowujemy przed oddaniem operatorowi.

## Dlaczego DevExpress.Spreadsheet, nie EPPlus

Przeszukanie folderu bibliotek serwera
(`C:\enovaServer\2512.5.6\Soneta.Products.Server.Standard\`) — **EPPlus/OfficeOpenXml
tam nie ma**. Dokładanie nowej DLL do współdzielonego folderu serwera (obsługuje
też inne bazy) to inwazyjna, trudna do odwrócenia zmiana z restartem usługi —
świadomie tego nie robimy. Za to w tym samym folderze już leżą
`DevExpress.Docs.v24.1.dll` (klasa `Workbook`) i
`DevExpress.Spreadsheet.v24.1.Core.dll` (`ColumnCollection.AutoFit`,
`Worksheet.GetUsedRange` itd.) — biblioteka tego samego dostawcy, funkcjonalny
odpowiednik EPPlus, nic nie trzeba dokładać.

## Zweryfikowane na DLL-ach serwera (ilspycmd, 2026-09-07)

- `IReportService` (`Soneta.Business.UI`): `Stream GenerateReport(ReportResult)`,
  `Type[] GetParameterTypes(string, Context)`. Zarejestrowany jako
  `[Service(typeof(IReportService), typeof(ReportServiceImpl), ServiceScope.Session)]`
  → `session.GetRequiredService<IReportService>()` działa.
- `ReportFormats` (`Soneta.Business.UI`) **ma wartość `XLSX`** (obok
  `XLS/CSV/DOCX/RTF/PDF/HTML/...`). `DxReportGenerator.BuildReport`:
  `ReportFormats.XLSX` → `Report.ExportToXlsx(stream, new XlsxExportOptions())`.
- **`GetParameterTypes` zwraca dokładnie typy property `[Context(Required = true)]`
  z klas `ReportSnippet` w tym `.repx`** (`DxQueryParameters.Analyse`) — czyli
  `{ typeof(A1PelnaListaPlacSnippet.PrnParams) }`.
- **Bez wstawienia instancji tego typu do kontekstu** `ReportServiceImpl.Generate`
  zwraca `QueryContextInformation`, a `GenerateReport` rzuca „Problem z
  przygotowaniem raportu"; dodatkowo snippet i tak wszedłby w gałąź „designer"
  (`pars == null` → wczesny `return`) i wygenerował **pustą tabelę**. Dlatego
  worker robi pętlę: `foreach (Type t in GetParameterTypes(...)) ctx.Set(Activator.CreateInstance(t, ctx))`.
- `ReportResult.CheckConsistency(reportService: true)` **rzuca, gdy ustawiony jest
  `OutputHandler`** → przy wywołaniu przez `IReportService` nie wolno go ustawiać;
  post-processing robimy po odebraniu strumienia (tak jest w kodzie).
- `Context.Set(object)` kluczuje po `value.GetType()` → zaznaczenie musi mieć
  runtime-typ `Row[]` (stąd `Cast<Row>().ToArray()`), żeby snippetowe
  `dc?[typeof(Row[])]` je znalazło.
- `DevExpress.Spreadsheet`: klasa konkretna `Workbook` (public ctor,
  `LoadDocument`/`SaveDocument(Stream, DocumentFormat)`) → `DevExpress.Docs.v24.1.dll`;
  `ColumnCollection.AutoFit(int first, int last)`, `RowCollection.AutoFit(int, int)`,
  `Worksheet.GetUsedRange()` → `CellRange` (`LeftColumnIndex`/`RightColumnIndex`/
  `TopRowIndex`/`BottomRowIndex`) → `DevExpress.Spreadsheet.v24.1.Core.dll`.
  Oba pliki są w folderze serwera.

## Jak wgrać — Projekt kodu w bazie (nie „Kod źródłowy" wydruku!)

Worker to **rozszerzenie globalne** (`[assembly: Worker<...>]`) — inne miejsce niż
snippet wydruku. W bazie `Claude` mechanizm „kodu w bazie" jest już aktywny:
tabele `RuntimeProjects` (projekty) + `CodeFiles` (pliki źródłowe), np. gotowe
projekty użytkownika `Soneta.Runtime.Database.KadryPlace` (ID 13),
`Soneta.Runtime.Database.Handel` itd. (Solution 2 = „kod encji użytkownika").

1. Enova → **Narzędzia → Opcje → Ogólne → Programista** — upewnij się, że obsługa
   projektów w bazie jest włączona (w bazie `Claude` już są w niej pliki, więc
   powinna być).
2. W edytorze projektów w bazie otwórz projekt użytkownika dla kadr/płac
   (namespace `Soneta.Runtime.Database.KadryPlace`) — albo dowolny projekt
   „Database" z modułem Płace w referencjach.
3. Dodaj nowy plik źródłowy (np. `A1PelnaListaPlacWorker.cs`) i wklej całą
   zawartość pliku `Raporty/A1PelnaListaPlacWorker` z repo.
4. Zapisz / przelicz projekt — enova skompiluje kod. **Jeśli poleci błąd
   kompilacji `nie znaleziono typu lub przestrzeni nazw DevExpress.Spreadsheet`** —
   ten konkretny kompilator nie referencjonuje `DevExpress.Docs`/
   `DevExpress.Spreadsheet` (mimo obecności na dysku). Wtedy: albo dodać
   referencję do projektu (jeśli edytor projektów na to pozwala), albo wariant
   awaryjny (niżej).
5. Zrestartuj usługę enova (globalne rozszerzenia ładują się przy starcie) i wejdź
   na **Płace → Listy płac**, zaznacz 1+ pozycji → menu Czynności →
   **„Pełna lista płac → XLSX (auto-fit)"**.

Snippet `A1PelnaListaPlacSnippet` i wzorzec `A1PelnaListaPlac.repx` muszą być
**już wgrane** (patrz `A1PelnaListaPlac.md` → „Jak wgrać") — worker tylko je
wywołuje.

## Do potwierdzenia na żywej bazie (w tej kolejności)

1. **Kompilacja** — czy projekt w bazie w ogóle się kompiluje (referencje
   `DevExpress.Docs` / `DevExpress.Spreadsheet`, `Microsoft.Extensions.DependencyInjection`
   dla `GetRequiredService`).
2. **`NazwaWzorca` + `TemplateFileSource`** — `"A1PelnaListaPlac.repx"` +
   `AspxSource.Storage` musi trafić we wzorzec zarejestrowany w Projektancie
   wydruków. Jeśli `GenerateReport` nie znajdzie wzorca: spróbuj nazwy **bez**
   `.repx`, albo `AspxSource.Local`.
3. **Dane w pliku** — czy wynikowy `.xlsx` ma dane zaznaczonych list płac (czyli
   `Context[typeof(Row[])]` dotarł do snippetu), a nie pustą tabelę / wiersz
   `BRAK WYPŁAT`.
4. Porównaj wynik z ręcznym eksportem z podglądu — czy `AutoFit` daje wyraźnie
   czytelniejszy, spójny układ kolumn (bez „rozjazdu" z wcześniejszego zrzutu).

## Wariant awaryjny (gdy pkt 1 lub 2 zawiedzie)

- **Nie kompiluje się `DevExpress.Spreadsheet`** → zostaw sam snippet
  `A1PelnaListaPlacSnippet` z jego natywnym pseudo-autofit
  (`DopasujSzerokosciKolumn`/`DopasujSzerokoscStrony`) i eksportuj ręcznie z
  podglądu; dalej dopracowuj tam szerokości (to jedyna ścieżka, która już działa
  end-to-end).
- **Nie da się wgrać globalnego workera przez GUI** (brak edytora projektów w
  bazie w tej licencji) → trzeba skompilowany dodatek `.csproj`
  (`dotnet new soneta-addon`, deploy DLL do folderu serwera, restart) — większa
  operacja, wymaga osobnej zgody.
