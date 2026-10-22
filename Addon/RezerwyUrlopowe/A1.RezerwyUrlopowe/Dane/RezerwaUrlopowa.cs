namespace A1.Rezerwy;

/// <summary>Rezerwa urlopowa albo budżet rezerwy za jeden miesiąc (nagłówek z pozycjami pracowników).</summary>
public class RezerwaUrlopowa : RezerwyModule.RezerwaUrlopowaRow {

    static RezerwaUrlopowa() {
        RezerwyModule.RezerwaUrlopowaSchema.AddRodzajAfterEdit(rezerwa => UzbrojWeryfikatory((RezerwaUrlopowa)rezerwa));
        RezerwyModule.RezerwaUrlopowaSchema.AddMiesiacAfterEdit(rezerwa => UzbrojWeryfikatory((RezerwaUrlopowa)rezerwa));
    }

    static void UzbrojWeryfikatory(RezerwaUrlopowa rezerwa) {
        if (rezerwa.State != Soneta.Business.RowState.Detached)
            rezerwa.Session.Verifiers.Add(new BudzetPozaSierpniemVerifier(rezerwa));
    }

    public bool Zamknieta => StanRezerwy == StanRezerwyUrlopowej.Zamknieta;

    public override string ToString() => $"{Rodzaj} {Rok}/{Miesiac:00}";
}
