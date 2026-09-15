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

## 2. Poprawka błędu sumowania godzin (16.09.2026)

Pętla sumująca godziny do porównania z normą 8h błędnie sprawdzała `Row.Definicja.Nazwa.Contains(...)`
(definicję wiersza, dla którego liczona jest cecha — stałą przez całą pętlę) zamiast
`st.Definicja.Nazwa.Contains(...)` (definicję aktualnie iterowanej strefy `st`). W efekcie do sumy
wliczały się godziny **wszystkich** stref danego dnia (np. „Lider zmiany", „Praca w normie"), nie
tylko stref „Praca poza normą" — co mogło sztucznie zawyżać `sumę` ponad normę i błędnie kwalifikować
godziny „poza normą" jako nadgodziny 50%, mimo że same w sobie nie przekraczały normy. Poprawione
przez użytkownika bezpośrednio w edytorze skryptów enova (2026-09-16), zsynchronizowane do repo.

## 3. Usunięte wykluczenie godzin „czarnej dziury"

Wcześniejsza wersja cechy zawierała blok wykluczający godziny „czarnej dziury" (po dobie
niedzielno-świątecznej a przed dobą planowanego dnia roboczego, gdy dzień poprzedni był świąteczny)
— ten sam warunek co w cesze **„Nadgodziny okresowe"** (`Cechy/Nadgodziny okresowe`), zwracający `0`
dla takich godzin, by uniknąć podwójnego naliczenia między obiema cechami. Blok ten został usunięty
razem z poprawką błędu sumowania (patrz wyżej) — potwierdzone przez użytkownika (2026-09-16), że
aktualny stan w bazie `Claude` (bez tego bloku) jest obowiązujący.
