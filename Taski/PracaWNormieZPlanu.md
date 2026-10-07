# Task „Praca w normie z planu”

## Co robi

Przy każdym zapisie dnia pracy pracownika (ręcznie albo importem czasu pracy)
dopisuje strefy **„Praca w normie”** na godziny z **planu pracy**, których nie
zajmują już strefy wliczane do czasu pracy.

Dla każdego zmienionego w danym zapisie dnia pracy:

1. Dzień ma już strefę „Praca w normie” → pomijany (dzięki temu task można
   wywoływać wielokrotnie i nie wpada w pętlę — sam dopisuje strefy, co
   ponownie go wyzwala).
2. Plan pracy dnia (`Pracownik.Czasy.KalkPlanu[data]` — kalendarz wzorcowy +
   wyjątki z kalendarza indywidualnego) nie ma godzin → pomijany.
3. Godziny pracy z planu (`Dzien.Praca`) **minus** strefy dnia z zaznaczonym
   `Wchodzi` (dowolnego typu — także przerwy) i godziną od → to, co zostanie,
   dostaje strefy „Praca w normie”. Warunek jest identyczny z weryfikatorem
   enova `DefinicjaDnia.StrefyVerifier` („Strefy wliczane do czasu faktycznie
   przepracowanego nie mogą na siebie zachodzić”).

Przykłady — plan 8–12 i 13–17:
- „Praca poza normą” 15:00 4:00 → „Praca w normie” 8–12 i 13–15,
- „Przerwa bezpłatna” 11:00 2:00 (Wchodzi, typ „Nie wpływa”) → 8–11 i 13–17.

Strefy nie wchodzące do czasu pracy (np. „Nadgodziny 50%”, „Dyżur domowy”)
niczego nie zabierają.

**Poprawka 2026-10-07:** pierwsza wersja odejmowała tylko strefy
`Wchodzi` + `Typ = Zwiększa`. Przy „Przerwie bezpłatnej” 11–13 (typ „Nie
wpływa”) norma nachodziła na przerwę i zapis dnia kończył się błędem
weryfikatora nachodzenia stref.

Strefy bez godziny od nie są odejmowane (nie da się ich umieścić na osi
czasu). Gdy w bazie nie ma definicji strefy „Praca w normie”, task nic nie
robi (nie blokuje zapisu).

## Pliki

- `PracaWNormieZPlanu` — sekcje kodu definicji zadania (IsEnable, IsActive,
  IsRealised, Action), w formacie znaczników `//** begin ... **` jak w enova.
- `PracaWNormieZPlanu - wyzwalacz DniPracy` — kod wyzwalacza na tabeli DniPracy.
- `PracaWNormieZPlanu - wyzwalacz StrefyPracy` — kod wyzwalacza na tabeli StrefyPracy.

## Konfiguracja w enova

Nowa definicja zadania:

| Pole | Wartość |
|---|---|
| Nazwa | Praca w normie z planu |
| Tabela | Pracownicy |
| Algorytm (kod) | tak |
| Typ definicji | **Brak** |
| Blokada | nie |

Sekcje kodu wkleić z pliku `PracaWNormieZPlanu` (każdą w odpowiednie miejsce
edytora). Wyzwalacze (oba z przeznaczeniem „Uniwersalny”):

| Tabela | Kod |
|---|---|
| DniPracy | `PracaWNormieZPlanu - wyzwalacz DniPracy` |
| StrefyPracy | `PracaWNormieZPlanu - wyzwalacz StrefyPracy` |

Wyzwalacz na **StrefyPracy jest potrzebny**: import dla istniejącego dnia
kasuje i dodaje strefy, a sam wiersz dnia może się wtedy nie zmienić.

## Jak to działa w enova (zdekompilowane, 2512.5.6)

- `Soneta.Business.Internal.Saver.doEventsAndTasks` przy zapisie sesji
  przegląda zmienione wiersze tabel z wyzwalaczami, woła `GetGuidedRows()`
  i dla zwróconego pracownika przelicza task. Dzieje się to **tylko w sesji
  z zalogowanym operatorem** (`!IsInternal && Login.Operator != null`) —
  GUI i import z menu tak, wewnętrzne sesje systemowe nie.
- Typ definicji „Brak” → `TaskCalculatorNone.Analyse()`: najpierw
  `IsRealised()`, potem `IsEnable()`; dopiero `IsEnable() == true` tworzy
  rekord zadania. Dlatego cała praca jest w `IsEnable()`, które zwraca
  `false` — **żadne zadania nie powstają**, zostają tylko strefy.
- Kalkulator dostaje tylko pracownika; dni do obróbki bierze z wierszy
  zmienionych w tym zapisie (`DniPracy.Rows.Changed`, `StrefyPracy.Rows.Changed`).
- Zmiany zrobione przez task są w tej samej transakcji zapisu; jeśli
  zakończą się błędem (np. weryfikator), cały zapis się nie uda.

## Status

- Kod skompilowany lokalnie na DLL-ach serwera 2512.5.6 (klasy bazowe
  odtworzone jak w generatorze enova) — **bez błędów**.
- Odejmowanie przedziałów (`FromTimes.Sub`) sprawdzone na prawdziwej klasie
  enova dla scenariuszy z pliku testowego generatora czasu pracy.
- **Baza Claude (2026-10-07):** definicja założona SQL-em jako kopia
  istniejącej definicji typu „Brak” na Pracownikach — `TaskDefs` ID 284,
  wyzwalacze `TaskTriggers` ID 179 (DniPracy) i 180 (StrefyPracy). Po wstawieniu
  trzeba było podbić `RuntimeProjects.Stamp` projektu Business (ID 2), inaczej
  `dbmgr compile` bierze stary cache. Skompilowana biblioteka zawiera klasy
  `Task_Praca_w_normie_z_planu` i oba wyzwalacze — kompilacja bez błędów.
- **Potwierdzone na żywo (2026-10-07):** import pliku testowego PP-01 z menu
  uruchomił wyzwalacze i Task dopisał „Praca w normie”. Test: plik
  `ImportyXML/Generator czasu pracy/Czas pracy - test PP-01 wrzesien 2026.xml`
  (tabela oczekiwanych wyników w `Generator czasu pracy.md`).
