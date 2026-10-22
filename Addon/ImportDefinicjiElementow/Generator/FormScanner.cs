using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ImportDefinicjiElementow.Generator;

/// <summary>Zakładka formularza enova odczytana z *.pageform.xml.</summary>
sealed class FormZakladka
{
    public string Id;
    public string Etykieta;
    public string Zasob;
    public int Priorytet;
    public string Licencja;
    public string WidocznoscUI;
    public List<FormSekcja> Sekcje = new();
}

sealed class FormSekcja
{
    /// <summary>Ścieżka etykiet grup, np. "Algorytm naliczania wartości / Kreator algorytmu / Podstawa".</summary>
    public string Etykieta;
    /// <summary>Ścieżka właściwości grupy (np. Algorytm.KreatorAlgorytmu) - do reguł IsVisible na rodzicu.</summary>
    public string Kontekst;
    public List<string> WidocznoscUI = new();
    public List<FormPole> Pola = new();
}

sealed class FormPole
{
    /// <summary>Ścieżka właściwości od obiektu głównego (po rozwinięciu DataContext/EditValue).</summary>
    public string Sciezka;
    public string Etykieta;
    public string OpisUI;
    public List<string> WidocznoscUI = new();
    public bool TylkoOdczytUI;
    /// <summary>Kolumny listy (Grid): ścieżki względem elementu listy.</summary>
    public List<FormPole> Kolumny;
}

/// <summary>
/// Odczyt zakładek, sekcji i pól z zasobów *.pageform.xml / *.form.xml biblioteki UI.
/// Rozwija DataContext/EditValue grup do pełnych ścieżek od obiektu głównego i dołącza Include.
/// </summary>
sealed class FormScanner
{
    static readonly XNamespace Ns = "http://www.enova.pl/schema/form.xsd";
    readonly Dictionary<string, string> zasoby;
    readonly Konfiguracja konf;
    public readonly List<string> Ostrzezenia = new();

    public FormScanner(Dictionary<string, string> zasobyUI, Konfiguracja konf)
    {
        zasoby = zasobyUI;
        this.konf = konf;
    }

    public List<FormZakladka> Skanuj()
    {
        var wynik = new List<FormZakladka>();
        foreach (var kz in konf.Zakladki)
        {
            var nazwa = zasoby.Keys.FirstOrDefault(k => k.EndsWith("." + kz.Zasob + ".pageform.xml", StringComparison.Ordinal));
            if (nazwa == null)
            {
                Ostrzezenia.Add($"Brak zasobu zakładki {kz.Zasob}.pageform.xml - zakładka pominięta (zmiana w wersji enova?).");
                continue;
            }
            var doc = XDocument.Parse(zasoby[nazwa]);
            var root = doc.Root;
            var page = root.Element(Ns + "Page");
            var zak = new FormZakladka
            {
                Id = kz.Id,
                Zasob = kz.Zasob,
                Etykieta = kz.Etykieta ?? Tekst(page?.Attribute("CaptionHtml")?.Value),
                Priorytet = int.TryParse(root.Attribute("Priority")?.Value, out var p) ? p : 0,
                Licencja = root.Attribute("Contexts")?.Value?.Replace("License.", ""),
                WidocznoscUI = page?.Attribute("Visibility")?.Value,
            };
            var ctx = Rozwin("", page?.Attribute("DataContext")?.Value) ?? "";
            var sekcja = new FormSekcja { Etykieta = zak.Etykieta, Kontekst = ctx };
            zak.Sekcje.Add(sekcja);
            Przejdz(page, zak, ctx, new List<string>(), new List<string>(), sekcja);
            zak.Sekcje.RemoveAll(s => s.Pola.Count == 0);
            wynik.Add(zak);
        }
        return wynik;
    }

