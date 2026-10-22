using System.IO.Compression;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GenWzor;

// Generator wzorów analizy przedwdrożeniowej KPCP.
// Każdy plik Wzor_analizy/Tresc/*.txt (lekki znacznikowy format – opis w STYLE_GUIDE.md, rozdz. 9)
// zamieniany jest na osobny dokument .docx w Wzor_analizy/. Style, motyw i czcionki pochodzą
// z Generator/baza (kopie części dokumentu wzorcowego, bez treści klienta), zrzuty ekranu z Generator/obrazy.
//
//   dotnet run -c Release -- "<katalog Wzor_analizy>"            – buduje wszystkie wzory
//   dotnet run -c Release -- "<katalog Wzor_analizy>" sprawdz    – kontrola gotowych plików (style, numeracja,
//                                                                  zakazane słowa z _robocze/zakazane_slowa.txt)
static class Program
{
    static int Main(string[] args)
    {
        var root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        var tryb = args.Length > 1 ? args[1] : "buduj";
        var gen = Path.Combine(root, "Generator");
        var pliki = Directory.GetFiles(Path.Combine(root, "Tresc"), "*.txt").OrderBy(x => x).ToList();

        var wyniki = new List<string>();
        foreach (var plik in pliki)
        {
            var dok = new Dokument(gen);
            dok.Wczytaj(plik);
            var cel = Path.Combine(root, dok.PlikWynikowy);
            if (tryb == "buduj")
            {
                Pakiet.Zapisz(dok, gen, cel);
                Console.WriteLine($"{dok.PlikWynikowy}: nagłówki {dok.Naglowki}, pola do uzupełnienia {dok.Pola}, " +
                                  $"sekcje opcjonalne {dok.Opcjonalne}, tabele {dok.Tabele}, rysunki {dok.Rysunki}");
            }
            wyniki.Add(cel);
        }
        return Kontrola.Sprawdz(wyniki, Path.Combine(root, "_robocze", "zakazane_slowa.txt")) ? 0 : 1;
    }
}

// Treść jednego wzoru: parsowanie pliku .txt i budowa document.xml.
class Dokument
{
    public const string Pole = "DO UZUPEŁNIENIA";
    public const string ZnacznikOpcjonalnej = "[SEKCJA OPCJONALNA – usuń, jeśli nie dotyczy]";
    const int SzerTresci = 9027;   // szerokość tekstu w sekcji treści (A4, marginesy 2,54 cm), twip
    const int SzerTytulowej = 9747; // szerokość tekstu w sekcji tytułowej (marginesy 1,9 cm), twip
    const long MaxSzerObrazu = 5732000, MaxWysObrazu = 6800000; // EMU

    readonly string gen;
    public string Tytul = "ANALIZA PRZEDWDROŻENIOWA", Klient = "[DO UZUPEŁNIENIA: nazwa klienta]", Obszar, Opis = "";
    public string PlikWynikowy, NazwaWzoru;
    public readonly List<(string Rid, string Sciezka)> Media = new();
    public int Naglowki, Pola, Opcjonalne, Tabele, Rysunki;
    readonly StringBuilder tresc = new();
    int rid = 100, docPr = 10, nrRys, nrTab;

    public Dokument(string gen) => this.gen = gen;
    public string Body { get; private set; }

    public void Wczytaj(string plik)
    {
        var linie = File.ReadAllLines(plik, Encoding.UTF8);
        int i = 0;
        // nagłówek pliku: dyrektywy @tytul, @klient, @obszar, @plik, @opis (do pierwszej pustej linii)
        for (; i < linie.Length && linie[i].StartsWith("@"); i++)
        {
            var (klucz, wartosc) = Dyrektywa(linie[i]);
            switch (klucz)
            {
                case "tytul": Tytul = wartosc; break;
                case "klient": Klient = wartosc; break;
                case "obszar": Obszar = wartosc; break;
                case "plik": PlikWynikowy = wartosc; break;
                case "opis": Opis = wartosc; break;
                default: throw new InvalidDataException($"{plik}:{i + 1}: nieznana dyrektywa nagłówka @{klucz}");
            }
        }
        NazwaWzoru = Path.GetFileNameWithoutExtension(PlikWynikowy);
        StronaTytulowa();

        var tabela = new List<string[]>();
        int[] kolumny = null;
        for (; i < linie.Length; i++)
        {
            var l = linie[i].TrimEnd();
            if (l.StartsWith("|"))
            {
                tabela.Add(l.Trim('|').Split('|').Select(c => c.Trim()).ToArray());
                continue;
            }
            if (tabela.Count > 0) { Tabela(tabela, kolumny); tabela.Clear(); kolumny = null; }
            if (l.Length == 0 || l.StartsWith("//")) continue;

            if (l.StartsWith("#"))
            {
                int poziom = l.TakeWhile(c => c == '#').Count();
                Akapit($"Nagwek{poziom}", l[poziom..].Trim());
                Naglowki++;
            }
            else if (l.StartsWith("  - ")) Akapit("Listapunktowana", l[4..], "<w:numPr><w:ilvl w:val=\"1\"/><w:numId w:val=\"2\"/></w:numPr>");
            else if (l.StartsWith("- ")) Akapit("Listapunktowana", l[2..]);
            else if (l.StartsWith("> ")) Akapit("Tekstkomentarza", l[2..]);
            else if (l.StartsWith("@"))
            {
                var (klucz, wartosc) = Dyrektywa(l);
                switch (klucz)
                {
                    case "kolumny": kolumny = wartosc.Split(',').Select(int.Parse).ToArray(); break;
                    case "tabela": Podpis("Tabela", wartosc); break;
                    case "rys": Rysunek(wartosc); break;
                    case "opcjonalna":
                        Akapit("Sekcjaopcjonalna", ZnacznikOpcjonalnej + (wartosc.Length > 0 ? " " + wartosc : ""));
                        Opcjonalne++;
                        break;
                    case "strona": tresc.Append("<w:p><w:r><w:br w:type=\"page\"/></w:r></w:p>"); break;
                    case "spis-rys": Pole_("Spisilustracji", "TOC \\h \\z \\c \"Rys.\"", "Wykaz rysunków – zaktualizuj pole (F9)."); break;
                    case "spis-tab": Pole_("Spisilustracji", "TOC \\h \\z \\c \"Tabela\"", "Wykaz tabel – zaktualizuj pole (F9)."); break;
                    case "kontakt": Kontakt(); break;
                    case "akceptacja": Akceptacja(); break;
                    default: throw new InvalidDataException($"{plik}:{i + 1}: nieznana dyrektywa @{klucz}");
                }
            }
            else Akapit("Normalny", l);
        }
        if (tabela.Count > 0) Tabela(tabela, kolumny);
        Body = tresc.ToString();
    }

