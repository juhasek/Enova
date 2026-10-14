using System.Drawing;
using System.Text;
using DevExpress.Spreadsheet;

// Generator agend warsztatów analizy przedwdrożeniowej Kadry / Płace / Czas pracy.
// Treść każdego wariantu jest w osobnym pliku (AgendaKorekta.cs, AgendaAutorska.cs) –
// to źródło prawdy; generator zapisuje z niej .xlsx (do pracy na spotkaniu) i .md.

internal record Dzien(string Nr, string Data, string Godziny, string Tytul, string Cel);

internal record Blok(string Dzien, string Godz, string Temat, string Zrodlo,
    string Pokaz, string Pytania, string Odn, string Ryzyko);

internal record Uwaga(string Nr, string Obszar, string Co, string Propozycja);

internal record Material(string Termin, string Co, string PoCo);

internal record Pokrycie(string Pozycja, string TwojaAgenda, string MojaAgenda);

internal record Persona(string Nr, string Przypadek, string CoSprawdza, string Dni);

internal record Agenda(
    string Plik, string Tytul, string Legenda, string KolZrodlo, string KolRyzyko,
    Dictionary<string, Color> Kolory, Dzien[] Dni, Blok[] Bloki,
    string ArkuszUwag, string[] KolUwag, Uwaga[] Uwagi,
    string[] KolPokrycia, Pokrycie[] Pokrycie,
    Material[] Materialy, Persona[] Persony)
{
    public const string Przerwa = "Przerwa";

    public static readonly Color Naglowek = Color.FromArgb(0x1F, 0x4E, 0x79);
    public static readonly Color DzienTlo = Color.FromArgb(0xDD, 0xEB, 0xF7);
    public static readonly Color PrzerwaTlo = Color.FromArgb(0xF2, 0xF2, 0xF2);
    public static readonly Color UmowaTlo = Color.FromArgb(0xFC, 0xE4, 0xD6);
    public static readonly Color DodaneTlo = Color.FromArgb(0xE2, 0xEF, 0xDA);
    public static readonly Color DecyzjaTlo = Color.FromArgb(0xFF, 0xF2, 0xCC);
}

internal static class Program
{
    private static int Main(string[] args)
    {
        var dir = args.Length > 0 ? args[0] : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        foreach (var a in new[] { AgendaKorekta.Utworz(), AgendaAutorska.Utworz() })
        {
            var xlsx = Path.Combine(dir, a.Plik + ".xlsx");
            var md = Path.Combine(dir, a.Plik + ".md");
            using (var wb = new Workbook())
            {
                ArkuszAgenda(wb.Worksheets[0], a);
                ArkuszUwagi(wb.Worksheets.Add(), a);
                if (a.Persony != null) ArkuszPersony(wb.Worksheets.Add(), a);
                ArkuszPokrycie(wb.Worksheets.Add(), a);
                ArkuszMaterialy(wb.Worksheets.Add(), a);
                wb.Worksheets.ActiveWorksheet = wb.Worksheets[0];
                wb.SaveDocument(xlsx, DocumentFormat.Xlsx);
            }
            File.WriteAllText(md, Markdown(a), new UTF8Encoding(false));
            Console.WriteLine($"{a.Plik}: bloki {a.Bloki.Length}, {a.ArkuszUwag.ToLower()} {a.Uwagi.Length}, materiały {a.Materialy.Length}, pokrycie umowy {a.Pokrycie.Length}, pracownicy wzorcowi {a.Persony?.Length ?? 0}");
        }
        return 0;
    }

    private static void Glowka(Worksheet ws, string[] kolumny, int[] szerokosci, int wiersz = 0)
    {
        for (int c = 0; c < kolumny.Length; c++)
        {
            var cell = ws.Cells[wiersz, c];
            cell.Value = kolumny[c];
            cell.Font.Bold = true;
            cell.Font.Color = Color.White;
            cell.Fill.BackgroundColor = Agenda.Naglowek;
            cell.Alignment.WrapText = true;
            cell.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
            ws.Columns[c].WidthInCharacters = szerokosci[c];
        }
        ws.FreezeRows(wiersz);
    }

