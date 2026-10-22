using System;
using System.Collections.Generic;
using Soneta.Business;
using Soneta.Core;
using Soneta.Kadry;
using Soneta.Kalend;
using Soneta.Place;
using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>
/// Obliczenie rezerwy urlopowej (i budżetu) jednego pracownika za miesiąc - jedno źródło obliczeń dodatku.
/// Przeniesione z rozwiązania skryptowego (Rezerwy urlopowe/Rezerwa Al/Rezerwa urlopowa), stałe zastąpione
/// konfiguracją (UstawieniaRezerwUrlopowych). Wywoływane z algorytmu elementu wynagrodzenia na planowanej liście płac
/// (podstawa 2 wymaga elementu wypłaty - NaliczanieEkwiwalent).
/// Zapis obliczeń: Log("Rezerwa urlopowa") - silnik zbiera go do Zapisu obliczeń elementu.
/// </summary>
public class KalkulatorRezerwyUrlopowej {

    public const string KategoriaLogu = "Rezerwa urlopowa";

    readonly UstawieniaRezerwUrlopowych ustawienia;
    readonly Log log = new Log(KategoriaLogu);

    public KalkulatorRezerwyUrlopowej(UstawieniaRezerwUrlopowych ustawienia) {
        this.ustawienia = ustawienia;
    }

