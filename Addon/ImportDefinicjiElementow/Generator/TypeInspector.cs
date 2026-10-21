using System.Collections;
using System.Reflection;

namespace ImportDefinicjiElementow.Generator;

/// <summary>Wynik rozwinięcia ścieżki właściwości na klasie biznesowej.</summary>
sealed class InfoPola
{
    public bool Znalezione;
    public string Blad;
    /// <summary>bool, int, decimal, string, text, date, time, percent, fraction, currency, enum, slownik, lista, inny.</summary>
    public string Typ;
    public string TypNet;
    public string Enum;
    public bool Zapisywalne;
    public bool BazaDanych;
    public bool Wymagane;
    public int? MaxDlugosc;
    public object Domyslna;
    public string KlasaDeklarujaca;
    public List<string> RegulyWKodzie = new();
    public string OpisZKodu;
}

sealed class InfoEnum
{
    public string Typ;
    public List<InfoWartosciEnum> Wartosci = new();
}

sealed class InfoWartosciEnum
{
    public string Nazwa;
    public long Wartosc;
    public string Etykieta;
    public bool Ukryta;
}

/// <summary>
/// Rozwija ścieżki właściwości (np. Deklaracje.Zaliczka.Typ) przez refleksję na klasie wiersza
/// i równoległej klasie rekordu (pola bazodanowe z [Required]/[MaxLength] i wartości początkowe).
/// </summary>
sealed class TypeInspector
{
    const BindingFlags Pub = BindingFlags.Public | BindingFlags.Instance;
    readonly Type typWiersza;
    readonly Type typRekordu;
    readonly object rekord;
    public readonly Dictionary<string, InfoEnum> Enumy = new(StringComparer.Ordinal);

    public TypeInspector(Type typWiersza, Type typRekordu)
    {
        this.typWiersza = typWiersza;
        this.typRekordu = typRekordu;
        try
        {
            rekord = Activator.CreateInstance(typRekordu);
        }
        catch
        {
            rekord = null;
        }
    }

    public InfoPola Rozwin(string sciezka)
    {
        var info = new InfoPola();
        var segmenty = sciezka.Split('.');
        Type t = typWiersza;
        Type tRek = typRekordu;
        object wRek = rekord;
        PropertyInfo prop = null;
        for (int i = 0; i < segmenty.Length; i++)
        {
            var seg = segmenty[i];
            prop = Wlasciwosc(t, seg);
            if (prop == null)
            {
                info.Blad = $"brak właściwości '{seg}' w {t.FullName}";
                return info;
            }
            DodajReguly(info, t, seg, i == segmenty.Length - 1);
            // równoległe zejście po rekordzie (pola bazodanowe)
            if (tRek != null)
            {
                var f = tRek.GetField(seg, Pub);
                if (f != null)
                {
                    if (i == segmenty.Length - 1)
                    {
                        info.BazaDanych = true;
                        info.Wymagane = f.GetCustomAttributesData().Any(a => a.AttributeType.Name == "RequiredAttribute");
                        var ml = f.GetCustomAttributesData().FirstOrDefault(a => a.AttributeType.Name == "MaxLengthAttribute");
                        if (ml != null && ml.ConstructorArguments.Count > 0)
                            info.MaxDlugosc = Convert.ToInt32(ml.ConstructorArguments[0].Value);
                        // wartość początkowa rekordu ma sens tylko, gdy właściwość wiersza ma ten sam typ co pole rekordu
                        if (wRek != null && f.FieldType == prop.PropertyType)
                            info.Domyslna = WartoscDomyslna(f.GetValue(wRek), f.FieldType);
                    }
                    else
                    {
                        wRek = wRek == null ? null : f.GetValue(wRek);
                        tRek = f.FieldType;
                    }
                }
                else
                {
                    tRek = null;
                    wRek = null;
                }
            }
            t = prop.PropertyType;
        }
        info.Znalezione = true;
        info.TypNet = t.FullName;
        info.KlasaDeklarujaca = prop.DeclaringType?.FullName;
        info.Zapisywalne = prop.SetMethod != null && prop.SetMethod.IsPublic;
        info.OpisZKodu = Opis(prop);
        info.Typ = MapujTyp(t, out var nazwaEnum);
        if (nazwaEnum != null)
        {
            info.Enum = nazwaEnum;
            DodajEnum(t);
        }
        return info;
    }

    /// <summary>Typ elementu listy (Grid), np. dla Wspolczynnik.Progi → ProgStazuPracy.</summary>
    public Type TypElementuListy(string sciezka)
    {
        Type t = typWiersza;
        foreach (var seg in sciezka.Split('.'))
        {
            var p = Wlasciwosc(t, seg);
            if (p == null)
                return null;
            t = p.PropertyType;
        }
        if (t.IsGenericType)
            return t.GetGenericArguments()[0];
        var idx = t.GetProperties(Pub).FirstOrDefault(p => p.GetIndexParameters().Length == 1 && p.GetIndexParameters()[0].ParameterType == typeof(int));
        return idx?.PropertyType;
    }

