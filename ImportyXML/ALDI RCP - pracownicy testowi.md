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

## Dane z RCP

Plik: `ALDI RCP - dane RCP.xml` — import **według rekordów**, klasa
`Soneta.Kalend.WejscieWyjscieI` (tabela `WejsciaWyjsciaI`, czyli lista „Dane z RCP").

```
dbmgr importxml Al "ALDI RCP - dane RCP.xml" --standard
```

32 zdarzenia (Wejście/Wyjście, Stan=Aktywny) — po jednym komplecie na scenariusz,
dokładnie wg kart w `Dokumentacja_scenariusze_testowe.html`. **Tabela nie jest
guidowana → import jest jednorazowy** (bez `guid`/`where`; ponowne uruchomienie
dopisze kolejne wiersze zamiast je zaktualizować — przy reimporcie najpierw
skasować wcześniej wstawione wiersze).

Scenariusze wieloetapowe mają wpisaną tylko **pierwszą partię** zdarzeń — kolejne
kroki robi tester ręcznie w GUI:

- **S5** (doimport w trakcie dnia) — wpisane tylko 8:00→17:00 (krok 1). Dopisanie
  18:00→22:00 i ponowny import (krok 2) — ręcznie.
- **S7** (dzień z rozliczonymi nadgodzinami) — wpisane tylko 8:00→18:00 (jak S2,
  warunek wstępny). Ręczna rozliczenie nadgodzin na 10.08 + dopisanie zdarzeń
  19:00→20:00 i ponowny import — ręcznie.
- **S6** (ponowny import z nadpisaniem) dostał od razu **komplet** 4 zdarzeń
  czwartku (8:00/17:00/18:00/22:00) — to jego warunek wstępny, nie kolejny krok.

## Poza zakresem tych plików — robi tester ręcznie w GUI

1. **Magazyn nadgodzin rozliczany od** — Narzędzia → Opcje → Kadry i płace →
   Kalendarze → Czas pracy; miesiąc ≤ sierpień 2026. W bazie `Al` było **puste**.
   Wymagane przed pierwszym importem RCP (scenariusz S7).
2. Samo uruchomienie czynności **„Importuj dane z RCP (ALDI)"** dla każdego
   scenariusza (i doimporty/nadpisania/rozliczenia opisane wyżej).
3. **T5** — plan na piątek **07.08.2026** ręcznie skrócony do 8:00–14:00 (6h)
   w Kalendarz → Norma czasu pracy.
4. Weryfikacja naliczonego planu (pkt 1.1 dokumentacji).

## Stan konfiguracji bazy Al (sprawdzone 2026-09-04)

- Strefa **„Godziny ponadwymiarowe"** (`GodzPonadWym`) — **istnieje** (pkt 3.1 OK).
- Strefy „Praca poza normą", „Nadgodziny do przeniesienia" — standardowe, są.
- „Magazyn nadgodzin rozliczany od" — **puste** (do włączenia, patrz wyżej).

Backup bazy przed importem: `C:\enovaServer\Projekty\Al_przed_pracownikami_RCP.bac`.
