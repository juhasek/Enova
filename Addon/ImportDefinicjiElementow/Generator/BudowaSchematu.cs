using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ImportDefinicjiElementow.Generator;

/// <summary>
/// Scala dane z DLL (zakładki, typy, enumy, słowniki) z opisami ręcznymi w jeden schemat JSON.
/// </summary>
sealed class BudowaSchematu
{
    readonly Konfiguracja konf;
    readonly Opisy opisy;
    readonly TypeInspector insp;
    readonly Dictionary<string, string> zasobyLogiki;
    readonly string wersjaEnova;

    public readonly List<string> Ostrzezenia = new();
    public readonly List<string> DoWeryfikacji = new();
    public readonly List<string> NoweSzkielety = new();
    /// <summary>Czy dopisano szkielety do opisy.json (tylko wtedy plik jest zapisywany ponownie).</summary>
    public bool OpisyZmienione;
    readonly JsonObject pola = new();
    readonly JsonArray pominiete = new();
    readonly HashSet<string> uzyteEnumy = new();
    readonly HashSet<string> uzyteSlowniki = new();
    readonly Dictionary<string, string> kluczeEnumow = new();

    public BudowaSchematu(Konfiguracja konf, Opisy opisy, TypeInspector insp, Dictionary<string, string> zasobyLogiki, string wersjaEnova)
    {
        this.konf = konf;
        this.opisy = opisy;
        this.insp = insp;
        this.zasobyLogiki = zasobyLogiki;
        this.wersjaEnova = wersjaEnova;
    }

