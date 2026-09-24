# A1NaglowekListaSnippet — nagłówek z pieczątką oddziału pracownika

Wzorzec użytkownika enova365 podmieniający standardowy nagłówek
(`Soneta.Business.UI.DxReports.NaglowekListaSnippet`, nazwa logiczna
**„nagłówek - lista"**). W bazie: tabela `SystemFiles`
(`Name = 'A1NaglowekListaSnippet'`, `RuntimeInfoIdentifier =
Soneta.Business.UI.DxReports.NaglowekListaSnippet`, kolumna `Code`).

## Po co ta zmiana

Pieczątka w nagłówku ma pokazywać dane **oddziału pracownika z bieżącego rekordu
wydruku** (wg jego wydziału), a nie:

- Firmy głównej (`Config.Firma.Pieczątka`) — pusta w wielooddziałowym wdrożeniu,
  gdzie licencje są na oddziałach, **ani**
- oddziału operatora (`AktualnyOddziałWorker`) — `null`, gdy operator drukuje
  w kontekście „wszystkie".

Standardowy `ReportTools.GetHeaderData` nie potrafi wziąć oddziału z rekordu
kadrowego: `DxReportService.GetDepartmentFromDataContext` rozpoznaje tylko
`IOddziałNagłówkaDokumentuInfo` / `IDokumentKsiegowalny` / `IElementStrukturyFirmy`
/ `IOddzialProvider`, a `Pracownik` / `PracHistoria` / `Wyplata` żadnego z nich
nie implementują → `Department == null` → nagłówek spada na pieczątkę Firmy.

## Jak działa

Nagłówek „nagłówek - lista" ma `DataKind="Context"` — `Report.DataAdapter` jest
`null`, a `GetCurrentRow()` zwraca `Context` (nie rekord). Bieżący rekord bierzemy
więc z listy danych raportu-rodzica:
`DxReportHelpers.GetDataSourceList<Row>(rc.Report)` pod indeksem `e.CurrentRow`
(z `Report_DataSourceRowChanged`), z fallbackiem na `rc.CurrentDataSourceIndex` /
`rc.Report.CurrentRowIndex` / `0`.

Z rekordu → `PracHistoria`:

| Typ rekordu | Ścieżka |
|---|---|
| `Wyplata` (np. `WyplataEtat` — pasek wypłaty) | `Wyplata.PracHistoria` (= `Pracownik[data wypłaty]`) |
| `Pracownik` | `Pracownik.Last` (ostatni zapis historii) |
| `PracHistoria` | wprost |

dalej: `PracHistoria.Etat.Wydzial` → `Wydzial.WyliczOddzial()` (w górę drzewa
**wydziałów**, aż do przypiętego oddziału) → `OddzialFirmy.GetPieczątka(data)`.
`GetPieczątka` **sama schodzi po `Nadrzedny`**, gdy `NumerLicencji == ""`, więc
zwraca pieczątkę z oddziału, który faktycznie nosi licencję (pracownik → oddział
podrzędny bez licencji → oddział nadrzędny z licencją). Data pieczątki = data
wypłaty (dla `Wyplata`) lub `Date.Today`.

Nadpisywane labele: `NazwaFormatowana`, `NazwaSkrócona`, `Adres.Linia1/Linia2`,
`NIP`.

Zmiana jest **addytywna**: `Report_BeforePrint` i początek
`Report_DataSourceRowChanged` wypełniają nagłówek standardowo z `GetHeaderData`;
nadpisanie pieczątką oddziału następuje tylko, gdy uda się przejść całą ścieżkę
rekord → wydział → oddział → pieczątka. Dla rekordów niekadrowych nagłówek
zostaje standardowy.

## Historia dochodzenia (diagnostyka)

Wersja diagnostyczna logowała do `%TEMP%\naglowek-lista-diag.log`. Ustalenia:

- wydruk testowy to **pojedyncza wypłata** — `mainRows[0] = Soneta.Place.WyplataEtat`;
- `Report.DataAdapter == null`, `GetCurrentRow() == Context` (stąd wcześniejsza
  wersja brała zły obiekt i `ph` wychodziło `null`);
- `Report_DataSourceRowChanged` **jest** wołane; `e.CurrentRow` = indeks wiersza
  raportu-rodzica;
- standardowy `GetHeaderData` zwracał `CompanyName=''`, `CompanyTIN=''`, mimo że
  rozpoznał `Department = Centrala` (pieczątka Centrali też pusta).

Pułapki kompilacji edytora skryptów enova napotkane po drodze:
`Login.GetLicenceData()` wymaga `using Soneta.Business.Licence;` (brak w edytorze);
`PracHistoria` **nie ma** `.Do` (to składowa wewnętrznego workera, nie wiersza).

## Ograniczenia / do weryfikacji

- **Baza demo:** `OddzialFirmy.GetPieczątka` przy licencji demonstracyjnej
  zwraca `"* DEMO * <oddział>"` (klasa `PieczątkaDemo`, NIP `222-222-22-22`).
- **`GetPieczątka` rzuca wyjątek** („adres nie jest zgodny", „nie znaleziono
  licencji"), gdy numer licencji oddziału nadrzędnego jest wpisany, ale sama
  licencja niezgodna / niewczytana — snippet łapie i zostawia pieczątkę
  standardową; wtedy naprawić licencję oddziału nadrzędnego.
- **`PobierzBiezacyWiersz`** dla wsadu wielu wypłat opiera się na `e.CurrentRow`
  / indeksach z `ReportContext` — potwierdzić na wydruku listy płac (wiele
  pasków naraz), że każdy pasek dostaje pieczątkę swojego oddziału.
- Zależność od `Soneta.Kadry` / `Soneta.Place` — snippet skompiluje się tylko
  w konfiguracji kadrowo-płacowej.

## Jak wgrać

1. enova → Narzędzia → Opcje → Systemowe → Wydruki → Wzorce użytkownika →
   `A1NaglowekListaSnippet`.
2. W „Kod źródłowy" wklej całą zawartość `Raporty/A1NaglowekListaSnippet`.
3. Zapisz — enova skompiluje klasę i podepnie ją pod nagłówek.
4. Wydrukuj pasek wypłaty pracownika z oddziału podrzędnego i sprawdź pieczątkę.
