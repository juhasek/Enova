# Weryfikatory Al — sesja 2026-10-15

## 1. Scalenie weryfikatora normy okresu rozliczeniowego

Decyzja operatora: zostaje jedna metoda — logika dawnej „v2”, bez dopisku v2 w nazwie.

Wykonane w bazie Al (SQL, jedna transakcja):

- Dodatkowy kod do kompilacji `A1WeryfikatoryKalendarza` (CodeFiles ID 13): usunięta stara metoda
  `A1WeryfikujNormeWOkresieRozliczeniowym`, metoda `...V2` przemianowana na
  `A1WeryfikujNormeWOkresieRozliczeniowym`. Odczyt po zapisie zgodny z wgranym kodem.
- Definicja **37** „A1_Norma w okresie rozliczeniowym” bez zmian — woła tę samą nazwę metody, więc od
  razu działa na nowej logice. Podpięcia: kalendarze 3 miesięczny (45), Podstawowy system_1_8:16 (47),
  Standard (1), poziom Błąd.
- Definicja **2052** „A1_Norma w okresie rozl. v2” usunięta (nie była podpięta; wołałaby nieistniejącą
  metodę).
- Podbity znacznik projektu kompilacji (RuntimeProjects 29).
- Kopie sprzed zmiany (kod CodeFiles 13, wiersz definicji 2052) w scratchpadzie sesji.

Repo: commit 5a28096 — kod i opis w `Weryfikatory czasu pracy/Weryfikator normy okresu rozliczeniowego(.md)`,
pliki v2 usunięte.

Nowa logika względem pierwszej wersji: okres przycięty do zatrudnienia na etacie, miesiące minione nie są
rezerwą, komunikat wskazuje dni do poprawy („Skróć plan dni…” / „Wydłuż plan dni…”).

## 2. Restart serwera i test na żywo — PRZERWANE

- Zabite oba procesy `server.dll` (orchestrator postawił nowy o 09:50).
- SonetaFrame nie odpowiada na MCP `soneta_ui_al` (4× timeout 120 s) — prawdopodobnie po restarcie
  serwera wymaga ponownego zalogowania do bazy Al. Czeka na operatora.

Plan testu po powrocie:

1. Pracownik testowy **TEST-T7** (kod T7, kalendarz Standard, okres rozliczeniowy 1 miesiąc, maks.
   dobowe 8:00) — brak wpisów planu w 10 i 11.2026.
2. **Niedobór:** dzień roboczy listopada z planem 0:00 → oczekiwany błąd „Nie da się już osiągnąć normy…
   brakuje (8:00)… Wydłuż plan dni: XX.11 (+8:00)”.
3. **Brak błędu:** cofnięcie zmiany → zapis bez komunikatu.
4. **Nadmiar** (opcjonalnie): pełny plan dni roboczych + soboty po 8h → „Przekroczono limit… Skróć plan dni…”.

Każdy zapis przez MCP wymaga potwierdzenia „TAK” w oknie SonetaFrame.
