using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ImportDefinicjiElementow.Silnik;

/// <summary>
/// Reguły schematu - port 1:1 z Formularz/src/warunki.js (zmieniać oba pliki naraz).
/// Wartość efektywna pola = wartość z pliku, jeżeli pole jest aktywne (widoczne i edytowalne) i niepuste,
/// w przeciwnym razie wartość domyślna ze schematu.
/// </summary>
public static class Warunki
{
    static readonly Dictionary<string, int> Licencje = new() { ["srebrna"] = 1, ["zlota"] = 2, ["platynowa"] = 3 };

    public static bool Pusty(JsonNode v) =>
        v == null
        || (v is JsonValue jv && jv.TryGetValue<string>(out var s) && s.Length == 0)
        || (v is JsonArray a && a.Count == 0);

    public static bool Rowne(JsonNode a, JsonNode b)
    {
        if (a == null || b == null)
            return a == null && b == null;
        if (Liczba(a, out var la) || Liczba(b, out _))
            return Liczba(a, out la) && Liczba(b, out var lb) && la == lb;
        return JsonNode.DeepEquals(a, b);
    }

    internal static bool Liczba(JsonNode v, out decimal d)
    {
        d = 0;
        if (v is not JsonValue jv)
            return false;
        if (jv.TryGetValue<decimal>(out d))
            return true;
        if (jv.TryGetValue<double>(out var x))
        {
            d = (decimal)x;
            return true;
        }
        return false;
    }

    /// <summary>Ocena warunku DSL; wartosc(sciezka) zwraca wartość efektywną innego pola.</summary>
    public static bool Ocen(JsonNode w, Func<string, JsonNode> wartosc, string licencja)
    {
        if (w == null)
            return true;
        var o = w.AsObject();
        if (o["wszystkie"] is JsonArray all)
            return all.All(x => Ocen(x, wartosc, licencja));
        if (o["dowolny"] is JsonArray any)
            return any.Any(x => Ocen(x, wartosc, licencja));
        if (o["nie"] is JsonNode n)
            return !Ocen(n, wartosc, licencja);
        if (o["licencja"] is JsonNode lic)
            return Licencje.GetValueOrDefault(licencja ?? "", 0) >= Licencje.GetValueOrDefault(lic.GetValue<string>(), 99);
        var v = wartosc(o["pole"]!.GetValue<string>());
        if (o.ContainsKey("rowne")) return Rowne(v, o["rowne"]);
        if (o.ContainsKey("rozne")) return !Rowne(v, o["rozne"]);
        if (o["wsrod"] is JsonArray ws) return ws.Any(x => Rowne(v, x));
        if (o["poza"] is JsonArray pz) return !pz.Any(x => Rowne(v, x));
        if (o["pusty"] is JsonNode p) return Pusty(v) == p.GetValue<bool>();
        if (o["wiekszeOd"] is JsonNode wo) return Liczba(v, out var d) && d > wo.GetValue<decimal>();
        throw new ArgumentException("Nieznany warunek: " + w.ToJsonString());
    }

    public static string NazwaLicencji(string l) => l == "zlota" ? "złotej lub platynowej" : l == "platynowa" ? "platynowej" : l;

    /// <summary>Czytelny opis warunku (komunikaty).</summary>
    public static string Opisz(JsonNode w, Schemat s)
    {
        if (w == null)
            return "";
        var o = w.AsObject();
        if (o["wszystkie"] is JsonArray all) return string.Join(" i ", all.Select(x => Nawias(x, s)));
        if (o["dowolny"] is JsonArray any) return string.Join(" lub ", any.Select(x => Nawias(x, s)));
        if (o["nie"] is JsonNode n) return "nie (" + Opisz(n, s) + ")";
        if (o["licencja"] is JsonNode lic) return "licencja " + NazwaLicencji(lic.GetValue<string>());
        var pole = o["pole"]!.GetValue<string>();
        var et = s.Pole(pole) != null ? "„" + s.Etykieta(pole) + "”" : pole;
        if (o.ContainsKey("rowne")) return et + " = " + s.EtykietaWartosci(pole, o["rowne"]);
        if (o.ContainsKey("rozne")) return et + " ≠ " + s.EtykietaWartosci(pole, o["rozne"]);
        if (o["wsrod"] is JsonArray ws) return et + ": " + string.Join(" / ", ws.Select(x => s.EtykietaWartosci(pole, x)));
        if (o["poza"] is JsonArray pz) return et + " inne niż " + string.Join(" / ", pz.Select(x => s.EtykietaWartosci(pole, x)));
        if (o["pusty"] is JsonNode p) return et + (p.GetValue<bool>() ? " puste" : " wypełnione");
        if (o["wiekszeOd"] is JsonNode wo) return et + " > " + wo;
        return w.ToJsonString();
    }

