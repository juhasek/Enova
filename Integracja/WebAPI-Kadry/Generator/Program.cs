// Generator scenariuszy testowych integracji WebAPI (kadry).
// Jedna lista scenariuszy -> kolekcja Postman (ramki zadan) + arkusz scenariuszy xlsx.
// Uzycie: dotnet bin\out\GenScenariusze.dll <katalog WebAPI-Kadry>
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DevExpress.Spreadsheet;

namespace GenScenariusze;

enum Oczek { Sukces, Odrzucone, BezWyjatku, Recznie }

record Sc(string Id, string Grupa, string Nazwa, string Cel, string Warunki,
          string Usluga, string Metoda, string Pars, string DaneKluczowe,
          string Oczekiwany, string Kontrola, Oczek Ocz,
          string ZapiszId = null, string Haslo = null);

static class Program
{
    // ---------- pomocnicze budowanie ramek JSON (z placeholderami Postman {{...}}) ----------
    static string D(int y, int m, int d) => "{ \"Year\": " + y + ", \"Month\": " + m + ", \"Day\": " + d + " }";

    static string Adr(string ulica = "Testowa", string woj = "6", string nazwaFirmy = null)
    {
        var sb = new StringBuilder("{ ");
        if (nazwaFirmy != null) sb.Append("\"NazwaFirmy\": \"" + nazwaFirmy + "\", ");
        sb.Append("\"Ulica\": \"" + ulica + "\", \"NrDomu\": \"1\", \"NrLokalu\": \"2\", \"KodPocztowyS\": \"30-001\", ");
        sb.Append("\"Miejscowosc\": \"Kraków\", \"Poczta\": \"Kraków\", \"KodKraju\": \"PL\", \"Kraj\": \"Polska\", ");
        if (woj != null) sb.Append("\"Wojewodztwo\": " + woj + ", ");
        sb.Append("\"Powiat\": \"Kraków\", \"Gmina\": \"Kraków\" }");
        return sb.ToString();
    }

    static string Zatr(string od = null, int wn = 1, int wd = 1, string jednostka = "{{wydzialKod}}",
                       string stanowisko = "{{stanowisko}}", string doDaty = "null")
    {
        od ??= D(2026, 1, 1);
        return "{\n" +
               "      \"OkresZatrudnieniaOd\": " + od + ",\n" +
               "      \"OkresZatrudnieniaDo\": " + doDaty + ",\n" +
               "      \"DataZawarciaUmowy\": " + D(2025, 12, 15) + ",\n" +
               "      \"DataRozpoczeciaPracy\": " + od + ",\n" +
               "      \"JednostkaOrg\": \"" + jednostka + "\",\n" +
               "      \"ZatrudnienieNaPodstawie\": 1,\n" +
               "      \"TypUmowy\": 1,\n" +
               "      \"RodzajZatrudnienia\": 0,\n" +
               "      \"Stanowisko\": \"" + stanowisko + "\",\n" +
               "      \"Wymiar\": { \"Numerator\": " + wn + ", \"Denominator\": " + wd + " },\n" +
               "      \"TypPracownika\": \"{{typPracownika}}\"\n" +
               "    }";
    }

    const string RachunekDomyslny =
        "[ { \"Numer\": \"PL61109010140000071219812874\", \"Kraj\": null, \"SWIFT\": null, \"KodBanku\": null, \"Domyslne\": true, \"Blokada\": false } ]";

    const string KontaktyDomyslne =
        "[\n      { \"Rodzaj\": 1, \"Kontakt\": \"600100200\", \"Opis\": \"test API\", \"Domyslny\": true },\n" +
        "      { \"Rodzaj\": 2, \"Kontakt\": \"jan.testowy@example.com\", \"Opis\": \"test API\", \"Domyslny\": true }\n    ]";

    static string Emp(string kod = "{{kodTest}}", string dataAkt = null, string nazwisko = "Testowy-API",
                      string pesel = "85031512351", string adresZam = null, string zatr = null,
                      string rachunki = RachunekDomyslny, string kontakty = KontaktyDomyslne,
                      string features = "[]", bool pominKod = false)
    {
        dataAkt ??= "{ \"From\": " + D(2026, 1, 1) + ", \"To\": null }";
        adresZam ??= Adr();
        zatr ??= Zatr();
        var sb = new StringBuilder("{\n");
        sb.Append("    \"DataAktualizacji\": " + dataAkt + ",\n");
        if (!pominKod) sb.Append("    \"Kod\": \"" + kod + "\",\n");
        sb.Append("    \"DataUrodzenia\": \"1985-03-15T00:00:00\",\n");
        sb.Append("    \"Imie\": \"Jan\",\n    \"ImieDrugie\": \"\",\n");
        sb.Append("    \"Nazwisko\": \"" + nazwisko + "\",\n    \"NazwiskoRodowe\": \"Testowy-API\",\n");
        sb.Append("    \"ImieOjca\": \"Adam\",\n    \"ImieMatki\": \"Ewa\",\n");
        sb.Append("    \"PESEL\": \"" + pesel + "\",\n");
        sb.Append("    \"Obywatelstwo\": \"polskie\",\n    \"MiejsceUrodzenia\": \"Kraków\",\n");
        sb.Append("    \"AdresZameldowania\": " + Adr() + ",\n");
        sb.Append("    \"AdresZamieszkania\": " + adresZam + ",\n");
        sb.Append("    \"AdresDoKorespondencji\": " + Adr(nazwaFirmy: "") + ",\n");
        sb.Append("    \"Podatki\": { \"KodUrzeduSkarbowego\": \"{{kodUS}}\", \"IdentyfikatorPodatkowy\": 1 },\n");
        sb.Append("    \"KodPrawaDostepu\": \"{{wydzialKod}}\",\n");
        sb.Append("    \"Zatrudnienie\": " + zatr + ",\n");
        sb.Append("    \"PKID\": \"{{pkid}}\",\n");
        sb.Append("    \"Rachunki\": " + rachunki + ",\n");
        sb.Append("    \"DaneKontaktowe\": " + kontakty + ",\n");
        sb.Append("    \"Features\": " + features + "\n  }");
        return sb.ToString();
    }

