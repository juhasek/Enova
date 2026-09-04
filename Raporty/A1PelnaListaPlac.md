# A1PelnaListaPlac — pełna lista płac wg elementów (Płace / Listy płac)

**Status: DZIAŁA na bazie testowej `Claude`** (potwierdzone przez użytkownika, 2026-09-05) —
wersja z 6 kolumnami podsumowania (Składki ZUS pracownik/pracodawca, PPK pracownik/pracodawca,
Zaliczka na PIT, Kwota do wypłaty) generuje się poprawnie po ręcznym wklejeniu kodu w
„Kod źródłowy” projektanta wydruków i zapisaniu (patrz „Jak wgrać”). **NIEPRZETESTOWANE
od 2026-09-05:** rozbicie ZUS na 5 szczegółowych rodzajów składek (17 kolumn podsumowania
łącznie, patrz niżej) oraz pseudo-autofit szerokości kolumn/wysokości wierszy przy
eksporcie do Excela (`TextExportMode.Value`, `TextFormatString`, `DopasujSzerokosciKolumn`,
`UstawWysokoscWierszy`) — nowa, jeszcze niepotwierdzona na żywo zmiana.

Wzorzec wydruku Enova (`Raporty/A1PelnaListaPlac.repx`) zasilany snippetem
`Raporty/A1PelnaListaPlacSnippet` (klasa `A1PelnaListaPlacSnippet`), uruchamiany z
poziomu **Płace → Listy płac** na zaznaczonej pozycji (lub kilku pozycjach) listy płac.

## Stan w bazie `Claude` (na dziś)

Kod snippetu jest już wstawiony do tabeli `SystemFiles` w bazie `Claude`
(`Name='A1PelnaListaPlac'`, `RuntimeInfoIdentifier='x.x.Reports.A1PelnaListaPlacSnippet'`)
— wstawiony bezpośrednio SQL-em, zweryfikowany na poziomie punktów kodowych Unicode
(zob. pamięć „SystemFiles przez SQL”). **To NIE wystarcza, żeby wydruk pojawił się w
GUI enova** — w przeciwieństwie do `A1NaglowekListaSnippet` (który nadpisuje kod
**istniejącego, fabrycznie zarejestrowanego** wydruku „nagłówek - lista”), ten wydruk
jest **całkiem nowy** i nie ma jeszcze żadnego zarejestrowanego layoutu `.repx`, do
którego mógłby się „przypiąć” kod z `SystemFiles`. Nie znalazłem w schemacie SQL tabeli
przechowującej samodzielny nowy layout `.repx` — musi zostać zaimportowany przez
projektanta wydruków w GUI enova (sekcja „Jak wgrać” niżej). Dopiero po imporcie
`.repx` z `ComponentStorage/ReportSnippetComponent/SnippetTypeName` wskazującym na
`x.x.Reports.A1PelnaListaPlacSnippet,x.x.Reports` wydruk powinien znaleźć i skompilować
kod już czekający w `SystemFiles`.

## Co pokazuje

Jeden wiersz = jedna wypłata (`Wyplata`) wchodząca w skład zaznaczonej listy/list płac:

