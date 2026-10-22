using System.Text;
using System.Text.Json.Nodes;
using ImportDefinicjiElementow.Silnik;
using Xunit;

namespace ImportDefinicjiElementow.Testy;

/// <summary>Reguły schematu - te same przypadki co Formularz/test/warunki.test.mjs (parzystość z formularzem).</summary>
public class TestyWarunkow
{
    static readonly Schemat S = Schemat.Wbudowany();

    static JsonObject Def(string json = "{}")
    {
        var o = JsonNode.Parse(json)!.AsObject();
        o.TryAdd("RodzajZrodla", "Dodatek");
        o.TryAdd("Nazwa", "Test");
        o.TryAdd("Skrot", "T");
        return o;
    }

    static KontekstDefinicji K(JsonObject d, string lic = "zlota") => new(S, d, lic);
    static List<Komunikat> Bledy(JsonObject d, IEnumerable<JsonObject> inne = null, string lic = "zlota") =>
        Walidacja.Waliduj(S, d, inne ?? Array.Empty<JsonObject>(), lic).Where(b => b.Poziom == "blad").ToList();

    [Fact]
    public void Wartosci_domyslne_potwierdzone_w_enova()
    {
        var k = K(Def());
        Assert.Equal("PłatnaZDołu", k.Wartosc("OkresNaliczania.Naliczanie")!.GetValue<string>());
        Assert.Equal("CoNMiesięcy", k.Wartosc("OkresNaliczania.Typ")!.GetValue<string>());
        Assert.True(k.Wartosc("DoWyplaty")!.GetValue<bool>());
        Assert.Equal("Etat", k.Wartosc("Zatrudnienie")!.GetValue<string>());
        Assert.Equal("Umowa", K(Def("""{"RodzajZrodla":"Umowa"}""")).Wartosc("Zatrudnienie")!.GetValue<string>());
        Assert.Equal("DoPełnegoGrosza", k.Wartosc("Zaokraglenie.Precyzja")!.GetValue<string>());
    }

    [Fact]
    public void Sekcje_kreatora_zaleza_od_algorytmu_i_podstawy()
    {
        Assert.True(K(Def()).Stan("Algorytm.KreatorAlgorytmu.Podstawa").Aktywne);
        Assert.False(K(Def("""{"Algorytm.KreatorAlgorytmu.PodstawaTyp":"Wskaźniki"}""")).Stan("Algorytm.KreatorAlgorytmu.Podstawa").Aktywne);
        var jak = K(Def("""{"Algorytm.Typ":"JakUrlopWypoczynkowy"}"""));
        Assert.False(jak.Stan("Algorytm.KreatorAlgorytmu.PodstawaTyp").Aktywne);
        Assert.True(jak.Stan("Algorytm.JakUrlopWypoczynkowy.StdOkresPodstawy").Aktywne);
        Assert.True(jak.Stan("Algorytm.JakUrlopWypoczynkowy.OkresPodstawy").Widoczne);
        Assert.False(jak.Stan("Algorytm.JakUrlopWypoczynkowy.OkresPodstawy").Edytowalne);
    }

    [Fact]
    public void Licencja_steruje_polami_pomniejszen()
    {
        var mies = Def("""{"Algorytm.KreatorAlgorytmu.PodstawaZa":"Miesięcznie"}""");
        Assert.True(K(mies).Stan("Algorytm.KreatorAlgorytmu.Korekta.Chorobowe").Aktywne);
        Assert.False(K(mies).Stan("Algorytm.KreatorAlgorytmu.Korekta.ZUS").Aktywne);
        Assert.True(K(mies, "srebrna").Stan("Algorytm.KreatorAlgorytmu.Korekta.ZUS").Aktywne);
    }

    [Fact]
    public void Dozwolone_algorytmy_wg_rodzaju_i_blad_niedozwolonej_wartosci()
    {
        Assert.Equal(new[] { "KreatorAlgorytmu", "EdytorAlgorytmu" }, K(Def("""{"RodzajZrodla":"Umowa"}""")).Stan("Algorytm.Typ").Dozwolone);
        Assert.Contains(Bledy(Def("""{"RodzajZrodla":"Umowa","Algorytm.Typ":"ZaOkresNieobecności"}""")), b => b.Sciezka == "Algorytm.Typ");
    }