    void Przejdz(XElement el, FormZakladka zak, string ctx, List<string> grupy, List<string> widocznosc, FormSekcja sekcja)
    {
        foreach (var dz in el.Elements())
        {
            var nazwa = dz.Name.LocalName;
            var vis = dz.Attribute("Visibility")?.Value;
            var widocznoscDz = vis == null ? widocznosc : new List<string>(widocznosc) { vis };
            switch (nazwa)
            {
                case "Field":
                {
                    var ev = dz.Attribute("EditValue")?.Value;
                    var sc = Rozwin(ctx, ev);
                    if (sc == null)
                    {
                        Ostrzezenia.Add($"[{zak.Zasob}] pole '{Tekst(dz.Attribute("CaptionHtml")?.Value)}': nierozwinięte wyrażenie {ev}");
                        break;
                    }
                    sekcja.Pola.Add(new FormPole
                    {
                        Sciezka = sc,
                        Etykieta = Tekst(dz.Attribute("CaptionHtml")?.Value),
                        OpisUI = Tekst(dz.Attribute("DescriptionHtml")?.Value),
                        WidocznoscUI = widocznoscDz.Skip(sekcja.WidocznoscUI.Count).ToList(),
                        TylkoOdczytUI = dz.Attribute("IsReadOnly")?.Value == "true",
                    });
                    break;
                }
                case "Grid":
                {
                    var sc = Rozwin(ctx, dz.Attribute("EditValue")?.Value);
                    if (sc == null)
                    {
                        Ostrzezenia.Add($"[{zak.Zasob}] lista: nierozwinięte wyrażenie {dz.Attribute("EditValue")?.Value}");
                        break;
                    }
                    var pole = new FormPole
                    {
                        Sciezka = sc,
                        Etykieta = grupy.LastOrDefault() ?? zak.Etykieta,
                        WidocznoscUI = widocznoscDz.Skip(sekcja.WidocznoscUI.Count).ToList(),
                        TylkoOdczytUI = dz.Attribute("IsReadOnly")?.Value == "true",
                        Kolumny = new List<FormPole>(),
                    };
                    foreach (var kol in dz.Elements(Ns + "Field"))
                        pole.Kolumny.Add(new FormPole
                        {
                            Sciezka = Rozwin("", kol.Attribute("EditValue")?.Value),
                            Etykieta = Tekst(kol.Attribute("CaptionHtml")?.Value),
                            TylkoOdczytUI = kol.Attribute("IsReadOnly")?.Value == "true",
                        });
                    sekcja.Pola.Add(pole);
                    break;
                }
                case "Include":
                {
                    var src = dz.Attribute("Source")?.Value;
                    var klucz = src == null ? null : zasoby.Keys.FirstOrDefault(k => k.EndsWith("." + src, StringComparison.Ordinal));
                    if (klucz == null)
                    {
                        Ostrzezenia.Add($"[{zak.Zasob}] Include {src}: brak zasobu w bibliotece UI - pominięty.");
                        break;
                    }
                    var dcInc = dz.Attribute("DataContext")?.Value ?? dz.Attribute("EditValue")?.Value;
                    var ctxInc = dcInc != null ? Rozwin(ctx, dcInc) : ctx;
                    if (ctxInc == null)
                        break;
                    var inc = XDocument.Parse(zasoby[klucz]).Root;
                    Przejdz(inc, zak, ctxInc, grupy, widocznoscDz, sekcja);
                    break;
                }
                case "Group":
                case "Stack":
                case "Row":
                case "Panel":
                {
                    var ctxDz = ctx;
                    var dc = dz.Attribute("DataContext")?.Value ?? dz.Attribute("EditValue")?.Value;
                    if (dc != null)
                    {
                        ctxDz = Rozwin(ctx, dc);
                        if (ctxDz == null)
                        {
                            Ostrzezenia.Add($"[{zak.Zasob}] grupa '{Tekst(dz.Attribute("CaptionHtml")?.Value)}': nierozwinięte wyrażenie {dc} - pominięta.");
                            break;
                        }
                    }
                    var cap = nazwa == "Group" ? Tekst(dz.Attribute("CaptionHtml")?.Value) : null;
                    if (!string.IsNullOrEmpty(cap))
                    {
                        var grupyDz = new List<string>(grupy) { cap };
                        var nowa = new FormSekcja
                        {
                            Etykieta = string.Join(" / ", grupyDz),
                            Kontekst = ctxDz,
                            WidocznoscUI = widocznoscDz,
                        };
                        zak.Sekcje.Add(nowa);
                        Przejdz(dz, zak, ctxDz, grupyDz, widocznoscDz, nowa);
                        // pola po zamknięciu grupy wracają do sekcji nadrzędnej - nowa sekcja o tej samej nazwie
                        var powrot = new FormSekcja { Etykieta = sekcja.Etykieta, Kontekst = sekcja.Kontekst, WidocznoscUI = sekcja.WidocznoscUI };
                        zak.Sekcje.Add(powrot);
                        sekcja = powrot;
                    }
                    else
                    {
                        Przejdz(dz, zak, ctxDz, grupy, widocznoscDz, sekcja);
                    }
                    break;
                }
            }
        }
    }

    /// <summary>"{A.B}" w kontekście "X" → "X.A.B"; "{DataSource}" → kontekst; wyrażenia "new ..." wg konfiguracji.</summary>
    string Rozwin(string ctx, string wyr)
    {
        if (string.IsNullOrEmpty(wyr))
            return ctx;
        var m = Regex.Match(wyr.Trim(), @"^\{(.*)\}$");
        if (!m.Success)
            return null;
        var w = m.Groups[1].Value.Trim();
        if (w == "DataSource")
            return ctx;
        if (w.StartsWith("new ", StringComparison.OrdinalIgnoreCase))
        {
            var klucz = "new " + w.Substring(4).Trim();
            foreach (var kv in konf.Wyrazenia)
                if (string.Equals(kv.Key, klucz, StringComparison.OrdinalIgnoreCase))
                    return kv.Value;
            return null;
        }
        if (w.Contains('(') || w.Contains(' '))
            return null;
        return string.IsNullOrEmpty(ctx) ? w : ctx + "." + w;
    }

    static string Tekst(string html) => html == null ? null : WebUtility.HtmlDecode(Regex.Replace(html, "<.*?>", "")).Trim();
}