    private static void Wiersz(Worksheet ws, int r, params string[] wartosci)
    {
        for (int c = 0; c < wartosci.Length; c++)
        {
            var cell = ws.Cells[r, c];
            cell.Value = wartosci[c];
            cell.Alignment.WrapText = true;
            cell.Alignment.Vertical = SpreadsheetVerticalAlignment.Top;
            cell.Borders.SetAllBorders(Color.FromArgb(0xBF, 0xBF, 0xBF), BorderLineStyle.Thin);
        }
    }

    private static void ArkuszAgenda(Worksheet ws, Agenda a)
    {
        ws.Name = "Agenda";
        ws.Cells[0, 0].Value = a.Tytul;
        ws.Cells[0, 0].Font.Bold = true;
        ws.Cells[0, 0].Font.Size = 14;
        ws.Cells[1, 0].Value = a.Legenda;
        ws.Cells[1, 0].Alignment.WrapText = true;
        ws.MergeCells(ws.Range.FromLTRB(0, 1, 8, 1));
        ws.Rows[1].Height = 135;

        string[] kol = { "Dzień", "Godziny", "Temat", a.KolZrodlo, "Co pokazujemy w enova365 (standard)", "Kluczowe pytania do klienta", "Odniesienie", a.KolRyzyko, "Notatki z warsztatu (S/K/C/X)" };
        Glowka(ws, kol, new[] { 7, 12, 34, 16, 55, 70, 24, 40, 40 }, 3);

        int r = 4;
        foreach (var d in a.Dni)
        {
            Wiersz(ws, r, d.Nr, d.Data, d.Tytul, "", d.Godziny, d.Cel, "", "", "");
            var rz = ws.Range.FromLTRB(0, r, 8, r);
            rz.Fill.BackgroundColor = Agenda.DzienTlo;
            rz.Font.Bold = true;
            r++;
            foreach (var b in a.Bloki.Where(x => x.Dzien == d.Nr))
            {
                Wiersz(ws, r, b.Dzien, b.Godz, b.Temat, b.Zrodlo, b.Pokaz, b.Pytania, b.Odn, b.Ryzyko, "");
                if (b.Zrodlo == Agenda.Przerwa) ws.Range.FromLTRB(0, r, 8, r).Fill.BackgroundColor = Agenda.PrzerwaTlo;
                else if (a.Kolory.TryGetValue(b.Zrodlo, out var kolor)) ws.Cells[r, 3].Fill.BackgroundColor = kolor;
                r++;
            }
        }
        ws.Rows.AutoFit(4, r - 1);
        ws.PrintOptions.FitToPage = true;
        ws.PrintOptions.FitToWidth = 1;
        ws.PrintOptions.FitToHeight = 0;
        ws.ActiveView.Orientation = PageOrientation.Landscape;
    }

    private static void ArkuszUwagi(Worksheet ws, Agenda a)
    {
        ws.Name = a.ArkuszUwag;
        Glowka(ws, a.KolUwag, new[] { 5, 28, 70, 80 });
        for (int i = 0; i < a.Uwagi.Length; i++)
            Wiersz(ws, i + 1, a.Uwagi[i].Nr, a.Uwagi[i].Obszar, a.Uwagi[i].Co, a.Uwagi[i].Propozycja);
        ws.Rows.AutoFit(1, a.Uwagi.Length);
    }

    private static void ArkuszPersony(Worksheet ws, Agenda a)
    {
        ws.Name = "Pracownicy wzorcowi";
        Glowka(ws, new[] { "Nr", "Przypadek (do wskazania przez klienta, dane zanonimizowane)", "Co sprawdza w enova365", "Wraca w dniach", "Wskazany pracownik / kod", "Wynik w bazie testowej" },
            new[] { 5, 50, 70, 14, 26, 30 });
        for (int i = 0; i < a.Persony.Length; i++)
            Wiersz(ws, i + 1, a.Persony[i].Nr, a.Persony[i].Przypadek, a.Persony[i].CoSprawdza, a.Persony[i].Dni, "", "");
        ws.Rows.AutoFit(1, a.Persony.Length);
    }

