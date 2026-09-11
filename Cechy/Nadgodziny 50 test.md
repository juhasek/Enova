# Nadgodziny 50 (test) – dokumentacja biznesowa

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy cecha

Cecha wyliczana (typu Kwota/liczba godzin, `decimal`), przypisana do wiersza strefy pracy
(`Row` = `StrefaPracy`). Dla wierszy, których definicja zawiera w nazwie „Praca poza normą",
wylicza liczbę godzin, które mają zostać rozliczone jako **nadgodziny 50%** — zarówno w dni
robocze (ponad dobową normę 8h), jak i w dni wolne od pracy nieobjęte nadgodzinami
niedzielno-świątecznymi (ponad 8h w danym dniu wolnym).

Wersja robocza/testowa (`_test` w nazwie właściwości) — do finalnej weryfikacji przed wdrożeniem
produkcyjnym.

## 2. Wykluczenie godzin „czarnej dziury" (01.09.2026)

Cecha rozpoznaje tzw. „czarne dziury", czyli godziny przypadające po dobie niedzielno-świątecznej
(od godz. 6:00 w niedzielę/święto) a przed dobą planowanego dnia roboczego, gdy dzień poprzedni był
świąteczny (`TypDnia.Świąteczny`), a bieżący dzień ma niezerowy plan pracy — ten sam warunek co w
cesze **„Nadgodziny okresowe"** (`Cechy/Nadgodziny okresowe`).

**Godziny „czarnej dziury" to nadgodziny okresowe, a nie nadgodziny 50%** — rozlicza je cecha
„Nadgodziny okresowe". Dlatego ta cecha, wykrywszy że wiersz mieści się w „czarnej dziurze" (w
całości lub części — niezależnie od wariantu nakładania się strefy z granicą doby/planu), zwraca dla
niego `0`, zamiast liczyć te godziny jako 50%. Zapobiega to podwójnemu naliczeniu tych samych godzin
przez obie cechy jednocześnie.
