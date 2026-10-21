using System.Text;
using System.Text.Json.Nodes;

namespace ImportDefinicjiElementow.Generator;

/// <summary>Czytelny opis warunków DSL schematu (te same reguły interpretuje formularz i import).</summary>
static class Warunek
{
    public static string Opisz(JsonNode w, JsonObject schemat)
    {
        if (w == null)
            return "";
        var o = w.AsObject();
        if (o["wszystkie"] is JsonArray all)
            return string.Join(" i ", all.Select(x => Nawias(x, schemat)));
        if (o["dowolny"] is JsonArray any)
            return string.Join(" lub ", any.Select(x => Nawias(x, schemat)));
        if (o["nie"] is JsonNode n)
            return "nie (" + Opisz(n, schemat) + ")";
        if (o["licencja"] is JsonNode lic)
            return "licencja " + lic.GetValue<string>();
        var pole = o["pole"]?.GetValue<string>();
        var et = Etykieta(pole, schemat);
        if (o["rowne"] is JsonNode r) return $"{et} = {Wartosc(pole, r, schemat)}";
        if (o["rozne"] is JsonNode rz) return $"{et} ≠ {Wartosc(pole, rz, schemat)}";
        if (o["wsrod"] is JsonArray ws) return $"{et} ∈ {{{string.Join(", ", ws.Select(x => Wartosc(pole, x, schemat)))}}}";
        if (o["poza"] is JsonArray pz) return $"{et} ∉ {{{string.Join(", ", pz.Select(x => Wartosc(pole, x, schemat)))}}}";
        if (o["pusty"] is JsonNode p) return p.GetValue<bool>() ? $"{et} puste" : $"{et} wypełnione";
        if (o["wiekszeOd"] is JsonNode wo) return $"{et} > {wo}";
        return w.ToJsonString();
    }

    static string Nawias(JsonNode x, JsonObject s)
    {
        var t = Opisz(x, s);
        return x.AsObject().ContainsKey("wszystkie") || x.AsObject().ContainsKey("dowolny") ? "(" + t + ")" : t;
    }

    static string Etykieta(string pole, JsonObject s) =>
        pole == null ? "?" : s["pola"]?[pole]?["etykieta"]?.GetValue<string>() is string e ? $"„{e}”" : pole;

    static string Wartosc(string pole, JsonNode v, JsonObject s)
    {
        if (v is JsonValue jv && jv.TryGetValue<string>(out var str))
        {
            var en = s["pola"]?[pole]?["enum"]?.GetValue<string>();
            var et = en == null ? null : s["enumy"]?[en]?["wartosci"]?.AsArray().FirstOrDefault(x => x["nazwa"]?.GetValue<string>() == str)?["etykieta"]?.GetValue<string>();
            return et ?? str;
        }
        if (v is JsonValue b && b.TryGetValue<bool>(out var bb))
            return bb ? "Tak" : "Nie";
        return v.ToJsonString();
    }
}