    static string Nawias(JsonNode x, Schemat s)
    {
        var t = Opisz(x, s);
        return x.AsObject().ContainsKey("wszystkie") || x.AsObject().ContainsKey("dowolny") ? "(" + t + ")" : t;
    }
}

/// <summary>Stan pola w kontekście konkretnej definicji.</summary>
public sealed class StanPola
{
    public bool Widoczne;
    public bool Edytowalne;
    public bool Aktywne => Widoczne && Edytowalne;
    public bool Wymagane;
    /// <summary>Dozwolone wartości (enum lub lista tekstów) albo null, gdy dowolne.</summary>
    public List<string> Dozwolone;
}

/// <summary>Kontekst oceny jednej definicji (odpowiednik Kontekst z warunki.js).</summary>
public sealed class KontekstDefinicji
{
    readonly Schemat schemat;
    readonly JsonObject wartosci;
    readonly string licencja;
    readonly Dictionary<string, JsonNode> efektywne = new();
    readonly Dictionary<string, StanPola> stany = new();
    readonly HashSet<string> wToku = new();

    public KontekstDefinicji(Schemat schemat, JsonObject wartosci, string licencja)
    {
        this.schemat = schemat;
        this.wartosci = wartosci ?? new JsonObject();
        this.licencja = string.IsNullOrEmpty(licencja) ? "zlota" : licencja;
    }

    public string Licencja => licencja;

    bool Ocen(JsonNode w) => Warunki.Ocen(w, Wartosc, licencja);

    public JsonNode Uzytkownika(string sciezka) => wartosci[sciezka];

    public JsonNode Domyslna(string sciezka)
    {
        var pole = schemat.Pole(sciezka);
        if (pole == null)
            return null;
        var d = pole["domyslna"];
        if (d is JsonObject o && o["wgWarunku"] is JsonArray reguly)
        {
            foreach (var r in reguly)
                if (Ocen(r!["gdy"]))
                    return r["wartosc"]?.DeepClone();
            return o["inaczej"]?.DeepClone();
        }
        if (d == null && pole["typ"]?.GetValue<string>() == "bool")
            return JsonValue.Create(false);
        return d?.DeepClone();
    }

    public JsonNode Wartosc(string sciezka)
    {
        if (efektywne.TryGetValue(sciezka, out var e))
            return e;
        if (wToku.Contains(sciezka))
        {
            var u = Uzytkownika(sciezka);
            return Warunki.Pusty(u) ? Domyslna(sciezka) : u;
        }
        wToku.Add(sciezka);
        var v = Uzytkownika(sciezka);
        if (Warunki.Pusty(v) || !Stan(sciezka).Aktywne)
            v = Domyslna(sciezka);
        wToku.Remove(sciezka);
        efektywne[sciezka] = v;
        return v;
    }

