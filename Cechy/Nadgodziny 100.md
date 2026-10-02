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