| Kolumna | Źródło |
|---|---|
| Kod | `Wyplata.Pracownik.Kod` |
| Imię i Nazwisko | `Wyplata.Pracownik.NazwiskoImię` |
| Wydział | `Pracownik.Historia[Wyplata.Data].Etat.Wydzial.Nazwa` |
| *(dynamicznie, po jednej na każdy rodzaj elementu)* | suma `WypElement.Wartosc` dla danej `WypElement.Definicja` na tej wypłacie |
| Emerytalna (pracownik) / (pracodawca) | suma `WypElement.Podatki.Emerytalna.Prac` / `.Firma` |
| Rentowa (pracownik) / (pracodawca) | suma `WypElement.Podatki.Rentowa.Prac` / `.Firma` |
| Chorobowa (pracownik) / (pracodawca) | suma `WypElement.Podatki.Chorobowa.Prac` / `.Firma` (w Polsce `.Firma` zwykle 0 — chorobowe w całości po stronie pracownika) |
| Wypadkowa (pracownik) / (pracodawca) | suma `WypElement.Podatki.Wypadkowa.Prac` / `.Firma` (w Polsce `.Prac` zwykle 0 — wypadkowe w całości po stronie pracodawcy) |
| Zdrowotna (pracownik) / (pracodawca) | suma `WypElement.Podatki.Zdrowotna.Prac` / `.Firma` (w Polsce `.Firma` zwykle 0 — zdrowotna w całości po stronie pracownika) |
| Fundusz Pracy | suma `WypElement.Podatki.FP.Skladka` (w całości pracodawca) |
| FGŚP | suma `WypElement.Podatki.FGSP.Skladka` (w całości pracodawca) |
| FEP | suma `WypElement.Podatki.FEP.Skladka` (w całości pracodawca) |
| PPK (pracownik) | suma `WypElement.Podatki.PPK.Pracownika` po wszystkich składnikach wypłaty |
| PPK (pracodawca) | suma `WypElement.Podatki.PPK.Pracodawcy` po wszystkich składnikach wypłaty |
| Zaliczka na PIT | suma `WypElement.Podatki.ZalFIS` po wszystkich składnikach wypłaty |
| Kwota do wypłaty | `Wyplata.Wartosc` (kwota netto — pole całej wypłaty, NIE suma elementów) |

Kolumny elementów **nie są zaszyte na stałe** — snippet przegląda wszystkie wypłaty ze
wszystkich zaznaczonych list płac, zbiera unikalny zbiór `DefinicjaElementu` występujących
na jakiejkolwiek z nich (sortowany alfabetycznie wg nazwy) i dopiero wtedy buduje tabelę.
Jeśli jeden pracownik ma np. premię, a inny nie — kolumna „Premia” pojawia się dla obu,
z wartością `0,00` u tego, który jej nie ma.

Elementy wystornowane/stornujące (`WypElement.RozliczenieStorna == true`) są pomijane przy
liczeniu sum i przy wykrywaniu kolumn — inaczej wartość pierwotna i jej storno liczyłyby
się podwójnie.

**Siedemnaście ostatnich kolumn są STAŁE, nie dynamiczne** — w przeciwieństwie do kolumn
elementów, ZUS/narzuty/PPK/PIT **nie są odrębnymi `WypElement`** tylko wartościami
zaszytymi w strukturze `WypElement.Podatki` **każdego** składnika wypłaty (osobno część
pracownika `Prac`/`Pracownika` i część pracodawcy `Firma`/`Pracodawcy`; FP/FGŚP/FEP mają
tylko stronę pracodawcy), więc snippet sumuje je po wszystkich składnikach zamiast
wykrywać jako osobną kolumnę per definicja. „Kwota do wypłaty” to z kolei wprost pole
`Wyplata.Wartosc` (jedna wartość na wypłatę, nie suma elementów).

## Eksport do Excela z auto-dopasowaniem kolumn/wierszy — bez EPPlus

Raport ma trafiać docelowo do Excela (eksport z GUI enova), z kolumnami i wierszami od
razu dopasowanymi do treści.

**Dlaczego nie EPPlus:** w tym repo EPPlus był już wypróbowany dla dokładnie tego samego
problemu (auto-fit przy eksporcie do Excela) w `Raporty/SeopEnrollmentFileWorker` i
**wycofany** — zob. `Raporty/SeopEnrollmentFile.md` p.6 oraz historia commitów (`05e0824`
dodanie, `5ac1a17` wycofanie). Powód: standardowy eksport z podglądu wydruku Enova
(DevExpress) sam zapisuje plik `.xlsx` i nie ma udokumentowanego zdarzenia dającego dostęp
do gotowego pliku „po fakcie” — post-processing biblioteką EPPlus (`AutoFitColumns()`) by
tu nie zadziałał. Dodatkowo snippety w tym repo to kod wklejany do wbudowanego edytora
Enova (nie skompilowany projekt `.csproj` z NuGetem), więc `using OfficeOpenXml;`
wymagałoby, żeby DLL EPPlus był już referencjonowany przez samą instalację enova — nie ma
na to potwierdzenia w tej instalacji.

