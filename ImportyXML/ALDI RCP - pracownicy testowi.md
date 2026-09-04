# ALDI RCP — pracownicy testowi (podział nadgodzin, zlecenia 277381 / 277382)

Plik: `ALDI RCP - pracownicy testowi.xml` — import **według rekordów** do bazy `Al`.

```
dbmgr importxml Al "ALDI RCP - pracownicy testowi.xml" --standard
```

Guidy stałe (`a1d10000-…`) → import jest **idempotentny** (ponowne uruchomienie nie
duplikuje danych).

## Co tworzy

14 pracowników — **1 pracownik = 1 scenariusz** z dokumentacji
`Dokumentacja_scenariusze_testowe.html`. **Kod pracownika = numer scenariusza.**

| Kod | Nazwisko / Imię | Wymiar etatu | Kalendarz | Plan dnia (pn–pt) |
|-----|-----------------|--------------|-----------|-------------------|
| S1–S7 | TEST-S* / Pełny | **1/1** | Standard | 8:00–16:00 (8h) |
| T1, T2, T3, T4, T6 | TEST-T* / Niepełny | **3/4** | Standard | skaluje do 8:00–14:00 (6h) |
| T5 | TEST-T5 / Równoważny | **1/1** | Standard | 8:00–16:00 (8h) |
| T7 | TEST-T7 / Pełny | **1/1** | Standard | 8:00–16:00 (8h) |

Wspólne: zatrudnienie od **2026-01-01**, umowa o pracę na czas nieokreślony,
wydział „Główny wydział firmy", ubezpieczenia obowiązkowe od dnia zatrudnienia,
PESEL z poprawną cyfrą kontrolną, komplet `PracHistoria2` + 3 adresy (kartoteka
otwiera się w GUI bez błędów — wzór: `Dodatek roczny - test 01 pracownicy.xml`).

Kalendarz „Standard" ma `UwzglWymiarEtatu = Tak`, więc etat 3/4 sam obniża normę
dobową do 6h i plan do 8:00–14:00 (pkt 3.2 dokumentacji — „kalendarz 6-godzinny
lub ręczna korekta planu").

## Poza zakresem tego pliku — robi tester ręcznie w GUI

1. **Magazyn nadgodzin rozliczany od** — Narzędzia → Opcje → Kadry i płace →
   Kalendarze → Czas pracy; miesiąc ≤ sierpień 2026. W bazie `Al` było **puste**.
   Wymagane przed pierwszym importem RCP (scenariusz S7).
2. **Wpisy „Dane z RCP"** (Kadry → Czas pracy → Dane z RCP → Nowy) — pary
   Wejście/Wyjście dla każdego scenariusza wg dokumentacji.
3. **T5** — plan na piątek **07.08.2026** ręcznie skrócony do 8:00–14:00 (6h)
   w Kalendarz → Norma czasu pracy.
4. Weryfikacja naliczonego planu (pkt 1.1 dokumentacji).

## Stan konfiguracji bazy Al (sprawdzone 2026-09-04)

- Strefa **„Godziny ponadwymiarowe"** (`GodzPonadWym`) — **istnieje** (pkt 3.1 OK).
- Strefy „Praca poza normą", „Nadgodziny do przeniesienia" — standardowe, są.
- „Magazyn nadgodzin rozliczany od" — **puste** (do włączenia, patrz wyżej).

Backup bazy przed importem: `C:\enovaServer\Projekty\Al_przed_pracownikami_RCP.bac`.
