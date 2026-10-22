using System.Text;
using ImportDefinicjiElementow;
using ImportDefinicjiElementow.Silnik;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Tools;
using Soneta.Types;
using Soneta.Place;

[assembly: Worker(typeof(ImportDefinicjiWorker), typeof(DefinicjaElementu))]

namespace ImportDefinicjiElementow;

/// <summary>Parametry czynności: plik z formularza klienta i tryb importu.</summary>
public class ParametryImportu : ContextBase
{
    public ParametryImportu(Context context) : base(context) { }

    [Caption("Plik z formularza (elementy-wynagrodzenia.json)")]
    [Priority(1)]
    public NamedStream Plik { get; set; }

    public FileDialogInfo GetListPlik() =>
        new FileDialogInfo { Title = "Plik definicji elementów wynagrodzenia", ForbidMultiSelection = true }
            .AddFilter("Plik formularza (*.json)", "*.json")
            .AddAllFilesFilter();

    [Caption("albo ścieżka do pliku na serwerze")]
    [Priority(2)]
    public string SciezkaPliku { get; set; }

    [Caption("Tryb importu")]
    [Priority(3)]
    public TrybImportu Tryb { get; set; } = TrybImportu.NoweIAktualizacja;

    [Caption("Zapisz bez okna podglądu")]
    [Priority(4)]
    public bool BezPodgladu { get; set; }
}

/// <summary>
/// Czynność „Importuj definicje z pliku” na liście definicji elementów wynagrodzenia:
/// wczytanie pliku → analiza i podgląd (nowe / aktualizacje z różnicami / błędy) → zapis po potwierdzeniu → raport.
/// </summary>
public class ImportDefinicjiWorker
{
    [Context]
    public Context Context { get; set; }

    [Context]
    public ParametryImportu Parametry { get; set; }

    [Action("Importuj definicje z pliku", Target = ActionTarget.Menu, Mode = ActionMode.OnlyTable, Priority = 11, Icon = ActionIcon.Open)]
    public object Importuj()
    {
        var schemat = Schemat.Wbudowany();
        PlikImportu plik;
        try
        {
            plik = PlikImportu.Wczytaj(Dane(), schemat);
        }
        catch (BladPliku e)
        {
            return Komunikat("Import definicji – błąd pliku", e.Message, MessageBoxInformationType.Error);
        }

        WynikAnalizy analiza;
        using (var sesja = Context.Login.CreateSession(true, true, "Import definicji – analiza"))
            analiza = Analizator.Analizuj(schemat, plik, new BazaEnova(sesja, schemat), Parametry?.Tryb ?? TrybImportu.NoweIAktualizacja);

        var podglad = Raport.Podglad(analiza, schemat);
        if (!analiza.DoZapisu.Any())
        {
            var brak = new WynikZapisu { BladOgolny = "Nie zapisywano – brak definicji do zapisania.", Operator = Context.Login.Operator?.Name, Baza = Context.Login.Database?.Name };
            ZapiszRaportObokPliku(analiza, brak, schemat);
            return new MessageBoxInformation("Import definicji – podgląd")
            {
                Type = MessageBoxInformationType.Warning,
                TextMarkdown = podglad + "\n\nPobrać raport analizy?",
                YesHandler = () => PlikRaportu(analiza, brak, schemat),
                NoHandler = () => null,
            };
        }
        if (Parametry?.BezPodgladu == true)
            return Zapisz(schemat, analiza);
        return new MessageBoxInformation("Import definicji – podgląd")
        {
            Type = MessageBoxInformationType.Information,
            TextMarkdown = podglad,
            YesHandler = () => Zapisz(schemat, analiza),
            NoHandler = () => null,
        };
    }

    object Zapisz(Schemat schemat, WynikAnalizy analiza)
    {
        var wynik = ZapisDefinicji.Zapisz(Context.Login, schemat, analiza);
        ZapiszRaportObokPliku(analiza, wynik, schemat);
        var tekst = wynik.Zapisano
            ? $"Zapisano **{wynik.Definicje.Count}** definicji (nowe: {analiza.Liczba(StatusDefinicji.Nowa)}, zaktualizowane: {analiza.Liczba(StatusDefinicji.Aktualizacja)})."
            : $"**Nic nie zapisano.** {wynik.BladOgolny}";
        return new MessageBoxInformation("Import definicji – wynik")
        {
            Type = wynik.Zapisano ? MessageBoxInformationType.Information : MessageBoxInformationType.Error,
            TextMarkdown = tekst + "\n\nPobrać raport importu (HTML)? Zawiera też uwagi klienta dla wdrożeniowca.",
            YesHandler = () => PlikRaportu(analiza, wynik, schemat),
            NoHandler = () => null,
        };
    }

    /// <summary>Przy imporcie z pliku na serwerze raport trafia też obok pliku (import wsadowy, kontrola wdrożeniowca).</summary>
    void ZapiszRaportObokPliku(WynikAnalizy analiza, WynikZapisu wynik, Schemat schemat)
    {
        if (Parametry?.Plik != null || string.IsNullOrWhiteSpace(Parametry?.SciezkaPliku))
            return;
        var p = PlikRaportu(analiza, wynik, schemat);
        var kat = Path.GetDirectoryName(Parametry.SciezkaPliku.Trim().Trim('"'));
        File.WriteAllBytes(Path.Combine(kat ?? ".", p.FileName), p.GetData());
    }

    byte[] Dane()
    {
        if (Parametry?.Plik != null)
            return Parametry.Plik.GetData();
        if (!string.IsNullOrWhiteSpace(Parametry?.SciezkaPliku))
        {
            var sc = Parametry.SciezkaPliku.Trim().Trim('"');
            if (!File.Exists(sc))
                throw new BladPliku($"Nie znaleziono pliku {sc} na serwerze.");
            return File.ReadAllBytes(sc);
        }
        throw new BladPliku("Wskaż plik elementy-wynagrodzenia.json zapisany formularzem klienta.");
    }

    static NamedStream PlikRaportu(WynikAnalizy analiza, WynikZapisu wynik, Schemat schemat)
    {
        var html = Raport.Html(analiza, wynik, schemat);
        return new NamedStream($"raport-importu-definicji-{DateTime.Now:yyyyMMdd-HHmm}.html", Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(html)).ToArray());
    }

    static MessageBoxInformation Komunikat(string tytul, string tekst, MessageBoxInformationType typ) =>
        new(tytul, tekst) { Type = typ };
}
