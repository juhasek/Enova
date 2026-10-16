using System;
using System.Collections.Generic;
using System.Linq;
using Soneta.Business;
using Soneta.Kadry;
using Soneta.Kalend;
using Soneta.Langs;
using Soneta.Tools;
using Soneta.Types;

namespace A1.KalkulatorPodstawy
{
    /// <summary>
    /// Okno kalkulatora podstawy zasiłku. Działa na sesji tylko do odczytu (jak kalkulator wynagrodzeń enova);
    /// obliczenie idzie w osobnej sesji symulacji (SymulacjaPodstawyZasilku), która nigdy nie jest zapisywana.
    /// </summary>
    [Caption("Kalkulator podstawy zasiłku")]
    public class KalkulatorPodstawyZasilkuParams : ContextBase
    {
        private readonly Pracownik[] wybraniPracownicy;
        private Pracownik pracownik;
        private DefinicjaNieobecnosci definicjaNieobecnosci;
        private Date dataOd;
        private Date dataDo;
        private PrzyczynaZwolnienia kodZwolnienia;
        private bool leczenieSzpitalne;
        private bool wymusPonowneUstalenie;

        public KalkulatorPodstawyZasilkuParams(Context context, Pracownik[] pracownicy) : base(context)
        {
            wybraniPracownicy = pracownicy ?? new Pracownik[0];
            pracownik = wybraniPracownicy.FirstOrDefault();
            dataOd = Date.Today;
            dataDo = Date.Today;
            DefinicjaNieobecnosci = DomyslnaDefinicja();
        }

        // ---------- parametry ----------

        [Caption("Pracownik")]
        public Pracownik Pracownik
        {
            get => pracownik;
            set { pracownik = value; WyczyscWynik(); OnChanged(); }
        }

        public Pracownik[] GetListPracownik() => wybraniPracownicy.Length > 1 ? wybraniPracownicy : null;

        [Caption("Data powstania niezdolności")]
        public Date DataOd
        {
            get => dataOd;
            set
            {
                if (dataDo < value || dataDo == dataOd)
                    dataDo = value;
                dataOd = value;
                WyczyscWynik();
                OnChanged();
            }
        }

        [Caption("Nieobecność do")]
        public Date DataDo
        {
            get => dataDo;
            set
            {
                if (value < dataOd)
                    throw new ArgumentException("Data końca nieobecności nie może być wcześniejsza niż data jej początku.".Translate());
                dataDo = value;
                WyczyscWynik();
                OnChanged();
            }
        }

        [Caption("Definicja nieobecności")]
        public DefinicjaNieobecnosci DefinicjaNieobecnosci
        {
            get => definicjaNieobecnosci;
            set
            {
                definicjaNieobecnosci = value;
                kodZwolnienia = DomyslnyKod(value);
                WyczyscWynik();
                OnChanged();
            }
        }

        /// <summary>Tylko nieobecności „ZUS” (choroba, opieka, macierzyński, rehabilitacja) bez blokady.</summary>
        public DefinicjaNieobecnosci[] GetListDefinicjaNieobecnosci()
        {
            return Session.GetKalend().DefNieobecnosci.Cast<DefinicjaNieobecnosci>()
                .Where(d => d.Typ == TypNieobecnosci.NieobecnośćZUS && !d.Blokada)
                .OrderBy(d => d.Nazwa)
                .ToArray();
        }

        [Caption("Rodzaj świadczenia")]
        public string RodzajSwiadczenia => definicjaNieobecnosci == null ? "" : OpisRodzaju(definicjaNieobecnosci.Przyczyna);

        [Caption("Kod zwolnienia")]
        public PrzyczynaZwolnienia KodZwolnienia
        {
            get => kodZwolnienia;
            set { kodZwolnienia = value; WyczyscWynik(); OnChanged(); }
        }

        public PrzyczynaZwolnienia[] GetListKodZwolnienia()
        {
            PrzyczynaNieobecnosci przyczyna = definicjaNieobecnosci?.Przyczyna ?? PrzyczynaNieobecnosci.NieDotyczy;
            return Enum.GetValues(typeof(PrzyczynaZwolnienia)).Cast<PrzyczynaZwolnienia>()
                .Where(k => KodPasujeDo(k, przyczyna))
                .ToArray();
        }

        [Caption("Leczenie szpitalne")]
        public bool LeczenieSzpitalne
        {
            get => leczenieSzpitalne;
            set { leczenieSzpitalne = value; WyczyscWynik(); OnChanged(); }
        }

        [Caption("Wymuś ponowne ustalenie podstawy (diagnostyka)")]
        public bool WymusPonowneUstalenie
        {
            get => wymusPonowneUstalenie;
            set { wymusPonowneUstalenie = value; WyczyscWynik(); OnChanged(); }
        }

        public bool IsVisibleKodZwolnienia() => GetListKodZwolnienia().Length > 1;
        public bool IsVisibleLeczenieSzpitalne() => definicjaNieobecnosci?.Przyczyna == PrzyczynaNieobecnosci.ZwolnienieChorobowe;

