# Porządek w GitHub – sesja 2026-10-26

## Cel
Uporządkowanie repozytorium wg zasady: `main` = rozwiązania zaakceptowane,
`Testy` = tylko rozwiązania w trakcie testów.

## Stan zastany
- Brak otwartych PR i issues.
- Branche: `main`, `Testy`, `claude/gifted-goldberg-3z9lm2` (porzucony, 16.10;
  jeden własny commit: stary raport etapu 1 kalkulatora podstawy zasiłku + 2 pliki `.whl`).
- `Testy` ma osobną historię (od 2026-10-20), bez wspólnego przodka z `main`.
- Treść: 496 plików identycznych w `Testy` i `main`, 31 plików tylko/nowsze w `Testy`.

## Co zrobiono
- Użytkownik potwierdził: wszystko, co jest na `main`, jest zaakceptowane.
- Z `Testy` usunięto 393 pliki identyczne z `main` (sprawdzone 1:1 po hashu blobów).
- W `Testy` zostały:
  - pliki wspólne: `CLAUDE.md`, `README.md`, `.claude/`, `.gitignore`, `.mcp.json`,
    `Addon/.gitignore`, `Pobrane/README.md`;
  - w testach: `Addon/ImportDefinicjiElementow/`, `Addon/Plan ImportDefinicjiElementow/`,
    `Addon/MetryczkaKlienta/`, `Wzor_analizy/` (zmiany w generatorze), 3 opisy sesji
    w `Zadania/` (analiza klienta B ×2, duży pasek wypłaty).
- `CLAUDE.md`: nowa sekcja „Podział main / Testy” (przenoszenie pojedynczych
  rozwiązań, bez scalania całych branchy; poprawka = przywrócenie z `main`).

## Otwarte punkty
- Branch `claude/gifted-goldberg-3z9lm2` – do decyzji użytkownika (usunąć / zachować raport).
- `CLAUDE.md` na `main` nie zawiera jeszcze nowej zasady – przeniesienie wymaga PR i zgody.

## Ciąg dalszy (2026-10-27)
- PR z nową zasadą w `CLAUDE.md` do `main`: https://github.com/juhasek/Enova/pull/32
  (branch `claude-md-podzial-do-main`), czeka na scalenie.
- Usunięcie brancha `claude/gifted-goldberg-3z9lm2` z sesji nie przeszło (serwer zrywa
  połączenie przy `git push --delete`) – do usunięcia ręcznie w GitHub (Branches).
