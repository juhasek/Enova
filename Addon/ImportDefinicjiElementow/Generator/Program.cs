using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ImportDefinicjiElementow.Generator;

// Użycie:
//   GeneratorSchematu --server <katalog Soneta.Products.Server.Standard> --schema <katalog schema>
// Czyta schema/zrodla.json (konfiguracja) i schema/opisy.json (opisy ręczne), zapisuje:
//   schema/definicja-elementu.schema.json - schemat (jedno źródło prawdy dla formularza i importu),
//   schema/PRZEGLAD.md                    - tabela do przeglądu,
//   schema/ZMIANY.md                      - różnice względem poprzedniego schematu (po upgradzie enova),
//   schema/opisy.json                     - uzupełniony o szkielety nowych pól (istniejące wpisy bez zmian).

Console.OutputEncoding = Encoding.UTF8;
var argumenty = Argumenty(args);
var serwer = argumenty.GetValueOrDefault("--server") ?? @"C:\enovaServer\2610\Soneta.Products.Server.Standard";
var katSchema = argumenty.GetValueOrDefault("--schema") ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "schema");
katSchema = Path.GetFullPath(katSchema);

var konf = Konfiguracja.Wczytaj(Path.Combine(katSchema, "zrodla.json"));
var plikOpisow = Path.Combine(katSchema, "opisy.json");
var opisy = Opisy.Wczytaj(plikOpisow);
var plikSchematu = Path.Combine(katSchema, "definicja-elementu.schema.json");
JsonObject poprzedni = File.Exists(plikSchematu) ? JsonNode.Parse(File.ReadAllText(plikSchematu))?.AsObject() : null;

var loader = new SonetaLoader(serwer);
var asmLogika = loader.Wczytaj(konf.BibliotekaLogiki);
var asmUI = loader.Wczytaj(konf.BibliotekaUI);
var wersjaEnova = SonetaLoader.Wersja(asmLogika);
Console.WriteLine($"enova {wersjaEnova} z {serwer}");

var typWiersza = asmLogika.GetType(konf.TypObiektu, throwOnError: true);
var typRekordu = asmLogika.GetType(konf.TypRekordu, throwOnError: true);
var insp = new TypeInspector(typWiersza, typRekordu);

var skaner = new FormScanner(SonetaLoader.ZasobyXml(asmUI), konf);
var zakladki = skaner.Skanuj();
var zasobyLogiki = SonetaLoader.ZasobyXml(asmLogika);

var budowa = new BudowaSchematu(konf, opisy, insp, zasobyLogiki, wersjaEnova);
var schemat = budowa.Buduj(zakladki);
foreach (var o in skaner.Ostrzezenia)
    budowa.Ostrzezenia.Insert(0, o);

File.WriteAllText(plikSchematu, schemat.ToJsonString(Json.Opcje), new UTF8Encoding(false));
if (budowa.OpisyZmienione)
    opisy.Zapisz(plikOpisow);
File.WriteAllText(Path.Combine(katSchema, "PRZEGLAD.md"), Przeglad.Generuj(schemat, budowa), new UTF8Encoding(false));
File.WriteAllText(Path.Combine(katSchema, "ZMIANY.md"), Zmiany.Generuj(poprzedni, schemat, budowa), new UTF8Encoding(false));

Console.WriteLine($"Zakładek: {schemat["zakladki"]!.AsArray().Count}, pól: {schemat["pola"]!.AsObject().Count}, enumów: {schemat["enumy"]!.AsObject().Count}");
Console.WriteLine($"Do weryfikacji opisu: {budowa.DoWeryfikacji.Count}, ostrzeżeń: {budowa.Ostrzezenia.Count}");
foreach (var o in budowa.Ostrzezenia)
    Console.WriteLine("  ! " + o);
return 0;

static Dictionary<string, string> Argumenty(string[] a)
{
    var d = new Dictionary<string, string>();
    for (int i = 0; i + 1 < a.Length; i += 2)
        d[a[i]] = a[i + 1];
    return d;
}
