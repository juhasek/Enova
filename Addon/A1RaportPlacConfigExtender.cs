using Soneta.Business;

// Extender bez typu danych ([assembly: Worker(typeof(...))] -> DataType = DBNull) jest
// dostepny w formularzach po NAZWIE KLASY: DataContext="{New A1RaportPlacConfigExtender}".
// (WorkerCollection.GetWorker(nazwa) buduje slownik z workerow zarejestrowanych na DBNull,
// a WorkerAttribute.Name = nazwa typu bez koncowki "Worker".)
[assembly: Worker(typeof(A1.Rozszerzenia.A1RaportPlacConfigExtender))]

namespace A1.Rozszerzenia
{
    // Kontekst danych strony Narzedzia -> Opcje -> A1Testy -> Konfiguracja raportu placowego
    // (zasob Config.A1RaportPlacowy.pageform.xml). Strona typu "Config.*" wiaze sie z Session,
    // wiec extender dostaje sesje konfiguracyjna okna Opcji przez [Context].
    public class A1RaportPlacConfigExtender
    {
        [Context]
        public Session Session { get; set; }

        A1RaportPlacUstawienia ustawienia;

        public A1RaportPlacUstawienia Ustawienia
        {
            get { return ustawienia ?? (ustawienia = new A1RaportPlacUstawienia(Session)); }
        }
    }
}
