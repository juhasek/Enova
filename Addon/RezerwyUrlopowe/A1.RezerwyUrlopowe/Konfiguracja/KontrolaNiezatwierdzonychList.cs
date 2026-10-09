using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>Co zrobić, gdy w miesiącu rezerwy są niezatwierdzone wypłaty etatowe.</summary>
public enum KontrolaNiezatwierdzonychList {
    [Caption("Blokada naliczenia")] Blokada = 1,
    [Caption("Tylko ostrzeżenie")] Ostrzezenie = 2,
}
