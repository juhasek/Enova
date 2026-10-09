namespace A1.Rezerwy;

/// <summary>Rezerwa urlopowa albo budżet rezerwy za jeden miesiąc (nagłówek z pozycjami pracowników).</summary>
public class RezerwaUrlopowa : RezerwyModule.RezerwaUrlopowaRow {

    public override string ToString() => $"{Rodzaj} {Rok}/{Miesiac:00}";
}
