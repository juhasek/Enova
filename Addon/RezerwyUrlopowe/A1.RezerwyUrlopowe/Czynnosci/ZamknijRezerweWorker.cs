using System;
using Soneta.Business;
using Soneta.Types;

[assembly: Worker(typeof(A1.Rezerwy.ZamknijRezerweWorker), typeof(A1.Rezerwy.RezerwaUrlopowa))]

namespace A1.Rezerwy;

/// <summary>„Zamknij miesiąc” - blokuje ponowne naliczenie i zmiany pozycji rezerwy.</summary>
public class ZamknijRezerweWorker {

    [Context]
    public RezerwaUrlopowa Rezerwa { get; set; }

    [Action("Zamknij miesiąc", Target = ActionTarget.ToolbarWithText | ActionTarget.Menu, Mode = ActionMode.SingleSession, Priority = 20)]
    public void Zamknij() {
        using (ITransaction transakcja = Rezerwa.Session.Logout(true)) {
            Rezerwa.StanRezerwy = StanRezerwyUrlopowej.Zamknieta;
            Rezerwa.DataZamkniecia = DateTime.Now;
            Rezerwa.ZamknalOperator = Rezerwa.Session.Login.Operator?.ToString() ?? "";
            transakcja.Commit();
        }
    }

    public bool IsEnabledZamknij() => Rezerwa != null && !Rezerwa.Zamknieta;
}