    static (string, string) Dyrektywa(string l)
    {
        var sp = l.IndexOf(' ');
        return sp < 0 ? (l[1..], "") : (l[1..sp], l[(sp + 1)..].Trim());
    }

    // ---------- strona tytułowa, historia zmian, spis treści (sekcja 1) ----------
    void StronaTytulowa()
    {
        tresc.Append($"<w:p><w:pPr><w:pStyle w:val=\"Logo\"/><w:jc w:val=\"center\"/></w:pPr>{Obraz(Path.Combine(gen, "baza", "logo_wykonawcy.png"), 2340000)}</w:p>");
        Akapit("Tytulanalizy", Tytul);
        Akapit("Tytulanalizy", "DLA " + Klient);
        if (!string.IsNullOrEmpty(Obszar)) Akapit("Obszaranalizy", Obszar);
        tresc.Append($"<w:p><w:pPr><w:jc w:val=\"center\"/><w:spacing w:before=\"240\" w:after=\"240\"/></w:pPr>{Obraz(Path.Combine(gen, "baza", "grafika_tytulowa.png"), 2520000)}</w:p>");

        var metryka = new[]
        {
            new[] { "Data utworzenia:", "[DO UZUPEŁNIENIA: data utworzenia, dd.mm.rrrr]" },
            new[] { "Wersja:", "[DO UZUPEŁNIENIA: numer wersji X.Y]" },
            new[] { "Autor:", "[DO UZUPEŁNIENIA: autorzy dokumentu]" },
            new[] { "Ostatnia modyfikacja:", "[DO UZUPEŁNIENIA: data ostatniej modyfikacji]" },
            new[] { "Status dokumentu:", "[DO UZUPEŁNIENIA: Roboczy / Do akceptacji Klienta / Obustronnie zaakceptowany – FINAL]" },
            new[] { "Ważna do:", "[DO UZUPEŁNIENIA: data ważności analizy]" },
        };
        tresc.Append("<w:tbl><w:tblPr><w:tblW w:w=\"" + SzerTytulowej + "\" w:type=\"dxa\"/><w:tblBorders><w:insideH w:val=\"single\" w:sz=\"4\" w:space=\"0\" w:color=\"FFFFFF\"/></w:tblBorders>" +
                     "<w:tblLayout w:type=\"fixed\"/><w:tblLook w:val=\"0000\" w:firstRow=\"0\" w:lastRow=\"0\" w:firstColumn=\"0\" w:lastColumn=\"0\" w:noHBand=\"1\" w:noVBand=\"1\"/></w:tblPr>" +
                     "<w:tblGrid><w:gridCol w:w=\"2500\"/><w:gridCol w:w=\"" + (SzerTytulowej - 2500) + "\"/></w:tblGrid>");
        foreach (var w in metryka)
        {
            tresc.Append("<w:tr>");
            tresc.Append(Komorka(w[0], 2500, "Metryka", "F2F2F2"));
            tresc.Append(Komorka(w[1], SzerTytulowej - 2500, "Metryka", "F2F2F2"));
            tresc.Append("</w:tr>");
        }
        tresc.Append("</w:tbl>");
        Akapit("Klauzula", "Dokument został opracowany przez AltOne wyłącznie dla " + Klient +
                           ". Nie może być reprodukowany, kopiowany lub publikowany w całości lub jakiejkolwiek jego części bez pisemnej zgody AltOne.");
        tresc.Append("<w:p><w:r><w:br w:type=\"page\"/></w:r></w:p>");

        Akapit("Nagwekspisutreci", "Historia zmian dokumentu");
        Tabela(new List<string[]>
        {
            new[] { "Wersja", "Data", "Autor", "Opis zmian" },
            new[] { "0.1", "[DO UZUPEŁNIENIA: data]", "[DO UZUPEŁNIENIA: autor]", "Utworzenie dokumentu na podstawie wzoru " + NazwaWzoru + "." },
        }, new[] { 12, 16, 24, 48 }, SzerTytulowej, policz: false);

        Akapit("Nagwekspisutreci", "Spis treści");
        Pole_("Spistreci10", "TOC \\o \"1-3\" \\h \\z \\u", "Spis treści – kliknij prawym przyciskiem myszy i wybierz „Aktualizuj pole”.");

        // koniec sekcji 1 (strona tytułowa + spis): marginesy 1,9 cm, inna pierwsza strona
        tresc.Append("<w:p><w:pPr><w:sectPr>" +
                     "<w:headerReference w:type=\"default\" r:id=\"rIdNaglowek\"/><w:headerReference w:type=\"first\" r:id=\"rIdNaglowekPierwszy\"/>" +
                     "<w:footerReference w:type=\"default\" r:id=\"rIdStopka\"/><w:footerReference w:type=\"first\" r:id=\"rIdStopka\"/>" +
                     "<w:pgSz w:w=\"11907\" w:h=\"16839\" w:code=\"9\"/><w:pgMar w:top=\"1080\" w:right=\"1080\" w:bottom=\"851\" w:left=\"1080\" w:header=\"680\" w:footer=\"720\" w:gutter=\"0\"/>" +
                     "<w:cols w:space=\"720\"/><w:titlePg/><w:docGrid w:linePitch=\"360\"/></w:sectPr></w:pPr></w:p>");
    }

    public static string SekcjaKoncowa =>
        "<w:sectPr><w:headerReference w:type=\"default\" r:id=\"rIdNaglowek\"/><w:footerReference w:type=\"default\" r:id=\"rIdStopka\"/>" +
        "<w:pgSz w:w=\"11907\" w:h=\"16839\" w:code=\"9\"/><w:pgMar w:top=\"1080\" w:right=\"1440\" w:bottom=\"1080\" w:left=\"1440\" w:header=\"720\" w:footer=\"720\" w:gutter=\"0\"/>" +
        "<w:cols w:space=\"720\"/><w:docGrid w:linePitch=\"360\"/></w:sectPr>";

