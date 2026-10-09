using Soneta.Business;
using Soneta.Kadry;

// Extender kartoteki pracownika - zakładka „Rezerwy urlopowe” (Pracownik.RezerwyUrlopowe.pageform.xml).
[assembly: Worker(typeof(A1.Rezerwy.UI.RezerwyPracownikaExtender), typeof(Pracownik))]

namespace A1.Rezerwy.UI;

/// <summary>Historia pozycji rezerw urlopowych pracownika (wszystkie miesiące i rodzaje).</summary>
public class RezerwyPracownikaExtender {

    [Context]
    public Pracownik Pracownik { get; set; }

    public View Pozycje {
        get {
            View widok = RezerwyModule.GetInstance(Pracownik.Session).PozRezerwUrlop.CreateView();
            widok.Condition &= new FieldCondition.Equal("Pracownik", Pracownik);
            return widok;
        }
    }
}
