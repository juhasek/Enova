# Cecha SumaDodatkow – sesja 2026-10-30

## Cel
Cecha na pracowniku (`Get_Feature_SumaDodatkow`, decimal) ma sumować dodatki do etatu obowiązujące
w okresie z filtra listy pracowników oraz wynagrodzenie zasadnicze z umowy o pracę.

## Co zrobiono
- `Cechy/SumaDodatkow` – kod cechy do wklejenia w edytor:
  - okres: `cx.Get(out OkresContext filtr) ? filtr.Okres : dziś`;
  - zasadnicze: `Row.Historia[koniec okresu].Etat.Zaszeregowanie.Stawka`, gdy `Etat.Okres` nachodzi na okres;
  - dodatki: `DodHistoria` z `Element.Dodatkowe.DodatekDoEtatu`, gdy `dh.Okres` nachodzi na okres.

## Ustalenia
- Okres obowiązywania dodatku to `DodHistoria.Okres`, a nie `Aktualnosc` (aktualność pierwszego zapisu
  historii jest otwarta od początku – dodatek od 01.11 sumował się w październiku).

## Wyniki testów
- Użytkownik zgłosił błąd z `Aktualnosc` (dodatek od 01.11 liczony w październiku) – poprawione na `Okres`.

## Otwarte punkty
- Dane czytane na ostatni dzień okresu – dodatek/umowa zakończone w trakcie okresu nie są liczone.
- Stawka zaszeregowania sumowana bez sprawdzania rodzaju (godzinowa/miesięczna).
