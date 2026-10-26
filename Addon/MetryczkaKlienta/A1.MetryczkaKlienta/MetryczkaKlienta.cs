using System;
using System.Collections.Generic;
using System.Linq;
using A1.Metryczka;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Config;
using Soneta.Types;

// Sekcja (folder menu) przed standardowymi "Kadry" (Priority=1) i "Płace" (2) - rozszerzenie przez atrybut, bez zmian standardu.
[assembly: FolderView("Kadry i płace/Metryczka klienta", Priority = 0,
    Description = "Cechy globalne klienta (TST_) z metryczki")]
[assembly: FolderView("Kadry i płace/Metryczka klienta/Metryczka", Priority = 1,
    ObjectType = typeof(MetryczkaKlienta),
    Description = "Cechy globalne TST_ wg kategorii: najpierw uzupełnione, potem nieaktywne")]

namespace A1.Metryczka;

/// <summary>Jedna cecha globalna w oknie metryczki.</summary>
public class CechaMetryczki
{
    public string Kategoria { get; init; }
    public string Nazwa { get; init; }
    public string Opis { get; init; }
    public string Wartosc { get; init; }
    public bool Aktywna { get; init; }
}

/// <summary>
/// Obiekt okna "Kadry i płace / Metryczka klienta". Lista cech budowana przy każdym odczycie
/// z definicji cech globalnych z prefiksem TST_ - nowa cecha dodana w Opcjach pojawia się sama.
/// Lista (Grid) zamiast pól generowanych w układzie, bo układ formularza jest cache'owany.
/// </summary>
public class MetryczkaKlienta
{
    const string WezelDodatku = "A1MetryczkaKlienta";
    const string WezelWidocznosci = "Widocznosc";

    public MetryczkaKlienta(Session session)
    {
        Session = session;
        widoczna = CzytajWidocznosc();
    }

    public Session Session { get; }

    public static IEnumerable<FeatureDefinition> DefinicjeTST(Session session) =>
        session.Global.Table.FeatureDefinitions.Where(fd => fd.Name.StartsWith(DefinicjeCech.Prefiks, StringComparison.Ordinal));

    public List<CechaMetryczki> Cechy => DefinicjeTST(Session)
        .Select(fd => {
            object v = Session.Global.Features[fd.Name];
            return new CechaMetryczki {
                Kategoria = string.IsNullOrEmpty(fd.Category) ? "Inne" : fd.Category,
                Nazwa = fd.Name,
                Opis = fd.Description,
                Wartosc = v is bool b ? (b ? "Tak" : "Nie") : v?.ToString(),
                Aktywna = JestAktywna(v),
            };
        })
        .OrderBy(c => c.Kategoria).ThenBy(c => c.Nazwa)
        .ToList();

    public List<CechaMetryczki> Aktywne => Cechy.Where(c => c.Aktywna).ToList();

    public List<CechaMetryczki> Nieaktywne => Cechy.Where(c => !c.Aktywna).ToList();

    public string Podsumowanie
    {
        get {
            var cechy = Cechy;
            return cechy.Count == 0
                ? "Brak cech globalnych z prefiksem TST_ - użyj Czynności → Utwórz cechy testowe (TST_)."
                : $"Cech TST_: {cechy.Count}, uzupełnionych: {cechy.Count(c => c.Aktywna)}.";
        }
    }

    /// <summary>Aktywna = uzupełniona: Bool = Tak, niepusty tekst, liczba ≠ 0, ustawiona data.</summary>
    public static bool JestAktywna(object v) => v switch {
        null => false,
        bool b => b,
        string s => !string.IsNullOrWhiteSpace(s),
        int i => i != 0,
        double d => d != 0,
        decimal m => m != 0,
        Date d => d != Date.Empty,
        YearMonth ym => ym != YearMonth.Empty,
        _ => !string.IsNullOrWhiteSpace(v.ToString()),
    };

    // ---------------- pokaż / ukryj (stan per operator w drzewie konfiguracji) ----------------

    bool widoczna;

    public bool Widoczna => widoczna;

    public string TekstPrzycisku => widoczna ? "Ukryj metryczkę" : "Pokaż metryczkę";

    public void PrzelaczWidocznosc()
    {
        widoczna = !widoczna;
        // Okno ma sesję tylko do odczytu - stan zapisujemy w osobnej sesji konfiguracyjnej.
        using (Session sesja = Session.Login.CreateSession(false, true, "Metryczka - widoczność sekcji"))
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
