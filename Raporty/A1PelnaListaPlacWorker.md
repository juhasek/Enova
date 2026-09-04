# A1PelnaListaPlacWorker — generowanie xlsx z prawdziwym auto-fit przez EPPlus

**Status: NIEPRZETESTOWANE, WYSOKIE RYZYKO** — to nowe podejście, alternatywne wobec
pseudo-autofit w samym `A1PelnaListaPlacSnippet`, wprowadzone na wyraźną prośbę
użytkownika po tym, jak zrzut z Excela pokazał, że natywne poszerzanie kolumn w
DevExpress (`Weight`/`WidthF` + poszerzenie strony) nadal daje niespójne/rozjeżdżające
się szerokości kolumn. Ma **kilka niezależnych, niepotwierdzonych założeń na raz** —
zob. „Do sprawdzenia w tej kolejności” niżej, zanim zaczniesz cokolwiek debugować.

## Co robi

Dokłada do listy **Płace → Listy płac** pozycję w menu Czynności — „Generuj pełną listę
płac (xlsx, auto-fit EPPlus)" — działającą na zaznaczonych pozycjach. W odróżnieniu od
zwykłego wydruku (`A1PelnaListaPlacSnippet`, eksportowanego ręcznie z podglądu wydruku):

1. Generuje plik `.xlsx` **programowo** przez `IReportService.GenerateReport(...)` —
   używa **tego samego** wzorca `A1PelnaListaPlac.repx` i tego samego snippetu, więc cała
   logika liczenia kolumn/wierszy (dynamiczne elementy, 17 kolumn podsumowania ZUS/PPK/
   PIT) jest **w 100% ponownie wykorzystana**, nic nie jest duplikowane.
2. Dostaje z tego wywołania gotowe **surowe bajty** pliku `.xlsx` (`Stream`) — w
   odróżnieniu od eksportu z podglądu wydruku, gdzie DevExpress sam zapisuje plik i nie
   ma zdarzenia z dostępem do gotowych bajtów „po fakcie" (dlatego EPPlus był wcześniej
   wycofany dla `Raporty/SeopEnrollmentFile`, zob. tamten `.md` p.6).
3. Otwiera te bajty jako `ExcelPackage` (EPPlus) i wywołuje `AutoFitColumns()` na całym
   użytym zakresie — to **prawdziwy** auto-fit (EPPlus realnie mierzy renderowany
   tekst), nie przybliżenie „długość znaku × stała" jak w pseudo-autofit samego snippetu.
   Ustawia też stałą wysokość wierszy (18).
4. Zwraca doformatowany plik jako `NamedStream` — operator dostaje go do pobrania.

## Dlaczego to inne podejście niż w SeopEnrollmentFile (który był wycofany)

`Raporty/SeopEnrollmentFileWorker` (commit `05e0824`) robił dokładnie to samo dla
raportu SEOP i został wycofany (`5ac1a17`) — **ale commit wycofujący nie podaje
technicznego powodu** („zostaw tylko zsynchronizowaną wersję raportu"), nie ma dowodu,
że EPPlus tam faktycznie nie zadziałał. Ten plik przywraca ten wzorzec 1:1 (ta sama
struktura: `IReportService.GenerateReport` → `MemoryStream` → `ExcelPackage.AutoFitColumns()`
→ `NamedStream`), na wyraźną prośbę użytkownika, żeby dać mu szansę zadziałać dla tego
konkretnego raportu.

## Do sprawdzenia w TEJ kolejności (każdy kolejny punkt zależy od poprzedniego)

1. **Czy EPPlus (`OfficeOpenXml`) jest w ogóle dostępny w tej instalacji enova.**
   Przeszukałem `C:\enovaServer` i `~/Downloads` z tego repo — **nie znalazłem** żadnego
   pliku `EPPlus.dll`/`OfficeOpenXml.dll`. To nie dowodzi, że go nie ma (mogła nie zostać
   przeszukana właściwa instalacja/GAC, albo biblioteka jest wmergowana w inną DLL), ale
   to sygnał ostrzegawczy. **Pierwszy test:** wklej ten plik w odpowiednim miejscu (patrz
   punkt 2) i sprawdź, czy w ogóle się kompiluje. Błąd typu „nie znaleziono typu lub
   przestrzeni nazw «OfficeOpenXml»" / „Could not load file or assembly" oznacza, że
   biblioteka nie jest zarejestrowana — snippety wklejane w edytorze skryptów **nie mogą**
   dodawać własnych referencji NuGet (nie ma tu pliku `.csproj`), więc to byłby ślepy
   zaułek dla tego podejścia.
2. **Gdzie w GUI enova wkleja się kod klasy `Worker`.** To rozszerzenie **globalne**
   dodatku (atrybut `[assembly: Worker<...>]`), nie kod przypięty do jednego konkretnego
   wzorca wydruku — inne miejsce niż „Kod źródłowy" wydruku, którego używaliśmy dla
   `A1PelnaListaPlacSnippet`. W tym repo **nie ma jeszcze potwierdzonego przykładu** takiej
   rejestracji przez GUI tej konkretnej instalacji — prawdopodobnie osobna sekcja typu
   „Kod źródłowy dodatku" / „Rozszerzenia" w Narzędzia → Opcje, ale wymaga sprawdzenia w
   GUI. **Nie wstawiłem tego pliku do `SystemFiles` przez SQL** (jak przy snippecie
   wydruku) — nie znam właściwego `Name`/`RuntimeInfoIdentifier` dla Workera w tej bazie.
3. **Czy `Context[typeof(Row[])]` faktycznie dotrze do `A1PelnaListaPlacSnippet`.**
   Worker jawnie robi `context.Set(zaznaczoneWiersze)` (typ `Row[]`) — dokładnie to, co
   `Report_BeforePrint` czyta (`dc?[typeof(Row[])] as Row[]`), więc **nie** polegamy
   wyłącznie na (niepotwierdzonym w SEOP) automatycznym wypełnianiu przez
   `ReportResult.Rows`. Mimo to wymaga sprawdzenia jednym wygenerowanym plikiem — czy
   wynikowy `.xlsx` faktycznie ma dane zaznaczonych list płac, a nie pustą tabelę.
4. **`TemplateFileName = "A1PelnaListaPlac.repx"`** musi się zgadzać z rzeczywistą nazwą
   wzorca zarejestrowaną w Enova (tak jak wgrałeś go w projektancie wydruków).
5. Jeśli punkty 1–4 przejdą: porównaj wynikowy plik z tym z ręcznego eksportu — czy
   `AutoFitColumns()` faktycznie daje czytelniejszy, spójny układ kolumn (bez „rozjazdu"
   widocznego na wcześniejszym zrzucie z ręcznego eksportu).

## Jeśli EPPlus się nie skompiluje (punkt 1 zawiedzie)

Wtedy jedyna dostępna droga to dalsze poprawianie natywnego pseudo-autofit w samym
`A1PelnaListaPlacSnippet` (`DopasujSzerokosciKolumn`/`DopasujSzerokoscStrony`) — np.
sprawdzenie, czy problem z „rozjazdem" po ostatnim zrzucie z Excela to kwestia
`ExportMode` (`SingleFilePageByPage` vs `SingleFile`) czy czegoś innego w konfiguracji
strony/marginesów, których nie widać bez kolejnego zrzutu/testu na żywo.
