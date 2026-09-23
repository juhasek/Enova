# ALDI – import danych z RCP z podziałem na strefy nadgodzin (zlecenia 277381 / 277382)

Czynność **„Importuj dane z RCP (ALDI)”** z dodatku `AltOne.Aldi.Ext` (enova365 2512.9.11).
Kod dodatku **nie jest przechowywany w tym repo** – tutaj jest dokumentacja biznesowa,
scenariusze testowe i dane testowe do importu.

- **Zlecenie 277381** – pełne etaty: podział dnia na pracę w normie / poza normą /
  nadgodziny do przeniesienia.
- **Zlecenie 277382** – niepełne etaty: dodatkowa strefa **„Godziny ponadwymiarowe”**
  (między planem a 8 h), nadgodziny dopiero powyżej 8 h.
- Zweryfikowano na żywo: **Nie** (brak potwierdzonych wyników testów akceptacyjnych
  w repo – wpisywać w arkuszu scenariuszy, kolumny „Wynik testu / Uwagi”).

## Po co

Standardowy import z RCP nanosi na kalendarz wyłącznie czas pracy – nie rozróżnia, co
mieści się w planie dnia, a co jest ponad plan. Nowa czynność robi to samo co standardowa,
a dodatkowo **po imporcie** porównuje czas przepracowany z planem dnia i gdy pracownik
pracował dłużej, dzieli dzień na strefy.

## Reguła podziału

### Pełny etat (277381) – wymiar etatu = 1

| Strefa | Wymiar | Godziny od–do | Wpływ na czas dnia |
|---|---|---|---|
| **Praca w normie** | do wysokości planu dnia | tak (pierwsze godziny pracy) | wlicza się |
| **Praca poza normą** | nadwyżka ponad plan | tak (godziny po wyczerpaniu planu) | wlicza się |
| **Nadgodziny do przeniesienia** | = nadwyżka (ten sam wymiar) | **brak** – zapisywany tylko czas | **nie** powiększa czasu dnia; zasila magazyn / odbiór nadgodzin |

Zasady szczegółowe:

- Cięcie następuje **dokładnie po wyczerpaniu planu** liczonego czasem pracy, a nie godziną
  zegarową z planu – przy pracy w kilku odcinkach (przerwa) granica przesuwa się
  (np. 8:00–12:00 + 12:30–18:30 przy planie 8 h → w normie do 16:30).
- Dzień bez przekroczenia planu (praca = plan lub krócej) **nie jest dzielony** – jedna
  strefa „Praca w normie”.
- Dzień wolny (plan = 0) – całość jako „Praca poza normą” + „Nadgodziny do przeniesienia”
  (**założenie do potwierdzenia przez ALDI**).
- **Doimport** w ciągu dnia (kolejne odbicia do już zaimportowanego dnia, Nadpisz dane = Nie)
  przelicza dzień całościowo – poranne godziny nie mogą zostać policzone dwa razy.
  „Nadgodziny do przeniesienia” pozostają **jednym wierszem** z sumą nadwyżki.
- Dzień, którego strefy są powiązane z **rozliczeniem nadgodzin** (odbiór), jest przy
  ponownym imporcie **pomijany** – strefy zostają bez zmian, w logu:
  „Pominięto … strefy dnia powiązane z rozliczeniem nadgodzin”.
- Log czynności zawiera wpisy „Podział stref: …”.

### Niepełny etat (277382) – wymiar etatu < 1

| Czas przepracowany | Strefy |
|---|---|
| ≤ plan | Praca w normie (bez podziału) |
| plan < czas ≤ 8:00 | Praca w normie (= plan) + **Godziny ponadwymiarowe** (czas − plan) |
| czas > 8:00 | Praca w normie (= plan) + Godziny ponadwymiarowe (8:00 − plan) + Praca poza normą (czas − 8:00) + Nadgodziny do przeniesienia (czas − 8:00) |

- Próg **8:00 jest stały** – nie uwzględnia obniżonej normy dobowej (np. 7 h dla osób
  niepełnosprawnych) – **do potwierdzenia przez ALDI**.
- Reguła dotyczy wyłącznie **wymiaru etatu < 1**. Pełny etat z krótszym planem dnia
  (np. ręcznie skrócony dzień 6 h) liczony jest jak w 277381 – bez godzin
  ponadwymiarowych.
