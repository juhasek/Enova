using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Kadry;
using Soneta.Place;
using Soneta.Types;
using DevExpress.Spreadsheet;

// UWAGA - typ danych workera to TABELA "ListyPlac", a NIE wiersz "ListaPlac".
// To decyduje o tym, GDZIE pojawi sie przycisk. Klient enova (Soneta.Net.Business,
// ViewInfoWindow.MyWorkers) buduje czynnosci widoku listy z dwoch "indeksow":
//   index 0 -> typ TABELI  (ListyPlac)  -> IsEnumerableItem == true
//   index 1 -> typ WIERSZA (ListaPlac)  -> IsEnumerableItem == false
// a WorkersMenu.RenderToolbarCommands rysuje przycisk tylko gdy:
//   IsToolbarAction(akcja) && (IsEnumerableItem(index) || Mode ma SingleSession/IsolatedSession)
// Przy rejestracji na wierszu (index 1) i Mode = None przycisk NIE byl rysowany na
// widoku listy - akcja ladowala dopiero na pasku FORMULARZA konkretnej listy plac.
// Rejestracja na tabeli daje przycisk na widoku "Kadry i place/Place/Listy plac".
// Tak samo robi to sama enova: [assembly: Worker(typeof(PodsumowanieWyplatListyWorker),
// typeof(ListyPlac))] - "Podsumowanie zaznaczonych list plac", tez z [Context] ListaPlac[].
[assembly: Worker<A1.Rozszerzenia.A1PelnaListaPlacWorker, ListyPlac>]

namespace A1.Rozszerzenia
{
    // Przycisk "Pelna lista plac (XLSX)" na pasku narzedzi widoku Place -> Listy plac
    // (dziala na zaznaczonych pozycjach). Buduje plik .xlsx OD ZERA biblioteka DevExpress.Spreadsheet
    // - czysta siatka komorek, wiec NIE MA problemu ze scalaniem/rozjezdzaniem kolumn,
    // ktory wystepowal przy eksporcie report-snippetu z podgladu wydruku (XRTable -> xlsx
    // scala kolumny tekstowe o zmiennej dlugosci; patrz Raporty/A1PelnaListaPlac.md).
    //
    // Logika danych (kolumny: Kod / Imie i Nazwisko / Wydzial + po jednej na kazda
    // definicje elementu + 17 stalych kolumn ZUS/PPK/PIT, pomijanie storna, wydzial
    // historyczny wg daty wyplaty, kwota do wyplaty = Wyplata.Wartosc) przeniesiona
    // 1:1 z Raporty/A1PelnaListaPlacSnippet.
    public class A1PelnaListaPlacWorker
    {
        // Zaznaczone wiersze widoku - klient wklada je do kontekstu okna jako ListaPlac[]
        // (ViewInfoWindow: context[selectedRows.GetType()] = selectedRows).
        [Context]
        public ListaPlac[] ZaznaczoneListyPlac { get; set; }

        // Parametry czynnosci - okienko z haslem do pliku. Enova sama tworzy obiekt
        // (ContextBase + ctor(Context)) i pokazuje dla niego formularz przed wykonaniem
        // akcji; szczegoly w A1HasloPlikuParams.cs. Haslo nie jest nigdzie zapisywane.
        [Context]
        public A1HasloPlikuParams Haslo { get; set; }

        // Swiadomie BEZ IsVisible*/IsEnabled* - przycisk ma byc widoczny na widoku zawsze,
        // a brak zaznaczenia obslugujemy komunikatem w samej akcji (dla paska narzedzi
        // klient i tak nie wywoluje Action.InitData, wiec predykaty bywaja pomijane).

        const double MaxSzerokoscZnaki = 42;
        const double MinSzerokoscLiczbaZnaki = 12;

        // Jeden wiersz raportu: klucze sortowania osobno, bo kolumny opisowe moga byc
        // wylaczone w konfiguracji i nie da sie sortowac "po kolumnie nr 1".
        class Wiersz
        {
            public string Kod;
            public string Nazwisko;
            public object[] Komorki;
        }

