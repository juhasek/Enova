using System;
using System.Collections.Generic;
using Soneta.Business;
using Soneta.Business.UI;
using Soneta.Kadry;
using Soneta.Place;
using Soneta.Types;

// Przycisk na widoku "Definicje planowanych list płac" (Ustawienia -> Kadry i płace ->
// Płace -> Definicje planowanych list płac). Rejestracja na TABELI DefPlanListPlac (nie
// na wierszu) - wzorzec jak A1PelnaListaPlacWorker, patrz
// reference_enova_przycisk_na_widoku_listy w pamięci projektu.
[assembly: Worker<A1.Rozszerzenia.A1RozwiazanieOdprawWorker, DefPlanListPlac>]

namespace A1.Rozszerzenia
{
    // PO CO TEN DODATEK ISTNIEJE:
    //
    // Standardowa czynność enova "Nalicz planowane listy płac..."
    // (Soneta.Place.NaliczaniePlanowanychListPłacWorker) działa WYŁĄCZNIE na
    // pracownikach aktualnie zaznaczonych na liście Kadry -> Pracownicy, z której
    // czynność została wywołana ([Context] Pracownik[] wstrzykiwane z zaznaczenia
    // widoku - potwierdzone dekompilacją Soneta.KadryPlace.dll, patrz
    // ElementyPlac/Rozwiązanie odprawa emerytalno-rentowa.md, sekcja "Szósta
    // iteracja"). Domyślny filtr tej listy pokazuje tylko aktualnie zatrudnionych,
    // więc pracownik zwolniony PRZED okresem planu (np. zwolniony 01/2026, odprawa
    // emerytalno-rentowa wypłacona na liście głównej dopiero 08/2026) nigdy nie
    // trafia do przeliczenia - operator generujący plan na 08/2026 "stoi" wtedy na
    // liście pracowników zatrudnionych DZIŚ, a nie w sierpniu, i nie ma powodu ani
    // sposobu, żeby ręcznie o nim pamiętać co miesiąc.
    //
    // Ten przycisk NIE zależy od żadnego zaznaczenia na liście Pracownicy. Sam
    // wyszukuje wszystkich pracowników (aktywnych i zwolnionych - status
    // zatrudnienia nie ma tu znaczenia), którzy w WYBRANYM okresie planu mają na
    // liście głównej niezerowo rozliczony element źródłowy "Odprawa emerytalna"
    // (DefElementow, ta sama definicja, po której nazwie filtruje już "_Param"
    // Dodatku automatycznego "Rozwiązanie odprawa emerytalno-rentowa" - patrz
    // ImportyXML/Rozwiązanie odprawa emerytalno-rentowa.dbinit.xml), po czym dla
    // każdego z nich woła TEN SAM wbudowany silnik enova co standardowa czynność
    // (ustawiamy Soneta.Place.NaliczaniePlanowanychListPłacWorker przez jego własne
    // publiczne właściwości Pracownik/Pars i wywołujemy jego Nalicz() - celowo NIE
    // odtwarzamy ręcznie jego wewnętrznej logiki sesji/transakcji: ten mechanizm
    // (DefPlanListPlac) jest niedokumentowany i już raz kosztował 5 rund poprawek
    // przy subtelnych błędach, patrz historia w tym samym pliku .md).
    public class A1RozwiazanieOdprawWorker
    {
        const string OdprawaEmerytalnaNazwa = "Odprawa emerytalna";

        [Context]
        public NaliczaniePlanowanychListPłacWorker.Params Pars { get; set; }

        [Action("Nalicz plan wg wypłaconych odpraw (także zwolnieni)...",
                Target = ActionTarget.ToolbarWithText | ActionTarget.Menu,
                Mode = ActionMode.SingleSession | ActionMode.Progress,
                Icon = ActionIcon.Wizard,
                Priority = 110)]
        public object NaliczWgOdpraw()
        {
            if (Pars == null || Pars.Definicja == null)
                return new MessageBoxInformation
                {
                    Caption = "Nalicz plan wg wypłaconych odpraw",
                    Text = "Wybierz definicję planowanej listy płac.",
                };

            PlaceModule place = PlaceModule.GetInstance(Pars.Definicja.Session);

            DefinicjaElementu odprawa = place.DefElementow.WgNazwy[OdprawaEmerytalnaNazwa];
            if (odprawa == null)
                return new MessageBoxInformation
                {
                    Caption = "Nalicz plan wg wypłaconych odpraw",
                    Text = "Nie znaleziono w bazie definicji elementu '" + OdprawaEmerytalnaNazwa + "'.",
                };

            Pracownik[] pracownicy = ZnajdźPracownikówZOdprawą(place, odprawa, Pars.Okres);
            if (pracownicy.Length == 0)
                return new MessageBoxInformation
                {
                    Caption = "Nalicz plan wg wypłaconych odpraw",
                    Text = "Brak pracowników z niezerowo rozliczoną '" + OdprawaEmerytalnaNazwa
                           + "' w wybranym okresie (" + Pars.Okres + ").",
                };

            NaliczaniePlanowanychListPłacWorker worker = new NaliczaniePlanowanychListPłacWorker();
            worker.Pars = Pars;
            worker.Pracownik = pracownicy;
            return worker.Nalicz();
        }

        // Wszyscy pracownicy (bez względu na aktualny status zatrudnienia), którzy w
        // podanym okresie mają niezerowo rozliczony element "Odprawa emerytalna" -
        // niezależnie od tego, z której listy płac (WypElementy to tabela historyczna
        // całego pracownika, nie tylko bieżącej listy). WgDefinicja to indeks tabeli
        // WypElementy po polu Definicja (Soneta.Place.PlaceModule.WypElementTable) -
        // zwraca elementy WSZYSTKICH pracowników dla danej definicji.
        static Pracownik[] ZnajdźPracownikówZOdprawą(PlaceModule place, DefinicjaElementu odprawa, FromTo okres)
        {
            List<Pracownik> wyniki = new List<Pracownik>();
            HashSet<Guid> widziani = new HashSet<Guid>();
            foreach (WypElement el in place.WypElementy.WgDefinicja[odprawa])
            {
                if (el.Okres != okres) continue;
                if (el.Wartosc == 0m) continue;
                Pracownik p = el.Pracownik;
                if (p == null) continue;
                if (widziani.Add(p.Guid))
                    wyniki.Add(p);
            }
            return wyniki.ToArray();
        }
    }
}