    // ---------- elementy treści ----------
    void Akapit(string styl, string tekst, string pPrDodatkowe = "")
    {
        tresc.Append($"<w:p><w:pPr><w:pStyle w:val=\"{styl}\"/>{pPrDodatkowe}</w:pPr>{Przebiegi(tekst)}</w:p>");
    }

    static readonly Regex Inline = new(@"\*\*(?<b>.+?)\*\*|(?<pole>\[DO UZUPEŁNIENIA:[^\]]*\])|(?<opc>\[SEKCJA OPCJONALNA[^\]]*\])", RegexOptions.Compiled);

    string Przebiegi(string tekst)
    {
        var sb = new StringBuilder();
        int poz = 0;
        foreach (Match m in Inline.Matches(tekst))
        {
            if (m.Index > poz) sb.Append(Run(tekst[poz..m.Index], ""));
            if (m.Groups["b"].Success) sb.Append(Run(m.Groups["b"].Value, "<w:b/>"));
            else if (m.Groups["pole"].Success) { sb.Append(Run(m.Value, "<w:rStyle w:val=\"PoledoUzupelnienia\"/><w:highlight w:val=\"yellow\"/>")); Pola++; }
            else sb.Append(Run(m.Value, ""));
            poz = m.Index + m.Length;
        }
        if (poz < tekst.Length) sb.Append(Run(tekst[poz..], ""));
        return sb.ToString();
    }

    static string Run(string t, string rPr) =>
        $"<w:r>{(rPr.Length > 0 ? "<w:rPr>" + rPr + "</w:rPr>" : "")}<w:t xml:space=\"preserve\">{SecurityElement.Escape(t)}</w:t></w:r>";

    static string PoleZlozone(string instr, string wynik, string rPr = "") =>
        "<w:r><w:fldChar w:fldCharType=\"begin\" w:dirty=\"true\"/></w:r>" +
        $"<w:r><w:instrText xml:space=\"preserve\"> {SecurityElement.Escape(instr)} </w:instrText></w:r>" +
        "<w:r><w:fldChar w:fldCharType=\"separate\"/></w:r>" + Run(wynik, rPr) +
        "<w:r><w:fldChar w:fldCharType=\"end\"/></w:r>";

    void Pole_(string styl, string instr, string zastepczy) =>
        tresc.Append($"<w:p><w:pPr><w:pStyle w:val=\"{styl}\"/></w:pPr>{PoleZlozone(instr, zastepczy)}</w:p>");

    void Podpis(string rodzaj, string tekst)
    {
        var nr = rodzaj == "Rys." ? ++nrRys : ++nrTab;
        var etykieta = rodzaj == "Rys." ? "Rys." : "Tabela";
        tresc.Append($"<w:p><w:pPr><w:pStyle w:val=\"Legenda\"/></w:pPr>{Run(etykieta + " ", "")}" +
                     $"{PoleZlozone($"SEQ {etykieta} \\* ARABIC", nr.ToString())}{Przebiegi(" " + tekst)}</w:p>");
    }

    void Rysunek(string wartosc)
    {
        var cz = wartosc.Split('|', 2);
        var plik = cz[0].Trim();
        var podpis = cz.Length > 1 ? cz[1].Trim() : "";
        if (plik == "BRAK")
            Akapit("Normalny", $"[DO UZUPEŁNIENIA: zrzut ekranu – {podpis}]", "<w:keepNext/><w:jc w:val=\"center\"/>");
        else
            tresc.Append($"<w:p><w:pPr><w:keepNext/><w:jc w:val=\"center\"/></w:pPr>{Obraz(Path.Combine(gen, "obrazy", plik))}</w:p>");
        Podpis("Rys.", podpis);
        Rysunki++;
    }

    internal string Obraz(string sciezka, long szerokoscEmu = 0)
    {
        if (!File.Exists(sciezka)) throw new FileNotFoundException("Brak obrazu", sciezka);
        var (px, py) = RozmiarPng(sciezka);
        long cx = szerokoscEmu > 0 ? szerokoscEmu : px * 9525L, cy = cx * py / px;
        if (cx > MaxSzerObrazu) { cy = cy * MaxSzerObrazu / cx; cx = MaxSzerObrazu; }
        if (cy > MaxWysObrazu) { cx = cx * MaxWysObrazu / cy; cy = MaxWysObrazu; }
        var r = "rIdObraz" + (rid++);
        Media.Add((r, sciezka));
        var id = docPr++;
        var nazwa = SecurityElement.Escape(Path.GetFileName(sciezka));
        return "<w:r><w:drawing><wp:inline distT=\"0\" distB=\"0\" distL=\"0\" distR=\"0\">" +
               $"<wp:extent cx=\"{cx}\" cy=\"{cy}\"/><wp:docPr id=\"{id}\" name=\"Obraz {id}\" descr=\"{nazwa}\"/>" +
               "<wp:cNvGraphicFramePr><a:graphicFrameLocks noChangeAspect=\"1\"/></wp:cNvGraphicFramePr>" +
               "<a:graphic><a:graphicData uri=\"http://schemas.openxmlformats.org/drawingml/2006/picture\"><pic:pic>" +
               $"<pic:nvPicPr><pic:cNvPr id=\"{id}\" name=\"{nazwa}\"/><pic:cNvPicPr/></pic:nvPicPr>" +
               $"<pic:blipFill><a:blip r:embed=\"{r}\"/><a:stretch><a:fillRect/></a:stretch></pic:blipFill>" +
               $"<pic:spPr><a:xfrm><a:off x=\"0\" y=\"0\"/><a:ext cx=\"{cx}\" cy=\"{cy}\"/></a:xfrm><a:prstGeom prst=\"rect\"><a:avLst/></a:prstGeom></pic:spPr>" +
               "</pic:pic></a:graphicData></a:graphic></wp:inline></w:drawing></w:r>";
    }

    static (int, int) RozmiarPng(string sciezka)
    {
        var b = new byte[24];
        using (var f = File.OpenRead(sciezka)) f.ReadExactly(b);
        int Be(int o) => (b[o] << 24) | (b[o + 1] << 16) | (b[o + 2] << 8) | b[o + 3];
        return (Be(16), Be(20));
    }

