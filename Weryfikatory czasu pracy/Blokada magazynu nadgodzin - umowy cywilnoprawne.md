# A1_Magazyn nadgodzin - umowy cywilne

## Cel

Godziny przepracowane na umowie cywilnoprawnej (zlecenie, dzieło) nie mogą trafiać do
**magazynu nadgodzin**. Magazyn dotyczy wyłącznie czasu pracy z umowy o pracę.

## Skąd ryzyko

W enova magazyn nadgodzin zasilają wyłącznie strefy w **czasie pracy pracownika** (Kadry → Czas pracy),
nigdy strefy kalendarza umowy cywilnoprawnej — tam magazyn nie istnieje. Problem pojawia się, gdy
czas pracy zleceniobiorcy jest rejestrowany na kartotece pracownika (np. z importu RCP): pracownik
bez etatu ma plan 0:00, więc import z podziałem stref zapisuje całą pracę jako
„Praca poza normą” + **„Nadgodziny do przeniesienia”**, a ta strefa tworzy pozycję w magazynie.

## Działanie

Kontrola uruchamia się przy zapisie dnia pracy (rodzaj definicji **Dzień pracy**) — także po dodaniu,
zmianie lub usunięciu strefy.

1. Szuka w dniu stref, które zasilają magazyn: definicja strefy z rozliczeniem
   **„W kolejnych miesiącach”** (np. *Nadgodziny do przeniesienia*, *Rozliczenie wyjścia prywatnego*).
   Rozpoznanie po ustawieniu definicji, nie po nazwie.
2. Brak takich stref (lub czas 0) — brak uwag.
3. Sprawdza zatrudnienie w tym dniu metodami enova:
   - jest umowa o pracę (`OkresZatrudnieniaEtat`) → brak uwag (to czas z etatu),
   - brak etatu i brak umowy cywilnoprawnej (`OkresZatrudnieniaUmowa`) → brak uwag (poza zakresem tej kontroli),
   - brak etatu, jest umowa cywilnoprawna → błąd:

   > Dnia (data) pracownik pracuje na umowie cywilnoprawnej - czasu (X) nie można przenieść do magazynu
   > nadgodzin (strefa Nadgodziny do przeniesienia) - pracownik

Podpięta jako **Błąd** — zapis dnia jest blokowany.

## Konfiguracja w bazie testowej

- Kod metody: Dodatkowy kod do kompilacji `A1WeryfikatoryKalendarza` (CodeFiles ID 13), metoda
  `A1BlokadaMagazynuNadgodzinUmowy`.
- Definicja: `DefWeryfKalend` ID **2051** „A1_Magazyn nadgodzin - umowy cywilne”, rodzaj Dzień pracy (50).
- Podpięcie (Typ = Błąd): **Standard** (1), „3 miesięczny” (45), „Podstawowy system_1_8:16” (47).
  Kalendarz **Standard jest konieczny** — dla pracownika bez etatu enova bierze do kontroli dnia
  pracy właśnie kalendarz Standard.
- Wgrane SQL-em — widoczne w programie dopiero po restarcie serwera enova.

## Do decyzji / ograniczenia

- **Pracownik z etatem i jednocześnie umową cywilnoprawną** — czas pracy na kartotece traktowany
  jest jako czas z etatu i kontrola go nie blokuje (enova nie rozróżnia, z której umowy pochodzą godziny).
- Blokowane jest tylko **zasilanie** magazynu. Strefy odbioru/wypłaty nadgodzin (rozliczenie
  „Z poprzednich miesięcy”, „Wypłata nadgodzin”) nie są sprawdzane — u zleceniobiorcy i tak nie mają
  czego rozliczać.
- Import RCP z podziałem stref ALDI dla zleceniobiorcy utworzy „Nadgodziny do przeniesienia”, które
  kontrola zablokuje — trzeba sprawdzić, jak import zachowa się przy błędzie zapisu (czy pominie tylko
  tę strefę, czy cały dzień).
- Status: wgrane i skompilowane 2026-10-07, **niesprawdzone na żywo**. Pracownik testowy: 082 (tylko
  umowa cywilnoprawna od 2015, bez etatu).
