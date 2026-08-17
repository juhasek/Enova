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

## Jak uruchomić dane

Sam plik `.repx` to tylko layout — bez powiązanego `ReportSnippet` (jak w
`OdbiciaRCP`) nie ma skąd wziąć wartości `Kolumna1..Kolumna6`. Aby wydruk
faktycznie coś pokazał, trzeba dodać snippet ustawiający źródło danych, np.:

```csharp
public class Wiersz
{
    public string Kolumna1 { get; set; }
    public string Kolumna2 { get; set; }
    public string Kolumna3 { get; set; }
    public string Kolumna4 { get; set; }
    public string Kolumna5 { get; set; }
    public string Kolumna6 { get; set; }
}
```

i przypisanie `List<Wiersz>` do `DxReportHelpers.GetDataSourceEmpty(this).CustomDataSource`
analogicznie jak w `Raporty/OdbiciaRCP`.

## Ważne zastrzeżenie

Plik został przygotowany ręcznie wg znanej struktury formatu DevExpress
`XtraReportsLayoutSerializer` (wersja `SerializerVersion="19.1.5.0"` w
nagłówku) — nie był wygenerowany ani zweryfikowany przez faktyczny
projektant wydruków Enova/DevExpress. Przy pierwszym otwarciu w projektancie:

- jeśli wersja DevExpress w Twojej instalacji Enova różni się od `19.1.5.0`,
  może być konieczna zamiana numeru wersji (i ewentualnie `PublicKeyToken`,
  choć ten jest stały dla DevExpress) w całym pliku przez znajdź-i-zamień,
  lub po prostu zapisanie pliku ponownie z projektanta — starsze wersje
  layoutu zwykle są automatycznie migrowane do nowszych;
- warto od razu przejrzeć układ w podglądzie wydruku i w razie potrzeby
  poprawić szerokości/wysokości komórek.
