# Weryfikatory Al — sesja 2026-10-13

## 1. Nowy weryfikator „A1_Brak zgody na pracę 8h”

Zlecenie: gdy na etacie pole „Zgoda na pracę 8h” jest niezaznaczone, przełożony nie może zaplanować
więcej niż 7h pracy — błąd blokujący zapis planu.

Ustalenia z operatorem:

| Pytanie | Decyzja |
|---|---|
| Kogo dotyczy | tylko stopień niepełnosprawności **umiarkowany lub znaczny** w dniu planu (pole jest puste także u osób bez niepełnosprawności) |
| Istniejąca definicja 2039 „A1_Norma niepełnosprawnych (dzień)” | zostaje bez zmian (zablokowana); nowa definicja obok |
| Podpięcie | kalendarze 45 (3 miesięczny) i 47 (Podstawowy system_1_8:16), poziom **Błąd** |

Reguła: stopień z danych o niepełnosprawności albo PFRON (wyższy z nich, okres `OkresExt`), zgoda
z etatu w zapisie historii obowiązującym w dniu (`Etat.PracownikNiepelnosprawnyZgodaNaPrace8h`);
plan dnia > 7:00 → komunikat:

> Dla dnia (…) zaplanowano czas (…), a pracownik bez zgody na pracę 8h (stopień niepełnosprawności …)
> może pracować maksymalnie 7:00 - pracownik

## 2. Wgranie do bazy Al (SQL, w jednej transakcji)

- Dodatkowy kod do kompilacji `A1WeryfikatoryKalendarza` (CodeFiles ID 13): dopisana metoda
  `A1BrakZgodyNaPrace8h(KalkulatorPlanuBase, Date)`; kopia kodu sprzed zmiany w scratchpadzie sesji.
- Definicja weryfikatora kalendarza **2069** „A1_Brak zgody na pracę 8h” — rodzaj Dzień planu, priorytet 29.
- Podpięcia (WeryfKalend 2030, 2031) do kalendarzy 45 i 47 jako Błąd.
- Podbity znacznik projektu kompilacji (RuntimeProjects 29).

Kompilacja: cała klasa kompiluje się lokalnie na bibliotekach 2512.5.6. Na 2610 nie dało się sprawdzić
lokalnie (wymaga .NET 10), a `dbmgr compile` 2610 kończy się bez błędu, ale nie tworzy nowej biblioteki —
nie jest dowodem kompilacji.

Repo: metoda i opis w `Weryfikatory czasu pracy/KontrolePlanuZAlgorytmuKalendarza` (+ `.md`, sekcja 8),
commit `6aa8834`.

## 3. Do zrobienia

1. Restart serwera enova — podpięcia wstawione SQL-em są widoczne na zakładce Weryfikatory dopiero po restarcie.
2. Test na żywo: w bazie Al nikt nie ma stopnia niepełnosprawności — ustawić pracownikowi z kalendarzem
   45/47 stopień umiarkowany, zostawić zgodę pustą, zaplanować 8h (oczekiwany błąd), potem zaznaczyć zgodę
   (zapis przechodzi). Błąd kompilacji wyłączyłby wszystkie weryfikatory A1 naraz.
3. Ewentualnie: scenariusze testowe w arkuszu weryfikatorów i sekcja w instrukcji weryfikatorów.
