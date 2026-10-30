using System.Text.Json;
using DevExpress.Spreadsheet;

var katalog = args.Length > 0 ? args[0] : "..";
var scenariusze = JsonDocument.Parse(File.ReadAllText(Path.Combine(katalog, "Generator", "scenariusze.json"))).RootElement;

using var skoroszyt = new Workbook();
var arkusz = skoroszyt.Worksheets[0];
arkusz.Name = "Scenariusze";

string[] naglowki = { "ID", "Grupa", "Scenariusz", "Metoda", "Oczekiwany wynik", "Podstawa", "Wynik (OK/BŁĄD)", "Co zwrócił serwer", "Uwagi" };
double[] szerokosci = { 8, 18, 34, 18, 58, 13, 14, 40, 30 };
for (var k = 0; k < naglowki.Length; k++)
{
    arkusz.Cells[0, k].Value = naglowki[k];
    arkusz.Cells[0, k].Font.Bold = true;
    arkusz.Cells[0, k].Fill.BackgroundColor = System.Drawing.Color.FromArgb(221, 230, 240);
    arkusz.Columns[k].ColumnWidthInCharacters = szerokosci[k];
}

var w = 1;
foreach (var s in scenariusze.EnumerateArray())
{
    string[] wartosci = { "Id", "Grupa", "Nazwa", "Metoda", "Oczekiwane", "Podstawa" };
    for (var k = 0; k < wartosci.Length; k++)
    {
        arkusz.Cells[w, k].Value = s.GetProperty(wartosci[k]).GetString();
        arkusz.Cells[w, k].Alignment.WrapText = true;
        arkusz.Cells[w, k].Alignment.Vertical = SpreadsheetVerticalAlignment.Top;
    }
    w++;
}

arkusz.FreezeRows(0);
arkusz.AutoFilter.Apply(arkusz.Range.FromLTRB(0, 0, naglowki.Length - 1, w - 1));
skoroszyt.SaveDocument(Path.Combine(katalog, "Scenariusze testowe.xlsx"), DocumentFormat.Xlsx);
Console.WriteLine($"OK: {w - 1} scenariuszy");
