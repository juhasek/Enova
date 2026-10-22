# Filtr okresu na liście Świadczeń socjalnych — sesja 2026-10-16

Analiza na podstawie dekompilacji bibliotek serwera 2610 (`Soneta.KadryPlace.UI.dll`,
`Soneta.Business.dll`, `Soneta.Types.dll`). Nic nie było zmieniane w bazach.

## Jak działa standardowy filtr

Lista **Kadry → Świadczenia socjalne** (klasa `Soneta.Kadry.Forms.ŚwiadczeniaSocjalneViewInfo`,
parametry `Params : IndiPracParams : WdzParams : StdParams : OkresContext`):

```csharp
View view = KadryModule.SwiadczeniaSoc.WgData.CreateView();
if (Definicja != null)   view.Condition &= Definicja == wybrana;
if (Okres != FromTo.All) view.Condition &= RowCondition.ContainsIn("Data", Okres);
if (Pracownik != null)   view.Condition &= Pracownik == wybrany;
```

- **Okres** filtruje po polu `Data` = „Data przyznania” (obustronnie włącznie), nie po dacie rozliczenia.
- Okres „Wszystko” = brak warunku na datę.
- Jednostka organizacyjna jest w parametrach (dziedziczona z `WdzParams`), ale ten widok jej **nie używa**.
- Enum `Zakresy` (Razem / Spłacone / Niespłacone) istnieje w klasie, ale nie jest wykorzystany.
- Filtry zapisują się w ustawieniach użytkownika; zwolnieni pracownicy nie są odfiltrowywani.

## Filtrowanie po dacie rozliczenia

Kolumna **„Data rozliczenia”** (`Rozliczenie.Data`) jest w standardowym układzie listy
(`SwiadczeniaSocjalne.viewform.xml`). Bez programowania: Okres = Wszystko (albo szeroki) +
filtr kolumny „Data rozliczenia” w siatce. Nierozliczone (pusta data) przy takim filtrze znikają.

## Czy własny filtr okresu da się zrobić bez DLL — NIE

1. **Widok z konfiguracji (CfgNodes)** — w `FoldersInfo` jest pole `ViewType`, ale
   `SubFolderViewAttribute` je ignoruje (`ViewType = parentAttr.ViewType`): podwidok zawsze
   używa klasy folderu nadrzędnego, więc „Okres” dalej filtruje po dacie przyznania.
2. **Dodatkowy kod kompilacji (CodeFiles)** — foldery menu (`[assembly: FolderView]`) i inne
   atrybuty poziomu biblioteki zbiera `AssemblyAttributes.GetFromAssemblies()` raz przy starcie
   aplikacji i tylko z bibliotek z plikiem na dysku (`!assembly.Location.IsNullOrEmpty()`).
   Kod bazy ładowany jest z pamięci (`RuntimeAssemblyImages.Load(byte[])` → `Assembly.Load(image)`),
   więc nie jest skanowany. Nie da się też podpiąć pod standardowy `ViewInfo` (zdarzenia
   `CreateView`/`InitContext` są instancyjne, rozszerzenia wpinane atrybutami).

Prawdziwe pole od–do po dacie rozliczenia w panelu filtrów = **dodatek DLL** (własny `ViewInfo`).

Alternatywy bez DLL:
- widok z konfiguracji z kolumnami pod rozliczenia + filtr kolumny „Data rozliczenia”,
- wydruk/zestawienie (snippet raportu) z parametrami od–do po dacie rozliczenia.

**Otwarte:** wybór drogi przez użytkownika.

## Rezerwa urlopowa — dodatek DLL (przy okazji)

Potwierdzone: dodatek `A1.RezerwyUrlopowe(.UI).dll` usunięty z bazy Al dziś rano na prośbę
użytkownika (`dbmgr extremove`, kopia `Al_przed_odinstalowaniem_dodatku_rezerwy_20261016.bak`).
`dbmgr extlist Al` → tylko `altone.aldi.ext.dll`. Rozwiązanie skryptowe nietknięte.

W bazie zostały dane po dodatku: elementy 270/271 „(dodatek)”, planowane listy AREZURL/ABUDREZURL
(ID 3/4), węzeł konfiguracji A1 → Rezerwy urlopowe, puste tabele `RezerwyUrlopowe`/`PozRezerwUrlop`,
wpis wersji `A1Rezerwy:1`. **Otwarte:** czy sprzątać (decyzja użytkownika).
