# Szablon wydruku – tabela z 6 kolumnami

Generyczny, pusty szablon wzorca wydruku Enova (plik `.repx`, format DevExpress
XtraReports) z tabelą o 6 równych kolumnach. Punkt startowy do dalszej edycji
w projektancie wydruków Enova, nie gotowy raport biznesowy.

## Co zawiera

- **PageHeaderBand** — wiersz nagłówka z 6 komórkami: „Kolumna 1” … „Kolumna 6”
  (tekst statyczny, pogrubiony).
- **DetailBand** — wiersz danych z 6 komórkami, każda związana wyrażeniem
  (`ExpressionBindings`) z polem źródła danych: `[Kolumna1]` … `[Kolumna6]`.
- Wszystkie kolumny mają `Weight="1"`, więc dzielą szerokość strony po równo.

## Format pliku

Struktura pliku (wersja serializatora, jednostki `TenthsOfAMillimeter` +
`Dpi="254"`, zwięzłe `ControlType` bez pełnej nazwy assembly dla
standardowych kontrolek DevExpress, atrybuty `Ref="N"` na każdym elemencie,
blok `ComponentStorage` z komponentem `BusinessContext`) została odtworzona
na podstawie realnego, działającego wzorca wyeksportowanego z projektanta
wydruków tej instalacji Enova (`DevExpress.XtraReports.v24.1`,
`SerializerVersion="24.1.5.0"`). Jeśli w innej instalacji Enova wersja
DevExpress jest inna, trzeba odpowiednio poprawić `SerializerVersion` i
`ControlType` na `XtraReportBase`/root (np. przez wyeksportowanie dowolnego
działającego wydruku z tamtej instalacji i porównanie nagłówka).

## Jak uruchomić dane

Plik zawiera tylko `ComponentStorage/BusinessContext` (żeby dziedziczyć
standardowe style) — nie ma podpiętego `BusinessSource` ani
`ReportSnippetComponent`, więc wyrażenia `[Kolumna1]`…`[Kolumna6]` w
`DetailBand` nie mają skąd wziąć wartości i w podglądzie wydrukują się jako
puste komórki. To celowo generyczny szablon layoutu (6 kolumn), a nie
gotowy raport z danymi.

Żeby podpiąć realne dane, trzeba — analogicznie jak w przykładzie
`PracownicyListaPelna` — dodać do `ComponentStorage`:

- `Soneta.Business.UI.DxReports.BusinessDataSource` (`Name="BusinessSource"`,
  `DataKind="CurrentList"`, `DesignDataTypeName="<TypObiektu>"`) wskazujący
  listę obiektów biznesowych, z której ma korzystać raport, albo
- `Soneta.Business.UI.DxReports.ReportSnippetComponent`
  (`SnippetTypeName="Namespace.KlasaSnippetu,Assembly"`) wskazujący klasę
  `ReportSnippet` (jak `Raporty/OdbiciaRCP`), która sama ustawia
  `DxReportHelpers.GetDataSourceEmpty(this).CustomDataSource` na listę
  obiektów z właściwościami `Kolumna1..Kolumna6` — to prostszy wariant dla
  danych, które nie odpowiadają wprost jednej klasie biznesowej Enova.