    static string Term(string kod = "{{kodTest}}", string od = null, string data = null,
                       string przyczyna = "{{przyczyna}}", string zus = "true")
    {
        od ??= D(2026, 1, 1);
        data ??= D(2026, 10, 31);
        return "{\n" +
               "    \"PracownikKod\": \"" + kod + "\",\n" +
               "    \"OkresUmowyOd\": " + od + ",\n" +
               "    \"OkresUmowyDo\": " + D(2026, 12, 31) + ",\n" +
               "    \"TerminationDate\": " + data + ",\n" +
               "    \"TerminationReason\": \"" + przyczyna + "\",\n" +
               "    \"RejestracjaZus\": " + zus + "\n  }";
    }

    static string Wydz(string id = null, string kod = "API-TEST-01", string nazwa = "Wydział testowy API",
                       string parents = "[ { \"StartDate\": \"2026-01-01T00:00:00\", \"ParentID\": {{parentWydzialId}} } ]")
    {
        var sb = new StringBuilder("{\n");
        if (id != null) sb.Append("    \"ID\": " + id + ",\n");
        sb.Append("    \"Code\": \"" + kod + "\",\n");
        sb.Append("    \"Name\": \"" + nazwa + "\",\n");
        sb.Append("    \"Parents\": " + parents + ",\n");
        sb.Append("    \"StartDate\": \"2026-01-01T00:00:00\",\n");
        sb.Append("    \"Locked\": false\n  }");
        return sb.ToString();
    }

