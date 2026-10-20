# Task „Kopiuj oryginalne dane RCP harmonogram”

Drugie z dwóch zadań kopiowania Oryginalnych danych RCP do Danych RCP — opis wspólny,
metoda kopiująca i dokumentacja: `KopiujOryginalneDaneRCP.md`.

Akcja harmonogramu `[AutoAction("Kopiuj oryginalne dane RCP")]` przegląda Oryginalne
dane RCP z ostatnich `DniWsteczHarmonogramu` (31) dni we własnej sesji
(`Session.Login.CreateSession()` + `Save`) i kopiuje zapisy, których nie skopiowało
zadanie przy zapisie (integracja bez operatora, błąd kopiowania, zapisy sprzed
uruchomienia zadań). Liczba skopiowanych zapisów i błędy trafiają do logu usługi
harmonogramu (kategoria „Scheduler”).

## Konfiguracja w enova

Definicja zadania (Ustawienia/Systemowe/Definicje zadań), wzór: standardowe „Import RCP”:

| Pole | Wartość |
|---|---|
| Nazwa | Kopiuj oryginalne dane RCP harmonogram |
| Tabela | CfgNodes |
| Algorytm (kod) | tak, kod z pliku `KopiujOryginalneDaneRCPHarmonogram` |
| Typ definicji | Brak |
| Wyzwalacze | brak |

Harmonogram — **na zadaniu globalnym** (Ustawienia/Workflow/Harmonogram zadań/Zadania globalne →
Dodaj → definicja „Kopiuj oryginalne dane RCP harmonogram”, źródło zadania `root` z CfgNodes),
zakładka „Automatyczne akcje”: harmonogram „Kopiuj oryginalne dane RCP co 15 min”, akcja
„Kopiuj oryginalne dane RCP”, cykl co 15 minut. Po zapisie zadania otworzyć je ponownie
(`open_form`, nie podformularz) → czynność **„Aktywuj wszystkie”** → zapis → powstaje planowane
wywołanie (`ScheduleItems`, host = zadanie).

Dlaczego tak (Soneta.Business/Soneta.Core 2610):

- harmonogram z listy Definicje harmonogramów zadań nie ma hosta — nigdy nie dostaje
  `ScheduleItems` (powstają przy zadaniu albo przez „Aktywuj wszystkie” na hoście,
  `IScheduleAutoJobRunScheduleWorker`),
- host = **definicja** zadania: `TaskDefinition.IScheduleAutoJob.AutoAction` przy każdym
  wywołaniu tylko tworzy nowe zadanie (`GetParent` → null dla CfgNodes, nic nie powstaje)
  i **nie wywołuje** metody `[AutoAction]` — sprawdzone 20.10.2026 (wywołanie „pomyślne”, brak kopii),
- host = **zadanie**: `Task.IScheduleAutoJob.AutoAction` wywołuje `AutoActionName` na kalkulatorze
  zadania — tak działa standardowe „Import RCP”.

W Al: TaskDefs 416, Tasks 2 (parent CfgNodes 1 „root”), ScheduleDefs 14.

Usługa harmonogramu na serwerze 2610 (`appsettings.json`,
`Orchestrator:Process:Composition:Scheduler`): `"Enabled": true`,
`"Overrides": { "DbName": "Al" }` oraz odblokowany operator „Harmonogram Zadań”.
