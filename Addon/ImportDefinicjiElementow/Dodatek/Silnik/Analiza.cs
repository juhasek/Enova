using System.Text.Json.Nodes;

namespace ImportDefinicjiElementow.Silnik;

public enum TrybImportu
{
    /// <summary>Tylko nowe definicje; istniejące (ta sama nazwa lub skrót) są pomijane.</summary>
    TylkoNowe,
    /// <summary>Nowe definicje i aktualizacja istniejących (poza standardowymi).</summary>
    NoweIAktualizacja,
}

public enum StatusDefinicji { Nowa, Aktualizacja, BezZmian, Pominieta, Blad }

/// <summary>Istniejąca w bazie definicja elementu (widok na potrzeby analizy).</summary>
public interface IIstniejacaDefinicja
{
    string Nazwa { get; }
    string Skrot { get; }
    string Rodzaj { get; }
    bool Standardowa { get; }
    /// <summary>Aktualna wartość pola w postaci jak w pliku (enum = nazwa, liczby, bool, słownik = klucz, lista = tablica).</summary>
    JsonNode Odczytaj(string sciezka);
}

/// <summary>Dostęp do bazy enova potrzebny analizie (w dodatku - sesja enova, w testach - atrapa).</summary>
public interface IBazaDefinicji
{
    /// <summary>Licencja programu: srebrna / zlota / platynowa.</summary>
    string Licencja { get; }
    IIstniejacaDefinicja SzukajPoNazwie(string nazwa);
    IIstniejacaDefinicja SzukajPoSkrocie(string skrot);
    /// <summary>Czy w bazie jest wartość słownika (slownik = krótka nazwa typu ze schematu, np. PozycjaPIT).</summary>
    bool SlownikIstnieje(string slownik, string klucz);
}

public sealed record Roznica(string Sciezka, string Etykieta, string Obecna, string Nowa);

public sealed class AnalizaDefinicji
{
    public int Numer;
    public string Id;
    public string Nazwa;
    public string Skrot;
    public string Rodzaj;
    public string Uwagi;
    /// <summary>„osoba, data” akceptacji klienta albo null.</summary>
    public string Akceptacja;
    public StatusDefinicji Status;
    public IIstniejacaDefinicja Istniejaca;
    public List<(string Sciezka, JsonNode Wartosc)> Przypisania = new();
    public List<Roznica> Roznice = new();
    public List<Komunikat> Komunikaty = new();
    public bool MaBledy => Komunikaty.Any(k => k.Poziom == "blad");
}

public sealed class WynikAnalizy
{
    public PlikImportu Plik;
    public TrybImportu Tryb;
    public string Licencja;
    public List<string> Ostrzezenia = new();
    public List<AnalizaDefinicji> Definicje = new();
    public IEnumerable<AnalizaDefinicji> DoZapisu => Definicje.Where(d => d.Status is StatusDefinicji.Nowa or StatusDefinicji.Aktualizacja);
    public int Liczba(StatusDefinicji s) => Definicje.Count(d => d.Status == s);
}