    // ---------- lista scenariuszy ----------
    static List<Sc> Scenariusze()
    {
        const string G0 = "0 Diagnostyka", G1 = "1 Wydziały", G2 = "2 Pracownik", G3 = "3 Rozwiązanie umowy", G4 = "4 Techniczne i bezpieczeństwo";
        var l = new List<Sc>
        {
            new("D-01", G0, "Połączenie i wersje (GetWebServiceInfo)",
                "Potwierdzić, że MethodInvoker odpowiada, operator się loguje, i odczytać wersję enova oraz biblioteki WCF.Core (analiza: rozjazd wersji 2510 / 2606).",
                "Dodatek WebAPI i WCF.Core wgrane na serwer testowy; środowisko Postman uzupełnione.",
                "Core", "GetWebServiceInfo", "{ }",
                "—",
                "IsException = false; ResultInstance.Data zawiera WersjaEnova, WersjaBibliotekiWS, LoginSuccess = true.",
                "Zanotować WersjaEnova i WersjaBibliotekiWS w kolumnie Uwagi.", Oczek.BezWyjatku),

            new("D-02", G0, "Mapa usług na kontrolery Dynamic WebAPI",
                "Sprawdzić, czy usługi dodatku (IStaff, IStrukturaWydzialowa) mają kontrolery Dynamic WebAPI (/api/{usługa}/{metoda}); w kodzie dodatku brak DynamicApiControllerAttribute.",
                "D-01 OK.",
                "Core", "GetServicesToControllersMap", "{ }",
                "—",
                "Lista par Service -> Controller.",
                "Jeśli IStaff / IStrukturaWydzialowa NIE występują - dodatek działa tylko przez MethodInvoker (stary endpoint). Wpisać wynik w Uwagi.", Oczek.BezWyjatku),

            new("D-03", G0, "Dokumentacja usługi IStaff",
                "Wygenerować dokumentację/wzorzec JSON metod dodatku i porównać z ramkami w tej kolekcji (nazwy pól, wymagalność).",
                "D-01 OK.",
                "Core", "GetDocumentation", "{ \"TypUslugi\": \"{{serviceStaff}}\", \"Wersja\": 2, \"SchemaJson\": true, \"DlaZapisu\": true, \"DynamicApi\": false }",
                "TypUslugi = pełna nazwa typu IStaff",
                "Zwrócona dokumentacja z przykładowym JSON-em UpsertEmployee / UpsertTermination.",
                "Rozbieżności z ramkami odnotować w Uwagi.", Oczek.BezWyjatku),

            // ---- Wydziały ----
            new("W-01", G1, "Lista oddziałów firmy",
                "Odczyt oddziałów (GetOddzialy); każde wywołanie robi ResyncCaches - zmierzyć czas.",
                "—", "Wydzialy", "GetOddzialy", "{ \"LastID\": 0, \"PacketSize\": 100 }",
                "PacketSize 100",
                "Lista oddziałów (Identifier, Symbol, Locked).",
                "Porównać z Narzędzia → Firma → Oddziały. Zanotować czas odpowiedzi.", Oczek.BezWyjatku),

            new("W-02", G1, "Lista wydziałów",
                "Odczyt drzewa wydziałów (GetWydzialy) i wybór ID wydziału nadrzędnego do W-03.",
                "—", "Wydzialy", "GetWydzialy", "{ \"LastID\": 0, \"PacketSize\": 200, \"UpdateDate\": " + D(2026, 10, 1) + " }",
                "UpdateDate = data, na którą czytane są kod/nazwa",
                "Lista wydziałów z Code, Name, Parents, StartDate, EndDate.",
                "Wpisać ID wybranego wydziału nadrzędnego do zmiennej parentWydzialId.", Oczek.BezWyjatku),

            new("W-03", G1, "Nowy wydział",
                "Założenie wydziału pod wskazanym rodzicem. Analiza: wynik nie ustawia Success = true.",
                "parentWydzialId uzupełnione (W-02); kod API-TEST-01 nie istnieje.",
                "Wydzialy", "UpsertWydzial", Wydz(),
                "Code API-TEST-01, rodzic {{parentWydzialId}} od 2026-01-01",
                "Zwrócone ID i Kod nowego wydziału. (Ryzyko: Success = false mimo zapisu.)",
                "Kadry → Wydziały: wydział API-TEST-01 istnieje pod właściwym rodzicem, okres od 2026-01-01.", Oczek.Sukces, ZapiszId: "testWydzialId"),

            new("W-04", G1, "Zmiana nazwy wydziału",
                "Modyfikacja nazwy po ID. Analiza: Kod/Nazwa ustawiane bez daty - sprawdzić, czy powstaje nowy zapis historii, czy nadpisywany jest bieżący.",
                "W-03 OK (testWydzialId ustawione).",
                "Wydzialy", "UpsertWydzial", Wydz(id: "{{testWydzialId}}", nazwa: "Wydział testowy API - zmiana", parents: "[ ]"),
                "ID {{testWydzialId}}, nowa nazwa",
                "Success / ID zwrócone; nazwa zmieniona.",
                "Historia danych wydziału: czy jest nowy zapis z datą, czy nadpisano poprzedni.", Oczek.Sukces),

            new("W-05", G1, "Nowy wydział bez rodzica",
                "Walidacja: brak Parents przy nowym wydziale.",
                "—", "Wydzialy", "UpsertWydzial", Wydz(kod: "API-TEST-02", parents: "[ ]"),
                "Parents pusta lista",
                "Odrzucone z komunikatem „Należy podac ID wydzialu nadrzednego”.",
                "Wydział API-TEST-02 NIE powstał.", Oczek.Odrzucone),

            new("W-06", G1, "Parents = null",
                "Analiza: Parents = null powoduje NullReferenceException zamiast czytelnego błędu.",
                "—", "Wydzialy", "UpsertWydzial", Wydz(kod: "API-TEST-03", parents: "null"),
                "Parents null",
                "Odrzucone z czytelnym komunikatem. (Ryzyko: NullReferenceException.)",
                "Zanotować treść błędu.", Oczek.Odrzucone),

            new("W-07", G1, "Nieistniejące ID wydziału",
                "Modyfikacja wydziału o nieistniejącym ID.",
                "—", "Wydzialy", "UpsertWydzial", Wydz(id: "999999", nazwa: "Nie istnieje", parents: "[ ]"),
                "ID 999999",
                "Odrzucone z czytelnym komunikatem. (Ryzyko: NullReferenceException.)",
                "Zanotować treść błędu.", Oczek.Odrzucone),

            new("W-08", G1, "Usunięcie wydziału przez operatora bez uprawnień",
                "Analiza: dodatek przekazuje pustą nazwę metody do kontroli uprawnień - DeleteWydzial może nie podlegać kontroli praw.",
                "W-03 OK. Operator {{operatorOgraniczony}} bez prawa do zmian wydziałów (Konfiguracja).",
                "Wydzialy", "DeleteWydzial", "{ \"ID\": {{testWydzialId}} }",
                "Operator ograniczony",
                "Odrzucone - brak uprawnień. (Ryzyko: wydział usunięty.)",
                "Wydział API-TEST-01 nadal istnieje.", Oczek.Odrzucone, Haslo: "ograniczony"),

            new("W-09", G1, "Usunięcie wydziału",
                "Usunięcie testowego wydziału. Analiza: wynik nie ustawia Success; zwraca ID nawet, gdy wydziału nie było.",
                "W-03 OK; wydział bez pracowników.",
                "Wydzialy", "DeleteWydzial", "{ \"ID\": {{testWydzialId}} }",
                "ID {{testWydzialId}}",
                "Wydział usunięty, zwrócone ID. (Ryzyko: Success = false.)",
                "Wydział API-TEST-01 nie istnieje. Powtórzyć żądanie - zanotować wynik dla nieistniejącego ID.", Oczek.Sukces),

            // ---- Pracownik ----
            new("P-01", G2, "Nowy pracownik - komplet danych",
                "Założenie pracownika ze wszystkimi sekcjami. Analiza: przy nowym pracowniku DataAktualizacji jest pomijana; Tyub4 ustawiany na sztywno 0110.",
                "Kod {{kodTest}} nie istnieje. Istnieją: wydział {{wydzialKod}}, stanowisko {{stanowisko}}, US {{kodUS}}, cechy „Typ pracownika” (historia) i „Numer koncernowy” (pracownik).",
                "Staff", "UpsertEmployee", Emp(),
                "Kod {{kodTest}}, PESEL 85031512351 (M), etat od 2026-01-01, 1/1, UoP na czas nieokreślony",
                "Success = true, „Employee was successfully added/updated”.",
                "Kartoteka: dane osobowe, płeć = mężczyzna (z PESEL), 3 adresy (woj. małopolskie), US, etat (wydział, stanowisko, wymiar, typ umowy), tytuł ubezp. 0110, rachunek PL61…, kontakty tel./e-mail domyślne, cechy. Data początku zapisu historii - zanotować.", Oczek.Sukces),

            new("P-02", G2, "Ponowne wysłanie tych samych danych",
                "Idempotentność: drugi raz ta sama ramka nie może dublować rachunków ani kontaktów.",
                "P-01 OK.",
                "Staff", "UpsertEmployee", Emp(),
                "Identyczna ramka jak P-01",
                "Success = true.",
                "Brak duplikatów rachunków i kontaktów; nie powstał nowy zapis historii (data 2026-01-01 = istniejący zapis).", Oczek.Sukces),

            new("P-03", G2, "Aktualizacja istniejącego bez daty",
                "Walidacja: istniejący pracownik bez DataAktualizacji.From.",
                "P-01 OK.",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": null, \"To\": null }", nazwisko: "Nie-Powinno-Sie-Zapisac"),
                "DataAktualizacji.From = null",
                "Success = false, „No employee updated/added”.",
                "Nazwisko bez zmian.", Oczek.Odrzucone),

            new("P-04", G2, "Aktualizacja danych osobowych z datą",
                "Zmiana nazwiska od 2026-03-01 tworzy nowy zapis historii.",
                "P-01 OK.",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": " + D(2026, 3, 1) + ", \"To\": null }", nazwisko: "Testowy-API-Zmiana", zatr: "null"),
                "From 2026-03-01, nowe nazwisko, Zatrudnienie = null",
                "Success = true.",
                "Historia: nowy zapis od 2026-03-01 z nowym nazwiskiem; zapis od 2026-01-01 ze starym. Etat bez zmian.", Oczek.Sukces),

