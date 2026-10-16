using System.Collections.Generic;
using Soneta.Types;

namespace A1.KalkulatorPodstawy
{
    /// <summary>
    /// Wynik jednego przeliczenia kalkulatora. Same wartości (bez wierszy enova), bo sesja symulacji
    /// jest zamykana bez zapisu zaraz po obliczeniu i jej obiekty przestają być ważne.
    /// </summary>
    public class WynikPodstawyZasilku
    {
        public string Pracownik { get; internal set; } = "";
        public string KodPracownika { get; internal set; } = "";

        /// <summary>Jak powstał wynik: symulacja nowej nieobecności albo przeliczenie istniejącej.</summary>
        public string Tryb { get; internal set; } = "";
        public string RodzajSwiadczenia { get; internal set; } = "";
        public string DefinicjaNieobecnosci { get; internal set; } = "";
        public FromTo OkresNieobecnosci { get; internal set; } = FromTo.Empty;

        /// <summary>Podstawa wyliczona silnikiem enova z rozbiciem (PodstawaZasiłku.Podstawa).</summary>
        public decimal Podstawa { get; internal set; }

        /// <summary>Podstawa wpisana w składnik naliczonego elementu (to trafia na listę płac).</summary>
        public decimal PodstawaNaElemencie { get; internal set; }

        /// <summary>Podstawa zapisana w bazie na wcześniej naliczonej nieobecności (porównanie z enova).</summary>
        public decimal PodstawaZapisanaWBazie { get; internal set; }
        public bool JestPodstawaWBazie { get; internal set; }
        public decimal RoznicaDoBazy => JestPodstawaWBazie ? PodstawaNaElemencie - PodstawaZapisanaWBazie : 0m;

        public decimal StawkaDzienna100 { get; internal set; }

        public YearMonth OkresPodstawyOd { get; internal set; } = YearMonth.Empty;
        public YearMonth OkresPodstawyDo { get; internal set; } = YearMonth.Empty;
        public string OkresPodstawy => OkresPodstawyOd == YearMonth.Empty
            ? "(brak - patrz reguły specjalne)"
            : OkresPodstawyOd + " - " + OkresPodstawyDo;

        public List<MiesiacPodstawy> Miesiace { get; } = new List<MiesiacPodstawy>();
        public List<SkladnikPodstawy> Skladniki { get; } = new List<SkladnikPodstawy>();
        public List<NaliczonyElement> NaliczoneElementy { get; } = new List<NaliczonyElement>();
        public List<StawkaDzienna> Stawki { get; } = new List<StawkaDzienna>();
        public List<RegulaSpecjalna> Reguly { get; } = new List<RegulaSpecjalna>();

        /// <summary>Podsumowanie silnika (PodstawaZasiłku.Info).</summary>
        public string Podsumowanie { get; internal set; } = "";

        /// <summary>Szczegółowy log silnika (kategoria "Chorobowe" z System.Diagnostics.Trace).</summary>
        public string Log { get; internal set; } = "";

        /// <summary>Komunikat dla operatora, gdy wyliczenie się nie udało (wtedy reszta jest pusta).</summary>
        public string Blad { get; internal set; } = "";
        public bool JestBlad => Blad.Length > 0;
    }

    public class MiesiacPodstawy
    {
        public YearMonth Miesiac { get; internal set; }
        public Time NormaCzas { get; internal set; }
        public int NormaDni { get; internal set; }
        public Time PrzepracowanyCzas { get; internal set; }
        public int PrzepracowaneDni { get; internal set; }

        /// <summary>Przepracował co najmniej połowę (ta sama porównywana miara co w enova: czas albo dni).</summary>
        public bool CoNajmniejPolowa { get; internal set; }

        /// <summary>Miesiąc wzięty do średniej (wg komunikatu silnika).</summary>
        public bool Wliczony { get; internal set; }
        public decimal KwotaPrzedUzupelnieniem { get; internal set; }
        public decimal KwotaPoUzupelnieniu { get; internal set; }
        public bool Okresowy { get; internal set; }
        public string Uwagi { get; internal set; } = "";
    }

    public class SkladnikPodstawy
    {
        public YearMonth Miesiac { get; internal set; }
        public string Definicja { get; internal set; } = "";
        public string Element { get; internal set; } = "";
        public FromTo ZaOkres { get; internal set; }
        public string OkresNaliczania { get; internal set; } = "";
        public decimal Wartosc { get; internal set; }

        /// <summary>wprost / uzupełniony / nominalnie (z umowy) / okresowy / wg algorytmu / pominięty.</summary>
        public string SposobUjecia { get; internal set; } = "";
        public decimal KwotaBrutto { get; internal set; }
        public decimal Mnoznik { get; internal set; }
        public string MnoznikOpis { get; internal set; } = "";

        /// <summary>Wkład do podstawy: po uzupełnieniu i po pomniejszeniu o składki pracownika.</summary>
        public decimal KwotaWPodstawie { get; internal set; }
        public string Uwagi { get; internal set; } = "";
    }

    public class NaliczonyElement
    {
        public string Definicja { get; internal set; } = "";
        public FromTo Okres { get; internal set; }
        public int Dni { get; internal set; }
        public decimal Podstawa { get; internal set; }
        public Percent Procent { get; internal set; }
        public decimal KwotaDzienna { get; internal set; }
        public decimal Wartosc { get; internal set; }
        public string Algorytm { get; internal set; } = "";
    }

    public class StawkaDzienna
    {
        public string Opis { get; internal set; } = "";
        public Percent Procent { get; internal set; }
        public decimal KwotaDzienna { get; internal set; }
    }

    public class RegulaSpecjalna
    {
        public string Regula { get; internal set; } = "";
        public string Opis { get; internal set; } = "";
    }

    /// <summary>Wiersz zestawienia przy przeliczeniu kilku zaznaczonych pracowników.</summary>
    public class WierszZestawienia
    {
        public string Kod { get; internal set; } = "";
        public string Pracownik { get; internal set; } = "";
        public string Tryb { get; internal set; } = "";
        public decimal Podstawa { get; internal set; }
        public decimal PodstawaNaElemencie { get; internal set; }
        public decimal PodstawaWBazie { get; internal set; }
        public decimal Roznica { get; internal set; }
        public string OkresPodstawy { get; internal set; } = "";
        public string Komunikat { get; internal set; } = "";
    }
}
