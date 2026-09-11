using Soneta.Business;
using Soneta.Types;

namespace A1.Rozszerzenia
{
    // Parametry czynnosci "Pelna lista plac (XLSX)" - okienko pytajace o haslo,
    // ktorym ma byc zaszyfrowany wygenerowany plik.
    //
    // MECHANIKA (wzor: Soneta.CRM.KontrahenciPrzypiszKategorieWorker+Params,
    // Soneta.CRM.Workers.LeadyZmienStanWorker+Params):
    // klasa parametrow dziedziczy po Soneta.Business.ContextBase i jest wstrzykiwana
    // do workera jako wlasciwosc z [Context]. Enova przed wykonaniem akcji tworzy taki
    // obiekt (ctor przyjmujacy Context) i pokazuje dla niego formularz parametrow.
    //
    // Uklad formularza bierze sie z zasobu
    //   A1.Rozszerzenia.A1HasloPlikuParams.Ogolne.pageform.xml
    // - trzy ostatnie czlony nazwy zasobu (po odcieciu ".xml") to
    //   <NazwaTypu>.<NazwaStrony>.pageform, a typ wyszukiwany jest po SAMEJ nazwie klasy
    //   (w enovie zasob Soneta.CRM.UI.UI.GusParams.DaneZGusParam.pageform.xml opisuje
    //   klase Soneta.CRM.Workers.GusParams - przestrzenie nazw sie nie zgadzaja).
    // Bez tego zasobu enova zbudowalaby formularz automatycznie, ale haslo byloby
    // widoczne jawnie - w XML pola maja Class="PasswordEdit" (kropki zamiast znakow).
    public class A1HasloPlikuParams : ContextBase
    {
        public A1HasloPlikuParams(Context context) : base(context) { }

        private string haslo = "";
        private string potwierdzenie = "";

        // UWAGA: hasla NIE trymujemy i NIE zapisujemy nigdzie (ani w konfiguracji,
        // ani przez ContextBase.SaveProperty) - zyje tylko na czas wykonania akcji.
        [Caption("Hasło do otwarcia pliku")]
        public string Haslo
        {
            get { return haslo; }
            set { haslo = value == null ? "" : value; }
        }

        [Caption("Powtórz hasło")]
        public string Potwierdzenie
        {
            get { return potwierdzenie; }
            set { potwierdzenie = value == null ? "" : value; }
        }

        // Puste haslo = plik bez zabezpieczenia (swiadomy wybor uzytkownika w okienku).
        public bool CzyChronic
        {
            get { return haslo.Length > 0; }
        }

        public bool HaslaZgodne
        {
            get { return haslo == potwierdzenie; }
        }
    }
}
