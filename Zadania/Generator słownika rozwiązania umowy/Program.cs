// Buduje Excel ze słownikami rozwiązania umowy (kody zwolnienia, podstawy prawne, przyczyny).
// Argumenty: <katalog z enumy i przyczyny.tsv> <plik .xlsx> [wersja enovy]. Uruchamiać przez Odswiez.ps1.
using System.Text.RegularExpressions;
using DevExpress.Spreadsheet;

var dir = args[0];
var outPath = args[1];
var wersja = args.Length > 2 ? args[2] : "?";

List<(int Wart, string Kod, string Opis)> Enum(string file)
{
    var txt = File.ReadAllText(file);
    var list = new List<(int, string, string)>();
    foreach (Match m in Regex.Matches(txt, @"\[Caption\(""(.*?)""\)\]\s*_\w+\s*=\s*(\d+)"))
    {
        var cap = m.Groups[1].Value;
        var p = Regex.Match(cap, @"^(\S+)\s*[-–]\s*(.*)$");
        list.Add((int.Parse(m.Groups[2].Value), p.Groups[1].Value, p.Groups[2].Value));
    }
    return list;
}

var kody = Enum(Directory.GetFiles(Path.Combine(dir, "enumy", "KodZwolnienia"), "*.cs")[0]);
var podst = Enum(Directory.GetFiles(Path.Combine(dir, "enumy", "KodPodstawyPrawnejZwolnienia"), "*.cs")[0]);
var podstDict = podst.ToDictionary(x => x.Wart);
var kodyDict = kody.ToDictionary(x => x.Wart);
string[] inicj = { "Nie dotyczy", "Pracownik", "Pracodawca" };

var wb = new Workbook();

void Naglowek(Worksheet ws, params (string Tytul, double Szer)[] kol)
{
    for (int i = 0; i < kol.Length; i++)
    {
        ws.Cells[0, i].Value = kol[i].Tytul;
        ws.Cells[0, i].Font.Bold = true;
        ws.Columns[i].WidthInCharacters = kol[i].Szer;
        ws.Columns[i].Alignment.WrapText = true;
        ws.Columns[i].Alignment.Vertical = SpreadsheetVerticalAlignment.Top;
    }
    ws.FreezePanes(0, 0);
}

var ws1 = wb.Worksheets[0];
ws1.Name = "Kody zwolnienia";
Naglowek(ws1, ("Kod", 8), ("Opis", 100), ("Wartość w bazie", 16));
for (int i = 0; i < kody.Count; i++)
{
    ws1.Cells[i + 1, 0].Value = kody[i].Kod;
    ws1.Cells[i + 1, 1].Value = kody[i].Opis;
    ws1.Cells[i + 1, 2].Value = kody[i].Wart;
}
ws1.AutoFilter.Apply(ws1.Range.FromLTRB(0, 0, 2, kody.Count));

var ws2 = wb.Worksheets.Add("Podstawy prawne");
Naglowek(ws2, ("Kod", 8), ("Podstawa prawna", 120));
for (int i = 0; i < podst.Count; i++)
{
    ws2.Cells[i + 1, 0].Value = podst[i].Kod;
    ws2.Cells[i + 1, 1].Value = podst[i].Opis;
}
ws2.AutoFilter.Apply(ws2.Range.FromLTRB(0, 0, 1, podst.Count));

var ws3 = wb.Worksheets.Add("Przyczyny rozwiązania");
Naglowek(ws3, ("Nazwa przyczyny", 60), ("Typ", 14), ("Podstawa prawna - kod", 12), ("Podstawa prawna", 70),
    ("Kod zwolnienia", 10), ("Inicjatywa", 14), ("Za odszkodowaniem", 12), ("Zablokowana", 12));
var wiersze = File.ReadAllLines(Path.Combine(dir, "przyczyny.tsv"));
for (int i = 0; i < wiersze.Length; i++)
{
    var c = wiersze[i].Split('\t');
    int r = i + 1;
    ws3.Cells[r, 0].Value = c[1];
    ws3.Cells[r, 1].Value = c[4] == "1" ? "Wygaśnięcie" : "Rozwiązanie";
    int pp = int.Parse(c[5]);
    if (podstDict.TryGetValue(pp, out var p)) { ws3.Cells[r, 2].Value = p.Kod; ws3.Cells[r, 3].Value = p.Opis; }
    int kz = int.Parse(c[6]);
    if (kodyDict.TryGetValue(kz, out var k)) ws3.Cells[r, 4].Value = k.Kod;
    ws3.Cells[r, 5].Value = inicj[int.Parse(c[7])];
    ws3.Cells[r, 6].Value = c[3] == "True" ? "Tak" : "Nie";
    ws3.Cells[r, 7].Value = c[2] == "True" ? "Tak" : "Nie";
}
ws3.AutoFilter.Apply(ws3.Range.FromLTRB(0, 0, 7, wiersze.Length));

var ws4 = wb.Worksheets.Add("Info");
ws4.Columns[0].WidthInCharacters = 110;
ws4.Columns[0].Alignment.WrapText = true;
string[] info = {
    $"Słowniki rozwiązania umowy o pracę w enova365 (wersja {wersja})",
    "",
    "Kody zwolnienia (20R-53W) i kody podstawy prawnej (400-463, 550) są wbudowane w program (typy wyliczeniowe KodZwolnienia i KodPodstawyPrawnejZwolnienia w Soneta.KadryPlace.dll) - nie ma ich jako listy w słownikach programu.",
    "Wybiera się je na pracowniku: Etat -> Rozwiązanie umowy (pola: Kod zwolnienia, Podstawa prawna, Inicjatywa).",
    "W bazie zapisywane są jako liczby (PracHistorie: EtatRozwiazanieUmowyKodZwolnienia, EtatRozwiazanieUmowyPodstawaPrawna) - kolumna „Wartość w bazie” w arkuszu Kody zwolnienia; dla podstaw prawnych wartość = kod.",
    "",
    "Arkusz „Przyczyny rozwiązania” to standardowy słownik przyczyn (Narzędzia -> Opcje / Słowniki kadrowe) z podpowiadaną podstawą prawną. Puste pole = przyczyna nie ma przypisanej podstawy / kodu.",
};
for (int i = 0; i < info.Length; i++) ws4.Cells[i, 0].Value = info[i];
ws4.Cells[0, 0].Font.Bold = true;

wb.Worksheets.ActiveWorksheet = ws1;
wb.SaveDocument(outPath, DocumentFormat.Xlsx);
Console.WriteLine($"OK: kody {kody.Count}, podstawy {podst.Count}, przyczyny {wiersze.Length}");
