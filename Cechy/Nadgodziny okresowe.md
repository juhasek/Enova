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
   niż pełna norma, w tym dzień bez planu). Chronologiczna suma **czasu pracy dnia** —
   stref spełniających warunek silnika `Definicja.Wchodzi && Definicja.Typ ==
   TypStrefy.Zwieksza`, czyli także „Godziny do odbioru”, „Praca zdalna”, „Wyjście
   służbowe” (patrz sekcja „Podstawa cechy = czas pracy dnia wg silnika” na końcu
   dokumentu) — z tą samą techniką `dolna`/`gorna` na sumie narastającej, co w
   `Nadgodziny 50`, więc przy kilku strefach „poza normą” w jednym dniu godziny nie są
   liczone podwójnie. Godziny, które wypełniły już okno okresowych w innych strefach (np.
   8:00 „Godziny do odbioru”), przestają więc być okresowe na strefie „poza normą” — ta
   wypada powyżej normy i jest nadgodziną dobową.

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

Cecha liczy to równoważnie — ale **tylko dla `ZPrzeniesienia`** — przez obniżenie samej
normy dobowej (wynik identyczny, a model „per strefa” zostaje nietknięty):

```
normaDobowa = normaDobowa - zPrzeniesienia   (nie mniej niż 0)
```

Czyli **odebrane godziny wypełniają normę dnia**. Dla `NG-08`/14.10.2026: norma dnia −
2:00 odbioru, praca 6:00 w normie + 3:00 poza normą → nadwyżka 3:00 na strefie „Praca poza
normą” (przy `Dobowe 100 = false` całość jako 50%, bez godzin nocnych).

### Dlaczego `DoPrzeniesienia` nie koryguje normy (02.10.2026)

Pierwsza wersja tej poprawki (01.10.2026) przesuwała normę w obie strony, czyli podnosiła
ją o godziny odłożone do magazynu (`DoPrzeniesienia`). To zostało wycofane.

Godziny odłożone do magazynu są **realnie przepracowane** i mają wypełniać normę dnia; to,
że zostały oznaczone do odebrania w kolejnym miesiącu, rozlicza magazyn nadgodzin
(strefa „Nadgodziny do przeniesienia” i jej późniejszy odbiór), a nie ta cecha. Silnik
odejmuje je od czasu pracy dnia, bo liczy jedną zagregowaną nadwyżkę na dobę — przy
rozbiciu per strefa odjęcie ich (czyli podniesienie normy) przeklasyfikowałoby realne
nadgodziny dobowe na okresowe. Zgłoszenie klienta `NG-07`, 10.10.2026: 8:00 w strefach
„Godziny do odbioru” (w całości przeniesione do magazynu) + 2:00 „Praca poza normą” →
**2:00 na 50%**, nie 2:00 okresowych.

Korekta o `ZPrzeniesienia` (odbiór i wypłata nadgodzin) zostaje — tam godziny faktycznie
nie są pracą tego dnia, a mimo to wypełniają jego normę.

**Świadomie pominięte:** `NiewliczaneDoNadgodzin` / `BezDopłatyDoNadgodzin`
(`DefinicjaStrefy.PodstawaNadgodzin` ≠ „Naliczaj”), które silnik odejmuje w tym samym
wyrażeniu. W tej instalacji wszystkie strefy mają `PodstawaNadgodzin = Naliczaj`, a w
modelu „per strefa” poprawnie byłoby wykluczyć taką strefę z podstawy, nie korygować nią
normy — do zrobienia, gdy klient faktycznie zacznie używać tych ustawień.

## Podstawa cechy = czas pracy dnia wg silnika (poprawka 02.10.2026)

Pierwotnie cechy sumowały chronologicznie tylko strefy, których nazwa zawiera „Praca
w normie” lub „Praca poza normą”. To za mało: **normę dobową wypełnia każda strefa, która
zwiększa czas pracy dnia**, a klient ma takich stref więcej (m.in. „Godziny do odbioru”
i „Godziny do odbioru awaria”).

