using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>Rodzaj rezerwy urlopowej. Wartości zapisane w bazie - nowe pozycje tylko na końcu.</summary>
public enum RodzajRezerwyUrlopowej {
    [Caption("Rezerwa")] Rezerwa = 1,
    [Caption("Budżet")] Budzet = 2,
}
