// Arkusz wymagalnosci pol systemu zrodlowego dla ramek WebAPI (kadry).
// Zrodlo tresci: Mapowanie pol.md (analiza kodu dodatku, atrybuty Required.Always + logika zapisu).
using System.Drawing;
using DevExpress.Spreadsheet;

namespace GenScenariusze;

record Pole(string Zrodlo, string Ramka, string Status, string Wymagane, string Slownik,
            string SkutekBraku, string Uwagi, string DoUzgodnienia = "");

static class Mapowanie
{
    const string WYM = "WYMAGANE", WYMS = "WYMAGANE + SŁOWNIK", WAR = "WARUNKOWE",
                 OPC = "OPCJONALNE", OPCS = "OPCJONALNE + SŁOWNIK", NADP = "OPCJONALNE – NADPISUJE", NIE = "NIEOBSŁUGIWANE";

    const string Odrz = "Żądanie odrzucone (błąd deserializacji)";
    const string Wyj = "Wyjątek – nic się nie zapisuje";
    const string BezZm = "Pole w enova bez zmian";
    const string Pomin = "Wartość nie trafia do enova";

    static List<Pole> Pracownik() => new()
    {
        new("Effective as of", "DataAktualizacji.From", WAR, "WARUNKOWO", "—",
            "Istniejący pracownik: „No employee updated/added”. Nowy: bez skutku (data pomijana).",
            "Obiekt DataAktualizacji musi być zawsze (WYMAGANE); From wymagane przy aktualizacji istniejącego pracownika. Format {Year, Month, Day}. Data starsza niż „Data odcięcia” → odrzucenie."),
        new("Person ID (=PK-ID)", "Kod", WYM, "TAK", "—", Odrz,
            "Klucz wyszukiwania pracownika (komunikat dodatku: „Employee code (PersonID)”).", "Które pole jest kluczem: Person ID, User ID czy KOD?"),
        new("Person ID (=PK-ID)", "PKID", OPCS, "NIE", "Cecha „Numer koncernowy” (Pracownik)", BezZm,
            "Zapis do cechy; cecha musi istnieć, inaczej wyjątek."),
        new("User ID", "—", NIE, "NIE", "—", Pomin, "Chyba że zostanie uzgodnione jako Kod.", "j.w."),
        new("Date Of Birth", "DataUrodzenia", WYM, "TAK", "—", Odrz, "Format ISO: \"1985-03-15T00:00:00\" (nie {Year…})."),
        new("Place Of Birth", "MiejsceUrodzenia", OPC, "NIE", "—", BezZm, ""),
        new("First Name", "Imie", WYM, "TAK", "—", Odrz, ""),
        new("Middle Name", "ImieDrugie", WYM, "TAK", "—", Odrz, "Pole musi być obecne nawet bez drugiego imienia – wysłać \"\" (null odrzucany)."),
        new("Last Name", "Nazwisko", WYM, "TAK", "—", Odrz, ""),
        new("Birth Name", "NazwiskoRodowe", OPC, "NIE", "—", BezZm, ""),
        new("Gender", "—", NIE, "NIE", "—", Pomin, "Płeć liczona wyłącznie z PESEL. Cudzoziemiec bez PESEL nie dostanie płci z integracji.", "Czy rozszerzyć dodatek o płeć dla osób bez PESEL?"),
        new("Nationality", "Obywatelstwo", OPC, "NIE", "—", BezZm, "Tekst – nazwa obywatelstwa (np. „polskie”)."),
        new("Tax Office", "Podatki.KodUrzeduSkarbowego", OPCS, "NIE", "Urzędy skarbowe (kod)", BezZm + "; błędny kod = " + Wyj.ToLower(),
            "Wymaga mapowania kodów systemu źródłowego na kody US w enova."),
        new("National Id Card Type", "(Podatki.IdentyfikatorPodatkowy)", NIE, "NIE", "—", Pomin,
            "Typ dokumentu nie jest zapisywany. Ewentualnie wybór identyfikatora podatkowego: 1 = PESEL, 2 = NIP.", "Obsługa dokumentów cudzoziemców?"),
        new("National Id", "PESEL (lub NIP)", OPC, "NIE", "—", BezZm, "Wysyłać jako PESEL tylko gdy typ dokumentu = PESEL. Paszport nieobsługiwany."),
        new("Country/Region", "Adres*.KodKraju / Kraj", OPC, "NIE", "—", BezZm, "KodKraju = ISO (PL), Kraj = nazwa.", "Który adres w enova (zamieszkania / zameldowania / korespondencyjny)?"),
        new("Street", "Adres*.Ulica", OPC, "NIE", "—", BezZm, ""),
        new("House Number", "Adres*.NrDomu", OPC, "NIE", "—", BezZm, ""),
        new("Apartment", "Adres*.NrLokalu", OPC, "NIE", "—", BezZm, ""),
        new("Municipality", "Adres*.Gmina", OPC, "NIE", "—", BezZm, ""),
        new("City", "Adres*.Miejscowosc", OPC, "NIE", "—", BezZm, ""),
        new("Postal Code", "Adres*.KodPocztowyS", OPC, "NIE", "—", BezZm, "Format 00-000."),
        new("Region", "Adres*.Wojewodztwo", NADP, "ZALECANE", "—", "Województwo ustawione na „nieokreślone”",
            "Liczba 0–16 (6 = małopolskie, 7 = mazowieckie…) – tabela mapowania w arkuszu „Mapowania”."),
        new("Post", "Adres*.Poczta", OPC, "NIE", "—", BezZm, ""),
        new("(brak w źródle)", "Adres*.Powiat", OPC, "NIE", "—", BezZm, "Źródło nie ma powiatu."),
        new("Email Address", "DaneKontaktowe[] { Rodzaj: 2, Kontakt }", OPC, "NIE", "Rodzaje kontaktów (standard)", BezZm,
            "W elemencie listy Rodzaj i Kontakt wymagane; pusty Kontakt = element pominięty. Domyslny: true ustawia e-mail w kontakcie pracownika."),
        new("Country/Region Code + Area Code + Phone Number", "DaneKontaktowe[] { Rodzaj: 1, Kontakt }", OPC, "NIE", "Rodzaje kontaktów (standard)", BezZm,
            "Skleić w jeden tekst (np. „+48 600100200”). 1 = komórkowy, 0 = stacjonarny (stacjonarny nie trafia do pola Telefon)."),
        new("Job country/region", "—", NIE, "NIE", "—", Pomin, "Ewentualnie RodzajZatrudnienia = 5 (pracownik za granicą).", "Czy potrzebne?"),
        new("Pay Type", "—", NIE, "NIE", "—", Pomin, ""),
        new("IBAN", "Rachunki[].Numer", OPC, "NIE", "—", BezZm, "Z prefiksem kraju lub bez, spacje dozwolone. Rachunki tylko dopisywane, nigdy usuwane."),
        new("BIC", "Rachunki[].SWIFT", OPC, "NIE", "—", BezZm, ""),
        new("(brak w źródle)", "Rachunki[].Blokada / Domyslne", NADP, "ZALECANE", "—", "Rachunek odblokowany / niedomyślny",
            "Wysyłać jawnie Domyslne: true dla głównego rachunku."),
        new("Contract Start Date", "Zatrudnienie.OkresZatrudnieniaOd", WAR, "TAK (gdy sekcja etatu)", "—", Odrz,
            "Cała sekcja Zatrudnienie jest opcjonalna; jeśli wysłana – pole wymagane."),
        new("Contract End Date", "Zatrudnienie.OkresZatrudnieniaDo", NADP, "ZALECANE", "—", "Umowa bez końca – COFA wcześniejsze rozwiązanie umowy",
            "Po rozwiązaniu wysyłać datę rozwiązania albo nie wysyłać sekcji Zatrudnienie (Analiza 3.1)."),
        new("Recruit Date", "Zatrudnienie.DataRozpoczeciaPracy / DataZawarciaUmowy", OPC, "NIE", "—", BezZm, "", "Data rozpoczęcia pracy czy data zawarcia umowy?"),
        new("Contract Type", "Zatrudnienie.TypUmowy", WAR, "TAK (gdy sekcja etatu)", "—", Odrz,
            "Liczba: 1 = czas nieokreślony, 2 = okres próbny, 3 = czas określony, 5 = zastępstwo… – arkusz „Mapowania”."),
        new("KOD", "Zatrudnienie.JednostkaOrg", WYMS, "TAK (gdy sekcja etatu)", "Wydziały (kod)", Odrz + " / " + Wyj,
            "Musi istnieć wydział o tym kodzie.", "Czy KOD to kod jednostki organizacyjnej?"),
        new("KOD", "KodPrawaDostepu", OPCS, "NIE", "Wydziały (kod)", BezZm + "; błędny kod = " + Wyj.ToLower(), "Wydział pracownika do praw dostępu."),
        new("Positionh Title PL", "Zatrudnienie.Stanowisko", WYMS, "TAK (gdy sekcja etatu)", "Definicje stanowisk HR (nazwa)", Odrz + " / " + Wyj,
            "Dokładna NAZWA definicji stanowiska; inna pisownia = wyjątek.", "Stanowisko z „Positionh Title PL” czy „Job Title”?"),
        new("Job Title", "—", NIE, "NIE", "—", Pomin, "Chyba że ma zastąpić Positionh Title PL.", "j.w."),
        new("Position", "—", NIE, "NIE", "—", Pomin, "Stanowisko szukane po nazwie, nie po kodzie pozycji."),
        new("Position Entry Date", "—", NIE, "NIE", "—", Pomin, "Zmiana stanowiska datowana przez DataAktualizacji.From."),
        new("Function Code", "—", NIE, "NIE", "—", Pomin, ""),
        new("Standard Weekly Hours", "Zatrudnienie.Wymiar { Numerator, Denominator }", WAR, "TAK (gdy sekcja etatu)", "—", Odrz,
            "Godziny / 40 jako ułamek (40 → 1/1, 20 → 1/2, 30 → 3/4). Denominator = 0 → wymiar pominięty."),
        new("Employee Class", "Zatrudnienie.TypPracownika", OPCS, "NIE", "Cecha „Typ pracownika” (historia pracownika)", BezZm,
            "Cecha musi istnieć, inaczej wyjątek.", "Czy Employee Class = cecha „Typ pracownika”?"),
        new("(brak w źródle)", "Zatrudnienie.ZatrudnienieNaPodstawie", OPC, "ZALECANE", "—", BezZm, "Zalecana stała 1 = umowa o pracę."),
        new("(brak w źródle)", "Zatrudnienie.RodzajZatrudnienia", NADP, "ZALECANE", "—", "Ustawione „Nie dotyczy” (0)", "Wysyłać jawnie (zwykle 0)."),
        new("(brak w źródle)", "NIP, ImieOjca, ImieMatki, Features", OPC, "NIE", "Features: cechy z flagą „Dostęp przez WebAPI”", BezZm, ""),
    };

