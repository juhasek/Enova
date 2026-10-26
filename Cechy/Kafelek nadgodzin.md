# Kafelek nadgodzin – cechy pracownika (baza Al)

Cechy kategorii **Kafelek** na tabeli `Pracownicy` (FeatureDefs 21–26) zasilające kafelki nadgodzin
na pulpicie. Wspólna logika: Dodatkowy kod kompilacji **A1KafelkiNadgodzin** (CodeFiles 2016,
plik `Kafelek nadgodzin - kod wspolny`, klasa `Soneta.Runtime.Database.Business.TblCodeFiles.KafelkiNadgodzin`).
Cechy to jednolinijkowe wywołania tej klasy.

| Cecha | Typ | Co pokazuje |
|---|---|---|
| WyjsciePrywatne | Time | „Wyjście prywatne” − „Rozliczenie wyjścia prywatnego” w bieżącym okresie rozliczeniowym |
| NadgRazem | Time | „Praca poza normą” − „Rozliczenie wyjścia prywatnego” w bieżącym roku |
| DniDoOdbioru | int | dni wolne do odbioru za pracę w dniu wolnym (magazyn nadgodzin) |
| NadgodzinyDoOdbioru | Time | godziny do odbioru z magazynu nadgodzin (bez części ujętej w dniach) |
| NadgodzinyDoWyplaty | Time | nadgodziny okresu do wypłaty, jeszcze nienaliczone na liście płac |
| OkresRozliczeniowy | FromTo | okres rozliczeniowy nadgodzin na dziś (bez zmian) |

## Zasady liczenia

**Magazyn (DniDoOdbioru, NadgodzinyDoOdbioru)** – liczone z zasobów (`ZasobRN`, nierozliczona reszta
strefy) i obrotów (`ObrotRN`, rozliczone pary) magazynu nadgodzin pracownika, **bez ograniczenia
do okresu** – nieodebrane godziny przechodzą na kolejne okresy, a odbiór zaplanowany na przyszłą datę
(wnioski z terminem odbioru) od razu zmniejsza bilans.
- **Niezależnie od parowania zasobów.** enova paruje odbiór/wypłatę z nadgodzinami dopiero czynnością
  „Rozlicz nadgodziny” (lub dokumentem rozliczenia czasu pracy) – wpisana strefa odbioru zwykle wisi
  jako osobny zasób „minus”. Rozliczenie sparowane (obrót) zdejmuje swoją strefę; niesparowane – od
  najstarszych: odbiór co najmniej dniówki zdejmuje najpierw najstarszy pasujący dzień wolny (odbiór dnia
  za święto/sobotę), mniejszy – najpierw godziny; gdy godzin brak, dzień jest rozbijany, reszta idzie do godzin.
- Odbiór pracownika/firmy i wypłata nadgodzin („przekwalifikowanie”) zmniejszają bilans,
  więc te same godziny nie występują jednocześnie w „do odbioru” i „do wypłaty”.
- Strefy wyjść prywatnych (nazwa zawiera „prywatn”: wyjście, rozliczenie, potrącenie) są pomijane:
  enova paruje wyjście prywatne z nadgodzinami w magazynie, a nie powinno to zmniejszać nadgodzin.
- Dzień wolny do odbioru: strefa nadgodzin w dniu wolnym/świątecznym **wg planu** (KalkPlanu).
  Część „dniówkowa” = min(godziny strefy, norma dobowa etatu **z wymiarem** – `Etat.WymNormaDobowa`,
  ½ etatu = 4 h). Gdy w kalendarzu „Wartość dobowa” jest niewypełniona (etat zwraca 0) – 8 h,
  inaczej cały dzień wolny szedł do godzin (objaw u klienta przy 2–3.05.2025, odtworzony w Al).
  Dzień jest liczony, dopóki ta część jest nieodebrana; po częściowym odbiorze dzień znika,
  a reszta przechodzi do godzin.
- Odbiór bez pokrycia w nadgodzinach („na zapas”) daje **ujemne** godziny (np. −8:00).

**NadgRazem** – praca poza normą minus odpracowanie wyjścia prywatnego **z tego samego dnia**
(samo odpracowanie bez pracy poza normą nie zmniejsza nadgodzin), w roku kalendarzowym
`Date.Today` – nadgodziny z następnego roku nie są wliczane. Suma niezależna od wymiaru etatu (stara wersja liczyła niepełnoetatowców
dzień po dniu i po zmianie etatu nadpisywała sumę). Godziny ponadwymiarowe mają własną strefę
i nie są wliczane.

**NadgodzinyDoWyplaty** – w bieżącym **miesiącu** (decyzja 26.10: nadgodziny tylko w swoim miesiącu, bez
miesięcy wcześniejszych i przyszłych): praca poza normą − rozliczenie wyjścia
prywatnego − strefy odłożone do magazynu („W kolejnych miesiącach”) + strefy „Wypłata nadgodzin”
− czas elementów wypłat Nadgodziny I/II/Św z okresem w tym okresie. Wynik nie mniejszy niż 0.

