using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Soneta.Business;
using Soneta.Business.App;
using Soneta.Kadry;
using Soneta.Kalend;
using Soneta.Place;
using Soneta.Types;

namespace A1.KalkulatorPodstawy
{
    /// <summary>Dane wejściowe jednego przeliczenia (wartości, nie wiersze - sesja symulacji jest osobna).</summary>
    public sealed class ParametrySymulacji
    {
        public Guid Pracownik { get; set; }
        public Guid DefinicjaNieobecnosci { get; set; }
        public Date DataOd { get; set; }
        public Date DataDo { get; set; }
        public PrzyczynaZwolnienia KodZwolnienia { get; set; }
        public bool LeczenieSzpitalne { get; set; }
        public bool WymusPonowneUstalenie { get; set; }
    }

    /// <summary>
    /// Kalkulator nie liczy reguł ustawy sam. Buduje w osobnej sesji sytuację "pracownik jest niezdolny
    /// do pracy od dnia D", nalicza ją standardowym naliczaniem wypłat enova (NaliczanieWypłat) i czyta
    /// wynik oraz rozbicie z natywnego silnika Soneta.Place.PodstawaZasiłku.
    ///
    /// Sesja symulacji: Login.CreateSession(readOnly: false) + Logout(editMode: true) bez Commit
    /// i bez Session.Save(); Dispose wycofuje wszystko (wzorzec kalkulatora wynagrodzeń enova
    /// KalkulatorWynagrodzenParams.Przelicz oraz NaliczanieZUS2022.PodstawaWychowawczego2012).
    /// </summary>
    public static class SymulacjaPodstawyZasilku
    {
        public static WynikPodstawyZasilku Policz(Login login, ParametrySymulacji parametry)
        {
            var wynik = new WynikPodstawyZasilku();
            try
            {
                using (var log = new PrzechwytywanieLogu())
                {
                    using (Session sesja = login.CreateSession(false, false, "Kalkulator podstawy zasiłku"))
                    {
                        using (ITransaction transakcja = sesja.Logout(true))
                        {
                            PoliczWSesji(sesja, parametry, wynik);
                            // Celowo bez transakcja.Commit() i bez sesja.Save() - wszystko wycofane.
                        }
                    }
                    wynik.Log = log.Tekst;
                }
            }
            catch (Exception wyjatek)
            {
                wynik.Blad = OpisBledu(wyjatek);
            }
            return wynik;
        }

