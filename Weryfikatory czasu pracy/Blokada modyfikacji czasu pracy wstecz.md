# Blokada modyfikacji czasu pracy wstecz

Metoda pomocnicza weryfikatora planu/czasu pracy. Blokuje zapis czasu pracy
w dniach należących do „zamkniętych" miesięcy. Zastępuje wcześniejszą wersję
`BlokadaMiesiacaWstecz`, w której data graniczna była zawsze 1. dniem bieżącego
miesiąca.

Cała logika jest w jednej metodzie — bez metod pomocniczych, bez typów
`Nullable`, bez `AddMonths`/`FirstDayMonth` (edytor skryptów enova ich nie
łyka: `int?` daje ostrzeżenie „int nigdy nie równa się null", a złe wnioskowanie
typu psuło `data < blokadaOd`).

## Zmiana względem poprzedniej wersji

| | Poprzednio | Teraz |
|---|---|---|
| Źródło progu | na sztywno: 1. dzień bieżącego miesiąca | cecha globalna z **dniem miesiąca** domknięcia |
| Wyłączenie blokady | niemożliwe | pusta cecha = brak blokady |
| Okno na domknięcie poprzedniego miesiąca | brak (poprzedni miesiąc zamknięty od 1.) | do dnia N bieżącego miesiąca poprzedni miesiąc jest jeszcze edytowalny |

## Cecha globalna

- **Gdzie:** okno „Cechy globalne".
- **Nazwa:** `BlokadaOdDnia` (w kodzie jako literał w `Features["BlokadaOdDnia"]`).
- **Typ:** liczba całkowita.
- **Wartość:** dzień miesiąca **1–28** (zalecane; 29–31 działa, ale w krótszych
  miesiącach próg jest przycinany do ostatniego dnia miesiąca).
- **Pusta / brak / < 1:** blokada całkowicie wyłączona (metoda zwraca `null`).

## Logika progu

Dla wpisanej wartości `N` i dzisiejszej daty:

- jeżeli **dzień dzisiejszy ≥ N** → `blokadaOd` = 1. dzień **bieżącego** miesiąca;
  zablokowane są wszystkie dni z miesiąca poprzedniego i wcześniejszych,
- jeżeli **dzień dzisiejszy < N** → `blokadaOd` = 1. dzień **poprzedniego** miesiąca;
  trwa jeszcze okno na domknięcie poprzedniego miesiąca, zablokowane są dopiero
  dni sprzed dwóch miesięcy i wcześniej.

Błąd zwracany, gdy `data < blokadaOd`. Dzień równy `blokadaOd` jest dozwolony
(semantyka `<`, jak w oryginale).

Komunikat zawiera **numer miesiąca i rok** granicy w formacie `MM.rrrr`
(np. `08.2026`) — składany z lokalnych `miesiac`/`rok`, bez `YearMonth`.
Aby pokazać nazwę miesiąca („sierpień 2026"), w skrypcie jest zakomentowana
tablica nazw — wystarczy ją odkomentować i podstawić `nazwy[miesiac - 1]`.

### Przykład (cecha = 5)

Piątego dnia miesiąca blokują się poprzednie miesiące. **5 września →
sierpień i wcześniejsze miesiące zablokowane.**

| Dziś | Próg aktywny? | `blokadaOd` | Można edytować |
|---|---|---|---|
| 2026-09-04 | nie (4 < 5) | 2026-08-01 | sierpień 2026 i nowsze |
| 2026-09-05 | tak (5 ≥ 5) | 2026-09-01 | wrzesień 2026 i nowsze (sierpień zablokowany) |
| 2026-09-20 | tak | 2026-09-01 | wrzesień 2026 i nowsze |

## Odczyt cechy globalnej

```csharp
object wartoscCechy = pracownik.Session.Global.Features["BlokadaOdDnia"];
if (wartoscCechy == null) return null;   // pusta cecha -> brak blokady
int dzienBlokady = (int)wartoscCechy;
```

`Session.Global` niesie cechy globalne; `.Features["nazwa"]` zwraca `object`
(`null`, gdy cecha bez wartości) — dlatego sprawdzamy `null` przed rzutem `(int)`.

## Sygnatura i wpięcie

```csharp
public static string BlokadaMiesiacaWstecz(Pracownik pracownik, Date data)
```

Sygnatura bez zmian — `pracownik` służy do pobrania `Session`
(`pracownik.Session`). Metodę wywołuje się z weryfikatora planu/kalendarza dla
`(pracownik, dzień)`; niepusty string = komunikat błędu, `null` = brak blokady.

## API użyte w skrypcie

- `pracownik.Session.Global.Features["nazwa"]` — odczyt cechy globalnej, rzut `(int)` po null-checku
- `Date.Today`, `Date.Day/Month/Year`, `new Date(rok, miesiąc, 1)`
- poprzedni miesiąc liczony ręcznie (`miesiac-1`, przejście przez 0 → grudzień/rok-1) — bez `AddMonths`
- `miesiac.ToString("00") + "." + rok` + `TranslateFormat` — komunikat `MM.rrrr` (np. `08.2026`)
- `System.DateTime.DaysInMonth` — przycięcie progu do długości miesiąca
