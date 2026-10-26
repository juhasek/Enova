using System;
using System.Collections.Generic;
using System.Linq;
using A1.Metryczka;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Types;

// Okno administracyjne (podgląd wszystkich cech TST_ + Czynności Utwórz/Usuń) w module Ogólne.
// Nie w Ustawieniach: drzewo Ustawień (ConfigurationFolderViewAttribute) składa się tylko ze stron Config.*.pageform.xml.
// Sekcja na ekranie modułu Kadry i płace to FolderMenu-KadryIPlace.form.xml + MetryczkaAktualnosciExtender.
[assembly: FolderView("Ogólne/Metryczka klienta (TST_)",
    ObjectType = typeof(MetryczkaKlienta),
    Description = "Cechy globalne TST_ z metryczki klienta: podgląd, tworzenie i usuwanie definicji")]

namespace A1.Metryczka;

/// <summary>Jedna cecha globalna metryczki (wiersz listy / aktualność).</summary>
public class CechaMetryczki
{
    public string Kategoria { get; init; }
    public string Nazwa { get; init; }
    public string Opis { get; init; }
    /// <summary>Kolumna "Zakres danych" metryczki (na widokach zamiast opisu).</summary>
    public string ZakresDanych { get; init; }
    public string Wartosc { get; init; }
    public bool Aktywna { get; init; }
}

/// <summary>
/// Obiekt okna "Ogólne / Metryczka klienta (TST_)". Lista budowana przy każdym odczycie
/// z definicji cech globalnych z prefiksem TST_ - nowa cecha dodana w Opcjach pojawia się sama.
/// Lista (Grid) zamiast pól generowanych w układzie, bo układ formularza jest cache'owany.
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
        .Select(fd => {
            object v = session.Global.Features[fd.Name];
            return new CechaMetryczki {
                Kategoria = string.IsNullOrEmpty(fd.Category) ? "Inne" : fd.Category,
                Nazwa = fd.Name,
                Opis = fd.Description,
                ZakresDanych = DefinicjeCech.Etykieta(fd.Name) ?? fd.Description,
                Wartosc = v is bool b ? (b ? "Tak" : "Nie") : v?.ToString(),
                Aktywna = JestAktywna(v),
            };
        })
        .OrderBy(c => c.Kategoria).ThenBy(c => c.Nazwa)
        .ToList();

    public List<CechaMetryczki> Aktywne => Cechy(Session).Where(c => c.Aktywna).ToList();

    public List<CechaMetryczki> Nieaktywne => Cechy(Session).Where(c => !c.Aktywna).ToList();

    public string Podsumowanie
    {
        get {
            var cechy = Cechy(Session);
            return cechy.Count == 0
                ? "Brak cech globalnych z prefiksem TST_ - użyj Czynności → Utwórz cechy testowe (TST_)."
                : $"Cech TST_: {cechy.Count}, uzupełnionych: {cechy.Count(c => c.Aktywna)}.";
        }
    }

    /// <summary>Aktywna = uzupełniona: Bool = Tak, niepusty tekst, liczba ≠ 0, ustawiona data.</summary>
    public static bool JestAktywna(object v) => v switch {
        null => false,
        bool b => b,
        string s => !string.IsNullOrWhiteSpace(s),
        int i => i != 0,
        double d => d != 0,
        decimal m => m != 0,
        Date d => d != Date.Empty,
        YearMonth ym => ym != YearMonth.Empty,
        _ => !string.IsNullOrWhiteSpace(v.ToString()),
    };

    // "Nieaktywne" domyślnie zwinięte; stan tylko na czas otwarcia okna.
    bool pokazNieaktywne;

    public bool NieaktywneWidoczne => pokazNieaktywne;

    public string TekstNieaktywnych => pokazNieaktywne
        ? "Ukryj nieaktywne"
        : $"Pokaż nieaktywne ({Nieaktywne.Count})";

    public void PrzelaczNieaktywne()
    {
        pokazNieaktywne = !pokazNieaktywne;
        Session.InvokeChanged();
    }
}