        // ---------- wynik ----------

        public WynikPodstawyZasilku Wynik { get; private set; } = new WynikPodstawyZasilku();
        public List<WierszZestawienia> Zestawienie { get; } = new List<WierszZestawienia>();

        public bool JestWielu => wybraniPracownicy.Length > 1;
        public bool IsVisibleZestawienie() => JestWielu;

        [TranslateIgnore]
        public void Przelicz()
        {
            if (pracownik == null)
                throw new InvalidOperationException("Wybierz pracownika.".Translate());
            Wynik = SymulacjaPodstawyZasilku.Policz(Session.Login, Parametry(pracownik));
            OnChanged();
        }

        [TranslateIgnore]
        public void PrzeliczZaznaczonych()
        {
            Zestawienie.Clear();
            foreach (Pracownik p in wybraniPracownicy)
            {
                WynikPodstawyZasilku w = SymulacjaPodstawyZasilku.Policz(Session.Login, Parametry(p));
                Zestawienie.Add(new WierszZestawienia
                {
                    Kod = p.Kod,
                    Pracownik = p.ToString(),
                    Tryb = w.Tryb,
                    Podstawa = w.Podstawa,
                    PodstawaNaElemencie = w.PodstawaNaElemencie,
                    PodstawaWBazie = w.PodstawaZapisanaWBazie,
                    Roznica = w.RoznicaDoBazy,
                    OkresPodstawy = w.JestBlad ? "" : w.OkresPodstawy,
                    Komunikat = w.Blad
                });
            }
            OnChanged();
        }

        private ParametrySymulacji Parametry(Pracownik p)
        {
            return new ParametrySymulacji
            {
                Pracownik = p.Guid,
                DefinicjaNieobecnosci = definicjaNieobecnosci?.Guid ?? Guid.Empty,
                DataOd = dataOd,
                DataDo = dataDo,
                KodZwolnienia = kodZwolnienia,
                LeczenieSzpitalne = leczenieSzpitalne,
                WymusPonowneUstalenie = wymusPonowneUstalenie
            };
        }

        private void WyczyscWynik()
        {
            Wynik = new WynikPodstawyZasilku();
        }

        // ---------- pomocnicze ----------

        private DefinicjaNieobecnosci DomyslnaDefinicja()
        {
            return GetListDefinicjaNieobecnosci().FirstOrDefault(d => d.Przyczyna == PrzyczynaNieobecnosci.ZwolnienieChorobowe)
                   ?? GetListDefinicjaNieobecnosci().FirstOrDefault();
        }

        private static PrzyczynaZwolnienia DomyslnyKod(DefinicjaNieobecnosci definicja)
        {
            switch (definicja?.Przyczyna)
            {
                case PrzyczynaNieobecnosci.ZwolnienieChorobowe: return PrzyczynaZwolnienia.ZwolnienieLekarskie;
                case PrzyczynaNieobecnosci.UrlopOpiekuńczy: return PrzyczynaZwolnienia.OpiekaSprawowanaNadDziećmiDoLat14;
                case PrzyczynaNieobecnosci.UrlopRehabilitacyjny: return PrzyczynaZwolnienia.UrlopRehabilitacyjny;
                default: return PrzyczynaZwolnienia.NieDotyczy;
            }
        }

        /// <summary>Kody zwolnień enova są pogrupowane numeracją: 1-99 choroba, 100-199 opieka, 200+ rehabilitacja.</summary>
        private static bool KodPasujeDo(PrzyczynaZwolnienia kod, PrzyczynaNieobecnosci przyczyna)
        {
            int wartosc = (int)kod;
            switch (przyczyna)
            {
                case PrzyczynaNieobecnosci.ZwolnienieChorobowe: return wartosc > 0 && wartosc < 100;
                case PrzyczynaNieobecnosci.UrlopOpiekuńczy: return wartosc >= 100 && wartosc < 200;
                case PrzyczynaNieobecnosci.UrlopRehabilitacyjny: return wartosc >= 200;
                default: return kod == PrzyczynaZwolnienia.NieDotyczy;
            }
        }

        private static string OpisRodzaju(PrzyczynaNieobecnosci przyczyna)
        {
            switch (przyczyna)
            {
                case PrzyczynaNieobecnosci.ZwolnienieChorobowe: return "Wynagrodzenie chorobowe / zasiłek chorobowy";
                case PrzyczynaNieobecnosci.UrlopOpiekuńczy: return "Zasiłek opiekuńczy";
                case PrzyczynaNieobecnosci.UrlopRehabilitacyjny: return "Świadczenie rehabilitacyjne";
                case PrzyczynaNieobecnosci.UrlopMacierzyński:
                case PrzyczynaNieobecnosci.UrlopMacierzyńskiUzupełniający:
                case PrzyczynaNieobecnosci.UrlopRodzicielski:
                case PrzyczynaNieobecnosci.UrlopOjcowski: return "Zasiłek macierzyński";
                default: return przyczyna.ToString();
            }
        }
    }
}