**Rozwiązanie — natywnie przez DevExpress w `Report_BeforePrint`** (ten sam wzorzec co
finalna, zachowana wersja `Raporty/SeopEnrollmentFile`):

- `DopasujSzerokosciKolumn()` — dla każdej kolumny (po indeksie komórki, wspólnym dla
  `tabelaNaglowek` i `tabelaDane`) liczy długość najdłuższego tekstu (nagłówek + wszystkie
  wiersze danych) i ustawia `Weight`/`WidthF` **wszystkich** komórek tej kolumny w obu
  tabelach na tę samą wartość (`długość znaków × 6,5 + margines 10`, przycięte do 40–400) —
  inaczej siatka kolumn nagłówka i danych rozjedzie się w wyeksportowanym Excelu. Prostsze
  niż w SEOP, bo tu **sami budujemy każdą komórkę w kodzie** (nie trzeba odczytywać
  `ExpressionBindings` przez refleksję — po prostu porównujemy `.Text.Length`).
- `UstawWysokoscWierszy()` — ustawia stałą wysokość (`HeightF = 18`) wszystkich wierszy
  obu tabel.
- `Report.ExportOptions.Xlsx.{ExportMode, RawDataMode, TextExportMode}` — te same 3
  ustawienia co w `SeopEnrollmentFile`: `ExportMode = SingleFilePageByPage`,
  `RawDataMode = false`, `TextExportMode = Value` — każą eksporterowi zapisać wartości
  liczbowe komórek jako prawdziwe liczby Excela (nie tekst), a nie tylko wygenerować plik
  z wyglądem identycznym jak podgląd.
- każda komórka liczbowa (`NowaKomorkaLiczba`) ma dodatkowo ustawiony
  `TextFormatString = "{0:N2}"`, żeby Excel zachował format „2 miejsca po przecinku”.

Pogrubienie nagłówków, obramowania komórek i wyrównanie do prawej (`TextAlignment`) to
standardowe właściwości `XRTableCell`, które DevExpress eksportuje do Excela bez
dodatkowego kodu.

**Ograniczenie względem EPPlus:** to przybliżenie (stała szerokość znaku dla domyślnej
czcionki, nie realny pomiar renderowanego tekstu jak w `AutoFitColumns()`) — powinno dać
wynik „wystarczająco dobry”, ale nie jest identyczne z prawdziwym auto-fit (zob. też
ograniczenie opisane w `SeopEnrollmentFile.md`).

**TODO/UNVERIFIED:** cała ta sekcja (oraz `this.Report` jako sposób dostępu do raportu w
`Report_BeforePrint`) jest wzorowana na kodzie z `SeopEnrollmentFile`, ale nie została
potwierdzona rzeczywistym eksportem do `.xlsx` **tego konkretnego wydruku**. Po wklejeniu
kodu w projektancie sprawdź najpierw, czy się w ogóle kompiluje, a potem wyeksportuj
wydruk do Excela i zweryfikuj: (a) szerokości kolumn i wysokości wierszy są sensownie
dopasowane do treści, (b) liczby są traktowane jako liczby (np. `=SUMA(...)` na kolumnie
działa), (c) widoczne są 2 miejsca po przecinku, (d) nagłówki są pogrubione, (e)
obramowania komórek są zachowane.

## Jak to jest zbudowane (dlaczego nie ma statycznej tabeli w .repx)

Liczba i nazwy kolumn nie są znane w momencie projektowania wydruku, więc `.repx` zawiera
tylko **dwie puste kontrolki `XRTable`**:

- `tabelaNaglowek` w `PageHeaderBand` (nagłówek kolumn, powtarzany na każdej stronie),
- `tabelaDane` w `DetailBand` (wiersze danych).

