using Soneta.Business;
using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>Ujemna kwota rezerwy jest dopuszczalna tylko przy włączonej opcji „Dopuszczaj ujemną rezerwę”.</summary>
internal sealed class UjemnaRezerwaVerifier(PozycjaRezerwyUrlopowej pozycja)
    : ColVerifier<PozycjaRezerwyUrlopowej>(pozycja, nameof(PozycjaRezerwyUrlopowej.Kwota)) {

    protected override bool IsValid() =>
        Row.Kwota.Value >= 0 || new UstawieniaRezerwUrlopowych(Row.Session).DopuszczajUjemna;

    public override string Description => "Ujemna rezerwa urlopowa jest niedozwolona w konfiguracji dodatku.";

    public override VerifierType Type => VerifierType.Error;
}