    /// <summary>
    /// Oblicza rezerwę dla pracownika elementu za miesiąc okresu elementu.
    /// <paramref name="element"/> i <paramref name="skladnik"/> - element wynagrodzenia na planowanej liście płac (podstawa 2).
    /// </summary>
    public WynikRezerwyUrlopowej Oblicz(WypElement element, WypSkladnik skladnik, RodzajRezerwyUrlopowej rodzaj) {
        WynikRezerwyUrlopowej wynik = new WynikRezerwyUrlopowej();
        FromTo miesiacRezerwy = new YearMonth(element.Okres.To);
        Date koniecMiesiaca = miesiacRezerwy.To;
        Pracownik pracownik = element.Pracownik;
        PracHistoria historiaPracownika = pracownik[koniecMiesiaca];
        bool trybBudzetu = rodzaj == RodzajRezerwyUrlopowej.Budzet;

        log.WriteLine($"=== {(trybBudzetu ? "BUDŻET REZERWY URLOPOWEJ" : "REZERWA URLOPOWA")} {koniecMiesiaca.Year}/{koniecMiesiaca.Month:00} - {pracownik} ===");

        // Rezerwa tylko dla zatrudnionych na ostatni dzień miesiąca (zwolnionym urlop rozlicza się ekwiwalentem).
        if (historiaPracownika == null || !historiaPracownika.Etat.OkresZatrudnienia.Contains(koniecMiesiaca)) {
            wynik.Pominiety = true;
            wynik.PowodPominiecia = $"Pracownik niezatrudniony na {koniecMiesiaca}";
            log.WriteLine(wynik.PowodPominiecia + " - pominięty.");
            return wynik;
        }

        PlaceModule place = PlaceModule.GetInstance(pracownik.Session);
        wynik.Wydzial = historiaPracownika.Etat.Wydzial;
        wynik.CentrumKosztow = CentrumKosztowWydzialu(wynik.Wydzial);
        wynik.WymiarEtatu = (double)historiaPracownika.Etat.Wymiar;
        if (wynik.WymiarEtatu <= 0)
            wynik.WymiarEtatu = 1;

        // Godzin w dniu urlopu - jak w standardowym ekwiwalencie enova (8 h albo norma dobowa kalendarza)
        double normaDobowaGodzin = 8.0;
        if (place.Config.Nieobecności.EkwiwalentZaUrlop.PrzliczenieDniNaGodzWgKalendarza && historiaPracownika.Etat.Kalendarz != null)
            normaDobowaGodzin = historiaPracownika.Etat.Kalendarz.NormaDobowa.TotalHours;
        if (normaDobowaGodzin <= 0)
            normaDobowaGodzin = 8.0;
        wynik.GodzinNaDzien = normaDobowaGodzin * wynik.WymiarEtatu;

        ElementyPodstawy elementyPodstawy = ZbierzElementyPodstawy(place, pracownik, koniecMiesiaca, element.Definicja);
        if (!elementyPodstawy.JestWyplata[0]) {
            string uwaga = $"brak wypłaty za {koniecMiesiaca.Year}/{koniecMiesiaca.Month:00} (lista niezatwierdzona lub nienaliczona)";
            wynik.DodajOstrzezenie(uwaga);
            log.WriteLine($"UWAGA: {uwaga} - podstawa 1 pominie ten miesiąc, standard (podstawa 2) naliczy go na nowo.");
        }

        if (trybBudzetu)
            StanUrlopuBudzet(wynik, pracownik, koniecMiesiaca.Year + 1);
        else
            StanUrlopuNaKoniecMiesiaca(wynik, pracownik, koniecMiesiaca);

        // Współczynnik do ekwiwalentu - zawsze z konfiguracji enova; brak = rezerwa 0.
        wynik.WspolczynnikEkwiwalentu = (double)place.Config.Nieobecności.ŚredniaNormaMiesięczna[koniecMiesiaca];
        double wspolczynnikWgWymiaru = wynik.WspolczynnikEkwiwalentu * wynik.WymiarEtatu;
        log.WriteLine($"[2] WSPÓŁCZYNNIK: {wynik.WspolczynnikEkwiwalentu:0.00} x wymiar {historiaPracownika.Etat.Wymiar} = {wspolczynnikWgWymiaru:0.00}; {normaDobowaGodzin:0.00} h w dniu urlopu");
        if (wynik.WspolczynnikEkwiwalentu <= 0) {
            wynik.Pominiety = true;
            wynik.PowodPominiecia = $"Brak współczynnika do ekwiwalentu w konfiguracji na {koniecMiesiaca}";
            log.WriteLine($"UWAGA: {wynik.PowodPominiecia} - rezerwa = 0.");
            return wynik;
        }

        Podstawa1(wynik, pracownik, elementyPodstawy, koniecMiesiaca, wspolczynnikWgWymiaru, normaDobowaGodzin);
        Podstawa2(wynik, element, skladnik, wspolczynnikWgWymiaru, elementyPodstawy.JestWyplata[0], koniecMiesiaca);

        // ---------------- 5. WYNIK ----------------
        string opisWariantu;
        switch (ustawienia.WariantPodstawy) {
            case WariantPodstawyRezerwy.Podstawa2:
                wynik.StawkaGodzinowa = wynik.Podstawa2ZaGodzine;
                opisWariantu = "podstawa 2";
                break;
            case WariantPodstawyRezerwy.Wyzsza:
                wynik.StawkaGodzinowa = Math.Max(wynik.Podstawa1ZaGodzine, wynik.Podstawa2ZaGodzine);
                opisWariantu = "wyższa z podstaw";
                break;
            default:
                wynik.StawkaGodzinowa = wynik.Podstawa1ZaGodzine;
                opisWariantu = "podstawa 1";
                break;
        }
        wynik.Kwota = (decimal)Math.Round(wynik.GodzinyRezerwy * wynik.StawkaGodzinowa, 2);
        log.WriteLine($"[5] WYNIK: {opisWariantu} -> {wynik.GodzinyRezerwy:0.00} h x {wynik.StawkaGodzinowa:0.0000} = {wynik.Kwota:0.00} zł");
        return wynik;
    }

    // ---------------- 1. STAN URLOPU ----------------

    IEnumerable<Guid> DefinicjeLimitow() {
        yield return DefinicjaLimitu.UrlopWypoczynkowy;
        if (ustawienia.UwzgledniajUrlopDodatkowy)
            yield return DefinicjaLimitu.UrlopDodatkowy;
    }

