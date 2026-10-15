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

## 2. Restart serwera i test na żywo — przerwane (wznowione w pkt 3)

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

## 3. Test na żywo — wznowiony, kroki 1–4 OK

MCP `soneta_ui_al` znów odpowiada. Zakładka pracownika **Kalendarz/Norma czasu pracy**
(`PracownikPlanPracy2Page`) przez MCP rzuca „Object reference not set…”, więc plan zmieniał operator
w GUI.

- **Krok 1:** TEST-T7 (Pracownicy ID 56, kalendarz indywidualny 1058) bez dni w `DniKalendarza`.
- **Krok 2 (niedobór):** 02.11.2026 = 0:00 → zapis zablokowany komunikatem definicji 37:
  „Nie da się już osiągnąć normy godzin (160:00) w okresie rozliczeniowym ((1...30.11.2026)),
  brakuje (8:00)… Dni bez wpisu od bieżącego miesiąca: 19 (maks. 152:00). Wydłuż plan dni: 02.11 (+8:00)”.
  Liczby zgodne (20 dni roboczych w 11.2026 → 160:00). Dodatkowo, zgodnie z założeniem, odezwał się
  weryfikator liczby dni wolnych: „!Zaplanowano zbyt dużo dni wolnych (11) wobec wymaganej liczby (10)…”.
- **Krok 3 (brak błędu):** cofnięcie zmiany → zapis bez komunikatu; w bazie znów zero wpisów planu.
- **Krok 4 (nadmiar):** 20 dni roboczych 11.2026 po 8:00 (strefa „Praca w normie”) wgrane `dbmgr importxml`
  wg rekordów (bez weryfikatorów); operator dopisał w GUI sobotę 07.11 = 8:00 → zapis zablokowany:
  „Przekroczono limit godzin (160:00) o (8:00)… Skróć plan dni: 02.11 (8:00), 03.11 (8:00), … 13.11 (8:00)
  i 11 kolejnych” + weryfikator dni wolnych „!Za mało dni wolnych (9) wobec wymaganej liczby (10)…”.
  Sprzątanie: import z pustą kolekcją `<Dni>` i `fromto` listopada → TEST-T7 znów bez wpisów planu.
- Import przez MCP `import_business_xml` (logika biznesowa) kończy się „Object reference not set…”
  i nic nie zapisuje — tak samo jak zakładka kalendarza przez MCP.

Uwaga do ewentualnej poprawki: przy równych godzinach „Skróć plan dni” sortuje po dacie, więc
sobota 07.11 (faktyczna przyczyna nadmiaru) jest dopiero na 6. miejscu. Można by stawiać na początku
dni wolne wg kalendarza (soboty, święta) — nie zmieniane, do decyzji.

## 4. Weryfikator 2069 „A1_Brak zgody na pracę 8h” — test na żywo, 9/10 OK

- Pracownicy testowi **NP1–NP9** (Pracownicy ID 59–67) wgrani `dbmgr importxml` z
  `ImportyXML/Weryfikator brak zgody 8h - pracownicy i plan.xml` (kal. 47, NP9 kal. 45; zgoda 8h tylko NP4;
  dni planu 11.2026 z 6:00). Pole `StopienNiepelnosp` nie da się zaimportować wg rekordów („Ambiguous match”),
  więc orzeczenia PFRON/ZUS dodane przez MCP na zakładce Kadry/Orzeczenia o niepełnosprawności.
- Wyzwalanie weryfikatorów przez MCP: lista **Podgląd tabel/Kalend/DzienKalendarzaBase** → `open_form` dnia →
  `edit_grid_rows` na `Strefy` (czas strefy) → komunikaty wracają już w `fieldUpdate.verifiers`.
  (Zakładka Kalendarz pracownika i `import_business_xml` przez MCP dają NRE.) Pierwsze `open_form` po
  `cancel_form` często przekracza 120 s — ponowienie działa.
- Wyniki w arkuszu **„Brak zgody 8h”** w `Weryfikatory czasu pracy/Scenariusze testowe weryfikatorow czasu pracy.xlsx`.
- **Negatywny scenariusz 8:** zmiana stopnia w trakcie zapisu historii (umiarkowany do 15.11, lekki od 16.11) —
  13.11 bez błędu. Przyczyna i proponowana poprawka w `KontrolePlanuZAlgorytmuKalendarza.md` (sekcja 8).
- Uwaga: norma okresu (37) nie uwzględnia skróconej normy osób niepełnosprawnych — konflikt z 2069.
- Dane testowe zostają w bazie Al (do ponownych testów). Zapisane dni 8:00: NP4, NP5, NP6 (02.11).

## 5. Import kalendarzy klienta (eksport z 2512) do Al (2610)

- Błąd importu „SwiateczneMiesiecznie … False is not a valid value for NadgodzinyŚwiąteczneMiesięcznie”:
  w 2610 pole „Nadgodziny za pracę w niedziele i święta rozliczane miesięcznie” jest enumem
  (Nie / Tak / TylkoPowyżejNormyDobowej), w 2512 było bool. Poprawka pliku: False → Nie, True → Tak
  (jedyne pole bool → enum; reszta zgodna z kolumnami 2610).
- Operator zaimportował 13 kalendarzy (12 nowych, ID 1070–1081, + nadpisany 47 „Podstawowy system_1_8:16”).
- Na 12 nowych przez MCP (Ustawienia → Kadry i płace → Kalendarze → Kalendarze, podformularz kalendarza):
  zakładka Weryfikatory – tylko 2069 „A1_Brak zgody na pracę 8h” jako Błąd, zakładka Algorytm wyczyszczona
  (enova wstawia pusty szablon, 1180 znaków). Sprawdzone w bazie.
- Ostatecznie (decyzja operatora): kal. 47 i 12 nowych (1070–1081) mają podpięcia jak kal. 47 przed importem —
  Ostrzeżenie: 1, 3 (Odpoczynek dobowy dzień planu / dzień pracy), 17 (Odpoczynek tygodniowy, plan);
  Błąd: 35, 37, 1036, 2044, 2048, 2049, 2051, 2069; algorytm pusty (szablon). Sprawdzone w bazie (11 podpięć każdy).
  Wcześniej po imporcie: 13 podpięć klienta, bez 2069, stary algorytm (aktywne tylko metody pomocnicze,
  wszystkie Sprawdź… zakomentowane).
- Import przez MCP `import_business_xml` i edycja kalendarza otwartego przez `open_form` → „Brak prawa do
  zapisu danych” (sesja operacyjna, kalendarze tylko do odczytu); działa tylko przez formularz konfiguracji.
  Zapis konfiguracji wymaga kliknięcia TAK w SonetaFrame.
