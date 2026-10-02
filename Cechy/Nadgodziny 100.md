# Nadgodziny 100 – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy

Cecha siostrzana `Cechy/Nadgodziny 50` — pełny kontekst, cel biznesowy i szczegóły
algorytmu (norma dobowa, chronologiczne przypisanie nadwyżki do strefy, limit `Nadgodz50`,
godziny nocne) opisuje `Cechy/Nadgodziny 50.md`. Ten dokument opisuje tylko różnicę.

Cecha wyliczana (typu Kwota/liczba godzin, `decimal`), przypisana do wiersza strefy pracy
(`Row` = `StrefaPracy`), dla stref „Praca poza normą” / „Praca poza normą awaria”. Liczy tę
część dobowej nadwyżki nad normą, która ma być rozliczona jako **nadgodziny 100%**, czyli
dopełnienie do „Nadgodziny 50”:

- część nadwyżki **ponad limit** `Kalendarz.Nadgodziny.Nadgodz50` — tylko gdy globalna
  konfiguracja `Config.Nadgodziny.Dobowe100` jest włączona (w bazie Claude: wyłączona, więc
  ten składnik obecnie zawsze wynosi `0`),
- część nadwyżki przypadająca na **godziny nocne** kalendarza (`Kalendarz.Nocne.Od`/`Do`) —
  aktywne w bazie Claude (`Config.Nadgodziny.Nocne100 = true`), okno nocne wyznacza sam
  system (`KalkulatorPracy.NocOkres`).

## 2. Uwaga: to jest realna zmiana w praktyce, nie tylko techniczna symetria

Stara cecha „Nadgodziny 50 test” (i jej domyślny odpowiednik „Nadgodziny 100” w bazie
Claude) **nigdy nie miała napisanego kodu** dla 100% — pole `Nadgodziny 100` w
`FeatureDefs` istniało jako pusty stub (`Algorithm = 0`, brak kodu). Efekt: godziny pracy
poza normą przypadające w nocy były dotąd **w całości liczone jako 50%**, mimo że globalna
konfiguracja bazy (`Nocne 100 = true`) mówi, że godziny nocne nadgodzin mają być rozliczane
jako 100%. „Nadgodziny 100” domyka tę lukę — nie jest to tylko refaktoryzacja, ale
realne poprawienie wyniku dla zmian nocnych z pracą „poza normą”.

## 3. Status: NIEZWERYFIKOWANE

Patrz `Cechy/Nadgodziny 50.md` pkt 6 — te samo ograniczenie (brak żywego testu w
edytorze skryptów, niesprawdzone bezpośrednio użycie `FromTimes`/`FromTime` i
`KalkulatorPracy.NocOkres`) dotyczy tej cechy identycznie, bo współdzieli tę samą logikę.

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

### Dlaczego `DoPrzeniesienia` NIE koryguje normy (02.10.2026)

Pierwsza wersja tej poprawki (01.10.2026) przesuwała normę w obie strony, czyli podnosiła
ją o godziny odłożone do magazynu (`DoPrzeniesienia`). To zostało wycofane.

Równoważność „korekta czasu pracy ⇔ korekta normy” zachodzi tylko wtedy, gdy obie strony
porównania mówią o tym samym zbiorze godzin. Silnik koryguje **czas pracy całego dnia**
(wszystkie strefy wchodzące do czasu pracy), a podstawą tej cechy są **tylko** strefy
„Praca w normie” / „Praca poza normą” (+ „awaria”). Dla `ZPrzeniesienia` to bez znaczenia —
strefy odbioru i wypłaty nadgodzin nie wchodzą do czasu pracy, więc nie ma ich po żadnej
stronie. Dla `DoPrzeniesienia` już nie: w tym wdrożeniu godziny idące do magazynu są
wpisywane w **osobnych strefach** („Godziny do odbioru” / „Godziny do odbioru awaria”),
których w podstawie cechy nie ma — nie ma więc czego kompensować, a podniesiona norma
„przesuwa” realne nadgodziny w kategorię okresowych.

**Konsekwencja do decyzji klienta:** jeżeli kiedyś godziny z samej strefy „Praca poza
normą” zostaną odłożone do magazynu (ta sama godzina w podstawie cechy **i** w
`DoPrzeniesienia`), cecha i tak pokaże je jako 50%/100%, a silnik nie policzy ich jako
nadgodzin dobowych (bo czekają na rozliczenie w kolejnym miesiącu). Poprawnie trzeba by
wtedy wyłączyć te godziny z podstawy cechy, a nie korygować nimi normę — do rozstrzygnięcia,
gdy taki przypadek faktycznie wystąpi.