        private static void PoliczWSesji(Session sesja, ParametrySymulacji parametry, WynikPodstawyZasilku wynik)
        {
            KadryModule kadry = sesja.GetKadry();
            KalendModule kalend = KalendModule.GetInstance(sesja);
            PlaceModule place = sesja.GetPlace();

            Pracownik pracownik = kadry.Pracownicy[parametry.Pracownik];
            if (pracownik == null)
                throw new InvalidOperationException("Nie znaleziono pracownika.");
            wynik.Pracownik = pracownik.ToString();
            wynik.KodPracownika = pracownik.Kod;

            Nieobecnosc istniejaca = ZnajdzNieobecnoscZUS(pracownik, parametry.DataOd);
            WypElementNieobecność zapisanyElement = istniejaca == null ? null : ZnajdzNaliczonyElement(pracownik, istniejaca);

            Nieobecnosc nieobecnosc;
            List<WypElementNieobecność> elementy;
            if (zapisanyElement != null)
            {
                // Nieobecność jest już naliczona na liście płac: przeliczamy podstawę na tym samym elemencie
                // (to samo robi natywna czynność "Pokaż log naliczania podstawy") i porównujemy z zapisaną.
                nieobecnosc = istniejaca;
                elementy = ElementyNieobecnosci(pracownik, istniejaca);
                wynik.Tryb = "Przeliczenie nieobecności naliczonej w bazie (porównanie z zapisaną podstawą)";
                wynik.PodstawaZapisanaWBazie = PodstawaSkladnika(zapisanyElement);
                wynik.JestPodstawaWBazie = true;
            }
            else
            {
                if (istniejaca != null)
                {
                    nieobecnosc = istniejaca;
                    wynik.Tryb = "Symulacja naliczenia istniejącej (nienaliczonej) nieobecności";
                }
                else
                {
                    nieobecnosc = DodajNieobecnosc(kalend, pracownik, parametry);
                    wynik.Tryb = "Symulacja: nieobecność od " + parametry.DataOd + " dodana tylko w pamięci";
                }
                elementy = NaliczWPamieci(pracownik, nieobecnosc);
            }

            wynik.DefinicjaNieobecnosci = nieobecnosc.Definicja.Nazwa;
            wynik.RodzajSwiadczenia = RodzajSwiadczenia(nieobecnosc.Definicja);
            wynik.OkresNieobecnosci = nieobecnosc.Okres;

            if (elementy.Count == 0)
                throw new InvalidOperationException(
                    "Naliczanie enova nie utworzyło elementu za tę nieobecność (np. brak prawa do świadczenia, " +
                    "okres wyczekiwania lub definicja nieobecności bez elementu płacowego). Szczegóły w logu.");

            foreach (WypElementNieobecność element in elementy)
                wynik.NaliczoneElementy.Add(OpisElementu(element));

            WypElementNieobecność pierwszy = elementy[0];
            wynik.PodstawaNaElemencie = PodstawaSkladnika(pierwszy);

            // Rozbicie: ten sam konstruktor, którego używa standardowy algorytm "Jak chorobowe".
            var silnik = new PodstawaZasiłku(pierwszy);
            if (parametry.WymusPonowneUstalenie)
                silnik.PomińPodstawęZPoprzedniejNieobecności = true;
            wynik.Podstawa = silnik.Podstawa;
            wynik.Podsumowanie = silnik.Info;
            if (silnik.Miesiące.Count > 0 || silnik.MiesiąceOkresowe.Count > 0)
            {
                wynik.OkresPodstawyOd = silnik.NaliczanieOd;
                wynik.OkresPodstawyDo = silnik.NaliczanieDo;
            }

            ChorobowyConfig(place, out bool miesiaceWgDni, out YearMonth wgDniOd);
            foreach (PodstawaZasiłku.PoliczMiesiąc miesiac in silnik.Miesiące)
                DodajMiesiac(wynik, miesiac, false, miesiaceWgDni && miesiac.Miesiąc >= wgDniOd);
            foreach (PodstawaZasiłku.PoliczMiesiąc miesiac in silnik.MiesiąceOkresowe)
                DodajMiesiac(wynik, miesiac, true, false);
            DodajPominieteSkladniki(wynik, pracownik, silnik, nieobecnosc.Definicja.Przyczyna);

            UstalStawki(wynik, place, pierwszy);
            UstalReguly(wynik, pracownik, silnik, parametry, pierwszy);
        }

        // ---------- nieobecność i naliczenie ----------

        private static Nieobecnosc ZnajdzNieobecnoscZUS(Pracownik pracownik, Date data)
        {
            foreach (Nieobecnosc n in pracownik.Nieobecnosci)
                if (n.Okres.Contains(data) && n.Definicja != null && n.Definicja.Typ == TypNieobecnosci.NieobecnośćZUS)
                    return n;
            return null;
        }

        private static List<WypElementNieobecność> ElementyNieobecnosci(Pracownik pracownik, Nieobecnosc nieobecnosc)
        {
            var lista = new List<WypElementNieobecność>();
            foreach (WypElement element in pracownik.Elementy[nieobecnosc.Okres])
            {
                if (element is WypElementNieobecność en && en.Nieobecność == nieobecnosc
                    && !element.RozliczenieStorna && element.StanStorna == StanStornaElementu.NieDotyczy
                    && en.SkładnikGłówny != null)
                    lista.Add(en);
            }
            return lista.OrderBy(e => e.Okres.From).ToList();
        }

