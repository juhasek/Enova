# Nieobecności długotrwałe i RODO — sesja 2026-10-20

Sesja uruchomiona z SonetaFrame (baza **Al**, MCP `soneta_ui`). Dane pracowników w tej notatce
celowo tylko jako kody.

## Co zrobiono w bazie Al

1. **Przegląd nieobecności > 30 dni** (Kadry i płace → Kadry → Czas pracy → Nieobecności, zakres
   2020–2027): przed sesją w całej bazie były tylko 4 nieobecności, żadna powyżej 30 dni.
2. **Dodane zwolnienia chorobowe** (definicja „Zwolnienie chorobowe”, przyczyna domyślna „Zwolnienie
   lekarskie”, bez kodu choroby i numeru ZLA) — zapisane przez MCP na kartotece pracownika
   (Kalendarz → Nieobecności → Dodaj), bez błędów weryfikatorów:

   | Kod | Okres | Dni |
   |---|---|---:|
   | 006 | 01.12.2026 – 29.01.2027 | 60 |
   | 007 | 01.10.2026 – 04.11.2026 | 35 |
   | 008 | 15.09.2026 – 29.10.2026 | 45 |
   | 009 | 01.09.2026 – 01.11.2026 | 62 |
   | 013 | 01.08.2026 – 30.10.2026 | 91 |

   006: start przesunięty na 01.12 (operator wybrał), bo 01–30.11.2026 ma zaplanowany urlop wypoczynkowy.
   Zwolnienia 007/008/009/013 sięgają wstecz — przy naliczonych wypłatach za 08–10/2026 potrzebne korekty.
3. **Zestawienie** „Długotrwałe zwolnienia lekarskie (> 30 dni)” pokazane operatorowi jednorazowo
   w oknie enova (`show_content`, markdown) — nie jest zapisane w bazie.

## Wydruk skierowań — porzucony

- Wydruków enova nie da się uruchomić przez MCP (brak narzędzia; wydruki to osobny mechanizm, nie
  czynność z `get_actions`). Jedyna droga do „prawdziwego” wydruku z kodu: dodatek DLL z czynnością
  drukującą wzór do PDF — zaproponowane, nie zamówione.
- PDF wygenerowany z HTML (Edge headless) został **usunięty** wraz z plikami tymczasowymi; operator
  zrezygnował z generowania PDF — wydruki robi sam w enova.

## RODO — ustalenia

- Wszystko, co zwraca MCP/SQL, trafia do rozmowy, czyli do Anthropic. W tej sesji wysłane zostały m.in.
  PESEL-e i adresy 5 osób oraz PDF przez `SendUserFile` (bez pytania — błąd, odnotowany).
- Anonimizacja po stronie modelu jest niemożliwa — trzeba nie pobierać danych (minimalizacja) albo
  maskować je przed wysłaniem (lokalny skrypt / pośrednik MCP — niezrealizowane).
- Pseudonimizacja (kody) to nadal dane osobowe; do weryfikacji z IOD: podstawa przetwarzania
  i umowa powierzenia z dostawcą AI (szczególnie dane o zdrowiu).
- Reguła pracy zapisana w pamięci projektu: `feedback_dane_osobowe_minimalizacja.md`.

## Konfiguracja

- `~/.claude/settings.json`: `permissions.allow` += `Bash(sqlcmd *)`, `PowerShell(sqlcmd *)`
  (zapytania modyfikujące nadal tylko po zgodzie operatora — zasada, nie blokada techniczna).