        // ToolbarWithText -> przycisk z opisem na pasku narzedzi widoku listy plac.
        // Menu zostawione jako zapasowe wejscie (ta sama czynnosc widoczna tez w "Czynnosci").
        [Action("Pełna lista płac (XLSX)",
                Target = ActionTarget.ToolbarWithText | ActionTarget.Menu,
                Icon = ActionIcon.ExcelPreview,
                Priority = 100)]
        public object GenerujXlsx()
        {
            if (ZaznaczoneListyPlac == null || ZaznaczoneListyPlac.Length == 0)
                return new MessageBoxInformation
                {
                    Caption = "Pełna lista płac (XLSX)",
                    Text = "Zaznacz na liście co najmniej jedną listę płac.",
                };

            // 0a. Haslo do pliku z okienka parametrow. Gdyby framework nie wstrzyknal
            //     parametrow (np. wywolanie akcji z pominieciem formularza), traktujemy
            //     to jak "bez hasla" zamiast wywalac sie z NullReferenceException.
            string hasloPliku = Haslo != null && Haslo.CzyChronic ? Haslo.Haslo : "";
            if (Haslo != null && !Haslo.HaslaZgodne)
                return new MessageBoxInformation
                {
                    Caption = "Pełna lista płac (XLSX)",
                    Text = "Hasło i jego powtórzenie różnią się. Wpisz to samo hasło w obu polach "
                         + "albo zostaw oba puste, żeby wygenerować plik bez zabezpieczenia.",
                };

            // 0. Ustawienia z Narzedzia -> Opcje -> A1Testy -> Konfiguracja raportu placowego.
            var ust = new A1RaportPlacUstawienia(ZaznaczoneListyPlac[0].Session);

            // 1. Zebranie wszystkich wyplat z zaznaczonych list plac.
            var wyplaty = new List<Wyplata>();
            foreach (ListaPlac lp in ZaznaczoneListyPlac)
                foreach (Wyplata w in lp.Wyplaty)
                    wyplaty.Add(w);

            // 2. Kolumny dynamiczne - unikalne definicje elementow (bez storna), alfabetycznie.
            var defs = new List<DefinicjaElementu>();
            foreach (Wyplata w in wyplaty)
                foreach (WypElement el in w.Elementy)
                {
                    if (el.RozliczenieStorna) continue;
                    DefinicjaElementu d = el.Definicja;
                    if (d != null && !defs.Contains(d)) defs.Add(d);
                }
            defs.Sort((a, b) => string.Compare(NazwaDef(a), NazwaDef(b), StringComparison.CurrentCulture));

            // 3. Naglowki. Kolumny opisowe = sloty z konfiguracji (zrodlo + parametr + naglowek);
            //    slot ze zrodlem "Brak" jest pomijany, kolejnosc slotow = kolejnosc kolumn.
            var zrodla = new List<A1ZrodloKolumny>();
            var parametry = new List<string>();
            var naglowki = new List<string>();
            for (int slot = 1; slot <= A1RaportPlacUstawienia.LiczbaSlotow; slot++)
            {
                A1ZrodloKolumny zr = ust.ZrodloSlotu(slot);
                if (zr == A1ZrodloKolumny.Brak) continue;
                zrodla.Add(zr);
                parametry.Add(ust.ParametrSlotu(slot));
                naglowki.Add(ust.NaglowekSlotu(slot));
            }
            int liczbaKolumnTekst = zrodla.Count;

            foreach (DefinicjaElementu d in defs) naglowki.Add(NazwaDef(d));