    [Fact]
    public void Nieedytowalne_pole_przyjmuje_wartosc_domyslna_i_nie_jest_zapisywane()
    {
        var d = Def("""{"Deklaracje.Zaliczka.Typ":"Procentowe"}""");
        Assert.Equal("NieNaliczać", K(d).Wartosc("Deklaracje.Zaliczka.Typ")!.GetValue<string>());
        Assert.DoesNotContain(Walidacja.DoZapisu(S, d, "zlota"), p => p.Sciezka == "Deklaracje.Zaliczka.Procent");
        var zle = Def("""{"Deklaracje.PozycjaPIT":"PIT-11 1/PIT-4R 1","Deklaracje.Zaliczka.Typ":"Procentowe","Deklaracje.Zaliczka.Procent":150}""");
        Assert.Contains(Bledy(zle), b => b.Sciezka == "Deklaracje.Zaliczka.Procent");
    }

    [Fact]
    public void Walidacja_wymagane_dlugosc_unikalnosc_nazwy_standardowe()
    {
        Assert.Contains(Bledy(JsonNode.Parse("""{"RodzajZrodla":"Dodatek"}""")!.AsObject()), b => b.Sciezka == "Nazwa");
        Assert.Contains(Bledy(Def("""{"Skrot":"1234567890123"}""")), b => b.Sciezka == "Skrot");
        var a = Def("""{"Nazwa":"Premia X","Skrot":"PX"}""");
        var b2 = Def("""{"Nazwa":"premia x ","Skrot":"PX"}""");
        var w = Walidacja.Waliduj(S, b2, new[] { a }, "zlota");
        Assert.Contains(w, x => x.Sciezka == "Nazwa" && x.Poziom == "blad");
        Assert.Contains(w, x => x.Sciezka == "Skrot" && x.Poziom == "ostrzezenie");
        Assert.Contains(Bledy(Def("""{"Nazwa":"Premia procentowa"}""")), x => x.Tekst.Contains("standardowa"));
    }

    [Fact]
    public void Nazwy_parametrow_tylko_dla_parametrow_algorytmu()
    {
        var procent = Def("""{"Algorytm.KreatorAlgorytmu.Wspolczynnik.Typ":"Procent"}""");
        Assert.True(K(procent).Stan("Algorytm.DodProcent").Aktywne);
        Assert.False(K(procent).Stan("Algorytm.DodUlamek").Aktywne);
        var edytor = Def("""{"Algorytm.Typ":"EdytorAlgorytmu","Algorytm.DodProcent":"Procent"}""");
        Assert.False(K(edytor).Stan("Algorytm.DodUlamek").Aktywne);
        Assert.True(K(edytor).Stan("Algorytm.ElPodstawa5").Aktywne);
    }

    [Fact]
    public void Szablony_przechodza_walidacje_po_nadaniu_nazwy()
    {
        foreach (var s in S.Json["szablony"]!.AsArray())
        {
            var w = s!["wartosci"]!.DeepClone().AsObject();
            w["Nazwa"] = "Szablon " + s["id"];
            w["Skrot"] = s["id"]!.GetValue<string>()[..Math.Min(12, s["id"]!.GetValue<string>().Length)];
            var b = Bledy(w).Where(x => !(s["id"]!.GetValue<string>() == "zasadnicze" && x.Sciezka.StartsWith("Algorytm.KreatorAlgorytmu"))).ToList();
            Assert.True(b.Count == 0, $"szablon {s["id"]}: " + string.Join("; ", b.Select(x => x.Sciezka + " " + x.Tekst)));
        }
    }
}

