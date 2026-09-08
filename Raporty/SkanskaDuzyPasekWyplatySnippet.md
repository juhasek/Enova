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

## Do potwierdzenia z użytkownikiem

Przy uruchomieniu z kartoteki pracownika wydruk obejmuje **wszystkie** wypłaty
etatowe pracownika z całej historii (grupowane po okresie → jeden pasek na
okres). Jeśli ma to być węższy zakres (np. bieżący rok, ostatnia wypłata,
parametr „okres od–do") — trzeba dołożyć filtr w gałęzi `Pracownik/PracHistoria`.
Zaznaczenie konkretnych wierszy w gridzie *Wypłaty* na formularzu nie jest
przekazywane do wydruku, gdy enova podaje jako CurrentList rekord nadrzędny
(`PracHistoria`).

## Jak wgrać

1. enova → Narzędzia → Opcje → Systemowe → Wydruki → Wzorce użytkownika →
   `SkanskaDuzyPasekWyplatySnippet` (albo import XML do `SystemFiles`).
2. W „Kod źródłowy" wklej całą zawartość `Raporty/SkanskaDuzyPasekWyplatySnippet`.
3. Zapisz — enova skompiluje klasę.
4. Sprawdź wydruk z `Płace/Wypłaty` **oraz** z kartoteki pracownika.

Wersja programu z raportu błędu: **2604.4.4** (release 8.09.2026).
