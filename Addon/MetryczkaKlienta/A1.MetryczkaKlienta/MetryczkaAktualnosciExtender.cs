using A1.Metryczka;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Config;

// {new MetryczkaAktualnosciExtender} w FolderMenu-KadryIPlace.form.xml - jak FolderMenuExtension standardu.
[assembly: Worker(typeof(MetryczkaAktualnosciExtender))]

namespace A1.Metryczka;

/// <summary>
/// Karta metryczki klienta nad sekcjami ekranu modułu Kadry i płace (HTML z cech TST_, KartaMetryczki).
/// Kafelek w nagłówku chowa/pokazuje kartę.
/// </summary>
public class MetryczkaAktualnosciExtender
{
    const string WezelDodatku = "A1MetryczkaKlienta";
    const string WezelWidocznosci = "Widocznosc";

    [Context]
    public Session Session { get; set; }

    string karta;

    /// <summary>Karta czytana w świeżej sesji: sesja ekranu modułu powstaje przy logowaniu i nie widzi późniejszych zmian cech.</summary>
    public string KartaHtml
    {
        get {
            if (karta != null)
                return karta;
            using (Session sesja = Session.Login.CreateSession(true, false, "Metryczka - karta"))
                return karta = KartaMetryczki.Html(MetryczkaKlienta.Cechy(sesja));
        }
    }

    public string Podsumowanie => new MetryczkaKlienta(Session).Podsumowanie;

    /// <summary>Kafelek "Metryczka klienta": zwrócony obiekt enova otwiera w oknie (karta + Czynności).</summary>
    public MetryczkaKlienta OtworzMetryczke() => new MetryczkaKlienta(Session);

    // ---------------- pokaż / ukryj kartę (stan per operator) ----------------

    // Wartość w loginie: sesja okna menu nie widzi zapisu z osobnej sesji, a odczyt ma być natychmiastowy.
    public bool Widoczna => Session.Login.GetValue(typeof(MetryczkaAktualnosciExtender), WezelWidocznosci) is bool b
        ? b
        : CzytajWidocznosc();

    public string TekstPrzycisku => Widoczna ? "Ukryj metryczkę" : "Pokaż metryczkę";

    public void PrzelaczWidocznosc()
    {
        bool widoczna = !Widoczna;
        Session.Login.SetValue(typeof(MetryczkaAktualnosciExtender), WezelWidocznosci, widoczna);
        // Trwały zapis per operator w drzewie konfiguracji - osobna sesja, bo sesja okna jest tylko do odczytu.
        using (Session sesja = Session.Login.CreateSession(false, true, "Metryczka - widoczność karty"))
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
        // Odczyt nie zakłada węzła (sesja tylko do odczytu); brak wpisu = karta widoczna.
        CfgNode wezel = new CfgManager(Session).Root?.FindSubNode(WezelDodatku, false)?.FindSubNode(WezelWidocznosci, false);
        return wezel == null || (bool)wezel.GetAttribute(KluczOperatora, typeof(bool), true);
    }
}
