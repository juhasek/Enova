# A1PelnaListaPlacWorker — generowanie xlsx z prawdziwym auto-fit (DevExpress.Spreadsheet)

**Status: NIEPRZETESTOWANE, WYSOKIE RYZYKO** — to nowe podejście, alternatywne wobec
pseudo-autofit w samym `A1PelnaListaPlacSnippet`, wprowadzone na wyraźną prośbę
użytkownika po tym, jak zrzut z Excela pokazał, że natywne poszerzanie kolumn w
DevExpress (`Weight`/`WidthF` + poszerzenie strony) nadal daje niespójne/rozjeżdżające
się szerokości kolumn. Ma **kilka niezależnych, niepotwierdzonych założeń na raz** —
zob. „Do sprawdzenia w tej kolejności” niżej, zanim zaczniesz cokolwiek debugować.

## Dlaczego DevExpress.Spreadsheet, nie EPPlus

Użytkownik poprosił wprost o EPPlus. Zamiast dokładać nową bibliotekę, najpierw
sprawdziłem, czy jest już dostępna: przeszukałem folder z bibliotekami tego serwera
enova (`C:\enovaServer\2512.5.6\Soneta.Products.Server.Standard\`, **442 pliki DLL** —
stąd proces serwera ładuje **wszystkie** swoje referencje, w tym już używany
`DevExpress.XtraReports.v24.1.dll`) — **EPPlus/OfficeOpenXml tam nie ma**.

Ręczne dołożenie nowego pliku DLL do tego folderu to zmiana **współdzielonej
infrastruktury serwera** (obsługuje prawdopodobnie też inne bazy, nie tylko `Claude`) —
inwazyjna, trudna do odwrócenia, i niemal na pewno wymagałaby restartu usługi enova
(co przerwałoby pracę innym użytkownikom, jeśli tacy są). To nie jest coś, co robię bez
wyraźnej zgody na ryzyko — a przede wszystkim: **nie było potrzeby**, bo w tym samym
folderze już leży `DevExpress.Spreadsheet.v24.1.Core.dll` — biblioteka tego samego
dostawcy (DevExpress) do tworzenia/odczytu/edycji plików `.xlsx`, funkcjonalnie
odpowiadająca EPPlus (`Workbook`/`Worksheet`, `AutoFitColumns()`/`AutoFitRows()`) — i
prawdopodobnie już referencjonowana przez ten sam kompilator, który kompiluje
`A1PelnaListaPlacSnippet` (ten sam dostawca, ten sam folder, ten sam mechanizm
kompilacji „Kod źródłowy").

**Jeśli mimo to chcesz koniecznie EPPlus** (np. bo masz pewność, że jest gdzieś
dostępny w tej instalacji, czego ja nie znalazłem) — daj znać, gdzie, albo potwierdź, że
się zgadzasz na ręczne dołożenie pliku DLL do folderu serwera (z restartem usługi) — wtedy
podmienię `SformatujExcel` z powrotem na `OfficeOpenXml`/`ExcelPackage` (kod z historii
commitów, `Raporty/SeopEnrollmentFileWorker`, jest gotowy do przywrócenia).

## Co robi

Dokłada do listy **Płace → Listy płac** pozycję w menu Czynności — „Generuj pełną listę
płac (xlsx, auto-fit)" — działającą na zaznaczonych pozycjach. W odróżnieniu od zwykłego
wydruku (`A1PelnaListaPlacSnippet`, eksportowanego ręcznie z podglądu wydruku):

1. Generuje plik `.xlsx` **programowo** przez `IReportService.GenerateReport(...)` —
   używa **tego samego** wzorca `A1PelnaListaPlac.repx` i tego samego snippetu, więc cała
   logika liczenia kolumn/wierszy (dynamiczne elementy, 17 kolumn podsumowania ZUS/PPK/
   PIT) jest **w 100% ponownie wykorzystana**, nic nie jest duplikowane.
2. Dostaje z tego wywołania gotowe **surowe bajty** pliku `.xlsx` (`Stream`) — w
   odróżnieniu od eksportu z podglądu wydruku, gdzie DevExpress sam zapisuje plik i nie
   ma zdarzenia z dostępem do gotowych bajtów „po fakcie".
3. Otwiera te bajty jako `DevExpress.Spreadsheet.Workbook` i wywołuje
   `Worksheet.AutoFitColumns()`/`AutoFitRows()` na całym arkuszu — to **prawdziwy**
   auto-fit (mierzy realnie renderowany tekst), nie przybliżenie „długość znaku × stała"
   jak w pseudo-autofit samego snippetu.
4. Zwraca doformatowany plik jako `NamedStream` — operator dostaje go do pobrania.

## Architektura wzorowana na wycofanym SeopEnrollmentFileWorker

`Raporty/SeopEnrollmentFileWorker` (commit `05e0824`) robił to samo dla raportu SEOP
(tam z EPPlus) i został wycofany (`5ac1a17`) — **ale commit wycofujący nie podaje
technicznego powodu** („zostaw tylko zsynchronizowaną wersję raportu”), nie ma dowodu,
że post-processing tam faktycznie nie zadziałał. Ten plik przywraca strukturę 1:1
(`IReportService.GenerateReport` → `MemoryStream` → post-processing → `NamedStream`),
tylko z inną biblioteką formatującą.

## Do sprawdzenia w TEJ kolejności (każdy kolejny punkt zależy od poprzedniego)

1. **Czy `DevExpress.Spreadsheet` jest referencjonowany przez kompilator kodu
   snippetów** (nie tylko obecny na dysku obok innych DLL) — wysoce prawdopodobne (ten
   sam dostawca, ten sam folder co już działający `DevExpress.XtraReports`), ale
   niepotwierdzone. **Pierwszy test:** wklej ten plik w odpowiednim miejscu (patrz punkt
   3) i sprawdź, czy w ogóle się kompiluje. Błąd typu „nie znaleziono typu lub przestrzeni
   nazw «DevExpress.Spreadsheet»” oznaczałby, że mimo obecności na dysku nie jest
   referencjonowany przez ten konkretny kompilator — wtedy wracamy do EPPlus (patrz wyżej)
   albo do dalszego poprawiania pseudo-autofit w samym snippecie.
2. **Dokładne nazwy/zachowanie `Worksheet.AutoFitColumns()`/`AutoFitRows()`**
   (bezparametrowe warianty = cały użyty zakres) — zgodne z ogólną wiedzą o API
   DevExpress Spreadsheet, ale nie zweryfikowane na wersji v24.1 konkretnie w tej
   instalacji. Jeśli te konkretne przeciążenia nie istnieją, kompilator wskaże błędną
   nazwę/sygnaturę.
3. **Gdzie w GUI enova wkleja się kod klasy `Worker`.** To rozszerzenie **globalne**
   dodatku (atrybut `[assembly: Worker<...>]`), nie kod przypięty do jednego konkretnego
   wzorca wydruku — inne miejsce niż „Kod źródłowy” wydruku, którego używaliśmy dla
   `A1PelnaListaPlacSnippet`. W tym repo **nie ma jeszcze potwierdzonego przykładu** takiej
   rejestracji przez GUI tej konkretnej instalacji — prawdopodobnie osobna sekcja typu
   „Kod źródłowy dodatku” / „Rozszerzenia” w Narzędzia → Opcje, ale wymaga sprawdzenia w
   GUI. **Nie wstawiłem tego pliku do `SystemFiles` przez SQL** (jak przy snippecie
   wydruku) — nie znam właściwego `Name`/`RuntimeInfoIdentifier` dla Workera w tej bazie.
4. **Czy `Context[typeof(Row[])]` faktycznie dotrze do `A1PelnaListaPlacSnippet`.**
   Worker jawnie robi `context.Set(zaznaczoneWiersze)` (typ `Row[]`) — dokładnie to, co
   `Report_BeforePrint` czyta (`dc?[typeof(Row[])] as Row[]`), więc **nie** polegamy
   wyłącznie na (niepotwierdzonym w SEOP) automatycznym wypełnianiu przez
   `ReportResult.Rows`. Mimo to wymaga sprawdzenia jednym wygenerowanym plikiem — czy
   wynikowy `.xlsx` faktycznie ma dane zaznaczonych list płac, a nie pustą tabelę.
5. **`TemplateFileName = "A1PelnaListaPlac.repx"`** musi się zgadzać z rzeczywistą nazwą
   wzorca zarejestrowaną w Enova (tak jak wgrałeś go w projektancie wydruków).
6. Jeśli punkty 1–5 przejdą: porównaj wynikowy plik z tym z ręcznego eksportu — czy
   auto-fit faktycznie daje czytelniejszy, spójny układ kolumn (bez „rozjazdu” widocznego
   na wcześniejszym zrzucie z ręcznego eksportu).

## Jeśli DevExpress.Spreadsheet się nie skompiluje (punkt 1 zawiedzie)

Wtedy albo (a) sprawdzić, czy EPPlus jest **jednak** gdzieś dostępny w tej instalacji
(np. inna wersja/instalacja enova, inny serwer) i wskazać mi to, albo (b) dalej
poprawiać natywny pseudo-autofit w samym `A1PelnaListaPlacSnippet`
(`DopasujSzerokosciKolumn`/`DopasujSzerokoscStrony`) — np. sprawdzenie, czy problem z
„rozjazdem” po zrzucie z Excela to kwestia `ExportMode` (`SingleFilePageByPage` vs
`SingleFile`) czy czegoś innego w konfiguracji strony/marginesów.
