using System.Collections;
using System.Text.Json.Nodes;
using ImportDefinicjiElementow.Silnik;
using Soneta.Business;
using Soneta.Business.Licence;
using Soneta.CRM;
using Soneta.Kadry;
using Soneta.Kalend;
using Soneta.Kasa;
using Soneta.Place;

namespace ImportDefinicjiElementow;

/// <summary>
/// Dostęp do bazy enova dla analizy i zapisu: słowniki (klucz wg pola „poleKlucza” ze schematu),
/// istniejące definicje elementów, licencja programu.
/// </summary>
sealed class BazaEnova : IBazaDefinicji
{
    readonly Session sesja;
    readonly Schemat schemat;
    readonly Dictionary<string, Dictionary<string, Row>> slowniki = new();
    readonly Dictionary<string, DefinicjaElementu> nowePoNazwie = new(StringComparer.CurrentCultureIgnoreCase);

    public BazaEnova(Session sesja, Schemat schemat)
    {
        this.sesja = sesja;
        this.schemat = schemat;
        Konwerter = new KonwerterSoneta(this);
        var lic = sesja.Login.GetLicenceData().Available;
        Licencja = lic[LicencjaProgramu.PL_Platynowy] ? "platynowa" : lic[LicencjaProgramu.PLXL] ? "zlota" : "srebrna";
    }

    public string Licencja { get; }
    public KonwerterSoneta Konwerter { get; }
    public Session Sesja => sesja;
    PlaceModule Place => PlaceModule.GetInstance(sesja);

    IEnumerable<DefinicjaElementu> Definicje() => Place.DefElementow.Cast<DefinicjaElementu>();

    public IIstniejacaDefinicja SzukajPoNazwie(string nazwa)
    {
        var d = Definicje().FirstOrDefault(x => string.Equals(x.Nazwa?.Trim(), nazwa?.Trim(), StringComparison.CurrentCultureIgnoreCase));
        return d == null ? null : new Istniejaca(d, this);
    }

    public IIstniejacaDefinicja SzukajPoSkrocie(string skrot)
    {
        var d = Definicje().FirstOrDefault(x => string.Equals(x.Skrot?.Trim(), skrot?.Trim(), StringComparison.CurrentCultureIgnoreCase));
        return d == null ? null : new Istniejaca(d, this);
    }

    public bool SlownikIstnieje(string slownik, string klucz) => Znajdz(slownik, klucz) != null;

    /// <summary>Wiersz słownika po kluczu (bez wielkości liter); definicje dodane w tym imporcie też są widoczne.</summary>
    public Row Znajdz(string slownik, string klucz)
    {
        if (string.IsNullOrWhiteSpace(klucz))
            return null;
        if (slownik == "DefinicjaElementu" && nowePoNazwie.TryGetValue(klucz.Trim(), out var nowa))
            return nowa;
        if (!slowniki.TryGetValue(slownik, out var mapa))
        {
            mapa = new Dictionary<string, Row>(StringComparer.CurrentCultureIgnoreCase);
            var pole = schemat.Slowniki[slownik]?["poleKlucza"]?.GetValue<string>() ?? "Nazwa";
            foreach (Row r in Wiersze(slownik))
            {
                var k = Klucz(r, pole);
                if (!string.IsNullOrWhiteSpace(k) && !mapa.ContainsKey(k.Trim()))
                    mapa[k.Trim()] = r;
            }
            slowniki[slownik] = mapa;
        }
        return mapa.GetValueOrDefault(klucz.Trim());
    }

    /// <summary>Klucz wiersza słownika (do zapisu w pliku i porównań).</summary>
    public string KluczWiersza(string slownik, Row r)
    {
        if (r == null)
            return null;
        var pole = schemat.Slowniki[slownik]?["poleKlucza"]?.GetValue<string>() ?? "Nazwa";
        return Klucz(r, pole);
    }

    static string Klucz(Row r, string pole)
    {
        if (r is RachunekBankowyPodmiotu rach)
            return new string((rach.Rachunek.Numer.Numer ?? "").Where(char.IsLetterOrDigit).ToArray());
        object v = r;
        foreach (var seg in pole.Split('.'))
            v = v == null ? null : Wlasciwosci.Wlasciwosc(v.GetType(), seg)?.GetValue(v);
        return v?.ToString();
    }

    IEnumerable Wiersze(string slownik)
    {
        var kadry = KadryModule.GetInstance(sesja);
        var kalend = KalendModule.GetInstance(sesja);
        switch (slownik)
        {
            case "PozycjaPIT": return Place.PozycjePIT;
            case "KodRSA": return Place.KodyRSA;
            case "DefinicjaListyPlac": return Place.DefListPlac;
            case "DefinicjaElementu": return Place.DefElementow;
            case "TytulUbezpieczenia4": return kadry.TytulyUbezpiecz4;
            case "Wydzial": return kadry.Wydzialy;
            case "DefPodstawyStazu": return kadry.DefPodstawStazu;
            case "IZakresDefinicji": return kadry.DefFundPozycz;
            case "DefinicjaLimitu": return kalend.DefinicjeLimitow;
            case "DefinicjaStrefy": return kalend.DefinicjeStref;
            case "EwidencjaSP": return KasaModule.GetInstance(sesja).EwidencjeSP;
            case "RachunekBankowyPodmiotu": return KasaModule.GetInstance(sesja).RachBankPodmiot;
            case "IPodmiotKasowy":
            {
                var crm = CRMModule.GetInstance(sesja);
                return crm.Kontrahenci.Cast<Row>().Concat(crm.Banki.Cast<Row>()).Concat(crm.UrzedySkarbowe.Cast<Row>()).Concat(crm.ZUSY.Cast<Row>()).ToList();
            }
            default:
                throw new InvalidOperationException($"Nieobsługiwany słownik {slownik} – dopisz go w BazaEnova.Wiersze.");
        }
    }

    /// <summary>Definicja dodana w trakcie importu - dostępna dla odwołań z innych definicji z tego samego pliku.</summary>
    public void ZarejestrujNowa(DefinicjaElementu d) => nowePoNazwie[d.Nazwa.Trim()] = d;

    /// <summary>Istniejąca definicja jako źródło wartości do porównania z plikiem.</summary>
    internal sealed class Istniejaca : IIstniejacaDefinicja
    {
        public DefinicjaElementu Wiersz { get; }
        readonly BazaEnova baza;

        public Istniejaca(DefinicjaElementu d, BazaEnova baza)
        {
            Wiersz = d;
            this.baza = baza;
        }

        public string Nazwa => Wiersz.Nazwa;
        public string Skrot => Wiersz.Skrot;
        public string Rodzaj => Wiersz.RodzajZrodla.ToString();
        public bool Standardowa => Wiersz.IsStandard;

        public JsonNode Odczytaj(string sciezka)
        {
            var pole = baza.schemat.Pole(sciezka);
            if (pole?["typ"]?.GetValue<string>() == "lista")
                return ZapisDefinicji.OdczytajListe(Wiersz, sciezka, pole, baza.Konwerter);
            try
            {
                return baza.Konwerter.DoPliku(Wlasciwosci.Odczytaj(Wiersz, sciezka), pole);
            }
            catch (Exception)
            {
                return null; // pole niedostępne dla tego rodzaju definicji - różnica zostanie pokazana
            }
        }
    }
}
