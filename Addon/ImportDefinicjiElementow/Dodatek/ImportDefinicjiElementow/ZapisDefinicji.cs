using System.Text.Json.Nodes;
using ImportDefinicjiElementow.Silnik;
using Soneta.Business;
using Soneta.Business.App;
using Soneta.Place;

namespace ImportDefinicjiElementow;

/// <summary>
/// Zapis przeanalizowanych definicji w jednej transakcji: najpierw zakładane są nowe definicje
/// (żeby odwołania między definicjami z pliku dało się rozwiązać), potem ustawiane pola w kolejności schematu.
/// Błąd dowolnego pola albo weryfikatora enova przy Save() = nic nie jest zapisywane.
/// </summary>
static class ZapisDefinicji
{
    const string Progi = "Algorytm.KreatorAlgorytmu.Wspolczynnik.Progi";
    const string PodstawyDefinicji = "PodstawyDefinicji";

    public static WynikZapisu Zapisz(Login login, Schemat schemat, WynikAnalizy analiza)
    {
        var wynik = new WynikZapisu { Operator = login.Operator?.Name, Baza = login.Database?.Name };
        var doZapisu = analiza.DoZapisu.ToList();
        if (doZapisu.Count == 0)
        {
            wynik.BladOgolny = "Brak definicji do zapisania.";
            return wynik;
        }
        using var sesja = login.CreateSession(false, true, "Import definicji elementów wynagrodzenia");
        var baza = new BazaEnova(sesja, schemat);
        var place = PlaceModule.GetInstance(sesja);
        var wiersze = new Dictionary<AnalizaDefinicji, DefinicjaElementu>();
        AnalizaDefinicji biezaca = null;
        string biezacePole = null;
        try
        {
            using (var t = sesja.Logout(true))
            {
                foreach (var a in doZapisu)
                {
                    biezaca = a;
                    biezacePole = "Nazwa";
                    DefinicjaElementu d;
                    if (a.Status == StatusDefinicji.Nowa)
                    {
                        d = new DefinicjaElementu(Enum.Parse<RodzajŹródłaWypłaty>(a.Rodzaj));
                        place.DefElementow.AddRow(d);
                        d.Nazwa = a.Nazwa;
                        d.Skrot = a.Skrot;
                        baza.ZarejestrujNowa(d);
                    }
                    else
                    {
                        d = sesja.Get(((BazaEnova.Istniejaca)a.Istniejaca).Wiersz);
                    }
                    wiersze[a] = d;
                }
                var zmienione = new Dictionary<AnalizaDefinicji, int>();
                foreach (var a in doZapisu)
                {
                    biezaca = a;
                    var d = wiersze[a];
                    var n = 0;
                    foreach (var (sciezka, wartosc) in a.Przypisania)
                    {
                        if (sciezka is "RodzajZrodla")
                            continue;
                        biezacePole = sciezka;
                        var pole = schemat.Pole(sciezka);
                        if (pole["typ"]?.GetValue<string>() == "lista")
                            n += UstawListe(place, d, sciezka, pole, wartosc.AsArray(), baza) ? 1 : 0;
                        else if (Wlasciwosci.Ustaw(d, sciezka, wartosc, pole, baza.Konwerter))
                            n++;
                    }
                    zmienione[a] = n;
                }
                biezaca = null;
                t.Commit();
                foreach (var a in doZapisu)
                    wynik.Definicje.Add(new WynikZapisuDefinicji(a, true, null, zmienione[a]));
            }
            sesja.Save();
            wynik.Zapisano = true;
        }
        catch (Exception e)
        {
            var komunikat = Komunikat(e);
            wynik.Zapisano = false;
            wynik.Definicje.Clear();
            if (biezaca != null)
            {
                var gdzie = $"„{biezaca.Nazwa}”, pole „{schemat.Etykieta(biezacePole)}”";
                wynik.BladOgolny = $"Błąd przy definicji {gdzie}: {komunikat}. Żadna definicja nie została zapisana.";
                wynik.Definicje.Add(new WynikZapisuDefinicji(biezaca, false, $"pole „{schemat.Etykieta(biezacePole)}”: {komunikat}", 0));
            }
            else
            {
                wynik.BladOgolny = $"enova nie zapisała zmian: {komunikat}. Żadna definicja nie została zapisana.";
            }
        }
        return wynik;
    }

    static string Komunikat(Exception e)
    {
        while (e is System.Reflection.TargetInvocationException && e.InnerException != null)
            e = e.InnerException;
        return e.Message;
    }

    /// <summary>Podlista (progi stażu, suma elementów) - zastępowana w całości, gdy różni się od pliku.</summary>
    static bool UstawListe(PlaceModule place, DefinicjaElementu d, string sciezka, JsonObject pole, JsonArray wiersze, BazaEnova baza)
    {
        if (Analizator.RowneWartosci(OdczytajListe(d, sciezka, pole, baza.Konwerter), wiersze))
            return false;
        var kolumny = pole["kolumny"]!.AsArray();
        if (sciezka == Progi)
        {
            foreach (var p in d.Algorytm.KreatorAlgorytmu.Wspolczynnik.Progi.Cast<ProgStazuPracy>().ToList())
                p.Delete();
            foreach (var w in wiersze)
            {
                var p = new ProgStazuPracy(d);
                place.ProgiStazuPracy.AddRow(p);
                UstawKolumny(p, w!.AsObject(), kolumny, baza);
            }
            return true;
        }
        if (sciezka == PodstawyDefinicji)
        {
            foreach (var p in d.PodstawyDefinicji.ToList())
                p.Delete();
            foreach (var w in wiersze)
            {
                var p = new PodstawaDefinicjiElementu(d);
                place.PodstDefElem.AddRow(p);
                UstawKolumny(p, w!.AsObject(), kolumny, baza);
            }
            return true;
        }
        throw new InvalidOperationException($"Nieobsługiwana lista {sciezka}.");
    }

    static void UstawKolumny(Row wiersz, JsonObject w, JsonArray kolumny, BazaEnova baza)
    {
        foreach (var kol in kolumny)
        {
            var nazwa = kol!["sciezka"]!.GetValue<string>();
            Wlasciwosci.Ustaw(wiersz, nazwa, w[nazwa], kol.AsObject(), baza.Konwerter);
        }
    }

    /// <summary>Odczyt podlisty w postaci jak w pliku (do porównań i raportu różnic).</summary>
    public static JsonNode OdczytajListe(DefinicjaElementu d, string sciezka, JsonObject pole, KonwerterSoneta konwerter)
    {
        IEnumerable<Row> wiersze = sciezka switch
        {
            Progi => d.Algorytm.KreatorAlgorytmu.Wspolczynnik.Progi.Cast<ProgStazuPracy>().OrderBy(p => p.Prog),
            PodstawyDefinicji => d.PodstawyDefinicji,
            _ => Enumerable.Empty<Row>(),
        };
        var wynik = new JsonArray();
        foreach (var r in wiersze)
        {
            var o = new JsonObject();
            foreach (var kol in pole["kolumny"]!.AsArray())
            {
                var nazwa = kol!["sciezka"]!.GetValue<string>();
                o[nazwa] = konwerter.DoPliku(Wlasciwosci.Odczytaj(r, nazwa), kol.AsObject());
            }
            wynik.Add(o);
        }
        return wynik;
    }
}
