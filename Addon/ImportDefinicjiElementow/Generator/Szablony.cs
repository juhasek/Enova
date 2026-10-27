using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ImportDefinicjiElementow.Generator;

/// <summary>
/// Szablony startowe formularza budowane z definicji standardowych enova (zasoby *.dbinit.xml).
/// Rekord w dbinit jest zapisem pól bazodanowych - część pól formularza to nakładki na inne pola rekordu,
/// dlatego wartości przechodzą przez przekształcenia poniżej (sprawdzone w kodzie 2610).
/// </summary>
static class Szablony
{
    public static JsonArray Buduj(Konfiguracja konf, Dictionary<string, string> zasobyLogiki, JsonObject pola, JsonObject slowniki, List<string> ostrzezenia,
        List<KonfSzablon> lista = null)
    {
        lista ??= konf.Szablony;
        var wynik = new JsonArray();
        if (lista.Count == 0)
            return wynik;
        var rekordy = RekordyStandardowe(konf, zasobyLogiki);
        foreach (var ks in lista)
        {
            if (!rekordy.TryGetValue(ks.ZrodloGuid, out var liscie))
            {
                ostrzezenia.Add($"Szablon {ks.Id}: brak definicji standardowej {ks.ZrodloGuid} w zasobach.");
                continue;
            }
            var wartosci = new JsonObject();
            var pominiete = new List<string>();
            foreach (var (sciezka, wartosc) in Przeksztalc(liscie))
            {
                if (string.IsNullOrEmpty(wartosc))
                    continue;
                if (pola[sciezka] is not JsonObject pole)
                {
                    pominiete.Add(sciezka);
                    continue;
                }
                var v = Wartosc(pole, wartosc, slowniki);
                if (v != null)
                    wartosci[sciezka] = v;
            }
            foreach (var kv in ks.Nadpisz)
                wartosci[kv.Key] = kv.Value?.DeepClone();
            wynik.Add(new JsonObject
            {
                ["id"] = ks.Id,
                ["etykieta"] = ks.Etykieta,
                ["opis"] = ks.Opis,
                ["uwagi"] = ks.Uwagi,
                ["zrodlo"] = $"definicja standardowa enova „{liscie.GetValueOrDefault("Nazwa")}” ({ks.ZrodloGuid})",
                ["wartosci"] = wartosci,
                ["polaRekorduBezOdpowiednika"] = new JsonArray(pominiete.Where(p => !Techniczne(p)).Select(p => (JsonNode)p).ToArray()),
            });
        }
        return wynik;
    }

