using System;
using Soneta.Business;
using Soneta.Config;

namespace A1.Rozszerzenia
{
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

        // Domyslne (uzywane, gdy w konfiguracji nic nie ustawiono).
        public const string DomyslnyNaglowekKod = "Kod";
        public const string DomyslnyNaglowekNazwisko = "Imię i Nazwisko";
        public const string DomyslnyNaglowekWydzial = "Wydział";
        public const string DomyslnyPrefiksPliku = "A1_Pelna_Lista_Plac_";
        public const string DomyslnaNazwaArkusza = "Lista płac";

        readonly Session session;

        public A1RaportPlacUstawienia(Session session)
        {
            this.session = session;
        }

        // --- kolumny opisowe: widocznosc + wlasny naglowek -----------------------------

        public bool PokazKod
        {
            get { return Bool("Kolumna Kod", true); }
            set { Zapisz("Kolumna Kod", value); }
        }

        public string NaglowekKod
        {
            get { return Str("Nagłówek Kod"); }
            set { Zapisz("Nagłówek Kod", value ?? ""); }
        }

        public bool PokazNazwisko
        {
            get { return Bool("Kolumna Nazwisko", true); }
            set { Zapisz("Kolumna Nazwisko", value); }
        }

        public string NaglowekNazwisko
        {
            get { return Str("Nagłówek Nazwisko"); }
            set { Zapisz("Nagłówek Nazwisko", value ?? ""); }
        }

        public bool PokazWydzial
        {
            get { return Bool("Kolumna Wydział", true); }
            set { Zapisz("Kolumna Wydział", value); }
        }

        public string NaglowekWydzial
        {
            get { return Str("Nagłówek Wydział"); }
            set { Zapisz("Nagłówek Wydział", value ?? ""); }
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

        // --- wartosci efektywne (puste ustawienie -> wartosc domyslna) -----------------

        public string NaglowekKodEfekt { get { return Efekt(NaglowekKod, DomyslnyNaglowekKod); } }
        public string NaglowekNazwiskoEfekt { get { return Efekt(NaglowekNazwisko, DomyslnyNaglowekNazwisko); } }
        public string NaglowekWydzialEfekt { get { return Efekt(NaglowekWydzial, DomyslnyNaglowekWydzial); } }
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
