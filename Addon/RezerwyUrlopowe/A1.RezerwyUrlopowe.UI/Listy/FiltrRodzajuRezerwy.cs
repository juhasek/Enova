using Soneta.Types;

namespace A1.Rezerwy.UI;

/// <summary>Filtr rodzaju na listach rezerw.</summary>
public enum FiltrRodzajuRezerwy {
    [Caption("Wszystkie")] Wszystkie = 0,
    [Caption("Rezerwa")] Rezerwa = 1,
    [Caption("Budżet")] Budzet = 2,
}
