using System;
using Soneta.Kadry;
using Soneta.Place;
using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>
/// Algorytm elementów wynagrodzenia „Rezerwa urlopowa (dodatek)” i „Budżet rezerwy urlopowej (dodatek)”.
/// Definicja elementu: Algorytm = Klasa algorytmu, nazwa klasy = A1.Rezerwy.AlgorytmRezerwy (pole ma limit 30 znaków)
/// (enova generuje: new A1.Rezerwy.AlgorytmRezerwy(Pracownik).Podstawa / .Wartosc).
///
/// Element liczy się tylko na planowanej liście płac (Rodzaj naliczania = Tylko planowane; dodatkowo kontrola
/// PlanowaneWynagrodzenie). Pola składnika jak w rozwiązaniu skryptowym:
///   Podstawa1/2 = podstawy miesięczne, Podstawa3/4/5 = urlop zaległy / bieżący / wykorzystany (dni),
///   Czas = godziny rezerwy, Ilosc = stawka za 1 godzinę.
/// </summary>
public class AlgorytmRezerwy : AlgorytmBase {

    public AlgorytmRezerwy(Pracownik pracownik) : base(pracownik) { }

    public override void Podstawa(WypElement Element, WypSkladnik Składnik) {
        Składnik.Podstawa1 = DoubleCy.Zero;
        Składnik.Podstawa2 = DoubleCy.Zero;
        Składnik.Podstawa3 = DoubleCy.Zero;
        Składnik.Podstawa4 = DoubleCy.Zero;
        Składnik.Podstawa5 = DoubleCy.Zero;
        Składnik.Czas = Time.Zero;
        Składnik.Ilosc = 0;
        if (!PlanowaneWynagrodzenie)
            return;

        UstawieniaRezerwUrlopowych ustawienia = new UstawieniaRezerwUrlopowych(Element.Session);
        RodzajRezerwyUrlopowej rodzaj = ustawienia.ElementBudzetu != null && Element.Definicja.Guid == ustawienia.ElementBudzetu.Guid
            ? RodzajRezerwyUrlopowej.Budzet
            : RodzajRezerwyUrlopowej.Rezerwa;

        WynikRezerwyUrlopowej wynik = new KalkulatorRezerwyUrlopowej(ustawienia).Oblicz(Element, Składnik, rodzaj);
        Date koniecMiesiaca = ((FromTo)new YearMonth(Element.Okres.To)).To;
        RejestrWynikowRezerwy.Zapisz(Pracownik.Guid, koniecMiesiaca.Year, koniecMiesiaca.Month, rodzaj, wynik);
        if (wynik.Pominiety)
            return;

        int minutRezerwy = (int)Math.Round(wynik.GodzinyRezerwy * 60);
        Składnik.Podstawa1 = new DoubleCy(wynik.Podstawa1);
        Składnik.Podstawa2 = new DoubleCy(wynik.Podstawa2);
        Składnik.Podstawa3 = new DoubleCy(Math.Round(wynik.ZaleglyGodz / wynik.GodzinNaDzien, 2));
        Składnik.Podstawa4 = new DoubleCy(Math.Round(wynik.BiezacyGodz / wynik.GodzinNaDzien, 2));
        Składnik.Podstawa5 = new DoubleCy(Math.Round(wynik.WykorzystanyGodz / wynik.GodzinNaDzien, 2));
        Składnik.Czas = minutRezerwy >= 0 ? new Time(minutRezerwy / 60, minutRezerwy % 60) : -new Time(-minutRezerwy / 60, -minutRezerwy % 60);
        Składnik.Ilosc = wynik.StawkaGodzinowa;
    }

    public override Currency Wartosc(WypElement Element, WypSkladnik Składnik) {
        if (!PlanowaneWynagrodzenie)
            return Currency.Zero;
        // kwota = godziny rezerwy x stawka za 1 godzinę (wyliczona w Podstawa)
        return new Currency((decimal)Math.Round(Składnik.Czas.TotalHours * Składnik.Ilosc, 2));
    }
}