        private static WypElementNieobecność ZnajdzNaliczonyElement(Pracownik pracownik, Nieobecnosc nieobecnosc)
        {
            return ElementyNieobecnosci(pracownik, nieobecnosc).FirstOrDefault();
        }

        private static Nieobecnosc DodajNieobecnosc(KalendModule kalend, Pracownik pracownik, ParametrySymulacji parametry)
        {
            DefinicjaNieobecnosci definicja = kalend.DefNieobecnosci[parametry.DefinicjaNieobecnosci];
            if (definicja == null)
                throw new InvalidOperationException("Wybierz definicję nieobecności.");

            var nieobecnosc = new NieobecnośćPracownika(pracownik);
            kalend.Nieobecnosci.AddRow(nieobecnosc);
            nieobecnosc.Definicja = definicja;
            nieobecnosc.Okres = new FromTo(parametry.DataOd, parametry.DataDo);
            if (nieobecnosc.Zwolnienie.Przyczyna != parametry.KodZwolnienia)
                nieobecnosc.Zwolnienie.Przyczyna = parametry.KodZwolnienia;
            if (parametry.LeczenieSzpitalne)
                nieobecnosc.Zwolnienie.LeczenieSzpitalne = true;
            if (parametry.WymusPonowneUstalenie)
                nieobecnosc.Zwolnienie.SetPonownieUstalPodstawe(true); // jak natywna czynność „Ustal ponownie podstawę naliczania”
            return nieobecnosc;
        }

        /// <summary>
        /// Standardowe naliczanie wypłat enova tylko za tę nieobecność, za miesiąc jej początku.
        /// Ustawienia jak w KalkulatorWynagrodzenParams.Przelicz (data wypłaty = koniec miesiąca).
        /// </summary>
        private static List<WypElementNieobecność> NaliczWPamieci(Pracownik pracownik, Nieobecnosc nieobecnosc)
        {
            FromTo miesiac = FromTo.Month(nieobecnosc.Okres.From);
            var naliczanie = new NaliczanieWypłat { Pracownik = pracownik };
            naliczanie.DataWypłaty = miesiac.To;
            naliczanie.DataListy = miesiac.To;
            naliczanie.Okres = miesiac;
            naliczanie.MiesiącZUS = miesiac.ToYearMonth();
            naliczanie.DodajŹródło(nieobecnosc);
            naliczanie.UstawTypWypłaty(TypWyplaty.Wszystkie);
            naliczanie.KontynacjaNaliczenia = true;
            naliczanie.Nalicz();

            BłądNaliczaniaWynagrodzenia blad = naliczanie.Nienaliczeni.FirstOrDefault();
            if (blad != null)
                throw new InvalidOperationException("Naliczanie enova zgłosiło błąd: " + blad);

            var lista = new List<WypElementNieobecność>();
            foreach (WypElement element in naliczanie.NaliczaneElementy)
                if (element is WypElementNieobecność en && en.Nieobecność == nieobecnosc && en.SkładnikGłówny != null)
                    lista.Add(en);
            return lista.OrderBy(e => e.Okres.From).ToList();
        }

        // ---------- mapowanie wyniku ----------

        private static decimal PodstawaSkladnika(WypElement element)
        {
            WypSkladnikGłówny skladnik = element.SkładnikGłówny;
            return skladnik == null ? 0m : Kwota(skladnik.Podstawa1);
        }

        /// <summary>Podstawa1 składnika to DoubleCy (wartość double) - kwota w groszach jak na wypłacie.</summary>
        private static decimal Kwota(DoubleCy wartosc) => Math.Round((decimal)wartosc.Value, 2, MidpointRounding.AwayFromZero);

