using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Enova.KadryPlace.DefinicjeElementow
{
    // Generator plikow session/DefinicjaElementu (business.xml) do importu definicji
    // elementow kadrowo-placowych w enova365. Struktura i wartosci domyslne modelu
    // odpowiadaja wzorcowi "Dodatek funkcyjny" - patrz DefElementow_wzorzec.xml oraz
    // opis pol w DefinicjaElementu_pola.xlsx / DefinicjaElementu.md w tym samym folderze.
    //
    // Uzycie:
    //   var el = new DefinicjaElementuDane { Nazwa = "Dodatek stazowy", Skrot = "Dod.staz.", Kolejnosc = 60 };
    //   DefinicjaElementuXmlBuilder.Zapisz(new[] { el }, @"C:\import\DefElementow.xml");

    public static class DefinicjaElementuXmlBuilder
    {
        public static readonly XNamespace Ns = "http://www.soneta.pl/schema/business";

        public static XDocument BuildSession(IEnumerable<DefinicjaElementuDane> elementy)
        {
            var root = new XElement(Ns + "session");
            int kolejny = 1;
            foreach (var el in elementy)
            {
                if (string.IsNullOrEmpty(el.Id))
                    el.Id = "DefinicjaElementu_" + kolejny;
                root.Add(el.ToXml());
                kolejny++;
            }

            var doc = new XDocument(new XDeclaration("1.0", "Unicode", null));
            doc.Add(root);
            return doc;
        }

        // Enova zapisuje/importuje pliki session XML w UTF-16 (encoding="Unicode"
        // w naglowku wzorca) - zachowujemy to samo kodowanie przy zapisie na dysk.
        public static void Zapisz(IEnumerable<DefinicjaElementuDane> elementy, string sciezkaPliku)
        {
            var doc = BuildSession(elementy);
            using (var writer = new StreamWriter(sciezkaPliku, false, Encoding.Unicode))
                doc.Save(writer);
        }
    }

    internal static class Xml
    {
        public static XElement El(XNamespace ns, string name, string value)
        {
            return string.IsNullOrEmpty(value) ? new XElement(ns + name) : new XElement(ns + name, value);
        }

        public static XElement ElBool(XNamespace ns, string name, bool value)
        {
            return new XElement(ns + name, value ? "True" : "False");
        }

        public static XElement ElInt(XNamespace ns, string name, int value)
        {
            return new XElement(ns + name, value.ToString(CultureInfo.InvariantCulture));
        }

        public static XElement ElKwota(XNamespace ns, string name, decimal value)
        {
            return new XElement(ns + name, value.ToString("0.00", CultureInfo.InvariantCulture) + " PLN");
        }

        public static XElement ElProcent(XNamespace ns, string name, decimal value)
        {
            return new XElement(ns + name, value.ToString("0.00", CultureInfo.InvariantCulture) + "%");
        }
    }

    // ---------------------------------------------------------------------
    // 1. Korzen: DefinicjaElementu
    // ---------------------------------------------------------------------

    public sealed class DefinicjaElementuDane
    {
        public string Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();

        // 1. Dane podstawowe
        public string Podstawowa { get; set; }
        public string Wydzial { get; set; }
        public string Nastepny { get; set; }
        public string DoDnia { get; set; } = "(max)";
        public string RodzajZrodla { get; set; } = "Dodatek"; // Dodatek | Potracenie
        public string Nazwa { get; set; }
        public string NazwaPomocnicza { get; set; }
        public string Skrot { get; set; }
        public string Kod { get; set; }
        public bool Blokada { get; set; }
        public int Kolejnosc { get; set; }
        public string Zatrudnienie { get; set; } = "Etat"; // Etat | UmowaCywilnoprawna
        public string DefinicjaListyPlac { get; set; } = "00000000-0007-0001-0001-000000000000";
        public string Zakres { get; set; }

        public OkresNaliczaniaDane OkresNaliczania { get; set; } = new OkresNaliczaniaDane();
        public AlgorytmDane Algorytm { get; set; } = new AlgorytmDane();
        public DeklaracjeDane Deklaracje { get; set; } = new DeklaracjeDane();
        public NieobecnosciDane Nieobecnosci { get; set; } = new NieobecnosciDane();
        public ZaokraglenieDane Zaokraglenie { get; set; } = new ZaokraglenieDane();
        public GusDane GUS { get; set; } = new GusDane();
        public RozliczenieDane Rozliczenie { get; set; } = new RozliczenieDane();
        public ZajecieDane Zajecie { get; set; } = new ZajecieDane();

        public bool Korygowany { get; set; } = true;
        public bool MinimalneWynagrodzenie { get; set; } = true;
        public bool DoWyplaty { get; set; } = true;
        public bool ZawszeGotowka { get; set; }
        public bool GenerujZerowy { get; set; }
        public bool Aktualizacja { get; set; }
        public bool Zerowanie { get; set; }

        public DodatkoweDane Dodatkowe { get; set; } = new DodatkoweDane();
        public string Tekst { get; set; }
        public string RodzajNaliczania { get; set; } = "Wszystkie";
        public PracownicyZaGranicaDane PracownicyZaGranica { get; set; } = new PracownicyZaGranicaDane();
        public string StanowiKUP { get; set; } = "Domyślnie";
        public string UwzgledniajZwrotSkladkiPPK { get; set; } = "NieDotyczy";
        public bool UwzgledniajWLimicieZFSS { get; set; }
        public bool PulpitPracownika { get; set; }
        public string WliczajDoPodstawyWynagrodzeniaZaNadgodziny { get; set; } = "Nie";
        public bool WliczajUmoweDoStazuPracy { get; set; }
        public string Definicja_relacji_odbiorcy_elementu { get; set; }
        public string Progi { get; set; }

        public XElement ToXml()
        {
            var ns = DefinicjaElementuXmlBuilder.Ns;

            // Pola pomocnicze kreatora algorytmu domyslnie odwoluja sie do nazwy elementu,
            // tak jak we wzorcu ("DodPodstawa" == Nazwa, "ElPodstawa1" == Nazwa + " nominalnie") -
            // jesli nie zostaly jawnie ustawione, uzupelniamy je na podstawie Nazwa.
            if (string.IsNullOrEmpty(Algorytm.DodPodstawa))
                Algorytm.DodPodstawa = Nazwa;
            if (string.IsNullOrEmpty(Algorytm.ElPodstawa1))
                Algorytm.ElPodstawa1 = string.IsNullOrEmpty(Nazwa) ? null : Nazwa + " nominalnie";

            return new XElement(ns + "DefinicjaElementu",
                new XAttribute("id", string.IsNullOrEmpty(Id) ? "DefinicjaElementu_1" : Id),
                new XAttribute("guid", Guid.ToString()),
                Xml.El(ns, "Podstawowa", Podstawowa),
                Xml.El(ns, "Wydzial", Wydzial),
                Xml.El(ns, "Nastepny", Nastepny),
                Xml.El(ns, "DoDnia", DoDnia),
                Xml.El(ns, "RodzajZrodla", RodzajZrodla),
                Xml.El(ns, "Nazwa", Nazwa),
                Xml.El(ns, "NazwaPomocnicza", NazwaPomocnicza),
                Xml.El(ns, "Skrot", Skrot),
                Xml.El(ns, "Kod", Kod),
                Xml.ElBool(ns, "Blokada", Blokada),
                Xml.ElInt(ns, "Kolejnosc", Kolejnosc),
                Xml.El(ns, "Zatrudnienie", Zatrudnienie),
                Xml.El(ns, "DefinicjaListyPlac", DefinicjaListyPlac),
                Xml.El(ns, "Zakres", Zakres),
                OkresNaliczania.ToXml(ns),
                Algorytm.ToXml(ns),
                Deklaracje.ToXml(ns),
                Nieobecnosci.ToXml(ns),
                Zaokraglenie.ToXml(ns),
                GUS.ToXml(ns),
                Rozliczenie.ToXml(ns),
                Zajecie.ToXml(ns),
                Xml.ElBool(ns, "Korygowany", Korygowany),
                Xml.ElBool(ns, "MinimalneWynagrodzenie", MinimalneWynagrodzenie),
                Xml.ElBool(ns, "DoWyplaty", DoWyplaty),
                Xml.ElBool(ns, "ZawszeGotowka", ZawszeGotowka),
                Xml.ElBool(ns, "GenerujZerowy", GenerujZerowy),
                Xml.ElBool(ns, "Aktualizacja", Aktualizacja),
                Xml.ElBool(ns, "Zerowanie", Zerowanie),
                Dodatkowe.ToXml(ns),
                Xml.El(ns, "Tekst", Tekst),
                Xml.El(ns, "RodzajNaliczania", RodzajNaliczania),
                PracownicyZaGranica.ToXml(ns),
                Xml.El(ns, "StanowiKUP", StanowiKUP),
                Xml.El(ns, "UwzgledniajZwrotSkladkiPPK", UwzgledniajZwrotSkladkiPPK),
                Xml.ElBool(ns, "UwzgledniajWLimicieZFSS", UwzgledniajWLimicieZFSS),
                Xml.ElBool(ns, "PulpitPracownika", PulpitPracownika),
                Xml.El(ns, "WliczajDoPodstawyWynagrodzeniaZaNadgodziny", WliczajDoPodstawyWynagrodzeniaZaNadgodziny),
                new XElement(ns + "RuntimeInfo",
                    Xml.El(ns, "Project", null),
                    Xml.El(ns, "Identifier", null),
                    Xml.El(ns, "FileName", null)),
                Xml.ElBool(ns, "WliczajUmoweDoStazuPracy", WliczajUmoweDoStazuPracy),
                Xml.El(ns, "Definicja_relacji_odbiorcy_elementu", Definicja_relacji_odbiorcy_elementu),
                Xml.El(ns, "Progi", Progi));
        }
    }

    // ---------------------------------------------------------------------
    // 2. OkresNaliczania
    // ---------------------------------------------------------------------

    public sealed class OkresNaliczaniaDane
    {
        public string Naliczanie { get; set; } = "PłatnaZDołu";
        public int Opoznienie { get; set; }
        public string Typ { get; set; } = "CoNMiesięcy";
        public int Ilosc { get; set; } = 1;
        public string DlaKazdejUmowy { get; set; } = "Nie";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "OkresNaliczania",
                Xml.El(ns, "Naliczanie", Naliczanie),
                Xml.ElInt(ns, "Opoznienie", Opoznienie),
                Xml.El(ns, "Typ", Typ),
                Xml.ElInt(ns, "Ilosc", Ilosc),
                Xml.El(ns, "DlaKazdejUmowy", DlaKazdejUmowy));
        }
    }

    // ---------------------------------------------------------------------
    // 3. Algorytm (+ KreatorAlgorytmu i pochodne)
    // ---------------------------------------------------------------------

    public sealed class AlgorytmDane
    {
        public int Priorytet { get; set; }
        public bool Potracenie { get; set; }
        public string LimitPotracenia { get; set; } = "NieDotyczy";
        public string Typ { get; set; } = "KreatorAlgorytmu";
        public KreatorAlgorytmuDane KreatorAlgorytmu { get; set; } = new KreatorAlgorytmuDane();

        public string JakUrlopWypoczynkowy { get; set; }
        public string JakUrlopOkolicznosciowy { get; set; }
        public JakEkwiwalentZaUrlopDane JakEkwiwalentZaUrlop { get; set; } = new JakEkwiwalentZaUrlopDane();
        public string Edytor { get; set; }
        public JakChorobweDane JakChorobowe { get; set; } = new JakChorobweDane();
        public KlasaDane Klasa { get; set; } = new KlasaDane();
        public ZaNieobecnoscDane ZaNieobecnosc { get; set; } = new ZaNieobecnoscDane();
        public PomniejszeniaIOdchylkiDane PomniejszeniaIOdchylki { get; set; } = new PomniejszeniaIOdchylkiDane();

        public int Liczba { get; set; }
        public decimal Podstawa { get; set; }
        public string Ulamek { get; set; } = "0/1";
        public string Czas { get; set; } = "0:00";
        public int Dni { get; set; }

        // Uzupelniane automatycznie na podstawie Nazwa elementu, jesli nie ustawione recznie.
        public string DodPodstawa { get; set; }
        public string DodUlamek { get; set; }
        public string DodProcent { get; set; }
        public string DodWspolczynnik { get; set; }
        public string DodCzas { get; set; }
        public string DodDni { get; set; }

        public bool ElPodglad { get; set; }
        public string ElPodstawa1 { get; set; }
        public string ElPodstawa2 { get; set; }
        public string ElPodstawa3 { get; set; }
        public string ElPodstawa4 { get; set; }
        public string ElPodstawa5 { get; set; }
        public string ElUlamek { get; set; }
        public string ElProcent { get; set; }
        public string ElWspolczynnik { get; set; }
        public string ElCzas { get; set; }
        public string ElDni { get; set; }
        public string ElIlosc { get; set; }
        public string ElPodstawaOkres { get; set; }
        public string ElRazemCzas { get; set; }
        public string ElRazemDni { get; set; }

        public UmowaAlgorytmDane Umowa { get; set; } = new UmowaAlgorytmDane();
        public RozliczenieCzasuUmowyDane RozliczenieCzasuUmowy { get; set; } = new RozliczenieCzasuUmowyDane();
        public ZapisObliczenDane ZapisObliczen { get; set; } = new ZapisObliczenDane();
        public bool JakEkwiwalent { get; set; }
        public bool TylkoPelnePotracenie { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Algorytm",
                Xml.ElInt(ns, "Priorytet", Priorytet),
                Xml.ElBool(ns, "Potracenie", Potracenie),
                Xml.El(ns, "LimitPotracenia", LimitPotracenia),
                Xml.El(ns, "Typ", Typ),
                KreatorAlgorytmu.ToXml(ns),
                Xml.El(ns, "JakUrlopWypoczynkowy", JakUrlopWypoczynkowy),
                Xml.El(ns, "JakUrlopOkolicznosciowy", JakUrlopOkolicznosciowy),
                JakEkwiwalentZaUrlop.ToXml(ns),
                Xml.El(ns, "Edytor", Edytor),
                JakChorobowe.ToXml(ns),
                Klasa.ToXml(ns),
                ZaNieobecnosc.ToXml(ns),
                PomniejszeniaIOdchylki.ToXml(ns),
                Xml.ElInt(ns, "Liczba", Liczba),
                Xml.ElKwota(ns, "Podstawa", Podstawa),
                Xml.El(ns, "Ulamek", Ulamek),
                Xml.El(ns, "Czas", Czas),
                Xml.ElInt(ns, "Dni", Dni),
                Xml.El(ns, "DodPodstawa", DodPodstawa),
                Xml.El(ns, "DodUlamek", DodUlamek),
                Xml.El(ns, "DodProcent", DodProcent),
                Xml.El(ns, "DodWspolczynnik", DodWspolczynnik),
                Xml.El(ns, "DodCzas", DodCzas),
                Xml.El(ns, "DodDni", DodDni),
                Xml.ElBool(ns, "ElPodglad", ElPodglad),
                Xml.El(ns, "ElPodstawa1", ElPodstawa1),
                Xml.El(ns, "ElPodstawa2", ElPodstawa2),
                Xml.El(ns, "ElPodstawa3", ElPodstawa3),
                Xml.El(ns, "ElPodstawa4", ElPodstawa4),
                Xml.El(ns, "ElPodstawa5", ElPodstawa5),
                Xml.El(ns, "ElUlamek", ElUlamek),
                Xml.El(ns, "ElProcent", ElProcent),
                Xml.El(ns, "ElWspolczynnik", ElWspolczynnik),
                Xml.El(ns, "ElCzas", ElCzas),
                Xml.El(ns, "ElDni", ElDni),
                Xml.El(ns, "ElIlosc", ElIlosc),
                Xml.El(ns, "ElPodstawaOkres", ElPodstawaOkres),
                Xml.El(ns, "ElRazemCzas", ElRazemCzas),
                Xml.El(ns, "ElRazemDni", ElRazemDni),
                Umowa.ToXml(ns),
                RozliczenieCzasuUmowy.ToXml(ns),
                ZapisObliczen.ToXml(ns),
                Xml.ElBool(ns, "JakEkwiwalent", JakEkwiwalent),
                Xml.ElBool(ns, "TylkoPelnePotracenie", TylkoPelnePotracenie));
        }
    }

    public sealed class KreatorAlgorytmuDane
    {
        public string PodstawaTyp { get; set; } = "Kwota"; // Kwota | Wzor | ...
        public string PodstawaNazwa { get; set; }
        public bool Proporcjonalnie { get; set; }
        public string SposobProporcjonalnosci { get; set; } = "NiePomniejszany";
        public string PodstawaZa { get; set; } = "Miesięcznie";
        public WspolczynnikDane Wspolczynnik { get; set; } = new WspolczynnikDane();
        public CzasKreatoraDane Czas { get; set; } = new CzasKreatoraDane();
        public KorektaDane Korekta { get; set; } = new KorektaDane();
        public string PrzeliczNa1h { get; set; } = "NiePrzeliczaj";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "KreatorAlgorytmu",
                Xml.El(ns, "PodstawaTyp", PodstawaTyp),
                Xml.El(ns, "PodstawaNazwa", PodstawaNazwa),
                Xml.ElBool(ns, "Proporcjonalnie", Proporcjonalnie),
                Xml.El(ns, "SposobProporcjonalnosci", SposobProporcjonalnosci),
                Xml.El(ns, "PodstawaZa", PodstawaZa),
                Wspolczynnik.ToXml(ns),
                Czas.ToXml(ns),
                Korekta.ToXml(ns),
                Xml.El(ns, "PrzeliczNa1h", PrzeliczNa1h));
        }
    }

    public sealed class WspolczynnikDane
    {
        public string Typ { get; set; } = "BezWspółczynnika";
        public bool Wskaznik { get; set; }
        public string NazwaWskaznika { get; set; }
        public bool PracaWFirmie { get; set; }
        public string PracaNaDzien { get; set; } = "OstatniDzieńOkresu";
        public string PodstawaStazu { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Wspolczynnik",
                Xml.El(ns, "Typ", Typ),
                Xml.ElBool(ns, "Wskaznik", Wskaznik),
                Xml.El(ns, "NazwaWskaznika", NazwaWskaznika),
                Xml.ElBool(ns, "PracaWFirmie", PracaWFirmie),
                Xml.El(ns, "PracaNaDzien", PracaNaDzien),
                Xml.El(ns, "PodstawaStazu", PodstawaStazu));
        }
    }

    public sealed class CzasKreatoraDane
    {
        public string Typ { get; set; } = "NieUwzględniaj";
        public string Strefa { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Czas",
                Xml.El(ns, "Typ", Typ),
                Xml.El(ns, "Strefa", Strefa));
        }
    }

    public sealed class KorektaDane
    {
        public bool Odchylki { get; set; } = true;
        public string Nieusprawiedliwione { get; set; } = "Proporcjonalnie";
        public string Usprawiedliwione { get; set; } = "Proporcjonalnie";
        public string Platne { get; set; } = "NiePomniejsza";
        public string Macierzynske { get; set; } = "Proporcjonalnie";
        public string Rehabilitacyjne { get; set; } = "Proporcjonalnie";
        public string Opiekuncze { get; set; } = "Proporcjonalnie";
        public string Chorobowe { get; set; } = "Proporcjonalnie";
        public string Wychowawcze { get; set; } = "Proporcjonalnie";
        public bool OgraniczeniePomniejszen { get; set; }
        public bool WybraneOdchylki { get; set; }
        public bool OdchylkiPlus { get; set; }
        public bool OdchylkiMinus { get; set; }
        public bool Postojowe { get; set; }
        public bool OdchylkiAkord { get; set; }
        public bool WybraneOdchylkiPlus { get; set; }
        public bool OdchylkiPlus50 { get; set; }
        public bool OdchylkiPlus100 { get; set; }
        public bool OdchylkiPlusSW { get; set; }
        public bool OdchylkiPlusPozostale { get; set; }
        public bool OdchylkiRozliczane { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Korekta",
                Xml.ElBool(ns, "Odchylki", Odchylki),
                Xml.El(ns, "Nieusprawiedliwione", Nieusprawiedliwione),
                Xml.El(ns, "Usprawiedliwione", Usprawiedliwione),
                Xml.El(ns, "Platne", Platne),
                Xml.El(ns, "Macierzynske", Macierzynske),
                Xml.El(ns, "Rehabilitacyjne", Rehabilitacyjne),
                Xml.El(ns, "Opiekuncze", Opiekuncze),
                Xml.El(ns, "Chorobowe", Chorobowe),
                Xml.El(ns, "Wychowawcze", Wychowawcze),
                Xml.ElBool(ns, "OgraniczeniePomniejszen", OgraniczeniePomniejszen),
                Xml.ElBool(ns, "WybraneOdchylki", WybraneOdchylki),
                Xml.ElBool(ns, "OdchylkiPlus", OdchylkiPlus),
                Xml.ElBool(ns, "OdchylkiMinus", OdchylkiMinus),
                Xml.ElBool(ns, "Postojowe", Postojowe),
                Xml.ElBool(ns, "OdchylkiAkord", OdchylkiAkord),
                Xml.ElBool(ns, "WybraneOdchylkiPlus", WybraneOdchylkiPlus),
                Xml.ElBool(ns, "OdchylkiPlus50", OdchylkiPlus50),
                Xml.ElBool(ns, "OdchylkiPlus100", OdchylkiPlus100),
                Xml.ElBool(ns, "OdchylkiPlusSW", OdchylkiPlusSW),
                Xml.ElBool(ns, "OdchylkiPlusPozostale", OdchylkiPlusPozostale),
                Xml.ElBool(ns, "OdchylkiRozliczane", OdchylkiRozliczane));
        }
    }

    public sealed class JakEkwiwalentZaUrlopDane
    {
        public bool WgWymiaruEtatu { get; set; }
        public string DefinicjaLimitu { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "JakEkwiwalentZaUrlop",
                Xml.ElBool(ns, "WgWymiaruEtatu", WgWymiaruEtatu),
                Xml.El(ns, "DefinicjaLimitu", DefinicjaLimitu));
        }
    }

    public sealed class JakChorobweDane
    {
        public int Zatrudnienie { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "JakChorobowe", Xml.ElInt(ns, "Zatrudnienie", Zatrudnienie));
        }
    }

    public sealed class KlasaDane
    {
        public string Nazwa { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Klasa", Xml.El(ns, "Nazwa", Nazwa));
        }
    }

    public sealed class ZaNieobecnoscDane
    {
        public bool Macierzynskiego { get; set; }
        public bool Rehabilitacyjnego { get; set; }
        public bool Opiekunczego { get; set; }
        public bool Chorobowego { get; set; }
        public bool Wychowawczego { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "ZaNieobecnosc",
                Xml.ElBool(ns, "Macierzynskiego", Macierzynskiego),
                Xml.ElBool(ns, "Rehabilitacyjnego", Rehabilitacyjnego),
                Xml.ElBool(ns, "Opiekunczego", Opiekunczego),
                Xml.ElBool(ns, "Chorobowego", Chorobowego),
                Xml.ElBool(ns, "Wychowawczego", Wychowawczego));
        }
    }

    public sealed class PomniejszeniaIOdchylkiDane
    {
        public string WybranaStrefa { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "PomniejszeniaIOdchylki", Xml.El(ns, "WybranaStrefa", WybranaStrefa));
        }
    }

    public sealed class UmowaAlgorytmDane
    {
        public string TypWartosci { get; set; } = "Dowolny";
        public string RodzajRozliczenia { get; set; } = "Dowolny";
        public string MinimalnaStawkaGodz { get; set; } = "NieDotyczy";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Umowa",
                Xml.El(ns, "TypWartosci", TypWartosci),
                Xml.El(ns, "RodzajRozliczenia", RodzajRozliczenia),
                Xml.El(ns, "MinimalnaStawkaGodz", MinimalnaStawkaGodz));
        }
    }

    public sealed class RozliczenieCzasuUmowyDane
    {
        public string Podstawowa { get; set; }
        public bool WliczanyDoRozliczenia { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "RozliczenieCzasuUmowy",
                Xml.El(ns, "Podstawowa", Podstawowa),
                Xml.ElBool(ns, "WliczanyDoRozliczenia", WliczanyDoRozliczenia));
        }
    }

    public sealed class ZapisObliczenDane
    {
        public string Nazwa { get; set; } = "Algorytmy płacowe";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "ZapisObliczen", Xml.El(ns, "Nazwa", Nazwa));
        }
    }

    // ---------------------------------------------------------------------
    // 4. Deklaracje
    // ---------------------------------------------------------------------

    public sealed class DeklaracjeDane
    {
        public string PozycjaPIT { get; set; } = "00000000-0007-0004-0001-000000000000";
        public string OpisNaPIT8C { get; set; }
        public int Priorytet { get; set; }
        public KosztyDane Koszty { get; set; } = new KosztyDane();
        public ZaliczkaDane Zaliczka { get; set; } = new ZaliczkaDane();
        public UlgaDane Ulga { get; set; } = new UlgaDane();
        public string KodRSA { get; set; }
        public string PozycjaRCA { get; set; } = "NieDotyczy";
        public string KodRNA { get; set; } = "PremiaMiesięczna";
        public string PozycjaRP7 { get; set; } = "SkładnikiStałe";
        public SpoleczneDane Spoleczne { get; set; } = new SpoleczneDane();
        public ZdrowotneDane Zdrowotne { get; set; } = new ZdrowotneDane();
        public NarzutyDane Narzuty { get; set; } = new NarzutyDane();
        public string Umowa { get; set; } = "NieDotyczy";
        public string UmowaTyUb { get; set; }
        public bool UmowaJakEtat { get; set; }
        public bool NieWliczajSOD { get; set; }
        public bool NieRozrzucaj { get; set; }
        public PodstawaSkladekDane PodstawaSkladek { get; set; } = new PodstawaSkladekDane();
        public bool NieoskladkowanyZUS { get; set; }
        public string TrybPodleganiaRozpMF070122 { get; set; } = "Domyślnie";
        public string WliczajDoKosztowPlacy { get; set; } = "Domyślnie";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Deklaracje",
                Xml.El(ns, "PozycjaPIT", PozycjaPIT),
                Xml.El(ns, "OpisNaPIT8C", OpisNaPIT8C),
                Xml.ElInt(ns, "Priorytet", Priorytet),
                Koszty.ToXml(ns),
                Zaliczka.ToXml(ns),
                Ulga.ToXml(ns),
                Xml.El(ns, "KodRSA", KodRSA),
                Xml.El(ns, "PozycjaRCA", PozycjaRCA),
                Xml.El(ns, "KodRNA", KodRNA),
                Xml.El(ns, "PozycjaRP7", PozycjaRP7),
                Spoleczne.ToXml(ns),
                Zdrowotne.ToXml(ns),
                Narzuty.ToXml(ns),
                Xml.El(ns, "Umowa", Umowa),
                Xml.El(ns, "UmowaTyUb", UmowaTyUb),
                Xml.ElBool(ns, "UmowaJakEtat", UmowaJakEtat),
                Xml.ElBool(ns, "NieWliczajSOD", NieWliczajSOD),
                Xml.ElBool(ns, "NieRozrzucaj", NieRozrzucaj),
                PodstawaSkladek.ToXml(ns),
                Xml.ElBool(ns, "NieoskladkowanyZUS", NieoskladkowanyZUS),
                Xml.El(ns, "TrybPodleganiaRozpMF070122", TrybPodleganiaRozpMF070122),
                Xml.El(ns, "WliczajDoKosztowPlacy", WliczajDoKosztowPlacy));
        }
    }

    public sealed class KosztyDane
    {
        public string Typ { get; set; } = "ZeStosunkuPracy";
        public decimal Procent { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Koszty",
                Xml.El(ns, "Typ", Typ),
                Xml.ElProcent(ns, "Procent", Procent));
        }
    }

    public sealed class ZaliczkaDane
    {
        public string Typ { get; set; } = "NaliczaćWgSkaliPodatkowej";
        public decimal Procent { get; set; }
        public bool PomniejszonaZUS { get; set; } = true;
        public string NaliczaniePit26 { get; set; } = "Warunkowo";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Zaliczka",
                Xml.El(ns, "Typ", Typ),
                Xml.ElProcent(ns, "Procent", Procent),
                Xml.ElBool(ns, "PomniejszonaZUS", PomniejszonaZUS),
                Xml.El(ns, "NaliczaniePit26", NaliczaniePit26));
        }
    }

    public sealed class UlgaDane
    {
        public string Typ { get; set; } = "Naliczać";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Ulga", Xml.El(ns, "Typ", Typ));
        }
    }

    public sealed class SpoleczneDane
    {
        public string Typ { get; set; } = "Naliczać";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Spoleczne", Xml.El(ns, "Typ", Typ));
        }
    }

    public sealed class ZdrowotneDane
    {
        public string Typ { get; set; } = "Naliczać";
        public string PomniejszaFIS { get; set; } = "PomniejszaDoFIS";
        public bool PomniejszoneZUS { get; set; } = true;

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Zdrowotne",
                Xml.El(ns, "Typ", Typ),
                Xml.El(ns, "PomniejszaFIS", PomniejszaFIS),
                Xml.ElBool(ns, "PomniejszoneZUS", PomniejszoneZUS));
        }
    }

    public sealed class NarzutyDane
    {
        public string SkladkaFP { get; set; } = "Domyślna";
        public string SkladkaFGSP { get; set; } = "Domyślna";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Narzuty",
                Xml.El(ns, "SkladkaFP", SkladkaFP),
                Xml.El(ns, "SkladkaFGSP", SkladkaFGSP));
        }
    }

    public sealed class PodstawaSkladekDane
    {
        public string UrlopWychowawczy { get; set; } = "WliczaćDla";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "PodstawaSkladek", Xml.El(ns, "UrlopWychowawczy", UrlopWychowawczy));
        }
    }

    // ---------------------------------------------------------------------
    // 5. Nieobecnosci
    // ---------------------------------------------------------------------

    public sealed class NieobecnosciDane
    {
        public bool WahaniaWysokosci { get; set; }
        public UrlopNieobDane Urlop { get; set; } = new UrlopNieobDane();
        public UrlopNieobDane Ekwiwalent { get; set; } = new UrlopNieobDane();
        public ZasilkiDane ZasilkiPracownicy { get; set; } = new ZasilkiDane();
        public ZasilkiDane ZasilkiInni { get; set; } = new ZasilkiDane();
        public string DefinicjaLimitu { get; set; }
        public string WliczanieDoPodstawyChorobowego { get; set; } = "Domyślnie";
        public string OdMiesiaca { get; set; } = "styczen";
        public bool DoMinimalnej { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Nieobecnosci",
                Xml.ElBool(ns, "WahaniaWysokosci", WahaniaWysokosci),
                Urlop.ToXml(ns, "Urlop"),
                Ekwiwalent.ToXml(ns, "Ekwiwalent"),
                ZasilkiPracownicy.ToXml(ns, "ZasilkiPracownicy"),
                ZasilkiInni.ToXml(ns, "ZasilkiInni"),
                Xml.El(ns, "DefinicjaLimitu", DefinicjaLimitu),
                Xml.El(ns, "WliczanieDoPodstawyChorobowego", WliczanieDoPodstawyChorobowego),
                Xml.El(ns, "OdMiesiaca", OdMiesiaca),
                Xml.ElBool(ns, "DoMinimalnej", DoMinimalnej));
        }
    }

    public sealed class UrlopNieobDane
    {
        public string Typ { get; set; } = "NieWliczać";
        public bool Dopelnienie { get; set; }
        public int WspolczynnikTyp { get; set; }
        public string WspolczynnikNazwa { get; set; }

        public XElement ToXml(XNamespace ns, string nazwaElementu)
        {
            return new XElement(ns + nazwaElementu,
                Xml.El(ns, "Typ", Typ),
                Xml.ElBool(ns, "Dopelnienie", Dopelnienie),
                Xml.ElInt(ns, "WspolczynnikTyp", WspolczynnikTyp),
                Xml.El(ns, "WspolczynnikNazwa", WspolczynnikNazwa));
        }
    }

    public sealed class ZasilkiDane
    {
        public string Typ { get; set; } = "WliczaćDla";
        public bool Chorobowych { get; set; } = true;
        public bool Macierzynskich { get; set; } = true;
        public bool Rehabilitacyjnych { get; set; } = true;
        public bool Opiekunczych { get; set; } = true;

        public XElement ToXml(XNamespace ns, string nazwaElementu)
        {
            return new XElement(ns + nazwaElementu,
                Xml.El(ns, "Typ", Typ),
                Xml.ElBool(ns, "Chorobowych", Chorobowych),
                Xml.ElBool(ns, "Macierzynskich", Macierzynskich),
                Xml.ElBool(ns, "Rehabilitacyjnych", Rehabilitacyjnych),
                Xml.ElBool(ns, "Opiekunczych", Opiekunczych));
        }
    }

    // ---------------------------------------------------------------------
    // 6-9. Zaokraglenie, GUS, Rozliczenie, Zajecie
    // ---------------------------------------------------------------------

    public sealed class ZaokraglenieDane
    {
        public string Precyzja { get; set; } = "DoPełnegoGrosza";
        public string Sposob { get; set; } = "Standardowe";
        public bool Podstawa { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Zaokraglenie",
                Xml.El(ns, "Precyzja", Precyzja),
                Xml.El(ns, "Sposob", Sposob),
                Xml.ElBool(ns, "Podstawa", Podstawa));
        }
    }

    public sealed class GusDane
    {
        public string Kategoria { get; set; } = "WgDefinicjiElementu";
        public bool ZaliczajDoCzasuPracy { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "GUS",
                Xml.El(ns, "Kategoria", Kategoria),
                Xml.ElBool(ns, "ZaliczajDoCzasuPracy", ZaliczajDoCzasuPracy));
        }
    }

    public sealed class RozliczenieDane
    {
        public string Ewidencja { get; set; }
        public bool ZmianaOdbiorcy { get; set; }
        public bool WspolnaPlatnosc { get; set; }
        public string OpisPrzelewu { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Rozliczenie",
                Xml.El(ns, "Ewidencja", Ewidencja),
                Xml.ElBool(ns, "ZmianaOdbiorcy", ZmianaOdbiorcy),
                Xml.ElBool(ns, "WspolnaPlatnosc", WspolnaPlatnosc),
                Xml.El(ns, "OpisPrzelewu", OpisPrzelewu));
        }
    }

    public sealed class ZajecieDane
    {
        public string Komornik { get; set; } = "Domyślnie";
        public string Alimenty { get; set; } = "Domyślnie";
        public string Pozostale { get; set; } = "Domyślnie";
        public bool UwzgledniajJakWyplatyZFSS { get; set; }

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Zajecie",
                Xml.El(ns, "Komornik", Komornik),
                Xml.El(ns, "Alimenty", Alimenty),
                Xml.El(ns, "Pozostale", Pozostale),
                Xml.ElBool(ns, "UwzgledniajJakWyplatyZFSS", UwzgledniajJakWyplatyZFSS));
        }
    }

    // ---------------------------------------------------------------------
    // 10. Dodatkowe, PracownicyZaGranica
    // ---------------------------------------------------------------------

    public sealed class DodatkoweDane
    {
        public bool DodatekDoEtatu { get; set; } = true;
        public string RefundowanyInfo { get; set; } = "Domyślny";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "Dodatkowe",
                Xml.ElBool(ns, "DodatekDoEtatu", DodatekDoEtatu),
                Xml.El(ns, "RefundowanyInfo", RefundowanyInfo));
        }
    }

    public sealed class PracownicyZaGranicaDane
    {
        public bool KorektaPodstaw { get; set; }
        public string Podstawa { get; set; } = "NieDotyczy";

        public XElement ToXml(XNamespace ns)
        {
            return new XElement(ns + "PracownicyZaGranica",
                Xml.ElBool(ns, "KorektaPodstaw", KorektaPodstaw),
                Xml.El(ns, "Podstawa", Podstawa));
        }
    }
}