/// <summary>Plik z formularza i analiza importu na atrapie bazy.</summary>
public class TestyAnalizy
{
    static readonly Schemat S = Schemat.Wbudowany();
    static byte[] Przyklad() => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "dane", "przyklad-z-formularza.json"));

    sealed class Istniejaca : IIstniejacaDefinicja
    {
        public string Nazwa { get; init; }
        public string Skrot { get; init; }
        public string Rodzaj { get; init; } = "Dodatek";
        public bool Standardowa { get; init; }
        public Dictionary<string, JsonNode> Pola { get; init; } = new();
        public JsonNode Odczytaj(string sciezka) => Pola.TryGetValue(sciezka, out var v) ? v?.DeepClone() : null;
    }

    sealed class Baza : IBazaDefinicji
    {
        public string Licencja { get; init; } = "zlota";
        public List<Istniejaca> Definicje = new();
        public HashSet<string> BrakujaceSlowniki = new();
        public IIstniejacaDefinicja SzukajPoNazwie(string n) => Definicje.FirstOrDefault(d => string.Equals(d.Nazwa, n, StringComparison.CurrentCultureIgnoreCase));
        public IIstniejacaDefinicja SzukajPoSkrocie(string s) => Definicje.FirstOrDefault(d => string.Equals(d.Skrot, s, StringComparison.CurrentCultureIgnoreCase));
        public bool SlownikIstnieje(string sl, string k) => !BrakujaceSlowniki.Contains(sl + ":" + k);
    }

    [Fact]
    public void Plik_z_formularza_wczytany_i_bez_bledow()
    {
        var plik = PlikImportu.Wczytaj(Przyklad(), S);
        Assert.Equal(2, plik.Definicje.Count);
        var a = Analizator.Analizuj(S, plik, new Baza(), TrybImportu.NoweIAktualizacja);
        Assert.All(a.Definicje, d => Assert.Equal(StatusDefinicji.Nowa, d.Status));
        Assert.Equal("Procent ustala kierownik co miesiąc.", a.Definicje[1].Uwagi);
        Assert.Contains(a.Definicje[0].Przypisania, p => p.Sciezka == "Algorytm.KreatorAlgorytmu.Wspolczynnik.Typ" && p.Wartosc!.GetValue<string>() == "Procent");
        Assert.Equal("RodzajZrodla", a.Definicje[0].Przypisania[0].Sciezka);
    }

    [Fact]
    public void Plik_obcy_lub_inna_wersja_odrzucony()
    {
        Assert.Throws<BladPliku>(() => PlikImportu.Wczytaj(Encoding.UTF8.GetBytes("nie json"), S));
        Assert.Throws<BladPliku>(() => PlikImportu.Wczytaj(Encoding.UTF8.GetBytes("""{"format":"inny","definicje":[]}"""), S));
        var inna = JsonNode.Parse(Przyklad())!.AsObject();
        inna["wersjaSchematu"] = "2.0";
        Assert.Throws<BladPliku>(() => PlikImportu.Wczytaj(Encoding.UTF8.GetBytes(inna.ToJsonString()), S));
    }

    [Fact]
    public void Aktualizacja_pokazuje_roznice_a_bez_zmian_nie_zapisuje()
    {
        var plik = PlikImportu.Wczytaj(Przyklad(), S);
        var d0 = plik.Definicje[0];
        var pola = Walidacja.DoZapisu(S, d0.Wartosci, "zlota").ToDictionary(p => p.Sciezka, p => p.Wartosc);
        var identyczna = new Istniejaca { Nazwa = d0.Wartosci["Nazwa"]!.GetValue<string>(), Skrot = "inny", Pola = pola };
        var a = Analizator.Analizuj(S, plik, new Baza { Definicje = { identyczna } }, TrybImportu.NoweIAktualizacja);
        Assert.Equal(StatusDefinicji.BezZmian, a.Definicje[0].Status);

        var zmieniona = new Istniejaca { Nazwa = identyczna.Nazwa, Skrot = "inny", Pola = new(pola) };
        zmieniona.Pola["Algorytm.Priorytet"] = 5;
        zmieniona.Pola["Algorytm.KreatorAlgorytmu.Wspolczynnik.Procent"] = 7.5m;
        a = Analizator.Analizuj(S, plik, new Baza { Definicje = { zmieniona } }, TrybImportu.NoweIAktualizacja);
        Assert.Equal(StatusDefinicji.Aktualizacja, a.Definicje[0].Status);
        Assert.Equal(2, a.Definicje[0].Roznice.Count);
        Assert.Contains(a.Definicje[0].Roznice, r => r.Sciezka == "Algorytm.Priorytet" && r.Obecna == "5");

        a = Analizator.Analizuj(S, plik, new Baza { Definicje = { zmieniona } }, TrybImportu.TylkoNowe);
        Assert.Equal(StatusDefinicji.Pominieta, a.Definicje[0].Status);
    }

    [Fact]
    public void Definicja_standardowa_nie_jest_nadpisywana_a_zmiana_rodzaju_to_blad()
    {
        var plik = PlikImportu.Wczytaj(Przyklad(), S);
        var nazwa = plik.Definicje[0].Wartosci["Nazwa"]!.GetValue<string>();
        var a = Analizator.Analizuj(S, plik, new Baza { Definicje = { new Istniejaca { Nazwa = nazwa, Standardowa = true } } }, TrybImportu.NoweIAktualizacja);
        Assert.Equal(StatusDefinicji.Pominieta, a.Definicje[0].Status);
        Assert.Contains(a.Definicje[0].Komunikaty, k => k.Tekst.Contains("standardowa"));
        a = Analizator.Analizuj(S, plik, new Baza { Definicje = { new Istniejaca { Nazwa = nazwa, Rodzaj = "Etat" } } }, TrybImportu.NoweIAktualizacja);
        Assert.Equal(StatusDefinicji.Blad, a.Definicje[0].Status);
    }

    [Fact]
    public void Brak_wartosci_slownika_w_bazie_to_blad()
    {
        var plik = PlikImportu.Wczytaj(Przyklad(), S);
        var baza = new Baza { BrakujaceSlowniki = { "PozycjaPIT:PIT-11 1/PIT-4R 1" } };
        var a = Analizator.Analizuj(S, plik, baza, TrybImportu.NoweIAktualizacja);
        Assert.All(a.Definicje, d => Assert.Equal(StatusDefinicji.Blad, d.Status));
        Assert.Contains(a.Definicje[0].Komunikaty, k => k.Sciezka == "Deklaracje.PozycjaPIT");
        var podglad = Raport.Podglad(a, S);
        Assert.Contains("Nie ma nic do zapisania", podglad);
    }

    [Fact]
    public void Odwolanie_do_definicji_z_tego_samego_pliku_jest_poprawne()
    {
        var plik = PlikImportu.Wczytaj(Przyklad(), S);
        var bazowa = plik.Definicje[0].Wartosci["Nazwa"]!.GetValue<string>();
        var d1 = plik.Definicje[1].Wartosci;
        d1["Algorytm.Typ"] = "ZaOkresNieobecności";
        d1["Algorytm.ZaNieobecnosc.Podstawowa"] = bazowa;
        var baza = new Baza { BrakujaceSlowniki = { "DefinicjaElementu:" + bazowa } };
        var a = Analizator.Analizuj(S, plik, baza, TrybImportu.NoweIAktualizacja);
        Assert.Equal(StatusDefinicji.Nowa, a.Definicje[1].Status);
    }

    [Fact]
    public void Raport_html_zawiera_wynik_i_uwagi()
    {
        var plik = PlikImportu.Wczytaj(Przyklad(), S);
        var a = Analizator.Analizuj(S, plik, new Baza(), TrybImportu.NoweIAktualizacja);
        var z = new WynikZapisu { Zapisano = true, Definicje = a.Definicje.Select(d => new WynikZapisuDefinicji(d, true, null, 10)).ToList() };
        var html = Raport.Html(a, z, S);
        Assert.Contains("Zapisano 2 definicji", html);
        Assert.Contains("Procent ustala kierownik co miesiąc.", html);
    }
}

