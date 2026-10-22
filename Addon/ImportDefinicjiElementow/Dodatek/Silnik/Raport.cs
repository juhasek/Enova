using System.Net;
using System.Text;

namespace ImportDefinicjiElementow.Silnik;

/// <summary>Wynik zapisu jednej definicji.</summary>
public sealed record WynikZapisuDefinicji(AnalizaDefinicji Definicja, bool Zapisana, string Blad, int ZmienionychPol);

public sealed class WynikZapisu
{
    public bool Zapisano;
    public string BladOgolny;
    public DateTime Data = DateTime.Now;
    public string Operator;
    public string Baza;
    public List<WynikZapisuDefinicji> Definicje = new();
}

/// <summary>Podgląd przed zapisem (Markdown do okna enova) i raport końcowy (HTML do pobrania).</summary>
public static class Raport
{
    static readonly Dictionary<StatusDefinicji, string> Statusy = new()
    {
        [StatusDefinicji.Nowa] = "nowa",
        [StatusDefinicji.Aktualizacja] = "do aktualizacji",
        [StatusDefinicji.BezZmian] = "bez zmian",
        [StatusDefinicji.Pominieta] = "pominięta",
        [StatusDefinicji.Blad] = "błąd",
    };

    public static string Status(StatusDefinicji s) => Statusy[s];

