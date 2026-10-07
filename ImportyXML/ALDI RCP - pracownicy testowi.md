# ALDI RCP — pracownicy testowi (podział nadgodzin, zlecenia 277381 / 277382)

Plik: `ALDI RCP - pracownicy testowi.xml` — import **według rekordów** do bazy `Al`.

```
dbmgr importxml Al "ALDI RCP - pracownicy testowi.xml" --standard
```

Guidy stałe (`a1d10000-…`) → import jest **idempotentny** (ponowne uruchomienie nie
duplikuje danych).

## Co tworzy

14 pracowników — **1 pracownik = 1 scenariusz** z dokumentacji
`Dokumentacja_scenariusze_testowe.html`
(przepisane scenariusze: [Zadania/ALDI Import RCP - scenariusze testowe.xlsx](../Zadania/ALDI%20Import%20RCP%20-%20scenariusze%20testowe.xlsx),
dokumentacja: [Zadania/ALDI Import RCP - podział nadgodzin.md](../Zadania/ALDI%20Import%20RCP%20-%20podzia%C5%82%20nadgodzin.md)). **Kod pracownika = numer scenariusza.**

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
dokładnie wg kart w `Dokumentacja_scenariusze_testowe.html`
(przepisane scenariusze: [Zadania/ALDI Import RCP - scenariusze testowe.xlsx](../Zadania/ALDI%20Import%20RCP%20-%20scenariusze%20testowe.xlsx),
dokumentacja: [Zadania/ALDI Import RCP - podział nadgodzin.md](../Zadania/ALDI%20Import%20RCP%20-%20podzia%C5%82%20nadgodzin.md)). **Tabela nie jest
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

## Scenariusz DOBA1 — odbicia przed zmianą po dniu wolnym (dodane 2026-10-07)

Odtworzenie przypadku klienta: pracownik przyszedł rano w dniu, w którym plan zaczyna się
po południu, a poprzedni dzień jest wolny. Import RCP przenosi takie odbicia na poprzednią dobę.

- Pracownik **DOBA1** (`TEST-DOBA1 Doba`, guid `a1d10000-…-000000000015`), etat 1/1 od 2026-01-01,
  kalendarz Standard + własny kalendarz pracownika z wyjątkami planu:
  26.01 12:00–22:15, 27–28.01 14:15–22:15, **29.01 Wolny**, **30.01 14:00–20:00**, 31.01 14:15–22:30.
  Plik: `ALDI RCP - scenariusz DOBA1 - pracownik i plan.xml` (idempotentny).
- Odbicia: **30.01.2026 Wejście 5:45, Wyjście 11:45**, stan Nieoznaczony.
  Plik: `ALDI RCP - scenariusz DOBA1 - odbicia.xml` (jednorazowy, tabela nieguidowana).
- Oczekiwany wynik przy konfiguracji RCP → Ogólne jak u klienta (Początek doby = Zawsze wg
  kalendarza, tolerancja doby minus 2:00, plus 3:00): po „Importuj dane z RCP” czas pracy na
  **29.01 jako +5:45..+11:45**, na 30.01 brak pracy (log: „przesunięcie początku na dzień 29.01”).

Backup bazy przed importem: `C:\enovaServer\Projekty\Al_przed_DOBA1.bac`.

### Wynik testu DOBA1 (2026-10-07)

| Tolerancja doby minus | Wynik importu |
|---|---|
| 2:00 (jak u klienta) | DP **29.01** +5:45..+11:45 → podział ALDI: Praca poza normą 6:00 + Nadgodziny do przeniesienia 6:00 (wpis w magazynie nadgodzin); 30.01 pusty |
| **10:00** | DP **30.01** 5:45–11:45; 29.01 pusty — **zgodnie z oczekiwaniem** |

Reguła importu (Początek doby = Zawsze wg kalendarza): wejście przechodzi na dzień poprzedni,
gdy jest wcześniej niż *start planu dnia − tolerancja minus*; wyjście — gdy wcześniej niż
*start + tolerancja plus* i nie poprzedza go odbicie przypisane do tego samego dnia.

Słabe strony tolerancji 10:00 (do przetestowania):
1. Dzień ze startem planu ≤ 10:00 — wejście po północy (powrót z przerwy przy pracy przeciągniętej
   po północy) zostaje na nowej dobie, para rozbija się między dni.
2. Ponowny import tych samych odbić na inny dzień (po zmianie planu/tolerancji, import zaznaczonych)
   → praca na dwóch dniach; duplikaty i „Nadpisz dane” działają tylko na dniu docelowym.
3. Doba ustalana wg planu z chwili importu — późniejsza zmiana planu nie przelicza zapisów.
4. Samotne wyjście (brak wejścia) i typ „Niezdefiniowany” (import traktuje jak wyjście) przed
   start + 3:00 → dzień poprzedni.
5. Import okresami z „Nadpisz dane”: wyjście po północy z pierwszego dnia nowego okresu kasuje
   wcześniejsze zapisy ostatniego dnia poprzedniego okresu.
6. Dzień wolny bierze start z ostatniego dnia z planem (do 14 dni wstecz) — po rannej zmianie
   wyjście przed ok. 9:00 w dniu wolnym idzie na dzień wcześniej.
7. Algorytm weryfikacji ALDI (okno −2 h / +17 h od startu planu) nie korzysta z tolerancji —
   może uznać dzień za kompletny, a import rozbije parę.