            bool skladki = ust.PokazSkladki;
            if (skladki)
                naglowki.AddRange(new[]
                {
                    "Emerytalna (pracownik)", "Emerytalna (pracodawca)",
                    "Rentowa (pracownik)", "Rentowa (pracodawca)",
                    "Chorobowa (pracownik)", "Chorobowa (pracodawca)",
                    "Wypadkowa (pracownik)", "Wypadkowa (pracodawca)",
                    "Zdrowotna (pracownik)", "Zdrowotna (pracodawca)",
                    "Fundusz Pracy", "FGŚP", "FEP",
                    "PPK (pracownik)", "PPK (pracodawca)",
                    "Zaliczka na PIT", "Kwota do wypłaty",
                });
            int nKol = naglowki.Count;
            if (nKol == 0)
                return new MessageBoxInformation
                {
                    Caption = "Pełna lista płac (XLSX)",
                    Text = "Konfiguracja wyłącza wszystkie kolumny raportu, a zaznaczone listy płac nie mają "
                         + "elementów wynagrodzenia. Włącz kolumny w Narzędzia → Opcje → A1Testy → "
                         + "Konfiguracja raportu płacowego.",
                };

            // 4. Wiersze danych.
            var wiersze = new List<Wiersz>();
            foreach (Wyplata w in wyplaty)
            {
                Pracownik pr = w.Pracownik as Pracownik;
                var row = new object[nKol];

                // Klucze sortowania liczone zawsze, niezaleznie od tego, czy sa w kolumnach.
                string kod = Txt(() => pr != null ? pr.Kod : "");
                string nazwisko = Txt(() => pr != null ? pr.NazwiskoImię : "");

                for (int i = 0; i < zrodla.Count; i++)
                    row[i] = Wartosc(zrodla[i], parametry[i], w, pr);

                var wart = new decimal[defs.Count];
                decimal emP = 0, emF = 0, reP = 0, reF = 0, chP = 0, chF = 0, wyP = 0, wyF = 0,
                        zdP = 0, zdF = 0, fp = 0, fgsp = 0, fep = 0, ppkP = 0, ppkF = 0, pit = 0;

                foreach (WypElement el in w.Elementy)
                {
                    if (el.RozliczenieStorna) continue;
                    DefinicjaElementu d = el.Definicja;
                    if (d != null)
                    {
                        int i = defs.IndexOf(d);
                        if (i >= 0) wart[i] += el.Wartosc;
                    }
                    emP += el.Podatki.Emerytalna.Prac; emF += el.Podatki.Emerytalna.Firma;
                    reP += el.Podatki.Rentowa.Prac; reF += el.Podatki.Rentowa.Firma;
                    chP += el.Podatki.Chorobowa.Prac; chF += el.Podatki.Chorobowa.Firma;
                    wyP += el.Podatki.Wypadkowa.Prac; wyF += el.Podatki.Wypadkowa.Firma;
                    zdP += el.Podatki.Zdrowotna.Prac; zdF += el.Podatki.Zdrowotna.Firma;
                    fp += el.Podatki.FP.Skladka;
                    fgsp += el.Podatki.FGSP.Skladka;
                    fep += el.Podatki.FEP.Skladka;
                    pit += el.Podatki.ZalFIS;
                    ppkP += el.Podatki.PPK.Pracownika;
                    ppkF += el.Podatki.PPK.Pracodawcy;
                }

                int c = liczbaKolumnTekst;
                for (int i = 0; i < defs.Count; i++) row[c++] = wart[i];
                if (skladki)
                {
                    row[c++] = emP; row[c++] = emF;
                    row[c++] = reP; row[c++] = reF;
                    row[c++] = chP; row[c++] = chF;
                    row[c++] = wyP; row[c++] = wyF;
                    row[c++] = zdP; row[c++] = zdF;
                    row[c++] = fp; row[c++] = fgsp; row[c++] = fep;
                    row[c++] = ppkP; row[c++] = ppkF;
                    row[c++] = pit;
                    row[c++] = Dec(() => w.Wartosc.Value);
                }

                wiersze.Add(new Wiersz { Kod = kod, Nazwisko = nazwisko, Komorki = row });
            }

