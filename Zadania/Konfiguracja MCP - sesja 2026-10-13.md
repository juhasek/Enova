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

## 3. Porządki po bazie testowej ramki

- Usunięta baza testowa ramki (SQL, rejestracja `dbmgr`, źródło w ustawieniach ramki, serwer MCP,
  cache `buscall`) — na maszynie zostały tylko bazy **Al** i **Claude**.
- Usunięta komenda symulacji podstawy zasiłkowej (plik komendy + uprawnienie); historia `Testy`
  przepisana i wypchnięta (`--force-with-lease`).

## 4. Naliczenie wypłat — baza Al, pracownicy 006, 007, 008

Zakres: 12 miesięcy **10.2025–09.2026**, definicja **LPE**, płatna z dołu, data listy = ostatni dzień
miesiąca, data wypłaty = 5. dzień następnego miesiąca (wzór: istniejąca lista LPE za 10.2026).

Sposób (MCP `soneta_ui_al`): *Kadry i płace/Kadry/Pracownicy* → zaznaczenie 006/007/008 → czynność
**Nalicz wypłaty** (`Soneta.Place.NaliczanieSeryjne+PracownicyConcurretWorker`) → parametry
(`Okres`, `DataListy`, `DataWypłaty`, `DefinicjaListy=Symbol=LPE`, jednostka pusta) → `accept_form`
→ formularz „Naliczanie wypłat” → `save_form` (zapis bez udziału operatora).

Wynik: 24 listy płac — co miesiąc osobno **LPE/SR** (006, 007) i **LPE/ADM** (008), numery
`LPE/SR/RRRR/MM/1` i `LPE/ADM/RRRR/MM/1`, 36 wypłat. Listy **niezatwierdzone**.
Odchylenia kwot od typowego miesiąca: 11.2025 (wszyscy), 04.2026 (006), 08.2026 (wszyscy) —
nie analizowane.

## Otwarte punkty

- [x] Sprawdzić `/mcp` — bazy **Al** i **Claude** łączą się i mają narzędzia.
- [ ] Potwierdzić GUID agenta AI w bazach Al i Claude — w `.mcp.json` użyto
      `00000000-00a1-0003-0001-000000000000`; agent jest definiowany osobno w każdej bazie.
- [x] Dodatek rezerw urlopowych ładuje się w bazie Al na 2610 (czynność „Nalicz rezerwę urlopową...” widoczna na liście pracowników).
- [ ] Wyjaśnić odchylenia kwot wypłat w 11.2025, 04.2026 (006) i 08.2026; ewentualnie zatwierdzić listy.
- [ ] Wyjaśnić typ naliczania definicji elementu *Rozwiązanie odprawa emerytalno-rentowa* —
      naliczanie wypłat na liście LPE (z dołu) przerywa błąd
      „Błędny typ naliczania wypłaty. Element: Rozwiązanie odprawa emerytalno-rentowa".
- [ ] Po potwierdzeniu działania baz usunąć kopie `*_przed2610_20261013.bak`.
