using System;
using Soneta.Business;
using Soneta.Config;
using Soneta.Types;

namespace A1.Rozszerzenia
{
    // Zrodlo tresci kolumny opisowej raportu "Pelna lista plac (XLSX)".
    // [Caption] daje polska etykiete na liscie wyboru w oknie Opcji.
    public enum A1ZrodloKolumny
    {
        [Caption("(kolumna nieużywana)")] Brak = 0,
        [Caption("Kod pracownika")] KodPracownika = 1,
        [Caption("Nazwisko i imię")] NazwiskoImie = 2,
        [Caption("Nazwisko")] Nazwisko = 3,
        [Caption("Imię")] Imie = 4,
        [Caption("Wydział (wg daty wypłaty)")] Wydzial = 5,
        [Caption("Stanowisko (wg daty wypłaty)")] Stanowisko = 6,
        [Caption("PESEL")] Pesel = 7,
        [Caption("Numer listy płac")] NumerListyPlac = 8,
        [Caption("Definicja listy płac")] DefinicjaListyPlac = 9,
        [Caption("Data wypłaty")] DataWyplaty = 10,
        [Caption("Okres wypłaty")] OkresWyplaty = 11,
        [Caption("Cecha pracownika (nazwa w kolumnie Parametr)")] CechaPracownika = 12,
        [Caption("Cecha wypłaty (nazwa w kolumnie Parametr)")] CechaWyplaty = 13,
    }

    // Ustawienia raportu "Pelna lista plac (XLSX)" trzymane w DRZEWIE KONFIGURACJI enova
    // (tabele CfgNodes/CfgAttributes - to samo miejsce, w ktorym enova trzyma swoje opcje):
    //
    //   Root
    //    └─ A1Testy                (CfgNodeType.Node)
    //        └─ Raport placowy     (CfgNodeType.Leaf)  <- atrybuty = parametry ponizej
    //
    // Odczyt NIGDY nie zaklada wezla (dziala na sesji tylko-do-odczytu i zwraca wartosci
    // domyslne, gdy wezla jeszcze nie ma). Wezel powstaje dopiero przy pierwszym ZAPISIE,
    // czyli gdy uzytkownik zmieni pole na zakladce Narzedzia -> Opcje -> A1Testy ->
    // Konfiguracja raportu placowego (Config.A1RaportPlacowy.pageform.xml).
    //
    // API konfiguracji: CfgManager(Session).Root, CfgNode.FindSubNode/AddNode/
    // GetAttribute/SetAttribute - wszystko publiczne w Soneta.Business (ns Soneta.Config).
    public class A1RaportPlacUstawienia
    {
        public const string WezelGlowny = "A1Testy";
        public const string WezelRaportu = "Raport placowy";

        // Liczba definiowalnych kolumn opisowych (slotow) na poczatku raportu.
        public const int LiczbaSlotow = 5;

        public const string DomyslnyPrefiksPliku = "A1_Pelna_Lista_Plac_";
        public const string DomyslnaNazwaArkusza = "Lista płac";

        readonly Session session;

        public A1RaportPlacUstawienia(Session session)
        {
            this.session = session;
        }

        // --- 5 slotow kolumn opisowych ------------------------------------------------
        // Domyslnie sloty 1-3 odtwarzaja uklad sprzed wersji definiowalnej
        // (Kod / Nazwisko i imie / Wydzial), sloty 4-5 sa puste.

        public A1ZrodloKolumny Zrodlo1 { get { return Zrodlo(1, A1ZrodloKolumny.KodPracownika); } set { ZapiszZrodlo(1, value); } }
        public string Parametr1 { get { return Str("Kolumna 1 parametr"); } set { Zapisz("Kolumna 1 parametr", value ?? ""); } }
        public string Naglowek1 { get { return Str("Kolumna 1 nagłówek"); } set { Zapisz("Kolumna 1 nagłówek", value ?? ""); } }

        public A1ZrodloKolumny Zrodlo2 { get { return Zrodlo(2, A1ZrodloKolumny.NazwiskoImie); } set { ZapiszZrodlo(2, value); } }
        public string Parametr2 { get { return Str("Kolumna 2 parametr"); } set { Zapisz("Kolumna 2 parametr", value ?? ""); } }
        public string Naglowek2 { get { return Str("Kolumna 2 nagłówek"); } set { Zapisz("Kolumna 2 nagłówek", value ?? ""); } }