        private static NaliczonyElement OpisElementu(WypElementNieobecność element)
        {
            WypSkladnikGłówny skladnik = element.SkładnikGłówny;
            decimal podstawa = Kwota(skladnik.Podstawa1);
            return new NaliczonyElement
            {
                Definicja = element.Definicja.Nazwa,
                Okres = element.Okres,
                Dni = skladnik.Dni,
                Podstawa = podstawa,
                Procent = skladnik.Procent,
                KwotaDzienna = KwotaDzienna(element.Module, skladnik.Okres, podstawa, (decimal)skladnik.Wspolczynnik),
                Wartosc = element.Wartosc,
                Algorytm = element.Definicja.Algorytm.Typ == TypAlgorytmuElementu.JakChorobowe
                    ? "Jak chorobowe (standard)"
                    : "Algorytm definicji: " + element.Definicja.Algorytm.Typ
            };
        }

        /// <summary>
        /// Ten sam wzór co natywne pole "Podstawa /dzień" składnika (WypSkladnik.ZaDzień): od miesiąca
        /// z konfiguracji "Zmiana wyliczenia stawki dziennej" najpierw zaokrąglenie podstawa/30, potem procent.
        /// Współczynnik = procent świadczenia × ilość (wymiar przy zbiegu).
        /// </summary>
        private static decimal KwotaDzienna(PlaceModule place, FromTo okresSkladnika, decimal podstawa, decimal wspolczynnik)
        {
            YearMonth zmianaOd = place.Config.Chorobowy.ZmianaWyliczeniaStawkiDziennejOd;
            if (zmianaOd != YearMonth.Empty && okresSkladnika.ToYearMonth() >= zmianaOd)
                return Math.Round(Math.Round(podstawa / 30m, 2, MidpointRounding.AwayFromZero) * wspolczynnik, 2, MidpointRounding.AwayFromZero);
            return Math.Round(podstawa * wspolczynnik / 30m, 2, MidpointRounding.AwayFromZero);
        }

        private static void ChorobowyConfig(PlaceModule place, out bool wgDni, out YearMonth wgDniOd)
        {
            wgDniOd = place.Config.Chorobowy.MiesiąceWPodstawieZasiłkuWgDni;
            wgDni = wgDniOd != YearMonth.Empty;
        }

        private static void DodajMiesiac(WynikPodstawyZasilku wynik, PodstawaZasiłku.PoliczMiesiąc miesiac, bool okresowy, bool wgDni)
        {
            string info = (miesiac.Info ?? "").Trim();
            var pozycja = new MiesiacPodstawy
            {
                Miesiac = miesiac.Miesiąc,
                Okresowy = okresowy,
                KwotaPoUzupelnieniu = miesiac.Suma,
                Uwagi = info.Replace(Environment.NewLine, "; ")
            };
            bool odrzucony = info.StartsWith("Odrzucony", StringComparison.Ordinal)
                             || info.Contains("nie wpływa na podstawę");
            if (!okresowy && !odrzucony)
            {
                CzasDni norma = miesiac.NormaBO;
                CzasDni praca = miesiac.RNA;
                pozycja.NormaCzas = norma.Czas;
                pozycja.NormaDni = norma.Dni;
                pozycja.PrzepracowanyCzas = praca.Czas;
                pozycja.PrzepracowaneDni = praca.Dni;
                pozycja.CoNajmniejPolowa = wgDni ? praca.Dni + praca.Dni >= norma.Dni : praca.Czas + praca.Czas >= norma.Czas;
            }
            // Silnik pisze "Przepracował co najmniej połowę miesiąca" wyłącznie dla miesięcy wziętych do średniej.
            pozycja.Wliczony = okresowy ? miesiac.Suma != 0m : info.Contains("Przepracował co najmniej połowę");

            foreach (ElementPodstawyZasilku element in miesiac.ElementyPodstawy)
            {
                pozycja.KwotaPrzedUzupelnieniem += element.KwotaDopełniana;
                wynik.Skladniki.Add(new SkladnikPodstawy
                {
                    Miesiac = miesiac.Miesiąc,
                    Definicja = element.Element?.Definicja?.Nazwa ?? "",
                    Element = element.Opis ?? "",
                    ZaOkres = element.Element?.Okres ?? FromTo.Empty,
                    OkresNaliczania = OpisOkresuNaliczania(element.Element?.Definicja),
                    Wartosc = element.Element?.Wartosc ?? 0m,
                    SposobUjecia = SposobUjecia(element, okresowy),
                    KwotaBrutto = element.KwotaDopełniana,
                    Mnoznik = element.Mnożnik,
                    MnoznikOpis = element.MnożnikOpis ?? "",
                    KwotaWPodstawie = element.KwotaDopełniona,
                    Uwagi = pozycja.Wliczony ? "" : "miesiąc nie wliczony do średniej"
                });
            }
            wynik.Miesiace.Add(pozycja);
        }