/// <summary>Mapowanie wartości z pliku na właściwości obiektów i typy Soneta.</summary>
public class TestyMapowania
{
    // atrapa struktury podobnej do DefinicjaElementu: podobiekty i właściwość ukryta przez „new”
    public class Baza1 { public virtual int Priorytet { get; set; } }
    public class Algorytm : Baza1 { public new int Priorytet { get; set; } public Kreator KreatorAlgorytmu { get; } = new(); }
    public class Kreator { public Soneta.Types.Percent Procent { get; set; } public DayOfWeek Dzien { get; set; } public string TylkoOdczyt => "x"; }
    public class Definicja { public Algorytm Algorytm { get; } = new(); }

    sealed class Konwerter : IKonwerterWartosci
    {
        public object DoObiektu(JsonNode v, Type cel, JsonObject pole) => KonwerterSoneta.Wartosc(v, cel);
        public JsonNode DoPliku(object o, JsonObject pole) => KonwerterSoneta.Prosta(o);
    }

    [Fact]
    public void Ustawianie_po_sciezce_przez_podobiekty_i_nadpisane_wlasciwosci()
    {
        var d = new Definicja();
        var k = new Konwerter();
        Assert.True(Wlasciwosci.Ustaw(d, "Algorytm.Priorytet", 7, null, k));
        Assert.Equal(7, d.Algorytm.Priorytet);
        Assert.False(Wlasciwosci.Ustaw(d, "Algorytm.Priorytet", 7, null, k), "ta sama wartość nie wywołuje settera");
        Assert.True(Wlasciwosci.Ustaw(d, "Algorytm.KreatorAlgorytmu.Procent", 12.5m, null, k));
        Assert.Equal(0.125m, (decimal)d.Algorytm.KreatorAlgorytmu.Procent);
        Assert.Equal(12.5m, KonwerterSoneta.Prosta(d.Algorytm.KreatorAlgorytmu.Procent)!.GetValue<decimal>());
        Wlasciwosci.Ustaw(d, "Algorytm.KreatorAlgorytmu.Dzien", "Friday", null, k);
        Assert.Equal(DayOfWeek.Friday, d.Algorytm.KreatorAlgorytmu.Dzien);
        Assert.Throws<InvalidOperationException>(() => Wlasciwosci.Ustaw(d, "Algorytm.KreatorAlgorytmu.TylkoOdczyt", "y", null, k));
        Assert.Throws<InvalidOperationException>(() => Wlasciwosci.Ustaw(d, "Algorytm.Brak", 1, null, k));
    }

