using System.Linq;
using System.Net;
using A1.Metryczka;
using Soneta.Business;
using Soneta.Business.App;
using Soneta.Core;
using Soneta.Core.Conversation.Models;
using Soneta.Business.Db;
using Soneta.Business.UI;

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

    /// <summary>
    /// Aktualności (Soneta.Core.Aktualnosc) z uzupełnionych cech TST_: jedna na kategorię, odbiorcy = aktywni operatorzy.
    /// Każde uruchomienie dodaje nowy komplet (stan metryczki na dzień) - poprzednie zostają w historii aktualności.
    /// </summary>
    [Action("Generuj aktualności z metryczki", Mode = ActionMode.SingleSession, Target = ActionTarget.Menu)]
    public object GenerujAktualnosci()
    {
        var kategorie = MetryczkaKlienta.Cechy(Metryczka.Session).Where(c => c.Aktywna).GroupBy(c => c.Kategoria).ToList();
        if (kategorie.Count == 0)
            return "Brak uzupełnionych cech TST_ - nie ma z czego wygenerować aktualności.";
        using (Session sesja = Metryczka.Session.Login.CreateSession(false, false, "Metryczka - aktualności"))
        {
            Operator[] operatorzy = BusinessModule.GetInstance(sesja).Operators.Cast<Operator>().Where(o => !o.Locked).ToArray();
            using (ITransaction t = sesja.Logout(true))
            {
                foreach (var kategoria in kategorie)
                {
                    var aktualnosc = new Aktualnosc();
                    sesja.AddRow(aktualnosc);
                    aktualnosc.Temat = MetryczkaAktualnosciExtender.PrefiksTematu + kategoria.Key;
                    aktualnosc.Opis = "<ul>" + string.Concat(kategoria.Select(c =>
                        $"<li><b>{WebUtility.HtmlEncode(c.ZakresDanych)}</b>: {WebUtility.HtmlEncode(c.Wartosc)}</li>")) + "</ul>";
                    // Autor dyskusji bywa już uczestnikiem - bez duplikatów.
                    var obecni = aktualnosc.Users.Select(u => u.Guid).ToHashSet();
                    foreach (Operator op in operatorzy.Where(o => !obecni.Contains(o.Guid)))
                        sesja.AddRow(new DiscussionParticipant(aktualnosc.Dyskusja, op));
                }
                t.Commit();
            }
            sesja.Save();
        }
        return $"Wygenerowano aktualności: {kategorie.Count} (kategorie), odbiorców: wszyscy aktywni operatorzy.";
    }

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
                    if (fd.DictionaryList != null)
                        foreach (DictionaryItem pozycja in fd.DictionaryList.ToList())
                            pozycja.Delete();
                    fd.Delete();
                    usuniete++;
                }
                t.Commit();
            }
            sesja.Save();
        }
        return usuniete;
    }
}
