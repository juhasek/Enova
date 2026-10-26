using System.Linq;
using A1.Metryczka;
using Soneta.Business;
using Soneta.Config;

// {new MetryczkaAktualnosciExtender} w FolderMenu-KadryIPlace.form.xml - jak FolderMenuExtension standardu.
[assembly: Worker(typeof(MetryczkaAktualnosciExtender))]

namespace A1.Metryczka;

/// <summary>
/// Sekcja aktualności na ekranie modułu Kadry i płace: standardowy widget aktualności operatora
/// (Dashboard.Aktualnosci.form.xml) - aktualności z metryczki tworzy czynność "Generuj aktualności z metryczki".
/// Kafelek w nagłówku sekcji chowa/pokazuje widget.
/// </summary>
public class MetryczkaAktualnosciExtender
{
    const string WezelDodatku = "A1MetryczkaKlienta";
    const string WezelWidocznosci = "Widocznosc";

    [Context]
    public Session Session { get; set; }

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
