using System.Collections.Generic;
using System.Linq;
using A1.Metryczka;
using Soneta.Business;
using Soneta.Types;

// DataContext="{New MetryczkaConfigExtender}" w Config.MetryczkaKlienta.pageform.xml (extender bez typu danych, jak A1RaportPlacConfigExtender).
[assembly: Worker(typeof(MetryczkaConfigExtender))]

namespace A1.Metryczka;

/// <summary>
/// Strona Narzędzia → Opcje → A1Testy → Metryczka klienta: siatki pól Tak/Nie i tekstowych z edycją w miejscu,
/// przyciski Dodaj/Usuń pole i jednorazowe wczytanie pól z metryczki (wartości ze starych cech TST_).
/// Zapis idzie w sesji okna Opcji - utrwala go przycisk Zapisz okna.
/// </summary>
public class MetryczkaConfigExtender
{
    [Context]
    public Session Session { get; set; }

    public IEnumerable<PoleMetryczki> PolaTakNie => KonfiguracjaMetryczki.Pola(Session).Where(p => p.Typ == TypPola.TakNie);

    public IEnumerable<PoleMetryczki> PolaTekstowe => KonfiguracjaMetryczki.Pola(Session).Where(p => p.Typ == TypPola.Tekst);

    public object DodajPole()
    {
        var p = new DodajPoleParams(Context.Empty.Clone(Session));
        return new QueryContextInformation(p) { Caption = "Dodaj pole metryczki", AcceptHandler = () => {
            if (string.IsNullOrWhiteSpace(p.Nazwa))
                return "Nie dodano pola - nazwa jest wymagana.";
            var pola = KonfiguracjaMetryczki.Pola(Session);
            using (ITransaction t = Session.Logout(true))
            {
                KonfiguracjaMetryczki.Dodaj(Session, KonfiguracjaMetryczki.NowyKlucz(Session), p.Nazwa.Trim(), p.Kategoria, p.Typ,
                    pola.Count == 0 ? 1 : pola.Max(x => x.Kolejnosc == int.MaxValue ? 0 : x.Kolejnosc) + 1);
                t.CommitUI();
            }
            return null;
        } };
    }

    public object UsunPole()
    {
        var p = new UsunPoleParams(Context.Empty.Clone(Session));
        return new QueryContextInformation(p) { Caption = "Usuń pole metryczki", AcceptHandler = () => {
            KonfiguracjaMetryczki.Pola(Session).FirstOrDefault(x => x.Nazwa == p.Pole)?.Usun();
            return null;
        } };
    }

    /// <summary>
    /// Zakłada brakujące pola z arkusza metryczki (DefinicjeCech) z wartościami starych cech globalnych TST_, jeśli są.
    /// Istniejących pól (ten sam klucz) nie zmienia, więc można uruchomić ponownie.
    /// </summary>
    public object WczytajZMetryczki()
    {
        int dodane = 0;
        var klucze = KonfiguracjaMetryczki.Pola(Session).Select(p => p.Klucz).ToHashSet();
        using (ITransaction t = Session.Logout(true))
        {
            foreach (DefinicjaCechy d in DefinicjeCech.Wszystkie)
            {
                string klucz = d.Nazwa.Substring(DefinicjeCech.Prefiks.Length);
                if (klucze.Contains(klucz))
                    continue;
                TypPola typ = d.Typ == FeatureTypeNumber.Bool ? TypPola.TakNie : TypPola.Tekst;
                PoleMetryczki pole = KonfiguracjaMetryczki.Dodaj(Session, klucz, DefinicjeCech.Etykieta(d.Nazwa) ?? d.Opis, d.Kategoria, typ, d.Wiersz);
                string stara = MetryczkaKlienta.WartoscCechy(Session, d.Nazwa);
                if (stara != null)
                {
                    if (typ == TypPola.TakNie) pole.Zaznaczone = stara == "Tak";
                    else pole.Tekst = stara;
                }
                dodane++;
            }
            t.CommitUI();
        }
        return $"Dodano pól: {dodane}, pominięto istniejące: {DefinicjeCech.Wszystkie.Length - dodane}. Zapisz okno Opcji.";
    }
}

public class DodajPoleParams : ContextBase
{
    public DodajPoleParams(Context cx) : base(cx)
    {
    }

    [Caption("Nazwa pola")]
    public string Nazwa { get; set; }

    [Caption("Kategoria (karta)")]
    public string Kategoria { get; set; }

    [Caption("Typ pola")]
    public TypPola Typ { get; set; }

    public IEnumerable<string> GetListKategoria() =>
        KonfiguracjaMetryczki.Pola(Session).Select(p => p.Kategoria).Distinct();
}

public class UsunPoleParams : ContextBase
{
    public UsunPoleParams(Context cx) : base(cx)
    {
    }

    [Caption("Pole do usunięcia")]
    public string Pole { get; set; }

    public IEnumerable<string> GetListPole() => KonfiguracjaMetryczki.Pola(Session).Select(p => p.Nazwa);
}
