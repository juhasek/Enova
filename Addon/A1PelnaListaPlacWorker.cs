using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Soneta.Business;
using Soneta.Kadry;
using Soneta.Place;
using Soneta.Types;
using DevExpress.Spreadsheet;

[assembly: Worker<A1.Rozszerzenia.A1PelnaListaPlacWorker, ListaPlac>]

namespace A1.Rozszerzenia
{
    // Czynnosc "Pelna lista plac -> XLSX" na liscie Place -> Listy plac (dziala na
    // zaznaczonych pozycjach). Buduje plik .xlsx OD ZERA biblioteka DevExpress.Spreadsheet
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
        [Context]
        public ListaPlac[] ZaznaczoneListyPlac { get; set; }

        public bool IsVisibleGenerujXlsx() => ZaznaczoneListyPlac != null && ZaznaczoneListyPlac.Length > 0;
        public bool IsEnabledGenerujXlsx() => ZaznaczoneListyPlac != null && ZaznaczoneListyPlac.Length > 0;

        const int LiczbaKolumnTekst = 3;
        const double MaxSzerokoscZnaki = 42;
        const double MinSzerokoscLiczbaZnaki = 12;

        [Action("Pełna lista płac (XLSX)")]
        public object GenerujXlsx()
        {
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

            // 3. Naglowki.
            var naglowki = new List<string> { "Kod", "Imię i Nazwisko", "Wydział" };
            foreach (DefinicjaElementu d in defs) naglowki.Add(NazwaDef(d));
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

            // 4. Wiersze danych.
            var wiersze = new List<object[]>();
            foreach (Wyplata w in wyplaty)
            {
                Pracownik pr = w.Pracownik as Pracownik;
                var row = new object[nKol];
                row[0] = Txt(() => pr != null ? pr.Kod : "");
                row[1] = Txt(() => pr != null ? pr.NazwiskoImię : "");
                row[2] = Txt(() => Wydzial(pr, w));

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

                int c = LiczbaKolumnTekst;
                for (int i = 0; i < defs.Count; i++) row[c++] = wart[i];
                row[c++] = emP; row[c++] = emF;
                row[c++] = reP; row[c++] = reF;
                row[c++] = chP; row[c++] = chF;
                row[c++] = wyP; row[c++] = wyF;
                row[c++] = zdP; row[c++] = zdF;
                row[c++] = fp; row[c++] = fgsp; row[c++] = fep;
                row[c++] = ppkP; row[c++] = ppkF;
                row[c++] = pit;
                row[c++] = Dec(() => w.Wartosc.Value);

                wiersze.Add(row);
            }
            wiersze.Sort((a, b) => string.Compare((string)(a[1] ?? ""), (string)(b[1] ?? ""), StringComparison.CurrentCulture));

            byte[] plik = Buduj(naglowki, wiersze);
            string nazwa = "A1_Pelna_Lista_Plac_" + Date.Today.ToString("yyyyMMdd") + ".xlsx";
            return new NamedStream(nazwa, plik);
        }

        static byte[] Buduj(List<string> naglowki, List<object[]> wiersze)
        {
            int nKol = naglowki.Count;
            using (var wb = new Workbook())
            {
                Worksheet ws = wb.Worksheets.ActiveWorksheet;
                ws.Name = "Lista płac";

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
                    object[] src = wiersze[i];
                    for (int c = 0; c < nKol; c++)
                    {
                        Cell cell = ws[r, c];
                        if (c < LiczbaKolumnTekst)
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
                    if (c >= LiczbaKolumnTekst && col.WidthInCharacters < MinSzerokoscLiczbaZnaki)
                        col.WidthInCharacters = MinSzerokoscLiczbaZnaki;
                }

                using (var ms = new MemoryStream())
                {
                    wb.SaveDocument(ms, DocumentFormat.Xlsx);
                    return ms.ToArray();
                }
            }
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
