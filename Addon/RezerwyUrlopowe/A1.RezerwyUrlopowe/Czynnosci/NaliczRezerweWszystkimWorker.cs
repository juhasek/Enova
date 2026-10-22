using System.Collections.Generic;
using Soneta.Business;
using Soneta.Business.Licence;
using Soneta.Business.UI;
using Soneta.Kadry;
using Soneta.Types;

// Czynność na liście Rezerwy urlopowe - wszyscy zatrudnieni na koniec miesiąca.
[assembly: Worker(typeof(A1.Rezerwy.NaliczRezerweWszystkimWorker), typeof(A1.Rezerwy.RezerwyUrlopowe))]

namespace A1.Rezerwy;

/// <summary>„Nalicz rezerwę urlopową...” na liście rezerw - dla wszystkich zatrudnionych na koniec miesiąca.</summary>
public class NaliczRezerweWszystkimWorker {

    [Context]
    public Context Kontekst { get; set; }

    [Context]
    public NaliczRezerweParams Parametry { get; set; }

    [Action("Nalicz rezerwę urlopową...", Target = ActionTarget.ToolbarWithText | ActionTarget.Menu,
            Mode = ActionMode.SingleSession | ActionMode.Progress, Icon = ActionIcon.Wizard, Priority = 10,
            Contexts = new object[] { LicencjeModułu.PL_Platynowy })]
    public object Nalicz() {
        Date koniecMiesiaca = ((FromTo)Parametry.Okres).To;
        List<Pracownik> zatrudnieni = new List<Pracownik>();
        foreach (Pracownik pracownik in KadryModule.GetInstance(Kontekst.Session).Pracownicy) {
            PracHistoria historia = pracownik[koniecMiesiaca];
            if (historia != null && historia.Etat.OkresZatrudnienia.Contains(koniecMiesiaca))
                zatrudnieni.Add(pracownik);
        }
        NaliczanieRezerwyUrlopowej naliczanie = new NaliczanieRezerwyUrlopowej(Kontekst);
        naliczanie.Nalicz(Parametry.Rodzaj, Parametry.Okres, zatrudnieni, Parametry.NaliczPonownie);
        return new MessageBoxInformation("Rezerwa urlopowa", string.Join("\n", naliczanie.Komunikaty));
    }
}
