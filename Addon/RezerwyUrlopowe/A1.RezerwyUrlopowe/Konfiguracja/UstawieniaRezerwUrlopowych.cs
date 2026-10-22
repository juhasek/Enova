using System;
using Soneta.Business;
using Soneta.Config;
using Soneta.Place;
using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>
/// Konfiguracja dodatku "Rezerwy urlopowe" w drzewie konfiguracji enova (CfgNodes/CfgAttributes):
/// Root → "A1" → "Rezerwy urlopowe". Edycja: Narzędzia → Opcje → Kadry i płace → Rezerwy urlopowe.
/// Odczyt nigdy nie zakłada węzła (działa w sesji tylko do odczytu i zwraca wartości domyślne);
/// węzeł powstaje przy pierwszym zapisie z okna Opcji.
/// </summary>
public class UstawieniaRezerwUrlopowych {

    public const string WezelFirmy = "A1";
    public const string WezelRezerw = "Rezerwy urlopowe";

    // GUID-y definicji zakładanych przez dane inicjujące dodatku (własne - nie nadpisują rozwiązania skryptowego).
    public static readonly Guid GuidElementuRezerwy = new Guid("8e6a7f2c-c06f-4917-9d81-7bf6b1c9aa66");
    public static readonly Guid GuidElementuBudzetu = new Guid("8edae287-6afb-4890-abc0-791b83bbb58c");
    public static readonly Guid GuidPlanowanejListyRezerwy = new Guid("c9dc5833-031f-4ed4-a90e-e16276948922");
    public static readonly Guid GuidPlanowanejListyBudzetu = new Guid("b1fb2739-d4bf-41e0-9784-b4bac9075546");

    readonly Session session;

    public UstawieniaRezerwUrlopowych(Session session) {
        this.session = session;
    }

    // ---------------- Podstawa ----------------

    public WariantPodstawyRezerwy WariantPodstawy {
        get => CzytajEnum("Wariant podstawy", WariantPodstawyRezerwy.Podstawa1);
        set => Zapisz("Wariant podstawy", value.ToString());
    }

    public bool ZasadniczeNominalne {
        get => CzytajBool("Zasadnicze nominalne", true);
        set => Zapisz("Zasadnicze nominalne", value);
    }

    public int MiesiecyPodstawy {
        get => CzytajInt("Miesięcy podstawy", 3);
        set => Zapisz("Miesięcy podstawy", value);
    }

    public bool PomijajMiesiaceBezWyplaty {
        get => CzytajBool("Pomijaj miesiące bez wypłaty", true);
        set => Zapisz("Pomijaj miesiące bez wypłaty", value);
    }

    // ---------------- Urlop ----------------

    public bool UwzgledniajUrlopDodatkowy {
        get => CzytajBool("Uwzględniaj urlop dodatkowy", true);
        set => Zapisz("Uwzględniaj urlop dodatkowy", value);
    }

    public bool ZaokraglajKolejnyUrlop {
        get => CzytajBool("Zaokrąglaj kolejny urlop", true);
        set => Zapisz("Zaokrąglaj kolejny urlop", value);
    }

    public bool DopuszczajUjemna {
        get => CzytajBool("Dopuszczaj ujemną rezerwę", false);
        set => Zapisz("Dopuszczaj ujemną rezerwę", value);
    }

    // ---------------- Narzuty i budżet ----------------

    public bool PpkWNarzutach {
        get => CzytajBool("PPK w narzutach", false);
        set => Zapisz("PPK w narzutach", value);
    }

    public bool BudzetZPelnymLimitem {
        get => CzytajBool("Budżet z pełnym limitem", true);
        set => Zapisz("Budżet z pełnym limitem", value);
    }

    // ---------------- Kontrole ----------------

    public KontrolaNiezatwierdzonychList NiezatwierdzoneListyPlac {
        get => CzytajEnum("Niezatwierdzone listy płac", KontrolaNiezatwierdzonychList.Blokada);
        set => Zapisz("Niezatwierdzone listy płac", value.ToString());
    }

    // ---------------- Powiązania z definicjami enova ----------------

    public DefinicjaElementu ElementRezerwy {
        get => Place.DefElementow[CzytajGuid("Element rezerwy", GuidElementuRezerwy)];
        set => Zapisz("Element rezerwy", value?.Guid.ToString() ?? "");
    }

    public DefinicjaElementu ElementBudzetu {
        get => Place.DefElementow[CzytajGuid("Element budżetu", GuidElementuBudzetu)];
        set => Zapisz("Element budżetu", value?.Guid.ToString() ?? "");
    }

    public DefinicjaPlanowanejListyPłac PlanowanaListaRezerwy {
        get => Place.DefPlanListPlac[CzytajGuid("Planowana lista rezerwy", GuidPlanowanejListyRezerwy)];
        set => Zapisz("Planowana lista rezerwy", value?.Guid.ToString() ?? "");
    }

