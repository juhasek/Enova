using System;
using System.Collections.Generic;
using System.Linq;
using Soneta.Business;
using Soneta.Config;
using Soneta.Types;

namespace A1.Metryczka;

public enum TypPola
{
    [Caption("Tak/Nie")] TakNie = 0,
    [Caption("Tekst")] Tekst = 1,
}

/// <summary>
/// Pola metryczki w drzewie konfiguracji enova (CfgNodes/CfgAttributes), jak ustawienia raportu płacowego A1Testy:
///   Root → A1Testy (Node) → Metryczka klienta (Node) → &lt;klucz pola&gt; (Leaf: Nazwa, Kategoria, Typ, Kolejnosc, Wartosc).
/// Edycja: Narzędzia → Opcje → A1Testy → Metryczka klienta (Config.MetryczkaKlienta.pageform.xml).
/// </summary>
public static class KonfiguracjaMetryczki
{
    public const string WezelGlowny = "A1Testy";
    public const string WezelMetryczki = "Metryczka klienta";

    /// <summary>Węzeł metryczki; przy odczycie (utworz=false) nie zakłada niczego - sesja może być tylko do odczytu.</summary>
    public static CfgNode Wezel(Session session, bool utworz)
    {
        CfgNode root = new CfgManager(session).Root;
        CfgNode a1 = root?.FindSubNode(WezelGlowny, false);
        if (a1 == null)
        {
            if (!utworz || root == null) return null;
            a1 = root.AddNode(WezelGlowny, CfgNodeType.Node);
        }
        CfgNode metryczka = a1.FindSubNode(WezelMetryczki, false);
        if (metryczka == null && utworz)
            metryczka = a1.AddNode(WezelMetryczki, CfgNodeType.Node);
        return metryczka;
    }

    public static List<PoleMetryczki> Pola(Session session) =>
        Wezel(session, false)?.Subkeys.Cast<CfgNode>().Select(n => new PoleMetryczki(n)).OrderBy(p => p.Kolejnosc).ToList()
        ?? new List<PoleMetryczki>();

    /// <summary>Dodaje pole; wołać w transakcji edycyjnej. Klucz (nazwa węzła) nie zmienia się przy zmianie nazwy pola.</summary>
    public static PoleMetryczki Dodaj(Session session, string klucz, string nazwa, string kategoria, TypPola typ, int kolejnosc)
    {
        CfgNode wezel = Wezel(session, true).AddNode(klucz, CfgNodeType.Leaf);
        var pole = new PoleMetryczki(wezel);
        wezel.SetAttribute("Nazwa", nazwa);
        wezel.SetAttribute("Kategoria", string.IsNullOrWhiteSpace(kategoria) ? "Inne" : kategoria.Trim());
        wezel.SetAttribute("Typ", typ.ToString());
        wezel.SetAttribute("Kolejnosc", kolejnosc);
        return pole;
    }

    public static string NowyKlucz(Session session)
    {
        var klucze = Pola(session).Select(p => p.Klucz).ToHashSet();
        int i = 1;
        while (klucze.Contains("Pole" + i)) i++;
        return "Pole" + i;
    }
}

/// <summary>Jedno pole metryczki (wiersz siatki w Opcjach) - odczyt i zapis wprost w atrybutach węzła konfiguracji.</summary>
public class PoleMetryczki
{
    readonly CfgNode wezel;

    public PoleMetryczki(CfgNode wezel)
    {
        this.wezel = wezel;
    }

    public string Klucz => wezel.Name;

    [Caption("Nazwa")]
    public string Nazwa
    {
        get => Atrybut("Nazwa", "");
        set => Zapisz("Nazwa", value?.Trim() ?? "");
    }

    [Caption("Kategoria")]
    public string Kategoria
    {
        get => Atrybut("Kategoria", "Inne");
        set => Zapisz("Kategoria", string.IsNullOrWhiteSpace(value) ? "Inne" : value.Trim());
    }

    public TypPola Typ => Enum.TryParse(Atrybut("Typ", ""), out TypPola t) ? t : TypPola.Tekst;

    public int Kolejnosc => Atrybut("Kolejnosc", int.MaxValue);

    [Caption("Tak/Nie")]
    public bool Zaznaczone
    {
        get => Atrybut("Wartosc", false);
        set => Zapisz("Wartosc", value);
    }

    [Caption("Wartość")]
    public string Tekst
    {
        get => Atrybut("Wartosc", "");
        set => Zapisz("Wartosc", value?.Trim() ?? "");
    }

    /// <summary>Tak/Nie: zapisane = uzupełnione (odznaczone też); tekst: niepusty.</summary>
    public bool Uzupelnione => Typ == TypPola.TakNie ? wezel.FindAttribute("Wartosc", false) != null : Tekst.Length > 0;

    /// <summary>Wartość na karcie: Tak/Nie (nieustawione = Nie, decyzja 27.10), tekst; null = brak.</summary>
    public string WartoscKarty => Typ == TypPola.TakNie ? (Zaznaczone ? "Tak" : "Nie") : Tekst.Length > 0 ? Tekst : null;

    T Atrybut<T>(string nazwa, T domyslna) => wezel.FindAttribute(nazwa, false)?.Value is T v ? v : domyslna;

    // Jak ChangeInfosConfig.TableInfo w standardzie: zapis w transakcji edycyjnej sesji okna Opcji.
    void Zapisz(string nazwa, object wartosc)
    {
        using (ITransaction t = wezel.Session.Logout(true))
        {
            wezel.SetAttribute(nazwa, wartosc);
            t.CommitUI();
        }
    }

    public void Usun()
    {
        using (ITransaction t = wezel.Session.Logout(true))
        {
            wezel.Delete();
            t.CommitUI();
        }
    }
}
