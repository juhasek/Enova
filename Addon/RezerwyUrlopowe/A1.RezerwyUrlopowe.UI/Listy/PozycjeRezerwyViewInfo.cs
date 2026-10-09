using System;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Tools;
using Soneta.Types;

[assembly: FolderView("Kadry i płace/Płace/Rezerwy urlopowe - pozycje", TableName = "PozRezerwUrlop",
    Priority = 96, IconName = "lista",
    Description = "Pozycje rezerw urlopowych pracowników - analizy wielomiesięczne, grupowanie wg MPK.",
    ViewType = typeof(A1.Rezerwy.UI.PozycjeRezerwyViewInfo))]

namespace A1.Rezerwy.UI;

/// <summary>Pozycje rezerw wszystkich miesięcy - z filtrem rodzaju i zakresu miesięcy.</summary>
public class PozycjeRezerwyViewInfo : ViewInfo {

    public PozycjeRezerwyViewInfo() {
        ResourceName = "PozycjeRezerwy";
        AllowNewInPlace = false;
        InitContext += (sender, args) => args.Context.Set(new Params(args.Context));
        CreateView += UtworzWidok;
    }

    void UtworzWidok(object sender, CreateViewEventArgs args) {
        Params parametry = args.Context.GetRequired<Params>();
        View widok = RezerwyModule.GetInstance(args.Session).PozRezerwUrlop.CreateView();
        if (parametry.Rodzaj != FiltrRodzajuRezerwy.Wszystkie)
            widok.Condition &= new FieldCondition.Equal("Rezerwa.Rodzaj", (RodzajRezerwyUrlopowej)(int)parametry.Rodzaj);
        if (parametry.Rok > 0)
            widok.Condition &= new FieldCondition.Equal("Rezerwa.Rok", parametry.Rok);
        if (parametry.Miesiac > 0)
            widok.Condition &= new FieldCondition.Equal("Rezerwa.Miesiac", parametry.Miesiac);
        args.DataSource = widok;
    }

    public class Params : ContextBase {
        const string klucz = "A1.RezerwyUrlopowe.Pozycje";

        public Params(Context context) : base(context) {
            rodzaj = LoadProperty(nameof(Rodzaj), klucz, FiltrRodzajuRezerwy.Rezerwa);
            rok = LoadProperty(nameof(Rok), klucz, Date.Today.Year);
            miesiac = LoadProperty(nameof(Miesiac), klucz, 0);
        }

        FiltrRodzajuRezerwy rodzaj;
        [Caption("Rodzaj"), Priority(10)]
        public FiltrRodzajuRezerwy Rodzaj {
            get => rodzaj;
            set { rodzaj = value; Session.InvokeChanged(); SaveProperty(nameof(Rodzaj), klucz); }
        }

        int rok;
        [Caption("Rok (0 = wszystkie)"), Priority(20)]
        public int Rok {
            get => rok;
            set { rok = value; Session.InvokeChanged(); SaveProperty(nameof(Rok), klucz); }
        }

        int miesiac;
        [Caption("Miesiąc (0 = wszystkie)"), Priority(30)]
        public int Miesiac {
            get => miesiac;
            set { miesiac = value; Session.InvokeChanged(); SaveProperty(nameof(Miesiac), klucz); }
        }
    }
}
