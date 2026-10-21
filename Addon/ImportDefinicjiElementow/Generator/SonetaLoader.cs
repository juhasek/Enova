using System.Reflection;
using System.Runtime.Loader;

namespace ImportDefinicjiElementow.Generator;

/// <summary>
/// Ładuje biblioteki serwera enova365 z podanego katalogu do osobnego kontekstu,
/// tylko do odczytu metadanych (typy, enumy, atrybuty) i zasobów osadzonych.
/// </summary>
sealed class SonetaLoader : AssemblyLoadContext
{
    readonly string katalog;

    public SonetaLoader(string katalogSerwera) : base("enova", isCollectible: false)
    {
        katalog = katalogSerwera;
    }

    protected override Assembly Load(AssemblyName name)
    {
        var plik = Path.Combine(katalog, name.Name + ".dll");
        return File.Exists(plik) ? LoadFromAssemblyPath(plik) : null;
    }

    public Assembly Wczytaj(string nazwa) => LoadFromAssemblyPath(Path.Combine(katalog, nazwa + ".dll"));

    /// <summary>Wersja produktu z atrybutów biblioteki (np. 2610.0.0-beta.241094).</summary>
    public static string Wersja(Assembly asm)
    {
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrEmpty(info))
            return info.Split('+')[0];
        return asm.GetName().Version?.ToString();
    }

    /// <summary>Wszystkie zasoby XML biblioteki: nazwa zasobu → treść.</summary>
    public static Dictionary<string, string> ZasobyXml(Assembly asm)
    {
        var wynik = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var nazwa in asm.GetManifestResourceNames())
        {
            if (!nazwa.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                continue;
            using var s = asm.GetManifestResourceStream(nazwa);
            if (s == null)
                continue;
            using var r = new StreamReader(s, detectEncodingFromByteOrderMarks: true);
            wynik[nazwa] = r.ReadToEnd();
        }
        return wynik;
    }
}
