# A1NaglowekListaSnippet — nagłówek listy z pieczątką oddziału pracownika

Wzorzec użytkownika enova365 podmieniający standardowy nagłówek list
(`Soneta.Business.UI.DxReports.NaglowekListaSnippet`, nazwa logiczna
**„nagłówek - lista"**). W bazie trzymany w tabeli `SystemFiles`
(`Name = 'A1NaglowekListaSnippet'`, `RuntimeInfoIdentifier =
Soneta.Business.UI.DxReports.NaglowekListaSnippet`).

## Po co ta zmiana

Na listach kadrowych pieczątka w nagłówku ma pokazywać dane **oddziału
pracownika z bieżącego wiersza wydruku** (wg jego wydziału), a nie:

- Firmy głównej (`Config.Firma.Pieczątka`) — która w wielooddziałowym wdrożeniu
  bywa pusta, bo licencje są na oddziałach, **ani**
- oddziału operatora (`AktualnyOddziałWorker`) — który jest `null`, gdy operator
  drukuje w kontekście „wszystkie".

Standardowy `ReportTools.GetHeaderData` **nie potrafi** wziąć oddziału z wiersza
pracownika: `DxReportService.GetDepartmentFromDataContext` rozpoznaje tylko
`IOddziałNagłówkaDokumentuInfo` / `IDokumentKsiegowalny` / `IElementStrukturyFirmy`
/ `IOddzialProvider`, a `Pracownik` ani `PracHistoria` żadnego z nich nie
implementują → `Department == null` → nagłówek spada na pieczątkę Firmy.

## Jak działa

`Report_DataSourceRowChanged` (odpala się raz na pracownika):

1. bieżący rekord: `Report.DataAdapter` (fallback `GetCurrentRow()`),
   mapowany na `PracHistoria` — `Pracownik` → `.Last`, `PracHistoria` → wprost;
2. wydział: `PracHistoria.Etat.Wydzial`;
3. oddział: `Wydzial.WyliczOddzial()` — idzie w górę drzewa **wydziałów**, aż
   trafi na wydział z przypiętym oddziałem;
4. pieczątka: `OddzialFirmy.GetPieczątka(Date.Today)` — metoda **sama schodzi po
   `Nadrzedny`**, gdy `NumerLicencji == ""`, więc zwraca pieczątkę z oddziału,
   który faktycznie nosi licencję (scenariusz: pracownik → oddział podrzędny bez
   licencji → oddział nadrzędny z licencją);
5. nadpisanie labeli: `NazwaFormatowana`, `NazwaSkrócona`, `Adres.Linia1/Linia2`,
   `NIP`.

Zmiana jest **addytywna**. `Report_BeforePrint` (raz, pod strażą `headerData`) i
początek `DataSourceRowChanged` wypełniają nagłówek wartościami standardowymi z
`GetHeaderData`; nadpisanie pieczątką oddziału następuje tylko, gdy uda się
przejść całą ścieżkę rekord → wydział → oddział → pieczątka. Dla list
niekadrowych (rekord nie jest `Pracownik`/`PracHistoria`) nagłówek zostaje
standardowy.

## Ograniczenia / do weryfikacji

- **Baza demo:** `OddzialFirmy.GetPieczątka` przy licencji demonstracyjnej
  **zawsze** zwraca `"* DEMO * <nazwa oddziału>"` (klasa `PieczątkaDemo`, NIP
  `222-222-22-22`). Na bazie `Claude` (piaskownica, licencja demo) nagłówek
  pokaże więc `* DEMO * <oddział pracownika>` — to potwierdza tylko, że ścieżka
  rozwiązywania oddziału działa. **Realny test wymaga bazy z licencją klienta.**
- **`GetPieczątka` rzuca wyjątek** (np. „adres nie jest zgodny", „nie znaleziono
  licencji"), gdy numer licencji oddziału nadrzędnego jest wpisany, ale sama
  licencja jest niezgodna albo niewczytana. Snippet łapie wyjątek i zostawia
  pieczątkę standardową — jeśli nagłówek dalej jest „nie ten", trzeba naprawić
  licencję oddziału nadrzędnego (`dbmgr licence` / wczytanie w Opcjach).
- **Data pieczątki** = `Date.Today`. Dla wydruku „na datę" (pracownicy zwolnieni,
  historyczne dane oddziału) podmień na `ph.Do` w wywołaniu `GetPieczątka`.
- **`Report.DataAdapter`** — wzorowane na standardowym
  `Soneta.KadryPlace.Reports.NaglowekPracodawcaSnippet`, który tak właśnie czyta
  bieżącego pracownika w nagłówku. Zweryfikować, że na docelowych listach
  (`Report.DataAdapter`) niesie `Pracownik`/`PracHistoria`.
- Zależność od `Soneta.Kadry` (`Pracownik`, `Wydzial`, `Etat`) — snippet
  skompiluje się tylko w konfiguracji z modułem kadrowo-płacowym.

## Jak wgrać

1. enova → Narzędzia → Opcje → Systemowe → Wydruki → Wzorce użytkownika →
   `A1NaglowekListaSnippet` (nagłówek „nagłówek - lista").
2. W „Kod źródłowy" wklej całą zawartość `Raporty/A1NaglowekListaSnippet`.
3. Zapisz — enova skompiluje klasę i podepnie ją pod nagłówek list.
4. Wydrukuj dowolną listę kadrową dla pracownika z oddziału podrzędnego i
   sprawdź pieczątkę.
