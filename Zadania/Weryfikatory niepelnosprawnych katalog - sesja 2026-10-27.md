# Weryfikatory niepełnosprawnych – osobny katalog – sesja 2026-10-27

## Cel
Zebranie wszystkich materiałów o weryfikatorach dla osób z niepełnosprawnością
(definicje 2069, 2070–2072, 3070, 3071; stara 2039 zablokowana) w jednym katalogu
`Weryfikatory czasu pracy/Niepelnosprawni/` oraz osobna instrukcja i arkusz scenariuszy.

## Co zrobiono
- Rozwiązanie jest zaakceptowane (leży na `Produkcja`), więc zmiany idą przez branch
  `niepelnosprawni-do-Produkcja` i **PR #34** do `Produkcja` (decyzja użytkownika: „jeśli jest
  na produkcji, dodaj na produkcji”). Scalenie PR – po stronie użytkownika (automatyczne scalanie
  bez przeglądu zablokowane).
- Nowy katalog `Weryfikatory czasu pracy/Niepelnosprawni/`:
  - `Weryfikatory niepelnosprawnych.md` – sekcje 8, 8a, 8b wycięte z
    `KontrolePlanuZAlgorytmuKalendarza.md` (tam został odsyłacz),
  - `Instrukcja_Weryfikatory_niepelnosprawnych.docx` – kopia `Instrukcja_Weryfikatory.docx`
    tylko z 6 sekcjami (2069, 2071, 2072, 2070, 3070, 3071); spis treści do odświeżenia (F9 /
    updateFields przy otwarciu),
  - `Scenariusze testowe weryfikatorow niepelnosprawnych.xlsx` – arkusze Informacje (nowy opis),
    „Brak zgody 8h”, „Zgody nadgodziny i noc”, „Norma tygodniowa 35-40h” (z wynikami testów),
  - 3 pliki danych testowych XML przeniesione z `ImportyXML/`.
- Docx/xlsx wygenerowane skryptem PowerShell na XML pakietu (bez Office na maszynie – pliki
  nie otwarte w Word/Excel, tylko sprawdzona struktura).

## Ustalenia
- Kod metod zostaje w jednym pliku `KontrolePlanuZAlgorytmuKalendarza` (= CodeFiles 13 w bazie),
  bez rozdzielania.
- Wspólne `Instrukcja_Weryfikatory.docx` i `Scenariusze testowe weryfikatorow czasu pracy.xlsx`
  bez zmian – sekcje niepełnosprawnych są na razie w dwóch miejscach.

## Otwarte
- Scalić PR #34 i sprawdzić docx/xlsx w Office.
- Decyzja: usunąć sekcje niepełnosprawnych ze wspólnej instrukcji i arkusza?
- Stare opisy sesji w `Zadania/` (2026-10-15, 2026-10-19) wskazują ścieżki `ImportyXML/...` – nieaktualne.
- Pytania do klienta bez zmian: niepełny tydzień w 3070, zgoda 8h = 40/8, średnia pon–pt przy
  zmianie stopnia, włączenie 2039, kal. 1080 (norma 8:00 przy limicie 7:00).
