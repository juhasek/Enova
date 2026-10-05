# BlokadaZmianyGodzinDAK (Dodatkowy kod do kompilacji)

Plik **Dodatkowego kodu do kompilacji** (System → Dodatkowy kod do kompilacji, tabela
`CodeFiles`) z logiką weryfikatorów „Blokada zmiany godzin przez pracownika” (plan i czas
pracy). Reguła, konfiguracja i status testów:
[Blokada zmiany godzin przez pracownika - plan.md](Blokada%20zmiany%20godzin%20przez%20pracownika%20-%20plan.md).

## Klasa

`Soneta.Runtime.Database.Business.TblCodeFiles.BlokadaZmianyGodzinDAK` (statyczna):

| Metoda | Wywoływana z definicji rodzaju |
|---|---|
| `string Weryfikuj(DzienKalendarzaAktualizacja dzien)` | `DzienPlanuAktualizacja` |
| `string Weryfikuj(DzienPracyAktualizacja dzien)` | `DzienPracyAktualizacja` |

Obie metody przekazują źródło planu pozycji i datę do wspólnej, prywatnej metody `Sprawdz`.
Zwracają `null`, gdy nie ma blokady, albo treść komunikatu błędu.

Namespace `...Business.TblCodeFiles` to ten sam wzorzec, którego używają inne pliki kodu
w repo (np. `LicznikCzasuPracy`). Z definicji woła się klasę pełną nazwą.

## Uwagi do kodu

- Komunikat składany jest przez `string.Format`, nie `TranslateFormat`. W Dodatkowym kodzie
  nie ma automatycznego `using`, który daje to rozszerzenie w edytorze definicji
  (kompilacja testowa z `TranslateFormat` nie przechodziła).
- Umowa o pracę: `zrodlo as Pracownik` (pozycja dla umowy cywilnoprawnej ma źródło `Umowa`)
  + `Pracownik.JestZatrudnionyNaEtat(FromTo.Day(data))`.
- Zalogowany pracownik: `Session.Login.WebUserOperatingInstance?.Host as Pracownik`
  (`null` poza pulpitem). Porównanie po `Guid`, bo Host pochodzi z innej sesji.
