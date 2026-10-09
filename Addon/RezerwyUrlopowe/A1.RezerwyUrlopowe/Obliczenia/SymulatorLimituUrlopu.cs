using System;
using System.Collections.Generic;
using Soneta.Business;
using Soneta.Kadry;
using Soneta.Kalend;
using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>
/// Budżet rezerwy: symulacja limitu urlopowego na rok następny ("Limity nieobecności / Nalicz")
/// w osobnej, NIEZAPISYWANEJ sesji - po jej zamknięciu nic nie trafia do bazy.
/// </summary>
public static class SymulatorLimituUrlopu {

    /// <summary>Zaległy na 01.01 i należny w roku <paramref name="rokBudzetu"/> (godziny) dla wskazanych limitów.</summary>
    public static (double zaleglyGodz, double naleznyGodz) Symuluj(Pracownik pracownik, int rokBudzetu, IEnumerable<Guid> definicjeLimitow, Log log) {
        double zaleglyGodz = 0, naleznyGodz = 0;
        Date poczatekRoku = new Date(rokBudzetu, 1, 1);
        Date koniecRoku = new Date(rokBudzetu, 12, 31);
        using (Session sesjaSymulacji = pracownik.Session.Login.CreateSession(false, false)) {
            Pracownik pracownikWSymulacji = sesjaSymulacji.Get(pracownik);
            KalendModule kalendSymulacji = KalendModule.GetInstance(sesjaSymulacji);
            foreach (Guid guidDefinicji in definicjeLimitow) {
                DefinicjaLimitu definicjaLimitu = kalendSymulacji.DefinicjeLimitow[guidDefinicji];
                if (definicjaLimitu == null)
                    continue;
                try {
                    NaliczanieLimitow.Params parametryNaliczania = new NaliczanieLimitow.Params(Context.Empty.Clone(sesjaSymulacji));
                    parametryNaliczania.Definicja = definicjaLimitu;
                    parametryNaliczania.Okres = FromTo.Year(rokBudzetu);
                    new NaliczanieLimitowUrlopowych().DodajLimit(parametryNaliczania, pracownikWSymulacji, new Log("Symulacja limitu", false));
                }
                catch (Exception blad) {
                    log.WriteLine($"Symulacja limitu {definicjaLimitu.Nazwa} - błąd: {blad.Message}");
                }
                LimitNieobecnosci pierwszyLimitRoku = null;
                foreach (LimitNieobecnosci limit in kalendSymulacji.LimNieobecnosci.WgPracownik[pracownikWSymulacji, definicjaLimitu]) {
                    if (limit.Okres.To < poczatekRoku || limit.Okres.From > koniecRoku)
                        continue;
                    if (pierwszyLimitRoku == null || limit.Okres.From < pierwszyLimitRoku.Okres.From)
                        pierwszyLimitRoku = limit;
                    double naleznyLimitu = (limit.LimitGodz + limit.ZmianaGodz - limit.WykorzystanyPoprzGodz).TotalHours;
                    naleznyGodz += naleznyLimitu;
                    log.WriteLine($"Symulacja {definicjaLimitu.Nazwa} {limit.Okres}: należny {naleznyLimitu:0.00} h");
                }
                if (pierwszyLimitRoku != null)
                    zaleglyGodz += pierwszyLimitRoku.PrzeniesienieGodz.TotalHours;
            }
            // sesja NIE jest zapisywana (brak sesjaSymulacji.Save())
        }
        return (zaleglyGodz, naleznyGodz);
    }
}
