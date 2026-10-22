using Soneta.Core;
using Soneta.Kadry;

namespace A1.Rezerwy;

/// <summary>Wynik obliczenia rezerwy urlopowej jednego pracownika za miesiąc (przenoszony do pozycji rezerwy).</summary>
public class WynikRezerwyUrlopowej {

    /// <summary>Pracownik pominięty (niezatrudniony na koniec miesiąca, brak współczynnika) - pozycja nie powstaje.</summary>
    public bool Pominiety { get; set; }
    public string PowodPominiecia { get; set; } = "";

    public Wydzial Wydzial { get; set; }
    public CentrumKosztow CentrumKosztow { get; set; }
    public double WymiarEtatu { get; set; }
    public double GodzinNaDzien { get; set; }

    public double ZaleglyGodz { get; set; }
    public double BiezacyGodz { get; set; }
    public double WykorzystanyGodz { get; set; }
    public double GodzinyRezerwy { get; set; }

    public double WspolczynnikEkwiwalentu { get; set; }
    public double Podstawa1 { get; set; }
    public int MiesiecyPodstawy1 { get; set; }
    public double Podstawa1ZaGodzine { get; set; }
    public double Podstawa2 { get; set; }
    public double Podstawa2ZaDzien { get; set; }
    public double Podstawa2ZaGodzine { get; set; }
    public double StawkaGodzinowa { get; set; }
    public decimal Kwota { get; set; }

    public string Ostrzezenia { get; set; } = "";

    public void DodajOstrzezenie(string tresc) => Ostrzezenia = Ostrzezenia.Length == 0 ? tresc : Ostrzezenia + "; " + tresc;
}
