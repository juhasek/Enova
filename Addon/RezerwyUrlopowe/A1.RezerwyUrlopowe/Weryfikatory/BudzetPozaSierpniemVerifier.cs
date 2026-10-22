using Soneta.Business;
using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>Budżet rezerwy liczony jest zwykle w sierpniu - ostrzeżenie przy innym miesiącu.</summary>
internal sealed class BudzetPozaSierpniemVerifier(RezerwaUrlopowa rezerwa)
    : MultiColVerifier<RezerwaUrlopowa>(rezerwa, nameof(RezerwaUrlopowa.Rodzaj), nameof(RezerwaUrlopowa.Miesiac)) {

    public const int MiesiacBudzetu = 8;

    protected override bool IsValid() => Row.Rodzaj != RodzajRezerwyUrlopowej.Budzet || Row.Miesiac == MiesiacBudzetu;

    public override string Description => "Budżet rezerwy urlopowej liczony jest zwykle w sierpniu - sprawdź okres.";

    public override VerifierType Type => VerifierType.Warning;
}