        private static string SposobUjecia(ElementPodstawyZasilku element, bool okresowy)
        {
            if (okresowy)
                return "okresowy (" + (element.MnożnikOpis ?? "") + ")";
            switch (element.Sposób)
            {
                case ElementPodstawyZasilku.SposóbWliczenia.Nominalna:
                    return "nominalnie (kwota z umowy)";
                case ElementPodstawyZasilku.SposóbWliczenia.Dopełnione:
                    return element.Mnożnik == 1m ? "wprost (bez potrzeby uzupełnienia)" : "uzupełniony";
                default:
                    return "wprost";
            }
        }

        private static string OpisOkresuNaliczania(DefinicjaElementu definicja)
        {
            if (definicja == null)
                return "";
            OkresNaliczania okres = definicja.OkresNaliczania;
            if (okres.Typ == TypOkresuNaliczania.CoNMiesięcy && okres.Ilosc > 1)
                return "co " + okres.Ilosc + " mies.";
            return okres.Typ.ToString();
        }

        /// <summary>
        /// Elementy wypłat z miesięcy okresu podstawy, których silnik nie wziął. Powód z konfiguracji definicji;
        /// gdy definicja wchodzi do podstawy, a element i tak pominięto - wynika to z innych reguł enova.
        /// </summary>
        private static void DodajPominieteSkladniki(WynikPodstawyZasilku wynik, Pracownik pracownik,
            PodstawaZasiłku silnik, PrzyczynaNieobecnosci przyczyna)
        {
            var wziete = new HashSet<WypElement>();
            foreach (PodstawaZasiłku.PoliczMiesiąc m in silnik.Miesiące.Concat(silnik.MiesiąceOkresowe))
                foreach (ElementPodstawyZasilku e in m.ElementyPodstawy)
                    if (e.Element != null)
                        wziete.Add(e.Element);

            foreach (PodstawaZasiłku.PoliczMiesiąc m in silnik.Miesiące)
            {
                foreach (WypElement element in pracownik.Elementy[m.Miesiąc.ToFromTo()])
                {
                    if (wziete.Contains(element) || element.RozliczenieStorna || element.Wartosc == 0m)
                        continue;
                    IPodstawaZasilkow ustawienia = PodstawaZasiłku.PoliczMiesiąc.PoliczDefinicjePodstawyZasiłków(false, element);
                    wynik.Skladniki.Add(new SkladnikPodstawy
                    {
                        Miesiac = m.Miesiąc,
                        Definicja = element.Definicja?.Nazwa ?? "",
                        Element = element.Nazwa,
                        ZaOkres = element.Okres,
                        OkresNaliczania = OpisOkresuNaliczania(element.Definicja),
                        Wartosc = element.Wartosc,
                        SposobUjecia = "pominięty",
                        Uwagi = PowodPominiecia(ustawienia, przyczyna)
                    });
                }
            }
        }

