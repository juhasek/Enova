using System;
using System.Collections.Generic;
using System.Linq;
using Soneta.Business;
using Soneta.Business.Licence;
using Soneta.Kadry;
using Soneta.Place;
using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>
/// Naliczenie rezerwy urlopowej (lub budżetu) za miesiąc dla listy pracowników:
/// kontrole → nagłówek → standardowe naliczenie planowanej listy płac (element z klasą AlgorytmRezerwy liczy
/// rezerwę, silnik narzuty) → pozycje z wyników kalkulatora i planowanych elementów → sumy nagłówka.
/// </summary>
public class NaliczanieRezerwyUrlopowej {

    readonly Session session;
    readonly Context kontekst;
    readonly UstawieniaRezerwUrlopowych ustawienia;

    public NaliczanieRezerwyUrlopowej(Context kontekst) {
        this.kontekst = kontekst;
        session = kontekst.Session;
        ustawienia = new UstawieniaRezerwUrlopowych(session);
    }

    public List<string> Komunikaty { get; } = new List<string>();

    public RezerwaUrlopowa Nalicz(RodzajRezerwyUrlopowej rodzaj, YearMonth okres, IEnumerable<Pracownik> pracownicy, bool naliczPonownie) {
        // zmiany danych w czynności wymagają transakcji edycyjnej; błąd (wyjątek) = wycofanie całości
        using (ITransaction transakcja = session.Logout(true)) {
            RezerwaUrlopowa rezerwa = NaliczWTransakcji(rodzaj, okres, pracownicy, naliczPonownie);
            transakcja.Commit();
            return rezerwa;
        }
    }

    RezerwaUrlopowa NaliczWTransakcji(RodzajRezerwyUrlopowej rodzaj, YearMonth okres, IEnumerable<Pracownik> pracownicy, bool naliczPonownie) {
        FromTo miesiac = okres;
        Date koniecMiesiaca = miesiac.To;
        DefinicjaElementu element = ustawienia.Element(rodzaj);
        DefinicjaPlanowanejListyPłac definicjaPlanu = ustawienia.PlanowanaLista(rodzaj);

        // ---------- 1. kontrole ----------
        if (!session.Login.GetLicenceData().Available[LicencjaProgramu.PL_Platynowy])
            throw new RowException(null, "Rezerwy urlopowe wymagają licencji Płace Platynowe (planowane listy płac).");
        if (!PlanListyPlac.GetEnabled(session))
            throw new RowException(null, "Planowane listy płac są wyłączone - włącz „Rezerwy urlopowe do testów” w konfiguracji Płac (Rezerwy urlopowe).");
        List<string> bledyKonfiguracji = KontrolaKonfiguracji.Sprawdz(ustawienia, rodzaj);
        if (bledyKonfiguracji.Count > 0)
            throw new RowException(null, "Konfiguracja rezerw urlopowych jest niepoprawna:\n" + string.Join("\n", bledyKonfiguracji));
        if ((double)PlaceModule.GetInstance(session).Config.Nieobecności.ŚredniaNormaMiesięczna[koniecMiesiaca] <= 0)
            throw new RowException(null, $"Brak współczynnika do ekwiwalentu (średnia norma miesięczna) w konfiguracji na {koniecMiesiaca}.");

        RezerwaUrlopowa rezerwa = ZnajdzLubUtworzRezerwe(rodzaj, okres);
        if (rezerwa.Zamknieta)
            throw new RowException(rezerwa, $"Rezerwa {rezerwa} jest zamknięta - otwórz ją przed ponownym naliczeniem.");

        List<Pracownik> doNaliczenia = new List<Pracownik>();
        foreach (Pracownik pracownik in pracownicy) {
            PozycjaRezerwyUrlopowej istniejaca = Pozycja(rezerwa, pracownik);
            if (istniejaca != null && !naliczPonownie)
                continue;
            doNaliczenia.Add(pracownik);
        }
        if (doNaliczenia.Count == 0) {
            Komunikaty.Add("Brak pracowników do naliczenia.");
            return rezerwa;
        }

        bool sąNiezatwierdzone = NiezatwierdzoneWyplaty(doNaliczenia, miesiac, out string listaNiezatwierdzonych);
        if (sąNiezatwierdzone && ustawienia.NiezatwierdzoneListyPlac == KontrolaNiezatwierdzonychList.Blokada)
            throw new RowException(rezerwa, "Niezatwierdzone wypłaty etatowe za miesiąc rezerwy - zatwierdź listy płac przed naliczeniem rezerwy:\n" + listaNiezatwierdzonych);
        if (sąNiezatwierdzone)
            Komunikaty.Add("Uwaga: niezatwierdzone wypłaty etatowe (uwzględnione w obliczeniu): " + listaNiezatwierdzonych);

        // ---------- 2. nagłówek i usunięcie poprzedniego naliczenia ----------
        decimal wspolczynnik = PlaceModule.GetInstance(session).Config.Nieobecności.ŚredniaNormaMiesięczna[koniecMiesiaca];
        ustawienia.KopiujDo(rezerwa.Parametry, wspolczynnik);
        foreach (Pracownik pracownik in doNaliczenia) {
            Pozycja(rezerwa, pracownik)?.Delete();
            UsunNiezatwierdzonePlanowaneWyplaty(pracownik, definicjaPlanu, miesiac);
        }

        // ---------- 3. standardowe naliczenie planowanej listy płac ----------
        NaliczaniePlanowanychListPłacWorker.Params parametryPlanu = new NaliczaniePlanowanychListPłacWorker.Params(kontekst) {
            Definicja = definicjaPlanu,
            DataWypłaty = koniecMiesiaca,
            Okres = miesiac,
            Naliczanie = TypNaliczenia.PłatnaZDołu,
            TypWypłaty = TypWyplaty.Etat,
            UwzgledniajNieZatwierdzoneListyPlac = sąNiezatwierdzone,
        };
        NaliczaniePlanowanychListPłacWorker naliczaniePlanu = new NaliczaniePlanowanychListPłacWorker {
            Pracownik = doNaliczenia.ToArray(),
            Pars = parametryPlanu,
        };
        NaliczaniePlanowanychListPłac wynikPlanu = naliczaniePlanu.Nalicz();
        HashSet<PlanowanaWypłata> noweWyplaty = new HashSet<PlanowanaWypłata>(wynikPlanu.Wypłaty.Cast<PlanowanaWypłata>());

        // ---------- 4. pozycje ----------
        int pominieci = 0;
        foreach (Pracownik pracownik in doNaliczenia) {
            WynikRezerwyUrlopowej wynik = RejestrWynikowRezerwy.Pobierz(pracownik.Guid, okres.Year, okres.Month, rodzaj);
            if (wynik == null || wynik.Pominiety) {
                pominieci++;
                if (wynik != null && wynik.PowodPominiecia.Length > 0)
                    Komunikaty.Add($"{pracownik}: {wynik.PowodPominiecia}");
                continue;
            }
            PlanowanyElementWypłaty elementPlanu = PlanowanyElement(pracownik, element, noweWyplaty);
            PozycjaRezerwyUrlopowej pozycja = new PozycjaRezerwyUrlopowej(rezerwa);
            RezerwyModule.GetInstance(session).PozRezerwUrlop.AddRow(pozycja);
            PrzepiszWynik(pozycja, pracownik, wynik, elementPlanu);
        }

        // ---------- 5. sumy nagłówka ----------
        PrzeliczSumy(rezerwa);
        rezerwa.DataNaliczenia = DateTime.Now;
        rezerwa.NaliczylOperator = session.Login.Operator?.ToString() ?? "";
        Komunikaty.Insert(0, $"Naliczono {doNaliczenia.Count - pominieci} pozycji rezerwy {rezerwa} (pominięto: {pominieci}).");
        return rezerwa;
    }

