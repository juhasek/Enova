using System.Text;
using System.Text.Json.Nodes;

namespace ImportDefinicjiElementow.Silnik;

/// <summary>Plik elementy-wynagrodzenia.json zapisany formularzem klienta.</summary>
public sealed class PlikImportu
{
    public const string Format = "enova-definicje-elementow-wynagrodzenia";

    public string WersjaSchematu;
    public string WersjaEnova;
    public string DataEksportu;
    public string Licencja;
    public List<DefinicjaZPliku> Definicje = new();

    public sealed class DefinicjaZPliku
    {
        public string Id;
        public string Szablon;
        public string Uwagi;
        /// <summary>Akceptacja klienta: kto i kiedy (null = niezaakceptowana).</summary>
        public string AkceptacjaOsoba;
        public string AkceptacjaData;
        public JsonObject Wartosci;
        /// <summary>Sprawy z ankiety formularza do wyjaśnienia przez wdrożeniowca (gotowe komunikaty).</summary>
        public List<string> Ankieta = new();
        public List<string> BledyFormularza = new();
    }

    /// <summary>Wczytanie i sprawdzenie formatu oraz zgodności wersji schematu (ta sama wersja główna).</summary>
    public static PlikImportu Wczytaj(byte[] dane, Schemat schemat)
    {
        JsonNode json;
        try
        {
            var tekst = Encoding.UTF8.GetString(dane).TrimStart('﻿');
            json = JsonNode.Parse(tekst);
        }
        catch (Exception e)
        {
            throw new BladPliku("Plik nie jest poprawnym plikiem JSON: " + e.Message);
        }
        if (json is not JsonObject o || o["format"]?.GetValue<string>() != Format || o["definicje"] is not JsonArray defs)
            throw new BladPliku("To nie jest plik z formularza definicji elementów wynagrodzenia (brak pola format = " + Format + ").");
        var plik = new PlikImportu
        {
            WersjaSchematu = o["wersjaSchematu"]?.GetValue<string>(),
            WersjaEnova = o["wersjaEnova"]?.GetValue<string>(),
            DataEksportu = o["dataEksportu"]?.GetValue<string>(),
            Licencja = o["ustawienia"]?["licencja"]?.GetValue<string>(),
        };
        if (Glowna(plik.WersjaSchematu) != Glowna(schemat.WersjaSchematu))
            throw new BladPliku($"Plik zapisano formularzem w wersji {plik.WersjaSchematu}, a dodatek obsługuje wersję {schemat.WersjaSchematu}. Przekaż klientowi aktualny formularz.");
        foreach (var d in defs)
        {
            if (d is not JsonObject dobj)
                continue;
            plik.Definicje.Add(new DefinicjaZPliku
            {
                Id = dobj["id"]?.GetValue<string>(),
                Szablon = dobj["szablon"]?.GetValue<string>(),
                Uwagi = dobj["uwagi"]?.GetValue<string>() ?? "",
                AkceptacjaOsoba = dobj["akceptacja"]?["osoba"]?.GetValue<string>(),
                AkceptacjaData = dobj["akceptacja"]?["data"]?.GetValue<string>(),
                Wartosci = (dobj["wartosci"] as JsonObject)?.DeepClone().AsObject() ?? new JsonObject(),
                BledyFormularza = (dobj["bledy"] as JsonArray)?.Select(b => b!.GetValue<string>()).ToList() ?? new(),
                Ankieta = UwagiAnkiety(dobj["ankieta"] as JsonObject),
            });
        }
        if (plik.Definicje.Count == 0)
            throw new BladPliku("Plik nie zawiera definicji.");
        return plik;
    }

    static string Glowna(string w) => (w ?? "").Split('.')[0];

    static List<string> UwagiAnkiety(JsonObject a)
    {
        var wynik = new List<string>();
        if (a == null)
            return wynik;
        if (a["zakonczona"]?.GetValue<bool>() == false)
            wynik.Add("Klient nie dokończył ankiety.");
        wynik.AddRange(Teksty(a["doWyjasnienia"]).Select(p => $"Klient nie zna odpowiedzi (do wyjaśnienia): {p}"));
        wynik.AddRange(Teksty(a["doPotwierdzenia"]).Select(p => $"Potwierdź zgodność z przepisami: {p}"));
        wynik.AddRange(Teksty(a["dlaWdrozeniowca"]));
        return wynik;
    }

    static IEnumerable<string> Teksty(JsonNode n) => (n as JsonArray)?.Select(x => x!.GetValue<string>()) ?? Enumerable.Empty<string>();
}

public sealed class BladPliku : Exception
{
    public BladPliku(string komunikat) : base(komunikat) { }
}