    public InfoPola RozwinNaTypie(Type typ, string sciezka)
    {
        var insp = new TypeInspector(typ, typ.Assembly.GetType(typ.FullName + "Record") ?? NajblizszyRekord(typ));
        var info = insp.Rozwin(sciezka);
        foreach (var kv in insp.Enumy)
            Enumy[kv.Key] = kv.Value;
        return info;
    }

    static Type NajblizszyRekord(Type typ)
    {
        for (var b = typ; b != null; b = b.BaseType)
        {
            if (b.Name.EndsWith("Row"))
            {
                var nazwa = b.FullName.Substring(0, b.FullName.Length - 3) + "Record";
                var r = b.Assembly.GetType(nazwa) ?? b.DeclaringType?.GetNestedType(b.Name.Substring(0, b.Name.Length - 3) + "Record");
                if (r != null)
                    return r;
            }
        }
        return null;
    }

    static PropertyInfo Wlasciwosc(Type t, string nazwa)
    {
        // "new" w klasie pochodnej ukrywa właściwość bazową - bierzemy najbardziej pochodną
        for (var b = t; b != null; b = b.BaseType)
        {
            var p = b.GetProperty(nazwa, Pub | BindingFlags.DeclaredOnly);
            if (p != null && p.GetIndexParameters().Length == 0)
                return p;
        }
        if (t.IsInterface)
            return t.GetProperty(nazwa, Pub);
        return null;
    }

    static void DodajReguly(InfoPola info, Type t, string seg, bool ostatni)
    {
        foreach (var pref in new[] { "IsVisible", "IsReadOnly", "GetList", "IsRequired" })
        {
            var m = t.GetMethod(pref + seg, Pub, Type.EmptyTypes);
            if (m != null)
                info.RegulyWKodzie.Add($"{t.Name}.{m.Name}{(ostatni ? "" : " (grupa)")}");
        }
    }

    static string Opis(PropertyInfo p)
    {
        foreach (var a in p.GetCustomAttributesData())
        {
            var n = a.AttributeType.Name;
            if ((n == "DescriptionAttribute" || n == "CaptionAttribute") && a.ConstructorArguments.Count > 0 && a.ConstructorArguments[0].Value is string s && s.Length > 0)
                return s;
        }
        return null;
    }

    string MapujTyp(Type t, out string nazwaEnum)
    {
        nazwaEnum = null;
        if (t.IsEnum)
        {
            nazwaEnum = t.FullName;
            return "enum";
        }
        switch (t.FullName)
        {
            case "System.Boolean": return "bool";
            case "System.Int32": return "int";
            case "System.Decimal": return "decimal";
            case "System.Double": return "decimal";
            case "System.String": return "string";
            case "Soneta.Types.MemoText": return "text";
            case "Soneta.Types.Date": return "date";
            case "Soneta.Types.Time": return "time";
            case "Soneta.Types.Percent": return "percent";
            case "Soneta.Types.Fraction": return "fraction";
            case "Soneta.Types.Currency": return "currency";
        }
        if (typeof(IEnumerable).IsAssignableFrom(t) || t.Name.StartsWith("SubTable"))
            return "lista";
        for (var b = t; b != null; b = b.BaseType)
            if (b.FullName == "Soneta.Business.Row")
                return "slownik";
        if (t.IsInterface && t.GetInterfaces().Any(i => i.Name == "IRow"))
            return "slownik";
        return "inny";
    }

    void DodajEnum(Type t)
    {
        if (Enumy.ContainsKey(t.FullName))
            return;
        var e = new InfoEnum { Typ = t.FullName };
        foreach (var f in t.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var w = new InfoWartosciEnum { Nazwa = f.Name, Wartosc = Convert.ToInt64(f.GetRawConstantValue()) };
            foreach (var a in f.GetCustomAttributesData())
            {
                var n = a.AttributeType.Name;
                if (n == "CaptionAttribute" && a.ConstructorArguments.Count > 0)
                    w.Etykieta = a.ConstructorArguments[0].Value as string;
                if (n == "ObsoleteAttribute")
                    w.Ukryta = true;
                if (n == "BrowsableAttribute" && a.ConstructorArguments.Count > 0 && a.ConstructorArguments[0].Value is false)
                    w.Ukryta = true;
            }
            e.Wartosci.Add(w);
        }
        Enumy[t.FullName] = e;
    }

    static object WartoscDomyslna(object v, Type t)
    {
        if (v == null)
            return null;
        if (t.IsEnum)
            return Enum.GetName(t, v); // 0 spoza enuma = „nie ustawiono” → brak wartości domyślnej
        if (v is bool or int or long or decimal or string)
            return v;
        // typy Soneta (Date, Percent, Fraction, Currency, Time) - postać tekstowa; IRow - brak
        var s = v.ToString();
        return string.IsNullOrEmpty(s) ? null : s;
    }
}