Brak strefy o podanej nazwie = 0 (wcześniej `WgNazwy[...] == null` liczyło wszystkie strefy dnia).

## Test na bazie Al (26.10.2026)

| Pracownik | Dane | Dni | Godz. do odbioru | NadgRazem |
|---|---|---|---|---|
| S2 | 2 h w magazynie, w pełni odebrane 10.08 | 0 | 0:00 | 2:00 |
| S4 | sobota 4 h do magazynu | 1 | 0:00 | 4:00 |
| S5 | dzień roboczy 6 h do magazynu | 0 | 6:00 | 6:00 |
| T4 (niepełny) | 1 h do magazynu + 2 h ponadwymiarowe | 0 | 1:00 | 1:00 |
| T6 (niepełny) | sobota 3 h | 1 | 0:00 | 3:00 |
| 026 | odbiór 8 h bez nadgodzin | 0 | −8:00 | 0:00 |

### Test stref wpisanych na kalendarz (bez wniosków), 26.10.2026

Strefy wpisane `import_business_xml` (logika biznesowa – zakłada zasoby magazynu, nie paruje ich).
Wynik: dni / godz. do odbioru / NadgRazem / do wypłaty / WP.

| # | Pracownik | Przypadek | Wynik |
|---|---|---|---|
| K01 | S7 | sobota 7.11, 6 h do magazynu → odbiór 6 h 16.11 | 1/0/6 → **0/0:00** |
| K02 | S1 | sobota 14.11, 10 h → odbiór 8 h 23.11 | 1/2:00/10 → **0/2:00** |
| K03 | T1 | niedziela 15.11, 4 h (niepełny) → odbiór 2 h 17.11 | 1/0:00/4 → **0/2:00** |
| K04 | T2 | święto 11.11, 8 h → odbiór 8 h 2.12 | 1/0:00/8 → **0/0:00** |
| K05 | T3 | sobota 5.12 5 h + niedziela 10.01.2027 3 h | 2/0:00/**5:00** (2027 poza rokiem) |
| K06 | S2 | WP 1 h 27.10 → odpracowanie 28.10 (PPN 1 h + rozl. WP 1 h) | WP 1:00 → 0:00, NadgRazem bez zmian |
| K07 | S5 | WP 1 h przy 6 h w magazynie → odpracowanie samą strefą rozl. WP | do odbioru 6:00, NadgRazem 6:00 bez zmian |
| K08 | S7 | 27.10: 2 h poza normą bez magazynu | do wypłaty 2:00 |
| K09 | S6 | 30.10: wypłata 5 h z magazynu | do odbioru 0:00, do wypłaty 5:00 |
| K10 | S1 | 28.10: praca w normie 9 h | bez zmian |

Pierwsza wersja (parowanie zasobów) dawała przy odbiorach dzień nadal widoczny i ujemne godziny,
a samo odpracowanie WP zmniejszało NadgRazem – poprawione, wszystkie przypadki OK.
Niesprawdzone: odjęcie elementów z listy płac w „do wypłaty”.

### Runda 3 (uwagi klientki: święto > 8 h, ½ etatu, wolny za święto), 26.10.2026

| # | Pracownik | Przypadek | Wynik |
|---|---|---|---|
| K12 | S5 (6 h otwarte z sierpnia) | święto 11.11 10 h → odbiór 8 h 12.11 | **0 dni / 8:00** (FIFO dawało 1 dzień / 0:00 – objaw klientki) |
| K13 | T1 (¾, norma 6 h) | niedziela 22.11 7 h | dzień + 1 h (dniówka 6 h z wymiaru) |
| K14 | T3 (¾) | niedziela 20.12 9 h | +1 dzień, +3:00, NadgRazem +9 (14:00) |
| K15 | DW2 / NP7 | 14.08 „Wolny za święto” + 15.08 święto, 8+9 h / 7+8 h | 2 dni / 1:00 |
| K16 | NP4, kalendarz z „Wartością dobową” 0 (na czas testu) | 14.08 8 h + 15.08 9 h | stary kod 0 / 17:00, nowy **2 / 1:00** |
| K17 | S7 | 3 h do wypłaty we wrześniu | „do wypłaty” = tylko październik (2:00) |

Przykład 2 klientki (9 h → +10, 10 h → +12 w bilansie wszystkich) i „do wypłaty” bez odjęcia listy płac
to zachowanie **starego** kodu cech – u klientki nowa wersja (CodeFiles A1KafelkiNadgodzin) nie była wgrana.

Kompilacja w Al: po SQL-update kodu serwer nie przekompilował sam – zadziałała czynność
**„Wymuś rekompilację”** na formularzu wpisu „Dodatkowy kod do kompilacji” (Ustawienia/Systemowe).
