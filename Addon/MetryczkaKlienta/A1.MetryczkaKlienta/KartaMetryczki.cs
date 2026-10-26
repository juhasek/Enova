using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Net.WebUtility;

namespace A1.Metryczka;

/// <summary>
/// Karta klienta z cech TST_ jako HTML (element Html formularza): pasek z danymi klienta i kartami kategorii.
/// Przeglądarka enova nie obsługuje zmiennych CSS (var(--x)) ani atrybutu style - kolory tylko w klasach &lt;style&gt;.
/// </summary>
static class KartaMetryczki
{
    // Cechy pokazane w pasku nagłówka zamiast na karcie "Klient".
    static readonly string[] WNaglowku =
        { "TST_NazwaKlienta", "TST_NrKlienta", "TST_NazwaBazySQL", "TST_JGSAP", "TST_FTE", "TST_LiczbaKartotek", "TST_Specjalista" };

    // Kolory kart w kolejności kategorii arkusza; od 9. kategorii kolory powtarzają się od początku listy.
    static readonly string[] Kolory = { "#00704a", "#1565c0", "#6a1b9a", "#c2185b", "#ef6c00", "#00838f", "#5d4037", "#455a64" };

    const string Styl = @"<style>
*{box-sizing:border-box}
.mk{font:14px/1.4 'Segoe UI',Roboto,Arial,sans-serif;color:#1f2a30;background:#f4f6f7;padding:12px}
.hero{display:flex;flex-wrap:wrap;gap:16px;align-items:center;background:linear-gradient(120deg,#00704a,#0a8f63);color:#fff;border-radius:14px;padding:18px 22px}
.hero h1{margin:0;font-size:22px;font-weight:600}
.sub{opacity:.85;font-size:13px}
.kpis{display:flex;gap:10px;margin-left:auto}
.kpi{background:rgba(255,255,255,.14);border-radius:10px;padding:8px 14px;min-width:96px}
.kpi b{display:block;font-size:20px;font-weight:600}
.kpi span{font-size:11px;text-transform:uppercase;letter-spacing:.04em;opacity:.85}
.prog{flex-basis:100%;display:flex;align-items:center;gap:10px;font-size:12px}
.bar{flex:1;height:6px;background:rgba(255,255,255,.25);border-radius:3px;overflow:hidden}
.bar i{display:block;height:100%;background:#fff}
.grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(330px,1fr));gap:14px;margin-top:14px}
.card{background:#fff;border-radius:12px;border:1px solid #e3e8eb;border-top:4px solid #00704a;padding:12px 16px 8px}
.card h2{margin:0 0 6px;font-size:13px;text-transform:uppercase;letter-spacing:.05em;color:#00704a;display:flex;justify-content:space-between}
.card h2 small{color:#6b7780;font-weight:400;text-transform:none;letter-spacing:0}
.row{display:flex;justify-content:space-between;gap:12px;padding:6px 0;border-top:1px solid #e3e8eb}
.row:first-of-type{border-top:0}
.k{color:#6b7780}
.v{font-weight:600;text-align:right}
.tak,.brak{display:inline-block;border-radius:999px;padding:1px 10px;font-size:12px;font-weight:600;white-space:nowrap}
.tak{background:#e6f4ee;color:#00704a}
.brak{background:#fff4e0;color:#b26a00}
.legend{margin-top:12px;font-size:12px;color:#6b7780;display:flex;gap:14px;flex-wrap:wrap}
";

    public static string Html(IList<CechaMetryczki> cechy)
    {
        if (cechy.Count == 0)
            return "<p>Brak cech globalnych z prefiksem TST_ – Ogólne → Metryczka klienta (TST_) → Czynności → Utwórz cechy testowe.</p>";

        // Kolejność wierszy arkusza metryczki; cechy dodane ręcznie na końcu.
        var wgArkusza = cechy.OrderBy(c => DefinicjeCech.Kolejnosc(c.Nazwa)).ToList();
        var wartosci = cechy.ToDictionary(c => c.Nazwa, c => c.Wartosc);
        string W(string nazwa) => wartosci.TryGetValue(nazwa, out string v) && v != null ? HtmlEncode(v) : null;

        var kategorie = wgArkusza.Where(c => !WNaglowku.Contains(c.Nazwa)).GroupBy(c => c.Kategoria).ToList();
        int uzupelnione = cechy.Count(c => c.Uzupelniona);
        int procent = 100 * uzupelnione / cechy.Count;

        var sb = new StringBuilder(Styl);
        for (int i = 0; i < kategorie.Count; i++)
            sb.Append($".c{i}{{border-top-color:{Kolory[i % Kolory.Length]}}}.c{i} h2{{color:{Kolory[i % Kolory.Length]}}}\n");
        sb.Append($".bar i{{width:{procent}%}}\n</style><div class=\"mk\">");

        sb.Append($"<div class=\"hero\"><div><h1>{W("TST_NazwaKlienta") ?? "Klient"}</h1><div class=\"sub\">")
          .Append($"Nr klienta {W("TST_NrKlienta") ?? "<i>brak</i>"} · baza SQL: {W("TST_NazwaBazySQL") ?? "<i>brak</i>"}")
          .Append($" · JG SAP: {W("TST_JGSAP") ?? "<i>brak</i>"}</div></div><div class=\"kpis\">")
          .Append(Kpi(W("TST_FTE"), "FTE")).Append(Kpi(W("TST_LiczbaKartotek"), "Kartotek")).Append(Kpi(W("TST_Specjalista"), "Specjalista"))
          .Append($"</div><div class=\"prog\">Uzupełniono {uzupelnione} z {cechy.Count} pozycji <div class=\"bar\"><i></i></div> {procent}%</div></div>");

        sb.Append("<div class=\"grid\">");
        for (int i = 0; i < kategorie.Count; i++)
        {
            var k = kategorie[i];
            sb.Append($"<div class=\"card c{i}\"><h2>{HtmlEncode(k.Key)} <small>{k.Count(c => c.Uzupelniona)}/{k.Count()}</small></h2>");
            foreach (CechaMetryczki c in k)
                sb.Append($"<div class=\"row\"><span class=\"k\">{HtmlEncode(c.ZakresDanych)}</span><span class=\"v\">{Wartosc(c.Wartosc)}</span></div>");
            sb.Append("</div>");
        }
        sb.Append("</div><div class=\"legend\"><span><span class=\"tak\">✓ Tak</span> jest</span>")
          .Append("<span><span class=\"brak\">brak</span> nieuzupełnione w metryczce (także Tak/Nie bez zaznaczenia)</span></div></div>");
        return sb.ToString();
    }

    static string Kpi(string wartosc, string opis) => $"<div class=\"kpi\"><b>{wartosc ?? "–"}</b><span>{opis}</span></div>";

    static string Wartosc(string v) => v switch {
        null => "<span class=\"brak\">brak</span>",
        "Tak" => "<span class=\"tak\">✓ Tak</span>",
        _ => HtmlEncode(v),
    };
}