    void StanUrlopuNaKoniecMiesiaca(WynikRezerwyUrlopowej wynik, Pracownik pracownik, Date koniecMiesiaca) {
        log.WriteLine("[1] STAN URLOPU");
        using (log.IncrementIndent()) {
            KalendModule kalend = KalendModule.GetInstance(pracownik.Session);
            Date poczatekRoku = new Date(koniecMiesiaca.Year, 1, 1);
            Date koniecRoku = new Date(koniecMiesiaca.Year, 12, 31);
            foreach (Guid guidDefinicji in DefinicjeLimitow()) {
                DefinicjaLimitu definicjaLimitu = kalend.DefinicjeLimitow[guidDefinicji];
                if (definicjaLimitu == null)
                    continue;
                LimitNieobecnosci pierwszyLimitRoku = null;
                double biezacyKolejnyUrlopGodzin = 0, biezacyPierwszyUrlopGodzin = 0;
                foreach (LimitNieobecnosci limit in kalend.LimNieobecnosci.WgPracownik[pracownik, definicjaLimitu]) {
                    if (limit.Okres.To < poczatekRoku || limit.Okres.From > koniecRoku)
                        continue;
                    if (pierwszyLimitRoku == null || limit.Okres.From < pierwszyLimitRoku.Okres.From)
                        pierwszyLimitRoku = limit;
                    // należny proporcjonalnie: miesiące okresu limitu do końca miesiąca rezerwy / miesiące okresu limitu
                    Date poczatekLimitu = limit.Okres.From < poczatekRoku ? poczatekRoku : limit.Okres.From;
                    Date koniecLimitu = limit.Okres.To > koniecRoku ? koniecRoku : limit.Okres.To;
                    Date koniecLimituDoRezerwy = koniecLimitu > koniecMiesiaca ? koniecMiesiaca : koniecLimitu;
                    int miesiecyLimitu = NumerMiesiaca(koniecLimitu) - NumerMiesiaca(poczatekLimitu) + 1;
                    int miesiecyDoRezerwy = NumerMiesiaca(koniecLimituDoRezerwy) - NumerMiesiaca(poczatekLimitu) + 1;
                    if (miesiecyLimitu <= 0 || miesiecyDoRezerwy <= 0)
                        continue;
                    double naleznyGodzin = (limit.LimitGodz + limit.ZmianaGodz - limit.WykorzystanyPoprzGodz).TotalHours;
                    if (limit.PierwszyUrlop)
                        biezacyPierwszyUrlopGodzin += naleznyGodzin * miesiecyDoRezerwy / miesiecyLimitu;
                    else
                        biezacyKolejnyUrlopGodzin += naleznyGodzin * miesiecyDoRezerwy / miesiecyLimitu;
                    string rodzajUrlopu = limit.PierwszyUrlop ? "pierwszy urlop" : "kolejny urlop";
                    log.WriteLine($"Limit {definicjaLimitu.Nazwa} {limit.Okres} ({rodzajUrlopu}): należny {naleznyGodzin:0.00} h x {miesiecyDoRezerwy}/{miesiecyLimitu} mies.");
                }
                // kolejny urlop: część proporcjonalna w górę do pełnego dnia; pierwszy urlop - bez zaokrąglenia
                if (ustawienia.ZaokraglajKolejnyUrlop && biezacyKolejnyUrlopGodzin > 0) {
                    double dniPoZaokragleniu = Math.Ceiling(Math.Round(biezacyKolejnyUrlopGodzin / wynik.GodzinNaDzien, 6));
                    log.WriteLine($"Kolejny urlop: {biezacyKolejnyUrlopGodzin / wynik.GodzinNaDzien:0.00} dni -> zaokrąglone w górę {dniPoZaokragleniu:0} dni = {dniPoZaokragleniu * wynik.GodzinNaDzien:0.00} h");
                    biezacyKolejnyUrlopGodzin = dniPoZaokragleniu * wynik.GodzinNaDzien;
                }
                wynik.BiezacyGodz += biezacyKolejnyUrlopGodzin + biezacyPierwszyUrlopGodzin;
                if (pierwszyLimitRoku != null)
                    wynik.ZaleglyGodz += pierwszyLimitRoku.PrzeniesienieGodz.TotalHours;
                // wykorzystany od 01.01 do końca miesiąca rezerwy (nieobecności pomniejszające ten limit)
                foreach (OkresNieobecności nieobecnosc in pracownik.Czasy.Nieobecnosci(new FromTo(poczatekRoku, koniecMiesiaca), true)) {
                    if (nieobecnosc.Definicja != null && nieobecnosc.Definicja.Limit == definicjaLimitu)
                        wynik.WykorzystanyGodz += nieobecnosc.Norma().Czas.TotalHours;
                }
            }
            ZamknijStanUrlopu(wynik, "Zaległy", "Bieżący proporcjonalny", $"Wykorzystany do {koniecMiesiaca}");
        }
    }

    void StanUrlopuBudzet(WynikRezerwyUrlopowej wynik, Pracownik pracownik, int rokBudzetu) {
        log.WriteLine("[1] STAN URLOPU (symulacja limitu na rok następny)");
        using (log.IncrementIndent()) {
            (double zaleglyGodz, double naleznyGodz) = SymulatorLimituUrlopu.Symuluj(pracownik, rokBudzetu, DefinicjeLimitow(), log);
            wynik.ZaleglyGodz = zaleglyGodz;
            wynik.BiezacyGodz = ustawienia.BudzetZPelnymLimitem ? naleznyGodz : 0;
            ZamknijStanUrlopu(wynik, $"Zaległy na 01.01.{rokBudzetu}", $"Limit na rok {rokBudzetu}", "Wykorzystany");
        }
    }

