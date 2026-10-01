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
