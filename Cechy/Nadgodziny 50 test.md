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

## 2. Ograniczenie: brak obsługi „czarnych dziur"

Cecha **nie obsługuje** tzw. „czarnych dziur", czyli godzin przypadających po dobie
niedzielno-świątecznej w niedzielę/święto, a przed dobą poniedziałkową (przejście doby na granicy
dnia świątecznego i kolejnego dnia roboczego).

Kod obsługujący ten przypadek istnieje już w cesze **„Nadgodziny okresowe"**
(`Cechy/Nadgodziny okresowe`, sekcja z komentarzem „obsługa tzn «czarnych dziur»...") i w razie
potrzeby powinien zostać analogicznie przeniesiony/dostosowany do tej cechy.