    public StanPola Stan(string sciezka)
    {
        if (stany.TryGetValue(sciezka, out var st))
            return st;
        st = new StanPola();
        stany[sciezka] = st; // ochrona przed cyklem
        var pole = schemat.Pole(sciezka);
        if (pole == null)
            return st;
        var wyst = schemat.Wystapienia.GetValueOrDefault(sciezka) ?? new();
        var kontener = wyst.Any(w => Ocen(w.zakladka["widocznaGdy"]) && Ocen(w.sekcja["widocznaGdy"]));
        var lic = pole["licencja"] == null || Ocen(new JsonObject { ["licencja"] = pole["licencja"]!.GetValue<string>() });
        var widoczne = kontener && lic && Ocen(pole["widocznyGdy"]);
        var edytowalne = widoczne && Ocen(pole["edytowalnyGdy"]);
        var wynik = new StanPola
        {
            Widoczne = widoczne,
            Edytowalne = edytowalne,
            Wymagane = edytowalne && (pole["wymagane"]?.GetValue<bool>() == true || (pole["wymaganeGdy"] != null && Ocen(pole["wymaganeGdy"]))),
            Dozwolone = Dozwolone(pole),
        };
        stany[sciezka] = wynik;
        return wynik;
    }

    List<string> Dozwolone(JsonObject pole)
    {
        List<string> lista = null;
        var en = pole["enum"]?.GetValue<string>();
        if (en != null && schemat.Enumy[en] != null)
            lista = schemat.WartosciEnuma(en);
        if (pole["dozwoloneWartosci"] is JsonArray reguly)
            foreach (var r in reguly)
            {
                if (r!["gdy"] != null && !Ocen(r["gdy"]))
                    continue;
                var dozw = r["wartosci"]!.AsArray().Select(x => x!.GetValue<string>()).ToList();
                lista = lista == null ? dozw : lista.Where(dozw.Contains).ToList();
                break;
            }
        return lista;
    }
}

/// <summary>Komunikat walidacji (poziom: blad, ostrzezenie, informacja).</summary>
public sealed record Komunikat(string Sciezka, string Poziom, string Tekst);

/// <summary>Walidacja definicji wg schematu - te same reguły co w formularzu (warunki.js / waliduj).</summary>
public static class Walidacja
{
    static readonly Regex WzorzecUlamek = new(@"^\s*\d+([.,]\d+)?\s*(/\s*\d+([.,]\d+)?)?\s*$");
    static readonly Regex WzorzecCzas = new(@"^\s*\d{1,4}:[0-5]\d\s*$");

    static string Norm(JsonNode v) => (v is JsonValue jv && jv.TryGetValue<string>(out var s) ? s : v?.ToJsonString() ?? "").Trim().ToLower(new CultureInfo("pl-PL"));

    public static string SprawdzTyp(JsonObject pole, JsonNode v)
    {
        var typ = pole["typ"]?.GetValue<string>();
        var jv = v as JsonValue;
        switch (typ)
        {
            case "int":
                return Warunki.Liczba(v, out var d) && d == decimal.Truncate(d) ? null : "Wpisz liczbę całkowitą.";
            case "decimal":
            case "percent":
            case "currency":
                return Warunki.Liczba(v, out _) ? null : "Wpisz liczbę.";
            case "bool":
                return jv != null && jv.TryGetValue<bool>(out _) ? null : "Wybierz Tak albo Nie.";
            case "fraction":
                return WzorzecUlamek.IsMatch(Tekst(v)) ? null : "Wpisz ułamek, np. 1/2.";
            case "time":
                return WzorzecCzas.IsMatch(Tekst(v)) ? null : "Wpisz czas w formacie gg:mm.";
            case "string":
            case "text":
            case "slownik":
            case "enum":
                if (jv == null || !jv.TryGetValue<string>(out var s))
                    return "Wpisz tekst.";
                var max = pole["maxDlugosc"]?.GetValue<int>();
                return max != null && s.Length > max ? $"Maksymalnie {max} znaków (jest {s.Length})." : null;
            case "lista":
                return v is JsonArray ? null : "Nieprawidłowa lista.";
            default:
                return null;
        }
    }

    static string Tekst(JsonNode v) => v is JsonValue jv && jv.TryGetValue<string>(out var s) ? s : v?.ToJsonString() ?? "";

