using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace ImportDefinicjiElementow.Testy;

/// <summary>
/// Biblioteki enova potrzebne w testach ładowane z katalogu serwera (jak w dodatku: nie kopiujemy ich do bin).
/// Katalog: zmienna SONETA_DIR albo domyślnie serwer 2610.
/// </summary>
static class BibliotekiSerwera
{
    [ModuleInitializer]
    internal static void Inicjuj()
    {
        var kat = Environment.GetEnvironmentVariable("SONETA_DIR") ?? @"C:\enovaServer\2610\Soneta.Products.Server.Standard";
        AssemblyLoadContext.Default.Resolving += (ctx, nazwa) =>
        {
            var plik = Path.Combine(kat, nazwa.Name + ".dll");
            return File.Exists(plik) ? ctx.LoadFromAssemblyPath(plik) : null;
        };
    }
}
