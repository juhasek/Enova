using Soneta.Business;

namespace A1.Metryczka;

/// <summary>Definicja jednej cechy globalnej z metryczki klienta (wiersz arkusza "metryczka.xlsx").</summary>
record DefinicjaCechy(int Wiersz, string Nazwa, FeatureTypeNumber Typ, string Kategoria, string Opis, string[] Slownik = null);

/// <summary>Mapowanie metryczki zaakceptowane 2026-10-26: niepewne pola jako String bez słownika.</summary>
static class DefinicjeCech
{
    public const string Prefiks = "TST_";

    // Cechy globalne = cechy węzła głównego konfiguracji (FeatureDefs.TableName = 'CfgNodes', Session.Global).
    public const string TabelaCechGlobalnych = "CfgNodes";

    // Zestaw cech, przez który cechy TST_ są widoczne w Opcje → Cechy globalne.
    public const string NazwaZestawu = "TST_Metryczka";

    const FeatureTypeNumber Bool = FeatureTypeNumber.Bool;
    const FeatureTypeNumber Tekst = FeatureTypeNumber.String;
    const FeatureTypeNumber Calkowita = FeatureTypeNumber.Int;

    const string Klient = "Klient";
    const string Zakres = "Zakres usług";
    const string Listy = "Listy płac";
    const string Integracje = "Integracje";
    const string Wydruki = "Wydruki i raporty";
    const string Bank = "Bank i waluty";
    const string Kontakt = "Kontakt";
    const string Instytucje = "Instytucje";

    public static readonly DefinicjaCechy[] Wszystkie =
    {
        new(2, "TST_NazwaBazySQL", Tekst, Klient, "Nazwa bazy w SQL"),
        new(3, "TST_NrKlienta", Tekst, Klient, "Nr klienta"),
        new(4, "TST_NazwaKlienta", Tekst, Klient, "Nazwa klienta wyświetlana dla użytkowników systemu"),
        new(5, "TST_JGSAP", Tekst, Klient, "JG SAP"),
        new(6, "TST_FTE", FeatureTypeNumber.Double, Klient, "FTE – liczba ma wpływ na pracochłonność migracji"),
        new(7, "TST_LiczbaKartotek", Calkowita, Klient, "Liczba kartotek – nr SAP (pracochłonność migracji eTeczki)"),
        new(8, "TST_PierwszaListaPlac", FeatureTypeNumber.YearMonth, Listy, "Miesiąc i rok pierwszych danych z list płac, w tym danych migracyjnych"),
        new(9, "TST_Specjalista", Tekst, Klient, "Specjalista obsługujący"),
        new(10, "TST_Place", Bool, Zakres, "Płace"),
        new(11, "TST_TerminListyGlownej", Tekst, Listy, "Lista główna – termin naliczenia",
            new[] { "w tym samym miesiącu", "z przesunięciem", "i tak i tak" }),
        new(12, "TST_ListaDodatkowa", Bool, Listy, "Czy jest stała lista dodatkowa"),
        new(13, "TST_TerminListyDodatkowej", Tekst, Listy, "Lista dodatkowa – kiedy liczona"),
        new(14, "TST_Kadry", Bool, Zakres, "Kadry"),
        new(15, "TST_UmowyZlecenia", Bool, Zakres, "Umowy zlecenia"),
        new(16, "TST_WorkTime", Bool, Integracje, "Integracja z WorkTime"),
        new(17, "TST_InterfejsyDedykowane", Bool, Integracje, "Dedykowane interfejsy poza WorkTime"),
        new(18, "TST_WydrukiDedykowane", Bool, Wydruki, "Dedykowane wydruki (umowy, aneksy, pasek)"),
        new(19, "TST_RaportyDedykowane", Bool, Wydruki, "Dedykowane raporty"),
        new(20, "TST_PIT8CzSAP", Bool, Wydruki, "PIT-8C dodatkowe z SAP PIT-11"),
        new(21, "TST_DostepOnline", Bool, Integracje, "Dostęp on-line do systemu dla klientów"),
        new(22, "TST_LiczbaDostepow", Calkowita, Integracje, "Ile dostępów do systemu dla klientów"),
        new(23, "TST_PPK", Bool, Zakres, "PPK"),
        new(24, "TST_INGRPS", Bool, Bank, "ING RPS"),
        new(25, "TST_BankKlienta", Tekst, Bank, "Bank klienta"),
        new(26, "TST_PKzSystemu", Bool, Listy, "Czy jest generowany PK z systemu"),
        new(27, "TST_PrzelewyWaluta", Bool, Bank, "Czy przelewy w innych walutach"),
        new(28, "TST_Waluta", Tekst, Bank, "Waluta przelewów"),
        new(29, "TST_ETeczka", Bool, Zakres, "E-teczka"),
        new(30, "TST_WieleJG", Tekst, Klient, "JG, gdy baza jest na więcej niż 1 JG (puste = jedna JG)"),
        new(31, "TST_KontoPowiadomien", Tekst, Kontakt, "Konto pocztowe wychodzące do powiadomień mailowych"),
        new(32, "TST_KontaktOsoba", Tekst, Kontakt, "Główna osoba kontaktowa po stronie klienta"),
        new(33, "TST_KontaktEmail", Tekst, Kontakt, "Kontakt mail klienta – na niego wysyłane są powiadomienia"),
        new(34, "TST_KontaktTelefon", Tekst, Kontakt, "Kontakt numer telefonu klienta"),
        new(35, "TST_PlatformaPPE", Tekst, Integracje, "PPE platforma klienta"),
        new(36, "TST_PracaGov", Bool, Instytucje, "Praca.gov"),
        new(37, "TST_GUSWspolpraca", Tekst, Instytucje, "GUS rodzaj współpracy"),
        new(38, "TST_EPFRONWspolpraca", Tekst, Instytucje, "E-PFRON rodzaj współpracy"),
        new(39, "TST_BankWspolpraca", Tekst, Instytucje, "Bank klienta rodzaj współpracy"),
        new(40, "TST_PPKWspolpraca", Tekst, Instytucje, "PPK rodzaj współpracy"),
        new(41, "TST_ZUSPUE", Bool, Instytucje, "Dostęp ZUS PUE"),
        new(42, "TST_USUPL1", Bool, Instytucje, "US UPL-1"),
        new(43, "TST_Platnik", Bool, Instytucje, "Dostęp PŁATNIK"),
    };
}