/// <summary>
/// Analiza pliku przed zapisem: walidacja wg schematu (te same reguły co formularz), istnienie wartości słowników
/// w bazie, dopasowanie do istniejących definicji (po nazwie lub skrócie), ochrona definicji standardowych, różnice pól.
/// </summary>
public static class Analizator
{
    public static WynikAnalizy Analizuj(Schemat schemat, PlikImportu plik, IBazaDefinicji baza, TrybImportu tryb)
    {
        var wynik = new WynikAnalizy { Plik = plik, Tryb = tryb, Licencja = baza.Licencja };
        if (!string.IsNullOrEmpty(plik.Licencja) && plik.Licencja != baza.Licencja)
            wynik.Ostrzezenia.Add($"Plik wypełniono dla licencji „{plik.Licencja}”, a program ma licencję „{baza.Licencja}” – reguły pól liczone są wg licencji programu.");
        if (!string.IsNullOrEmpty(plik.WersjaEnova) && !string.IsNullOrEmpty(schemat.WersjaEnova) && plik.WersjaEnova != schemat.WersjaEnova)
            wynik.Ostrzezenia.Add($"Formularz zbudowano dla enova {plik.WersjaEnova}, dodatek ma schemat enova {schemat.WersjaEnova}.");

        var wszystkie = plik.Definicje.Select(d => d.Wartosci).ToList();
        var nazwyWPliku = new HashSet<string>(plik.Definicje.Select(d => Tekst(d.Wartosci["Nazwa"])).Where(n => n != ""), StringComparer.CurrentCultureIgnoreCase);
        var numer = 0;
        foreach (var d in plik.Definicje)
        {
            var a = new AnalizaDefinicji
            {
                Numer = ++numer,
                Id = d.Id,
                Uwagi = d.Uwagi,
                Nazwa = Tekst(d.Wartosci["Nazwa"]),
                Skrot = Tekst(d.Wartosci["Skrot"]),
                Rodzaj = Tekst(d.Wartosci["RodzajZrodla"]),
                Akceptacja = string.IsNullOrWhiteSpace(d.AkceptacjaOsoba) ? null : $"{d.AkceptacjaOsoba.Trim()}, {Raport.Data(d.AkceptacjaData)}",
            };
            wynik.Definicje.Add(a);
            if (a.Akceptacja == null)
                a.Komunikaty.Add(new("Akceptacja", "ostrzezenie", "Klient nie zaakceptował tej definicji w formularzu."));
            a.Komunikaty.AddRange(Walidacja.Waliduj(schemat, d.Wartosci, wszystkie.Where(w => !ReferenceEquals(w, d.Wartosci)), baza.Licencja)
                .Where(k => k.Poziom != "informacja"));
            a.Przypisania = Walidacja.DoZapisu(schemat, d.Wartosci, baza.Licencja);
            SprawdzSlowniki(schemat, a, baza, nazwyWPliku);

            var poNazwie = a.Nazwa == "" ? null : baza.SzukajPoNazwie(a.Nazwa);
            var poSkrocie = a.Skrot == "" ? null : baza.SzukajPoSkrocie(a.Skrot);
            if (poNazwie != null && poSkrocie != null && !ReferenceEquals(poNazwie, poSkrocie) && poNazwie.Nazwa != poSkrocie.Nazwa)
                a.Komunikaty.Add(new("Skrot", "blad", $"Nazwa wskazuje definicję „{poNazwie.Nazwa}”, a skrót – inną („{poSkrocie.Nazwa}”). Zmień skrót."));
            a.Istniejaca = poNazwie ?? poSkrocie;

            if (a.MaBledy)
            {
                a.Status = StatusDefinicji.Blad;
                continue;
            }
            if (a.Istniejaca == null)
            {
                a.Status = StatusDefinicji.Nowa;
                continue;
            }
            if (a.Istniejaca.Standardowa)
            {
                a.Status = StatusDefinicji.Pominieta;
                a.Komunikaty.Add(new("Nazwa", "ostrzezenie", $"„{a.Istniejaca.Nazwa}” to definicja standardowa enova – import jej nie nadpisuje."));
                continue;
            }
            if (tryb == TrybImportu.TylkoNowe)
            {
                a.Status = StatusDefinicji.Pominieta;
                a.Komunikaty.Add(new("Nazwa", "ostrzezenie", $"Definicja „{a.Istniejaca.Nazwa}” już istnieje – w trybie „tylko nowe” jest pomijana."));
                continue;
            }
            if (a.Istniejaca.Rodzaj != a.Rodzaj)
            {
                a.Status = StatusDefinicji.Blad;
                a.Komunikaty.Add(new("RodzajZrodla", "blad", $"Istniejąca definicja „{a.Istniejaca.Nazwa}” ma rodzaj {schemat.EtykietaWartosci("RodzajZrodla", JsonValue.Create(a.Istniejaca.Rodzaj))} – rodzaju nie można zmienić."));
                continue;
            }
            foreach (var (s, v) in a.Przypisania)
            {
                if (s == "RodzajZrodla")
                    continue;
                var obecna = a.Istniejaca.Odczytaj(s);
                if (!RowneWartosci(obecna, v))
                    a.Roznice.Add(new(s, schemat.Etykieta(s), Pokaz(schemat, s, obecna), Pokaz(schemat, s, v)));
            }
            a.Status = a.Roznice.Count == 0 ? StatusDefinicji.BezZmian : StatusDefinicji.Aktualizacja;
        }
        return wynik;
    }