    /// <summary>Walidacja jednej definicji; inne - pozostałe definicje z pliku (unikalność).</summary>
    public static List<Komunikat> Waliduj(Schemat schemat, JsonObject wartosci, IEnumerable<JsonObject> inne, string licencja)
    {
        var k = new KontekstDefinicji(schemat, wartosci, licencja);
        var inneK = (inne ?? Enumerable.Empty<JsonObject>()).Select(w => new KontekstDefinicji(schemat, w, licencja)).ToList();
        var wynik = new List<Komunikat>();
        var standardowe = schemat.NazwyStandardowe();
        foreach (var s in schemat.KolejnoscPol)
        {
            var pole = schemat.Pole(s);
            var st = k.Stan(s);
            if (!st.Aktywne)
                continue;
            var v = k.Wartosc(s);
            if (Warunki.Pusty(v))
            {
                if (st.Wymagane)
                    wynik.Add(new(s, "blad", "Pole jest wymagane."));
                continue;
            }
            var bladTypu = SprawdzTyp(pole, v);
            if (bladTypu != null)
            {
                wynik.Add(new(s, "blad", bladTypu));
                continue;
            }
            if (st.Dozwolone != null && pole["typ"]?.GetValue<string>() != "lista" && !st.Dozwolone.Contains(Tekst(v)))
                wynik.Add(new(s, "blad", $"Wartość „{schemat.EtykietaWartosci(s, v)}” jest niedozwolona przy obecnych ustawieniach."));
            if (pole["walidacje"] is JsonObject wal)
            {
                Warunki.Liczba(v, out var liczba);
                var jestLiczba = Warunki.Liczba(v, out _);
                if (wal["min"] != null && jestLiczba && liczba < wal["min"]!.GetValue<decimal>())
                    wynik.Add(new(s, "blad", $"Wartość nie może być mniejsza niż {wal["min"]}."));
                if (wal["max"] != null && jestLiczba && liczba > wal["max"]!.GetValue<decimal>())
                    wynik.Add(new(s, "blad", $"Wartość nie może być większa niż {wal["max"]}."));
                if (wal["wzorzec"] != null && !Regex.IsMatch(Tekst(v), wal["wzorzec"]!.GetValue<string>()))
                    wynik.Add(new(s, "blad", wal["komunikat"]?.GetValue<string>() ?? "Nieprawidłowy format."));
                if (wal["unikalne"] != null)
                {
                    var poziom = wal["unikalne"]!.GetValue<string>() == "blad" ? "blad" : "ostrzezenie";
                    if (inneK.Any(ik => Norm(ik.Wartosc(s)) == Norm(v)))
                        wynik.Add(new(s, poziom, "Ta sama wartość jest w innej definicji w tym pliku."));
                    if (s == "Nazwa" && standardowe.Contains(Tekst(v).Trim()))
                        wynik.Add(new(s, "blad", "Taką nazwę ma definicja standardowa enova – wybierz inną."));
                }
            }
            if (pole["typ"]?.GetValue<string>() == "lista" && pole["kolumny"] is JsonArray kolumny)
            {
                var i = 0;
                foreach (var w in v.AsArray())
                {
                    i++;
                    foreach (var kol in kolumny)
                    {
                        var kv = w?[kol!["sciezka"]!.GetValue<string>()];
                        var et = kol["etykieta"]?.GetValue<string>();
                        if (Warunki.Pusty(kv))
                            wynik.Add(new(s, "blad", $"Wiersz {i}: uzupełnij „{et}”."));
                        else if (SprawdzTyp(kol.AsObject(), kv) is string b)
                            wynik.Add(new(s, "blad", $"Wiersz {i}, „{et}”: {b}"));
                    }
                }
            }
        }
        return wynik;
    }

    /// <summary>Wartości do zapisu: efektywne wartości aktywnych pól w kolejności schematu (warunki.js / doEksportu).</summary>
    public static List<(string Sciezka, JsonNode Wartosc)> DoZapisu(Schemat schemat, JsonObject wartosci, string licencja)
    {
        var k = new KontekstDefinicji(schemat, wartosci, licencja);
        var wynik = new List<(string, JsonNode)>();
        foreach (var s in schemat.KolejnoscPol)
        {
            if (!k.Stan(s).Aktywne)
                continue;
            var v = k.Wartosc(s);
            if (!Warunki.Pusty(v))
                wynik.Add((s, v.DeepClone()));
        }
        return wynik;
    }
}
