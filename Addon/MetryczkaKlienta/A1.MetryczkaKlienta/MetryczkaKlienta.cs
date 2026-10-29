using System;
using System.Collections.Generic;
using System.Linq;
using A1.Metryczka;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Types;

// Okno z kartą metryczki (+ Czynności: sprzątanie starych cech TST_) w module Ogólne.
// Pola i wartości: Narzędzia → Opcje → A1Testy → Metryczka klienta (KonfiguracjaMetryczki).
// Sekcja na ekranie modułu Kadry i płace to FolderMenu-KadryIPlace.form.xml + MetryczkaKafelekExtender.
[assembly: FolderView("Ogólne/Metryczka klienta",
    ObjectType = typeof(MetryczkaKlienta),
    // Wymagane przez klienta okienkowego: ObjectView robi CalcObjectPages(...).FirstOrDefault() bez sprawdzenia null.
    ObjectPage = "MetryczkaKlienta.Metryczka.pageform.xml",
    Description = "Karta metryczki klienta; pola i wartości w Narzędzia → Opcje → A1Testy → Metryczka klienta")]

namespace A1.Metryczka;

/// <summary>Jedno pole metryczki na karcie.</summary>
public class CechaMetryczki
{
    public string Kategoria { get; init; }
    /// <summary>Klucz pola (nazwa węzła konfiguracji) - po nim pasek nagłówka karty znajduje dane klienta.</summary>
    public string Nazwa { get; init; }
    /// <summary>Nazwa pola wyświetlana na karcie.</summary>
    public string ZakresDanych { get; init; }
    /// <summary>Tak/Nie (nieustawione = Nie), tekst; null = brak.</summary>
    public string Wartosc { get; init; }
    /// <summary>Jedyny warunek "uzupełniona" - używa go karta i podsumowanie kafelka.</summary>
    public bool Uzupelniona { get; init; }
}

/// <summary>
/// Obiekt okna "Ogólne / Metryczka klienta": karta (KartaMetryczki) budowana przy każdym odczycie z pól konfiguracji -
/// pole dodane w Opcjach pojawia się samo.
/// </summary>
public class MetryczkaKlienta
{
    // Klient okienkowy tworzy obiekt folderu konstruktorem bezparametrowym i wstrzykuje [Context] (jak LogFolder).
    public MetryczkaKlienta()
    {
    }

    public MetryczkaKlienta(Session session)
    {
        Session = session;
    }

    [Context]
    public Session Session { get; set; }

    public static IEnumerable<FeatureDefinition> DefinicjeTST(Session session) =>
        session.Global.Table.FeatureDefinitions.Where(fd => fd.Name.StartsWith(DefinicjeCech.Prefiks, StringComparison.Ordinal));

    public static List<CechaMetryczki> Cechy(Session session) => KonfiguracjaMetryczki.Pola(session)
        .Select(p => new CechaMetryczki {
            Kategoria = p.Kategoria,
            Nazwa = p.Klucz,
            ZakresDanych = p.Nazwa,
            Wartosc = p.WartoscKarty,
            Uzupelniona = p.Uzupelnione,
        })
        .ToList();

    /// <summary>
    /// Wartość starej cechy globalnej TST_ jako tekst (do wczytania pól z metryczki): Bool = Tak/Nie, null = brak
    /// (cecha nieustawiona, liczba 0, pusty tekst/data).
    /// </summary>
    public static string WartoscCechy(Session session, string nazwa)
    {
        FeatureCollection cechy = session.Global.Features;
        if (session.Global.Table.FeatureDefinitions[nazwa, false] == null || !cechy.Exists(nazwa))
            return null;
        return cechy[nazwa] switch {
            bool b => b ? "Tak" : "Nie",
            int i when i == 0 => null,
            double d when d == 0 => null,
            decimal m when m == 0 => null,
            Date d when d == Date.Empty => null,
            YearMonth ym when ym == YearMonth.Empty => null,
            var v => string.IsNullOrWhiteSpace(v?.ToString()) ? null : v.ToString(),
        };
    }

    /// <summary>
    /// Pola w świeżej sesji: sesja ekranu modułu i okna powstaje przy otwarciu i nie widzi zmian zapisanych
    /// później w Opcjach.
    /// </summary>
    List<CechaMetryczki> CechyAktualne()
    {
        using (Session sesja = Session.Login.CreateSession(true, false, "Metryczka - odczyt pól"))
            return Cechy(sesja);
    }

    public string KartaHtml => KartaMetryczki.Html(CechyAktualne());

    public string Podsumowanie
    {
        get {
            var cechy = CechyAktualne();
            return cechy.Count == 0
                ? "Brak pól – Narzędzia → Opcje → A1Testy → Metryczka klienta"
                : $"Uzupełniono {cechy.Count(c => c.Uzupelniona)} z {cechy.Count} pozycji";
        }
    }
}
