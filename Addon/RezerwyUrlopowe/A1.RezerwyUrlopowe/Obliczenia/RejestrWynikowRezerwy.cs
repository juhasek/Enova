using System;
using System.Collections.Concurrent;

namespace A1.Rezerwy;

/// <summary>
/// Przekazanie pełnych wyników obliczeń z algorytmu elementu (naliczanie planowanej listy płac, osobna sesja silnika)
/// do czynności naliczenia rezerwy, która zapisuje pozycje. Klucz: pracownik + miesiąc + rodzaj.
/// Czynność czyści wpisy po odczycie; gdy planowana lista była naliczana ręcznie (bez czynności), wpisy zostają
/// nadpisane przy kolejnym naliczeniu tego samego pracownika i miesiąca.
/// </summary>
public static class RejestrWynikowRezerwy {

    static readonly ConcurrentDictionary<(Guid pracownik, int rok, int miesiac, RodzajRezerwyUrlopowej rodzaj), WynikRezerwyUrlopowej> wyniki = new();

    public static void Zapisz(Guid pracownik, int rok, int miesiac, RodzajRezerwyUrlopowej rodzaj, WynikRezerwyUrlopowej wynik) =>
        wyniki[(pracownik, rok, miesiac, rodzaj)] = wynik;

    /// <summary>Pobiera i usuwa wynik; null, gdy algorytm nie liczył tego pracownika.</summary>
    public static WynikRezerwyUrlopowej Pobierz(Guid pracownik, int rok, int miesiac, RodzajRezerwyUrlopowej rodzaj) =>
        wyniki.TryRemove((pracownik, rok, miesiac, rodzaj), out WynikRezerwyUrlopowej wynik) ? wynik : null;
}