        private static string PowodPominiecia(IPodstawaZasilkow ustawienia, PrzyczynaNieobecnosci przyczyna)
        {
            if (ustawienia.GetTyp == TypPodstawyZasilku.NieWliczać)
                return "definicja: nie wliczać do podstawy zasiłków";
            bool dotyczy;
            switch (przyczyna)
            {
                case PrzyczynaNieobecnosci.UrlopOpiekuńczy: dotyczy = ustawienia.GetOpiekunczych; break;
                case PrzyczynaNieobecnosci.UrlopRehabilitacyjny: dotyczy = ustawienia.GetRehabilitacyjnych; break;
                case PrzyczynaNieobecnosci.UrlopMacierzyński:
                case PrzyczynaNieobecnosci.UrlopMacierzyńskiUzupełniający:
                case PrzyczynaNieobecnosci.UrlopRodzicielski:
                case PrzyczynaNieobecnosci.UrlopOjcowski: dotyczy = ustawienia.GetMacierzynskich; break;
                default: dotyczy = ustawienia.GetChorobowych; break;
            }
            if (!dotyczy)
                return "definicja: nie wchodzi do podstawy tego świadczenia";
            return "pominięty przez enova (np. element nieoskładkowany, za okres zasiłku - art. 41, inny tytuł ubezpieczenia)";
        }

        private static void UstalStawki(WynikPodstawyZasilku wynik, PlaceModule place, WypElementNieobecność element)
        {
            decimal podstawa = wynik.PodstawaNaElemencie;
            FromTo okres = element.SkładnikGłówny.Okres;
            wynik.StawkaDzienna100 = KwotaDzienna(place, okres, podstawa, 1m);

            var chorobowy = place.Config.Chorobowy;
            void Dodaj(string opis, Percent procent)
            {
                wynik.Stawki.Add(new StawkaDzienna
                {
                    Opis = opis,
                    Procent = procent,
                    KwotaDzienna = KwotaDzienna(place, okres, podstawa, (decimal)procent)
                });
            }
            Dodaj("Wynagrodzenie chorobowe (art. 92 KP)", chorobowy.ProcentPłaciFirma);
            Dodaj("Zasiłek chorobowy", chorobowy.ProcentPłaciZUS);
            Dodaj("Leczenie szpitalne", chorobowy.ProcentSzpital);
            Dodaj("Ciąża", chorobowy.ProcentCiąży);
            Dodaj("Wypadek przy pracy / w drodze", chorobowy.ProcentWypadek);
            Dodaj("Zasiłek opiekuńczy", chorobowy.ProcentOpiekuńczego);
        }