        public A1ZrodloKolumny Zrodlo3 { get { return Zrodlo(3, A1ZrodloKolumny.Wydzial); } set { ZapiszZrodlo(3, value); } }
        public string Parametr3 { get { return Str("Kolumna 3 parametr"); } set { Zapisz("Kolumna 3 parametr", value ?? ""); } }
        public string Naglowek3 { get { return Str("Kolumna 3 nagłówek"); } set { Zapisz("Kolumna 3 nagłówek", value ?? ""); } }

        public A1ZrodloKolumny Zrodlo4 { get { return Zrodlo(4, A1ZrodloKolumny.Brak); } set { ZapiszZrodlo(4, value); } }
        public string Parametr4 { get { return Str("Kolumna 4 parametr"); } set { Zapisz("Kolumna 4 parametr", value ?? ""); } }
        public string Naglowek4 { get { return Str("Kolumna 4 nagłówek"); } set { Zapisz("Kolumna 4 nagłówek", value ?? ""); } }

        public A1ZrodloKolumny Zrodlo5 { get { return Zrodlo(5, A1ZrodloKolumny.Brak); } set { ZapiszZrodlo(5, value); } }
        public string Parametr5 { get { return Str("Kolumna 5 parametr"); } set { Zapisz("Kolumna 5 parametr", value ?? ""); } }
        public string Naglowek5 { get { return Str("Kolumna 5 nagłówek"); } set { Zapisz("Kolumna 5 nagłówek", value ?? ""); } }

        // Dostep po numerze slotu (1..LiczbaSlotow) - uzywane przez worker.
        public A1ZrodloKolumny ZrodloSlotu(int nr)
        {
            switch (nr)
            {
                case 1: return Zrodlo1;
                case 2: return Zrodlo2;
                case 3: return Zrodlo3;
                case 4: return Zrodlo4;
                case 5: return Zrodlo5;
                default: return A1ZrodloKolumny.Brak;
            }
        }

        public string ParametrSlotu(int nr)
        {
            switch (nr)
            {
                case 1: return Parametr1;
                case 2: return Parametr2;
                case 3: return Parametr3;
                case 4: return Parametr4;
                case 5: return Parametr5;
                default: return "";
            }
        }

        // Naglowek kolumny: wlasny z konfiguracji, a gdy pusty - opis zrodla.
        public string NaglowekSlotu(int nr)
        {
            string wlasny;
            switch (nr)
            {
                case 1: wlasny = Naglowek1; break;
                case 2: wlasny = Naglowek2; break;
                case 3: wlasny = Naglowek3; break;
                case 4: wlasny = Naglowek4; break;
                case 5: wlasny = Naglowek5; break;
                default: wlasny = ""; break;
            }
            if (!string.IsNullOrEmpty(wlasny) && wlasny.Trim().Length > 0) return wlasny.Trim();

            A1ZrodloKolumny zr = ZrodloSlotu(nr);
            if (zr == A1ZrodloKolumny.CechaPracownika || zr == A1ZrodloKolumny.CechaWyplaty)
            {
                string p = ParametrSlotu(nr);
                if (!string.IsNullOrEmpty(p) && p.Trim().Length > 0) return p.Trim();
            }
            return DomyslnyNaglowek(zr);
        }

        public static string DomyslnyNaglowek(A1ZrodloKolumny zr)
        {
            switch (zr)
            {
                case A1ZrodloKolumny.KodPracownika: return "Kod";
                case A1ZrodloKolumny.NazwiskoImie: return "Imię i Nazwisko";
                case A1ZrodloKolumny.Nazwisko: return "Nazwisko";
                case A1ZrodloKolumny.Imie: return "Imię";
                case A1ZrodloKolumny.Wydzial: return "Wydział";
                case A1ZrodloKolumny.Stanowisko: return "Stanowisko";
                case A1ZrodloKolumny.Pesel: return "PESEL";
                case A1ZrodloKolumny.NumerListyPlac: return "Lista płac";
                case A1ZrodloKolumny.DefinicjaListyPlac: return "Definicja listy";
                case A1ZrodloKolumny.DataWyplaty: return "Data wypłaty";
                case A1ZrodloKolumny.OkresWyplaty: return "Okres";
                case A1ZrodloKolumny.CechaPracownika: return "Cecha pracownika";
                case A1ZrodloKolumny.CechaWyplaty: return "Cecha wypłaty";
                default: return "";
            }
        }

