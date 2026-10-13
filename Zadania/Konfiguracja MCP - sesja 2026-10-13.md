# Konfiguracja MCP — sesja 2026-10-13

## 1. Konfiguracja projektu do pracy z enova365 przez MCP

Commit `1c465f0` na `Testy` (wypchnięty do `origin/Testy`):

| Plik | Zawartość |
|---|---|
| `.mcp.json` | serwery `soneta_ui_al` (Al), `soneta_ui_claude` (Claude) — `buscall.exe mcp` |
| `.claude/skills/soneta-mcp-ui/` | skill obsługi interfejsu programu |
| `CLAUDE.md` | sekcja „Obsługa programu enova365 (MCP)" z mapowaniem serwer → baza |

Poza repo (lokalnie): `.claude/settings.local.json` — włączenie serwerów MCP i uprawnienia.

## 2. Uruchomienie MCP na bazach Al i Claude

- Objaw: `/mcp` pokazywał „no tools · connected" — `buscall --db <baza> mcp` zwracał pustą listę narzędzi.
- Przyczyna: bazy Al i Claude były w wersji 2512, a narzędzia MCP daje dopiero serwer 2610;
  dodatkowo pod `localhost:5000` działały osierocone procesy web/router 2512.
- Naprawa:
  1. kopie zapasowe baz przed konwersją (`*_przed2610_20261013.bak` w katalogu Backup SQLEXPRESS),
  2. konwersja Al i Claude do 2610 (`dbmgr convert`),
  3. dopisanie źródeł `http://localhost:5000;db=Al` i `;db=Claude` w ustawieniach ramki SonetaFrame,
  4. restart usługi / orchestratora 2610,
  5. ręczne zalogowanie operatora do każdej bazy w ramce — dopiero wtedy `buscall` pobiera listę narzędzi,
  6. `/mcp` → reconnect: Al i Claude mają po 50 narzędzi.

## Otwarte punkty

- [x] Sprawdzić `/mcp` — bazy **Al** i **Claude** łączą się i mają narzędzia.
- [ ] Potwierdzić GUID agenta AI w bazach Al i Claude — w `.mcp.json` użyto
      `00000000-00a1-0003-0001-000000000000`; agent jest definiowany osobno w każdej bazie.
- [ ] Sprawdzić, czy dodatek rezerw urlopowych (DLL zbudowany pod 2512) ładuje się w bazie Al na 2610.
- [ ] Wyjaśnić typ naliczania definicji elementu *Rozwiązanie odprawa emerytalno-rentowa* —
      naliczanie wypłat na liście LPE (z dołu) przerywa błąd
      „Błędny typ naliczania wypłaty. Element: Rozwiązanie odprawa emerytalno-rentowa".
- [ ] Po potwierdzeniu działania baz usunąć kopie `*_przed2610_20261013.bak`.
