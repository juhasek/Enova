# SkanskaDuzyPasekWyplatySnippet — „Pasek wypłaty duży" (Skanska / AltOne)

Wzorzec użytkownika (ReportSnippet) podmieniający DataSource wydruku „duży pasek
wypłaty" na listę zagregowanych DTO `SkanskaPasekSource` (jeden pasek =
`Pracownik` + `Okres` + typ wypłaty, z sumowaniem wielu wypłat w okresie).

Klasa: `Soneta.KadryPlace.Reports.SkanskaDuzyPasekWyplatySnippet`.
W bazie: tabela `SystemFiles` (`FileType = DxSnippet`,
`RuntimeInfoIdentifier = Soneta.KadryPlace.Reports.SkanskaDuzyPasekWyplatySnippet`).
Task pierwotny 237978 (autor Benedykt Kluś), task modyfikacji 276264 (sumowanie).

## Naprawiony błąd — uruchomienie z kartoteki pracownika

**Objaw:** wydruk z `Płace / Wypłaty` działał, ale z kartoteki pracownika
(zakładka *Wypłaty* / toolbar formularza) leciał wyjątek:

```
Unable to cast object of type 'Soneta.Kadry.PracHistoria' to type 'Soneta.Place.Wyplata'.
  at ...CastIterator... at ...ToList...
  at SkanskaDuzyPasekWyplatySnippet.DuzyPasekWyplaty_BeforePrint
```

**Przyczyna:** handler pobierał listę bieżącą przez
`DxReportHelpers.GetDataSourceList<Wyplata>(this)`, a ta metoda robi
`GetDataSource(report, BusinessDataKind.CurrentList).Cast<Wyplata>()`. Zawartość
*CurrentList* zależy od miejsca uruchomienia:

| Uruchomienie | Typ wierszy CurrentList |
|---|---|
| `Płace / Wypłaty` | `Wyplata` / `WyplataEtat` |
| kartoteka pracownika | `PracHistoria` (lub `Pracownik`) |
| lista list płac | `ListaPlac` |

Z kartoteki `Cast<Wyplata>()` na `PracHistoria` rzucał `InvalidCastException`
(standardowy `PaskiWyplatySnippet` ma ten sam problem — po prostu nie jest
oferowany z tego kontekstu).

**Poprawka** (`DuzyPasekWyplaty_BeforePrint`): CurrentList pobierana jako
`object` i normalizowana do płaskiej listy `WyplataEtat`:

- `WyplataEtat` → bierzemy wprost (dedup po `Guid`);
- inna `Wyplata` (nie-etat) → wyjątek „tylko wypłaty etatowe";
- `ListaPlac` → wszystkie jej `WyplataEtat`;
- `Pracownik` / `PracHistoria` → wszystkie wypłaty etatowe pracownika przez
  `PlaceModule.GetInstance(context).Wyplaty.WgPracownik[prac]`.

Dalej bez zmian: grupowanie `(Pracownik.Guid, ListaPlac.Okres, typ)` + przełącznik
`SrParams.SumujWyplaty`.

## Skąd bierze się typ wierszy CurrentList (i jak dostać zaznaczone wypłaty)

`GetDataSourceList` czyta `BusinessDataSource` o `DataKind="CurrentList"`, a ten
w runtime dostaje `CalculateCurrentListHandler = () => Printer.DataSource`
(`Soneta.Business.UI.DxReports`, `DxReportPrinterTarget.InitializeReport`).
`Printer.DataSource` = lista/zaznaczenie, z którego **fizycznie** odpalono wydruk.
`.repx` nie wymusza typu (`BusinessSource` bez `DesignDataTypeName`).

| Skąd odpalasz | Printer.DataSource |
|---|---|
| `Płace / Wypłaty` (zaznaczone) | `WyplataEtat` — zaznaczone ✅ |
| kartoteka → zakładka **Wypłaty** (pasek narzędzi tej listy) | `WyplataEtat` — zaznaczone ✅ |
| kartoteka → wydruk z ramki formularza / lista `Pracownicy` | `PracHistoria` / `Pracownik` ❌ (brak zaznaczenia wypłat) |

### Zaznaczone wypłaty z kartoteki pracownika

Z zakładki *Płace/Wypłaty* na kartotece enova podaje w CurrentList **pracownika**
(`PracHistoria`), a nie zaznaczone wypłaty. Kod próbuje odzyskać zaznaczenie z
`INavigatorContext.SelectedRows` (`[Context]`, niesie `SelectedRows` +
`FocusedRow` + `RowType`), filtrując do `WyplataEtat` danego pracownika:

- w gałęzi `Pracownik`/`PracHistoria`: **jeśli** nawigator zwróci zaznaczone
  `WyplataEtat` → drukujemy tylko je;
- **inaczej** fallback → wszystkie wypłaty etatowe pracownika
  (`PlaceModule.Wyplaty.WgPracownik[prac]`).

**Potwierdzone na żywej bazie (2026-09-08):** `INavigatorContext` z pod-listy
*Wypłaty* na kartotece **niesie zaznaczenie wypłat** — wydruk poprawnie obejmuje
tylko zaznaczone zapisy zarówno z `Płace/Wypłaty`, jak i z kartoteki.

## Grupowanie per miesiąc

Klucz grupowania to `(Pracownik.Guid, rep.Data.Month, typ wypłaty)` — wypłaty z
tego samego miesiąca schodzą się na jeden zbiorczy pasek. W nagłówku
„Wypłata za okres" idzie `rep.ListaPlac.Okres.ToYearMonth()` (**metoda**, z
nawiasami — `FromTo.ToYearMonth()`), a linia „Data wypłaty" jest zakomentowana,
bo dla paska zbiorczego pojedyncza data byłaby myląca.

Uwaga: `Data.Month` to sam numer miesiąca, bez roku — luty 2025 i luty 2026
wpadłyby na jeden pasek. Jeśli kiedyś zacznie przeszkadzać, dołożyć
`&& rep.Data.Year == w.Data.Year`.

## Jak wgrać

1. enova → Narzędzia → Opcje → Systemowe → Wydruki → Wzorce użytkownika →
   `SkanskaDuzyPasekWyplatySnippet` (albo import XML do `SystemFiles`).
2. W „Kod źródłowy" wklej całą zawartość `Raporty/SkanskaDuzyPasekWyplatySnippet`.
3. Zapisz — enova skompiluje klasę.
4. Sprawdź wydruk z `Płace/Wypłaty` **oraz** z kartoteki pracownika.

Wersja programu z raportu błędu: **2604.4.4** (release 8.09.2026).
