# Analiza przedwdrożeniowa klienta „B” – zestawienie zadań po dniach 1–2 – sesja 2026-10-26

## 1. Zlecenie

Wyciągnąć z dwóch dni warsztatów (21.10 Kadry cz. 1, 23.10 Kadry cz. 2 i Płace cz. 1) listę rzeczy do przygotowania
przez AltOne oraz plików i informacji, które ma przesłać Klient. Wynik w Excelu.

## 2. Wynik (poza gitem – `Klienci\<klient>\Notatki\`)

- `2026-10-26_Zadania_AltOne_i_Klienta_po_D1_D2.xlsx`, trzy arkusze:
  - „AltOne – do przygotowania” – 33 pozycje (Obszar, Co przygotować / sprawdzić, Szczegóły, Źródło, Zależy od,
    Termin, Status, Uwagi),
  - „Klient – do przesłania” – 25 pozycji, z kolumną „Rodzaj” (plik / informacja / decyzja),
  - „Informacje” – źródła i oznaczenia (pkt, OQ, WR, D, R).
- Źródła: pliki treści notatek D1 i D2 (`zrodla\`) i pliki weryfikacji materiałów obu dni.
- Arkusz zbudowano konsolówką net10 na DevExpress.Spreadsheet 25.2 z katalogu serwera 2610. Narzędzie zostało
  w scratchpadzie, bo dane są wpisane w kod, więc go nie commitowano.

## 3. Ustalenia

- Wszystkie pozycje mają status „Otwarte” i każda ma źródło (dzień, punkt notatki, numer OQ/WR/D/R).
- Arkusz będzie aktualizowany po kolejnych dniach warsztatów (ten sam plik, nowe wiersze).
- Plik zawiera nazwy spółek, więc zostaje w `Klienci\` – do repo trafia tylko ten opis.

## 4. Otwarte punkty

- Formatka definicji stanowisk (z zadań D1) jest pewnie zbędna po decyzji D-009 – do potwierdzenia rezygnacji.
- Formatka przyczyn rozwiązania umowy jest w D1, w D2 jej nie powtórzono – do potwierdzenia.
- Arkusz charakterystyki spółek miał termin 23.10.2026 – sprawdzić, czy wpłynął.
- Arkusz można przekazać Klientowi jako obiecane podsumowanie rzeczy do uzupełnienia (zadanie z D2).
- 27.10.2026 – warsztat z czasu pracy; potem dopisać nowe pozycje.
