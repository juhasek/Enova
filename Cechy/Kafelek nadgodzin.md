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
- Odbiór pracownika/firmy i wypłata nadgodzin („przekwalifikowanie”) zmniejszają bilans – enova
  rozlicza je z zasobami, więc te same godziny nie występują jednocześnie w „do odbioru” i „do wypłaty”.
- Strefy wyjść prywatnych (nazwa zawiera „prywatn”: wyjście, rozliczenie, potrącenie) są pomijane:
  enova paruje wyjście prywatne z nadgodzinami w magazynie, a nie powinno to zmniejszać nadgodzin.
- Dzień wolny do odbioru: strefa nadgodzin w dniu wolnym/świątecznym **wg planu** (KalkPlanu).
  Część „dniówkowa” = min(godziny strefy, `Etat.NormaDobowa`) – jak w enova przy odbiorze dnia wolnego.
  Dzień jest liczony, dopóki ta część jest nieodebrana; po częściowym odbiorze dzień znika,
  a reszta przechodzi do godzin.
- Odbiór bez pokrycia w nadgodzinach („na zapas”) daje **ujemne** godziny (np. −8:00).

**NadgRazem** – suma stref niezależna od wymiaru etatu (stara wersja liczyła niepełnoetatowców
dzień po dniu i po zmianie etatu nadpisywała sumę). Godziny ponadwymiarowe mają własną strefę
i nie są wliczane.

**NadgodzinyDoWyplaty** – w bieżącym okresie rozliczeniowym: praca poza normą − rozliczenie wyjścia
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

Niesprawdzone na danych (brak przypadków w Al): częściowy odbiór dnia wolnego, wypłata
z magazynu, wyjście prywatne sparowane z nadgodzinami, odjęcie elementów z listy płac.