    static List<Pole> Rozwiazanie() => new()
    {
        new("Person ID / KOD pracownika", "PracownikKod", WYM, "TAK", "Pracownicy (kod)", Odrz + " / „Employee code (PersonID) not found”",
            "Ten sam klucz co Kod w UpsertEmployee.", "Które pole jest kluczem pracownika?"),
        new("Contract Start Date", "OkresUmowyOd", WYM, "TAK", "Etat pracownika", Odrz,
            "Musi być DOKŁADNIE równe początkowi etatu w enova, inaczej „Employee's contract was not updated” i brak zmian."),
        new("Contract End Date", "OkresUmowyDo", WYM, "TAK", "—", Odrz,
            "Kod go nie używa, ale bez niego żądanie jest odrzucane – wysłać dowolną datę."),
        new("Termination Date", "TerminationDate", WYM, "TAK", "—", Odrz, ""),
        new("TerminationReason", "TerminationReason", WYMS, "TAK", "Przyczyny rozwiązania umowy (nazwa)", Odrz,
            "Dokładna NAZWA przyczyny. Nieznana nazwa = przyczyna pusta, a wynik Success = true (błąd po cichu!). „NO SHOW” przy dacie rozwiązania = początku umowy kasuje okres etatu."),
        new("Effective as of:", "—", NIE, "NIE", "—", Pomin, "Rozwiązanie nie ma daty aktualizacji i nie podlega „Dacie odcięcia”."),
        new("Event Reason", "—", NIE, "NIE", "—", Pomin, "", "Czy to źródło przyczyny rozwiązania?"),
        new("(brak w źródle)", "RejestracjaZus", OPC, "NIE", "—", "Domyślnie true – wyrejestrowanie z ZUS",
            "false usuwa tytuł ubezpieczenia z zapisu historii (Analiza 3.6)."),
    };

