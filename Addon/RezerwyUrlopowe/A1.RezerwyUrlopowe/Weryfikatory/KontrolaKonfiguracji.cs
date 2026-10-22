using System.Collections.Generic;
using Soneta.Place;

namespace A1.Rezerwy;

/// <summary>
/// Sprawdzenie konfiguracji dodatku przed naliczeniem (konfiguracja nie jest wierszem bazy, więc zamiast
/// weryfikatora sesji - lista błędów zwracana czynności naliczenia).
/// </summary>
public static class KontrolaKonfiguracji {

    public static List<string> Sprawdz(UstawieniaRezerwUrlopowych ustawienia, RodzajRezerwyUrlopowej rodzaj) {
        List<string> bledy = new List<string>();
        DefinicjaElementu element = ustawienia.Element(rodzaj);
        DefinicjaPlanowanejListyPłac planowanaLista = ustawienia.PlanowanaLista(rodzaj);

        if (element == null)
            bledy.Add($"Brak elementu wynagrodzenia dla rodzaju {rodzaj} (Narzędzia → Opcje → Kadry i płace → Rezerwy urlopowe).");
        else {
            if (element.RodzajNaliczania != RodzajeNaliczaniaListPłac.TylkoPlanowane)
                bledy.Add($"Element „{element.Nazwa}”: Rodzaj naliczania musi być „Tylko planowane”.");
            if (element.DoWyplaty)
                bledy.Add($"Element „{element.Nazwa}”: pole „Do wypłaty” musi być wyłączone.");
            if (element.Algorytm.Typ != TypAlgorytmuElementu.KlasaAlgorytmu || element.Algorytm.Klasa.Nazwa != typeof(AlgorytmRezerwy).FullName)
                bledy.Add($"Element „{element.Nazwa}”: algorytm musi być klasą {typeof(AlgorytmRezerwy).FullName}.");
        }

        if (planowanaLista == null)
            bledy.Add($"Brak definicji planowanej listy płac dla rodzaju {rodzaj}.");
        else {
            if (element != null && planowanaLista.Element != element)
                bledy.Add($"Planowana lista {planowanaLista.Symbol}: pole Element musi wskazywać „{element.Nazwa}”.");
            if (string.IsNullOrWhiteSpace(planowanaLista.Numeracja.Wzor))
                bledy.Add($"Planowana lista {planowanaLista.Symbol}: brak wzoru numeracji.");
        }

        if (ustawienia.MiesiecyPodstawy < 1 || ustawienia.MiesiecyPodstawy > 12)
            bledy.Add("Liczba miesięcy podstawy musi mieścić się w zakresie 1-12.");
        return bledy;
    }
}
