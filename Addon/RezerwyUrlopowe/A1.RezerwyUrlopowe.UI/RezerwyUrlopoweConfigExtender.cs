using Soneta.Business;

// Extender bez typu danych - dostępny w formularzu po nazwie klasy:
// DataContext="{New RezerwyUrlopoweConfigExtender}" (strona Config.RezerwyUrlopowe.pageform.xml).
[assembly: Worker(typeof(A1.Rezerwy.UI.RezerwyUrlopoweConfigExtender))]

namespace A1.Rezerwy.UI;

/// <summary>Kontekst strony Narzędzia → Opcje → Kadry i płace → Rezerwy urlopowe.</summary>
public class RezerwyUrlopoweConfigExtender {

    [Context]
    public Session Session { get; set; }

    UstawieniaRezerwUrlopowych ustawienia;

    public UstawieniaRezerwUrlopowych Ustawienia => ustawienia ??= new UstawieniaRezerwUrlopowych(Session);
}
