using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>Stan rezerwy (miesiąca). Wartości zapisane w bazie - nowe pozycje tylko na końcu.</summary>
public enum StanRezerwyUrlopowej {
    [Caption("Naliczona")] Naliczona = 1,
    [Caption("Zamknięta")] Zamknieta = 2,
}
