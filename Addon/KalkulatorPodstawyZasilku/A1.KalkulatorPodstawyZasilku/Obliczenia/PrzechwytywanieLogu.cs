using System;
using System.Diagnostics;
using System.Text;

namespace A1.KalkulatorPodstawy
{
    /// <summary>
    /// Zbiera szczegółowy log silnika podstawy zasiłku. PodstawaZasiłku pisze tabelę wliczanych elementów
    /// przez System.Diagnostics.Trace z kategorią "Chorobowe" (to samo, co pokazuje natywna czynność
    /// "Pokaż log naliczania podstawy"), a do właściwości Info trafia tylko podsumowanie.
    /// Trace.Listeners jest wspólne dla całego procesu serwera, więc listener bierze wyłącznie wpisy
    /// z wątku, który go utworzył - wpisy innych operatorów liczących w tym samym czasie są pomijane.
    /// Używać w using: Dispose odpina listener.
    /// </summary>
    internal sealed class PrzechwytywanieLogu : TraceListener
    {
        private const string KategoriaChorobowe = "Chorobowe";

        private readonly int watek = Environment.CurrentManagedThreadId;
        private readonly StringBuilder tekst = new StringBuilder();

        public PrzechwytywanieLogu()
        {
            Trace.Listeners.Add(this);
        }

        public string Tekst => tekst.ToString();

        public override void WriteLine(string message, string category)
        {
            if (category == KategoriaChorobowe && Environment.CurrentManagedThreadId == watek)
                tekst.AppendLine(message);
        }

        public override void Write(string message, string category)
        {
            if (category == KategoriaChorobowe && Environment.CurrentManagedThreadId == watek)
                tekst.Append(message);
        }

        // Wpisy bez kategorii (np. "Naliczanie wypłat") nie należą do podstawy zasiłku.
        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Trace.Listeners.Remove(this);
            base.Dispose(disposing);
        }
    }
}
