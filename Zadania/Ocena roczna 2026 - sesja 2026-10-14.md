# Ocena roczna 2026 (samoocena) — sesja 2026-10-14

Sesja poza artefaktami enova365: przygotowanie propozycji samooceny pracowniczej na 2026 r.
na podstawie arkusza z 2025 r. i prac widocznych w repo. Treść ocen i komentarzy (dane osobowe)
pozostaje wyłącznie w `Pobrane/` — tutaj tylko przebieg i stan.

## 1. Wejście (`Pobrane/`, poza gitem)

- `Oceny 2025_TK.xlsx` — samoocena 2025 (bez ocen przełożonego w pliku).
- `Oceny 2026 pracownicy konsultanci, PM.xlsx` — pusty arkusz 2026 (wypełnione tylko kryt. 1).
- Kryteria w obu latach identyczne: 8 kryteriów w skali 1–6 (4 = oczekiwana) + sekcje opisowe
  IV–VII (wyniki, głos pracownika, plan rozwojowy, komentarze).

## 2. Wynik

`Pobrane/Oceny 2026 pracownicy konsultanci, PM - propozycja.xlsx` — kopia arkusza 2026
z wpisaną samooceną (kol. G), komentarzami (kol. H) i sekcjami IV–VI. Oryginał bez zmian.

- Średnia ok. 4,9 wobec 4,6 w 2025; wzrost w kryt. 4 (wiedza — nowe obszary) i 6 (dokumentacja
  i transfer wiedzy), czyli w punktach wskazanych rok temu jako słabsze.
- Przykłady oparte na pracach z repo (sierpień–październik 2026), bez nazw klientów.
- Cel rozwojowy z 2025 („nowy moduł”) oznaczony jako zrealizowany.

## 3. Technika

Brak Pythona i Excela na stacji: xlsx rozpakowany, `xl/worksheets/sheet2.xml` uzupełniony
skryptem node (komórki `inlineStr`, styl istniejącej komórki komentarza, ręczna wysokość
wierszy), wpis podmieniony w kopii przez `System.IO.Compression.ZipArchive` (tryb Update).
Weryfikacja: poprawność XML + ponowny odczyt komórek; w Excelu nie oglądane.

## 4. Do zrobienia (po stronie użytkownika)

1. Uzupełnić miejsca w nawiasach kwadratowych: godziny/mies. (kryt. 3), szkolenie innych
   (kryt. 6), decyzja 4/5 w kryt. 7, konkretny przypadek (kryt. 8).
2. Dopisać przykłady z I półrocza 2026 (repo ich nie obejmuje).
3. Otworzyć w Excelu i poprawić wysokości wierszy przed wysłaniem do przełożonej.
