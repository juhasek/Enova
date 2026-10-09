using System;
using Soneta.Business;
using Soneta.Tools;
using Soneta.Types;

namespace A1.Rezerwy;

/// <summary>Parametry czynności „Nalicz rezerwę urlopową” (okno przed wykonaniem).</summary>
public class NaliczRezerweParams : ContextBase {

    public NaliczRezerweParams(Context context) : base(context) {
        Date dzis = Date.Today;
        // domyślnie poprzedni miesiąc (listy płac bieżącego zwykle jeszcze niezamknięte)
        Okres = dzis.Month == 1 ? new YearMonth(dzis.Year - 1, 12) : new YearMonth(dzis.Year, dzis.Month - 1);
        Rodzaj = RodzajRezerwyUrlopowej.Rezerwa;
    }

    RodzajRezerwyUrlopowej rodzaj;
    [Caption("Rodzaj"), Priority(10)]
    public RodzajRezerwyUrlopowej Rodzaj {
        get => rodzaj;
        set { rodzaj = value; OnChanged(EventArgs.Empty); }
    }

    YearMonth okres;
    [Caption("Miesiąc"), Priority(20), Required]
    public YearMonth Okres {
        get => okres;
        set { okres = value; OnChanged(EventArgs.Empty); }
    }

    bool naliczPonownie = true;
    [Caption("Nalicz ponownie pracowników już naliczonych"), Priority(30)]
    public bool NaliczPonownie {
        get => naliczPonownie;
        set { naliczPonownie = value; OnChanged(EventArgs.Empty); }
    }
}