            if (ust.SortujWgKodu)
                wiersze.Sort((a, b) => string.Compare(a.Kod ?? "", b.Kod ?? "", StringComparison.CurrentCulture));
            else
                wiersze.Sort((a, b) => string.Compare(a.Nazwisko ?? "", b.Nazwisko ?? "", StringComparison.CurrentCulture));

            byte[] plik = Buduj(naglowki, wiersze, liczbaKolumnTekst, ust.NazwaArkuszaEfekt, hasloPliku);
            string nazwa = ust.PrefiksNazwyPlikuEfekt + Date.Today.ToString("yyyyMMdd") + ".xlsx";
            return new NamedStream(nazwa, plik);
        }

        static byte[] Buduj(List<string> naglowki, List<Wiersz> wiersze, int liczbaKolumnTekst,
                            string nazwaArkusza, string hasloPliku)
        {
            int nKol = naglowki.Count;
            using (var wb = new Workbook())
            {
                Worksheet ws = wb.Worksheets.ActiveWorksheet;
                ws.Name = nazwaArkusza;

                Color obram = Color.FromArgb(0xBF, 0xBF, 0xBF);
                Color tloNaglowka = Color.FromArgb(0xE6, 0xE6, 0xE6);

                for (int c = 0; c < nKol; c++)
                {
                    Cell k = ws[0, c];
                    k.Value = naglowki[c];
                    k.Font.Bold = true;
                    k.Alignment.WrapText = true;
                    k.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
                    k.Alignment.Vertical = SpreadsheetVerticalAlignment.Center;
                    k.FillColor = tloNaglowka;
                    k.Borders.SetAllBorders(obram, BorderLineStyle.Thin);
                }
                ws.Rows[0].Height = 42;

                for (int i = 0; i < wiersze.Count; i++)
                {
                    int r = 1 + i;
                    object[] src = wiersze[i].Komorki;
                    for (int c = 0; c < nKol; c++)
                    {
                        Cell cell = ws[r, c];
                        if (c < liczbaKolumnTekst)
                        {
                            cell.Value = (src[c] as string) ?? "";
                            cell.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Left;
                        }
                        else
                        {
                            cell.Value = (src[c] is decimal) ? (decimal)src[c] : 0m;
                            cell.NumberFormat = "#,##0.00";
                            cell.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Right;
                        }
                        cell.Borders.SetAllBorders(obram, BorderLineStyle.Thin);
                    }
                }

                int rOstatni = wiersze.Count;

                ws.FreezeRows(1);
                try { ws.AutoFilter.Apply(ws.Range.FromLTRB(0, 0, nKol - 1, rOstatni)); }
                catch { }

                ws.Columns.AutoFit(0, nKol - 1);
                for (int c = 0; c < nKol; c++)
                {
                    DevExpress.Spreadsheet.Column col = ws.Columns[c];
                    if (col.WidthInCharacters > MaxSzerokoscZnaki)
                        col.WidthInCharacters = MaxSzerokoscZnaki;
                    if (c >= liczbaKolumnTekst && col.WidthInCharacters < MinSzerokoscLiczbaZnaki)
                        col.WidthInCharacters = MinSzerokoscLiczbaZnaki;
                }

                using (var ms = new MemoryStream())
                {
                    if (string.IsNullOrEmpty(hasloPliku))
                    {
                        wb.SaveDocument(ms, DocumentFormat.Xlsx);
                    }
                    else
                    {
                        // Haslo do OTWARCIA pliku (szyfrowanie), nie blokada edycji.
                        // EncryptionType.Strong = AES (Excel 2007+); Compatible to stary RC4.
                        using (var enc = new EncryptionSettings(hasloPliku, EncryptionType.Strong))
                            wb.SaveDocument(ms, DocumentFormat.Xlsx, enc);
                    }
                    return ms.ToArray();
                }
            }
        }