    void ZamknijStanUrlopu(WynikRezerwyUrlopowej wynik, string opisZaleglego, string opisBiezacego, string opisWykorzystanego) {
        double stanUrlopuGodzin = wynik.ZaleglyGodz + wynik.BiezacyGodz - wynik.WykorzystanyGodz;
        log.WriteLine($"{"",-32}{"godz.",10}{"dni",10}");
        log.WriteLine($"{opisZaleglego,-32}{wynik.ZaleglyGodz,10:0.00}{wynik.ZaleglyGodz / wynik.GodzinNaDzien,10:0.00}");
        log.WriteLine($"{opisBiezacego,-32}{wynik.BiezacyGodz,10:0.00}{wynik.BiezacyGodz / wynik.GodzinNaDzien,10:0.00}");
        log.WriteLine($"{opisWykorzystanego,-32}{wynik.WykorzystanyGodz,10:0.00}{wynik.WykorzystanyGodz / wynik.GodzinNaDzien,10:0.00}");
        log.WriteLine($"{"Do rezerwy",-32}{stanUrlopuGodzin,10:0.00}{stanUrlopuGodzin / wynik.GodzinNaDzien,10:0.00}");
        if (stanUrlopuGodzin < 0 && !ustawienia.DopuszczajUjemna) {
            log.WriteLine("Ujemny stan urlopu - rezerwa = 0.");
            stanUrlopuGodzin = 0;
        }
        wynik.GodzinyRezerwy = Math.Round(stanUrlopuGodzin, 2);
    }

    // ---------------- 3. PODSTAWA 1 ----------------

    /// <summary>Elementy wypłat pracownika z miesięcy podstawy (jeden przebieg po WypElementy). Indeks 0 = miesiąc rezerwy.</summary>
    class ElementyPodstawy {
        public Date[] PoczatekMiesiaca;
        public Date[] KoniecMiesiaca;
        public bool[] JestWyplata;
        public double[] ZasadniczeNaliczone;
        public List<WypElement>[] SkladnikiZmienne;
    }

    ElementyPodstawy ZbierzElementyPodstawy(PlaceModule place, Pracownik pracownik, Date koniecMiesiacaRezerwy, DefinicjaElementu definicjaRezerwy) {
        int liczbaMiesiecy = Math.Max(1, Math.Min(12, ustawienia.MiesiecyPodstawy));
        ElementyPodstawy elementy = new ElementyPodstawy {
            PoczatekMiesiaca = new Date[liczbaMiesiecy],
            KoniecMiesiaca = new Date[liczbaMiesiecy],
            JestWyplata = new bool[liczbaMiesiecy],
            ZasadniczeNaliczone = new double[liczbaMiesiecy],
            SkladnikiZmienne = new List<WypElement>[liczbaMiesiecy],
        };
        for (int numerMiesiaca = 0; numerMiesiaca < liczbaMiesiecy; numerMiesiaca++) {
            int miesiacLiczony = NumerMiesiaca(koniecMiesiacaRezerwy) - numerMiesiaca;
            elementy.PoczatekMiesiaca[numerMiesiaca] = new Date(miesiacLiczony / 12, miesiacLiczony % 12 + 1, 1);
            FromTo okresMiesiaca = new YearMonth(elementy.PoczatekMiesiaca[numerMiesiaca]);
            elementy.KoniecMiesiaca[numerMiesiaca] = okresMiesiaca.To;
            elementy.SkladnikiZmienne[numerMiesiaca] = new List<WypElement>();
        }
        Date poczatekPodstawy = elementy.PoczatekMiesiaca[liczbaMiesiecy - 1];
        foreach (WypElement elementWyplaty in place.WypElementy.WgPracownik[pracownik]) {
            if (elementWyplaty.Definicja == null || elementWyplaty.Definicja == definicjaRezerwy)
                continue;
            Date koniecOkresuElementu = elementWyplaty.Okres.To;
            if (koniecOkresuElementu < poczatekPodstawy || koniecOkresuElementu > koniecMiesiacaRezerwy)
                continue;
            int numerMiesiaca = NumerMiesiaca(koniecMiesiacaRezerwy) - NumerMiesiaca(koniecOkresuElementu);
            if (elementWyplaty.RodzajZrodla == RodzajŹródłaWypłaty.Etat) {
                elementy.JestWyplata[numerMiesiaca] = true;
                elementy.ZasadniczeNaliczone[numerMiesiaca] += (double)elementWyplaty.Wartosc;
            }
            else if (elementWyplaty.Definicja.Nieobecnosci.Ekwiwalent.Typ != TypPodstawyUrlopu.NieWliczać)
                elementy.SkladnikiZmienne[numerMiesiaca].Add(elementWyplaty);
        }
        return elementy;
    }

