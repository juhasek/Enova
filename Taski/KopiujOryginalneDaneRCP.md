# Task „Kopiuj oryginalne dane RCP” (kopiowanie przy zapisie)

## Problem

Integracja czasu pracy wczytuje odbicia z czytników do **Oryginalnych danych
RCP** (tabela `WejsciaWyjsciaO`). Do **Danych RCP** (`WejsciaWyjsciaI`), na
których pracuje algorytm RCP, trzeba je było przenosić ręcznie czynnością
**„Kopiuj oryginalne dane”**.

Standardowy import enova z pliku czytnika (`AutoImportRCP`) zapisuje od razu
do obu tabel — integracja klienta zapisuje tylko do oryginalnych.

## Rozwiązanie — dwa zadania

| Zadanie | Tabela | Uruchomienie | Plik kodu |
|---|---|---|---|
| Kopiuj oryginalne dane RCP | Pracownicy | wyzwalacz na `WejsciaWyjsciaO` — przy zapisie oryginału | `KopiujOryginalneDaneRCP` |
| Kopiuj oryginalne dane RCP harmonogram | CfgNodes | harmonogram co 15 min, ostatnie 31 dni | `KopiujOryginalneDaneRCPHarmonogram` (opis: `KopiujOryginalneDaneRCPHarmonogram.md`) |

Oba zadania kopiują jedną metodą `A1KopiowanieRCP.KopiujZapis` z **dodatkowego kodu do
kompilacji** „A1KopiowanieRCP” (Ustawienia/Systemowe/Dodatkowy kod do kompilacji, projekt
Business — ten sam, w którym kompilują się definicje zadań; plik `A1KopiowanieRCP`), tak jak
worker enova `WejsciaWyjsciaOWorker.Kopiuj` (zdekompilowany, 2610):

| Dane RCP | Skąd |
|---|---|
| Data | Data edytowana oryginału, a gdy pusta — Data |
| Godzina | Godzina edytowana oryginału, a gdy pusta (albo 0:00 przy pustej dacie edytowanej) — Godzina |
| Typ, Operacja, Czytnik RCP, Uwagi | z oryginału |
| Powiązanie z oryginałem (`WeWyO`) | oryginał |
| Zaimportowany | nie |
| Stan | Nieoznaczony |

Zapis **nie jest kopiowany**, gdy w Danych RCP już jest:

- zapis powiązany z tym oryginałem (`WgWeWyO[oryginal]`), albo
- zapis bez powiązania z tym samym pracownikiem, datą, godziną i typem (np. ze
  standardowego importu z czytnika). Szukany kluczem `WgPracownik[pracownik, data]`
  — obejmuje też zapisy dodane w tej samej sesji. Pierwsza wersja używała
  `FieldCondition` na podtabeli, który zapisów niezapisanych jeszcze w bazie nie widział
  i dublował odbicie z importu wykonanego w tej samej sesji (scenariusz W3).

Każdy oryginał jest kopiowany raz — późniejsza zmiana oryginału ani usunięcie
kopii nie są synchronizowane.

## Zadanie przy zapisie (to)

Wyzwalacz na `WejsciaWyjsciaO` (plik `KopiujOryginalneDaneRCP - wyzwalacz WejsciaWyjsciaO`,
przeznaczenie Uniwersalny) zwraca pracownika zapisu; `IsEnable()` kopiuje zmienione
oryginały tego pracownika (`WejsciaWyjsciaO.Rows.Changed`) i zwraca `false` — enova
nie tworzy rekordów zadań (Tasks). Błąd kopiowania jest przechwytywany, żeby nie
blokować zapisu oryginału.

enova uruchamia wyzwalacze **tylko w sesji z operatorem** (`Saver.doEventsAndTasks`:
`!IsInternal && Login.Operator != null`) — import według rekordów (`dbmgr importxml`)
czy zapis prosto do bazy ich nie uruchamia (scenariusz W4); takie zapisy dogania harmonogram.

## Konfiguracja w enova

Definicja zadania (Ustawienia/Systemowe/Definicje zadań):

| Pole | Wartość |
|---|---|
| Nazwa | Kopiuj oryginalne dane RCP |
| Tabela | Pracownicy |
| Algorytm (kod) | tak |
| Typ definicji | **Brak** |
| Blokada | nie |

Sekcje kodu z pliku `KopiujOryginalneDaneRCP` (format znaczników `//** begin ... **`).
Wyzwalacz: tabela `WejsciaWyjsciaO`, kod z pliku wyzwalacza.

Po zmianie kodu SQL-em: `update RuntimeProjects set Description=Description where ID=2`
(stamp), a potem na definicji zadania czynność **„Wymuś rekompilację”** — serwer
(`server.dll`) trzyma skompilowany kod w pamięci i sam go nie przeładowuje (usługa
harmonogramu przekompilowuje się sama ok. minuty po zmianie stampu).

## Dokumentacja i testy

- `Instrukcja_Kopiowanie_RCP.docx` — opis obu zadań, konfiguracja, komunikaty, scenariusze
  (format jak `Weryfikatory czasu pracy/Instrukcja_Import_RCP.docx`).
- `Scenariusze testowe kopiowania danych RCP.xlsx` — arkusze „Kopiowanie przy zapisie”
  (W1–W4) i „Kopiowanie harmonogramem” (H1–H4) z wynikami testu.
- Dane testowe: `ImportyXML/Task kopiuj oryginalne dane RCP - test wyzwalacza.xml`
  (logika biznesowa, sesja operatora — MCP `import_business_xml`; konstruktor wiersza
  wymaga pracownika w atrybucie `prac`) i `... - test harmonogramu.xml`
  (`dbmgr importxml`, bez operatora; typ razem z `DefinicjaZdarzenia`).

## Status

- 2026-10-20, baza **Al**: TaskDefs **415** + TaskTriggers **238** (przy zapisie),
  TaskDefs **416** (harmonogram) + zadanie globalne Tasks **2** + ScheduleDefs **14**
  (host = zadanie; docelowo co 15 min, na czas testów użytkownik ustawił co minutę),
  planowane wywołanie aktywne. Usunięte próby: harmonogram 12 (z listy, bez hosta)
  i 13 (host = definicja — nie wywołuje akcji). Wspólny kod: CodeFiles **1016**
  „A1KopiowanieRCP” (RuntimeProjects 2 Business) — „Sprawdź kod”: 0 błędów; po przeniesieniu
  metody sprawdzone ponownie przy zapisie i harmonogramem.
- Test live 20.10.2026: **W1–W4 i H1–H4 pozytywne** (arkusz scenariuszy). Dwie poprawki
  w trakcie: klucz `WgPracownik[pracownik, data]` (W3) oraz godzina edytowana 0:00 przy
  pustej dacie edytowanej = brak edycji (H1 — import wg rekordów zostawia w bazie
  `GodzinaEdytowana = 0`, a nie `Time.Empty`; pierwsze kopie miały godzinę 0:00).