    static readonly (string Status, string Opis, Color Kolor)[] Legenda =
    {
        (WYM, "Pole musi być w JSON i nie może być null – inaczej całe żądanie jest odrzucane.", Color.FromArgb(0xF8, 0xCB, 0xAD)),
        (WYMS, "Jak wyżej + wartość musi istnieć w słowniku enova, inaczej wyjątek i nic się nie zapisuje (cała metoda = jedna transakcja).", Color.FromArgb(0xF4, 0xA0, 0x8A)),
        (WAR, "Wymagane tylko w określonej sytuacji (np. gdy wysyłana jest sekcja Zatrudnienie).", Color.FromArgb(0xFF, 0xE6, 0x99)),
        (NADP, "Można pominąć, ale brak ZERUJE wartość w enova – wysyłać zawsze.", Color.FromArgb(0xFF, 0xD9, 0x66)),
        (OPCS, "Można pominąć; jeśli wysłane – wartość musi istnieć w słowniku enova.", Color.FromArgb(0xDD, 0xEB, 0xF7)),
        (OPC, "Można pominąć; brak = pole w enova bez zmian.", Color.FromArgb(0xE2, 0xEF, 0xDA)),
        (NIE, "Dodatek nie ma takiego pola – wartość nie trafi do enova.", Color.FromArgb(0xD9, 0xD9, 0xD9)),
    };