    static void SprawdzSlowniki(Schemat schemat, AnalizaDefinicji a, IBazaDefinicji baza, HashSet<string> nazwyWPliku)
    {
        bool Istnieje(string slownik, string klucz) =>
            baza.SlownikIstnieje(slownik, klucz) || (slownik == "DefinicjaElementu" && nazwyWPliku.Contains(klucz) && !string.Equals(klucz, a.Nazwa, StringComparison.CurrentCultureIgnoreCase));
        foreach (var (s, v) in a.Przypisania)
        {
            var pole = schemat.Pole(s);
            var sl = pole["slownik"]?.GetValue<string>();
            if (sl != null && !Istnieje(sl, Tekst(v)))
                a.Komunikaty.Add(new(s, "blad", $"W bazie enova nie ma wartości „{Tekst(v)}” ({schemat.Slowniki[sl]?["etykieta"]?.GetValue<string>() ?? sl})."));
            if (pole["kolumny"] is JsonArray kolumny && v is JsonArray wiersze)
                foreach (var kol in kolumny)
                {
                    var ksl = kol!["slownik"]?.GetValue<string>();
                    if (ksl == null)
                        continue;
                    foreach (var w in wiersze)
                    {
                        var kv = Tekst(w?[kol["sciezka"]!.GetValue<string>()]);
                        if (kv != "" && !Istnieje(ksl, kv))
                            a.Komunikaty.Add(new(s, "blad", $"W bazie enova nie ma wartości „{kv}” ({kol["etykieta"]})."));
                    }
                }
        }
    }

    public static string Tekst(JsonNode v) => v is JsonValue jv && jv.TryGetValue<string>(out var s) ? s.Trim() : v?.ToJsonString() ?? "";

    /// <summary>Porównanie wartości z bazy i z pliku (liczby po wartości, teksty bez wielkości liter i spacji na końcach).</summary>
    public static bool RowneWartosci(JsonNode a, JsonNode b)
    {
        if (Warunki.Pusty(a) && Warunki.Pusty(b))
            return true;
        if (a is JsonArray || b is JsonArray)
            return a is JsonArray x && b is JsonArray y && x.Count == y.Count && x.Zip(y).All(p => RowneWiersze(p.First, p.Second));
        if (Warunki.Liczba(a, out var la) && Warunki.Liczba(b, out var lb))
            return la == lb;
        return string.Equals(Tekst(a), Tekst(b), StringComparison.CurrentCultureIgnoreCase);
    }

    static bool RowneWiersze(JsonNode a, JsonNode b)
    {
        if (a is not JsonObject x || b is not JsonObject y)
            return false;
        return x.Select(kv => kv.Key).Union(y.Select(kv => kv.Key)).All(k => RowneWartosci(x[k], y[k]));
    }

    public static string Pokaz(Schemat s, string sciezka, JsonNode v)
    {
        if (Warunki.Pusty(v))
            return "(puste)";
        if (v is JsonArray arr)
            return string.Join("; ", arr.Select(w => string.Join(", ", w!.AsObject().Select(kv => Tekst(kv.Value)))));
        return s.EtykietaWartosci(sciezka, v);
    }
}
