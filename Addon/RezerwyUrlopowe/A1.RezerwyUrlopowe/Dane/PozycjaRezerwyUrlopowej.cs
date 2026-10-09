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

    /// <summary>Kwota pozycji tego pracownika w poprzednim miesiącu (ten sam rodzaj rezerwy); 0, gdy brak.</summary>
    public decimal KwotaPoprzednia {
        get {
            if (Rezerwa == null || Pracownik == null)
                return 0m;
            int poprzedniRok = Rezerwa.Miesiac == 1 ? Rezerwa.Rok - 1 : Rezerwa.Rok;
            int poprzedniMiesiac = Rezerwa.Miesiac == 1 ? 12 : Rezerwa.Miesiac - 1;
            foreach (PozycjaRezerwyUrlopowej pozycja in RezerwyModule.GetInstance(Session).PozRezerwUrlop.WgPracownikaHistoria[Pracownik])
                if (pozycja.Rezerwa.Rodzaj == Rezerwa.Rodzaj && pozycja.Rezerwa.Rok == poprzedniRok && pozycja.Rezerwa.Miesiac == poprzedniMiesiac)
                    return pozycja.Kwota.Value;
            return 0m;
        }
    }

    /// <summary>Zmiana rezerwy miesiąc do miesiąca (dodatnia = zawiązanie, ujemna = rozwiązanie).</summary>
    public decimal Zmiana => Kwota.Value - KwotaPoprzednia;

    /// <summary>Dni rezerwy (godziny rezerwy / godzin w dniu urlopu).</summary>
    public double DniRezerwy => GodzinNaDzien > 0 ? System.Math.Round(GodzinyRezerwy / GodzinNaDzien, 2) : 0;

    public override string ToString() => $"{Rezerwa} - {Pracownik}";
}
