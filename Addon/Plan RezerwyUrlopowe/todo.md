# Dodatek „Rezerwy urlopowe” (A1.RezerwyUrlopowe) — TODO

Stan planu (2026-10-09): Etap 1, 2, 3 zaakceptowane. Otwarte nieblokujące: 4, 5, 11, 12, 16 (`otwarte-kwestie.md`).
Rozwiązanie skryptowe (`Rezerwy urlopowe/`) **zostaje bez zmian** i działa równolegle (kwestia 15).

## Uzupełnienie planu
- [x] Etap 1 — wizja (zaakceptowany)
- [x] Etap 2 — architektura (zaakceptowany)
- [x] Etap 3 — specyfikacja (zaakceptowany)
- [ ] Zamknięcie otwartych kwestii nieblokujących: 4 (domyślne parametry), 5 (umowy cywilnoprawne), 11 (prawo otwarcia),
      12 (zatwierdzanie planowanych list przy zamknięciu), 16 (widoczność DLL w kodzie generowanym — test W4)

## Implementacja (kolejność)

| # | Zadanie | Skill / wzorzec | Szac. h |
|---|---|---|---|
| 1 | Rusztowanie solucji w `Addon/RezerwyUrlopowe/`: `A1.RezerwyUrlopowe`, `.UI`, `.Tests` (net8, Reference do DLL serwera 2512.5.6 jak `Addon/A1PelnaListaPlacAddon.csproj`) | `/soneta-programming` (new-addon-cli), pamięć: kompilacja dodatku lokalnie | 3 |
| 2 | `business.xml`: `RezerwaUrlopowa` (`RezerwyUrlopowe`), `PozycjaRezerwyUrlopowej` (`PozRezerwUrlop`), enumy, subrow `ParametryNaliczenia`, klucze/indeksy | `/soneta-business-xml` | 4 |
| 3 | `*.rightstree.xml` — gałąź „Kadry i płace / Rezerwy urlopowe”, węzeł „Otwieranie zamkniętej rezerwy” | `/soneta-business-xml` (rights-tree) | 1 |
| 4 | Konfiguracja `CfgNodes` + strona Opcji `Config.RezerwyUrlopowe.pageform.xml` + extender | `/soneta-form-xml`, pamięć: zakładka w Opcjach | 5 |
| 5 | `KalkulatorRezerwyUrlopowej` + `SymulatorLimituUrlopu` — przeniesienie 1:1 z algorytmu skryptowego (parametry z konfiguracji zamiast stałych) | `/soneta-programming` | 8 |
| 6 | `AlgorytmRezerwyUrlopowej : AlgorytmBase` (klasa algorytmu elementu) | `/soneta-programming` | 3 |
| 7 | Dane inicjujące `*.dbinit.xml` (EmbeddedResource): 2 elementy „(dodatek)”, 2 definicje AREZURL / ABUDREZURL, własne GUID-y; test `dbmgr importxml` na bazie testowej | `/soneta-config` (import-export-xml), `/soneta-tools` | 3 |
| 8 | Weryfikatory 3.6 (unikalność, konfiguracja elementów i definicji) | `/soneta-programming` (verifiers) | 3 |
| 9 | `NaliczRezerweUrlopowaWorker` (+ okno parametrów, kontrole, postęp, planowana lista, przepisanie narzutów) | `/soneta-programming` (worker-extender), pamięć: okno parametrów czynności | 8 |
| 10 | `ZamknijRezerweWorker`, `OtworzRezerweWorker`; pola wyliczane KwotaPoprzednia / Zmiana | `/soneta-programming` | 3 |
| 11 | Listy i folder menu `Kadry i płace/Płace/Rezerwy urlopowe` (+ folder Pozycje), formularze rezerwy i pozycji, zakładka w kartotece pracownika | `/soneta-form-xml`, `/soneta-config` (scan-folders) | 8 |
| 12 | Wydruk „Zestawienie rezerwy wg MPK” (.repx), raport zmian m/m | — | 5 |
| 13 | Testy integracyjne (3.13): kalkulator RU-01…RU-30 (parametryzowane), symulator, algorytm, worker + kontrole, zamknięcie/otwarcie, weryfikatory, zgodność ze skryptem, wydajność 1 000 pracowników | `/soneta-programming` (integration-tests) | 15 |
| 14 | Dane demo / testowe (XML jak `ImportyXML/`) | `/soneta-config` | 3 |
| 15 | Dokumentacja: instrukcja użytkownika (docx, styl jak `Instrukcja_Rezerwa_Urlopowa.docx`), README dodatku, instrukcja instalacji (ExtPath, restart) | — | 4 |
| 16 | Pilotaż u klienta: instalacja obok rozwiązania skryptowego, 2 zamknięcia miesiąca, porównanie wyników skrypt ↔ dodatek | — | 8 |
| | **Razem** | | **~84 h** (+ planowanie ~10 h → ~95 h, w widełkach Etapu 1) |

Pozycje szablonu nieobjęte zakresem v1: procesy Workflow (brak akceptacji — tylko stany), integracje zewnętrzne,
transakcje serwerowe (niepotrzebne — 3.8), wskaźniki BI (kierunek rozwoju).

## Kryteria zakończenia
- Wszystkie testy integracyjne zielone, w tym RU-01…RU-30 i zgodność kwot ze skryptem.
- Pilotaż: 2 zamknięte miesiące bez błędów krytycznych; rozwiązanie skryptowe nadal działa bez zmian.