    void Tabela(List<string[]> wiersze, int[] procenty, int szerokosc = SzerTresci, bool policz = true)
    {
        int n = wiersze.Max(w => w.Length);
        procenty ??= Enumerable.Repeat(100 / n, n).ToArray();
        if (procenty.Length != n) throw new InvalidDataException($"@kolumny: {procenty.Length} szerokości, a tabela ma {n} kolumn ({wiersze[0][0]})");
        var szer = procenty.Select(p => szerokosc * p / procenty.Sum()).ToArray();
        tresc.Append("<w:tbl><w:tblPr><w:tblStyle w:val=\"ProposalTable\"/>" +
                     $"<w:tblW w:w=\"{szer.Sum()}\" w:type=\"dxa\"/><w:tblLayout w:type=\"fixed\"/>" +
                     "<w:tblLook w:val=\"0420\" w:firstRow=\"1\" w:lastRow=\"0\" w:firstColumn=\"0\" w:lastColumn=\"0\" w:noHBand=\"1\" w:noVBand=\"1\"/></w:tblPr><w:tblGrid>");
        foreach (var s in szer) tresc.Append($"<w:gridCol w:w=\"{s}\"/>");
        tresc.Append("</w:tblGrid>");
        for (int r = 0; r < wiersze.Count; r++)
        {
            tresc.Append(r == 0 ? "<w:tr><w:trPr><w:cantSplit/><w:tblHeader/></w:trPr>" : "<w:tr><w:trPr><w:cantSplit/></w:trPr>");
            for (int c = 0; c < n; c++)
                tresc.Append(Komorka(c < wiersze[r].Length ? wiersze[r][c] : "", szer[c], "Tekstabeli", null));
            tresc.Append("</w:tr>");
        }
        tresc.Append("</w:tbl>");
        // akapit odstępu po tabeli – bez niego kolejna tabela/podpis skleja się z siatką
        tresc.Append("<w:p><w:pPr><w:pStyle w:val=\"Tekstabeli\"/></w:pPr></w:p>");
        if (policz) Tabele++;
    }

    string Komorka(string tekst, int szer, string styl, string tlo)
    {
        var sb = new StringBuilder($"<w:tc><w:tcPr><w:tcW w:w=\"{szer}\" w:type=\"dxa\"/>");
        if (tlo != null) sb.Append($"<w:shd w:val=\"clear\" w:color=\"auto\" w:fill=\"{tlo}\"/>");
        sb.Append("</w:tcPr>");
        // w komórce „<br>” rozdziela akapity, „- ” na początku akapitu daje punktor
        foreach (var czesc in tekst.Split("<br>"))
        {
            var t = czesc.Trim();
            var punktor = t.StartsWith("- ");
            if (punktor) t = t[2..];
            sb.Append($"<w:p><w:pPr><w:pStyle w:val=\"{styl}\"/>" +
                      (punktor ? "<w:numPr><w:ilvl w:val=\"0\"/><w:numId w:val=\"2\"/></w:numPr><w:ind w:left=\"284\" w:hanging=\"227\"/>" : "") +
                      $"</w:pPr>{Przebiegi(t)}</w:p>");
        }
        return sb.Append("</w:tc>").ToString();
    }

    void Kontakt()
    {
        foreach (var rola in new[] { "Kierownik projektu", "Konsultant wdrożeniowy – Kadry i Płace", "Konsultant wdrożeniowy – Czas pracy i Pulpity" })
        {
            Akapit("Bezodstpw", $"[DO UZUPEŁNIENIA: imię i nazwisko – {rola}]", "<w:spacing w:before=\"160\"/>");
            Akapit("Bezodstpw", rola);
            Akapit("Bezodstpw", "[DO UZUPEŁNIENIA: adres e-mail]");
            Akapit("Bezodstpw", "tel. kom.: [DO UZUPEŁNIENIA: numer telefonu]");
        }
        Akapit("Bezodstpw", "AltOne, [DO UZUPEŁNIENIA: adres siedziby wykonawcy]", "<w:spacing w:before=\"240\"/>");
        Akapit("Bezodstpw", "tel. [DO UZUPEŁNIENIA: telefon centrali], [DO UZUPEŁNIENIA: adres www i e-mail wykonawcy]");
        tresc.Append($"<w:p><w:pPr><w:spacing w:before=\"240\"/></w:pPr>{Obraz(Path.Combine(gen, "baza", "partner_enova365.png"), 1620000)}</w:p>");
    }

    void Akceptacja()
    {
        var wiersze = new List<string[]>
        {
            new[] { "", "Zamawiający", "Wykonawca" },
            new[] { "Nazwa", Klient, "AltOne" },
            new[] { "Imię i nazwisko", "[DO UZUPEŁNIENIA: osoba akceptująca po stronie Klienta]", "[DO UZUPEŁNIENIA: osoba akceptująca po stronie Wykonawcy]" },
            new[] { "Stanowisko", "[DO UZUPEŁNIENIA: stanowisko]", "[DO UZUPEŁNIENIA: stanowisko]" },
            new[] { "Wersja dokumentu", "[DO UZUPEŁNIENIA: akceptowana wersja X.Y]", "[DO UZUPEŁNIENIA: akceptowana wersja X.Y]" },
            new[] { "Data akceptacji", "[DO UZUPEŁNIENIA: data]", "[DO UZUPEŁNIENIA: data]" },
            new[] { "Podpis", "<br><br>", "<br><br>" },
        };
        Tabela(wiersze, new[] { 24, 38, 38 });
        Podpis("Tabela", "Akceptacja dokumentu");
    }
}

// Złożenie pakietu .docx: części stałe (style, numeracja, nagłówek, stopka) + document.xml z treścią.
static class Pakiet
{
    const string NsW = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    const string NsDok = "xmlns:w=\"" + NsW + "\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" " +
                         "xmlns:wp=\"http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing\" " +
                         "xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" " +
                         "xmlns:pic=\"http://schemas.openxmlformats.org/drawingml/2006/picture\"";