    RezerwaUrlopowa ZnajdzLubUtworzRezerwe(RodzajRezerwyUrlopowej rodzaj, YearMonth okres) {
        RezerwyModule modul = RezerwyModule.GetInstance(session);
        foreach (RezerwaUrlopowa istniejaca in modul.RezerwyUrlopowe)
            if (istniejaca.Rodzaj == rodzaj && istniejaca.Rok == okres.Year && istniejaca.Miesiac == okres.Month)
                return istniejaca;
        RezerwaUrlopowa nowa = new RezerwaUrlopowa();
        modul.RezerwyUrlopowe.AddRow(nowa);
        nowa.Rodzaj = rodzaj;
        nowa.Rok = okres.Year;
        nowa.Miesiac = okres.Month;
        nowa.StanRezerwy = StanRezerwyUrlopowej.Naliczona;
        return nowa;
    }

    static PozycjaRezerwyUrlopowej Pozycja(RezerwaUrlopowa rezerwa, Pracownik pracownik) {
        foreach (PozycjaRezerwyUrlopowej pozycja in rezerwa.Pozycje)
            if (pozycja.Pracownik == pracownik)
                return pozycja;
        return null;
    }

    bool NiezatwierdzoneWyplaty(IEnumerable<Pracownik> pracownicy, FromTo miesiac, out string lista) {
        PlaceModule place = PlaceModule.GetInstance(session);
        List<string> nazwiska = new List<string>();
        foreach (Pracownik pracownik in pracownicy) {
            foreach (WypElement elementWyplaty in place.WypElementy.WgPracownik[pracownik]) {
                if (elementWyplaty.RodzajZrodla != RodzajŹródłaWypłaty.Etat || !miesiac.Contains(elementWyplaty.Okres.To))
                    continue;
                if (elementWyplaty.Wyplata != null && !elementWyplaty.Wyplata.Zatwierdzona) {
                    nazwiska.Add(pracownik.ToString());
                    break;
                }
            }
        }
        lista = string.Join(", ", nazwiska.Take(20)) + (nazwiska.Count > 20 ? $" i {nazwiska.Count - 20} innych" : "");
        return nazwiska.Count > 0;
    }