    static Color KolorStatusu(string s) => Legenda.FirstOrDefault(l => l.Status == s).Kolor;

    static void Naglowek(Worksheet ws, string[] nag, int[] szer)
    {
        for (int c = 0; c < nag.Length; c++)
        {
            var cell = ws.Cells[0, c];
            cell.Value = nag[c];
            cell.Font.Bold = true;
            cell.Fill.BackgroundColor = Color.FromArgb(0x1F, 0x4E, 0x78);
            cell.Font.Color = Color.White;
            ws.Columns[c].WidthInCharacters = szer[c];
        }
    }

    static void Formatuj(Worksheet ws, int kolumn, int wierszy)
    {
        var z = ws.Range.FromLTRB(0, 0, kolumn - 1, wierszy - 1);
        z.Alignment.WrapText = true;
        z.Alignment.Vertical = SpreadsheetVerticalAlignment.Top;
        z.Borders.SetAllBorders(Color.FromArgb(0xBF, 0xBF, 0xBF), BorderLineStyle.Thin);
        ws.AutoFilter.Apply(z);
        ws.FreezeRows(0);
    }

    static void ArkuszPol(Workbook wb, string nazwa, string metoda, List<Pole> pola, bool pierwszy)
    {
        var ws = pierwszy ? wb.Worksheets[0] : wb.Worksheets.Add(nazwa);
        ws.Name = nazwa;
        string[] nag = { "Lp", "Pole w systemie źródłowym", "Pole w ramce (" + metoda + ")", "Status", "Musi być wysłane?",
                         "Słownik / obiekt w enova", "Skutek braku / błędu", "Format / uwagi", "Do uzgodnienia", "Odpowiedź klienta" };
        int[] szer = { 5, 28, 34, 22, 16, 28, 34, 60, 34, 30 };
        Naglowek(ws, nag, szer);
        int r = 1;
        foreach (var p in pola)
        {
            string[] w = { r.ToString(), p.Zrodlo, p.Ramka, p.Status, p.Wymagane, p.Slownik, p.SkutekBraku, p.Uwagi, p.DoUzgodnienia, "" };
            for (int c = 0; c < w.Length; c++) ws.Cells[r, c].Value = c == 0 ? r : w[c];
            ws.Cells[r, 3].Fill.BackgroundColor = KolorStatusu(p.Status);
            ws.Cells[r, 3].Font.Bold = true;
            if (p.DoUzgodnienia != "") ws.Cells[r, 8].Fill.BackgroundColor = Color.FromArgb(0xFF, 0xF2, 0xCC);
            r++;
        }
        Formatuj(ws, nag.Length, r);
    }