    public static void Zapisz(Dokument d, string gen, string cel)
    {
        if (File.Exists(cel)) File.Delete(cel);
        using var zip = ZipFile.Open(cel, ZipArchiveMode.Create);
        void Dodaj(string nazwa, string xml) { using var w = new StreamWriter(zip.CreateEntry(nazwa).Open(), new UTF8Encoding(false)); w.Write(xml); }
        void DodajPlik(string nazwa, string zrodlo) { using var s = zip.CreateEntry(nazwa).Open(); using var f = File.OpenRead(zrodlo); f.CopyTo(s); }

        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n";
        Dodaj("[Content_Types].xml", xml +
            "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
            "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>" +
            "<Default Extension=\"xml\" ContentType=\"application/xml\"/><Default Extension=\"png\" ContentType=\"image/png\"/>" +
            "<Override PartName=\"/word/document.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml\"/>" +
            "<Override PartName=\"/word/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml\"/>" +
            "<Override PartName=\"/word/numbering.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml\"/>" +
            "<Override PartName=\"/word/settings.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml\"/>" +
            "<Override PartName=\"/word/fontTable.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.fontTable+xml\"/>" +
            "<Override PartName=\"/word/theme/theme1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.theme+xml\"/>" +
            "<Override PartName=\"/word/header1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml\"/>" +
            "<Override PartName=\"/word/header2.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.header+xml\"/>" +
            "<Override PartName=\"/word/footer1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml\"/>" +
            "<Override PartName=\"/docProps/core.xml\" ContentType=\"application/vnd.openxmlformats-package.core-properties+xml\"/>" +
            "<Override PartName=\"/docProps/app.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.extended-properties+xml\"/>" +
            "</Types>");
        Dodaj("_rels/.rels", xml +
            "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
            "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"word/document.xml\"/>" +
            "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties\" Target=\"docProps/core.xml\"/>" +
            "<Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties\" Target=\"docProps/app.xml\"/>" +
            "</Relationships>");
        var teraz = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        Dodaj("docProps/core.xml", xml +
            "<cp:coreProperties xmlns:cp=\"http://schemas.openxmlformats.org/package/2006/metadata/core-properties\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" " +
            "xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" +
            $"<dc:title>{SecurityElement.Escape(d.Tytul + (d.Obszar != null ? " – " + d.Obszar : ""))}</dc:title>" +
            $"<dc:subject>{SecurityElement.Escape(d.Opis)}</dc:subject><dc:creator>Wzór analizy KPCP</dc:creator><cp:lastModifiedBy>Wzór analizy KPCP</cp:lastModifiedBy>" +
            $"<dcterms:created xsi:type=\"dcterms:W3CDTF\">{teraz}</dcterms:created><dcterms:modified xsi:type=\"dcterms:W3CDTF\">{teraz}</dcterms:modified>" +
            "</cp:coreProperties>");
        Dodaj("docProps/app.xml", xml +
            "<Properties xmlns=\"http://schemas.openxmlformats.org/officeDocument/2006/extended-properties\"><Application>Microsoft Office Word</Application><Company>AltOne</Company></Properties>");

        var rels = new StringBuilder(xml + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
        void Rel(string id, string typ, string cel_) => rels.Append($"<Relationship Id=\"{id}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/{typ}\" Target=\"{cel_}\"/>");
        Rel("rIdStyle", "styles", "styles.xml");
        Rel("rIdNumeracja", "numbering", "numbering.xml");
        Rel("rIdUstawienia", "settings", "settings.xml");
        Rel("rIdCzcionki", "fontTable", "fontTable.xml");
        Rel("rIdMotyw", "theme", "theme/theme1.xml");
        Rel("rIdNaglowek", "header", "header1.xml");
        Rel("rIdNaglowekPierwszy", "header", "header2.xml");
        Rel("rIdStopka", "footer", "footer1.xml");
        int nrMedia = 1;
        foreach (var (rid, sciezka) in d.Media)
        {
            var nazwa = $"media/obraz{nrMedia++}.png";
            Rel(rid, "image", nazwa);
            DodajPlik("word/" + nazwa, sciezka);
        }
        rels.Append("</Relationships>");
        Dodaj("word/_rels/document.xml.rels", rels.ToString());

        Dodaj("word/document.xml", xml + $"<w:document {NsDok}><w:body>{d.Body}{Dokument.SekcjaKoncowa}</w:body></w:document>");
        Dodaj("word/styles.xml", Style.Zbuduj(Path.Combine(gen, "baza", "styles.xml")));
        Dodaj("word/numbering.xml", xml + Style.Numeracja);
        Dodaj("word/settings.xml", xml + $"<w:settings xmlns:w=\"{NsW}\"><w:zoom w:percent=\"100\"/><w:updateFields w:val=\"true\"/>" +
            "<w:defaultTabStop w:val=\"720\"/><w:hyphenationZone w:val=\"425\"/><w:characterSpacingControl w:val=\"doNotCompress\"/>" +
            "<w:compat><w:compatSetting w:name=\"compatibilityMode\" w:uri=\"http://schemas.microsoft.com/office/word\" w:val=\"15\"/></w:compat>" +
            "<w:themeFontLang w:val=\"pl-PL\"/><w:decimalSymbol w:val=\",\"/><w:listSeparator w:val=\";\"/></w:settings>");
        DodajPlik("word/fontTable.xml", Path.Combine(gen, "baza", "fontTable.xml"));
        DodajPlik("word/theme/theme1.xml", Path.Combine(gen, "baza", "theme1.xml"));

        // nagłówek: logo wykonawcy do prawej; pierwsza strona (tytułowa) bez nagłówka
        var logo = new Dokument(gen);
        var logoRun = logo.Obraz(Path.Combine(gen, "baza", "logo_wykonawcy.png"), 1260000);
        Dodaj("word/header1.xml", xml + $"<w:hdr {NsDok}><w:p><w:pPr><w:pStyle w:val=\"Nagwek0\"/><w:jc w:val=\"right\"/></w:pPr>{logoRun}</w:p></w:hdr>");
        Dodaj("word/_rels/header1.xml.rels", xml + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
            $"<Relationship Id=\"{logo.Media[0].Rid}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/image\" Target=\"media/logo_naglowek.png\"/></Relationships>");
        DodajPlik("word/media/logo_naglowek.png", logo.Media[0].Sciezka);
        Dodaj("word/header2.xml", xml + $"<w:hdr {NsDok}><w:p><w:pPr><w:pStyle w:val=\"Nagwek0\"/></w:pPr></w:p></w:hdr>");
        // stopka jak w dokumentach źródłowych: „Strona | nr” do prawej + „system enova365”
        Dodaj("word/footer1.xml", xml + $"<w:ftr {NsDok}><w:p><w:pPr><w:pStyle w:val=\"Stopka0\"/><w:jc w:val=\"right\"/></w:pPr>" +
            "<w:r><w:t xml:space=\"preserve\">Strona | </w:t></w:r><w:r><w:fldChar w:fldCharType=\"begin\"/></w:r><w:r><w:instrText xml:space=\"preserve\"> PAGE   \\* MERGEFORMAT </w:instrText></w:r>" +
            "<w:r><w:fldChar w:fldCharType=\"separate\"/></w:r><w:r><w:t>1</w:t></w:r><w:r><w:fldChar w:fldCharType=\"end\"/></w:r></w:p>" +
            "<w:p><w:pPr><w:spacing w:line=\"240\" w:lineRule=\"auto\"/><w:jc w:val=\"left\"/></w:pPr><w:r><w:rPr><w:color w:val=\"5E5E5E\"/><w:sz w:val=\"18\"/></w:rPr><w:t>system enova365</w:t></w:r></w:p></w:ftr>");
    }
}

// Style: baza z dokumentu wzorcowego + ujednolicone definicje (numeracja nagłówków, tekst, listy, tabele, pola).
static class Style
{
    const string W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