    private static void ArkuszPokrycie(Worksheet ws, Agenda a)
    {
        ws.Name = "Pokrycie umowy";
        Glowka(ws, a.KolPokrycia, new[] { 60, 36, 60 });
        for (int i = 0; i < a.Pokrycie.Length; i++)
        {
            var p = a.Pokrycie[i];
            Wiersz(ws, i + 1, p.Pozycja, p.TwojaAgenda, p.MojaAgenda);
            if (p.TwojaAgenda.StartsWith("BRAK")) ws.Cells[i + 1, 1].Fill.BackgroundColor = Agenda.UmowaTlo;
        }
        ws.Rows.AutoFit(1, a.Pokrycie.Length);
    }

    private static void ArkuszMaterialy(Worksheet ws, Agenda a)
    {
        ws.Name = "Materiały przed spotkaniem";
        Glowka(ws, new[] { "Termin", "Materiał", "Do czego", "Otrzymano (tak/nie)" }, new[] { 24, 80, 40, 14 });
        for (int i = 0; i < a.Materialy.Length; i++)
            Wiersz(ws, i + 1, a.Materialy[i].Termin, a.Materialy[i].Co, a.Materialy[i].PoCo, "");
        ws.Rows.AutoFit(1, a.Materialy.Length);
    }

    private static string Md(string s) => s.Replace("|", "\\|").Replace("\n", "<br>");

    private static string Markdown(Agenda a)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {a.Tytul}");
        sb.AppendLine();
        sb.AppendLine($"> Plik generowany przez `Generator/` – nie edytować ręcznie. Wersja do pracy na spotkaniu: `{a.Plik}.xlsx`.");
        sb.AppendLine($"> {a.Legenda}");
        sb.AppendLine();
        sb.AppendLine($"## {a.ArkuszUwag}");
        sb.AppendLine();
        sb.AppendLine($"| {string.Join(" | ", a.KolUwag)} |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var u in a.Uwagi) sb.AppendLine($"| {u.Nr} | {Md(u.Obszar)} | {Md(u.Co)} | {Md(u.Propozycja)} |");
        sb.AppendLine();
        if (a.Persony != null)
        {
            sb.AppendLine("## Pracownicy wzorcowi");
            sb.AppendLine();
            sb.AppendLine("| Nr | Przypadek | Co sprawdza w enova365 | Dni |");
            sb.AppendLine("|---|---|---|---|");
            foreach (var p in a.Persony) sb.AppendLine($"| {p.Nr} | {Md(p.Przypadek)} | {Md(p.CoSprawdza)} | {Md(p.Dni)} |");
            sb.AppendLine();
        }
        sb.AppendLine("## Pokrycie zakresu z umowy");
        sb.AppendLine();
        sb.AppendLine($"| {string.Join(" | ", a.KolPokrycia)} |");
        sb.AppendLine("|---|---|---|");
        foreach (var p in a.Pokrycie) sb.AppendLine($"| {Md(p.Pozycja)} | {Md(p.TwojaAgenda)} | {Md(p.MojaAgenda)} |");
        sb.AppendLine();
        foreach (var d in a.Dni)
        {
            sb.AppendLine($"## Dzień {d.Nr} – {d.Data}, {d.Godziny}: {d.Tytul}");
            sb.AppendLine();
            sb.AppendLine($"*{d.Cel}*");
            sb.AppendLine();
            sb.AppendLine($"| Godziny | Temat | {a.KolZrodlo} | Co pokazujemy | Kluczowe pytania | Odniesienie | {a.KolRyzyko} |");
            sb.AppendLine("|---|---|---|---|---|---|---|");
            foreach (var b in a.Bloki.Where(x => x.Dzien == d.Nr))
                sb.AppendLine($"| {b.Godz} | {Md(b.Temat)} | {Md(b.Zrodlo)} | {Md(b.Pokaz)} | {Md(b.Pytania)} | {Md(b.Odn)} | {Md(b.Ryzyko)} |");
            sb.AppendLine();
        }
        sb.AppendLine("## Materiały do zebrania przed spotkaniem");
        sb.AppendLine();
        sb.AppendLine("| Termin | Materiał | Do czego |");
        sb.AppendLine("|---|---|---|");
        foreach (var m in a.Materialy) sb.AppendLine($"| {Md(m.Termin)} | {Md(m.Co)} | {Md(m.PoCo)} |");
        return sb.ToString();
    }
}
