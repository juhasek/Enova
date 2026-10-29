using System.Collections.Generic;
using System.Linq;
using A1.Metryczka;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Tools;
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

    /// <summary>Pola w kolejności; pole strony wiąże się po indeksie ({Pola[i].Tekst}), zapis idzie wprost do węzła.</summary>
    public List<PoleMetryczki> Pola => KonfiguracjaMetryczki.Pola(Session);

    /// <summary>Klucz układu strony: zmiana listy pól (Dodaj/Usuń, nazwa, kategoria, typ) przebudowuje wygenerowane sekcje.</summary>
    public string RenderKey => string.Join("|", Pola.Select(p => $"{p.Klucz}/{p.Typ}/{p.Kategoria}/{p.Nazwa}"));

    /// <summary>
    /// Sekcja na kategorię, w niej etykieta + pole (Tak/Nie = checkbox, tekst = pole edycji) -
    /// jak standardowa strona Ogólne → Cechy globalne (DefineItemsExtender.RenderElements).
    /// </summary>
    public void RenderPola(ContainerElement root)
    {
        List<PoleMetryczki> pola = Pola;
        for (int i = 0; i < pola.Count; i++)
        {
            string kategoria = pola[i].Kategoria;
            var grupa = (ContainerElement)root.Elements.FirstOrDefault(g => kategoria == (string)g.TagObject);
            if (grupa == null)
            {
                grupa = new GroupContainer { CaptionHtml = kategoria.HtmlEncode(), TagObject = kategoria, LabelWidth = "65" };
                root.Elements.Add(grupa);
            }
            grupa.Elements.Add(new FieldElement
            {
                Name = "Pole" + i,
                CaptionHtml = pola[i].Nazwa.HtmlEncode(),
                EditValue = pola[i].Typ == TypPola.TakNie ? $"{{Pola[{i}].Zaznaczone}}" : $"{{Pola[{i}].Tekst}}",
                Width = pola[i].Typ == TypPola.TakNie ? null : "50",
            });
        }
    }

    public string Podsumowanie
    {
        get {
            var pola = KonfiguracjaMetryczki.Pola(Session);
            return pola.Count == 0
                ? "Brak pól – kliknij „Wczytaj brakujące pola z metryczki” albo „+ Dodaj pole”."
                : $"Uzupełniono {pola.Count(p => p.Uzupelnione)} z {pola.Count} pól ({pola.Select(p => p.Kategoria).Distinct().Count()} kategorii).";
        }
    }

    /// <summary>Przycisk "Wczytaj" tylko, gdy brakuje któregoś pola z arkusza metryczki.</summary>
    public bool SaBrakujacePola
    {
        get {
            var klucze = KonfiguracjaMetryczki.Pola(Session).Select(p => p.Klucz).ToHashSet();
            return DefinicjeCech.Wszystkie.Any(d => !klucze.Contains(d.Nazwa.Substring(DefinicjeCech.Prefiks.Length)));
        }
    }

    public object DodajPole()
    {
        var p = new DodajPoleParams(Context.Empty.Clone(Session));
        return new QueryContextInformation(p) { Caption = "Dodaj pole metryczki", AcceptHandler = () => {
            if (string.IsNullOrWhiteSpace(p.Nazwa))
                return "Nie dodano pola - nazwa jest wymagana.";
            var pola = KonfiguracjaMetryczki.Pola(Session);
            using (ITransaction t = Session.Logout(true))
            {
                string kategoria = string.IsNullOrWhiteSpace(p.NowaKategoria) ? p.Kategoria : p.NowaKategoria;
                KonfiguracjaMetryczki.Dodaj(Session, KonfiguracjaMetryczki.NowyKlucz(Session), p.Nazwa.Trim(), kategoria, p.Typ,
                    pola.Count == 0 ? 1 : pola.Max(x => x.Kolejnosc == int.MaxValue ? 0 : x.Kolejnosc) + 1);
                t.CommitUI();
            }
            return null;
        } };
    }

    public object UsunPole()
    {
        var p = new UsunPoleParams(Context.Empty.Clone(Session));
        return new QueryContextInformation(p) { Caption = "Usuń pola metryczki", AcceptHandler = () => {
            foreach (WyborPola w in p.Pola.Where(w => w.Usun))
                w.Pole.Usun();
            return null;
        } };
    }

    public object ZmienNazwe()
    {
        var p = new ZmienNazweParams(Context.Empty.Clone(Session));
        return new QueryContextInformation(p) { Caption = "Zmień nazwę pola metryczki", AcceptHandler = () => {
            if (string.IsNullOrWhiteSpace(p.NowaNazwa))
                return "Nie zmieniono nazwy - nowa nazwa jest wymagana.";
            PoleMetryczki pole = KonfiguracjaMetryczki.Pola(Session).FirstOrDefault(x => x.Nazwa == p.Pole);
            if (pole != null)
                pole.Nazwa = p.NowaNazwa;
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

// Caption klasy = tytuł okna ("Edycja: ..."); bez niego web pokazuje nazwę klasy.
[Caption("Dodaj pole metryczki")]
public class DodajPoleParams : ContextBase
{
    public DodajPoleParams(Context cx) : base(cx)
    {
    }

    [Caption("Nazwa pola")]
    public string Nazwa { get; set; }

    [Caption("Kategoria (karta)")]
    public string Kategoria { get; set; }

    /// <summary>Nowa kategoria powstaje z pierwszym polem; wypełniona ma pierwszeństwo przed wyborem z listy.</summary>
    [Caption("Nowa kategoria")]
    public string NowaKategoria { get; set; }

    [Caption("Typ pola")]
    public TypPola Typ { get; set; }

    public IEnumerable<string> GetListKategoria() =>
        KonfiguracjaMetryczki.Pola(Session).Select(p => p.Kategoria).Distinct();
}

[Caption("Usuń pola metryczki")]
public class UsunPoleParams : ContextBase
{
    public UsunPoleParams(Context cx) : base(cx)
    {
    }

    List<WyborPola> pola;

    /// <summary>Siatka z checkboxem "Usuń"; stała instancja, inaczej zaznaczenia przepadają między odświeżeniami.</summary>
    public List<WyborPola> Pola => pola ??= KonfiguracjaMetryczki.Pola(Session).Select(p => new WyborPola(p)).ToList();
}

/// <summary>Wiersz siatki okna "Usuń pola": pole metryczki + zaznaczenie do usunięcia.</summary>
public class WyborPola
{
    public WyborPola(PoleMetryczki pole)
    {
        Pole = pole;
    }

    public PoleMetryczki Pole { get; }

    [Caption("Usuń")]
    public bool Usun { get; set; }

    [Caption("Pole")]
    public string Nazwa => Pole.Nazwa;

    [Caption("Kategoria")]
    public string Kategoria => Pole.Kategoria;
}

[Caption("Zmień nazwę pola metryczki")]
public class ZmienNazweParams : ContextBase
{
    public ZmienNazweParams(Context cx) : base(cx)
    {
    }

    [Caption("Pole")]
    public string Pole { get; set; }

    [Caption("Nowa nazwa")]
    public string NowaNazwa { get; set; }

    public IEnumerable<string> GetListPole() => KonfiguracjaMetryczki.Pola(Session).Select(p => p.Nazwa);
}