    static readonly string[] Definicje =
    {
        // tekst podstawowy: 10 pt, czarny, interlinia 1,5, wyjustowany – jak treść analiz źródłowych
        "<w:style w:type=\"paragraph\" w:default=\"1\" w:styleId=\"Normalny\"><w:name w:val=\"Normal\"/><w:qFormat/><w:pPr><w:spacing w:after=\"0\" w:line=\"360\" w:lineRule=\"auto\"/><w:jc w:val=\"both\"/></w:pPr><w:rPr><w:color w:val=\"000000\"/><w:sz w:val=\"20\"/><w:szCs w:val=\"20\"/><w:lang w:val=\"pl-PL\" w:eastAsia=\"pl-PL\"/></w:rPr></w:style>",
        Naglowek(1, 36, 400, 120, true),
        Naglowek(2, 28, 240, 80, false),
        Naglowek(3, 26, 200, 60, false),
        Naglowek(4, 22, 160, 40, false),
        "<w:style w:type=\"paragraph\" w:styleId=\"Listapunktowana\"><w:name w:val=\"List Bullet\"/><w:basedOn w:val=\"Normalny\"/><w:uiPriority w:val=\"1\"/><w:qFormat/><w:pPr><w:numPr><w:numId w:val=\"2\"/></w:numPr><w:spacing w:after=\"60\" w:line=\"288\" w:lineRule=\"auto\"/></w:pPr><w:rPr><w:color w:val=\"404040\"/></w:rPr></w:style>",
        "<w:style w:type=\"paragraph\" w:styleId=\"Legenda\"><w:name w:val=\"caption\"/><w:basedOn w:val=\"Normalny\"/><w:next w:val=\"Normalny\"/><w:uiPriority w:val=\"35\"/><w:unhideWhenUsed/><w:qFormat/><w:pPr><w:spacing w:before=\"60\" w:after=\"200\" w:line=\"240\" w:lineRule=\"auto\"/><w:jc w:val=\"left\"/></w:pPr><w:rPr><w:b/><w:bCs/><w:color w:val=\"404040\"/></w:rPr></w:style>",
        "<w:style w:type=\"paragraph\" w:styleId=\"Nagwekspisutreci\"><w:name w:val=\"TOC Heading\"/><w:basedOn w:val=\"Nagwek1\"/><w:next w:val=\"Normalny\"/><w:uiPriority w:val=\"39\"/><w:unhideWhenUsed/><w:qFormat/><w:pPr><w:numPr><w:ilvl w:val=\"0\"/><w:numId w:val=\"0\"/></w:numPr><w:ind w:left=\"0\" w:firstLine=\"0\"/><w:outlineLvl w:val=\"9\"/></w:pPr></w:style>",
        Spis(1, 0, true, 22), Spis(2, 440, false, 20), Spis(3, 880, false, 20),
        "<w:style w:type=\"paragraph\" w:styleId=\"Spisilustracji\"><w:name w:val=\"table of figures\"/><w:basedOn w:val=\"Normalny\"/><w:next w:val=\"Normalny\"/><w:uiPriority w:val=\"99\"/><w:unhideWhenUsed/><w:pPr><w:tabs><w:tab w:val=\"right\" w:leader=\"dot\" w:pos=\"9017\"/></w:tabs><w:spacing w:after=\"40\" w:line=\"240\" w:lineRule=\"auto\"/><w:jc w:val=\"left\"/></w:pPr></w:style>",
        // nowe style wzoru
        "<w:style w:type=\"paragraph\" w:customStyle=\"1\" w:styleId=\"Tekstabeli\"><w:name w:val=\"Tekst tabeli\"/><w:basedOn w:val=\"Normalny\"/><w:qFormat/><w:pPr><w:spacing w:before=\"40\" w:after=\"40\" w:line=\"240\" w:lineRule=\"auto\"/><w:jc w:val=\"left\"/></w:pPr><w:rPr><w:color w:val=\"404040\"/><w:sz w:val=\"18\"/><w:szCs w:val=\"18\"/></w:rPr></w:style>",
        "<w:style w:type=\"paragraph\" w:customStyle=\"1\" w:styleId=\"Metryka\"><w:name w:val=\"Metryka dokumentu\"/><w:basedOn w:val=\"Normalny\"/><w:qFormat/><w:pPr><w:spacing w:before=\"40\" w:after=\"40\" w:line=\"300\" w:lineRule=\"auto\"/><w:jc w:val=\"left\"/></w:pPr><w:rPr><w:sz w:val=\"20\"/><w:szCs w:val=\"20\"/></w:rPr></w:style>",
        "<w:style w:type=\"paragraph\" w:customStyle=\"1\" w:styleId=\"Tytulanalizy\"><w:name w:val=\"Tytuł analizy\"/><w:basedOn w:val=\"Normalny\"/><w:qFormat/><w:pPr><w:spacing w:before=\"120\" w:after=\"120\" w:line=\"240\" w:lineRule=\"auto\"/><w:jc w:val=\"center\"/></w:pPr><w:rPr><w:color w:val=\"306785\"/><w:spacing w:val=\"-7\"/><w:sz w:val=\"48\"/><w:szCs w:val=\"48\"/></w:rPr></w:style>",
        "<w:style w:type=\"paragraph\" w:customStyle=\"1\" w:styleId=\"Obszaranalizy\"><w:name w:val=\"Obszar analizy\"/><w:basedOn w:val=\"Tytulanalizy\"/><w:qFormat/><w:rPr><w:sz w:val=\"40\"/><w:szCs w:val=\"40\"/></w:rPr></w:style>",
        "<w:style w:type=\"paragraph\" w:customStyle=\"1\" w:styleId=\"Klauzula\"><w:name w:val=\"Klauzula poufności\"/><w:basedOn w:val=\"Normalny\"/><w:qFormat/><w:pPr><w:spacing w:before=\"240\" w:line=\"240\" w:lineRule=\"auto\"/></w:pPr><w:rPr><w:i/><w:color w:val=\"595959\"/><w:sz w:val=\"16\"/><w:szCs w:val=\"16\"/></w:rPr></w:style>",
        "<w:style w:type=\"paragraph\" w:customStyle=\"1\" w:styleId=\"Sekcjaopcjonalna\"><w:name w:val=\"Sekcja opcjonalna\"/><w:basedOn w:val=\"Normalny\"/><w:next w:val=\"Normalny\"/><w:qFormat/><w:pPr><w:keepNext/><w:shd w:val=\"clear\" w:color=\"auto\" w:fill=\"F2F2F2\"/><w:spacing w:before=\"60\" w:after=\"60\" w:line=\"240\" w:lineRule=\"auto\"/><w:jc w:val=\"left\"/></w:pPr><w:rPr><w:b/><w:i/><w:color w:val=\"7F7F7F\"/><w:sz w:val=\"18\"/><w:szCs w:val=\"18\"/></w:rPr></w:style>",
        "<w:style w:type=\"character\" w:customStyle=\"1\" w:styleId=\"PoledoUzupelnienia\"><w:name w:val=\"Pole do uzupełnienia\"/><w:uiPriority w:val=\"1\"/><w:qFormat/><w:rPr><w:shd w:val=\"clear\" w:color=\"auto\" w:fill=\"FFFF00\"/></w:rPr></w:style>",
    };

