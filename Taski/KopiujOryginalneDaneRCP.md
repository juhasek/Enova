# Task „Kopiuj oryginalne dane RCP”

## Problem

Integracja czasu pracy wczytuje odbicia z czytników do **Oryginalnych danych
RCP** (tabela `WejsciaWyjsciaO`). Do **Danych RCP** (`WejsciaWyjsciaI`), na
których pracuje algorytm RCP, trzeba je było przenosić ręcznie czynnością
**„Kopiuj oryginalne dane”**.

Standardowy import enova z pliku czytnika (`AutoImportRCP`) zapisuje od razu
do obu tabel — integracja klienta zapisuje tylko do oryginalnych.

## Co robi

Każdy zapis z Oryginalnych danych RCP kopiuje do Danych RCP **dokładnie raz**,
tak samo jak worker enova `WejsciaWyjsciaOWorker.Kopiuj` (zdekompilowany, 2610):

| Dane RCP | Skąd |
|---|---|
| Data | Data edytowana oryginału, a gdy pusta — Data |
| Godzina | Godzina edytowana oryginału, a gdy pusta — Godzina |
| Typ, Operacja, Czytnik RCP, Uwagi | z oryginału |
| Powiązanie z oryginałem (`WeWyO`) | oryginał |
| Zaimportowany | nie |
| Stan | Nieoznaczony |

Zapis **nie jest kopiowany**, gdy w Danych RCP już jest:

- zapis powiązany z tym oryginałem (wcześniej skopiowany workerem albo tym taskiem), albo
- zapis bez powiązania z tą samą datą, godziną i typem (np. ze standardowego
  importu z czytnika, który pisze do obu tabel bez powiązania).

Dzięki temu można bezpiecznie mieszać task z ręcznym kopiowaniem i nie powstają duplikaty.
Późniejsza zmiana oryginału ani usunięcie kopii z Danych RCP **nie są
synchronizowane** — każdy oryginał jest kopiowany raz.

## Dwie drogi uruchomienia

1. **Przy zapisie** (wyzwalacz na `WejsciaWyjsciaO`) — kopia powstaje w tej
   samej transakcji co oryginał. enova uruchamia wyzwalacze **tylko w sesji
   z operatorem** (`Saver.doEventsAndTasks`: `!IsInternal && Login.Operator != null`).
   Błąd kopiowania jest przechwytywany, żeby nie blokować zapisu oryginału.
2. **Harmonogram** — akcja automatyczna **„Kopiuj oryginalne dane RCP”**
   (`[AutoAction]`, tak jak istniejące zadanie „Import RCP”). Przegląda
   oryginalne zapisy z ostatnich **31 dni** (stała `DniWsteczHarmonogramu`)
   i dogania wszystko, czego nie skopiował wyzwalacz — np. gdy integracja
   zapisuje bez operatora albo prosto do bazy. Skopiowana liczba zapisów
   i błędy trafiają do logu harmonogramu (kategoria „Scheduler”).

Nie wiadomo, jak dokładnie zapisuje integracja klienta, dlatego są obie drogi:
wyzwalacz daje natychmiastową kopię, a harmonogram gwarantuje, że niczego nie zabraknie.

## Pliki

- `KopiujOryginalneDaneRCP` — sekcje kodu definicji zadania (IsEnable, IsActive,
  IsRealised, Action, pozostałe metody) w formacie znaczników `//** begin ... **`.
- `KopiujOryginalneDaneRCP - wyzwalacz WejsciaWyjsciaO` — kod wyzwalacza.

## Konfiguracja w enova

Nowa definicja zadania:

| Pole | Wartość |
|---|---|
| Nazwa | Kopiuj oryginalne dane RCP |
| Tabela | Pracownicy |
| Algorytm (kod) | tak |
| Typ definicji | **Brak** |
| Blokada | nie |

Sekcje kodu wkleić z pliku `KopiujOryginalneDaneRCP` (każdą w odpowiednie
miejsce edytora; metody `KopiujOryginalneDaneRCP`, `KopiujZapisRCP`,
`LogHarmonogramu` i stała — w „pozostałe metody”).

Wyzwalacz (przeznaczenie „Uniwersalny”):