    [Fact]
    public void Konwersje_typow_soneta()
    {
        Assert.Equal(new Soneta.Types.Fraction(1, 2), KonwerterSoneta.Wartosc("1/2", typeof(Soneta.Types.Fraction)));
        Assert.Equal(new Soneta.Types.Time(8, 30), KonwerterSoneta.Wartosc("8:30", typeof(Soneta.Types.Time)));
        Assert.Equal(new Soneta.Types.Currency(150.5m), KonwerterSoneta.Wartosc(150.5m, typeof(Soneta.Types.Currency)));
        Assert.Equal(new Soneta.Types.Percent(0.05m), KonwerterSoneta.Wartosc(5, typeof(Soneta.Types.Percent)));
        Assert.Equal(Soneta.Place.TypZaliczkiPodatku.NaliczaćWgSkaliPodatkowej, KonwerterSoneta.Wartosc("NaliczaćWgSkaliPodatkowej", typeof(Soneta.Place.TypZaliczkiPodatku)));
        Assert.Equal(3, KonwerterSoneta.Wartosc(3, typeof(int)));
        Assert.Equal("8:30", KonwerterSoneta.Prosta(new Soneta.Types.Time(8, 30))!.GetValue<string>());
    }

    [Fact]
    public void Wszystkie_pola_schematu_maja_typ_obslugiwany_przez_konwerter()
    {
        var obslugiwane = new HashSet<string> { "bool", "int", "decimal", "string", "percent", "currency", "fraction", "time", "enum", "slownik", "lista" };
        var s = Schemat.Wbudowany();
        var nie = s.KolejnoscPol.Where(p => !obslugiwane.Contains(s.Typ(p))).ToList();
        Assert.True(nie.Count == 0, "nieobsługiwane typy: " + string.Join(", ", nie.Select(p => p + ":" + s.Typ(p))));
    }
}
