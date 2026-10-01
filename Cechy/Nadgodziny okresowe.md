# Nadgodziny okresowe – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Kontekst i cel

Trzecia cecha z „nowego” zestawu — kontekst biznesowy (dlaczego w ogóle istnieją te cechy,
zamiast wbudowanego silnika enova) opisuje `Cechy/Nadgodziny 50.md` pkt 1. Ten dokument
opisuje tylko to, co specyficzne dla „okresowych”.

Cecha wyliczana (typu Kwota/liczba godzin, `decimal`), przypisana do wiersza strefy pracy
(`Row` = `StrefaPracy`), dla stref „Praca poza normą” / „Praca poza normą awaria”. Liczy
godziny, które nie są jeszcze nadgodzinami dobowymi (bo dzień był zaplanowany krócej niż
pełna norma dobowa), tylko „dopracowaniem” do normy — rozliczanym w ramach okresu
rozliczeniowego, nie jako dobowe 50%/100%.

Odpowiednik starej cechy `Cechy/Nadgodziny okresowe_old` — ta cecha **nie jest zgłoszona
jako błędna** (ostatnia poprawka z 14.09.2026 potwierdzona przez klienta na żywej bazie) i
pozostaje aktywna równolegle. „Nadgodziny okresowe” to niezależna, równoległa
implementacja tej samej logiki biznesowej, zbudowana od zera zgodnie z tą samą zasadą co
`Nadgodziny 50/100`; po porównaniu wyników przejęła nazwę „Nadgodziny okresowe”, a stara
wersja pracuje dalej jako `Nadgodziny okresowe_old`.

## 2. Algorytm

1. **Dzień świąteczny/NSW** (`NadgodzinySW` lub `Typ == Świąteczny`) → `0`. Te godziny
   rozlicza `Nadgodziny NSW`.
2. **„Czarna dziura” przed startem zaplanowanej zmiany** — jeśli `Row` zaczyna się przed
   godziną startu planu dnia (`DzienPlanu.OdGodziny`) i dzień ma niezerowy plan, część
   strefy przypadająca przed tym startem to zawsze okresowe — **niezależnie** od tego, czy
   plan dnia jest krótszy, czy dłuższy od normy (bo w kolejności zegarowej wypada przed
   startem planu, więc nie złapie się w chronologiczne okno z pkt 4). Ta reguła jest
   przeniesieniem konkretnego, potwierdzonego przez klienta przypadku ze starej cechy
   (`Cechy/Nadgodziny okresowe.md` pkt 5, zgłoszenie 14.09.2026: strefa 6:00–7:00 przed
   zmianą zaplanowaną od 19:00 → 1h okresowych) — zaimplementowana tu od nowa, nie
   skopiowana z istniejącego kodu.
3. **Norma dobowa** — identycznie jak w `Nadgodziny 50/100` (wg
   `Kalendarz.Nadgodziny.AlgorytmDobowa`, nie sztywne 8h).
4. **Okno `(plan, norma]`** — istnieje tylko, gdy `plan < norma` (dzień zaplanowany krócej
   niż pełna norma). Chronologiczna suma stref „Praca w normie” + „Praca poza normą” (obu
   wariantów) z tą samą techniką `dolna`/`gorna` na sumie narastającej, co w
   `Nadgodziny 50` — więc przy kilku strefach „poza normą” w jednym dniu godziny nie są
   liczone podwójnie.

## 3. Różnica względem starej cechy „Nadgodziny okresowe”

Stara cecha miała **dwa osobne, ręcznie pisane fragmenty kodu** — jeden dla dni typu
„Wolny” (z hardkodowaną normą `8:00`), drugi (z klamrowym `dolna`/`gorna`) dla zwykłych dni
roboczych, z osobnym sprawdzeniem `Zaszeregowanie.Wymiar == Fraction.One`. Nowa cecha ma
**jedną wspólną ścieżkę** dla każdego typu dnia — bo norma dobowa jest już czytana z
realnej konfiguracji kalendarza (patrz `Nadgodziny 50.md` pkt 3.2), więc nie trzeba
rozróżniać w kodzie dnia wolnego od roboczego ani sprawdzać wymiaru etatu osobno.

## 4. Świadomie poza zakresem

Stara cecha zawiera (nieaktywny w praktyce, bo kod zawsze wykonuje wcześniej `return`) blok
obsługi „czarnej dziury” między dobą niedzielno-świąteczną a poniedziałkiem — ten fragment
**nie został przeniesiony** do nowej cechy, bo w starej wersji jest martwym kodem (nigdy nie
wykonywanym), więc nie ma potwierdzonego zachowania do odtworzenia. Jeśli klient zgłosi taki
przypadek na żywo, wymaga to osobnej analizy i osobnego zlecenia.

## 5. Status: NIEZWERYFIKOWANE