Snippet w `Report_BeforePrint` sam buduje całą zawartość obu tabel przez API DevExpress
(`XRTableRow`/`XRTableCell` dodawane do `Rows`/`Cells` w kodzie) — to zwykła, w pełni
wspierana funkcjonalność DevExpress XtraReports, niezależna od specyfiki Soneta.

Żeby `DetailBand` odpalił się dokładnie raz (a nie raz na wiersz danych, bo to w tym
wydruku nie ma sensu — cała tabela ze wszystkimi wierszami budowana jest za jednym
zamachem), snippet ustawia `CustomDataSource` na komponencie `BusinessSource`
(`DataKind="Empty"`) na listę z jednym elementem — dokładnie wzorem
`Raporty/SzablonTabela6KolumnSnippet`.

## Skąd bierze dane wejściowe (zaznaczenie)

Podobnie jak `Raporty/SzablonTabela6KolumnSnippet` i `Raporty/OdbiciaRCP`: snippet ma
właściwość `Params` typu `PrnParams : ContextBase` oznaczoną `[Context(Required = true)]`
— to wymusza wstrzyknięcie działającego `Context` (i pokazanie okna parametrów przed
wydrukiem, nawet gdy `PrnParams` jest pusta).

Zaznaczenie pobierane jest przez `Context[typeof(Row[])]`, tak jak w
`SzablonTabela6KolumnSnippet`. Z zaznaczonych wierszy zbierane są `ListaPlac` — bezpośrednio
(gdy wydruk uruchomiono z listy „Płace → Listy płac”) albo pośrednio przez
`Wyplata.ListaPlac` (gdyby ktoś uruchomił wydruk z poziomu listy wypłat). Fallback na
`Context[typeof(ListaPlac)]` (wydruk z poziomu otwartego pojedynczego dokumentu listy
płac, bez zaznaczenia w gridzie) jest **TODO/UNVERIFIED** — do potwierdzenia w GUI, bo nie
ma w tym repo wcześniejszego precedensu na taki dostęp do kontekstu.

## Odporność na błędy

Każde pole wiersza danych liczone jest przez `Bezpiecznie(...)` — błąd dla konkretnej
wypłaty (np. brak etatu/wydziału na dany dzień) wstawia `[BŁĄD: ...]` w tej jednej komórce
zamiast wywalać cały wydruk. Cała metoda `Report_BeforePrint` jest dodatkowo opakowana w
try/catch z etykietą etapu (`etap`) — błąd poza pętlą po wypłatach (np. przy pobieraniu
zaznaczenia) trafia jako pojedynczy wiersz diagnostyczny `BŁĄD | ETAP=... | TYP=... |
KOMUNIKAT=...` w tabeli danych.

## Do weryfikacji po wgraniu (na bazie testowej `Claude`)

1. **Zaznaczenie z listy „Listy płac”** faktycznie trafia do `Context[typeof(Row[])]` jako
   `ListaPlac[]` (a nie np. `PlanowanaListaPłac` czy coś innego) — do potwierdzenia
   uruchomieniem na 1 i na kilku zaznaczonych pozycjach.
2. **Szerokość/czytelność tabeli przy wielu kolumnach elementów** — układ jest w orientacji
   pionowej (A4 portret), szerokość `1704` (0,1 mm), kolumny dzielą się proporcjonalnie
   przez `Weight`. Przy dużej liczbie rodzajów elementów kolumny mogą się zrobić bardzo
   wąskie — w razie potrzeby przełączyć wydruk na `Landscape="true"` w projektancie Enova
   (samo `AnchorHorizontal="Both"` na obu tabelach dociągnie szerokość automatycznie).
3. **`Pracownik.Historia[data].Etat.Wydzial`** — sprawdzić, czy dla wypłat z przeszłości
   (np. skorygowana lista sprzed zmiany wydziału pracownika) wydział pokazuje się
   historycznie poprawny (wg daty wypłaty), a nie bieżący.
