using System.Text;
using System.Text.RegularExpressions;
using DevExpress.Spreadsheet;

namespace GenAnaliza;

internal sealed class Pytanie
{
    public string Obszar = "";
    public string Tresc = "";
}

internal sealed class Flaga
{
    public string Obszar = "";
    public string Tresc = "";
}

internal sealed class PozycjaZakresu
{
    public string Obszar = "";
    public string Proces = "";
    public string Nr = "";
    public string Funkcjonalnosc = "";
    public string Instrukcja = "";
    public string Licencja = "";
    public string Klasyfikacja = "";
}

internal static class Program
{
    // rozdział 1 = zakres procesów, 2–18 = pytania warsztatowe, 20 = sygnały ostrzegawcze
    private const int SekcjaZakresu = 1;
    private const int PierwszaSekcjaPytan = 2;
    private const int OstatniaSekcjaPytan = 18;
    private const int SekcjaSygnalow = 20;

    private static readonly Regex RxSekcja = new(@"^##\s+(\d+)\.\s+(.+)$");
    private static readonly Regex RxPytanie = new(@"^(\d+)\.\s+(.+)$");
    private static readonly Regex RxCheck = new(@"^-\s+\[\s*\]\s+(.+)$");
    private static readonly Regex RxFlaga = new(@"^\*\*Czerwone flagi.*?:\*\*\s*(.*)$");
    private static readonly Regex RxObszarZakresu = new(@"^###\s+([A-Z]{2,4})\s+—\s+(.+)$");
    private static readonly Regex RxProcesZakresu = new(@"^####\s+([A-Z]{2,4}-\d{2})\s+·\s+(.+)$");
    private static readonly Regex RxPozycjaZakresu = new(@"^\|\s*([A-Z]{2,4}-\d{2}-\d{3})\s*\|");

    private static int Main(string[] args)
    {
        // bin\Debug\net8.0 -> Generator -> Analiza
        var folderAnalizy = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

        var mdPath = args.Length > 0
            ? args[0]
            : Path.Combine(folderAnalizy, "Analiza przedwdrożeniowa - Kadry, Płace, Czas pracy.md");
        var outPath = args.Length > 1
            ? args[1]
            : Path.Combine(folderAnalizy, "Analiza przedwdrożeniowa - kwestionariusz.xlsx");

        if (!File.Exists(mdPath))
        {
            Console.Error.WriteLine($"Nie znaleziono pliku źródłowego: {mdPath}");
            Console.Error.WriteLine("Użycie: dotnet GenAnaliza.dll [plik.md] [plik.xlsx]");
            return 1;
        }

        var lines = File.ReadAllLines(mdPath, Encoding.UTF8);

        var pytania = new List<Pytanie>();
        var flagi = new List<Flaga>();
        var materialy = new List<string>();
        var sygnaly = new List<(string Mowi, string Znaczy)>();
        var zakres = new List<PozycjaZakresu>();

        int sekcjaNr = -1;
        string sekcjaNazwa = "";
        string obszarZakresu = "";
        string procesZakresu = "";
        Pytanie biezace = null;
        Flaga biezacaFlaga = null;

        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();

            var mSek = RxSekcja.Match(line);
            if (mSek.Success)
            {
                sekcjaNr = int.Parse(mSek.Groups[1].Value);
                sekcjaNazwa = mSek.Groups[2].Value.Trim();
                biezace = null;
                biezacaFlaga = null;
                continue;
            }

            if (line.StartsWith("## "))
            {
                sekcjaNr = -1;
                biezace = null;
                biezacaFlaga = null;
                continue;
            }

            // kontynuacja wielolinijkowa (wcięcie)
            if (raw.StartsWith("   ") && line.Trim().Length > 0)
            {
                if (biezace != null) { biezace.Tresc += " " + line.Trim(); continue; }
                if (biezacaFlaga != null) { biezacaFlaga.Tresc += " " + line.Trim(); continue; }
            }

            if (line.Trim().Length == 0) { biezace = null; biezacaFlaga = null; continue; }

            if (sekcjaNr == 0)
            {
                var mChk = RxCheck.Match(line.Trim());
                if (mChk.Success) materialy.Add(Oczysc(mChk.Groups[1].Value));
                continue;
            }

            var mFl = RxFlaga.Match(line.Trim());
            if (mFl.Success && sekcjaNr > 0)
            {
                biezace = null;
                biezacaFlaga = new Flaga { Obszar = $"{sekcjaNr}. {sekcjaNazwa}", Tresc = Oczysc(mFl.Groups[1].Value) };
                flagi.Add(biezacaFlaga);
                continue;
            }

            if (sekcjaNr == SekcjaZakresu)
            {
                var mObs = RxObszarZakresu.Match(line);
                if (mObs.Success)
                {
                    obszarZakresu = $"{mObs.Groups[1].Value} — {Oczysc(mObs.Groups[2].Value)}";
                    procesZakresu = "";
                    continue;
                }

                var mProc = RxProcesZakresu.Match(line);
                if (mProc.Success)
                {
                    procesZakresu = $"{mProc.Groups[1].Value} · {Oczysc(mProc.Groups[2].Value)}";
                    continue;
                }

                if (RxPozycjaZakresu.IsMatch(line))
                {
                    // | Nr | Funkcjonalność | Instr. | Lic. | Wymagania | Kl. | Opis |
                    var kol = line.Trim().Trim('|').Split('|');
                    if (kol.Length >= 6)
                    {
                        zakres.Add(new PozycjaZakresu
                        {
                            Obszar = obszarZakresu,
                            Proces = procesZakresu,
                            Nr = Oczysc(kol[0]),
                            Funkcjonalnosc = Oczysc(kol[1]),
                            Instrukcja = Oczysc(kol[2]),
                            Licencja = Oczysc(kol[3]),
                            Klasyfikacja = Oczysc(kol[5])
                        });
                    }
                    continue;
                }
            }

            if (sekcjaNr >= PierwszaSekcjaPytan && sekcjaNr <= OstatniaSekcjaPytan)
            {
                var mPyt = RxPytanie.Match(line.Trim());
                if (mPyt.Success && !line.StartsWith(" "))
                {
                    biezacaFlaga = null;
                    biezace = new Pytanie { Obszar = $"{sekcjaNr}. {sekcjaNazwa}", Tresc = Oczysc(mPyt.Groups[2].Value) };
                    pytania.Add(biezace);
                    continue;
                }
            }

            if (sekcjaNr == SekcjaSygnalow && line.TrimStart().StartsWith("| „"))
            {
                var parts = line.Trim().Trim('|').Split('|');
                if (parts.Length >= 2)
                    sygnaly.Add((Oczysc(parts[0]), Oczysc(parts[1])));
            }

            biezace = null;
            biezacaFlaga = null;
        }

