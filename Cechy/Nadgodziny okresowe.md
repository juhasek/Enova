# Nadgodziny okresowe – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy cecha

Cecha wyliczana (typu Kwota/liczba godzin, `decimal`), przypisana do wiersza strefy pracy
(`Row` = `StrefaPracy`). Dla wierszy, których definicja zawiera w nazwie „Praca poza normą" (w tym
warianty typu „Praca poza normą awaria"), wylicza liczbę godzin, które mają zostać rozliczone jako
**nadgodziny okresowe**.

## 2. Zgłoszony błąd i poprawka (27.08.2026)

**Zgłoszenie klienta:** nadgodziny okresowe błędnie się naliczają, gdy „Praca poza normą"/„Praca
poza normą awaria" przypada **w święto**. W święto praca tego typu **nie powinna w ogóle** trafiać
do nadgodzin okresowych — ma być rozliczana osobną cechą „Nadgodziny NSW" (nadgodziny
niedzielno-świąteczne, poza zakresem tego pliku).

**Przyczyna:** kod rozróżniał tylko jeden szczególny typ dnia — `TypDnia.Wolny` (dzień wolny w
grafiku, o nazwie zawierającej „Dzień wolny") — dla którego liczył nadgodziny proporcjonalnie do
realnie przepracowanego czasu w danej strefie. Święto to jednak inny typ dnia —
`TypDnia.Świąteczny` — więc praca w święto nie łapała się w ten warunek i trafiała do gałęzi
`else` napisanej z myślą o zwykłym dniu roboczym pełnoetatowca. Ponieważ w święto planowany czas
pracy (`plan`) wynosi zwykle 0, ta gałąź zwracała **sztywne 8 godzin** (norma − plan) za samą
obecność jakiejkolwiek pracy „poza normą", niezależnie od tego, ile godzin faktycznie przepracowano
— a przy kilku strefach tego typu w tym samym święcie wynik zwielokrotniał się przy sumowaniu na
liście płac.

**Poprawka:** dodano wczesny warunek — jeśli `dzienPracy.Definicja.Typ == TypDnia.Świąteczny`,
cecha od razu zwraca `0`, zanim wykona się jakakolwiek dalsza logika (gałąź dla dnia wolnego czy
dla zwykłego dnia roboczego). Nadgodziny za pracę „poza normą" w święto mają być liczone przez
oddzielną cechę „Nadgodziny NSW".

## 3. Drugi zgłoszony błąd i poprawka (27.08.2026) — dzień wolny w grafiku

**Obserwacja klienta:** po poprawce ze święta, praca „poza normą" wpisana w zwykłą sobotę (dzień
wolny w grafiku, 4h) też naliczała błędne, sztywne **8h** zamiast realnych 4h.

**Diagnoza (log z produkcji):** dla tej soboty `dzienPracy.Definicja.Typ = Wolny`, ale
`dzienPracy.Definicja.Nazwa = "Wolny"` — **nie** „Dzień wolny". Warunek pierwszej gałęzi
(`dzienPracy.Definicja.Nazwa.Contains("Dzień wolny")`) był więc zawsze fałszywy w tym środowisku,
mimo że dzień faktycznie jest dniem wolnym (`TypDnia.Wolny`, `NadgodzinySW = False`) — praca
trafiała do gałęzi `else` (pełny etat) i dostawała to samo płaskie `norma - plan = 8h`, co opisany
wyżej błąd świąteczny.

**Poprawka:** usunięto dopasowanie po nazwie (`Nazwa.Contains("Dzień wolny")`) z warunku pierwszej
gałęzi — zostaje tylko `dzienPracy.Definicja.Typ == TypDnia.Wolny && !dzienPracy.Definicja.NadgodzinySW`,
analogicznie do sposobu, w jaki rozpoznawana jest gałąź świąteczna. Dzięki temu praca „poza normą"
w dowolnym dniu typu `Wolny` (niezależnie od jego nazwy w konkretnej konfiguracji) liczy się
proporcjonalnie do realnie przepracowanego czasu (z limitem 8h/dobę), a nie płaskim `norma - plan`.

Zweryfikowano logiem produkcyjnym: dla soboty 17.10.2026 (4h pracy poza normą, `Wolny`/`Wolny`,
`NadgodzinySW=False`) nowy warunek poprawnie zwraca `4h`; dla niedzieli 25.10.2026
(`Świąteczny`/„Niedziela", `NadgodzinySW=True`) nadal poprawnie zwraca `0h`.

## 4. Do potwierdzenia przed wdrożeniem

- Kod cechy „Nadgodziny NSW", która ma faktycznie rozliczać pracę „poza normą" w święto/dzień
  wolny objęty NSW, nie jest jeszcze dodany do repozytorium — do uzupełnienia, gdy będzie dostępny.
- Reszta pliku (gałąź dla zwykłego dnia roboczego, obsługa „czarnych dziur" wokół doby
  niedzielno-świątecznej, martwe zmienne `licznik`/`roznica`, duży blok zakomentowanego starego
  kodu) pozostała bez zmian — nie była przedmiotem tych zgłoszeń.