4. **Sumowanie kilku elementów o tej samej definicji na jednej wypłacie** (np. składnik
   główny + korekta tego samego dodatku) — obecnie sumowane razem w jednej kolumnie; do
   potwierdzenia, czy to zgodne z oczekiwaniem użytkownika, czy woli osobne kolumny per
   `WypElement` (a nie per `Definicja`).
5. **Storno** (`RozliczenieStorna`) — potwierdzić na przykładzie z realną korektą wypłaty,
   że pomijanie wystornowanych/stornujących elementów daje poprawną (nie podwójną) sumę.
6. **17 kolumn podsumowania** (Emerytalna/Rentowa/Chorobowa/Wypadkowa/Zdrowotna ×
   pracownik/pracodawca, Fundusz Pracy, FGŚP, FEP, PPK pracownik/pracodawca, Zaliczka na
   PIT, Kwota do wypłaty) — potwierdzić na realnie naliczonej wypłacie (nie tylko dodanej
   do kartoteki, ale przeliczonej listą płac), że sumy z `WypElement.Podatki.*` zgadzają
   się z paskiem wypłaty w GUI, w tym że kolumny „zerowe po jednej stronie” (np.
   Chorobowa pracodawca, Wypadkowa pracownik, Zdrowotna pracodawca) faktycznie wychodzą
   0,00 zgodnie z założeniem opisanym w tabeli kolumn wyżej.
7. **Eksport do Excela z auto-dopasowaniem** (`DopasujSzerokosciKolumn`,
   `UstawWysokoscWierszy`, `TextExportMode.Value`, `TextFormatString`, `this.Report`) —
   potwierdzić kompilację kodu w projektancie (compileruje bez błędu — `this.Report` musi
   być poprawną właściwością bazy `ReportSnippet`), a potem że wyeksportowany `.xlsx` ma
   kolumny/wiersze sensownie dopasowane do treści i liczby jako wartości liczbowe (nie
   tekst) z zachowanym formatem — patrz sekcja „Eksport do Excela z auto-dopasowaniem
   kolumn/wierszy — bez EPPlus” wyżej. Przy 17+ kolumnach podsumowania i kolumnach
   elementów tabela będzie szeroka — w Excelu to nieistotne (brak ograniczenia szerokości
   strony), ale sam podgląd wydruku w Enova może wymagać przewijania w poziomie.

## Jak wgrać

1. Enova → Narzędzia → Opcje → Systemowe → Wydruki → **Zaimportuj** `A1PelnaListaPlac.repx`
   (albo dodaj jako nowy wzorzec przypięty do listy „Listy płac”, jeśli import pliku nie
   jest dostępny w tej wersji — zależnie od trybu rejestracji wydruków w tej instalacji).
2. W oknie edycji wydruku znajdź „Kod źródłowy” i wklej tam całą zawartość pliku
   `Raporty/A1PelnaListaPlacSnippet` (kod w `SystemFiles` w bazie `Claude` już czeka pod
   identyfikatorem `x.x.Reports.A1PelnaListaPlacSnippet` — ale to GUI/import decyduje,
   czy go znajdzie i użyje, czy nadpisze własnym zapisem; wklejenie ręczne jest pewniejsze
   i zgodne ze sprawdzonym przepisem z `SzablonTabela6Kolumn`).
3. Zapisz — Enova skompiluje snippet i podepnie go pod wydruk (analogicznie do
   `Raporty/SzablonTabela6Kolumn` — zob. tamten `.md`, sekcja „Jak podpiąć snippet”).
4. Z listy „Płace → Listy płac” zaznacz jedną lub kilka pozycji i uruchom wydruk
   „A1 - Pełna lista płac (wg elementów)”. Powinno pojawić się okno parametrów (puste,
   tylko do zatwierdzenia) — to potwierdza, że `Context` został poprawnie wstrzyknięty.
5. Sprawdź: kolumny Kod/Imię i Nazwisko/Wydział, kolumny elementów dopasowane do
   rzeczywistych dodatków/składników na wypłatach, `0,00` tam gdzie pracownik danego
   elementu nie ma.