    void Podstawa1(WynikRezerwyUrlopowej wynik, Pracownik pracownik, ElementyPodstawy elementy, Date koniecMiesiacaRezerwy,
                   double wspolczynnikWgWymiaru, double normaDobowaGodzin) {
        log.WriteLine($"[3] PODSTAWA 1 (średnia z {elementy.PoczatekMiesiaca.Length} mies.)");
        double sumaPodstawy1 = 0;
        using (log.IncrementIndent()) {
            for (int numerMiesiaca = 0; numerMiesiaca < elementy.PoczatekMiesiaca.Length; numerMiesiaca++) {
                Date poczatekMiesiaca = elementy.PoczatekMiesiaca[numerMiesiaca];
                Date koniecMiesiaca = elementy.KoniecMiesiaca[numerMiesiaca];
                string opisMiesiaca = $"{poczatekMiesiaca.Year}/{poczatekMiesiaca.Month:00}";
                FromTo okresMiesiaca = new FromTo(poczatekMiesiaca, koniecMiesiaca);
                PracHistoria historiaWMiesiacu = pracownik[koniecMiesiaca];
                if (historiaWMiesiacu == null || !historiaWMiesiacu.Etat.OkresZatrudnienia.Contains(koniecMiesiaca)) {
                    log.WriteLine($"{opisMiesiaca}   brak zatrudnienia - pominięty");
                    continue;
                }
                if (!elementy.JestWyplata[numerMiesiaca] && ustawienia.PomijajMiesiaceBezWyplaty) {
                    log.WriteLine($"{opisMiesiaca}   brak wypłaty - pominięty");
                    continue;
                }
                // miesiąc niepełnego zatrudnienia: zasadnicze i zmienne w wartości nominalnej (za pełny miesiąc)
                FromTo zatrudnienieWMiesiacu = historiaWMiesiacu.Etat.OkresZatrudnienia * okresMiesiaca;
                bool niepelnyMiesiac = zatrudnienieWMiesiacu != okresMiesiaca;
                double przelicznikNormy = 1;
                if (niepelnyMiesiac) {
                    double normaMiesiaca = pracownik.Czasy.Norma(okresMiesiaca).Czas.TotalHours;
                    double normaWOkresieZatrudnienia = pracownik.Czasy.Norma(zatrudnienieWMiesiacu).Czas.TotalHours;
                    if (normaWOkresieZatrudnienia > 0)
                        przelicznikNormy = normaMiesiaca / normaWOkresieZatrudnienia;
                }
                double zasadnicze = ustawienia.ZasadniczeNominalne
                    ? AlgorytmyPłacowe.ZasadniczeNominalne(pracownik, koniecMiesiaca).Value
                    : elementy.ZasadniczeNaliczone[numerMiesiaca] * przelicznikNormy;
                double zmienne = 0;
                foreach (WypElement skladnikZmienny in elementy.SkladnikiZmienne[numerMiesiaca]) {
                    double wartoscSkladnika = (double)skladnikZmienny.Wartosc;
                    if (niepelnyMiesiac) {
                        // dodatek z kwotą w kartotece - kwota nominalna; pozostałe - przeliczenie do pełnego miesiąca wg normy
                        if (skladnikZmienny is WypElementDodatek elementDodatku && elementDodatku.DodHistoria != null && elementDodatku.DodHistoria.Podstawa.Value != 0)
                            wartoscSkladnika = (double)elementDodatku.DodHistoria.Podstawa.Value;
                        else
                            wartoscSkladnika *= przelicznikNormy;
                    }
                    zmienne += wartoscSkladnika;
                }
                wynik.MiesiecyPodstawy1++;
                sumaPodstawy1 += zasadnicze + zmienne;
                string uwagaNiepelnyMiesiac = niepelnyMiesiac ? $"   (niepełne zatrudnienie {zatrudnienieWMiesiacu}, wartości nominalne, przelicznik normy {przelicznikNormy:0.0000})" : "";
                log.WriteLine($"{opisMiesiaca}   zasadnicze {zasadnicze,10:0.00}   zmienne {zmienne,10:0.00}   razem {zasadnicze + zmienne,10:0.00}{uwagaNiepelnyMiesiac}");
            }
            if (wynik.MiesiecyPodstawy1 > 0)
                wynik.Podstawa1 = Math.Round(sumaPodstawy1 / wynik.MiesiecyPodstawy1, 2);
            else {
                // brak wypłat w miesiącach podstawy (np. przed pierwszą listą płac) - zasadnicze nominalne z miesiąca rezerwy
                wynik.Podstawa1 = Math.Round(AlgorytmyPłacowe.ZasadniczeNominalne(pracownik, koniecMiesiacaRezerwy).Value, 2);
                log.WriteLine("Brak wypłat w miesiącach podstawy - przyjęto zasadnicze nominalne z miesiąca rezerwy.");
            }
            wynik.Podstawa1ZaGodzine = wynik.Podstawa1 / wspolczynnikWgWymiaru / normaDobowaGodzin;
            log.WriteLine($"Średnia z {wynik.MiesiecyPodstawy1} mies.: {wynik.Podstawa1:0.00} -> za dzień {wynik.Podstawa1 / wspolczynnikWgWymiaru:0.00} -> za godz. {wynik.Podstawa1ZaGodzine:0.0000}");
        }
    }

