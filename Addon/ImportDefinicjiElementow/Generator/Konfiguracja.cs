using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImportDefinicjiElementow.Generator;

/// <summary>
/// schema/zrodla.json - co i skąd generator czyta. Utrzymywane ręcznie, rzadko zmieniane.
/// </summary>
sealed class Konfiguracja
{
    /// <summary>Pełna nazwa klasy wiersza, np. Soneta.Place.DefinicjaElementu.</summary>
    public string TypObiektu { get; set; }
    /// <summary>Pełna nazwa klasy rekordu (pola bazodanowe, [Required], [MaxLength], wartości początkowe).</summary>
    public string TypRekordu { get; set; }
    public string BibliotekaLogiki { get; set; }
    public string BibliotekaUI { get; set; }

    /// <summary>Wersja formatu schematu/pliku klienta (major = zgodność importu).</summary>
    public string WersjaSchematu { get; set; }

    /// <summary>Zakładki formularza w kolejności enova (wg Priority).</summary>
    public List<KonfZakladka> Zakladki { get; set; } = new();

    /// <summary>Pola spoza zakładek (np. rodzaj elementu wybierany przy dodawaniu) - wstawiane na początek zakładki.</summary>
    public List<KonfPoleDodatkowe> PolaDodatkowe { get; set; } = new();

    /// <summary>
    /// Wyrażenia z formularzy, których nie da się rozwinąć mechanicznie (extendery UI) → ścieżka właściwości.
    /// Np. "new DefinicjaElementuStazExtender.GetKreatorAlgorytmu()" → "Algorytm.KreatorAlgorytmu".
    /// </summary>
    public Dictionary<string, string> Wyrazenia { get; set; } = new();

    /// <summary>
    /// Ścieżki UI wskazujące ten sam zapis w bazie → ścieżka kanoniczna (prefiks → prefiks).
    /// Np. Algorytm.Edytor.Korekta. → Algorytm.KreatorAlgorytmu.Korekta.
    /// </summary>
    public Dictionary<string, string> Aliasy { get; set; } = new();

    /// <summary>Pola pominięte w formularzu klienta: ścieżka → powód (dla PRZEGLAD.md).</summary>
    public Dictionary<string, string> Pominiete { get; set; } = new();

    /// <summary>Słowniki z wartościami standardowymi z zasobów *.dbinit.xml.</summary>
    public Dictionary<string, KonfSlownik> Slowniki { get; set; } = new();

    /// <summary>Zasoby *.dbinit.xml z definicjami standardowymi (lista nazw do ostrzeżeń przy imporcie).</summary>
    public List<string> DefinicjeStandardowe { get; set; } = new();

    public static Konfiguracja Wczytaj(string plik) =>
        JsonSerializer.Deserialize<Konfiguracja>(File.ReadAllText(plik), Json.Opcje);
}

sealed class KonfZakladka
{
    /// <summary>Identyfikator zakładki w schemacie (bez polskich znaków).</summary>
    public string Id { get; set; }
    /// <summary>Nazwa zasobu pageform, np. DefinicjaElementuPro (bez prefiksu i rozszerzenia).</summary>
    public string Zasob { get; set; }
    /// <summary>Opcjonalna etykieta zastępująca CaptionHtml strony.</summary>
    public string Etykieta { get; set; }
}

sealed class KonfPoleDodatkowe
{
    public string Sciezka { get; set; }
    public string Zakladka { get; set; }
    public string Etykieta { get; set; }
}

sealed class KonfSlownik
{
    /// <summary>Etykieta słownika dla użytkownika.</summary>
    public string Etykieta { get; set; }
    /// <summary>Zasoby *.dbinit.xml (sufiksy nazw) z wartościami standardowymi.</summary>
    public List<string> Zasoby { get; set; } = new();
    /// <summary>Nazwa elementu XML rekordu, np. PozycjaPIT.</summary>
    public string Element { get; set; }
    /// <summary>Pole identyfikujące rekord w bazie (po nim import szuka wartości).</summary>
    public string PoleKlucza { get; set; }
    /// <summary>Pole z opisem do podpowiedzi.</summary>
    public string PoleOpisu { get; set; }
    /// <summary>Pole z okresem obowiązywania (pozycje historyczne są pomijane).</summary>
    public string PoleOkresu { get; set; }
}

static class Json
{
    public static readonly JsonSerializerOptions Opcje = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