    static string Naglowek(int n, int sz, int przed, int po, bool linia) =>
        $"<w:style w:type=\"paragraph\" w:styleId=\"Nagwek{n}\"><w:name w:val=\"heading {n}\"/><w:basedOn w:val=\"Normalny\"/><w:next w:val=\"Normalny\"/><w:link w:val=\"Nagwek{n}Znak\"/><w:uiPriority w:val=\"9\"/>{(n > 1 ? "<w:unhideWhenUsed/>" : "")}<w:qFormat/>" +
        $"<w:pPr><w:keepNext/><w:keepLines/><w:numPr><w:ilvl w:val=\"{n - 1}\"/><w:numId w:val=\"1\"/></w:numPr>" +
        (linia ? "<w:pBdr><w:bottom w:val=\"single\" w:sz=\"4\" w:space=\"1\" w:color=\"418AB3\"/></w:pBdr>" : "") +
        $"<w:spacing w:before=\"{przed}\" w:after=\"{po}\" w:line=\"240\" w:lineRule=\"auto\"/><w:jc w:val=\"left\"/><w:outlineLvl w:val=\"{n - 1}\"/></w:pPr>" +
        $"<w:rPr><w:color w:val=\"306785\"/><w:sz w:val=\"{sz}\"/><w:szCs w:val=\"{sz}\"/></w:rPr></w:style>";

    static string Spis(int n, int wciecie, bool pogrubiony, int sz) =>
        $"<w:style w:type=\"paragraph\" w:styleId=\"Spistreci{n}0\"><w:name w:val=\"toc {n}\"/><w:basedOn w:val=\"Normalny\"/><w:next w:val=\"Normalny\"/><w:autoRedefine/><w:uiPriority w:val=\"39\"/><w:unhideWhenUsed/>" +
        $"<w:pPr><w:tabs><w:tab w:val=\"right\" w:leader=\"dot\" w:pos=\"9737\"/></w:tabs><w:spacing w:before=\"{(n == 1 ? 120 : 0)}\" w:after=\"60\" w:line=\"240\" w:lineRule=\"auto\"/><w:ind w:left=\"{wciecie}\"/><w:jc w:val=\"left\"/></w:pPr>" +
        $"<w:rPr>{(pogrubiony ? "<w:b/>" : "")}<w:sz w:val=\"{sz}\"/><w:szCs w:val=\"{sz}\"/></w:rPr></w:style>";

    public static string Zbuduj(string bazowe)
    {
        var x = XDocument.Load(bazowe);
        var root = x.Root!;
        XNamespace w = W;
        foreach (var def in Definicje)
        {
            var nowy = XElement.Parse(def.Replace("<w:style ", $"<w:style xmlns:w=\"{W}\" "));
            var id = nowy.Attribute(w + "styleId")!.Value;
            var stary = root.Elements(w + "style").FirstOrDefault(s => (string)s.Attribute(w + "styleId") == id);
            if (stary != null) stary.ReplaceWith(nowy); else root.Add(nowy);
        }
        // znaczniki zmian (rsid) z dokumentu źródłowego są zbędne
        foreach (var r in root.Descendants(w + "rsid").ToList()) r.Remove();
        return x.Declaration + "\r\n" + x.Root.ToString(SaveOptions.DisableFormatting);
    }

