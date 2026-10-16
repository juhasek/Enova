using System.Linq;
using A1.KalkulatorPodstawy;
using Soneta.Business;
using Soneta.Business.App;
using Soneta.Kadry;

// Czynność na formularzu pracownika i na liście pracowników (zaznaczeni).
[assembly: Worker(typeof(KalkulatorPodstawyZasilkuWorker), typeof(Pracownik))]
[assembly: Worker(typeof(KalkulatorPodstawyZasilkuWorker), typeof(Pracownicy))]

namespace A1.KalkulatorPodstawy
{
    /// <summary>
    /// Otwiera okno kalkulatora podstawy zasiłku. Wzór: KalkulatorWynagrodzenWorker enova -
    /// akcja zwraca obiekt parametrów na nowej sesji tylko do odczytu, enova pokazuje jego formularz.
    /// </summary>
    public class KalkulatorPodstawyZasilkuWorker
    {
        [Context]
        public Context Context { get; set; }

        [Action("Kalkulator podstawy zasiłku", Target = ActionTarget.Menu | ActionTarget.ToolbarWithText,
            Icon = ActionIcon.Organizer, Priority = 10)]
        public KalkulatorPodstawyZasilkuParams Kalkulator(Login login)
        {
            if (login == null)
                return null;

            Session sesja = login.CreateSession(true, false, "Kalkulator podstawy zasiłku - okno");
            Pracownik[] wybrani = WybraniPracownicy().Select(p => sesja.Get(p)).ToArray();
            return new KalkulatorPodstawyZasilkuParams(Context.Empty.Clone(sesja), wybrani);
        }

        private Pracownik[] WybraniPracownicy()
        {
            if (Context == null)
                return new Pracownik[0];
            if (Context.Get(out Pracownik[] zaznaczeni) && zaznaczeni != null && zaznaczeni.Length > 0)
                return zaznaczeni;
            if (Context.Get(out Pracownik pracownik) && pracownik != null)
                return new[] { pracownik };
            return new Pracownik[0];
        }
    }
}