Patrz `Cechy/Nadgodziny 50.md` pkt 6 — te same ograniczenia środowiska (brak
buscall/GUI, brak kompilacji na żywo) dotyczą tej cechy. Dodatkowo: reguła z pkt 2 (czarna
dziura przed startem zmiany) była w starej cesze potwierdzona tylko dla **jednego**
konkretnego scenariusza klienta (pełny etat, zwykły dzień roboczy) — przypadek etatu
niepełnego i częściowego nachodzenia strefy na start zmiany nie był testowany ani w starej,
ani w tej cesze.

## Strefy rozliczeniowe dnia a norma dobowa (poprawka 01.10.2026)

W jednym dniu mogą wystąpić **jednocześnie odbiór nadgodzin i wypracowane nadgodziny** —
np. pracownik odbiera 2:00 z magazynu nadgodzin (strefa „Rozliczenie nadgodzin (prac)”),
pracuje 6:00 w normie i 3:00 poza normą (przypadek wprowadzony przez klienta na
pracowniku `NG-08`, 14.10.2026). Strefa odbioru **nie wchodzi** do czasu pracy
(`DefinicjaStrefy.Wchodzi = false`), więc nie było jej w chronologicznej podstawie cechy —
i w efekcie odebrane godziny „zjadały” część normy dobowej, która powinna przypaść na
pracę poza normą. Cecha pokazywała 1:00 zamiast 3:00.

Sam system robi to inaczej: w `KalkulatorNadgodzin.NadgodzinyDobowe` (i analogicznie
`WyliczPodstawęZaOkres` na poziomie okresu) **koryguje czas pracy przed porównaniem z
normą**:

```
czas += dzien.ZPrzeniesienia - dzien.DoPrzeniesienia - ...
```

gdzie (`Dzien.PrzeliczPrzeniesienia`):

- `ZPrzeniesienia` = suma stref o `Definicja.Rozliczenie` = **„Z poprzednich miesięcy”**
  lub **„Wypłata nadgodzin”** (odbiór nadgodzin pracownika/firmy, wyjście prywatne,
  wypłata nadgodzin),
- `DoPrzeniesienia` = suma stref o `Definicja.Rozliczenie` = **„W kolejnych miesiącach”**
  (godziny odłożone do magazynu, np. „Nadgodziny do przeniesienia”).

Cecha liczy to równoważnie — przez przesunięcie samej normy dobowej (wynik identyczny,
a model „per strefa” zostaje nietknięty):

```
normaDobowa = normaDobowa - zPrzeniesienia + doPrzeniesienia   (nie mniej niż 0)
```

Czyli: **odebrane godziny wypełniają normę dnia**, a godziny odłożone do magazynu jej nie
wypełniają. Dla `NG-08`/14.10.2026: norma 8:00 − 2:00 = 6:00, praca 9:00 → nadwyżka 3:00
na strefie „Praca poza normą” (przy `Dobowe 100 = false` całość jako 50%, bez godzin
nocnych) — zgodnie z silnikiem.

Ta sama korekta jest w `Nadgodziny 50`, `Nadgodziny 100` i `Nadgodziny okresowe`, więc
granica między dobowymi a okresowymi przesuwa się spójnie i godziny nie dublują się ani
nie znikają. `Nadgodziny NSW` nie była zmieniana (nie korzysta z normy dobowej).

**Świadomie pominięte:** `NiewliczaneDoNadgodzin` / `BezDopłatyDoNadgodzin`
(`DefinicjaStrefy.PodstawaNadgodzin` ≠ „Naliczaj”), które silnik odejmuje w tym samym
wyrażeniu. W tej instalacji wszystkie strefy mają `PodstawaNadgodzin = Naliczaj`, a w
modelu „per strefa” poprawnie byłoby wykluczyć taką strefę z podstawy, nie korygować nią
normy — do zrobienia, gdy klient faktycznie zacznie używać tych ustawień.

### Status poprawki

Kod wgrany do bazy `Claude` (`FeatureDefs` ID 13 / 14 / 15) i zgodny bajt w bajt z plikami
w repo. Sama poprawka **nie była jeszcze przeliczona w GUI** — środowisko robocze tego repo
nie ma dostępu do żywej aplikacji. Do sprawdzenia u pracownika `NG-08`, 14.10.2026
(oczekiwane: `Nadgodziny 50` = 3:00, `Nadgodziny 100` = 0:00, `Nadgodziny okresowe` = 0:00).
Jedyny dzień w bazie ze strefą rozliczeniową to właśnie ten — w pozostałych scenariuszach
(`NG-01`…`NG-07`, `RC-01`…`RC-09`, `LZ-01`…`LZ-04`) korekta wynosi 0, więc ich wyniki
pozostają bez zmian. Niepewny element składni: porównania do `TypRozliczenia.*` wprost w
kodzie cechy (nowość względem dotychczasowych cech tego repo, które sięgały tylko po
`TypDnia`/`AlgorytmNorma`).
