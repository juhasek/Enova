using System.Reflection;
using System.Text.Json.Nodes;

namespace ImportDefinicjiElementow.Silnik;

/// <summary>Konwersja wartości z pliku na typ właściwości obiektu biznesowego i z powrotem (do porównań).</summary>
public interface IKonwerterWartosci
{
    /// <summary>Wartość z pliku → obiekt typu docelowego (enum, Percent, Currency, wiersz słownika…).</summary>
    object DoObiektu(JsonNode wartosc, Type cel, JsonObject pole);
    /// <summary>Wartość właściwości → postać jak w pliku.</summary>
    JsonNode DoPliku(object wartosc, JsonObject pole);
}

/// <summary>Odczyt i zapis właściwości po ścieżce (np. Deklaracje.Zaliczka.Typ) - kolejno przez podobiekty.</summary>
public static class Wlasciwosci
{
    const BindingFlags Pub = BindingFlags.Public | BindingFlags.Instance;

    /// <summary>Właściwość o danej nazwie - najbardziej pochodna (enova ukrywa właściwości bazowe przez „new”).</summary>
    public static PropertyInfo Wlasciwosc(Type t, string nazwa)
    {
        for (var b = t; b != null; b = b.BaseType)
        {
            var p = b.GetProperty(nazwa, Pub | BindingFlags.DeclaredOnly);
            if (p != null && p.GetIndexParameters().Length == 0)
                return p;
        }
        return null;
    }

    /// <summary>Obiekt-właściciel ostatniego segmentu ścieżki i jego właściwość.</summary>
    public static (object Wlasciciel, PropertyInfo Wlasciwosc) Rozwin(object korzen, string sciezka)
    {
        var segmenty = sciezka.Split('.');
        var o = korzen;
        for (var i = 0; i < segmenty.Length - 1; i++)
        {
            var p = Wlasciwosc(o.GetType(), segmenty[i]) ?? throw new InvalidOperationException($"Brak właściwości „{segmenty[i]}” w {o.GetType().Name} (ścieżka {sciezka}).");
            o = p.GetValue(o) ?? throw new InvalidOperationException($"Właściwość „{segmenty[i]}” jest pusta (ścieżka {sciezka}).");
        }
        var ostatnia = Wlasciwosc(o.GetType(), segmenty[^1]) ?? throw new InvalidOperationException($"Brak właściwości „{segmenty[^1]}” w {o.GetType().Name} (ścieżka {sciezka}).");
        return (o, ostatnia);
    }

    public static object Odczytaj(object korzen, string sciezka)
    {
        var (o, p) = Rozwin(korzen, sciezka);
        return p.GetValue(o);
    }

    /// <summary>Ustawia wartość, jeżeli różni się od obecnej (setter enova bywa kosztowny i ma skutki uboczne).</summary>
    /// <returns>true, gdy wartość zmieniono.</returns>
    public static bool Ustaw(object korzen, string sciezka, JsonNode wartosc, JsonObject pole, IKonwerterWartosci konwerter)
    {
        var (o, p) = Rozwin(korzen, sciezka);
        if (p.SetMethod == null || !p.SetMethod.IsPublic)
            throw new InvalidOperationException($"Właściwość {sciezka} jest tylko do odczytu.");
        var nowa = konwerter.DoObiektu(wartosc, p.PropertyType, pole);
        var obecna = p.GetValue(o);
        if (Equals(obecna, nowa))
            return false;
        p.SetValue(o, nowa);
        return true;
    }
}
