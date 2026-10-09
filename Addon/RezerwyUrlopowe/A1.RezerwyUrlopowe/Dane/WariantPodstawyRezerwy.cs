using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>Która podstawa decyduje o kwocie rezerwy. Wartości zapisane w bazie - nowe pozycje tylko na końcu.</summary>
public enum WariantPodstawyRezerwy {
    [Caption("Podstawa 1 (średnia z miesięcy)")] Podstawa1 = 1,
    [Caption("Podstawa 2 (standard ekwiwalentu)")] Podstawa2 = 2,
    [Caption("Wyższa z obu")] Wyzsza = 3,
}
