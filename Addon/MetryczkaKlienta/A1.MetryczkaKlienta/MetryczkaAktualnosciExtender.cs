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
    // Stałe liczby slotów w FolderMenu-KadryIPlace.form.xml: 8 kategorii metryczki, najwięcej 8 pozycji w kategorii.
    public const int LiczbaSekcji = 8;
    public const int LiczbaPozycji = 8;

    SekcjaMetryczki[] sekcje;

    /// <summary>Najnowsza aktualność metryczki z każdej kategorii (starsze komplety pomijane) - jedna sekcja kafelków.</summary>
    SekcjaMetryczki[] Sekcje => sekcje ??= CzytajSekcje();

    SekcjaMetryczki[] CzytajSekcje()
    {
        // Świeża sesja: sesja ekranu modułu powstaje przy logowaniu i nie widzi aktualności wygenerowanych później.
        using (Session sesja = Session.Login.CreateSession(true, false, "Metryczka - kafelki aktualności"))
            return CoreModule.GetInstance(sesja).Aktualnosci.Cast<Aktualnosc>()
                .Where(a => a.Temat != null && a.Temat.StartsWith(PrefiksTematu))
                .GroupBy(a => a.Temat)
                .Select(g => g.OrderByDescending(a => a.DataEdycji).ThenByDescending(a => a.ID).First())
                .OrderBy(a => a.Temat)
                .Take(LiczbaSekcji)
                .Select(a => new SekcjaMetryczki(a.Temat.Substring(PrefiksTematu.Length), SekcjaMetryczki.Pozycje(a.Opis.ToString())))
                .ToArray();
    }

    SekcjaMetryczki Sekcja(int i) => i < Sekcje.Length ? Sekcje[i] : SekcjaMetryczki.Pusta;
    public SekcjaMetryczki Sekcja0 => Sekcja(0);
    public SekcjaMetryczki Sekcja1 => Sekcja(1);
    public SekcjaMetryczki Sekcja2 => Sekcja(2);
    public SekcjaMetryczki Sekcja3 => Sekcja(3);
    public SekcjaMetryczki Sekcja4 => Sekcja(4);
    public SekcjaMetryczki Sekcja5 => Sekcja(5);
    public SekcjaMetryczki Sekcja6 => Sekcja(6);
    public SekcjaMetryczki Sekcja7 => Sekcja(7);


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

/// <summary>Sekcja kafelków jednej kategorii metryczki: kafelek na każdą pozycję aktualności.</summary>
public class SekcjaMetryczki
{
    public static readonly SekcjaMetryczki Pusta = new SekcjaMetryczki(null, new PozycjaMetryczki[0]);

    readonly PozycjaMetryczki[] pozycje;

    public SekcjaMetryczki(string tytul, PozycjaMetryczki[] pozycje)
    {
        Tytul = tytul;
        this.pozycje = pozycje;
    }

    public bool Jest => Tytul != null;

    public string Tytul { get; }

    PozycjaMetryczki Pozycja(int i) => i < pozycje.Length ? pozycje[i] : PozycjaMetryczki.Pusta;
    public PozycjaMetryczki Pozycja0 => Pozycja(0);
    public PozycjaMetryczki Pozycja1 => Pozycja(1);
    public PozycjaMetryczki Pozycja2 => Pozycja(2);
    public PozycjaMetryczki Pozycja3 => Pozycja(3);
    public PozycjaMetryczki Pozycja4 => Pozycja(4);
    public PozycjaMetryczki Pozycja5 => Pozycja(5);
    public PozycjaMetryczki Pozycja6 => Pozycja(6);
    public PozycjaMetryczki Pozycja7 => Pozycja(7);

    /// <summary>Punkty opisu z generatora: &lt;li&gt;&lt;b&gt;Zakres danych&lt;/b&gt;: Dane&lt;/li&gt;.</summary>
    public static PozycjaMetryczki[] Pozycje(string html) => Regex.Matches(html ?? "", @"<li><b>(.*?)</b>:\s*(.*?)</li>")
        .Select(m => new PozycjaMetryczki(WebUtility.HtmlDecode(m.Groups[2].Value), WebUtility.HtmlDecode(m.Groups[1].Value)))
        .Take(MetryczkaAktualnosciExtender.LiczbaPozycji)
        .ToArray();
}

/// <summary>Kafelek jednej informacji metryczki: tytuł = Dane, opis = Zakres danych.</summary>
public class PozycjaMetryczki
{
    public static readonly PozycjaMetryczki Pusta = new PozycjaMetryczki(null, "");

    public PozycjaMetryczki(string dane, string zakresDanych)
    {
        Dane = dane;
        ZakresDanych = zakresDanych;
    }

    public bool Jest => Dane != null;

    public string Dane { get; }

    public string ZakresDanych { get; }

    /// <summary>Kliknięcie kafelka: pełne aktualności (komentarze, polubienia) w Komunikacja → Moje aktualności.</summary>
    public object Otworz() => new NavigationResult("Komunikacja/Moje aktualności");
}