    public static void ZapiszXlsx(string katalog)
    {
        using var wb = new Workbook();
        ArkuszPol(wb, "UpsertEmployee", "UpsertEmployee", Pracownik(), true);
        ArkuszPol(wb, "UpsertTermination", "UpsertTermination", Rozwiazanie(), false);

        // Legenda
        var wl = wb.Worksheets.Add("Legenda");
        Naglowek(wl, new[] { "Status", "Znaczenie", "Liczba pól – pracownik", "Liczba pól – rozwiązanie" }, new[] { 26, 90, 22, 24 });
        int i = 1;
        foreach (var l in Legenda)
        {
            wl.Cells[i, 0].Value = l.Status;
            wl.Cells[i, 0].Fill.BackgroundColor = l.Kolor;
            wl.Cells[i, 0].Font.Bold = true;
            wl.Cells[i, 1].Value = l.Opis;
            wl.Cells[i, 2].Value = Pracownik().Count(p => p.Status == l.Status);
            wl.Cells[i, 3].Value = Rozwiazanie().Count(p => p.Status == l.Status);
            i++;
        }
        wl.Cells[i + 1, 0].Value = "Uwaga";
        wl.Cells[i + 1, 0].Font.Bold = true;
        wl.Cells[i + 1, 1].Value = "Wymagalność (Required.Always) jest sprawdzana przy wywołaniu przez MethodInvoker (Newtonsoft). " +
                                   "Przy Dynamic WebAPI (System.Text.Json) braki nie są odrzucane na wejściu, tylko kończą się błędem lub wartością domyślną w trakcie zapisu. " +
                                   "Stan na podstawie analizy kodu dodatku – niezweryfikowany na żywym serwerze.";
        wl.Range.FromLTRB(0, 0, 3, i + 1).Alignment.WrapText = true;
        wl.Range.FromLTRB(0, 0, 3, i + 1).Alignment.Vertical = SpreadsheetVerticalAlignment.Top;

        // Slowniki
        var wsl = wb.Worksheets.Add("Słowniki enova");
        Naglowek(wsl, new[] { "Lp", "Słownik / obiekt w enova", "Klucz dopasowania", "Pole źródłowe", "Skutek braku pozycji", "Gotowe? (TAK/NIE)" }, new[] { 5, 40, 22, 28, 50, 18 });
        (string, string, string, string)[] sl =
        {
            ("Wydziały", "kod", "KOD", Wyj),
            ("Definicje stanowisk (HR)", "nazwa (dokładna)", "Positionh Title PL", Wyj),
            ("Urzędy skarbowe", "kod", "Tax Office", Wyj + " (gdy pole wysłane)"),
            ("Przyczyny rozwiązania umowy", "nazwa (dokładna)", "TerminationReason", "BŁĄD PO CICHU: przyczyna pusta, Success = true"),
            ("Cecha „Numer koncernowy” (Pracownik)", "nazwa cechy", "Person ID (PKID)", Wyj + " (gdy pole wysłane)"),
            ("Cecha „Typ pracownika” (historia pracownika)", "nazwa cechy", "Employee Class", Wyj + " (gdy pole wysłane)"),
            ("Banki", "kod", "— (KodBanku, brak w źródle)", Wyj + " (gdy pole wysłane)"),
        };
        i = 1;
        foreach (var s in sl) { wsl.Cells[i, 0].Value = i; wsl.Cells[i, 1].Value = s.Item1; wsl.Cells[i, 2].Value = s.Item2; wsl.Cells[i, 3].Value = s.Item3; wsl.Cells[i, 4].Value = s.Item4; i++; }
        Formatuj(wsl, 6, i);

        // Mapowania wartosci
        var wm = wb.Worksheets.Add("Mapowania");
        Naglowek(wm, new[] { "Pole w ramce", "Wartość w ramce", "Znaczenie w enova", "Wartość w systemie źródłowym" }, new[] { 26, 16, 40, 34 });
        var mapy = new List<(string, string, string)>();
        string[] woj = { "nieokreślone", "dolnośląskie", "kujawsko-pomorskie", "lubelskie", "lubuskie", "łódzkie", "małopolskie", "mazowieckie", "opolskie",
                         "podkarpackie", "podlaskie", "pomorskie", "śląskie", "świętokrzyskie", "warmińsko-mazurskie", "wielkopolskie", "zachodniopomorskie" };
        for (int k = 0; k < woj.Length; k++) mapy.Add(("Wojewodztwo (Region)", k.ToString(), woj[k]));
        string[] typ = { "Brak", "Na czas nieokreślony", "Na okres próbny", "Na czas określony", "Na czas wykonywania pracy", "Na okres zastępstwa",
                         "Na okres trwania mandatu", "Na czas pełnienia funkcji", "Do dnia porodu", "Na czas określony – dorywczy/sezonowy", "Na okres próbny do dnia porodu" };
        for (int k = 0; k < typ.Length; k++) mapy.Add(("TypUmowy (Contract Type)", k.ToString(), typ[k]));
        mapy.Add(("Wymiar (Standard Weekly Hours)", "1/1", "40 h tygodniowo"));
        mapy.Add(("Wymiar (Standard Weekly Hours)", "3/4", "30 h tygodniowo"));
        mapy.Add(("Wymiar (Standard Weekly Hours)", "1/2", "20 h tygodniowo"));
        mapy.Add(("Wymiar (Standard Weekly Hours)", "1/4", "10 h tygodniowo"));
        mapy.Add(("DaneKontaktowe.Rodzaj", "0", "Telefon stacjonarny"));
        mapy.Add(("DaneKontaktowe.Rodzaj", "1", "Telefon komórkowy"));
        mapy.Add(("DaneKontaktowe.Rodzaj", "2", "E-mail"));
        mapy.Add(("Podatki.IdentyfikatorPodatkowy", "1", "PESEL"));
        mapy.Add(("Podatki.IdentyfikatorPodatkowy", "2", "NIP"));
        mapy.Add(("Zatrudnienie.ZatrudnienieNaPodstawie", "1", "Umowa o pracę"));
        mapy.Add(("Zatrudnienie.RodzajZatrudnienia", "0", "Nie dotyczy"));
        i = 1;
        foreach (var m in mapy) { wm.Cells[i, 0].Value = m.Item1; wm.Cells[i, 1].Value = m.Item2; wm.Cells[i, 2].Value = m.Item3; i++; }
        Formatuj(wm, 4, i);

        // Pytania
        var wp = wb.Worksheets.Add("Pytania do klienta");
        Naglowek(wp, new[] { "Lp", "Pytanie", "Dotyczy pól", "Odpowiedź", "Data / kto" }, new[] { 5, 70, 34, 50, 16 });
        (string, string)[] pyt =
        {
            ("Które pole jest kluczem pracownika (Kod): Person ID, User ID czy KOD? Komunikat błędu dodatku sugeruje Person ID.", "Person ID, User ID, KOD"),
            ("Czy KOD to kod jednostki organizacyjnej (wydziału)?", "KOD"),
            ("Stanowisko z „Positionh Title PL” czy z „Job Title”?", "Positionh Title PL, Job Title"),
            ("Recruit Date → data rozpoczęcia pracy czy data zawarcia umowy?", "Recruit Date"),
            ("Employee Class → cecha „Typ pracownika”?", "Employee Class"),
            ("Źródło ma jeden adres – do którego adresu w enova (zamieszkania / zameldowania / korespondencyjny), czy do wszystkich trzech?", "pola adresowe"),
            ("Gender i National Id Card Type dla cudzoziemców bez PESEL – dodatek ich nie obsługuje; czy rozszerzyć?", "Gender, National Id Card Type"),
            ("Event Reason – czy to źródło przyczyny rozwiązania umowy?", "Event Reason, TerminationReason"),
        };
        i = 1;
        foreach (var p in pyt) { wp.Cells[i, 0].Value = i; wp.Cells[i, 1].Value = p.Item1; wp.Cells[i, 2].Value = p.Item2; i++; }
        Formatuj(wp, 5, i);

        wb.Worksheets.ActiveWorksheet = wb.Worksheets[0];
        wb.SaveDocument(Path.Combine(katalog, "Wymagalność pól.xlsx"), DocumentFormat.Xlsx);
    }
}
