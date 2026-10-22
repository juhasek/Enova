using System.Text.Json.Nodes;

namespace ImportDefinicjiElementow.Silnik;

/// <summary>
/// Schemat definicji elementu wynagrodzenia (schema/definicja-elementu.schema.json) - wspólny z formularzem klienta.
/// </summary>
public sealed class Schemat
{
    public JsonObject Json { get; }
    public string WersjaSchematu => Json["wersjaSchematu"]?.GetValue<string>();
    public string WersjaEnova => Json["wersjaEnova"]?.GetValue<string>();
    public JsonObject Pola => Json["pola"]!.AsObject();
    public JsonObject Enumy => Json["enumy"]!.AsObject();
    public JsonObject Slowniki => Json["slowniki"]!.AsObject();
    public JsonArray Zakladki => Json["zakladki"]!.AsArray();

    /// <summary>Ścieżki pól w kolejności formularza enova (kolejność ustawiania przy imporcie).</summary>
    public IReadOnlyList<string> KolejnoscPol { get; }

    /// <summary>Ścieżka pola → wystąpienia (zakładka, sekcja) - pole może być w kilku sekcjach.</summary>
    internal Dictionary<string, List<(JsonObject zakladka, JsonObject sekcja)>> Wystapienia { get; } = new();

    public Schemat(JsonObject json)
    {
        Json = json;
        KolejnoscPol = Pola.Select(kv => kv.Key).ToList();
        foreach (var z in Zakladki)
            foreach (var s in z!["sekcje"]!.AsArray())
                foreach (var p in s!["pola"]!.AsArray())
                {
                    var sc = p!.GetValue<string>();
                    if (!Wystapienia.TryGetValue(sc, out var l))
                        Wystapienia[sc] = l = new();
                    l.Add((z.AsObject(), s.AsObject()));
                }
    }

    /// <summary>Schemat wbudowany w bibliotekę (zasób z pliku schema/).</summary>
    public static Schemat Wbudowany()
    {
        using var s = typeof(Schemat).Assembly.GetManifestResourceStream("definicja-elementu.schema.json")
            ?? throw new InvalidOperationException("Brak wbudowanego schematu definicja-elementu.schema.json.");
        return new Schemat(JsonNode.Parse(s)!.AsObject());
    }

    public static Schemat ZPliku(string plik) => new(JsonNode.Parse(File.ReadAllText(plik))!.AsObject());

    public JsonObject Pole(string sciezka) => Pola[sciezka] as JsonObject;

    public string Etykieta(string sciezka) => Pole(sciezka)?["etykieta"]?.GetValue<string>() ?? sciezka;

    public string Typ(string sciezka) => Pole(sciezka)?["typ"]?.GetValue<string>();

    /// <summary>Nazwy wartości enuma bez ukrytych.</summary>
    public List<string> WartosciEnuma(string nazwaEnuma) =>
        Enumy[nazwaEnuma]?["wartosci"]?.AsArray()
            .Where(w => w!["ukryta"] == null)
            .Select(w => w!["nazwa"]!.GetValue<string>()).ToList() ?? new List<string>();

    public string EtykietaWartosci(string sciezka, JsonNode v)
    {
        if (v is JsonValue jv)
        {
            if (jv.TryGetValue<bool>(out var b))
                return b ? "Tak" : "Nie";
            if (jv.TryGetValue<string>(out var s))
            {
                var en = Pole(sciezka)?["enum"]?.GetValue<string>();
                var w = en == null ? null : Enumy[en]?["wartosci"]?.AsArray().FirstOrDefault(x => x!["nazwa"]?.GetValue<string>() == s);
                return w?["etykieta"]?.GetValue<string>() ?? s;
            }
        }
        return v?.ToJsonString() ?? "";
    }

    /// <summary>Nazwy definicji standardowych enova (import ich nie nadpisuje).</summary>
    public HashSet<string> NazwyStandardowe() =>
        new(Json["definicjeStandardowe"]?.AsArray().Select(d => d!["nazwa"]!.GetValue<string>()) ?? Enumerable.Empty<string>(), StringComparer.CurrentCultureIgnoreCase);
}