    // ---------------- 4. PODSTAWA 2 ----------------

    void Podstawa2(WynikRezerwyUrlopowej wynik, WypElement element, WypSkladnik skladnik, double wspolczynnikWgWymiaru,
                   bool jestWyplataWMiesiacuRezerwy, Date koniecMiesiaca) {
        // Pełny log standardu trafia do Zapisu obliczeń tylko po dopisaniu kategorii "Urlop" w definicji elementu.
        log.WriteLine("[4] PODSTAWA 2 (standard enova - ekwiwalent za urlop)");
        using (log.IncrementIndent()) {
            try {
                using (element.Session.Logout(true)) {
                    new NaliczanieEkwiwalent(element, skladnik).NaliczPodstawy();
                    // odczyt WEWNĄTRZ bloku Logout - po jego zamknięciu zmiany składnika są cofane
                    wynik.Podstawa2ZaDzien = skladnik.Podstawa1.Value;    // NaliczanieEkwiwalent: Podstawa1 = za 1 dzień
                    wynik.Podstawa2ZaGodzine = skladnik.Podstawa2.Value;  //                       Podstawa2 = za 1 godzinę
                }
            }
            catch (Exception blad) {
                wynik.DodajOstrzezenie("błąd podstawy 2: " + blad.Message);
                log.WriteLine($"Błąd naliczania wg standardu: {blad.Message}");
            }
            if (!jestWyplataWMiesiacuRezerwy)
                log.WriteLine($"Brak wypłaty za {koniecMiesiaca.Year}/{koniecMiesiaca.Month:00} - standard naliczył elementy miesiąca na nowo.");
            wynik.Podstawa2 = Math.Round(wynik.Podstawa2ZaDzien * wspolczynnikWgWymiaru, 2);
            log.WriteLine($"Podstawa {wynik.Podstawa2:0.00} -> za dzień {wynik.Podstawa2ZaDzien:0.00} -> za godz. {wynik.Podstawa2ZaGodzine:0.0000}");
        }
    }

    // ---------------- pomocnicze ----------------

    /// <summary>Kolejny numer miesiąca (rok × 12 + miesiąc - 1) - do liczenia różnic miesięcy.</summary>
    static int NumerMiesiaca(Date data) => data.Year * 12 + data.Month - 1;

    /// <summary>MPK z wydziału, a gdy wydział go nie ma - z najbliższego wydziału nadrzędnego.</summary>
    public static CentrumKosztow CentrumKosztowWydzialu(Wydzial wydzial) {
        while (wydzial != null && wydzial.CentrumKosztow == null)
            wydzial = wydzial.Nadrzedny;
        return wydzial?.CentrumKosztow;
    }
}
