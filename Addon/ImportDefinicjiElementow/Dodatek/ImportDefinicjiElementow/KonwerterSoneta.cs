using System.Globalization;
using System.Text.Json.Nodes;
using ImportDefinicjiElementow.Silnik;
using Soneta.Business;
using Soneta.Types;

namespace ImportDefinicjiElementow;

/// <summary>
/// Wartości z pliku (JSON) ↔ typy enova. Procent w pliku to liczba (10 = 10%), kwota to liczba,
/// ułamek "1/2", czas "8:00", enum - nazwa wartości, słownik - klucz wg schematu.
/// </summary>
sealed class KonwerterSoneta : IKonwerterWartosci
{
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    readonly BazaEnova baza;

    public KonwerterSoneta(BazaEnova baza) => this.baza = baza;

    public object DoObiektu(JsonNode v, Type cel, JsonObject pole)
    {
        var typ = pole?["typ"]?.GetValue<string>();
        if (typ == "slownik")
        {
            var sl = pole["slownik"]!.GetValue<string>();
            var klucz = Analizator.Tekst(v);
            return baza.Znajdz(sl, klucz) ?? throw new InvalidOperationException($"W bazie nie ma wartości „{klucz}” ({sl}).");
        }
        return Wartosc(v, cel);
    }

    /// <summary>Konwersja wartości prostej na typ docelowy (także kolumny list).</summary>
    public static object Wartosc(JsonNode v, Type cel)
    {
        var t = Nullable.GetUnderlyingType(cel) ?? cel;
        if (t.IsEnum)
            return Enum.Parse(t, Analizator.Tekst(v));
        if (t == typeof(bool))
            return v.GetValue<bool>();
        if (t == typeof(int))
            return (int)Liczba(v);
        if (t == typeof(decimal))
            return Liczba(v);
        if (t == typeof(string))
            return Analizator.Tekst(v);
        if (t == typeof(Percent))
            return new Percent(Liczba(v) / 100m);
        if (t == typeof(Currency))
            return new Currency(Liczba(v));
        if (t == typeof(Fraction))
            return Fraction.Parse(Analizator.Tekst(v).Replace(',', '.').Replace(" ", ""), Inv);
        if (t == typeof(Time))
            return Time.Parse(Analizator.Tekst(v).Trim(), Inv);
        if (t == typeof(Date))
            return Date.Parse(Analizator.Tekst(v), Inv);
        throw new InvalidOperationException($"Brak konwersji wartości {v?.ToJsonString()} na {t.FullName}.");
    }

    static decimal Liczba(JsonNode v)
    {
        if (v is JsonValue jv && jv.TryGetValue<decimal>(out var d))
            return d;
        return decimal.Parse(Analizator.Tekst(v).Replace(',', '.'), Inv);
    }

    public JsonNode DoPliku(object o, JsonObject pole)
    {
        var typ = pole?["typ"]?.GetValue<string>();
        if (typ == "slownik")
            return o is Row r ? JsonValue.Create(baza.KluczWiersza(pole["slownik"]!.GetValue<string>(), r)) : null;
        return Prosta(o);
    }

    public static JsonNode Prosta(object o) => o switch
    {
        null => null,
        Enum e => JsonValue.Create(e.ToString()),
        bool b => JsonValue.Create(b),
        int i => JsonValue.Create(i),
        decimal d => JsonValue.Create(d),
        string s => JsonValue.Create(s),
        Percent p => JsonValue.Create(Math.Round((decimal)p * 100m, 2)),
        Currency c => JsonValue.Create(c.Value),
        Fraction f => JsonValue.Create(f.ToString()),
        Time t => JsonValue.Create(t.ToString()),
        Date dt => JsonValue.Create(dt.ToString()),
        Row r => JsonValue.Create(r.ToString()),
        _ => JsonValue.Create(o.ToString()),
    };
}
