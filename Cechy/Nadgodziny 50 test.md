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

## 2. Obsługa „czarnych dziur" (01.09.2026)

Cecha obsługuje tzw. „czarne dziury", czyli godziny przypadające po dobie niedzielno-świątecznej
(od godz. 6:00 w niedzielę/święto) a przed dobą planowanego dnia roboczego, gdy dzień poprzedni był
świąteczny (`TypDnia.Świąteczny`), a bieżący dzień ma niezerowy plan pracy. Logika przeniesiona
analogicznie z cechy **„Nadgodziny okresowe"** (`Cechy/Nadgodziny okresowe`):

- gdy cały czas strefy mieści się w „czarnej dziurze" (między dobą niedzielną a początkiem pracy z
  planu) — zwracany jest cały czas strefy;
- gdy strefa zaczyna się przed dobą niedzielną, a kończy w „czarnej dziurze" — zwracana jest tylko
  część od doby niedzielnej;
- gdy strefa zaczyna się w „czarnej dziurze", a kończy po starcie planu — zwracana jest tylko część
  do początku planu;
- gdy strefa obejmuje całą „czarną dziurę" (zaczyna się przed dobą niedzielną i kończy po starcie
  planu) — zwracana jest cała długość „czarnej dziury".

Dopasowanie zwraca wynik od razu (`return`), z pominięciem standardowego liczenia nadgodzin 50% dla
danego wiersza — jest to niezależny od strefy przypadek, tak samo jak w cesze „Nadgodziny
okresowe".