        // --- pozostale parametry raportu ----------------------------------------------

        // Blok 17 stalych kolumn skladkowo-podatkowych (ZUS/PPK/PIT + kwota do wyplaty).
        public bool PokazSkladki
        {
            get { return Bool("Kolumny ZUS PPK PIT", true); }
            set { Zapisz("Kolumny ZUS PPK PIT", value); }
        }

        public string PrefiksNazwyPliku
        {
            get { return Str("Prefiks nazwy pliku"); }
            set { Zapisz("Prefiks nazwy pliku", value ?? ""); }
        }

        public string NazwaArkusza
        {
            get { return Str("Nazwa arkusza"); }
            set { Zapisz("Nazwa arkusza", value ?? ""); }
        }

        // false = sortowanie wg nazwiska (domyslne), true = wg kodu pracownika.
        public bool SortujWgKodu
        {
            get { return Bool("Sortuj wg kodu", false); }
            set { Zapisz("Sortuj wg kodu", value); }
        }

        public string PrefiksNazwyPlikuEfekt { get { return Efekt(PrefiksNazwyPliku, DomyslnyPrefiksPliku); } }

        public string NazwaArkuszaEfekt
        {
            get
            {
                string n = Efekt(NazwaArkusza, DomyslnaNazwaArkusza);
                // Excel: max 31 znakow, bez : \ / ? * [ ]
                foreach (char zly in new[] { ':', '\\', '/', '?', '*', '[', ']' })
                    n = n.Replace(zly, ' ');
                n = n.Trim();
                if (n.Length == 0) n = DomyslnaNazwaArkusza;
                if (n.Length > 31) n = n.Substring(0, 31);
                return n;
            }
        }

        static string Efekt(string wartosc, string domyslna)
        {
            return string.IsNullOrEmpty(wartosc) || wartosc.Trim().Length == 0 ? domyslna : wartosc.Trim();
        }

        // --- dostep do drzewa konfiguracji --------------------------------------------

        // Zrodlo trzymane jako NAZWA elementu enum (string), nie jako liczba: CfgAttribute
        // nie zna typu enum, a konwersja int->enum przez TypeConverter nie dziala.
        A1ZrodloKolumny Zrodlo(int nr, A1ZrodloKolumny domyslne)
        {
            string s = Str("Kolumna " + nr + " źródło");
            if (string.IsNullOrEmpty(s)) return domyslne;
            try { return (A1ZrodloKolumny)Enum.Parse(typeof(A1ZrodloKolumny), s, true); }
            catch { return domyslne; }
        }

        void ZapiszZrodlo(int nr, A1ZrodloKolumny wartosc)
        {
            Zapisz("Kolumna " + nr + " źródło", wartosc.ToString());
        }

        CfgNode Wezel(bool utworz)
        {
            if (session == null) return null;
            CfgNode root = new CfgManager(session).Root;
            if (root == null) return null;

            CfgNode a1 = root.FindSubNode(WezelGlowny, false);
            if (a1 == null)
            {
                if (!utworz) return null;
                a1 = root.AddNode(WezelGlowny, CfgNodeType.Node);
            }

            CfgNode raport = a1.FindSubNode(WezelRaportu, false);
            if (raport == null)
            {
                if (!utworz) return null;
                raport = a1.AddNode(WezelRaportu, CfgNodeType.Leaf);
            }
            return raport;
        }

        bool Bool(string nazwa, bool domyslna)
        {
            try
            {
                CfgNode w = Wezel(false);
                if (w == null) return domyslna;
                return (bool)w.GetAttribute(nazwa, typeof(bool), domyslna);
            }
            catch { return domyslna; }
        }

        string Str(string nazwa)
        {
            try
            {
                CfgNode w = Wezel(false);
                if (w == null) return "";
                return (w.GetAttribute(nazwa, typeof(string), "") as string) ?? "";
            }
            catch { return ""; }
        }

        void Zapisz(string nazwa, object wartosc)
        {
            CfgNode w = Wezel(true);
            if (w != null) w.SetAttribute(nazwa, wartosc);
        }
    }
}
