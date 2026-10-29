using A1.Metryczka;
using Soneta.Business;

// {new MetryczkaKafelekExtender} w FolderMenu-KadryIPlace.form.xml - jak FolderMenuExtension standardu.
[assembly: Worker(typeof(MetryczkaKafelekExtender))]

namespace A1.Metryczka;

/// <summary>
/// Kafelek "Metryczka klienta" nad sekcjami ekranu modułu Kadry i płace: stan uzupełnienia, kliknięcie otwiera kartę.
/// Samej karty (Html) ekran modułu nie rysuje - tylko kafelki (sprawdzone 27.10, jak Grid i ThreadComments).
/// </summary>
public class MetryczkaKafelekExtender
{
    [Context]
    public Session Session { get; set; }

    public string Podsumowanie => new MetryczkaKlienta(Session).Podsumowanie;

    /// <summary>Zwrócony obiekt enova otwiera w oknie (karta + Czynności).</summary>
    public MetryczkaKlienta OtworzMetryczke() => new MetryczkaKlienta(Session);
}