    public static string Podglad(WynikAnalizy w, Schemat s, int maksRoznic = 15)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"**Plik:** {w.Plik.Definicje.Count} definicji, zapisany {Data(w.Plik.DataEksportu)}, tryb: **{(w.Tryb == TrybImportu.TylkoNowe ? "tylko nowe" : "nowe i aktualizacja")}**, licencja programu: {w.Licencja}.");
        sb.AppendLine();
        sb.AppendLine($"Nowe: **{w.Liczba(StatusDefinicji.Nowa)}** · do aktualizacji: **{w.Liczba(StatusDefinicji.Aktualizacja)}** · bez zmian: {w.Liczba(StatusDefinicji.BezZmian)} · pominięte: {w.Liczba(StatusDefinicji.Pominieta)} · z błędami: **{w.Liczba(StatusDefinicji.Blad)}**");
        foreach (var o in w.Ostrzezenia)
            sb.AppendLine().AppendLine("⚠ " + o);
        foreach (var d in w.Definicje)
        {
            sb.AppendLine();
            sb.AppendLine($"**{d.Numer}. {Md(d.Nazwa == "" ? "(bez nazwy)" : d.Nazwa)}** – {Status(d.Status)}");
            foreach (var k in d.Komunikaty)
                sb.AppendLine($"- {(k.Poziom == "blad" ? "❌" : "⚠")} {Md(s.Etykieta(k.Sciezka))}: {Md(k.Tekst)}");
            foreach (var r in d.Roznice.Take(maksRoznic))
                sb.AppendLine($"- {Md(r.Etykieta)}: {Md(r.Obecna)} → **{Md(r.Nowa)}**");
            if (d.Roznice.Count > maksRoznic)
                sb.AppendLine($"- … i {d.Roznice.Count - maksRoznic} innych zmian (pełna lista w raporcie)");
        }
        sb.AppendLine();
        var doZapisu = w.DoZapisu.Count();
        sb.AppendLine(doZapisu == 0
            ? "**Nie ma nic do zapisania.**"
            : $"Zapisać **{doZapisu}** definicji w jednej transakcji? Definicje z błędami i pominięte nie zostaną zapisane.");
        return sb.ToString();
    }

    public static string Html(WynikAnalizy w, WynikZapisu z, Schemat s)
    {
        var sb = new StringBuilder();
        sb.Append("<!doctype html><html lang=\"pl\"><head><meta charset=\"utf-8\"><title>Raport importu definicji elementów wynagrodzenia</title><style>")
          .Append("body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#1f2328}h1{font-size:20px}h2{font-size:16px;margin-top:24px}")
          .Append("table{border-collapse:collapse;width:100%;margin:8px 0}th,td{border:1px solid #d0d7de;padding:4px 8px;text-align:left;vertical-align:top;font-size:13px}")
          .Append("th{background:#f3f4f6}.blad{color:#b42318}.ok{color:#1a7f37}.ostrz{color:#9a6700}.uwagi{white-space:pre-wrap;background:#f6f8fa;padding:6px 8px}")
          .Append("</style></head><body>");
        sb.Append("<h1>Raport importu definicji elementów wynagrodzenia</h1>");
        sb.Append($"<p>Data: {z.Data:yyyy-MM-dd HH:mm} · baza: {H(z.Baza)} · operator: {H(z.Operator)} · tryb: {(w.Tryb == TrybImportu.TylkoNowe ? "tylko nowe" : "nowe i aktualizacja")} · licencja: {H(w.Licencja)}<br>");
        sb.Append($"Plik: {w.Plik.Definicje.Count} definicji, zapisany {H(Data(w.Plik.DataEksportu))}, wersja formularza {H(w.Plik.WersjaSchematu)}, enova {H(w.Plik.WersjaEnova)}</p>");
        sb.Append(z.Zapisano
            ? $"<p class=\"ok\"><b>Zapisano {z.Definicje.Count(d => d.Zapisana)} definicji.</b></p>"
            : $"<p class=\"blad\"><b>Nic nie zapisano.</b> {H(z.BladOgolny)}</p>");
        foreach (var o in w.Ostrzezenia)
            sb.Append($"<p class=\"ostrz\">⚠ {H(o)}</p>");

        sb.Append("<table><tr><th>#</th><th>Nazwa</th><th>Skrót</th><th>Rodzaj</th><th>Status</th><th>Wynik</th></tr>");
        foreach (var d in w.Definicje)
        {
            var wz = z.Definicje.FirstOrDefault(x => x.Definicja == d);
            var wynik = wz == null ? "" : wz.Zapisana ? $"<span class=\"ok\">zapisana (zmienione pola: {wz.ZmienionychPol})</span>" : $"<span class=\"blad\">{H(wz.Blad)}</span>";
            sb.Append($"<tr><td>{d.Numer}</td><td>{H(d.Nazwa)}</td><td>{H(d.Skrot)}</td><td>{H(s.EtykietaWartosci("RodzajZrodla", System.Text.Json.Nodes.JsonValue.Create(d.Rodzaj)))}</td><td>{Status(d.Status)}</td><td>{wynik}</td></tr>");
        }
        sb.Append("</table>");

        foreach (var d in w.Definicje)
        {
            sb.Append($"<h2>{d.Numer}. {H(d.Nazwa == "" ? "(bez nazwy)" : d.Nazwa)} – {Status(d.Status)}</h2>");
            if (d.Komunikaty.Count > 0)
            {
                sb.Append("<ul>");
                foreach (var k in d.Komunikaty)
                    sb.Append($"<li class=\"{(k.Poziom == "blad" ? "blad" : "ostrz")}\">{H(s.Etykieta(k.Sciezka))}: {H(k.Tekst)}</li>");
                sb.Append("</ul>");
            }
            if (d.Roznice.Count > 0)
            {
                sb.Append("<table><tr><th>Pole</th><th>Obecnie w enova</th><th>Z pliku</th></tr>");
                foreach (var r in d.Roznice)
                    sb.Append($"<tr><td>{H(r.Etykieta)}</td><td>{H(r.Obecna)}</td><td><b>{H(r.Nowa)}</b></td></tr>");
                sb.Append("</table>");
            }
            if (!string.IsNullOrWhiteSpace(d.Uwagi))
                sb.Append($"<p><b>Uwagi klienta dla wdrożeniowca:</b></p><div class=\"uwagi\">{H(d.Uwagi)}</div>");
        }
        sb.Append("</body></html>");
        return sb.ToString();
    }

    static string Data(string iso) => DateTime.TryParse(iso, out var d) ? d.ToLocalTime().ToString("yyyy-MM-dd HH:mm") : iso ?? "";

    static string H(string t) => WebUtility.HtmlEncode(t ?? "");

    static string Md(string t) => (t ?? "").Replace("*", "\\*").Replace("_", "\\_");
}
