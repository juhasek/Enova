# Duży pasek wypłaty – sesja 2026-10-26

## Cel
Adres pracownika na pasku (strona do koperty) ma być brany w kolejności:
adres do korespondencji → adres zameldowania → adres zamieszkania.

## Co zrobiono
- `Raporty/SkanskaDuzyPasekWyplatySnippet` i `ImportyXML/SkanskaDuzyPasekWyplatySnippet.import.xml`:
  dodany trzeci krok – gdy brak korespondencyjnego i zameldowania, bierzemy `ph.Adres`
  (zamieszkania). Adres uznawany za pusty, gdy nie ma miejscowości.
- Commit `2ae9cb4` na `Testy`; na `main` przeniesiony tylko ten commit (`a11a9bc`,
  cherry-pick przez tymczasowy branch, usunięty po wypchnięciu). Pozostałe zmiany
  z `Testy` nie trafiły na `main`.

## Testy
Przetestowane przez wdrożeniowca u klienta – działa.

## Otwarte punkty
Brak.