        Console.WriteLine($"Zakres: {zakres.Count} pozycji, pytania: {pytania.Count}, flagi: {flagi.Count}, " +
                          $"materiały: {materialy.Count}, sygnały: {sygnaly.Count}");

        using var wb = new Workbook();
        wb.BeginUpdate();
        try
        {
            wb.Worksheets[0].Name = "Instrukcja";
            BudujInstrukcje(wb.Worksheets["Instrukcja"]);
            BudujMaterialy(Dodaj(wb, "Materiały od klienta"), materialy);
            BudujZakres(Dodaj(wb, "Zakres procesów"), zakres);
            BudujKwestionariusz(Dodaj(wb, "Kwestionariusz"), pytania);
            BudujFlagi(Dodaj(wb, "Czerwone flagi"), flagi);
            BudujSygnaly(Dodaj(wb, "Sygnały ostrzegawcze"), sygnaly);
            BudujRejestrC(Dodaj(wb, "Rejestr customizacji"));
            BudujOtwarte(Dodaj(wb, "Otwarte pytania"));
        }
        finally
        {
            wb.EndUpdate();
        }

        wb.SaveDocument(outPath, DocumentFormat.Xlsx);
        Console.WriteLine("Zapisano: " + outPath);
        return 0;
    }

    private static Worksheet Dodaj(Workbook wb, string nazwa)
    {
        var ws = wb.Worksheets.Add();
        ws.Name = nazwa;
        return ws;
    }

    private static string Oczysc(string s)
    {
        s = s.Replace("**", "").Replace("(→ C)", "").Trim();
        s = Regex.Replace(s, @"\s{2,}", " ");
        return s.Trim();
    }

    private static void Naglowek(Worksheet ws, int row, params string[] kolumny)
    {
        for (int i = 0; i < kolumny.Length; i++)
        {
            var c = ws.Cells[row, i];
            c.Value = kolumny[i];
            c.Font.Bold = true;
            c.Alignment.WrapText = true;
            c.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
        }
        ws.Rows[row].Height = 420;
    }

    private static void BudujInstrukcje(Worksheet ws)
    {
        var wiersze = new (string A, string B)[]
        {
            ("Analiza przedwdrożeniowa – Kadry, Płace, Czas pracy (enova365)", ""),
            ("", ""),
            ("Arkusz roboczy do prowadzenia warsztatu z klientem.", ""),
            ("Pełny opis, komentarze i uwagi metodyczne są w pliku .md obok tego arkusza.", ""),
            ("", ""),
            ("Jak używać", ""),
            ("1.", "Przed spotkaniem wyślij klientowi arkusz „Materiały od klienta” i zbierz dokumenty."),
            ("2.", "Przed spotkaniem ustal wariant licencji klienta – arkusz „Zakres procesów”, kolumna „Lic.” pokazuje, które pozycje wymagają wersji platynowej lub złotej."),
            ("3.", "Na warsztacie rozmawiaj pytaniami z arkusza „Kwestionariusz”, a ustalenia zapisuj w arkuszu „Zakres procesów”."),
            ("4.", "W arkuszu „Zakres procesów” wypełniaj kolumny „Wymagania Klienta”, „Klasyfikacja” i „Opis realizacji” – wpisana klasyfikacja to propozycja konsultanta, potwierdź ją z klientem."),
            ("5.", "Każde pytanie i każdą pozycję zakresu klasyfikuj w kolumnie „Klasyfikacja” (S / K / C / X)."),
            ("6.", "Wszystko, co wyszło jako C, przepisz do arkusza „Rejestr customizacji” z szacunkiem pracochłonności."),
            ("7.", "Czego nie da się ustalić na spotkaniu – do arkusza „Otwarte pytania” z terminem i właścicielem."),
            ("8.", "Arkusz „Czerwone flagi” to ściąga: sytuacje, które zwykle oznaczają kod, nie konfigurację."),
            ("", ""),
            ("Legenda klasyfikacji", ""),
            ("S", "Standard – działa bez zmian, wystarczy pokazać klientowi."),
            ("K", "Konfiguracja – standard, ale wymaga ustawienia (definicje, cechy, kalendarze, prawa)."),
            ("C", "Customizacja – wymaga kodu: element płacowy, weryfikator, raport, worker, dodatek."),
            ("X", "Poza zakresem – proces zostaje poza systemem albo wymaga decyzji/zmiany u klienta."),
            ("", ""),
            ("Zasada", ""),
            ("", "Najpierw pokaż standard, potem pytaj o wymagania – klient opisujący proces „jak dziś” nieświadomie zamawia customizację."),
            ("", "Na warsztacie nie obiecuj: „da się, sprawdzę jakim kosztem” i zapisz."),
            ("", "Przy każdym nietypowym składniku pytaj, ilu pracowników dotyczy – to często przesuwa temat z C na X."),
        };

        for (int i = 0; i < wiersze.Length; i++)
        {
            ws.Cells[i, 0].Value = wiersze[i].A;
            ws.Cells[i, 1].Value = wiersze[i].B;
            ws.Cells[i, 1].Alignment.WrapText = true;
        }
        ws.Cells[0, 0].Font.Bold = true;
        ws.Cells[0, 0].Font.Size = 14;

        // nagłówki sekcji pogrubiamy po treści, nie po numerze wiersza –
        // inaczej każde dopisanie punktu rozjeżdża formatowanie
        foreach (var naglowek in new[] { "Jak używać", "Legenda klasyfikacji", "Zasada" })
        {
            for (int i = 0; i < wiersze.Length; i++)
            {
                if (wiersze[i].A == naglowek) { ws.Cells[i, 0].Font.Bold = true; break; }
            }
        }
        ws.Columns[0].WidthInCharacters = 22;
        ws.Columns[1].WidthInCharacters = 110;
    }

    private static void BudujMaterialy(Worksheet ws, List<string> materialy)
    {
        Naglowek(ws, 0, "Lp.", "Dokument / materiał do zebrania przed analizą", "Otrzymano (data)", "Od kogo", "Uwagi");
        for (int i = 0; i < materialy.Count; i++)
        {
            ws.Cells[i + 1, 0].Value = i + 1;
            ws.Cells[i + 1, 1].Value = materialy[i];
            ws.Cells[i + 1, 1].Alignment.WrapText = true;
        }
        ws.Columns[0].WidthInCharacters = 6;
        ws.Columns[1].WidthInCharacters = 80;
        ws.Columns[2].WidthInCharacters = 16;
        ws.Columns[3].WidthInCharacters = 20;
        ws.Columns[4].WidthInCharacters = 40;
        ws.FreezeRows(0);
    }

    private static void BudujZakres(Worksheet ws, List<PozycjaZakresu> zakres)
    {
        Naglowek(ws, 0, "Obszar", "Proces", "Nr", "Funkcjonalność systemowa", "Instr. (str.)", "Lic.",
            "Wymagania Klienta", "Klasyfikacja (S/K/C/X)", "Na start? (T/N)", "Szac. [h]",
            "Osoba decyzyjna", "Opis realizacji");

        for (int i = 0; i < zakres.Count; i++)
        {
            int r = i + 1;
            var p = zakres[i];
            ws.Cells[r, 0].Value = p.Obszar;
            ws.Cells[r, 1].Value = p.Proces;
            ws.Cells[r, 2].Value = p.Nr;
            ws.Cells[r, 3].Value = p.Funkcjonalnosc;

            // numer strony instrukcji jako liczba, jeżeli jest podany
            if (int.TryParse(p.Instrukcja, out int str)) ws.Cells[r, 4].Value = str;
            else ws.Cells[r, 4].Value = p.Instrukcja;

            ws.Cells[r, 5].Value = p.Licencja;
            ws.Cells[r, 7].Value = p.Klasyfikacja;

            ws.Cells[r, 0].Alignment.WrapText = true;
            ws.Cells[r, 1].Alignment.WrapText = true;
            ws.Cells[r, 3].Alignment.WrapText = true;
            ws.Cells[r, 6].Alignment.WrapText = true;
            ws.Cells[r, 11].Alignment.WrapText = true;
        }

        ws.Columns[0].WidthInCharacters = 24;
        ws.Columns[1].WidthInCharacters = 34;
        ws.Columns[2].WidthInCharacters = 13;
        ws.Columns[3].WidthInCharacters = 52;
        ws.Columns[4].WidthInCharacters = 10;
        ws.Columns[5].WidthInCharacters = 7;
        ws.Columns[6].WidthInCharacters = 50;
        ws.Columns[7].WidthInCharacters = 12;
        ws.Columns[8].WidthInCharacters = 10;
        ws.Columns[9].WidthInCharacters = 9;
        ws.Columns[10].WidthInCharacters = 20;
        ws.Columns[11].WidthInCharacters = 45;

        ws.FreezePanes(0, 3);
        if (zakres.Count > 0)
            ws.AutoFilter.Apply(ws.Range.FromLTRB(0, 0, 11, zakres.Count));

        try
        {
            var walKlas = ws.DataValidations.Add(
                ws.Range.FromLTRB(7, 1, 7, zakres.Count),
                DataValidationType.List, "S;K;C;X");
            walKlas.ErrorMessage = "Dozwolone: S, K, C, X";

            ws.DataValidations.Add(
                ws.Range.FromLTRB(8, 1, 8, zakres.Count),
                DataValidationType.List, "T;N");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Walidacja arkusza zakresu pominięta: " + ex.Message);
        }
    }

    private static void BudujKwestionariusz(Worksheet ws, List<Pytanie> pytania)
    {
        Naglowek(ws, 0, "Lp.", "Obszar", "Pytanie", "Odpowiedź klienta", "Klasyfikacja (S/K/C/X)",
            "Na start? (T/N)", "Szac. [h]", "Osoba decyzyjna", "Uwagi / ustalenia");

        for (int i = 0; i < pytania.Count; i++)
        {
            int r = i + 1;
            ws.Cells[r, 0].Value = i + 1;
            ws.Cells[r, 1].Value = pytania[i].Obszar;
            ws.Cells[r, 2].Value = pytania[i].Tresc;
            ws.Cells[r, 1].Alignment.WrapText = true;
            ws.Cells[r, 2].Alignment.WrapText = true;
            ws.Cells[r, 3].Alignment.WrapText = true;
            ws.Cells[r, 8].Alignment.WrapText = true;
            ws.Rows[r].Height = 340;
        }

        ws.Columns[0].WidthInCharacters = 5;
        ws.Columns[1].WidthInCharacters = 28;
        ws.Columns[2].WidthInCharacters = 75;
        ws.Columns[3].WidthInCharacters = 50;
        ws.Columns[4].WidthInCharacters = 12;
        ws.Columns[5].WidthInCharacters = 10;
        ws.Columns[6].WidthInCharacters = 9;
        ws.Columns[7].WidthInCharacters = 20;
        ws.Columns[8].WidthInCharacters = 45;

        ws.FreezePanes(0, 2);
        if (pytania.Count > 0)
            ws.AutoFilter.Apply(ws.Range.FromLTRB(0, 0, 8, pytania.Count));

        try
        {
            var walKlas = ws.DataValidations.Add(
                ws.Range.FromLTRB(4, 1, 4, pytania.Count),
                DataValidationType.List, "S;K;C;X");
            walKlas.ErrorMessage = "Dozwolone: S, K, C, X";

            var walStart = ws.DataValidations.Add(
                ws.Range.FromLTRB(5, 1, 5, pytania.Count),
                DataValidationType.List, "T;N");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Walidacja pominięta: " + ex.Message);
        }
    }

    private static void BudujFlagi(Worksheet ws, List<Flaga> flagi)
    {
        Naglowek(ws, 0, "Obszar", "Czerwone flagi – sytuacje, które zwykle oznaczają customizację (C)", "Czy występuje? (T/N)", "Notatki");
        for (int i = 0; i < flagi.Count; i++)
        {
            ws.Cells[i + 1, 0].Value = flagi[i].Obszar;
            ws.Cells[i + 1, 1].Value = flagi[i].Tresc;
            ws.Cells[i + 1, 0].Alignment.WrapText = true;
            ws.Cells[i + 1, 1].Alignment.WrapText = true;
            ws.Cells[i + 1, 3].Alignment.WrapText = true;
            ws.Rows[i + 1].Height = 700;
        }
        ws.Columns[0].WidthInCharacters = 28;
        ws.Columns[1].WidthInCharacters = 95;
        ws.Columns[2].WidthInCharacters = 16;
        ws.Columns[3].WidthInCharacters = 45;
        ws.FreezeRows(0);
    }

    private static void BudujSygnaly(Worksheet ws, List<(string Mowi, string Znaczy)> sygnaly)
    {
        Naglowek(ws, 0, "Klient mówi", "Co to zwykle znaczy", "Padło na spotkaniu? (T/N)", "Kontekst");
        for (int i = 0; i < sygnaly.Count; i++)
        {
            ws.Cells[i + 1, 0].Value = sygnaly[i].Mowi;
            ws.Cells[i + 1, 1].Value = sygnaly[i].Znaczy;
            ws.Cells[i + 1, 0].Alignment.WrapText = true;
            ws.Cells[i + 1, 1].Alignment.WrapText = true;
            ws.Cells[i + 1, 3].Alignment.WrapText = true;
            ws.Rows[i + 1].Height = 340;
        }
        ws.Columns[0].WidthInCharacters = 60;
        ws.Columns[1].WidthInCharacters = 55;
        ws.Columns[2].WidthInCharacters = 18;
        ws.Columns[3].WidthInCharacters = 45;
        ws.FreezeRows(0);
    }

    private static void BudujRejestrC(Worksheet ws)
    {
        Naglowek(ws, 0, "Nr", "Obszar", "Nazwa robocza", "Opis funkcjonalny (co ma robić)",
            "Typ artefaktu (element płacowy / weryfikator / raport / worker / dodatek / import)",
            "Blokuje start? (T/N)", "Szac. [h]", "Zgłosił / decyduje", "Status", "Uwagi");
        ws.Columns[0].WidthInCharacters = 6;
        ws.Columns[1].WidthInCharacters = 26;
        ws.Columns[2].WidthInCharacters = 34;
        ws.Columns[3].WidthInCharacters = 75;
        ws.Columns[4].WidthInCharacters = 28;
        ws.Columns[5].WidthInCharacters = 14;
        ws.Columns[6].WidthInCharacters = 9;
        ws.Columns[7].WidthInCharacters = 22;
        ws.Columns[8].WidthInCharacters = 16;
        ws.Columns[9].WidthInCharacters = 40;
        ws.FreezeRows(0);
    }

    private static void BudujOtwarte(Worksheet ws)
    {
        Naglowek(ws, 0, "Nr", "Obszar", "Pytanie / brakująca informacja", "Kto odpowiada (rola)",
            "Termin", "Dlaczego to blokuje", "Status", "Odpowiedź");
        ws.Columns[0].WidthInCharacters = 6;
        ws.Columns[1].WidthInCharacters = 26;
        ws.Columns[2].WidthInCharacters = 70;
        ws.Columns[3].WidthInCharacters = 24;
        ws.Columns[4].WidthInCharacters = 14;
        ws.Columns[5].WidthInCharacters = 40;
        ws.Columns[6].WidthInCharacters = 14;
        ws.Columns[7].WidthInCharacters = 55;
        ws.FreezeRows(0);
    }
}
