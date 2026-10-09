using Soneta.Business;

namespace A1.Rezerwy;

/// <summary>Rezerwa urlopowa jednego pracownika w miesiącu (migawka z chwili naliczenia).</summary>
public class PozycjaRezerwyUrlopowej : RezerwyModule.PozycjaRezerwyUrlopowejRow {

    static PozycjaRezerwyUrlopowej() {
        RezerwyModule.PozycjaRezerwyUrlopowejSchema.AddKwotaAfterEdit(pozycja => {
            if (pozycja.State != RowState.Detached)
                pozycja.Session.Verifiers.Add(new UjemnaRezerwaVerifier((PozycjaRezerwyUrlopowej)pozycja));
        });
    }

    public PozycjaRezerwyUrlopowej(RezerwaUrlopowa rezerwa) : base(rezerwa) { }

    public PozycjaRezerwyUrlopowej(RowCreator creator) : base(creator) { }

    /// <summary>Pozycje zamkniętego miesiąca są tylko do odczytu.</summary>
    public override bool IsReadOnly() => base.IsReadOnly() || (Rezerwa != null && Rezerwa.Zamknieta);

    /// <summary>Dni rezerwy (godziny rezerwy / godzin w dniu urlopu).</summary>
    public double DniRezerwy => GodzinNaDzien > 0 ? System.Math.Round(GodzinyRezerwy / GodzinNaDzien, 2) : 0;

    public override string ToString() => $"{Rezerwa} - {Pracownik}";
}