    /// <summary>guid → (ścieżka liścia → wartość), z nałożonymi kolejnymi wersjami (dbversion) tego samego rekordu.</summary>
    static Dictionary<string, Dictionary<string, string>> RekordyStandardowe(Konfiguracja konf, Dictionary<string, string> zasoby)
    {
        var wynik = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var wzorzec in konf.DefinicjeStandardowe)
            foreach (var nazwa in zasoby.Keys.Where(k => Regex.IsMatch(k, wzorzec)).OrderBy(k => k, StringComparer.Ordinal))
                foreach (var el in XDocument.Parse(zasoby[nazwa]).Root.Elements().Where(e => e.Name.LocalName == "DefinicjaElementu"))
                {
                    var guid = el.Attribute("guid")?.Value;
                    if (guid == null)
                        continue;
                    if (!wynik.TryGetValue(guid, out var r))
                        wynik[guid] = r = new Dictionary<string, string>();
                    Splaszcz(el, "", r);
                }
        return wynik;
    }

    static void Splaszcz(XElement el, string prefiks, Dictionary<string, string> wynik)
    {
        foreach (var dz in el.Elements())
        {
            var sc = prefiks + dz.Name.LocalName;
            if (sc == "Progi")
            {
                // progi stażu: lista wierszy ProgStazuPracy - zapis "prog=wartość;..."
                wynik[sc] = string.Join(";", dz.Elements().Select(p => p.Element(p.Name.Namespace + "Prog")?.Value + "=" + p.Element(p.Name.Namespace + "Wartosc")?.Value));
                continue;
            }
            if (dz.HasElements)
                Splaszcz(dz, sc + ".", wynik);
            else
                wynik[sc] = dz.Value;
        }
    }

    /// <summary>Pola rekordu → pola formularza (nakładki właściwości DefinicjaElementu w 2610).</summary>
    static IEnumerable<(string, string)> Przeksztalc(Dictionary<string, string> r)
    {
        string Pobierz(string k) => r.GetValueOrDefault(k) ?? "";
        bool Tak(string k) => string.Equals(Pobierz(k), "True", StringComparison.OrdinalIgnoreCase);
        var pominSurowe = new HashSet<string>
        {
            "Algorytm.Podstawa", "Algorytm.Czas", "Algorytm.Ulamek", "Algorytm.Liczba", "Algorytm.Dni",
            "Deklaracje.NieWliczajSOD", "Deklaracje.NieRozrzucaj",
            "Algorytm.KreatorAlgorytmu.Korekta.WybraneOdchylki", "Algorytm.KreatorAlgorytmu.Korekta.OdchylkiPlus",
            "Algorytm.KreatorAlgorytmu.Korekta.WybraneOdchylkiPlus", "Progi",
        };
        foreach (var kv in r)
            if (!pominSurowe.Contains(kv.Key))
                yield return (kv.Key, kv.Value);

        // DefinicjaElementu.Progi => KreatorAlgorytmu.Wspolczynnik.Progi (ta sama podlista)
        if (!string.IsNullOrEmpty(Pobierz("Progi"))) yield return ("Algorytm.KreatorAlgorytmu.Wspolczynnik.Progi", Pobierz("Progi"));
        // KreatorAlgorytmu.Podstawa / Czas.Czas => Algorytm.Podstawa / Algorytm.Czas
        if (r.ContainsKey("Algorytm.Podstawa")) yield return ("Algorytm.KreatorAlgorytmu.Podstawa", Pobierz("Algorytm.Podstawa"));
        if (r.ContainsKey("Algorytm.Czas")) yield return ("Algorytm.KreatorAlgorytmu.Czas.Czas", Pobierz("Algorytm.Czas"));
        // Wspolczynnik.Procent/Ulamek/Wspolczynnik => Algorytm.Ulamek, wg typu mnożnika
        if (r.ContainsKey("Algorytm.Ulamek"))
        {
            var u = Pobierz("Algorytm.Ulamek");
            switch (Pobierz("Algorytm.KreatorAlgorytmu.Wspolczynnik.Typ"))
            {
                case "Procent": yield return ("Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent", UlamekNaProcent(u)); break; // "10%"
                case "Ułamek": yield return ("Algorytm.KreatorAlgorytmu.Wspolczynnik.Ulamek", u); break;
                case "Współczynnik": yield return ("Algorytm.KreatorAlgorytmu.Wspolczynnik.Wspolczynnik", UlamekNaLiczbe(u)); break;
            }
        }
        // WliczajSOD = !NieWliczajSOD, RozrzucajPodatkiISkładki = !NieRozrzucaj
        if (r.ContainsKey("Deklaracje.NieWliczajSOD")) yield return ("Deklaracje.WliczajSOD", Tak("Deklaracje.NieWliczajSOD") ? "False" : "True");
        if (r.ContainsKey("Deklaracje.NieRozrzucaj")) yield return ("Deklaracje.RozrzucajPodatkiISkładki", Tak("Deklaracje.NieRozrzucaj") ? "False" : "True");
        // _Odchyłki / _OdchyłkiPlus (KorektaDefElementu) z pól Odchylki, WybraneOdchylki, OdchylkiPlus, WybraneOdchylkiPlus
        const string k = "Algorytm.KreatorAlgorytmu.Korekta.";
        if (r.ContainsKey(k + "Odchylki"))
        {
            var odch = !Tak(k + "Odchylki") ? "Nie" : Tak(k + "WybraneOdchylki") ? "Wybranych" : "Wszystkich";
            yield return (k + "_Odchyłki", odch);
            if (odch == "Wybranych")
                yield return (k + "_OdchyłkiPlus", !Tak(k + "OdchylkiPlus") ? "Nie" : Tak(k + "WybraneOdchylkiPlus") ? "Wybranych" : "Wszystkich");
        }
    }

    static string UlamekNaProcent(string u)
    {
        var d = UlamekNaDecimal(u);
        return d == null ? null : (d.Value * 100m).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "%";
    }

    static string UlamekNaLiczbe(string u) => UlamekNaDecimal(u)?.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);

    static decimal? UlamekNaDecimal(string u)
    {
        var m = Regex.Match(u ?? "", @"^\s*(-?[\d.]+)\s*/\s*([\d.]+)\s*$");
        if (!m.Success)
            return decimal.TryParse(u, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var x) ? x : null;
        var l = decimal.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        var mn = decimal.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
        return mn == 0 ? null : l / mn;
    }

    /// <summary>Wartość z dbinit w postaci używanej przez formularz (enum = nazwa, bool, liczba, słownik = klucz).</summary>
    static JsonNode Wartosc(JsonObject pole, string v, JsonObject slowniki)
    {
        if (v == null)
            return null;
        switch (pole["typ"]?.GetValue<string>())
        {
            case "bool":
                return JsonValue.Create(string.Equals(v, "True", StringComparison.OrdinalIgnoreCase));
            case "int":
                return int.TryParse(v, out var i) ? JsonValue.Create(i) : null;
            case "decimal":
                return decimal.TryParse(v, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var d) ? JsonValue.Create(d) : null;
            case "currency":
            {
                var m = Regex.Match(v, @"^-?[\d.]+");
                return m.Success ? JsonValue.Create(decimal.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture)) : null;
            }
            case "percent":
            {
                var p = TypeInspector.Liczba(v);
                return p == null ? null : JsonValue.Create(p.Value);
            }
            case "enum":
                return v == "0" || v == "" ? null : JsonValue.Create(v);
            case "slownik":
            {
                if (string.IsNullOrWhiteSpace(v))
                    return null;
                var sl = slowniki[pole["slownik"]!.GetValue<string>()];
                var poz = sl?["wartosciStandardowe"]?.AsArray().FirstOrDefault(x => string.Equals(x?["guid"]?.GetValue<string>(), v, StringComparison.OrdinalIgnoreCase));
                return poz?["klucz"]?.DeepClone();
            }
            case "lista":
                if (pole["sciezka"]?.GetValue<string>() != "Algorytm.KreatorAlgorytmu.Wspolczynnik.Progi" || string.IsNullOrEmpty(v))
                    return null;
                return new JsonArray(v.Split(";").Select(w => w.Split("=")).Where(w => w.Length == 2)
                    .Select(w => (JsonNode)new JsonObject { ["Prog"] = int.TryParse(w[0], out var pr) ? pr : 0, ["Wartosc"] = TypeInspector.Liczba(w[1]) ?? 0m }).ToArray());
            default:
                return v == "" ? null : JsonValue.Create(v);
        }
    }

    static bool Techniczne(string p) =>
        p.StartsWith("Algorytm.Dod") || p.StartsWith("Algorytm.El") || p.StartsWith("Algorytm.Klasa") || p.StartsWith("RuntimeInfo")
        || p.EndsWith("Nazwa") || p is "Podstawowa" or "Wydzial" or "Zakres" or "Tekst" or "Algorytm.Liczba" or "Algorytm.Dni"
        || p.Contains("PodstawaNazwa") || p.Contains("WspolczynnikNazwa") || p.Contains(".Proporcjonalnie");
}
