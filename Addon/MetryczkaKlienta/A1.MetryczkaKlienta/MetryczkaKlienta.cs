using System;
using System.Collections.Generic;
using System.Linq;
using A1.Metryczka;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Types;

// Okno administracyjne (podgląd wszystkich cech TST_ + Czynności Utwórz/Usuń) w module Ogólne.
// Nie w Ustawieniach: drzewo Ustawień (ConfigurationFolderViewAttribute) składa się tylko ze stron Config.*.pageform.xml.
// Sekcja na ekranie modułu Kadry i płace to FolderMenu-KadryIPlace.form.xml + MetryczkaKafelekExtender.
[assembly: FolderView("Ogólne/Metryczka klienta (TST_)",
    ObjectType = typeof(MetryczkaKlienta),
    Description = "Cechy globalne TST_ z metryczki klienta: podgląd, tworzenie i usuwanie definicji")]

namespace A1.Metryczka;

/// <summary>Jedna cecha globalna metryczki (wiersz karty).</summary>
public class CechaMetryczki
{
    public string Kategoria { get; init; }
    public string Nazwa { get; init; }
    public string Opis { get; init; }
    /// <summary>Kolumna "Zakres danych" metryczki (na widokach zamiast opisu).</summary>
    public string ZakresDanych { get; init; }
    /// <summary>Tekst wartości; null = nieuzupełniona (Bool bez zaznaczenia, 0, pusty tekst, pusta data/miesiąc).</summary>
    public string Wartosc { get; init; }
    /// <summary>Jedyny warunek "uzupełniona" - używa go karta i podsumowanie kafelka.</summary>
    public bool Uzupelniona => Wartosc != null;
}

/// <summary>
/// Obiekt okna "Ogólne / Metryczka klienta (TST_)": karta metryczki (KartaMetryczki) budowana przy każdym odczycie
/// z definicji cech globalnych z prefiksem TST_ - nowa cecha dodana w Opcjach pojawia się sama.
/// </summary>
public class MetryczkaKlienta
{
    public MetryczkaKlienta(Session session)
    {
        Session = session;
    }

    public Session Session { get; }

    public static IEnumerable<FeatureDefinition> DefinicjeTST(Session session) =>
        session.Global.Table.FeatureDefinitions.Where(fd => fd.Name.StartsWith(DefinicjeCech.Prefiks, StringComparison.Ordinal));

    public static List<CechaMetryczki> Cechy(Session session) => DefinicjeTST(session)
        .Select(fd => new CechaMetryczki {
            Kategoria = string.IsNullOrEmpty(fd.Category) ? "Inne" : fd.Category,
            Nazwa = fd.Name,
            Opis = fd.Description,
            ZakresDanych = DefinicjeCech.Etykieta(fd.Name) ?? fd.Description,
            Wartosc = Tekst(session.Global.Features[fd.Name]),
        })
        .ToList();

    // enova zapisuje w Features tylko Bool = Tak (Nie/nieustawiony = brak wiersza), więc "Nie" nie da się odróżnić
    // od nieuzupełnionego - decyzja 27.10: oba jako brak. Liczba 0 = domyślna, też brak.
    static string Tekst(object v) => v switch {
        bool b => b ? "Tak" : null,
        int i when i == 0 => null,
        double d when d == 0 => null,
        decimal m when m == 0 => null,
        Date d when d == Date.Empty => null,
        YearMonth ym when ym == YearMonth.Empty => null,
        _ => string.IsNullOrWhiteSpace(v?.ToString()) ? null : v.ToString(),
    };

    /// <summary>
    /// Cechy w świeżej sesji: sesja ekranu modułu i okna powstaje przy otwarciu i nie widzi wartości zmienionych
    /// później w Opcjach ani definicji dodanych/usuniętych czynnościami (zapis w osobnej sesji).
    /// </summary>
    List<CechaMetryczki> CechyAktualne()
    {
        using (Session sesja = Session.Login.CreateSession(true, false, "Metryczka - odczyt cech"))
            return Cechy(sesja);
    }

    public string KartaHtml => KartaMetryczki.Html(CechyAktualne());

    public string Podsumowanie
    {
        get {
            var cechy = CechyAktualne();
            return cechy.Count == 0
                ? "Brak cech TST_ – Czynności → Utwórz cechy testowe (TST_)"
                : $"Uzupełniono {cechy.Count(c => c.Uzupelniona)} z {cechy.Count} pozycji";
        }
    }
}