            new("P-05a", G2, "Przygotowanie: zmiana wymiaru w przyszłości",
                "Przygotowanie do P-05b - zapis historii od 2026-09-01 z wymiarem 1/2.",
                "P-04 OK.",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": " + D(2026, 9, 1) + ", \"To\": null }", nazwisko: "Testowy-API-Zmiana", zatr: Zatr(wn: 1, wd: 2)),
                "From 2026-09-01, wymiar 1/2",
                "Success = true.",
                "Historia: zapis od 2026-09-01 z wymiarem 1/2.", Oczek.Sukces),

            new("P-05b", G2, "Zmiana wsteczna nie może nadpisać przyszłych zapisów",
                "Analiza (krytyczne): dane z datą X trafiają do WSZYSTKICH późniejszych zapisów historii.",
                "P-05a OK.",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": " + D(2026, 4, 1) + ", \"To\": null }", nazwisko: "Testowy-API-Zmiana", zatr: Zatr(wn: 1, wd: 1)),
                "From 2026-04-01, wymiar 1/1",
                "Success = true; zapis od 2026-09-01 zachowuje wymiar 1/2. (Ryzyko: nadpisany na 1/1.)",
                "Historia: zapis 2026-04-01 wymiar 1/1, zapis 2026-09-01 wymiar 1/2 - zanotować faktyczny stan.", Oczek.Sukces),

            new("P-06", G2, "Adres bez województwa",
                "Analiza: Wojewodztwo zawsze nadpisywane (brak w JSON = nieokreślone).",
                "P-01 OK.",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": " + D(2026, 9, 1) + ", \"To\": null }", nazwisko: "Testowy-API-Zmiana", zatr: "null", adresZam: Adr(ulica: "Nowa", woj: null)),
                "AdresZamieszkania bez pola Wojewodztwo, ulica Nowa",
                "Success = true; ulica = Nowa, województwo nadal małopolskie. (Ryzyko: nieokreślone.)",
                "Adres zamieszkania w zapisie od 2026-09-01.", Oczek.Sukces),

            new("P-07", G2, "Zablokowany rachunek nie może zostać odblokowany",
                "Analiza: Blokada zawsze ustawiana z DTO (brak = false).",
                "P-01 OK. W enova ręcznie zaznaczyć Blokada na rachunku PL61….",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": " + D(2026, 9, 1) + ", \"To\": null }", nazwisko: "Testowy-API-Zmiana", zatr: "null",
                                               rachunki: "[ { \"Numer\": \"PL61 1090 1014 0000 0712 1981 2874\", \"Domyslne\": true } ]"),
                "Ten sam rachunek (ze spacjami), bez pola Blokada",
                "Success = true; rachunek nie zdublowany, Blokada pozostaje. (Ryzyko: odblokowany.)",
                "Rachunki pracownika: jeden rachunek PL61…, stan blokady.", Oczek.Sukces),

            new("P-08", G2, "Kontakty: telefon stacjonarny i dwa e-maile domyślne",
                "Analiza: telefon stacjonarny nie trafia do pola kontaktowego; kilka domyślnych tego samego rodzaju.",
                "P-01 OK.",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": " + D(2026, 9, 1) + ", \"To\": null }", nazwisko: "Testowy-API-Zmiana", zatr: "null",
                    kontakty: "[\n      { \"Rodzaj\": 0, \"Kontakt\": \"12 345 67 89\", \"Domyslny\": true },\n      { \"Rodzaj\": 2, \"Kontakt\": \"jan.drugi@example.com\", \"Domyslny\": true }\n    ]"),
                "TelStac domyślny, drugi e-mail domyślny",
                "Success = true.",
                "Dane kontaktowe: ile domyślnych e-maili; czy telefon stacjonarny jest w polu Telefon. Zapisać stan.", Oczek.Sukces),

            new("P-09", G2, "Nieistniejący wydział (JednostkaOrg)",
                "Walidacja słownika wydziałów.",
                "P-01 OK.",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": " + D(2026, 9, 1) + ", \"To\": null }", nazwisko: "Testowy-API-Zmiana", zatr: Zatr(jednostka: "NIEISTNIEJE-XYZ")),
                "JednostkaOrg NIEISTNIEJE-XYZ",
                "Odrzucone: „Nie udało się pobrać wydziału o kodzie …”. Nic nie zapisane.",
                "Etat bez zmian.", Oczek.Odrzucone),

            new("P-10", G2, "Nieistniejące stanowisko",
                "Walidacja słownika stanowisk (wyszukiwanie po nazwie).",
                "P-01 OK.",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": " + D(2026, 9, 1) + ", \"To\": null }", nazwisko: "Testowy-API-Zmiana", zatr: Zatr(stanowisko: "Stanowisko Nieistniejące XYZ")),
                "Stanowisko spoza słownika",
                "Odrzucone: „Nie udało się pobrać definicji stanowiska …”.",
                "Etat bez zmian.", Oczek.Odrzucone),

            new("P-11", G2, "Cechy przez listę Features",
                "Zapis cechy z flagą „Dostęp przez WebAPI” / „Obsługa WCF” przez ogólny mechanizm biblioteki.",
                "P-01 OK. Na Pracowniku istnieje cecha {{cechaWebApi}} (tekstowa) z flagą dostępu przez WebAPI.",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": " + D(2026, 9, 1) + ", \"To\": null }", nazwisko: "Testowy-API-Zmiana", zatr: "null",
                    features: "[ { \"Name\": \"{{cechaWebApi}}\", \"Value\": \"wartość z API\" } ]"),
                "Features: {{cechaWebApi}} = „wartość z API”",
                "Success = true.",
                "Cecha ma wartość „wartość z API”. Powtórzyć z cechą bez flagi - zanotować komunikat.", Oczek.Sukces),

            new("P-12", G2, "Data odcięcia - aktualizacja pracownika",
                "Dane starsze niż data odcięcia są odrzucane.",
                "W Narzędzia → Opcje → strona konfiguracji dodatku ustawić „Data odcięcia” = 2026-06-01. Po teście wyczyścić.",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": " + D(2026, 5, 1) + ", \"To\": null }", nazwisko: "Odcięte", zatr: "null"),
                "From 2026-05-01 < data odcięcia",
                "Success = false, „Data older than the cut-off date - synchronization skipped”.",
                "Brak zapisu historii od 2026-05-01.", Oczek.Odrzucone),

            // ---- Rozwiązanie umowy ----
            new("R-01", G3, "Rozwiązanie umowy - standard",
                "Zakończenie etatu, przyczyna, wyrejestrowanie z ZUS. Analiza: daty Do ubezpieczeń = data rozwiązania + 1.",
                "P-01 OK (etat od 2026-01-01). Data odcięcia wyczyszczona. Przyczyna {{przyczyna}} istnieje w słowniku.",
                "Staff", "UpsertTermination", Term(),
                "OkresUmowyOd 2026-01-01, TerminationDate 2026-10-31, RejestracjaZus = true",
                "Success = true, „The employee's contract has been updated successfully”.",
                "Etat: okres 2026-01-01…2026-10-31 we wszystkich zapisach od tej umowy; przyczyna rozwiązania; ubezpieczenia: Wyrejestrowany, data Do (zanotować - 31.10 czy 01.11?) i data na ZUS ZWUA.", Oczek.Sukces),

            new("R-02", G3, "Zwykła aktualizacja po rozwiązaniu nie może cofnąć rozwiązania",
                "Analiza (krytyczne): UpsertEmployee czyści pola rozwiązania i ustawia okres etatu z DTO (brak OkresZatrudnieniaDo = bez końca).",
                "R-01 OK.",
                "Staff", "UpsertEmployee", Emp(dataAkt: "{ \"From\": " + D(2026, 10, 1) + ", \"To\": null }", nazwisko: "Testowy-API-Zmiana", zatr: Zatr()),
                "From 2026-10-01, Zatrudnienie bez OkresZatrudnieniaDo",
                "Success = true; etat nadal kończy się 2026-10-31, przyczyna rozwiązania zachowana. (Ryzyko: rozwiązanie cofnięte.)",
                "Etat w zapisach od 2026-10-01: data końca, przyczyna, kod zwolnienia, wyrejestrowanie.", Oczek.Sukces),

            new("R-03", G3, "Przyczyna spoza słownika",
                "Analiza: przyczyna szukana po nazwie; brak = pusta przyczyna, a wynik i tak Success = true.",
                "R-01 OK.",
                "Staff", "UpsertTermination", Term(przyczyna: "PRZYCZYNA SPOZA SŁOWNIKA"),
                "TerminationReason spoza słownika",
                "Odrzucone z komunikatem. (Ryzyko: Success = true i pusta przyczyna.)",
                "Przyczyna rozwiązania na etacie.", Oczek.Odrzucone),

            new("R-04", G3, "Niezgodny początek umowy",
                "OkresUmowyOd nie pasuje do żadnego etatu.",
                "P-01 OK.",
                "Staff", "UpsertTermination", Term(od: D(2025, 1, 1)),
                "OkresUmowyOd 2025-01-01",
                "Success = false, „Employee's contract was not updated”.",
                "Etat bez zmian.", Oczek.Odrzucone),

            new("R-05", G3, "Nieistniejący pracownik",
                "Walidacja kodu pracownika.",
                "—",
                "Staff", "UpsertTermination", Term(kod: "NIEISTNIEJE-XYZ"),
                "PracownikKod NIEISTNIEJE-XYZ",
                "Odrzucone: „Employee code (PersonID) not found: NIEISTNIEJE-XYZ”.",
                "—", Oczek.Odrzucone),

            new("R-06a", G3, "Przygotowanie: drugi pracownik",
                "Pracownik do testów R-06b i R-07 (etat od 2026-11-01).",
                "Kod {{kodTest2}} nie istnieje.",
                "Staff", "UpsertEmployee", Emp(kod: "{{kodTest2}}", pesel: "90010112349",
                    dataAkt: "{ \"From\": " + D(2026, 11, 1) + ", \"To\": null }", zatr: Zatr(od: D(2026, 11, 1)),
                    rachunki: "[ ]", kontakty: "[ ]"),
                "Kod {{kodTest2}}, etat od 2026-11-01",
                "Success = true.",
                "Pracownik {{kodTest2}} istnieje, tytuł ubezp. 0110.", Oczek.Sukces),

            new("R-06b", G3, "Rozwiązanie bez rejestracji w ZUS",
                "Analiza: RejestracjaZus = false usuwa tytuł ubezpieczenia (Tyub4) z całego zapisu historii.",
                "R-06a OK.",
                "Staff", "UpsertTermination", Term(kod: "{{kodTest2}}", od: D(2026, 11, 1), data: D(2026, 11, 30), zus: "false"),
                "TerminationDate 2026-11-30, RejestracjaZus = false",
                "Success = true.",
                "Etat do 2026-11-30; tytuł ubezpieczenia - zanotować czy pusty; wpływ na deklaracje ZUS.", Oczek.Sukces),

            new("R-07", G3, "NO SHOW - pracownik nie podjął pracy",
                "TerminationReason = NO SHOW i data rozwiązania = początek umowy czyści okres etatu.",
                "R-06a OK.",
                "Staff", "UpsertTermination", Term(kod: "{{kodTest2}}", od: D(2026, 11, 1), data: D(2026, 11, 1), przyczyna: "NO SHOW"),
                "OkresUmowyOd = TerminationDate = 2026-11-01",
                "Success = true.",
                "Etat: okres pusty, pola rozwiązania wyczyszczone.", Oczek.Sukces),

            new("R-08", G3, "Data odcięcia - rozwiązanie umowy",
                "Analiza: UpsertTermination nie sprawdza daty odcięcia.",
                "R-01 OK. Ustawić „Data odcięcia” = 2026-12-01. Po teście wyczyścić.",
                "Staff", "UpsertTermination", Term(data: D(2026, 10, 15)),
                "TerminationDate 2026-10-15 < data odcięcia",
                "Odrzucone (zgodnie z ideą daty odcięcia). (Ryzyko: rozwiązanie przetworzone.)",
                "Etat: data końca - czy zmieniona na 2026-10-15.", Oczek.Odrzucone),

            // ---- Techniczne ----
            new("T-01", G4, "Błędne hasło",
                "Odrzucenie żądania przy złym haśle operatora.",
                "—",
                "Staff", "UpsertEmployee", Emp(), "Password = zle-haslo",
                "IsException = true / błąd logowania.", "—", Oczek.Odrzucone, Haslo: "zle"),

            new("T-02", G4, "Brak wymaganego pola Kod",
                "Walidacja wymagalności pól (Required.Always).",
                "—",
                "Staff", "UpsertEmployee", Emp(pominKod: true), "Brak pola Kod",
                "Odrzucone - błąd deserializacji z nazwą pola.", "Zanotować komunikat.", Oczek.Odrzucone),

            new("T-03", G4, "Dane osobowe w logach serwera",
                "Analiza: klasa bazowa zapisuje żądanie/odpowiedź do plików .txt - sprawdzić, czy PESEL i numery rachunków lądują na dysku.",
                "Wykonane P-01 i T-01.",
                null, null, null, "—",
                "Brak danych osobowych w plikach lub logowanie wyłączone.",
                "Na serwerze WebAPI/biznesowym wyszukać pliki .txt (folder Response / logi) z datą testu; sprawdzić, czy zawierają PESEL 85031512351 lub PL61…", Oczek.Recznie),
        };
        return l;
    }

    // ---------- Postman ----------
    static JsonObject Zadanie(Sc s)
    {
        string serwis = s.Usluga switch { "Staff" => "{{serviceStaff}}", "Wydzialy" => "{{serviceWydzialy}}", _ => "{{serviceCore}}" };
        string oper = s.Haslo == "ograniczony" ? "{{operatorOgraniczony}}" : "{{operator}}";
        string haslo = s.Haslo switch { "ograniczony" => "{{passwordOgraniczony}}", "zle" => "zle-haslo", _ => "{{password}}" };
        string body =
            "{\n" +
            "  \"DatabaseHandle\": \"{{database}}\",\n" +
            "  \"Operator\": \"" + oper + "\",\n" +
            "  \"Password\": \"" + haslo + "\",\n" +
            "  \"ServiceName\": \"" + serwis + "\",\n" +
            "  \"MethodName\": \"" + s.Metoda + "\",\n" +
            "  \"MethodArgs\": {\n  \"pars\": " + s.Pars + "\n  }\n}";

        var test = new List<string>
        {
            "const j = pm.response.json();",
            "let r = j.ResultInstance;",
            "if (typeof r === 'string') { try { r = JSON.parse(r); } catch (e) { } }",
            "console.log('" + s.Id + "', JSON.stringify(j, null, 2));",
            "pm.test('HTTP 200', () => pm.response.to.have.status(200));",
        };
        switch (s.Ocz)
        {
            case Oczek.Sukces:
                test.Add("pm.test('Brak wyjątku', () => pm.expect(j.IsException, j.ExceptionMessage).to.eql(false));");
                test.Add("pm.test('Success = true', () => pm.expect(r && r.Success, r && r.Description).to.eql(true));");
                break;
            case Oczek.Odrzucone:
                test.Add("pm.test('Żądanie odrzucone (wyjątek lub Success = false)', () => pm.expect(j.IsException === true || (r != null && r.Success === false), JSON.stringify(r)).to.eql(true));");
                break;
            case Oczek.BezWyjatku:
                test.Add("pm.test('Brak wyjątku', () => pm.expect(j.IsException, j.ExceptionMessage).to.eql(false));");
                break;
        }
        if (s.ZapiszId != null)
            test.Add("if (r && r.ID) { pm.collectionVariables.set('" + s.ZapiszId + "', r.ID); console.log('" + s.ZapiszId + " = ' + r.ID); }");

        var opis = new StringBuilder();
        opis.AppendLine("**Cel:** " + s.Cel).AppendLine();
        opis.AppendLine("**Warunki wstępne:** " + s.Warunki).AppendLine();
        opis.AppendLine("**Oczekiwany wynik:** " + s.Oczekiwany).AppendLine();
        opis.AppendLine("**Kontrola w enova:** " + s.Kontrola);

        return new JsonObject
        {
            ["name"] = s.Id + " " + s.Nazwa,
            ["event"] = new JsonArray(new JsonObject
            {
                ["listen"] = "test",
                ["script"] = new JsonObject { ["type"] = "text/javascript", ["exec"] = new JsonArray(test.Select(x => (JsonNode)x).ToArray()) }
            }),
            ["request"] = new JsonObject
            {
                ["method"] = "POST",
                ["header"] = new JsonArray(new JsonObject { ["key"] = "Content-Type", ["value"] = "application/json" }),
                ["body"] = new JsonObject
                {
                    ["mode"] = "raw",
                    ["raw"] = body,
                    ["options"] = new JsonObject { ["raw"] = new JsonObject { ["language"] = "json" } }
                },
                ["url"] = new JsonObject
                {
                    ["raw"] = "{{baseUrl}}/api/MethodInvoker/InvokeServiceMethod",
                    ["host"] = new JsonArray("{{baseUrl}}"),
                    ["path"] = new JsonArray("api", "MethodInvoker", "InvokeServiceMethod")
                },
                ["description"] = opis.ToString()
            }
        };
    }

    static void ZapiszPostman(List<Sc> sc, string katalog)
    {
        var foldery = new JsonArray();
        foreach (var g in sc.Where(s => s.Usluga != null).GroupBy(s => s.Grupa))
            foldery.Add(new JsonObject { ["name"] = g.Key, ["item"] = new JsonArray(g.Select(s => (JsonNode)Zadanie(s)).ToArray()) });

        var kolekcja = new JsonObject
        {
            ["info"] = new JsonObject
            {
                ["name"] = "Integracja WebAPI - Kadry (scenariusze testowe)",
                ["description"] = "Ramki testowe usług IStaff / IStrukturaWydzialowa dodatku WebAPI oraz diagnostyki WCF.Core. " +
                                  "Wywołania przez MethodInvoker (POST /api/MethodInvoker/InvokeServiceMethod). " +
                                  "Uruchamiać w kolejności folderów na BAZIE TESTOWEJ. Opis scenariuszy: Scenariusze testowe.xlsx.",
                ["schema"] = "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
            },
            ["item"] = foldery,
            ["variable"] = new JsonArray(
                new JsonObject { ["key"] = "testWydzialId", ["value"] = "" })
        };
        File.WriteAllText(Path.Combine(katalog, "Postman", "WebAPI-Kadry.postman_collection.json"),
            kolekcja.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }),
            new UTF8Encoding(false));

        var zm = Zmienne();
        var env = new JsonObject
        {
            ["name"] = "WebAPI-Kadry - TEST (szablon)",
            ["values"] = new JsonArray(zm.Select(z => (JsonNode)new JsonObject
            {
                ["key"] = z.Nazwa,
                ["value"] = z.Domyslna,
                ["type"] = z.Nazwa.StartsWith("password") ? "secret" : "default",
                ["enabled"] = true
            }).ToArray()),
            ["_postman_variable_scope"] = "environment"
        };
        File.WriteAllText(Path.Combine(katalog, "Postman", "WebAPI-Kadry.postman_environment.json"),
            env.ToJsonString(new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }),
            new UTF8Encoding(false));
    }

    record Zm(string Nazwa, string Opis, string Domyslna);

    static List<Zm> Zmienne() => new()
    {
        new("baseUrl", "Adres serwera WebAPI enova (bez końcowego /).", "https://SERWER-WEBAPI"),
        new("database", "Nazwa bazy enova (DatabaseHandle) - TYLKO testowa.", "NAZWA-BAZY-TESTOWEJ"),
        new("operator", "Operator z pełnymi prawami do kadr i konfiguracji.", "Administrator"),
        new("password", "Hasło operatora.", ""),
        new("operatorOgraniczony", "Operator BEZ praw do zmian wydziałów (W-08).", "OperatorTest"),
        new("passwordOgraniczony", "Hasło operatora ograniczonego.", ""),
        new("serviceStaff", "Pełna nazwa typu interfejsu IStaff z assembly, np. <Namespace>.Interfaces.IStaff, <NazwaAssembly> (patrz Analiza.md).", "UZUPELNIJ.Interfaces.IStaff, UZUPELNIJ"),
        new("serviceWydzialy", "Pełna nazwa typu interfejsu IStrukturaWydzialowa z assembly.", "UZUPELNIJ.Interfaces.IStrukturaWydzialowa, UZUPELNIJ"),
        new("serviceCore", "Usługa diagnostyczna biblioteki WCF.Core (wersja WebAPI).", "AltOne.WCF.Core.Interfaces.IOperationFunctions2, AltOne.WCF.Core"),
        new("kodTest", "Kod pracownika testowego nr 1 (nie może istnieć przed P-01).", "API-T01"),
        new("kodTest2", "Kod pracownika testowego nr 2 (nie może istnieć przed R-06a).", "API-T02"),
        new("wydzialKod", "Kod istniejącego wydziału (etat i prawo dostępu).", "UZUPELNIJ"),
        new("stanowisko", "NAZWA istniejącej definicji stanowiska (HR).", "UZUPELNIJ"),
        new("kodUS", "Kod istniejącego urzędu skarbowego.", "UZUPELNIJ"),
        new("typPracownika", "Wartość cechy „Typ pracownika” (cecha na historii pracownika musi istnieć).", "TEST"),
        new("pkid", "Wartość cechy „Numer koncernowy” (cecha na pracowniku musi istnieć).", "PKID-TEST-01"),
        new("przyczyna", "NAZWA istniejącej przyczyny rozwiązania umowy (słownik).", "UZUPELNIJ"),
        new("cechaWebApi", "Nazwa cechy tekstowej Pracownika z flagą „Dostęp przez WebAPI” (P-11).", "UZUPELNIJ"),
        new("parentWydzialId", "ID wydziału nadrzędnego dla W-03 (odczytać z W-02).", "1"),
    };

    // ---------- Excel ----------
    static void ZapiszXlsx(List<Sc> sc, string katalog)
    {
        using var wb = new Workbook();
        var ws = wb.Worksheets[0];
        ws.Name = "Scenariusze";
        string[] nag = { "ID", "Grupa", "Scenariusz", "Cel / ryzyko z analizy", "Warunki wstępne", "Żądanie (usługa.metoda)",
                         "Kluczowe dane wejściowe", "Oczekiwany wynik API", "Kontrola w enova", "Test Postman", "Wynik (OK/BŁĄD)", "Data / tester", "Uwagi" };
        int[] szer = { 7, 16, 32, 50, 40, 26, 36, 44, 50, 18, 14, 14, 40 };
        for (int c = 0; c < nag.Length; c++)
        {
            ws.Cells[0, c].Value = nag[c];
            ws.Cells[0, c].Font.Bold = true;
            ws.Columns[c].WidthInCharacters = szer[c];
        }
        int r = 1;
        foreach (var s in sc)
        {
            string zad = s.Usluga == null ? "ręcznie (bez żądania)" : (s.Usluga == "Core" ? "WCF.Core" : s.Usluga == "Staff" ? "IStaff" : "IStrukturaWydzialowa") + "." + s.Metoda;
            string tp = s.Ocz switch { Oczek.Sukces => "Success = true", Oczek.Odrzucone => "odrzucone", Oczek.BezWyjatku => "brak wyjątku", _ => "—" };
            string[] w = { s.Id, s.Grupa, s.Nazwa, s.Cel, s.Warunki, zad, s.DaneKluczowe, s.Oczekiwany, s.Kontrola, tp, "", "", "" };
            for (int c = 0; c < w.Length; c++) ws.Cells[r, c].Value = w[c];
            r++;
        }
        var zakres = ws.Range.FromLTRB(0, 0, nag.Length - 1, r - 1);
        zakres.Alignment.WrapText = true;
        zakres.Alignment.Vertical = SpreadsheetVerticalAlignment.Top;
        ws.AutoFilter.Apply(zakres);
        ws.FreezeRows(0);

        var wz = wb.Worksheets.Add("Zmienne Postman");
        string[] nz = { "Zmienna", "Opis", "Wartość domyślna w szablonie" };
        int[] sz = { 22, 80, 50 };
        for (int c = 0; c < nz.Length; c++) { wz.Cells[0, c].Value = nz[c]; wz.Cells[0, c].Font.Bold = true; wz.Columns[c].WidthInCharacters = sz[c]; }
        int i = 1;
        foreach (var z in Zmienne()) { wz.Cells[i, 0].Value = z.Nazwa; wz.Cells[i, 1].Value = z.Opis; wz.Cells[i, 2].Value = z.Domyslna; i++; }
        wz.Range.FromLTRB(0, 0, 2, i - 1).Alignment.WrapText = true;

        wb.SaveDocument(Path.Combine(katalog, "Scenariusze testowe.xlsx"), DocumentFormat.Xlsx);
    }

    static int Main(string[] args)
    {
        if (args.Length < 1) { Console.Error.WriteLine("Użycie: GenScenariusze <katalog WebAPI-Kadry>"); return 1; }
        var katalog = Path.GetFullPath(args[0]);
        var sc = Scenariusze();
        ZapiszPostman(sc, katalog);
        ZapiszXlsx(sc, katalog);
        Console.WriteLine($"Scenariusze: {sc.Count}, żądania Postman: {sc.Count(s => s.Usluga != null)}");
        return 0;
    }
}
