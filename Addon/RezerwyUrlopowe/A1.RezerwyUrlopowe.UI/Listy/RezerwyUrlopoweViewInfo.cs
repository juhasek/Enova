using System;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Tools;
using Soneta.Types;

[assembly: FolderView("Kadry i płace/Płace/Rezerwy urlopowe", TableName = "RezerwyUrlopowe",
    Priority = 95, IconName = "kalendarz_kwota",
    Description = "Rezerwy urlopowe i budżety rezerwy - miesiące z pozycjami pracowników.",
    ViewType = typeof(A1.Rezerwy.UI.RezerwyUrlopoweViewInfo))]

namespace A1.Rezerwy.UI;

/// <summary>Lista rezerw urlopowych (nagłówki miesięcy) z filtrem rodzaju i roku.</summary>
public class RezerwyUrlopoweViewInfo : ViewInfo {

    public RezerwyUrlopoweViewInfo() {
        ResourceName = "RezerwyUrlopowe";
        AllowNewInPlace = false;
        InitContext += (sender, args) => args.Context.Set(new Params(args.Context));
        CreateView += UtworzWidok;
    }

    void UtworzWidok(object sender, CreateViewEventArgs args) {
        Params parametry = args.Context.GetRequired<Params>();
        View widok = RezerwyModule.GetInstance(args.Session).RezerwyUrlopowe.CreateView();
        if (parametry.Rodzaj != FiltrRodzajuRezerwy.Wszystkie)
            widok.Condition &= new FieldCondition.Equal("Rodzaj", (RodzajRezerwyUrlopowej)(int)parametry.Rodzaj);
        if (parametry.Rok > 0)
            widok.Condition &= new FieldCondition.Equal("Rok", parametry.Rok);
        args.DataSource = widok;
    }

    public class Params : ContextBase {
        const string klucz = "A1.RezerwyUrlopowe.Lista";

        public Params(Context context) : base(context) {
            rodzaj = LoadProperty(nameof(Rodzaj), klucz, FiltrRodzajuRezerwy.Wszystkie);
            rok = LoadProperty(nameof(Rok), klucz, Date.Today.Year);
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
    }
}