    public JsonObject Buduj(List<FormZakladka> zakladki)
    {
        var jZakladki = new JsonArray();
        foreach (var zak in zakladki)
        {
            // zakładki i sekcje bez wpisu w opisy.json biorą etykiety z enova - szkieletów nie dopisujemy
            if (!opisy.Zakladki.TryGetValue(zak.Id, out var oz))
                oz = new OpisZakladki();
            if (oz.Pomin != null)
            {
                pominiete.Add(new JsonObject { ["sciezka"] = "zakładka " + zak.Etykieta, ["etykieta"] = zak.Etykieta, ["powod"] = oz.Pomin });
                continue;
            }
            var jSekcje = new JsonArray();
            var dodatkowe = konf.PolaDodatkowe.Where(p => p.Zakladka == zak.Id).ToList();
            if (dodatkowe.Count > 0 && zak.Sekcje.Count > 0)
            {
                var pierwsza = zak.Sekcje[0];
                for (int i = dodatkowe.Count - 1; i >= 0; i--)
                    pierwsza.Pola.Insert(0, new FormPole { Sciezka = dodatkowe[i].Sciezka, Etykieta = dodatkowe[i].Etykieta });
            }
            foreach (var sek in zak.Sekcje)
            {
                var klucz = zak.Id + "/" + sek.Etykieta;
                if (!opisy.Sekcje.TryGetValue(klucz, out var os))
                    os = new OpisSekcji();
                if (os.Pomin != null)
                {
                    pominiete.Add(new JsonObject { ["sciezka"] = "sekcja " + klucz, ["etykieta"] = sek.Etykieta, ["powod"] = os.Pomin });
                    continue;
                }
                var jPola = new JsonArray();
                foreach (var pole in sek.Pola)
                {
                    var sc = Kanoniczna(pole.Sciezka);
                    var powod = Pominiete(sc);
                    if (powod != null)
                    {
                        if (!pominiete.Any(p => p["sciezka"]?.GetValue<string>() == sc))
                            pominiete.Add(new JsonObject { ["sciezka"] = sc, ["etykieta"] = pole.Etykieta, ["powod"] = powod });
                        continue;
                    }
                    if (!pola.ContainsKey(sc))
                    {
                        var jp = BudujPole(sc, pole);
                        if (jp == null)
                            continue;
                        pola[sc] = jp;
                    }
                    if (!jPola.Any(n => n.GetValue<string>() == sc))
                        jPola.Add(sc);
                }
                if (jPola.Count == 0)
                    continue;
                var regulySekcji = new JsonArray();
                foreach (var r in sek.WidocznoscUI)
                    regulySekcji.Add("UI: " + r);
                if (!string.IsNullOrEmpty(sek.Kontekst))
                {
                    var info = insp.Rozwin(sek.Kontekst);
                    foreach (var r in info.RegulyWKodzie.Where(r => r.Contains(".IsVisible")))
                        regulySekcji.Add(r.Replace(" (grupa)", ""));
                }
                jSekcje.Add(new JsonObject
                {
                    ["id"] = klucz,
                    ["etykieta"] = os.Etykieta ?? OstatniSegment(sek.Etykieta),
                    ["etykietaEnova"] = sek.Etykieta,
                    ["opis"] = os.Opis,
                    ["widocznaGdy"] = os.WidocznaGdy?.DeepClone(),
                    ["regulyEnova"] = regulySekcji.Count > 0 ? regulySekcji : null,
                    ["pola"] = jPola,
                });
            }
            if (jSekcje.Count == 0)
                continue;
            jZakladki.Add(new JsonObject
            {
                ["id"] = zak.Id,
                ["etykieta"] = oz.Etykieta ?? zak.Etykieta,
                ["etykietaEnova"] = zak.Etykieta,
                ["zasobEnova"] = zak.Zasob + ".pageform.xml",
                ["opis"] = oz.Opis,
                ["licencja"] = zak.Licencja,
                ["widocznaGdy"] = oz.WidocznaGdy?.DeepClone(),
                ["regulyEnova"] = zak.WidocznoscUI == null ? null : new JsonArray("UI: " + zak.WidocznoscUI),
                ["sekcje"] = jSekcje,
            });
        }

        foreach (var sc in opisy.Pola.Keys.Where(k => !pola.ContainsKey(k) && !pominiete.Any(p => p["sciezka"]?.GetValue<string>() == k)))
            Ostrzezenia.Add($"opisy.json: pole {sc} nie występuje już na formularzach enova (usunięte/przeniesione?).");

        return new JsonObject
        {
            ["wersjaSchematu"] = konf.WersjaSchematu,
            ["wersjaEnova"] = wersjaEnova,
            ["wygenerowano"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["obiekt"] = konf.TypObiektu,
            ["zakladki"] = jZakladki,
            ["pola"] = pola,
            ["enumy"] = Enumy(),
            ["slowniki"] = Slowniki(),
            ["definicjeStandardowe"] = DefinicjeStandardowe(),
            ["szablony"] = opisy.Szablony?.DeepClone() ?? new JsonArray(),
            ["pominiete"] = pominiete,
        };
    }

    JsonObject BudujPole(string sc, FormPole pole)
    {
        var info = insp.Rozwin(sc);
        if (!info.Znalezione)
        {
            Ostrzezenia.Add($"Pole {sc} ({pole.Etykieta}): {info.Blad} - pominięte.");
            return null;
        }
        var lista = pole.Kolumny != null;
        if (!info.Zapisywalne && !lista && !pole.Sciezka.Equals("RodzajZrodla"))
        {
            pominiete.Add(new JsonObject { ["sciezka"] = sc, ["etykieta"] = pole.Etykieta, ["powod"] = "tylko do odczytu w enova (wyliczane)" });
            return null;
        }
        if (!opisy.Pola.TryGetValue(sc, out var op))
        {
            opisy.Pola[sc] = op = new OpisPola { OpisDoWeryfikacji = true };
            NoweSzkielety.Add(sc);
            OpisyZmienione = true;
        }
        if (op.OpisDoWeryfikacji == true || string.IsNullOrWhiteSpace(op.Opis))
            DoWeryfikacji.Add(sc);

        var j = new JsonObject
        {
            ["sciezka"] = sc,
            ["etykieta"] = op.Etykieta ?? pole.Etykieta ?? OstatniSegment(sc),
            ["etykietaEnova"] = pole.Etykieta,
            ["typ"] = lista ? "lista" : info.Typ,
        };
        if (info.Enum != null)
        {
            j["enum"] = KluczEnuma(info.Enum);
            uzyteEnumy.Add(info.Enum);
        }
        if (info.Typ == "slownik")
        {
            var krotka = info.TypNet.Substring(info.TypNet.LastIndexOf('.') + 1);
            j["slownik"] = krotka;
            uzyteSlowniki.Add(info.TypNet);
        }
        j["wymagane"] = op.Wymagane ?? info.Wymagane;
        if (op.WymaganeGdy != null) j["wymaganeGdy"] = op.WymaganeGdy.DeepClone();
        if (info.MaxDlugosc != null) j["maxDlugosc"] = info.MaxDlugosc;
        var dom = op.Domyslna?.DeepClone() ?? (info.Domyslna == null ? null : JsonValue.Create(info.Domyslna));
        if (dom != null) j["domyslna"] = dom;
        if (op.WidocznyGdy != null) j["widocznyGdy"] = op.WidocznyGdy.DeepClone();
        if (op.EdytowalnyGdy != null) j["edytowalnyGdy"] = op.EdytowalnyGdy.DeepClone();
        if (op.Walidacje != null) j["walidacje"] = op.Walidacje.DeepClone();
        if (op.DozwoloneWartosci != null) j["dozwoloneWartosci"] = op.DozwoloneWartosci.DeepClone();
        if (op.Licencja != null) j["licencja"] = op.Licencja;
        j["opis"] = op.Opis;
        j["podpowiedz"] = op.Podpowiedz;
        if (op.OpisDoWeryfikacji == true || string.IsNullOrWhiteSpace(op.Opis)) j["opisDoWeryfikacji"] = true;
        if (op.DomyslnaDoWeryfikacji == true) j["domyslnaDoWeryfikacji"] = true;
        if (op.ZrodloOpisu != null) j["zrodloOpisu"] = op.ZrodloOpisu;
        if (op.UwagiTechniczne != null) j["uwagiTechniczne"] = op.UwagiTechniczne;
        if (!string.IsNullOrEmpty(pole.OpisUI ?? info.OpisZKodu)) j["opisEnova"] = pole.OpisUI ?? info.OpisZKodu;
        var reguly = new JsonArray();
        foreach (var r in info.RegulyWKodzie.Where(r => !r.EndsWith("(grupa)")))
            reguly.Add(r);
        foreach (var r in pole.WidocznoscUI)
            reguly.Add("UI: " + r);
        if (reguly.Count > 0) j["regulyEnova"] = reguly;
        j["bazaDanych"] = info.BazaDanych;
        j["typEnova"] = info.TypNet;

        if (lista)
        {
            var typEl = insp.TypElementuListy(sc);
            var kol = new JsonArray();
            foreach (var k in pole.Kolumny.Where(k => !k.TylkoOdczytUI && k.Sciezka != null))
            {
                var ik = typEl == null ? null : insp.RozwinNaTypie(typEl, k.Sciezka);
                var jk = new JsonObject { ["sciezka"] = k.Sciezka, ["etykieta"] = k.Etykieta, ["typ"] = ik?.Typ ?? "inny" };
                if (ik?.Enum != null)
                {
                    jk["enum"] = KluczEnuma(ik.Enum);
                    uzyteEnumy.Add(ik.Enum);
                }
                if (ik?.Typ == "slownik")
                {
                    jk["slownik"] = ik.TypNet.Substring(ik.TypNet.LastIndexOf('.') + 1);
                    uzyteSlowniki.Add(ik.TypNet);
                }
                kol.Add(jk);
            }
            j["kolumny"] = kol;
            j["typElementuEnova"] = typEl?.FullName;
        }
        return j;
    }

    JsonObject Enumy()
    {
        var wynik = new JsonObject();
        foreach (var pelna in uzyteEnumy.OrderBy(x => KluczEnuma(x)))
        {
            var e = insp.Enumy[pelna];
            var klucz = KluczEnuma(pelna);
            opisy.Enumy.TryGetValue(klucz, out var oe);
            var wart = new JsonArray();
            foreach (var w in e.Wartosci)
            {
                OpisWartosci ow = null;
                oe?.TryGetValue(w.Nazwa, out ow);
                wart.Add(new JsonObject
                {
                    ["nazwa"] = w.Nazwa,
                    ["wartosc"] = w.Wartosc,
                    ["etykieta"] = ow?.Etykieta ?? w.Etykieta ?? RozdzielWielkieLitery(w.Nazwa),
                    ["etykietaEnova"] = w.Etykieta,
                    ["opis"] = ow?.Opis,
                    ["ukryta"] = (ow?.Ukryj ?? w.Ukryta) ? true : null,
                });
            }
            wynik[klucz] = new JsonObject { ["typEnova"] = pelna, ["wartosci"] = wart };
        }
        return wynik;
    }

    JsonObject Slowniki()
    {
        var wynik = new JsonObject();
        foreach (var pelna in uzyteSlowniki.OrderBy(x => x))
        {
            var krotka = pelna.Substring(pelna.LastIndexOf('.') + 1);
            konf.Slowniki.TryGetValue(krotka, out var ks);
            var j = new JsonObject
            {
                ["typEnova"] = pelna,
                ["etykieta"] = ks?.Etykieta ?? RozdzielWielkieLitery(krotka),
                ["dowolnaWartosc"] = true,
                ["poleKlucza"] = ks?.PoleKlucza ?? "Nazwa",
            };
            if (ks != null && ks.Zasoby.Count > 0)
                j["wartosciStandardowe"] = WartosciSlownika(ks);
            else if (krotka == "DefinicjaElementu")
                j["wartosciStandardowe"] = new JsonArray(DefinicjeStandardowe().Select(d => (JsonNode)new JsonObject { ["klucz"] = d["nazwa"]!.GetValue<string>() }).ToArray());
            wynik[krotka] = j;
        }
        return wynik;
    }

    JsonArray WartosciSlownika(KonfSlownik ks)
    {
        var rekordy = new Dictionary<string, Dictionary<string, string>>();
        var kolejnosc = new List<string>();
        foreach (var suf in ks.Zasoby)
        {
            var nazwa = zasobyLogiki.Keys.FirstOrDefault(k => k.EndsWith("." + suf, StringComparison.Ordinal));
            if (nazwa == null)
            {
                Ostrzezenia.Add($"Słownik {ks.Element}: brak zasobu {suf}.");
                continue;
            }
            foreach (var el in XDocument.Parse(zasobyLogiki[nazwa]).Root.Elements().Where(e => e.Name.LocalName == ks.Element))
            {
                var guid = el.Attribute("guid")?.Value ?? el.Attribute("id")?.Value;
                if (!rekordy.TryGetValue(guid, out var r))
                {
                    rekordy[guid] = r = new Dictionary<string, string>();
                    kolejnosc.Add(guid);
                }
                foreach (var p in el.Elements())
                    r[p.Name.LocalName] = p.Value;
            }
        }
        var wynik = new JsonArray();
        var dzis = DateTime.Today;
        foreach (var g in kolejnosc)
        {
            var r = rekordy[g];
            if (ks.PoleOkresu != null && r.TryGetValue(ks.PoleOkresu, out var okres) && Historyczny(okres, dzis))
                continue;
            if (!r.TryGetValue(ks.PoleKlucza, out var kl) || string.IsNullOrWhiteSpace(kl))
                continue;
            if (wynik.Any(n => n["klucz"]!.GetValue<string>() == kl))
                continue;
            r.TryGetValue(ks.PoleOpisu ?? "", out var opis);
            wynik.Add(new JsonObject { ["klucz"] = kl, ["opis"] = opis });
        }
        return wynik;
    }

    JsonArray definicjeStd;

    JsonArray DefinicjeStandardowe()
    {
        if (definicjeStd != null)
            return (JsonArray)definicjeStd.DeepClone();
        var rekordy = new Dictionary<string, JsonObject>();
        foreach (var suf in konf.DefinicjeStandardowe)
        {
            foreach (var nazwa in zasobyLogiki.Keys.Where(k => Regex.IsMatch(k, suf)))
            {
                foreach (var el in XDocument.Parse(zasobyLogiki[nazwa]).Root.Elements().Where(e => e.Name.LocalName == "DefinicjaElementu"))
                {
                    var guid = el.Attribute("guid")?.Value;
                    if (guid == null)
                        continue;
                    if (!rekordy.TryGetValue(guid, out var r))
                        rekordy[guid] = r = new JsonObject { ["guid"] = guid };
                    foreach (var pole in new[] { "Nazwa", "Skrot", "RodzajZrodla" })
                    {
                        var v = el.Element(el.Name.Namespace + pole)?.Value;
                        if (!string.IsNullOrEmpty(v))
                            r[pole.ToLowerInvariant() == "rodzajzrodla" ? "rodzajZrodla" : pole.ToLowerInvariant()] = v;
                    }
                }
            }
        }
        definicjeStd = new JsonArray(rekordy.Values.Where(r => r["nazwa"] != null).OrderBy(r => r["nazwa"]!.GetValue<string>()).Select(r => (JsonNode)r).ToArray());
        return (JsonArray)definicjeStd.DeepClone();
    }

    static bool Historyczny(string okres, DateTime dzis)
    {
        // format enova: "...", "...2003/09/30", "2004/01/01...", "2004/01/01...2010/12/31"
        var m = Regex.Match(okres ?? "", @"\.\.\.(\d{4}/\d{2}/\d{2})$");
        return m.Success && DateTime.TryParse(m.Groups[1].Value.Replace('/', '-'), out var d) && d < dzis;
    }

    string KluczEnuma(string pelna)
    {
        if (kluczeEnumow.TryGetValue(pelna, out var k))
            return k;
        var krotka = pelna.Substring(Math.Max(pelna.LastIndexOf('.'), pelna.LastIndexOf('+')) + 1);
        if (kluczeEnumow.ContainsValue(krotka))
            krotka = pelna.Replace('+', '.');
        kluczeEnumow[pelna] = krotka;
        return krotka;
    }

    string Kanoniczna(string sc)
    {
        foreach (var kv in konf.Aliasy.OrderByDescending(k => k.Key.Length))
            if (sc == kv.Key || sc.StartsWith(kv.Key + ".", StringComparison.Ordinal))
                return kv.Value + sc.Substring(kv.Key.Length);
        return sc;
    }

    string Pominiete(string sc)
    {
        foreach (var kv in konf.Pominiete)
        {
            if (kv.Key == sc)
                return kv.Value;
            if (kv.Key.EndsWith(".*") && (sc + ".").StartsWith(kv.Key.Substring(0, kv.Key.Length - 1), StringComparison.Ordinal))
                return kv.Value;
        }
        return null;
    }

    static string OstatniSegment(string s) => s?.Split(new[] { " / ", "." }, StringSplitOptions.None).Last();

    static string RozdzielWielkieLitery(string s) =>
        string.IsNullOrEmpty(s) ? s : Regex.Replace(s, @"(?<=\p{Ll})(?=\p{Lu})", " ").ToLowerInvariant() switch { var x => char.ToUpper(x[0]) + x.Substring(1) };
}
