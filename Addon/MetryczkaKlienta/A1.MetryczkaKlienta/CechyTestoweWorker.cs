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
/// Czynności okna "Metryczka klienta": zakładanie i usuwanie definicji cech globalnych z prefiksem TST_.
/// Okno pracuje na sesji tylko do odczytu, dlatego zapis idzie przez osobną sesję konfiguracyjną
/// (jedna transakcja, Commit + Save) - jak ZapisDefinicji w Addon/ImportDefinicjiElementow.
/// </summary>
public class CechyTestoweWorker
{
    [Context]
    public MetryczkaKlienta Metryczka { get; set; }

    [Action("Utwórz cechy testowe (TST_)", Mode = ActionMode.SingleSession, Target = ActionTarget.Menu)]
    public object Utworz()
    {
        int dodane = 0;
        using (Session sesja = Metryczka.Session.Login.CreateSession(false, true, "Metryczka - tworzenie cech TST_"))
        {
            using (ITransaction t = sesja.Logout(true))
            {
                FeatureDefinitions definicje = sesja.Global.Table.FeatureDefinitions;
                foreach (DefinicjaCechy d in DefinicjeCech.Wszystkie)
                {
                    // Idempotencja: istniejącej definicji nie zmieniamy (mogła zostać poprawiona ręcznie w Opcjach).
                    if (definicje[d.Nazwa, false] != null)
                        continue;
                    var fd = new FeatureDefinition(DefinicjeCech.TabelaCechGlobalnych);
                    sesja.AddRow(fd);
                    fd.TypeNumber = d.Typ;
                    fd.Name = d.Nazwa;
                    fd.Category = d.Kategoria;
                    fd.Description = d.Opis;
                    if (d.Slownik != null)
                    {
                        // Pole "Nazwa słownika" ma najwyżej 14 znaków (komunikat enova przy zapisie).
                        fd.Dictionary = d.Nazwa.Length <= 14 ? d.Nazwa : d.Nazwa.Substring(0, 14);
                        fd.StrictDictionary = true;
                        foreach (string wartosc in d.Slownik)
                        {
                            var pozycja = new DictionaryItem.ForFeatureDefinition(fd);
                            sesja.AddRow(pozycja);
                            pozycja.Value = wartosc;
                        }
                    }
                    dodane++;
                }
                DodajDoZestawu(sesja);
                t.Commit();
            }
            sesja.Save();
        }
        return $"Utworzono definicji cech: {dodane}, pominięto istniejące: {DefinicjeCech.Wszystkie.Length - dodane}.";
    }

    /// <summary>
    /// Strona Opcje → Cechy globalne (2610) pokazuje tylko cechy z zestawów cech hosta CfgNodes o celu GeneralPage
    /// (DefineItemsExtender.CalcGlobalDescriptors), więc same definicje nie wystarczą.
    /// </summary>
    static void DodajDoZestawu(Session sesja)
    {
        BusinessModule bm = BusinessModule.GetInstance(sesja);
        FeatureSetDefinition zestaw = Zestaw(sesja);
        if (zestaw == null)
        {
            zestaw = new FeatureSetDefinition(DefinicjeCech.TabelaCechGlobalnych);
            bm.FSDefs.AddRow(zestaw);
            zestaw.Name = DefinicjeCech.NazwaZestawu;
            zestaw.Target = FeatureSetDefinitionTargets.GeneralPage;
        }
        var sciezki = zestaw.FeatureSetDefinitionItems.Select(i => i.Path).ToHashSet();
        foreach (FeatureDefinition fd in MetryczkaKlienta.DefinicjeTST(sesja).ToList())
            if (!sciezki.Contains("Features." + fd.Name))
                bm.FSDefItems.AddRow(new FeatureSetDefinitionItem(zestaw, fd));
    }

    static FeatureSetDefinition Zestaw(Session sesja) =>
        BusinessModule.GetInstance(sesja).FSDefs.ByHostName[DefinicjeCech.TabelaCechGlobalnych]
            .FirstOrDefault(z => z.Name == DefinicjeCech.NazwaZestawu);

    [Action("Usuń cechy testowe (TST_)", Mode = ActionMode.SingleSession, Target = ActionTarget.Menu)]
    public object Usun()
    {
        int liczba = MetryczkaKlienta.DefinicjeTST(Metryczka.Session).Count();
        if (liczba == 0)
            return "Brak definicji cech z prefiksem TST_.";
        return new MessageBoxInformation("Usuwanie cech testowych",
            $"Usunąć {liczba} definicji cech globalnych z prefiksem TST_ razem z ich wartościami?")
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