    // Silnik planu przy ponownym naliczeniu dopisałby drugą wypłatę do niezatwierdzonej planowanej listy - usuwamy poprzednią.
    void UsunNiezatwierdzonePlanowaneWyplaty(Pracownik pracownik, DefinicjaPlanowanejListyPłac definicjaPlanu, FromTo miesiac) {
        List<PlanowanaWypłata> doUsuniecia = new List<PlanowanaWypłata>();
        foreach (PlanowanaWypłata wyplata in PlaceModule.GetInstance(session).PlanowaneWyplaty.WgPracownik[pracownik]) {
            PlanowanaListaPłac lista = wyplata.ListaPlac;
            if (lista != null && lista.Definicja == definicjaPlanu && lista.Okres == miesiac && !lista.Zatwierdzona)
                doUsuniecia.Add(wyplata);
        }
        foreach (PlanowanaWypłata wyplata in doUsuniecia)
            wyplata.Delete();
    }

    PlanowanyElementWypłaty PlanowanyElement(Pracownik pracownik, DefinicjaElementu element, HashSet<PlanowanaWypłata> noweWyplaty) {
        foreach (PlanowanyElementWypłaty elementPlanu in PlaceModule.GetInstance(session).PlanElementyWyp.WgPracownik[pracownik])
            if (elementPlanu.Definicja == element && noweWyplaty.Contains(elementPlanu.Wyplata))
                return elementPlanu;
        return null;
    }

    void PrzepiszWynik(PozycjaRezerwyUrlopowej pozycja, Pracownik pracownik, WynikRezerwyUrlopowej wynik, PlanowanyElementWypłaty elementPlanu) {
        pozycja.Pracownik = pracownik;
        pozycja.Wydzial = wynik.Wydzial;
        pozycja.CentrumKosztow = wynik.CentrumKosztow;
        pozycja.WymiarEtatu = wynik.WymiarEtatu;
        pozycja.GodzinNaDzien = wynik.GodzinNaDzien;
        pozycja.ZaleglyGodz = Math.Round(wynik.ZaleglyGodz, 2);
        pozycja.BiezacyGodz = Math.Round(wynik.BiezacyGodz, 2);
        pozycja.WykorzystanyGodz = Math.Round(wynik.WykorzystanyGodz, 2);
        pozycja.GodzinyRezerwy = wynik.GodzinyRezerwy;
        pozycja.Podstawa1 = new Currency((decimal)wynik.Podstawa1);
        pozycja.MiesiecyPodstawy1 = wynik.MiesiecyPodstawy1;
        pozycja.Podstawa2 = new Currency((decimal)wynik.Podstawa2);
        pozycja.StawkaGodzinowa = Math.Round(wynik.StawkaGodzinowa, 4);
        pozycja.Kwota = new Currency(wynik.Kwota);
        pozycja.Ostrzezenia = wynik.Ostrzezenia.Length > 200 ? wynik.Ostrzezenia.Substring(0, 200) : wynik.Ostrzezenia;
        if (elementPlanu == null) {
            pozycja.Ostrzezenia = (pozycja.Ostrzezenia + " brak elementu na planowanej liście (narzuty 0)").Trim();
            return;
        }
        decimal narzuty = elementPlanu.Narzuty;
        if (ustawienia.PpkWNarzutach)
            narzuty += elementPlanu.Podatki.PPK.PodstPracodawcy + elementPlanu.Podatki.PPK.DodPracodawcy;
        pozycja.Narzuty = new Currency(narzuty);
        pozycja.PlanowanyElement = elementPlanu;
        pozycja.ZapisObliczen = elementPlanu.ZapisObliczen.ToString();
    }

    static void PrzeliczSumy(RezerwaUrlopowa rezerwa) {
        int liczba = 0;
        double godziny = 0;
        decimal kwota = 0, narzuty = 0;
        foreach (PozycjaRezerwyUrlopowej pozycja in rezerwa.Pozycje) {
            liczba++;
            godziny += pozycja.GodzinyRezerwy;
            kwota += pozycja.Kwota.Value;
            narzuty += pozycja.Narzuty.Value;
        }
        rezerwa.LiczbaPozycji = liczba;
        rezerwa.GodzinyRazem = Math.Round(godziny, 2);
        rezerwa.KwotaRazem = new Currency(kwota);
        rezerwa.NarzutyRazem = new Currency(narzuty);
    }
}
