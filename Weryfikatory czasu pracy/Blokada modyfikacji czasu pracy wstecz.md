# Blokada modyfikacji czasu pracy wstecz

Metoda pomocnicza weryfikatora planu/czasu pracy. Blokuje zapis czasu pracy
w dniach należących do „zamkniętych" miesięcy. Zastępuje wcześniejszą wersję
`BlokadaMiesiacaWstecz`, w której data graniczna była zawsze 1. dniem bieżącego
miesiąca (`Date.Today.FirstDayMonth()`).

## Zmiana względem poprzedniej wersji

| | Poprzednio | Teraz |
|---|---|---|
| Źródło progu | na sztywno: 1. dzień bieżącego miesiąca | cecha globalna z **dniem miesiąca** domknięcia |
| Wyłączenie blokady | niemożliwe | pusta cecha = brak blokady |
| Okno na domknięcie poprzedniego miesiąca | brak (poprzedni miesiąc zamknięty od 1.) | do dnia N bieżącego miesiąca poprzedni miesiąc jest jeszcze edytowalny |

## Cecha globalna

- **Gdzie:** okno „Cechy globalne" (tabela `CfgNodes`).
- **Nazwa:** `BlokadaOdDnia` — stała `NazwaCechyBlokadaOdDnia` na górze skryptu;
  zmień w obu miejscach, jeśli nazwiesz cechę inaczej.
- **Typ:** liczba całkowita.
- **Wartość:** dzień miesiąca **1–28** (zalecane; 29–31 działa, ale w krótszych
  miesiącach próg jest przycinany do ostatniego dnia miesiąca).
- **Pusta / brak / poza 1–31:** blokada całkowicie wyłączona (metoda zwraca `null`).

## Logika progu

Dla wpisanej wartości `N` i dzisiejszej daty:

- jeżeli **dzień dzisiejszy ≥ N** → `blokadaOd` = 1. dzień **bieżącego** miesiąca;
  zablokowane są wszystkie dni z miesiąca poprzedniego i wcześniejszych,
- jeżeli **dzień dzisiejszy < N** → `blokadaOd` = 1. dzień **poprzedniego** miesiąca;
  trwa jeszcze okno na domknięcie poprzedniego miesiąca, zablokowane są dopiero
  dni sprzed dwóch miesięcy i wcześniej.

Błąd zwracany, gdy `data < blokadaOd`. Dzień równy `blokadaOd` jest dozwolony
(semantyka `<`, jak w oryginale).

### Przykład (cecha = 4)

| Dziś | Próg aktywny? | `blokadaOd` | Można edytować |
|---|---|---|---|
| 2026-08-02 | nie (2 < 4) | 2026-07-01 | lipiec 2026 i nowsze |
| 2026-08-04 | tak (4 ≥ 4) | 2026-08-01 | sierpień 2026 i nowsze |
| 2026-08-20 | tak | 2026-08-01 | sierpień 2026 i nowsze |

## Odczyt cechy globalnej

```csharp
object wartosc = session.Global.Features["BlokadaOdDnia"];
```

`Session.Global` niesie cechy globalne; `.Features["nazwa"]` zwraca `object`
(`null`, gdy cecha bez wartości). Wartość jest liczbą całkowitą:

```csharp
int dzien = System.Convert.ToInt32(wartosc);   // lub (int)wartosc, gdy pewne że boxed int
```

`PobierzDzienBlokady` sprawdza `null` przed rzutowaniem (pusta cecha → brak
blokady) i odrzuca wartości spoza 1..31.

## Sygnatura i wpięcie

```csharp
public static string BlokadaMiesiacaWstecz(Pracownik pracownik, Date data)
```

Bez zmian względem poprzedniej wersji — `pracownik` służy teraz wyłącznie do
pobrania `Session` (`pracownik.Session`). Metodę wywołuje się z właściwego
weryfikatora planu/kalendarza dla `(pracownik, dzień)`; zwrócony niepusty string
to komunikat błędu, `null` = brak blokady.

## API użyte w skrypcie

- `session.Global.Features["nazwa"]` — odczyt cechy globalnej
- `Date.Today`, `Date.FirstDayMonth()`, `Date.AddMonths(int)`, `Date.Day/Month/Year`
- `YearMonth(Date)` + `TranslateFormat` — komunikat jak w oryginale
- `System.DateTime.DaysInMonth`, `System.Math.Min` — przycięcie progu do długości miesiąca