- Dzień wolny niepełnoetatowca – jak dla pełnego etatu (całość poza normą + do przeniesienia),
  bez godzin ponadwymiarowych.
- Przykład ze zlecenia: plan 6 h, praca 9 h → 6:00 w normie + 2:00 ponadwymiarowe +
  **1:00** nadgodziny (a nie 3:00).
- **Włącznik mechanizmu:** reguła działa tylko, gdy w konfiguracji istnieje definicja strefy
  o nazwie dokładnie **„Godziny ponadwymiarowe”**. Bez niej wszyscy pracownicy (także
  niepełnoetatowi) liczeni są wg reguły 277381.

## Wymagana konfiguracja

1. **Magazyn nadgodzin** – `Narzędzia → Opcje → Kadry i płace → Kalendarze → Czas pracy`,
   pole „Magazyn nadgodzin rozliczany od” = miesiąc nie późniejszy niż pierwszy importowany.
   Musi być ustawiony **przed** importem – strefy „Nadgodziny do przeniesienia” zaimportowane
   przy wyłączonym magazynie nie zasilają go i nie da się ich odebrać. Jeśli magazyn włączono
   później – ponowny import z „Nadpisz dane = Tak”.
2. **Definicja strefy „Godziny ponadwymiarowe”** (tylko 277382) –
   `Narzędzia → Opcje → Kadry i płace → Kalendarze → Definicje stref → Nowy`:

   | Pole | Wartość |
   |---|---|
   | Nazwa | **Godziny ponadwymiarowe** (dokładnie tak – po nazwie szuka jej dodatek) |
   | Wpływ na czas pracy | Zwiększa |
   | Uwzględniaj w czasie faktycznie przepracowanym | **Tak** (inaczej enova nie pozwala zapisać godzin od–do i nie wlicza strefy do czasu dnia) |
   | Rozliczenie czasu pracy | W bieżącym miesiącu (godziny ponadwymiarowe nie trafiają do magazynu nadgodzin) |
   | Współczynnik | 100% (wyszarzone) |
   | Pozostałe (Przestój, Praca zdalna, Blokada, Podstawa nadgodzin, Indywidualne rozliczenie, Proponowany czas) | domyślne |

   Strefa nie jest standardowa – nazwa pochodzi z opisu zlecenia ALDI.
3. **Naliczony plan dnia** pracownika (`Kalendarz → Norma czasu pracy`) – bez planu cały
   czas pracy wyjdzie jako „poza normą”.

## Uruchomienie

Lista `Kadry i płace → Kadry → Czas pracy → Dane z RCP` → przycisk
**„Importuj dane z RCP (ALDI)”** (pasek narzędzi lub menu *Czynności*). Parametry:

| Parametr | Znaczenie |
|---|---|
| Tylko zaznaczone zapisy | Tak = import tylko zaznaczonych wierszy listy |
| Pracownik | puste = wszyscy |
| Za okres | zakres dat zdarzeń |
| Stan | Aktywny |
| Nadpisz dane | Tak = dzień budowany od nowa z odbić; Nie = doimport |

Wynik: kartoteka pracownika → `Kalendarz → Czas pracy` → dzień → sekcja
**Strefy czasu pracy** (Definicja / Od godziny / Czas) oraz czas dnia. Dni naniesione
importem mają na kalendarzu oznaczenie „DP: czas”.

## Scenariusze testowe

Pełna lista z kolumnami „Wynik testu / Uwagi z testu” →
[ALDI Import RCP - scenariusze testowe.xlsx](ALDI%20Import%20RCP%20-%20scenariusze%20testowe.xlsx).

W stosunku do pierwotnej wersji dokumentacji dla klienta
(`Dokumentacja_scenariusze_testowe.html`, 2026-08-19) scenariusze zostały przepisane pod
dane testowe z `ImportyXML/`: **1 pracownik = 1 scenariusz, kod pracownika = numer
scenariusza** (zamiast wspólnych RCP1/RCP2/RCP3). Dzięki temu scenariusze nie zależą od
siebie – S6 nie wymaga wcześniejszego S5, S7 nie wymaga S2, T7 nie wymaga ponownego
importu dnia z S2.

