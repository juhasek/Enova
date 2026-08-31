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

- **Tabela:** `CfgNodes` (cecha globalna / konfiguracji).
- **Nazwa:** `BlokadaCzasuPracyDzienMiesiaca` — stała `NazwaCechyDzienBlokady`
  na górze skryptu; zmień w obu miejscach, jeśli nazwiesz cechę inaczej.
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

## Odczyt cechy globalnej — DO POTWIERDZENIA W ŚRODOWISKU

Cecha globalna („Cechy globalne", okno `GlobalFeatures`) to w enova365 cecha
(`FeatureDefinition`) zarejestrowana dla tabeli `CfgNodes` — jej wartość leży w
tabeli `Features` przy wierszu roota konfiguracji (`Soneta.Config.CfgNode`).

Skrypt czyta ją przez:

```csharp
Configuration.GetInstance(session).Features["BlokadaCzasuPracyDzienMiesiaca"]
```

z `using Soneta.Config;`. **Tego wywołania nie udało się potwierdzić** — ani w
dokumentacji online, ani w lokalnym pakiecie `soneta-erp-skills` (opisuje on
klasę ORM `Verifier` i cechy zwykłych wierszy, nie cechy globalne ani skrypt
weryfikatora kalendarza). Jeśli nie kompiluje się w edytorze skryptów, podmień
ciało `PobierzDzienBlokady` na jeden z wariantów:

1. `session.GetBusiness().Config` → root konfiguracji (`CfgNodeProxy`), odczyt
   `...Features["..."]` na węźle
2. bezpośrednio przez tabelę `CfgNodes`: pobierz wiersz roota (`Parent == null`)
   i `root["BlokadaCzasuPracyDzienMiesiaca"]`
3. własny helper do cech globalnych, jeśli jest w środowisku (por. folder
   `RozwiązaniaA1`)

Najszybsze ustalenie: wkleić do edytora skryptów i sprawdzić błąd kompilatora,
albo przetestować na żywej bazie (`buscall call`). Po ustaleniu — zaktualizować
tę sekcję i `PobierzDzienBlokady`.

## Sygnatura i wpięcie

```csharp
public static string BlokadaMiesiacaWstecz(Pracownik pracownik, Date data)
```

Bez zmian względem poprzedniej wersji — `pracownik` służy teraz wyłącznie do
pobrania `Session` (`pracownik.Session`). Metodę wywołuje się z właściwego
weryfikatora planu/kalendarza dla `(pracownik, dzień)`; zwrócony niepusty string
to komunikat błędu, `null` = brak blokady.

## API użyte w skrypcie

- `Date.Today`, `Date.FirstDayMonth()`, `Date.AddMonths(int)`, `Date.Day/Month/Year`
- `YearMonth(Date)` + `TranslateFormat` — komunikat jak w oryginale
- `System.DateTime.DaysInMonth`, `System.Math.Min` — przycięcie progu do długości miesiąca
