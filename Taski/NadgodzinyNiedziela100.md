# Task „Nadgodziny 100% w niedziele”

## Po co

Przy pracy w niedzielę (dzień wolny wg rozkładu) silnik nadgodzin enova
liczy:

- godziny do normy dobowej (8h) → nadgodziny **okresowe** (koniec okresu),
- nadwyżkę → nadgodziny **dobowe**, ale podzielone jak w zwykły dzień:
  50% do limitu, 100% ponad limit lub w nocy („Nocne 100%”).

Wymaganie (zgodne z art. 151¹ §1 pkt 1 KP): nadwyżka ponad normę w niedzielę
ma być w całości **dobowa 100%**. Przykład 13.09.2026, praca 7–20 i 21–23
(15h): 8h okresowe + 7h dobowe 100% (silnik bez taska dawał 6h 50% + 1h 100%).

## Mechanizm enova (zdekompilowane, 2512.5.6)

`KalkulatorNadgodzin.NadgodzinyDobowe`: gdy dzień ma godziny w systemowych
strefach `Nadgodziny 50%` / `Nadgodziny 100%` / `Nadgodziny 100% okresowe` /
`Nadgodziny ŚW`, silnik bierze je wprost i nie dzieli dnia sam. Okresowe
liczone są potem jako czas pracy minus nadgodziny dobowe (`Podstawa`).
Strefa „Nadgodziny 100%” nie zwiększa czasu pracy (Typ „Nie wpływa”,
nie Wchodzi). Dzięki temu statystyka (Kalendarz → Statystyka) i elementy
płacowe pokazują ten sam wynik.

## Co robi

Przy każdym zapisie dnia pracy pracownika (ręcznie albo importem), dla dni,
których definicja wg planu jest typu **Świąteczny bez „Nadgodziny
świąteczne”** (czyli „Niedziela” w obecnej konfiguracji):

1. Czas pracy dnia = suma stref `Wchodzi` + `Typ = Zwiększa` (jak w silniku).
2. Nadwyżka = czas pracy − norma dobowa z etatu (`Etat.NormaDobowa`, puste
   pole na etacie = norma z kalendarza wzorcowego, zwykle 8:00).
3. Strefa „Nadgodziny 100%” na **ostatnie** godziny pracy o długości
   nadwyżki (np. 15:00–20:00 i 21:00–23:00). Gdy strefy pracy nie mają
   godzin od — strefa z samym czasem.
4. Istniejące strefy „Nadgodziny 100%” tego dnia są układane od nowa, gdy
   różnią się od wyliczonych (także dopisane ręcznie); brak nadwyżki → są
   usuwane. Gdy są już poprawne — nic się nie zmienia (ochrona przed pętlą
   wyzwalacza, który task sam uruchamia).

Święta z zaznaczonymi „Nadgodzinami świątecznymi” nie są ruszane — zostają
jako nadgodziny świąteczne.

## Wymagana konfiguracja

- Definicja dnia **„Niedziela”: „Nadgodziny świąteczne” odznaczone.**
- Element „Dopłata do nadgodzin 100% dobowe” płaci `N100Doba` (przeniesienie
  N50 z niedziel w algorytmie elementu jest wtedy zbędne — można je cofnąć).

## Pliki

- `NadgodzinyNiedziela100` — sekcje kodu definicji zadania (IsEnable,
  IsActive, IsRealised, Action).
- `NadgodzinyNiedziela100 - wyzwalacz DniPracy`, `... - wyzwalacz StrefyPracy`
  — kod wyzwalaczy (taki sam jak w `PracaWNormieZPlanu`).

## Konfiguracja w enova

| Pole | Wartość |
|---|---|
| Nazwa | Nadgodziny 100% w niedziele |
| Tabela | Pracownicy |
| Algorytm (kod) | tak |
| Typ definicji | **Brak** |

Wyzwalacze (przeznaczenie „Uniwersalny”): tabela **DniPracy** i **StrefyPracy**.
Szczegóły działania tasków przy zapisie: `PracaWNormieZPlanu.md`.

Dni zapisane przed założeniem taska trzeba „dotknąć” (otworzyć i zapisać),
żeby strefa się dopisała.

## Status (2026-10-08)

- Skompilowany lokalnie na DLL-ach serwera 2512.5.6 — bez błędów.
- Cięcie przedziałów (`FromTimes.Intersection(Time)` + `Sub`) sprawdzone na
  prawdziwej klasie enova: praca 7–20, 21–23, norma 8:00 → 15:00 5:00 i
  21:00 2:00 = 7:00.
- Mechanizm stref ręcznie potwierdzony przez użytkownika (strefa
  „Nadgodziny 100%” 7h na 13.09 → statystyka zgodna). Sam task
  **niesprawdzony na żywo**.
- **Baza Claude (2026-10-08):** definicja założona SQL-em jako kopia
  `TaskDefs` 284 — `TaskDefs` ID **285** „Nadgodziny 100% w niedziele”
  (klasa `Task_Nadgodziny_100_w_niedziele642765867`), wyzwalacze
  `TaskTriggers` **181** (DniPracy) i **182** (StrefyPracy). Po podbiciu
  `RuntimeProjects.Stamp` (ID 2) `dbmgr compile` bez błędów — biblioteka
  `4bb49.dll` zawiera klasę taska i oba wyzwalacze.
- **Poprawka 2026-10-08:** kod w `TaskDefs.Code` musi mieć same końce linii
  CRLF. Przy mieszanych (plik repo ma LF) formularz definicji zadania
  zgłaszał `TaskDefRoslynAlgorithmExtender.EnableCondition GET: length
  (-6838) must be a non-negative value`. Znormalizowane, ponowna kompilacja
  bez błędów.