        // Tresc jednej kolumny opisowej wg zrodla wybranego w konfiguracji.
        // "Parametr" ma znaczenie tylko dla zrodel cechowych (nazwa cechy).
        static string Wartosc(A1ZrodloKolumny zr, string parametr, Wyplata w, Pracownik pr)
        {
            switch (zr)
            {
                case A1ZrodloKolumny.KodPracownika:
                    return Txt(() => pr != null ? pr.Kod : "");
                case A1ZrodloKolumny.NazwiskoImie:
                    return Txt(() => pr != null ? pr.NazwiskoImię : "");
                case A1ZrodloKolumny.Nazwisko:
                    return Txt(() => pr != null ? pr.Nazwisko : "");
                case A1ZrodloKolumny.Imie:
                    return Txt(() => pr != null ? pr.Imie : "");
                case A1ZrodloKolumny.Wydzial:
                    return Txt(() => Wydzial(pr, w));
                case A1ZrodloKolumny.Stanowisko:
                    return Txt(() => Stanowisko(pr, w));
                case A1ZrodloKolumny.Pesel:
                    return Txt(() => pr != null ? pr.PESEL : "");
                case A1ZrodloKolumny.NumerListyPlac:
                    return Txt(() => w.ListaPlac != null ? w.ListaPlac.Numer.Pelny : "");
                case A1ZrodloKolumny.DefinicjaListyPlac:
                    return Txt(() => w.ListaPlac != null && w.ListaPlac.Definicja != null
                                     ? w.ListaPlac.Definicja.Nazwa : "");
                case A1ZrodloKolumny.DataWyplaty:
                    return Txt(() => w.Data == Date.Empty ? "" : w.Data.ToString("yyyy-MM-dd"));
                case A1ZrodloKolumny.OkresWyplaty:
                    // Okres jest na liscie plac (Wyplata.Okres jest protected).
                    return Txt(() => w.ListaPlac == null || w.ListaPlac.Okres.From == Date.Empty
                                     ? "" : w.ListaPlac.Okres.From.ToString("yyyy-MM"));
                case A1ZrodloKolumny.CechaPracownika:
                    return Cecha(pr, parametr);
                case A1ZrodloKolumny.CechaWyplaty:
                    return Cecha(w, parametr);
                default:
                    return "";
            }
        }

        // Cecha (Feature) obiektu - Row ma indekser po nazwie cechy.
        // Kwalifikacja Soneta.Business.Row, bo DevExpress.Spreadsheet tez ma typ Row.
        static string Cecha(Soneta.Business.Row row, string nazwaCechy)
        {
            if (row == null) return "";
            if (string.IsNullOrEmpty(nazwaCechy) || nazwaCechy.Trim().Length == 0)
                return "[BŁĄD: nie podano nazwy cechy]";
            try
            {
                object v = row[nazwaCechy.Trim()];
                return v == null ? "" : v.ToString();
            }
            catch (Exception ex) { return "[BŁĄD: " + ex.Message + "]"; }
        }

        static string Stanowisko(Pracownik pr, Wyplata w)
        {
            if (pr == null) return "";
            Date data = w.Data;
            if (data == Date.Empty) data = Date.Today;
            PracHistoria ph = pr.Historia[data];
            return ph != null && ph.Etat != null ? (ph.Etat.Stanowisko ?? "") : "";
        }

        static string NazwaDef(DefinicjaElementu d)
        {
            if (d == null) return "";
            string n = d.Nazwa;
            return string.IsNullOrEmpty(n) ? (d.Kod ?? "") : n;
        }

        static string Wydzial(Pracownik pr, Wyplata w)
        {
            if (pr == null) return "";
            Date data = w.Data;
            if (data == Date.Empty) data = Date.Today;
            PracHistoria ph = pr.Historia[data];
            Wydzial wydz = ph != null && ph.Etat != null ? ph.Etat.Wydzial : null;
            return wydz != null ? (wydz.Nazwa ?? "") : "";
        }

        static string Txt(Func<string> f)
        {
            try { return f() ?? ""; }
            catch (Exception ex) { return "[BŁĄD: " + ex.Message + "]"; }
        }

        static decimal Dec(Func<decimal> f)
        {
            try { return f(); }
            catch { return 0m; }
        }
    }
}