Podstawa jest teraz wyznaczana dokładnie tym warunkiem, którym posługuje się sam silnik
(`KalkulatorPracyBase`, budowa rzeczywistego czasu pracy doby):

```
Definicja.Wchodzi && Definicja.Typ == TypStrefy.Zwieksza
```

W bazie Claude daje to 9 definicji stref: „Praca w normie”, „Praca poza normą”, „Praca poza
normą awaria”, „Godziny do odbioru”, „Godziny do odbioru awaria”, „Praca zdalna”, „Praca
zdalna okazjonalna”, „Wyjście służbowe”, „Przerwa na karmienie”. Poza podstawą zostają:

- strefy informacyjne nakładające się na zmianę („Lider zmiany”, „Praca w godzinach
  nocnych”, „Delegacja służbowa”) — mają `Typ = Nie wpływa`, więc godzin nie dublują,
- przerwy i przestoje oraz „Dyżur domowy” — `Typ = Nie wpływa`,
- strefy rozliczeniowe („Nadgodziny do przeniesienia”, „Rozliczenie nadgodzin”, „Wypłata
  nadgodzin”, „Wyjście prywatne”) — nie wchodzą do czasu pracy; odbiór/wypłatę cecha
  uwzględnia obniżeniem normy (patrz sekcja wyżej).

Dla zgłoszenia `NG-07` / 10.10.2026 (dzień bez planu, norma dobowa 8:00 z Etatu):
6:30 + 1:30 „Godziny do odbioru” wypełnia normę 8:00, więc strefa „Praca poza normą”
20:00–22:00 wypada **powyżej** normy → 2:00 nadgodzin dobowych (50%), a okno okresowych
jest już zajęte → `Nadgodziny okresowe` = 0:00. Same „Godziny do odbioru” nie dostają
żadnej wartości — cecha liczy wyłącznie dla stref „Praca poza normą” — bo są rozliczane
magazynem nadgodzin.

Założenie: strefy typu „Zwiększa” **nie nachodzą na siebie** w czasie (w tej instalacji nie
nachodzą — nakładki są typu „Nie wpływa”). Gdyby zaczęły, podstawa liczyłaby te same
godziny dwa razy; silnik robi w tym miejscu sumę przedziałów (`FromTimes`), nie sumę czasów.

### Status poprawek

Kod wgrany do bazy `Claude` (`FeatureDefs` ID 13 / 14 / 15), zgodny z plikami w repo.
Poprawki **nie były przeliczone w GUI przez autora** — środowisko robocze tego repo nie ma
dostępu do żywej aplikacji; testuje klient. Oczekiwane po poprawkach:

| Pracownik / dzień | `Nadgodziny 50` | `Nadgodziny 100` | `Nadgodziny okresowe` |
| --- | --- | --- | --- |
| `NG-07` / 10.10.2026 (8:00 „do odbioru” + 2:00 poza normą 20:00–22:00) | 2:00 | 0:00 | 0:00 |
| `NG-07` / 22.08.2026 (ten sam układ, bez stref magazynu) | 2:00 | 0:00 | 0:00 |
| `NG-08` / 14.10.2026 (odbiór 2:00 + 6:00 w normie + 3:00 poza normą) | 3:00 | 0:00 | 0:00 |

Pozostałe scenariusze w bazie (`NG-01`…`NG-06`, `RC-01`…`RC-09`, `LZ-01`…`LZ-04`) **nie
zmieniają wyników**: na każdym z ich dni suma stref „Praca w normie” + „Praca poza normą”
jest równa całemu czasowi pracy dnia (sprawdzone SQL-em), więc rozszerzenie podstawy niczego
tam nie dodaje. Jedyne dni, na których podstawa się zmieniła, to właśnie dwa dni `NG-07`.
