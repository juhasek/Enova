# Nadgodziny okresowe – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy cecha

Cecha wyliczana (typu Kwota/liczba godzin, `decimal`), przypisana do wiersza strefy pracy
(`Row` = `StrefaPracy`). Dla wierszy, których definicja zawiera w nazwie „Praca poza normą" (w tym
warianty typu „Praca poza normą awaria"), wylicza liczbę godzin, które mają zostać rozliczone jako
**nadgodziny okresowe**.

## 2. Zgłoszony błąd i poprawka (27.08.2026)

**Zgłoszenie klienta:** nadgodziny okresowe błędnie się naliczają, gdy „Praca poza normą"/„Praca
poza normą awaria" przypada **w święto**.

**Przyczyna:** kod rozróżniał tylko jeden szczególny typ dnia — `TypDnia.Wolny` (dzień wolny w
grafiku, o nazwie zawierającej „Dzień wolny") — i dla takich dni liczył nadgodziny proporcjonalnie
do realnie przepracowanego czasu w danej strefie (z limitem 8h/dobę). Święto to jednak inny typ
dnia — `TypDnia.Świąteczny` — więc praca w święto nie łapała się w ten warunek i trafiała do gałęzi
`else` napisanej z myślą o zwykłym dniu roboczym pełnoetatowca. Ponieważ w święto planowany czas
pracy (`plan`) wynosi zwykle 0, ta gałąź zwracała **sztywne 8 godzin** (norma − plan) za samą
obecność jakiejkolwiek pracy „poza normą", niezależnie od tego, ile godzin faktycznie przepracowano
— a przy kilku strefach tego typu w tym samym święcie wynik zwielokrotniał się przy sumowaniu na
liście płac.

**Poprawka:** dodano warunek `dzienSwiateczny` (`TypDnia.Świąteczny` i brak flagi
`NadgodzinySW`) obok istniejącego `dzienWolny`, tak by praca „poza normą" w święto liczyła się tą
samą, proporcjonalną metodą co praca w dniu wolnym (suma stref dnia z limitem 8h), zamiast płaskim
`norma - plan` z gałęzi dla zwykłego dnia roboczego.

## 3. Do potwierdzenia przed wdrożeniem

- Czy flaga `NadgodzinySW` ma dla święta to samo znaczenie („dzień, dla którego nadgodziny okresowe
  liczone są już inną ścieżką, więc pomiń") co dla dnia wolnego — przyjęto tak przez analogię, do
  zweryfikowania z operatorem na danych produkcyjnych.
- Reszta pliku (gałąź dla zwykłego dnia roboczego, obsługa „czarnych dziur" wokół doby
  niedzielno-świątecznej, martwe zmienne `licznik`/`roznica`, duży blok zakomentowanego starego kodu)
  pozostała bez zmian — nie była przedmiotem tego zgłoszenia.
