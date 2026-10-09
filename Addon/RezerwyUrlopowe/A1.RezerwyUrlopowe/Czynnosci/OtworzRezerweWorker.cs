using System;
using Soneta.Business;
using Soneta.Types;

[assembly: Worker(typeof(A1.Rezerwy.OtworzRezerweWorker), typeof(A1.Rezerwy.RezerwaUrlopowa))]

namespace A1.Rezerwy;

/// <summary>
/// „Otwórz miesiąc” - przywraca stan Naliczona. Docelowo osobne prawo w drzewie uprawnień
/// (otwarta kwestia nr 11 planu); do czasu decyzji - czynność dostępna dla operatorów z prawem edycji rezerwy.
/// </summary>
public class OtworzRezerweWorker {

    [Context]
    public RezerwaUrlopowa Rezerwa { get; set; }

    [Action("Otwórz miesiąc", Target = ActionTarget.Menu, Mode = ActionMode.SingleSession, Priority = 30)]
    public void Otworz() {
        Rezerwa.StanRezerwy = StanRezerwyUrlopowej.Naliczona;
        Rezerwa.DataZamkniecia = DateTime.MinValue;
        Rezerwa.ZamknalOperator = "";
    }

    public bool IsEnabledOtworz() => Rezerwa != null && Rezerwa.Zamknieta;
}
