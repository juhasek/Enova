# Nadgodziny okresowe nowe – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Kontekst i cel

Trzecia cecha z „nowego” zestawu — kontekst biznesowy (dlaczego w ogóle istnieją te cechy,
zamiast wbudowanego silnika enova) opisuje `Cechy/Nadgodziny 50 nowe.md` pkt 1. Ten dokument
opisuje tylko to, co specyficzne dla „okresowych”.

Cecha wyliczana (typu Kwota/liczba godzin, `decimal`), przypisana do wiersza strefy pracy
(`Row` = `StrefaPracy`), dla stref „Praca poza normą” / „Praca poza normą awaria”. Liczy
godziny, które nie są jeszcze nadgodzinami dobowymi (bo dzień był zaplanowany krócej niż
pełna norma dobowa), tylko „dopracowaniem” do normy — rozliczanym w ramach okresu
rozliczeniowego, nie jako dobowe 50%/100%.

Odpowiednik istniejącej cechy `Cechy/Nadgodziny okresowe` — ta cecha **nie jest zgłoszona
jako błędna** (ostatnia poprawka z 14.09.2026 potwierdzona przez klienta na żywej bazie) i
pozostaje aktywna równolegle. „Nadgodziny okresowe nowe” to niezależna, równoległa
implementacja tej samej logiki biznesowej, zbudowana od zera zgodnie z tą samą zasadą co
`Nadgodziny 50/100 nowe` — dopisek „nowe” ma umożliwić porównanie wyników przed ewentualnym
przełączeniem elementów płacowych.

## 2. Algorytm

1. **Dzień świąteczny/NSW** (`NadgodzinySW` lub `Typ == Świąteczny`) → `0`. Te godziny
   rozlicza `Nadgodziny NSW nowe`.
2. **„Czarna dziura” przed startem zaplanowanej zmiany** — jeśli `Row` zaczyna się przed
   godziną startu planu dnia (`DzienPlanu.OdGodziny`) i dzień ma niezerowy plan, część
   strefy przypadająca przed tym startem to zawsze okresowe — **niezależnie** od tego, czy
   plan dnia jest krótszy, czy dłuższy od normy (bo w kolejności zegarowej wypada przed
   startem planu, więc nie złapie się w chronologiczne okno z pkt 4). Ta reguła jest
   przeniesieniem konkretnego, potwierdzonego przez klienta przypadku ze starej cechy
   (`Cechy/Nadgodziny okresowe.md` pkt 5, zgłoszenie 14.09.2026: strefa 6:00–7:00 przed
   zmianą zaplanowaną od 19:00 → 1h okresowych) — zaimplementowana tu od nowa, nie
   skopiowana z istniejącego kodu.
3. **Norma dobowa** — identycznie jak w `Nadgodziny 50/100 nowe` (wg
   `Kalendarz.Nadgodziny.AlgorytmDobowa`, nie sztywne 8h).
4. **Okno `(plan, norma]`** — istnieje tylko, gdy `plan < norma` (dzień zaplanowany krócej
   niż pełna norma). Chronologiczna suma stref „Praca w normie” + „Praca poza normą” (obu
   wariantów) z tą samą techniką `dolna`/`gorna` na sumie narastającej, co w
   `Nadgodziny 50 nowe` — więc przy kilku strefach „poza normą” w jednym dniu godziny nie są
   liczone podwójnie.

## 3. Różnica względem starej cechy „Nadgodziny okresowe”

Stara cecha miała **dwa osobne, ręcznie pisane fragmenty kodu** — jeden dla dni typu
„Wolny” (z hardkodowaną normą `8:00`), drugi (z klamrowym `dolna`/`gorna`) dla zwykłych dni
roboczych, z osobnym sprawdzeniem `Zaszeregowanie.Wymiar == Fraction.One`. Nowa cecha ma
**jedną wspólną ścieżkę** dla każdego typu dnia — bo norma dobowa jest już czytana z
realnej konfiguracji kalendarza (patrz `Nadgodziny 50 nowe.md` pkt 3.2), więc nie trzeba
rozróżniać w kodzie dnia wolnego od roboczego ani sprawdzać wymiaru etatu osobno.

## 4. Świadomie poza zakresem

Stara cecha zawiera (nieaktywny w praktyce, bo kod zawsze wykonuje wcześniej `return`) blok
obsługi „czarnej dziury” między dobą niedzielno-świąteczną a poniedziałkiem — ten fragment
**nie został przeniesiony** do nowej cechy, bo w starej wersji jest martwym kodem (nigdy nie
wykonywanym), więc nie ma potwierdzonego zachowania do odtworzenia. Jeśli klient zgłosi taki
przypadek na żywo, wymaga to osobnej analizy i osobnego zlecenia.

## 5. Status: NIEZWERYFIKOWANE

Patrz `Cechy/Nadgodziny 50 nowe.md` pkt 6 — te same ograniczenia środowiska (brak
buscall/GUI, brak kompilacji na żywo) dotyczą tej cechy. Dodatkowo: reguła z pkt 2 (czarna
dziura przed startem zmiany) była w starej cesze potwierdzona tylko dla **jednego**
konkretnego scenariusza klienta (pełny etat, zwykły dzień roboczy) — przypadek etatu
niepełnego i częściowego nachodzenia strefy na start zmiany nie był testowany ani w starej,
ani w tej cesze.