Tydzień testowy: **pn 03.08.2026 – sob 08.08.2026**.

| Lp | Pracownik | Dzień | Odbicia | Oczekiwane strefy (w normie / ponadwym. / poza normą / do przen.) | Czas dnia |
|---|---|---|---|---|---|
| S1 | S1 (1/1) | pn 03.08 | 8:00–16:00 | 8:00 / – / – / – | 8:00 |
| S2 | S2 (1/1) | wt 04.08 | 8:00–18:00 | 8:00 / – / 2:00 / 2:00 | 10:00 |
| S3 | S3 (1/1) | śr 05.08 | 8:00–12:00, 12:30–18:30 | 4:00+4:00 (do 16:30) / – / 2:00 / 2:00 | 10:00 |
| S4 | S4 (1/1) | sob 08.08 | 8:00–12:00 | – / – / 4:00 / 4:00 | 4:00 |
| S5 **kluczowy** | S5 (1/1) | czw 06.08 | 8:00–17:00, potem doimport 18:00–22:00 | 8:00 / – / 1:00+4:00 / 5:00 | 13:00 |
| S6 | S6 (1/1) | czw 06.08 | 8:00–17:00, 18:00–22:00; import, potem nadpisanie | 8:00 / – / 1:00+4:00 / 5:00 (bez zmian) | 13:00 |
| S7 | S7 (1/1) | wt 04.08 | 8:00–18:00, odbiór nadgodzin, doimport 19:00–20:00 | bez zmian po doimporcie | 10:00 |
| T1 | T1 (3/4) | pn 03.08 | 8:00–13:30 | 5:30 / – / – / – | 5:30 |
| T2 | T2 (3/4) | wt 04.08 | 8:00–15:30 | 6:00 / 1:30 / – / – | 7:30 |
| T3 | T3 (3/4) | śr 05.08 | 8:00–16:00 | 6:00 / 2:00 / – / – | 8:00 |
| T4 **przykład ze zlecenia** | T4 (3/4) | czw 06.08 | 8:00–17:00 | 6:00 / 2:00 / 1:00 / 1:00 | 9:00 |
| T5 | T5 (1/1, dzień skrócony do 6 h) | pt 07.08 | 8:00–15:00 | 6:00 / – / 1:00 / 1:00 | 7:00 |
| T6 | T6 (3/4) | sob 08.08 | 9:00–12:00 | – / – / 3:00 / 3:00 | 3:00 |
| T7 | T7 (1/1) | wt 04.08 | 8:00–18:00 (po włączeniu 277382) | 8:00 / – / 2:00 / 2:00 | 10:00 |

## Dane testowe

- [ImportyXML/ALDI RCP - pracownicy testowi.xml](../ImportyXML/ALDI%20RCP%20-%20pracownicy%20testowi.xml)
  – 14 pracowników S1–S7, T1–T7 (guidy `a1d10000-…`, import idempotentny),
  opis: [ALDI RCP - pracownicy testowi.md](../ImportyXML/ALDI%20RCP%20-%20pracownicy%20testowi.md).
- [ImportyXML/ALDI RCP - dane RCP.xml](../ImportyXML/ALDI%20RCP%20-%20dane%20RCP.xml)
  – 32 zdarzenia „Dane z RCP” (pierwsza partia każdego scenariusza; import **jednorazowy** –
  tabela nie jest guidowana).

Ręcznie w GUI (poza plikami importu): magazyn nadgodzin, skrócenie planu T5 na 07.08,
kolejne kroki S5 i S7, samo uruchamianie czynności importu.

## Do potwierdzenia przez ALDI

- Praca w dzień wolny: całość jako „poza normą” + „do przeniesienia” (S4, T6).
- Niepełny etat powyżej 8 h: nadgodziny liczone od 8 h (T4 – jedna nadgodzina), a nie od planu.
- Próg 8:00 jest stały (nie uwzględnia obniżonej normy dobowej, np. 7 h).

## Poza zakresem

Naliczenia płacowe i odbiory nadgodzin (poza blokadą dnia rozliczonego – S7); dni
z nieobecnością całodzienną i jednoczesnymi odbiciami RCP – w testach pomijać.
