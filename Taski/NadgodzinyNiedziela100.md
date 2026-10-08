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
## Poprawki i zabezpieczenia — 2026-10-08 (wersja obowiązująca)

**Błąd „Próba zmiany wartości pola 'Praca.Od godziny' - pole w trybie tylko do
odczytu”** (NG-04, 18.10.2026): strefa „Nadgodziny 100%” nie wchodzi do czasu
pracy, a enova pozwala wpisać godzinę od tylko strefom z `Wchodzi`
(`StrefaPracy.EnabledOdGodziny`). Task zapisuje teraz **jedną strefę z samym
czasem** (bez godzin od) — dla silnika wystarcza, bo czyta tylko czas strefy.
Punkty 3 i „ostatnie godziny pracy” z opisu wyżej są nieaktualne.

**Pomijanie dni (zabezpieczenie 1):** strefa „Nadgodziny 100%” wyłącza w
silniku całe liczenie dobowych dnia — także przeniesienia (magazyn),
niewliczane do nadgodzin i bez dopłaty. Żeby nie rozliczyć godzin podwójnie,
Task **nie rusza** dnia, w którym jest strefa:
- z rozliczeniem „W kolejnych miesiącach”, „Z poprzednich miesięcy”,
  „Wypłata nadgodzin” (przeniesienia/odbiór),
- z podstawą nadgodzin „Nie wliczaj” / „Nie naliczaj”,
- inna systemowa strefa nadgodzin (Nadgodziny 50%, 100% okresowe, ŚW) —
  znak ręcznego rozliczenia.
Taki dzień rozlicza się ręcznie. Nieobecność częściowa w niedzielę nie jest
uwzględniana (silnik ją dolicza do czasu) — przypadek marginalny.

**Kontrola (zabezpieczenie 2):** weryfikator kalendarza „A1_Nadgodziny 100% w
niedziele” (Dzień pracy), kod w `Weryfikatory czasu pracy/Nadgodziny 100 w
niedziele - kontrola`. Ostrzega, gdy w obsługiwanej niedzieli strefa
„Nadgodziny 100%” ≠ praca ponad normę (dni sprzed Taska, zapis bez operatora,
zmiana normy/etatu po zapisie dnia). **Podpinać jako Ostrzeżenie, nie Błąd** —
`Saver.ClientSave` uruchamia weryfikatory PRZED Taskami (i ponownie po nich),
więc Błąd zablokowałby zapis, zanim Task dopisze strefę.

**Baza Claude:** TaskDefs 285 zaktualizowany; DefWeryfKalend **1040**
(kod w definicji, nie w CodeFiles — w tej bazie nie ma
`A1WeryfikatoryKalendarza`), podpięty jako Ostrzeżenie do kalendarza
„Test nadgodziny nowe” (28).

## Wnioski o nadgodziny przy wersjonowaniu — 2026-10-08

Najważniejszy przypadek klienta: pracownik składa wniosek o nadgodziny na
niedzielę, a czas pracy przekracza normę (8h). Klient ma włączone
wersjonowanie kalendarzy.

**Mechanizm enova (dekompilacja 2512.5.6):**
- `DzienPracy.CalcReadOnly()` — przy `WersjonowanieCzas` kalendarza dzień
  pracy jest tylko do odczytu, chyba że `DniPracy.ForceWriteMode` (pole
  `internal`).
- Realizacja wniosku (`RozliczenieCzasuPracy.WykonajInt`) włącza
  `ForceWriteMode`, dopisuje strefę ze „zlecenia” (np. „Praca poza normą”),
  wyłącza `ForceWriteMode` i woła `RecalculateRO()` (`internal`) na
  zmienionych dniach. Tak samo zatwierdzenie DAK.
- Task uruchamia się dopiero potem (`Saver.doEventsAndTasks`), na dniu już
  zablokowanym — bez odblokowania dopisanie strefy kończy się błędem „tylko
  do odczytu” i wycofaniem całej realizacji wniosku / zatwierdzenia DAK.

**Rozwiązanie (zaakceptowane przez użytkownika):** Task, gdy dzień jest
tylko do odczytu, robi to samo co enova: na czas zmiany włącza
`ForceWriteMode` i przelicza stan dnia (`RecalculateRO`), potem przywraca.
Oba elementy są niepubliczne → dostęp przez refleksję.
- Dzień po odblokowaniu dalej tylko do odczytu (blokada okresu) → pomijany.
- Brak pola/metody po aktualizacji enova → dzień pomijany bez błędu
  (kontrola „A1_Nadgodziny 100% w niedziele” pokaże ostrzeżenie).
- Wniosek z odbiorem czasem wolnym („Nadgodziny do przeniesienia”) → dzień
  pomijany jak dotąd (godziny idą do magazynu).

**Ryzyka:**
- Zależność od wewnętrznych elementów enova — **sprawdzać po każdej
  aktualizacji** (test: wniosek na niedzielę > 8h).
- Dopisana strefa nie trafia do historii wersji dnia (`DzienPracyHistoria`).

**Status:** kompiluje się lokalnie na DLL 2512.5.6; refleksja sprawdzona na
prawdziwej bibliotece (`Soneta.Kalend.DniPracy.ForceWriteMode` bool
internal, `Soneta.Kalend.DzienPracy.RecalculateRO()` internal). Wgrane do
bazy Claude (TaskDefs 285, stamp podbity). Niesprawdzone na żywo.

## Kody kosztowe (cechy Projekt i Task) — 2026-10-08

Pracownik we wniosku o nadgodziny wypełnia cechy **Projekt** i **Task**
(kody kosztowe), które trafiają na strefę „Praca poza normą”. Strefa
„Nadgodziny 100%” dopisywana przez Task dostaje te same kody:

1. Nadwyżka ponad normę jest brana z **ostatnich godzin pracy** — strefy
   pracy (Wchodzi + Zwiększa) od tej, która kończy się najpóźniej; strefy bez
   godziny od na końcu kolejki.
2. Każda część nadwyżki dostaje cechy Projekt i Task strefy, z której
   pochodzi.
3. Części z tymi samymi kodami są łączone w jedną strefę „Nadgodziny 100%”;
   różne kody → kilka stref (suma = nadwyżka, silnik sumuje czas stref).
4. Idempotencja porównuje czas **i** kody każdej strefy — zmiana kodów na
   strefie pracy przelicza strefy „Nadgodziny 100%”.

Przykład 13.09 (Praca poza normą 7–20 i 21–23, kody P1/T1, norma 8h):
jedna strefa „Nadgodziny 100%” 7:00 z Projekt=P1, Task=T1. Gdyby 21–23 miało
kody P2/T2: „Nadgodziny 100%” 2:00 (P2/T2) + 5:00 (P1/T1).

Nazwy cech są stałymi na początku kodu (`NazwaCechyProjekt`,
`NazwaCechyTask`); brak definicji cechy w bazie = cecha pomijana bez błędu.
W bazie Claude tych cech nie ma — **niesprawdzone na danych**. Cechy muszą
być zwykłymi (nie algorytmicznymi) cechami tabeli StrefyPracy, inaczej zapis
wartości się nie uda.
