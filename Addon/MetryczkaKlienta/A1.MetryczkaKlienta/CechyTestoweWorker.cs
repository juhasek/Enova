using System.Linq;
using A1.Metryczka;
using Soneta.Business;
using Soneta.Business.Db;
using Soneta.Business.UI;
using Soneta.Config;
using Soneta.Core;

[assembly: Worker(typeof(CechyTestoweWorker), typeof(MetryczkaKlienta))]

namespace A1.Metryczka;

/// <summary>
/// Czynność okna "Metryczka klienta": usunięcie starych cech globalnych TST_ (poprzednia wersja metryczki) po wczytaniu
/// ich wartości do konfiguracji (Opcje → A1Testy → Metryczka klienta → Wczytaj pola z metryczki).
/// Okno pracuje na sesji tylko do odczytu, dlatego zapis idzie przez osobną sesję konfiguracyjną
/// (jedna transakcja, Commit + Save) - jak ZapisDefinicji w Addon/ImportDefinicjiElementow.
/// </summary>
public class CechyTestoweWorker
{
    [Context]
    public MetryczkaKlienta Metryczka { get; set; }

    static FeatureSetDefinition Zestaw(Session sesja) =>
        BusinessModule.GetInstance(sesja).FSDefs.ByHostName[DefinicjeCech.TabelaCechGlobalnych]
            .FirstOrDefault(z => z.Name == DefinicjeCech.NazwaZestawu);

    [Action("Usuń stare cechy TST_", Mode = ActionMode.SingleSession, Target = ActionTarget.Menu)]
    public object Usun()
    {
        int liczba = MetryczkaKlienta.DefinicjeTST(Metryczka.Session).Count();
        if (liczba == 0)
            return "Brak definicji cech z prefiksem TST_.";
        return new MessageBoxInformation("Usuwanie starych cech TST_",
            $"Usunąć {liczba} definicji cech globalnych z prefiksem TST_ razem z ich wartościami? Najpierw wczytaj je w Opcje → A1Testy → Metryczka klienta.")
        {
            Type = MessageBoxInformationType.Warning,
            YesHandler = () => $"Usunięto definicji cech: {UsunDefinicje()}.",
            NoHandler = () => null,
        };
    }

    int UsunDefinicje()
    {
        int usuniete = 0;
        using (Session sesja = Metryczka.Session.Login.CreateSession(false, true, "Metryczka - usuwanie cech TST_"))
        {
            using (ITransaction t = sesja.Logout(true))
            {
                FeatureSetDefinition zestaw = Zestaw(sesja);
                if (zestaw != null)
                {
                    foreach (FeatureSetDefinitionItem pozycja in zestaw.FeatureSetDefinitionItems.ToList())
                        pozycja.Delete();
                    zestaw.Delete();
                }
                foreach (FeatureDefinition fd in MetryczkaKlienta.DefinicjeTST(sesja).ToList())
                {
                    // Usunięcie definicji nie kasuje wartości w Features - po ponownym utworzeniu cechy wróciłyby stare wartości.
                    if (sesja.Global.Features.Exists(fd.Name))
                        sesja.Global.Features.Remove(fd.Name);
                    if (fd.DictionaryList != null)
                        foreach (DictionaryItem pozycja in fd.DictionaryList.ToList())
                            pozycja.Delete();
                    fd.Delete();
                    usuniete++;
                }
                UsunPozostalosci(sesja);
                t.Commit();
            }
            sesja.Save();
        }
        return usuniete;
    }

    /// <summary>
    /// Dane po usuniętych 27.10 funkcjach (żaden kod ich już nie czyta): aktualności z generatora metryczki
    /// i węzeł konfiguracji A1MetryczkaKlienta (stan Ukryj/Pokaż per operator).
    /// </summary>
    static void UsunPozostalosci(Session sesja)
    {
        foreach (Aktualnosc a in CoreModule.GetInstance(sesja).Aktualnosci.Cast<Aktualnosc>()
                     .Where(a => a.Temat != null && a.Temat.StartsWith("Metryczka klienta – ")).ToList())
            a.Delete();
        CfgNode wezel = new CfgManager(sesja).Root?.FindSubNode("A1MetryczkaKlienta", false);
        if (wezel != null)
        {
            wezel.FindSubNode("Widocznosc", false)?.Delete();
            wezel.Delete();
        }
    }
}
