using System.Collections.Generic;
using Soneta.Business;
using Soneta.Business.Licence;
using Soneta.Business.UI;
using Soneta.Kadry;
using Soneta.Place;
using Soneta.Types;

// Czynność na liście Pracownicy (zaznaczeni pracownicy) - jak standardowe „Nalicz planowane listy płac...”.
[assembly: Worker(typeof(A1.Rezerwy.NaliczRezerweWorker), typeof(Pracownicy))]

namespace A1.Rezerwy;

/// <summary>„Nalicz rezerwę urlopową...” dla zaznaczonych pracowników.</summary>
public class NaliczRezerweWorker {

    [Context]
    public Context Kontekst { get; set; }

    [Context]
    public Pracownik[] Pracownicy { get; set; }

    [Context]
    public NaliczRezerweParams Parametry { get; set; }

    [Action("Nalicz rezerwę urlopową...", Target = ActionTarget.Menu, Mode = ActionMode.SingleSession | ActionMode.Progress,
            Icon = ActionIcon.Wizard, Priority = 1000, Contexts = new object[] { LicencjeModułu.PL_Platynowy })]
    public object Nalicz() {
        if (Pracownicy == null || Pracownicy.Length == 0)
            return new MessageBoxInformation("Rezerwa urlopowa", "Zaznacz pracowników na liście.");
        NaliczanieRezerwyUrlopowej naliczanie = new NaliczanieRezerwyUrlopowej(Kontekst);
        naliczanie.Nalicz(Parametry.Rodzaj, Parametry.Okres, Pracownicy, Parametry.NaliczPonownie);
        return new MessageBoxInformation("Rezerwa urlopowa", string.Join("\n", naliczanie.Komunikaty));
    }

    public static bool IsVisibleNalicz(Context context) => PlanListyPlac.GetEnabled(context);
}
