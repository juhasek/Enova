using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using A1.Metryczka;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Config;
using Soneta.Core;

// {new MetryczkaAktualnosciExtender} w FolderMenu-KadryIPlace.form.xml - jak FolderMenuExtension standardu.
[assembly: Worker(typeof(MetryczkaAktualnosciExtender))]

namespace A1.Metryczka;

/// <summary>
/// Sekcja aktualności na ekranie modułu Kadry i płace: aktualności metryczki jako kafelki
/// (tworzy je czynność "Generuj aktualności z metryczki"). Kafelek w nagłówku sekcji chowa/pokazuje kafelki.
/// </summary>
public class MetryczkaAktualnosciExtender
{
    const string WezelDodatku = "A1MetryczkaKlienta";
    const string WezelWidocznosci = "Widocznosc";

    [Context]
    public Session Session { get; set; }

    public const string PrefiksTematu = "Metryczka klienta – ";
    const int LiczbaKafelkow = 8; // = liczba Command w FolderMenu-KadryIPlace.form.xml

    KafelekAktualnosci[] kafelki;

    /// <summary>Najnowsza aktualność metryczki z każdej kategorii (starsze komplety pomijane).</summary>
    KafelekAktualnosci[] Kafelki => kafelki ??= CzytajKafelki();

    KafelekAktualnosci[] CzytajKafelki()
    {
        // Świeża sesja: sesja ekranu modułu powstaje przy logowaniu i nie widzi aktualności wygenerowanych później.
        using (Session sesja = Session.Login.CreateSession(true, false, "Metryczka - kafelki aktualności"))
            return CoreModule.GetInstance(sesja).Aktualnosci.Cast<Aktualnosc>()
                .Where(a => a.Temat != null && a.Temat.StartsWith(PrefiksTematu))
                .GroupBy(a => a.Temat)
                .Select(g => g.OrderByDescending(a => a.DataEdycji).ThenByDescending(a => a.ID).First())
                .OrderBy(a => a.Temat)
                .Take(LiczbaKafelkow)
                .Select(a => new KafelekAktualnosci(a.Temat.Substring(PrefiksTematu.Length), KafelekAktualnosci.NaTekst(a.Opis.ToString())))
                .ToArray();
    }

    KafelekAktualnosci Kafelek(int i) => i < Kafelki.Length ? Kafelki[i] : KafelekAktualnosci.Pusty;
    public KafelekAktualnosci Kafelek0 => Kafelek(0);
    public KafelekAktualnosci Kafelek1 => Kafelek(1);
    public KafelekAktualnosci Kafelek2 => Kafelek(2);
    public KafelekAktualnosci Kafelek3 => Kafelek(3);
    public KafelekAktualnosci Kafelek4 => Kafelek(4);
    public KafelekAktualnosci Kafelek5 => Kafelek(5);
    public KafelekAktualnosci Kafelek6 => Kafelek(6);
    public KafelekAktualnosci Kafelek7 => Kafelek(7);

    public string Podsumowanie
    {
        get {
            int liczba = MetryczkaKlienta.Cechy(Session).Count(c => c.Aktywna);
            return $"Uzupełnionych cech TST_: {liczba} – Czynności → Generuj aktualności z metryczki";
        }
    }

    /// <summary>Kafelek "Metryczka klienta": zwrócony obiekt enova otwiera w oknie (cechy + Czynności).</summary>
    public MetryczkaKlienta OtworzMetryczke() => new MetryczkaKlienta(Session);

    // ---------------- pokaż / ukryj sekcję (stan per operator) ----------------

    // Wartość w loginie: sesja okna menu nie widzi zapisu z osobnej sesji, a odczyt ma być natychmiastowy.
    public bool Widoczna => Session.Login.GetValue(typeof(MetryczkaAktualnosciExtender), WezelWidocznosci) is bool b
        ? b
        : CzytajWidocznosc();

    public string TekstPrzycisku => Widoczna ? "Ukryj aktualności" : "Pokaż aktualności";

    public void PrzelaczWidocznosc()
    {
        bool widoczna = !Widoczna;
        Session.Login.SetValue(typeof(MetryczkaAktualnosciExtender), WezelWidocznosci, widoczna);
        // Trwały zapis per operator w drzewie konfiguracji - osobna sesja, bo sesja okna jest tylko do odczytu.
        using (Session sesja = Session.Login.CreateSession(false, true, "Metryczka - widoczność aktualności"))
        {
            using (ITransaction t = sesja.Logout(true))
            {
                CfgNode root = new CfgManager(sesja).Root;
                CfgNode dodatek = root.FindSubNode(WezelDodatku, false) ?? root.AddNode(WezelDodatku, CfgNodeType.Node);
                CfgNode wezel = dodatek.FindSubNode(WezelWidocznosci, false) ?? dodatek.AddNode(WezelWidocznosci, CfgNodeType.Leaf);
                wezel.SetAttribute(KluczOperatora, widoczna);
                t.Commit();
            }
            sesja.Save();
        }
        Session.InvokeChanged();
    }

    string KluczOperatora => Session.Login.Operator.Guid.ToString("N");

    bool CzytajWidocznosc()
    {
        // Odczyt nie zakłada węzła (sesja tylko do odczytu); brak wpisu = sekcja widoczna.
        CfgNode wezel = new CfgManager(Session).Root?.FindSubNode(WezelDodatku, false)?.FindSubNode(WezelWidocznosci, false);
        return wezel == null || (bool)wezel.GetAttribute(KluczOperatora, typeof(bool), true);
    }
}

/// <summary>Jeden kafelek sekcji: aktualność metryczki (temat bez prefiksu, punkty opisu w liniach).</summary>
public class KafelekAktualnosci
{
    public static readonly KafelekAktualnosci Pusty = new KafelekAktualnosci(null, "");

    public KafelekAktualnosci(string tytul, string opis)
    {
        Tytul = tytul;
        Opis = opis;
    }

    public bool Jest => Tytul != null;

    public string Tytul { get; }

    public string Opis { get; }

    /// <summary>Opis z generatora to lista HTML; kafelek pokazuje zwykły tekst (znaczniki dosłownie), więc punkty po " · ".</summary>
    public static string NaTekst(string html) => WebUtility.HtmlDecode(
        Regex.Replace(Regex.Replace(html ?? "", @"</li>\s*<li>", " · "), "<[^>]+>", ""));

    /// <summary>Kliknięcie kafelka: pełne aktualności (komentarze, polubienia) w Komunikacja → Moje aktualności.</summary>
    public object Otworz() => new NavigationResult("Komunikacja/Moje aktualności");
}