**Świadomie pominięte:** `NiewliczaneDoNadgodzin` / `BezDopłatyDoNadgodzin`
(`DefinicjaStrefy.PodstawaNadgodzin` ≠ „Naliczaj”), które silnik odejmuje w tym samym
wyrażeniu. W tej instalacji wszystkie strefy mają `PodstawaNadgodzin = Naliczaj`, a w
modelu „per strefa” poprawnie byłoby wykluczyć taką strefę z podstawy, nie korygować nią
normy — do zrobienia, gdy klient faktycznie zacznie używać tych ustawień.

## Dzień pracy bez planu: norma dobowa = 0:00 (poprawka 02.10.2026)

Zgłoszenie klienta (`NG-07`, 10.10.2026): dzień **typu „Pracy”, ale bez żadnego
zaplanowanego czasu pracy** (plan 0:00), strefy 6:30 + 1:30 „Godziny do odbioru”, 8:00
odłożone do magazynu („Nadgodziny do przeniesienia”) i 2:00 „Praca poza normą”. Cecha
pokazywała te 2:00 jako **okresowe**, a stara cecha produkcyjna (`Nadgodziny 50_old`) jako
**2:00 na 50%** — i to drugie klient potwierdził jako poprawne.

Przyczyna nie była w korekcie o strefy rozliczeniowe (ta dla tego dnia niczego nie zmieniała
w jedną ani w drugą stronę), a w **normie dobowej dnia bez planu**:

- silnik (`KalkulatorNadgodzin.WyliczNormęDobową`) dla planu 0:00 podstawia normę z Etatu —
  `AlgorytmNorma.Kalendarz`: „plan, a gdy 0:00 → `Etat.NormaDobowa`”, `Konfiguracja`:
  zawsze `Etat.NormaDobowa`, `RównoważnyCzasPracy`: `max(plan, Etat.NormaDobowa)`;
  `Etat.NormaDobowa` przy pustym polu na etacie (`StdNorma`) oddaje
  `Kalendarz.Nadgodziny.WartośćDobowa`, czyli u tych pracowników 8:00,
- stara cecha produkcyjna dla dnia typu „Pracy” bierze **normę = plan dnia** (dla pełnego
  etatu), czyli 0:00 — i cały czas pracy poza normą jest nadgodziną dobową.

Obowiązuje reguła klienta, więc cechy liczą teraz tak:

```
jeżeli plan dnia = 0:00 i Definicja dnia.Typ = Pracy  ->  norma dobowa = 0:00
w pozostałych przypadkach                             ->  jak dotąd, wg AlgorytmDobowa
```

**Dni wolne (`Typ = Wolny`) ten wyjątek nie obejmuje** — tam norma zostaje z konfiguracji
(zwykle 8:00 z Etatu), bo tak liczy i silnik, i stara cecha produkcyjna (jej osobna gałąź
dla dnia wolnego porównuje sumę stref z progiem 8:00). Dni niedzielno-świąteczne rozlicza
`Nadgodziny NSW` i są odrzucane wcześniej.

Skutek dla „okresowych”: w dniu pracy bez planu okno `(plan, norma]` jest puste
(`plan 0:00 >= norma 0:00`), więc cecha zwraca 0:00 — godziny trafiają w całości do
`Nadgodziny 50` / `Nadgodziny 100`.

### Status poprawek

Kod wgrany do bazy `Claude` (`FeatureDefs` ID 13 / 14 / 15), zgodny z plikami w repo.
Poprawki **nie były przeliczone w GUI przez autora** — środowisko robocze tego repo nie ma
dostępu do żywej aplikacji; `NG-07` testuje klient. Oczekiwane po poprawkach:

| Pracownik / dzień | `Nadgodziny 50` | `Nadgodziny 100` | `Nadgodziny okresowe` |
| --- | --- | --- | --- |
| `NG-07` / 10.10.2026 (strefa „Praca poza normą” 20:00–22:00) | 2:00 | 0:00 | 0:00 |
| `NG-07` / 22.08.2026 (ten sam układ bez stref magazynu) | 2:00 | 0:00 | 0:00 |
| `NG-08` / 14.10.2026 (strefa „Praca poza normą”, odbiór 2:00) | 3:00 | 0:00 | 0:00 |

Pozostałe scenariusze w bazie (`NG-01`…`NG-06`, `RC-01`…`RC-09`, `LZ-01`…`LZ-04`) **nie
zmieniają wyników**: dni z planem w ogóle nie dotyczy nowa reguła, a w dniach bez planu
(`NG-01`…`NG-03`, `LZ-04`/12.08) cały czas pracy mieści się w strefach „Praca w normie” +
„Praca poza normą”, więc obniżenie normy do 0:00 przesuwa równocześnie dolną granicę okna
i wynik na strefie „poza normą” zostaje ten sam (sprawdzone rachunkowo na danych z bazy,
nie przeliczeniem w enovie).