static class Przeglad
{
    public static string Generuj(JsonObject s, BudowaSchematu b)
    {
        var sb = new StringBuilder();
        var pola = s["pola"]!.AsObject();
        sb.AppendLine("# Przegląd schematu definicji elementu wynagrodzenia");
        sb.AppendLine();
        sb.AppendLine($"Wygenerowano {s["wygenerowano"]} z bibliotek enova **{s["wersjaEnova"]}** (wersja schematu {s["wersjaSchematu"]}).");
        sb.AppendLine("Plik generowany — nie edytować ręcznie. Opisy i warunki zmienia się w `opisy.json`, układ w `zrodla.json`,");
        sb.AppendLine("potem uruchamia generator (patrz README).");
        sb.AppendLine();
        sb.AppendLine($"- zakładek: {s["zakladki"]!.AsArray().Count}, pól: {pola.Count}, enumów: {s["enumy"]!.AsObject().Count}, słowników: {s["slowniki"]!.AsObject().Count}");
        sb.AppendLine($"- pól z opisem do weryfikacji: **{b.DoWeryfikacji.Count}**, pominiętych: {s["pominiete"]!.AsArray().Count}, definicji standardowych enova: {s["definicjeStandardowe"]!.AsArray().Count}");
        sb.AppendLine();
        sb.AppendLine("Legenda: **Wym.** — wymagane zawsze (✔) lub warunkowo (w kolumnie warunków); ⚠ — opis do weryfikacji; ⓓ — wartość domyślna do weryfikacji.");
        sb.AppendLine();

        foreach (var z in s["zakladki"]!.AsArray())
        {
            sb.AppendLine($"## {z!["etykieta"]}");
            sb.AppendLine();
            if (z["licencja"] != null) sb.AppendLine($"Licencja enova: `{z["licencja"]}`. ");
            if (z["widocznaGdy"] != null) sb.AppendLine($"Zakładka widoczna, gdy: {Warunek.Opisz(z["widocznaGdy"], s)}. ");
            if (z["opis"] != null) sb.AppendLine(z["opis"]!.GetValue<string>());
            sb.AppendLine();
            foreach (var sek in z["sekcje"]!.AsArray())
            {
                sb.AppendLine($"### {sek!["etykietaEnova"]}");
                sb.AppendLine();
                if (sek["widocznaGdy"] != null) sb.AppendLine($"Sekcja widoczna, gdy: {Warunek.Opisz(sek["widocznaGdy"], s)}.");
                else if (sek["regulyEnova"] != null) sb.AppendLine($"⚠ Reguła widoczności w enova bez odwzorowania: {string.Join("; ", sek["regulyEnova"]!.AsArray().Select(x => $"`{x}`"))}.");
                sb.AppendLine();
                sb.AppendLine("| Pole | Ścieżka | Typ | Wym. | Domyślna | Warunki | Opis |");
                sb.AppendLine("|---|---|---|---|---|---|---|");
                foreach (var sc in sek["pola"]!.AsArray())
                {
                    var p = pola[sc!.GetValue<string>()]!;
                    sb.Append("| ").Append(Esc(p["etykieta"]?.ToString()));
                    if (p["opisDoWeryfikacji"] != null) sb.Append(" ⚠");
                    sb.Append(" | `").Append(p["sciezka"]).Append("` | ").Append(Typ(p, s)).Append(" | ");
                    sb.Append(p["wymagane"]?.GetValue<bool>() == true ? "✔" : p["wymaganeGdy"] != null ? "warunk." : "").Append(" | ");
                    sb.Append(Esc(Domyslna(p, s))).Append(p["domyslnaDoWeryfikacji"] != null ? " ⓓ" : "").Append(" | ");
                    sb.Append(Esc(Warunki(p, s))).Append(" | ");
                    sb.Append(Esc(p["opis"]?.ToString() ?? "")).AppendLine(" |");
                }
                sb.AppendLine();
            }
        }

        sb.AppendLine("## Pola do weryfikacji (`opisDoWeryfikacji`)");
        sb.AppendLine();
        sb.AppendLine("Instrukcja enova nie opisuje tych pól wprost albo opis wymaga potwierdzenia — nie zgadujemy, pole czeka na decyzję.");
        sb.AppendLine();
        sb.AppendLine("| Pole | Ścieżka | Zakładka/sekcja | Uwagi |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var sc in b.DoWeryfikacji)
        {
            var p = pola[sc];
            if (p == null) continue;
            sb.AppendLine($"| {Esc(p["etykieta"]?.ToString())} | `{sc}` | {Esc(Polozenie(sc, s))} | {Esc(p["uwagiTechniczne"]?.ToString() ?? "")} |");
        }
        sb.AppendLine();

        sb.AppendLine("## Reguły enova bez odwzorowania w warunkach");
        sb.AppendLine();
        sb.AppendLine("Pola, dla których kod enova ma metodę `IsVisible…`/`IsReadOnly…`, a schemat nie ma `widocznyGdy`/`edytowalnyGdy`/`dozwoloneWartosci`.");
        sb.AppendLine("Sprawdzić, czy warunek jest potrzebny w formularzu klienta (np. reguły licencyjne lub zależne od bazy można pominąć).");
        sb.AppendLine();
        foreach (var kv in pola)
        {
            var p = kv.Value!;
            var reg = p["regulyEnova"]?.AsArray().Select(x => x!.GetValue<string>()).Where(r => !r.Contains("GetList")).ToList();
            if (reg == null || reg.Count == 0) continue;
            if (p["widocznyGdy"] != null || p["edytowalnyGdy"] != null || p["dozwoloneWartosci"] != null || p["licencja"] != null) continue;
            sb.AppendLine($"- `{kv.Key}` ({p["etykieta"]}): {string.Join(", ", reg.Select(r => $"`{r}`"))}{(p["uwagiTechniczne"] != null ? " — " + Esc(p["uwagiTechniczne"]!.ToString()) : "")}");
        }
        sb.AppendLine();

        sb.AppendLine("## Wartości domyślne do weryfikacji (`domyslnaDoWeryfikacji`)");
        sb.AppendLine();
        sb.AppendLine("Wartość początkowa rekordu enova różni się od instrukcji albo nie jest ustawiona (0) — potwierdzić na nowym elemencie w enova.");
        sb.AppendLine();
        foreach (var kv in pola.Where(kv => kv.Value!["domyslnaDoWeryfikacji"] != null))
            sb.AppendLine($"- `{kv.Key}` ({kv.Value!["etykieta"]}): przyjęto **{Esc(Domyslna(kv.Value, s))}**{(kv.Value["uwagiTechniczne"] != null ? " — " + Esc(kv.Value["uwagiTechniczne"]!.ToString()) : "")}");
        sb.AppendLine();

        sb.AppendLine("## Słowniki (wartości z bazy klienta)");
        sb.AppendLine();
        sb.AppendLine("Lista podpowiedzi + możliwość wpisania wartości spoza listy. Import sprawdza istnienie wartości w bazie.");
        sb.AppendLine();
        sb.AppendLine("| Słownik | Typ enova | Szukany po | Wartości standardowe |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var kv in s["slowniki"]!.AsObject())
            sb.AppendLine($"| {kv.Value!["etykieta"]} | `{kv.Value["typEnova"]}` | {kv.Value["poleKlucza"]} | {kv.Value["wartosciStandardowe"]?.AsArray().Count ?? 0} |");
        sb.AppendLine();

        sb.AppendLine("## Enumy");
        sb.AppendLine();
        foreach (var kv in s["enumy"]!.AsObject())
        {
            sb.AppendLine($"**{kv.Key}** (`{kv.Value!["typEnova"]}`)");
            sb.AppendLine();
            foreach (var w in kv.Value["wartosci"]!.AsArray())
                sb.AppendLine($"- `{w!["nazwa"]}` = {w["wartosc"]} — {w["etykieta"]}{(w["ukryta"] != null ? " *(ukryta)*" : "")}{(w["opis"] != null ? ": " + w["opis"] : "")}");
            sb.AppendLine();
        }

        sb.AppendLine("## Pominięte w formularzu klienta");
        sb.AppendLine();
        sb.AppendLine("| Pole / sekcja | Ścieżka | Powód |");
        sb.AppendLine("|---|---|---|");
        foreach (var p in s["pominiete"]!.AsArray())
            sb.AppendLine($"| {Esc(p!["etykieta"]?.ToString())} | `{p["sciezka"]}` | {Esc(p["powod"]?.ToString())} |");
        sb.AppendLine();

        if (b.Ostrzezenia.Count > 0)
        {
            sb.AppendLine("## Ostrzeżenia generatora");
            sb.AppendLine();
            foreach (var o in b.Ostrzezenia)
                sb.AppendLine("- " + Esc(o));
            sb.AppendLine();
        }
        return sb.ToString();
    }

    static string Typ(JsonNode p, JsonObject s)
    {
        var t = p["typ"]!.GetValue<string>();
        if (p["enum"] != null) return $"lista: `{p["enum"]}`";
        if (p["slownik"] != null) return $"słownik: `{p["slownik"]}`";
        if (p["maxDlugosc"] != null) return $"{t}({p["maxDlugosc"]})";
        if (t == "lista") return "tabela: " + string.Join(", ", p["kolumny"]!.AsArray().Select(k => k!["etykieta"]?.ToString()));
        return t;
    }

    static string Domyslna(JsonNode p, JsonObject s)
    {
        var d = p["domyslna"];
        if (d == null) return "";
        if (p["enum"] != null && d is JsonValue v && v.TryGetValue<string>(out var str))
            return s["enumy"]?[p["enum"]!.GetValue<string>()]?["wartosci"]?.AsArray().FirstOrDefault(x => x!["nazwa"]?.GetValue<string>() == str)?["etykieta"]?.GetValue<string>() ?? str;
        if (d is JsonValue b && b.TryGetValue<bool>(out var bb)) return bb ? "Tak" : "Nie";
        return d.ToJsonString().Trim('"');
    }

    static string Warunki(JsonNode p, JsonObject s)
    {
        var l = new List<string>();
        if (p["widocznyGdy"] != null) l.Add("widoczne: " + Warunek.Opisz(p["widocznyGdy"], s));
        if (p["edytowalnyGdy"] != null) l.Add("edytowalne: " + Warunek.Opisz(p["edytowalnyGdy"], s));
        if (p["wymaganeGdy"] != null) l.Add("wymagane: " + Warunek.Opisz(p["wymaganeGdy"], s));
        if (p["dozwoloneWartosci"] != null) l.Add("wartości zależne od warunków");
        if (p["licencja"] != null) l.Add("licencja " + p["licencja"]);
        if (p["walidacje"] != null) l.Add("walidacja: " + p["walidacje"]!.ToJsonString());
        return string.Join("; ", l);
    }

    static string Polozenie(string sc, JsonObject s)
    {
        foreach (var z in s["zakladki"]!.AsArray())
            foreach (var sek in z!["sekcje"]!.AsArray())
                if (sek!["pola"]!.AsArray().Any(x => x!.GetValue<string>() == sc))
                    return $"{z["etykieta"]} / {sek["etykieta"]}";
        return "";
    }

    static string Esc(string t) => (t ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
}

static class Zmiany
{
    public static string Generuj(JsonObject poprz, JsonObject nowy, BudowaSchematu b)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Zmiany schematu po ostatnim uruchomieniu generatora");
        sb.AppendLine();
        if (poprz == null)
        {
            sb.AppendLine("Pierwsze uruchomienie — brak poprzedniego schematu do porównania.");
            if (b.NoweSzkielety.Count > 0)
                sb.AppendLine($"\nDodano {b.NoweSzkielety.Count} szkieletów opisów w `opisy.json`.");
            return sb.ToString();
        }
        sb.AppendLine($"Wersja enova: {poprz["wersjaEnova"]} → {nowy["wersjaEnova"]}");
        sb.AppendLine();
        var p0 = poprz["pola"]!.AsObject();
        var p1 = nowy["pola"]!.AsObject();
        var dodane = p1.Select(k => k.Key).Except(p0.Select(k => k.Key)).ToList();
        var usuniete = p0.Select(k => k.Key).Except(p1.Select(k => k.Key)).ToList();
        Lista(sb, "Nowe pola (dopisane szkielety opisów — uzupełnić)", dodane);
        Lista(sb, "Pola usunięte z formularzy enova", usuniete);
        var zmienione = new List<string>();
        foreach (var kv in p1)
        {
            if (!p0.ContainsKey(kv.Key)) continue;
            var a = p0[kv.Key]!;
            var c = kv.Value!;
            foreach (var cecha in new[] { "typ", "enum", "slownik", "wymagane", "maxDlugosc", "typEnova" })
                if (a[cecha]?.ToJsonString() != c[cecha]?.ToJsonString())
                    zmienione.Add($"`{kv.Key}`: {cecha} {a[cecha]?.ToJsonString() ?? "-"} → {c[cecha]?.ToJsonString() ?? "-"}");
            if (a["regulyEnova"]?.ToJsonString() != c["regulyEnova"]?.ToJsonString())
                zmienione.Add($"`{kv.Key}`: reguły enova {a["regulyEnova"]?.ToJsonString() ?? "-"} → {c["regulyEnova"]?.ToJsonString() ?? "-"} (sprawdzić warunki w opisy.json)");
        }
        Lista(sb, "Zmienione pola", zmienione);
        var e0 = poprz["enumy"]!.AsObject();
        var zmEnum = new List<string>();
        foreach (var kv in nowy["enumy"]!.AsObject())
        {
            var n1 = kv.Value!["wartosci"]!.AsArray().Select(x => x!["nazwa"]!.GetValue<string>()).ToList();
            var n0 = e0[kv.Key]?["wartosci"]?.AsArray().Select(x => x!["nazwa"]!.GetValue<string>()).ToList() ?? new List<string>();
            foreach (var x in n1.Except(n0)) zmEnum.Add($"`{kv.Key}`: nowa wartość `{x}` (dopisać opis)");
            foreach (var x in n0.Except(n1)) zmEnum.Add($"`{kv.Key}`: usunięta wartość `{x}` (sprawdzić szablony i pliki klientów)");
        }
        Lista(sb, "Zmiany wartości enumów", zmEnum);
        return sb.ToString();
    }

    static void Lista(StringBuilder sb, string tytul, List<string> el)
    {
        sb.AppendLine($"## {tytul} ({el.Count})");
        sb.AppendLine();
        foreach (var e in el)
            sb.AppendLine("- " + e);
        if (el.Count == 0)
            sb.AppendLine("brak");
        sb.AppendLine();
    }
}