        private static void UstalReguly(WynikPodstawyZasilku wynik, Pracownik pracownik, PodstawaZasiłku silnik,
            ParametrySymulacji parametry, WypElementNieobecność element)
        {
            if (silnik.SkładnikPodstawyZNieobecności != null)
            {
                WypElement zrodlo = silnik.SkładnikPodstawyZNieobecności.Element;
                wynik.Reguly.Add(new RegulaSpecjalna
                {
                    Regula = "Przejęcie podstawy z poprzedniego zasiłku (art. 36 ust. 3)",
                    Opis = "Podstawa z elementu " + zrodlo?.Nazwa + " za okres " + zrodlo?.Okres
                           + " - przerwa krótsza niż 3 miesiące kalendarzowe."
                });
            }
            if (parametry.WymusPonowneUstalenie)
                wynik.Reguly.Add(new RegulaSpecjalna
                {
                    Regula = "Tryb diagnostyczny: wymuszone ponowne ustalenie",
                    Opis = "Podstawa w rozbiciu liczona bez przejęcia z poprzedniej nieobecności; " +
                           "na naliczonym elemencie zostaje to, co ustaliłaby enova (flaga zwolnienia „Ponownie ustal podstawę”)."
                });

            foreach (string linia in (silnik.Info ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string tekst = linia.Trim();
                if (tekst.StartsWith("Przyrównanie do minimalnej", StringComparison.Ordinal))
                    wynik.Reguly.Add(new RegulaSpecjalna { Regula = "Podstawa minimalna (art. 45)", Opis = tekst });
                else if (tekst.StartsWith("Podstawa zasiłków ZUS z kadr", StringComparison.Ordinal)
                         || tekst.StartsWith("Podstawa chorobowego wyliczona z danych kadrowych", StringComparison.Ordinal))
                    wynik.Reguly.Add(new RegulaSpecjalna { Regula = "Podstawa z umowy - brak pełnego miesiąca (art. 37)", Opis = tekst });
                else if (tekst.StartsWith("Brak miesiąca przepracowanego w ponad połowie", StringComparison.Ordinal))
                    wynik.Reguly.Add(new RegulaSpecjalna { Regula = "Wszystkie miesiące poniżej połowy czasu (art. 38)", Opis = tekst });
                else if (tekst.StartsWith("Ograniczenie", StringComparison.Ordinal) || tekst.StartsWith("Waloryzacja", StringComparison.Ordinal))
                    wynik.Reguly.Add(new RegulaSpecjalna { Regula = "Ograniczenie / waloryzacja", Opis = tekst });
            }

            // Zmiana wymiaru etatu w okresie podstawy lub w miesiącu niezdolności (art. 40) - informacyjnie,
            // decyzję o okresie podejmuje silnik enova (patrz okres podstawy).
            YearMonth miesiacNiezdolnosci = element.Okres.ToYearMonth();
            var okres = new FromTo(miesiacNiezdolnosci.AddMonths(-12).FirstDay, miesiacNiezdolnosci.LastDay);
            var zmiany = new List<string>();
            Fraction poprzedni = Fraction.Zero;
            bool pierwszy = true;
            foreach (PracHistoria historia in pracownik.Historia.GetIntersectedRows(okres))
            {
                Fraction wymiar = historia.Etat.Wymiar;
                if (!pierwszy && wymiar != poprzedni)
                    zmiany.Add("od " + historia.Aktualnosc.From + ": " + poprzedni + " → " + wymiar);
                poprzedni = wymiar;
                pierwszy = false;
            }
            if (zmiany.Count > 0)
                wynik.Reguly.Add(new RegulaSpecjalna
                {
                    Regula = "Zmiana wymiaru etatu (art. 40)",
                    Opis = string.Join("; ", zmiany) + ". Okres podstawy wg enova: " + wynik.OkresPodstawy + "."
                });

            if (element.Definicja.Algorytm.Typ != TypAlgorytmuElementu.JakChorobowe)
                wynik.Reguly.Add(new RegulaSpecjalna
                {
                    Regula = "Własny algorytm definicji",
                    Opis = "Element " + element.Definicja.Nazwa + " nie ma standardowego algorytmu „Jak chorobowe”. " +
                           "Wiążąca jest podstawa na naliczonym elemencie; rozbicie pochodzi ze standardowego silnika."
                });
        }

        private static string RodzajSwiadczenia(DefinicjaNieobecnosci definicja)
        {
            switch (definicja.Przyczyna)
            {
                case PrzyczynaNieobecnosci.ZwolnienieChorobowe:
                    return "Wynagrodzenie chorobowe / zasiłek chorobowy (podział wg naliczenia enova)";
                case PrzyczynaNieobecnosci.UrlopOpiekuńczy:
                    return "Zasiłek opiekuńczy";
                case PrzyczynaNieobecnosci.UrlopRehabilitacyjny:
                    return "Świadczenie rehabilitacyjne";
                case PrzyczynaNieobecnosci.UrlopMacierzyński:
                case PrzyczynaNieobecnosci.UrlopMacierzyńskiUzupełniający:
                case PrzyczynaNieobecnosci.UrlopRodzicielski:
                case PrzyczynaNieobecnosci.UrlopOjcowski:
                    return "Zasiłek macierzyński";
                default:
                    return definicja.Przyczyna.ToString();
            }
        }

        private static string OpisBledu(Exception wyjatek)
        {
            Exception wewnetrzny = wyjatek;
            while (wewnetrzny.InnerException != null && wewnetrzny is System.Reflection.TargetInvocationException)
                wewnetrzny = wewnetrzny.InnerException;
            return wewnetrzny.Message;
        }
    }
}