    public DefinicjaPlanowanejListyPłac PlanowanaListaBudzetu {
        get => Place.DefPlanListPlac[CzytajGuid("Planowana lista budżetu", GuidPlanowanejListyBudzetu)];
        set => Zapisz("Planowana lista budżetu", value?.Guid.ToString() ?? "");
    }

    // Listy wyboru pól powiązań na stronie Opcji (konwencja enova: GetList + nazwa pola).
    public View GetListElementRezerwy() => DodatkiAutomatyczne();
    public View GetListElementBudzetu() => DodatkiAutomatyczne();
    public View GetListPlanowanaListaRezerwy() => Place.DefPlanListPlac.CreateView();
    public View GetListPlanowanaListaBudzetu() => Place.DefPlanListPlac.CreateView();

    View DodatkiAutomatyczne() {
        View widok = Place.DefElementow.CreateView();
        widok.Condition &= new FieldCondition.Equal("RodzajZrodla", RodzajŹródłaWypłaty.DodatekAutomatyczny);
        return widok;
    }

    /// <summary>Element wynagrodzenia dla rodzaju rezerwy.</summary>
    public DefinicjaElementu Element(RodzajRezerwyUrlopowej rodzaj) =>
        rodzaj == RodzajRezerwyUrlopowej.Budzet ? ElementBudzetu : ElementRezerwy;

    /// <summary>Definicja planowanej listy płac dla rodzaju rezerwy.</summary>
    public DefinicjaPlanowanejListyPłac PlanowanaLista(RodzajRezerwyUrlopowej rodzaj) =>
        rodzaj == RodzajRezerwyUrlopowej.Budzet ? PlanowanaListaBudzetu : PlanowanaListaRezerwy;

    /// <summary>Kopia ustawień do nagłówka rezerwy (audyt naliczenia).</summary>
    public void KopiujDo(ParametryNaliczenia parametry, decimal wspolczynnikEkwiwalentu) {
        parametry.WariantPodstawy = WariantPodstawy;
        parametry.ZasadniczeNominalne = ZasadniczeNominalne;
        parametry.MiesiecyPodstawy = MiesiecyPodstawy;
        parametry.PomijajMiesiaceBezWyplaty = PomijajMiesiaceBezWyplaty;
        parametry.ZaokraglajKolejnyUrlop = ZaokraglajKolejnyUrlop;
        parametry.DopuszczajUjemna = DopuszczajUjemna;
        parametry.PpkWNarzutach = PpkWNarzutach;
        parametry.BudzetZPelnymLimitem = BudzetZPelnymLimitem;
        parametry.WspolczynnikEkwiwalentu = wspolczynnikEkwiwalentu;
    }

    // ---------------- dostęp do drzewa konfiguracji ----------------

    PlaceModule Place => PlaceModule.GetInstance(session);

    CfgNode Wezel(bool utworz) {
        if (session == null)
            return null;
        CfgNode korzen = new CfgManager(session).Root;
        if (korzen == null)
            return null;
        CfgNode wezelFirmy = korzen.FindSubNode(WezelFirmy, false);
        if (wezelFirmy == null) {
            if (!utworz)
                return null;
            wezelFirmy = korzen.AddNode(WezelFirmy, CfgNodeType.Node);
        }
        CfgNode wezelRezerw = wezelFirmy.FindSubNode(WezelRezerw, false);
        if (wezelRezerw == null) {
            if (!utworz)
                return null;
            wezelRezerw = wezelFirmy.AddNode(WezelRezerw, CfgNodeType.Leaf);
        }
        return wezelRezerw;
    }

    object Czytaj(string nazwa, Type typ, object domyslna) {
        try {
            CfgNode wezel = Wezel(false);
            return wezel == null ? domyslna : wezel.GetAttribute(nazwa, typ, domyslna) ?? domyslna;
        }
        catch {
            return domyslna;
        }
    }

    bool CzytajBool(string nazwa, bool domyslna) => (bool)Czytaj(nazwa, typeof(bool), domyslna);

    int CzytajInt(string nazwa, int domyslna) => (int)Czytaj(nazwa, typeof(int), domyslna);

    // Enum i Guid trzymane jako tekst - CfgAttribute nie zna typu enum.
    T CzytajEnum<T>(string nazwa, T domyslna) where T : struct {
        string tekst = Czytaj(nazwa, typeof(string), "") as string;
        return Enum.TryParse(tekst, true, out T wartosc) ? wartosc : domyslna;
    }

    Guid CzytajGuid(string nazwa, Guid domyslna) {
        string tekst = Czytaj(nazwa, typeof(string), "") as string;
        return Guid.TryParse(tekst, out Guid wartosc) ? wartosc : domyslna;
    }

    void Zapisz(string nazwa, object wartosc) {
        Wezel(true)?.SetAttribute(nazwa, wartosc);
    }
}
