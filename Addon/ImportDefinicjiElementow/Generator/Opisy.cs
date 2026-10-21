using System.Text.Json;
using System.Text.Json.Nodes;

namespace ImportDefinicjiElementow.Generator;

/// <summary>
/// schema/opisy.json - część utrzymywana ręcznie: etykiety, opisy dla kadrowej, warunki widoczności,
/// wymagalności i edytowalności, wartości domyślne, opisy wartości enumów, szablony.
/// Generator nigdy jej nie nadpisuje - dopisuje tylko szkielety dla nowych pól (z flagą opisDoWeryfikacji).
/// </summary>
sealed class Opisy
{
    public Dictionary<string, OpisZakladki> Zakladki { get; set; } = new();
    public Dictionary<string, OpisSekcji> Sekcje { get; set; } = new();
    public Dictionary<string, OpisPola> Pola { get; set; } = new();
    public Dictionary<string, Dictionary<string, OpisWartosci>> Enumy { get; set; } = new();

    public static Opisy Wczytaj(string plik) =>
        File.Exists(plik) ? JsonSerializer.Deserialize<Opisy>(File.ReadAllText(plik), Json.Opcje) : new Opisy();

    public void Zapisz(string plik) => File.WriteAllText(plik, JsonSerializer.Serialize(this, Json.Opcje));
}

sealed class OpisZakladki
{
    public string Etykieta { get; set; }
    public string Opis { get; set; }
    public JsonNode WidocznaGdy { get; set; }
    /// <summary>Zakładka pominięta w formularzu klienta (z powodem).</summary>
    public string Pomin { get; set; }
}

sealed class OpisSekcji
{
    public string Etykieta { get; set; }
    public string Opis { get; set; }
    public JsonNode WidocznaGdy { get; set; }
    public string Pomin { get; set; }
}

sealed class OpisPola
{
    public string Etykieta { get; set; }
    /// <summary>Pełny opis pod ikoną „?” (1-3 zdania, bez żargonu, z przykładem).</summary>
    public string Opis { get; set; }
    /// <summary>Krótka podpowiedź pod polem.</summary>
    public string Podpowiedz { get; set; }
    public JsonNode Domyslna { get; set; }
    public bool? Wymagane { get; set; }
    public JsonNode WymaganeGdy { get; set; }
    public JsonNode WidocznyGdy { get; set; }
    public JsonNode EdytowalnyGdy { get; set; }
    /// <summary>Walidacje: min/max/wzorzec/unikalne.</summary>
    public JsonNode Walidacje { get; set; }
    /// <summary>Ograniczenie listy wartości enuma (np. zależnie od rodzaju elementu).</summary>
    public JsonNode DozwoloneWartosci { get; set; }
    /// <summary>Wymagana licencja (np. PLXL = platynowa, złota/platynowa).</summary>
    public string Licencja { get; set; }
    public bool? OpisDoWeryfikacji { get; set; }
    public bool? DomyslnaDoWeryfikacji { get; set; }
    public string ZrodloOpisu { get; set; }
    /// <summary>Etykiety kolumn listy (ścieżka kolumny → etykieta).</summary>
    public Dictionary<string, string> Kolumny { get; set; }
    /// <summary>Uwagi dla wdrożeniowca/importu (nie pokazywane klientowi).</summary>
    public string UwagiTechniczne { get; set; }
}

sealed class OpisWartosci
{
    public string Etykieta { get; set; }
    public string Opis { get; set; }
    public bool? Ukryj { get; set; }
}