| Tabela | Kod |
|---|---|
| WejsciaWyjsciaO | `KopiujOryginalneDaneRCP - wyzwalacz WejsciaWyjsciaO` |

Harmonogram: nowa definicja harmonogramu wskazująca tę definicję zadania i akcję
„Kopiuj oryginalne dane RCP”, częstotliwość np. co 15 minut (albo zaraz po
harmonogramie integracji).

Typ „Brak” + `IsEnable() == false` → enova nie tworzy rekordów zadań (Tasks)
przy zapisach (zob. `PracaWNormieZPlanu.md`).

## Status

- 2026-10-16: kod napisany na podstawie zdekompilowanego workera i importu RCP
  z enova 2610; **kompiluje się** lokalnie na bibliotekach 2512.5.6 razem
  ze skompilowanym projektem Business bazy (klasa bazowa `TaskCalculatorPracownicy`).
- 2026-10-20, baza **Al**: definicja zadania TaskDefs **415** + wyzwalacz TaskTriggers **238**
  (WejsciaWyjsciaO) założone i skompilowane przez serwer 2610 (akcja `[AutoAction]` widoczna
  w definicji harmonogramu). Harmonogram ScheduleDefs **12** „Kopiuj oryginalne dane RCP”:
  co 15 minut od 20.10.2026, akcja „Kopiuj oryginalne dane RCP”.
- Uruchomienie usługi harmonogramu zadań na serwerze 2610 (`appsettings.json`, sekcja
  `Orchestrator:Process:Composition:Scheduler`) wymagało trzech rzeczy:
  1. `"Enabled": true` (domyślnie `false`),
  2. `"Overrides": { "DbName": "Al" }` — bez bazy scheduler pada co ~10 s z błędem
     „Database cannot be determined” (to odpowiednik `scheduler -d Al`),
  3. odblokowanego operatora **„Harmonogram Zadań”** w bazie (Ustawienia/Systemowe/Uprawnienia/
     Operatorzy, status „Wszystkie”) — scheduler loguje się na tego operatora, a zablokowany daje
     „Nieznaleziony kontekst autoryzacji dla operatora”. Po nieudanym logowaniu scheduler nie
     ponawia próby — trzeba zrestartować proces.
- Formularz „Oryginalne dane z RCP” jest w UI niedostępny („Brak praw do formularza danych”),
  więc wyzwalacza nie da się sprawdzić ręcznym dodaniem zapisu — test wymaga zapisu z integracji.
- **Harmonogram w obecnej postaci nie działa:** zaplanowane wywołania (`ScheduleItems`) enova
  przypina zawsze do zadania (`Tasks`). Powstają przy tworzeniu zadania danej definicji albo
  czynnością „Uruchom harmonogramy”, dostępną tylko dla definicji globalnych (tabela `?`)
  — `TaskDefinitionRunScheduleWorker` w Soneta.Core. Definicja 415 (Pracownicy, `IsEnable() == false`)
  zadań nie tworzy, więc harmonogram 12 nigdy nie dostał wywołania (`ScheduleItems` puste).
- Oryginał ID 3 (16.10, sprzed założenia taska) nie ma kopii w Danych RCP — wyzwalacz
  kopiuje tylko nowe zapisy.

## Decyzja 2026-10-20

Dane do Oryginalnych danych RCP przychodzą integracją, więc **na razie zostaje jeden task**
— definicja 415 z wyzwalaczem (kopiowanie przy zapisie). Harmonogram 12 jest nieaktywny
(brak zaplanowanych wywołań); metoda `[AutoAction]` w kodzie 415 jest nieużywana.

Docelowo (gdy okaże się potrzebny harmonogram, np. integracja zapisuje bez operatora):
**dwa zadania** —
1. obecna definicja 415 na Pracownikach (wyzwalacz przy zapisie),
2. osobna definicja zadania do harmonogramu **na tabeli `CfgNodes`** (wzór: TaskDefs 61
   „Import RCP”) z akcją `[AutoAction]` kopiującą zaległe oryginały z ostatnich 31 dni;
   harmonogram przypięty do niej, a akcja harmonogramu usunięta z 415.

Do zrobienia: test wyzwalacza pierwszym zapisem z integracji (czy kopia powstaje od razu,
z powiązaniem WeWyO).