    public static string Numeracja =>
        $"<w:numbering xmlns:w=\"{W}\">" +
        "<w:abstractNum w:abstractNumId=\"0\"><w:multiLevelType w:val=\"multilevel\"/>" +
        PoziomNaglowka(0, "%1.", 567) + PoziomNaglowka(1, "%1.%2.", 709) + PoziomNaglowka(2, "%1.%2.%3.", 851) + PoziomNaglowka(3, "%1.%2.%3.%4.", 992) +
        string.Concat(Enumerable.Range(4, 5).Select(i => $"<w:lvl w:ilvl=\"{i}\"><w:start w:val=\"1\"/><w:numFmt w:val=\"decimal\"/><w:lvlText w:val=\"%{i + 1}.\"/><w:lvlJc w:val=\"left\"/><w:pPr><w:ind w:left=\"{1134 + 142 * (i - 4)}\" w:hanging=\"1134\"/></w:pPr></w:lvl>")) +
        "</w:abstractNum>" +
        "<w:abstractNum w:abstractNumId=\"1\"><w:multiLevelType w:val=\"hybridMultilevel\"/>" +
        "<w:lvl w:ilvl=\"0\"><w:start w:val=\"1\"/><w:numFmt w:val=\"bullet\"/><w:pStyle w:val=\"Listapunktowana\"/><w:lvlText w:val=\"\"/><w:lvlJc w:val=\"left\"/><w:pPr><w:ind w:left=\"360\" w:hanging=\"288\"/></w:pPr><w:rPr><w:rFonts w:ascii=\"Symbol\" w:hAnsi=\"Symbol\" w:hint=\"default\"/><w:color w:val=\"418AB3\"/></w:rPr></w:lvl>" +
        "<w:lvl w:ilvl=\"1\"><w:start w:val=\"1\"/><w:numFmt w:val=\"bullet\"/><w:lvlText w:val=\"–\"/><w:lvlJc w:val=\"left\"/><w:pPr><w:ind w:left=\"720\" w:hanging=\"288\"/></w:pPr><w:rPr><w:color w:val=\"418AB3\"/></w:rPr></w:lvl>" +
        "</w:abstractNum>" +
        "<w:num w:numId=\"1\"><w:abstractNumId w:val=\"0\"/></w:num><w:num w:numId=\"2\"><w:abstractNumId w:val=\"1\"/></w:num></w:numbering>";

    static string PoziomNaglowka(int ilvl, string tekst, int wciecie) =>
        $"<w:lvl w:ilvl=\"{ilvl}\"><w:start w:val=\"1\"/><w:numFmt w:val=\"decimal\"/><w:pStyle w:val=\"Nagwek{ilvl + 1}\"/><w:lvlText w:val=\"{tekst}\"/><w:lvlJc w:val=\"left\"/>" +
        $"<w:pPr><w:ind w:left=\"{wciecie}\" w:hanging=\"{wciecie}\"/></w:pPr></w:lvl>";
}

// Kontrola gotowych wzorów: style istnieją, nagłówki mają ciągłą hierarchię, brak zakazanych słów (nazwy klientów, nazwiska).
static class Kontrola
{
    static readonly XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

    public static bool Sprawdz(List<string> pliki, string zakazane)
    {
        var slowa = File.Exists(zakazane)
            ? File.ReadAllLines(zakazane, Encoding.UTF8).Select(s => s.Trim()).Where(s => s.Length > 0 && !s.StartsWith("#")).ToList()
            : new List<string>();
        Console.WriteLine($"\nKontrola ({(slowa.Count > 0 ? slowa.Count + " zakazanych słów" : "BRAK listy zakazanych słów")}):");
        bool ok = true;
        foreach (var plik in pliki)
        {
            using var zip = ZipFile.OpenRead(plik);
            XDocument Wczytaj(string n) { using var s = zip.GetEntry(n)!.Open(); return XDocument.Load(s); }
            var dok = Wczytaj("word/document.xml");
            var style = Wczytaj("word/styles.xml").Root!.Elements(w + "style").Select(s => (string)s.Attribute(w + "styleId")).ToHashSet();
            var bledy = new List<string>();

            var uzyte = dok.Descendants(w + "pStyle").Concat(dok.Descendants(w + "rStyle")).Concat(dok.Descendants(w + "tblStyle"))
                           .Select(e => (string)e.Attribute(w + "val")).Distinct();
            bledy.AddRange(uzyte.Where(s => !style.Contains(s)).Select(s => "brak stylu " + s));

            // hierarchia nagłówków: poziom n+1 tylko po poziomie n; numeracja wyliczana jak w Wordzie
            var licz = new int[5];
            var spis = new List<string>();
            foreach (var p in dok.Descendants(w + "p"))
            {
                var st = (string)p.Element(w + "pPr")?.Element(w + "pStyle")?.Attribute(w + "val");
                if (st == null || !Regex.IsMatch(st, "^Nagwek[1-4]$")) continue;
                int n = st[6] - '0';
                if (n > 1 && licz[n - 1] == 0) bledy.Add($"nagłówek poziomu {n} bez nadrzędnego: {Tekst(p)}");
                licz[n]++;
                for (int k = n + 1; k < licz.Length; k++) licz[k] = 0;
                spis.Add(string.Join(".", licz.Skip(1).Take(n)) + ". " + Tekst(p));
            }

            // akapity rozdzielane znakiem nowej linii – inaczej sklejenie sąsiednich akapitów daje fałszywe trafienia
            var caly = string.Join("\n", dok.Descendants(w + "p").Select(Tekst)) + "\n" + string.Concat(zip.Entries.Where(e => e.FullName.StartsWith("word/header") || e.FullName.StartsWith("word/footer") || e.FullName.StartsWith("docProps/"))
                                                                .Select(e => { using var r = new StreamReader(e.Open()); return r.ReadToEnd(); }));
            foreach (var s in slowa)
                if (caly.Contains(s, StringComparison.OrdinalIgnoreCase)) bledy.Add("zakazane słowo: " + s);
            var pola = Regex.Matches(caly, @"\[DO UZUPEŁNIENIA:").Count;
            var opcjonalne = Regex.Matches(caly, Regex.Escape(Dokument.ZnacznikOpcjonalnej)).Count;

            Console.WriteLine($"- {Path.GetFileName(plik)}: nagłówki {spis.Count}, pola {pola}, sekcje opcjonalne {opcjonalne}, " +
                              (bledy.Count == 0 ? "OK" : "BŁĘDY:\n    " + string.Join("\n    ", bledy)));
            // _robocze jest poza gitem – na nowym środowisku katalogu może nie być
            var robocze = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(plik)!, "_robocze")).FullName;
            File.WriteAllLines(Path.Combine(robocze, Path.GetFileNameWithoutExtension(plik) + ".spis.txt"), spis, Encoding.UTF8);
            ok &= bledy.Count == 0;
        }
        return ok;
    }

    static string Tekst(XElement e) => string.Concat(e.Descendants(w + "t").Select(t => t.Value));
}
