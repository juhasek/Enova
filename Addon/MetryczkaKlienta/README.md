# Dodatek A1.MetryczkaKlienta (enova365 2610)

Metryczka klienta (`Pobrane/metryczka.xlsx`, 42 pozycje + pola dodane przez użytkownika) z konfiguracją w
**Narzędzia → Opcje → A1Testy → Metryczka klienta**, kartą w oknie **Ogólne → Metryczka klienta** i kafelkiem
w module **Kadry i płace**, wyświetlanym jako pierwszy, nad sekcją „Kadry”.

## Od 2026-10-29: pola w Opcjach zamiast cech globalnych

Cechy globalne były nieczytelne, więc pola przeniesiono na własną stronę Opcji, obok „A1Testy/Konfiguracja raportu płacowego”.

- Zapis jest w drzewie konfiguracji (`CfgNodes`/`CfgAttributes`):
  `Root → A1Testy → Metryczka klienta → <klucz>`. Atrybuty liścia to `Nazwa`, `Kategoria`, `Typ` (`TakNie`/`Tekst`), `Kolejnosc`
  i `Wartosc`. Kod: `KonfiguracjaMetryczki.cs`.
- Strona (`Formularze/Config.MetryczkaKlienta.pageform.xml`, `MetryczkaConfigExtender.cs`) ma dwie siatki z edycją
  w miejscu, obok siebie: **Tak / Nie** z checkboxem i **Wartości tekstowe**. Obie są pogrupowane po kategorii (kategoria ustawiana w „Dodaj pole”), nad nimi stan „Uzupełniono X z Y”, a nazwę pola
  można poprawić w siatce. Przycisk „Wczytaj brakujące pola z metryczki” widać tylko, gdy brakuje pozycji z arkusza. Wzorzec standardu: Systemowe/Zmiany ustawienia (`ChangeInfosConfig.TableInfos`), czyli setter
  zapisuje atrybut w `Session.Logout(true)` + `CommitUI()`. Całość utrwala przycisk Zapisz okna Opcji.
- Przyciski:
  - **Dodaj pole** otwiera okno z polami nazwa, kategoria (lista istniejących) i typ (Tak/Nie, Tekst). Nowe pole dostaje klucz `PoleN` i trafia na koniec.
  - **Usuń pole** usuwa pole wybrane z listy nazw.
  - **Wczytaj pola z metryczki** zakłada brakujące pozycje arkusza (`DefinicjeCech.cs`; klucz = nazwa cechy bez `TST_`)
    z wartościami starych cech `TST_`, jeśli są. Liczba, miesiąc i słownik przechodzą jako tekst. Istniejących pól nie zmienia.
- Karta i kafelek czytają pola z konfiguracji. Pasek nagłówka szuka kluczy `NazwaKlienta`, `NrKlienta`, `NazwaBazySQL`,
  `JGSAP`, `FTE`, `LiczbaKartotek` i `Specjalista`. Kolejność kart wynika z kolejności pierwszego pola każdej kategorii.
- „Uzupełnione”: dla Tak/Nie wartość zapisana (także odznaczona), dla tekstu wartość niepusta. Nieustawiony Tak/Nie pokazuje „Nie”.
- Okno Ogólne → Metryczka klienta: czynność „Utwórz cechy testowe” usunięta, a **„Usuń stare cechy TST_”** zostaje do
  sprzątnięcia po wczytaniu wartości. Poniższe ustalenia o cechach globalnych dotyczą poprzedniej wersji.

## Ustalenia (2026-10-26)

- **Cechy globalne** = cechy węzła głównego konfiguracji: `FeatureDefs.TableName = 'CfgNodes'`, wartości w `Features`
  z `ParentType='CfgNodes', Parent=1` („(root)”), w kodzie `Session.Global.Features["Nazwa"]`
  (`Session.Global => CfgNodes.Root`). W Opcjach to strona `GlobalFeatures` („Ogólne/Cechy globalne”).
- Kategoria cechy jest w polu **`Category`** definicji. `Group` to bool do grupowania w drzewie list.
- **W 2610 strona Opcje → Cechy globalne pokazuje tylko cechy z zestawu cech** (`FSDefs` z `HostName='CfgNodes'`,
  `Target = GeneralPage`, pozycje `FSDefItems.Path = 'Features.<nazwa>'`), zob. `DefineItemsExtender.CalcGlobalDescriptors`.
  Istniejące cechy „Licznik” są w automatycznym zestawie „-”. Dlatego worker zakłada też zestaw **`TST_Metryczka`**
  i dopina do niego cechy TST_. Same definicje w `FeatureDefs` są widoczne dla kodu, ale nie w Opcjach.
- Nazwa słownika cechy (`Dictionary`) ma najwyżej 14 znaków, więc słownik `TST_TerminListyGlownej` nazywa się `TST_TerminList`.
- Mapowanie wierszy Excela → cech: `A1.MetryczkaKlienta/DefinicjeCech.cs`. Pola niepewne (35, 37–40) są
  typu String bez słownika (decyzja z 26.10, do ewentualnej zmiany). Wiersz 11 to String ze słownikiem „tylko słownik”
  (3 wartości z walidacji Excela).
- **Kafelek nad „Kadry”**: własny układ ekranu modułu `Formularze/FolderMenu-KadryIPlace.form.xml` (enova bierze
  `FolderMenu-<NormalizedPath>.form.xml` przed standardowym). Ekran modułu rysuje tylko kafelki – `Html`, `Grid`
  i lista aktualności się tam nie wyświetlają (sprawdzone 26–27.10). Dlatego jest jeden kafelek „Metryczka klienta”
  ze stanem „Uzupełniono X z Y pozycji”, który otwiera okno z kartą.
- **Karta metryczki** (`KartaMetryczki.cs`) to HTML w elemencie `<Html>` okna: pasek z danymi klienta (nazwa, nr,
  baza SQL, JG SAP, FTE, kartoteki, specjalista) i pasek uzupełnienia, pod nim karta na kategorię w kolejności wierszy
  arkusza, z plakietkami ✓ Tak / Nie / brak. Budowana przy każdym odczycie z definicji `TST_*`, więc nowa cecha
  dodana w Opcjach pojawia się sama. Przeglądarka enova nie obsługuje zmiennych CSS ani atrybutu `style` –
  kolory tylko w klasach `<style>`.
- Tak/Nie zawsze jako „Tak”/„Nie” – także nieustawiony (decyzja 27.10). „Brak” = liczba 0, pusty tekst, pusta data/miesiąc.
  Licznik „Uzupełniono X z Y” liczy tylko wartości zapisane (`FeatureCollection.Exists`): odznaczony Tak/Nie (wiersz z 0) – tak,
  nigdy nieustawiony – nie, mimo że pokazuje „Nie”. Warunek w jednym miejscu: `CechaMetryczki.Uzupelniona`.
- Kafelek i karta czytają cechy w świeżej sesji (`MetryczkaKlienta.CechyAktualne`) – sesja ekranu modułu/okna nie widzi
  zmian z Opcji ani skutków czynności Utwórz/Usuń.
- „Usuń cechy testowe” sprząta też pozostałości po usuniętych funkcjach: aktualności „Metryczka klienta – …” i węzeł
  konfiguracji `A1MetryczkaKlienta` (Ukryj/Pokaż).

## Okno „Metryczka klienta” (wersja do 28.10, Ogólne → Metryczka klienta (TST_))

| Element | Działanie |
|---|---|
| Karta | `KartaMetryczki.Html` – ta sama, którą otwiera kafelek nad „Kadry” |
| Czynności → Utwórz cechy testowe (TST_) | zakłada brakujące definicje (idempotentnie: istniejących nie zmienia) i zestaw `TST_Metryczka` z brakującymi pozycjami |
| Czynności → Usuń cechy testowe (TST_) | po potwierdzeniu usuwa zestaw `TST_Metryczka` i wszystkie definicje `TST_*` (z pozycjami słownika) |

Zapis z czynności idzie przez osobną sesję konfiguracyjną (jedna transakcja, `Commit` + `Save`),
bo okno folderu ma sesję tylko do odczytu. Generator aktualności i przycisk Ukryj/Pokaż usunięte 27.10
(karta je zastępuje).

## Budowanie i instalacja

- SDK .NET 10 (`~/.dotnet10`). Referencje do DLL-i serwera 2610 (`SonetaDir`), jak w `KalkulatorPodstawyZasilku`:
  paczki Soneta.Sdk 2610 z NuGet nie mieszczą się na dysku tej maszyny.
- `dotnet build A1.MetryczkaKlienta -c Release` → `A1.MetryczkaKlienta/bin/Release/A1.MetryczkaKlienta.dll`.
- **Visual Studio:** otworzyć `A1.MetryczkaKlienta.sln`. Wymaga VS z obsługą .NET 10 (Visual Studio 2026) i zainstalowanego
  systemowo SDK .NET 10 (`~/.dotnet10` VS nie znajdzie; na tej maszynie w `Program Files\dotnet` jest tylko SDK 8).
  Referencje wskazują `C:\enovaServer\2610\Soneta.Products.Server.Standard`; na innym komputerze ustawić `SonetaDir`
  (właściwość w `.csproj` albo zmienna środowiskowa `SonetaDir`) na katalog serwera enova 2610.
- Zatrzymać procesy 2610 (orchestrator i dotnet), `dbmgr extupdate Claude <dll> --common --server`, uruchomić orchestrator.
  Przy działającym serwerze krok „Compiling database” kończy się timeoutem SQL. Po starcie zalogować się ręcznie w SonetaFrame.
- Wycofanie: `dbmgr extremove Claude A1.MetryczkaKlienta.dll` (węzeł konfiguracji `A1Testy/Metryczka klienta` zostaje; stare cechy: Czynności → Usuń stare cechy TST_ przed usunięciem DLL). Dawniej: `dbmgr extremove Claude A1.MetryczkaKlienta.dll`.

## Odczyt pola metryczki w cesze / kodzie

Pole = węzeł `A1Testy → Metryczka klienta → <klucz>` w konfiguracji (CfgNodes), wartość w atrybucie `Wartosc`
(Tak/Nie = `bool`, reszta = `string`). Klucz nie zmienia się przy zmianie nazwy pola: dla pól z arkusza to nazwa bez
`TST_` (`NrKlienta`, `FTE`, `INGRPS`, … – lista w `DefinicjeCech.cs`), dla pól z „Dodaj pole” `Pole1`, `Pole2`, …
Nie wymaga referencji do DLL dodatku:

```csharp
public string Feature_NrKlienta {
    get {
        Soneta.Config.CfgNode pole = new Soneta.Config.CfgManager(Row.Session).Root
            ?.FindSubNode("A1Testy", false)?.FindSubNode("Metryczka klienta", false)?.FindSubNode("NrKlienta", false);
        return pole?.FindAttribute("Wartosc", false)?.Value as string ?? "";
    }
}
```

Tak/Nie: `pole?.FindAttribute("Wartosc", false)?.Value is bool b && b`. Liczby (FTE, liczba kartotek) są zapisane
jako tekst – parsować (`decimal.TryParse`). Przy kilku cechach wspólną metodę odczytu dać do Dodatkowego kodu
kompilacji, nie kopiować.

## Test (baza Claude)

| Krok | Wynik |
|---|---|
| Kafelek „Metryczka klienta” nad „KADRY” ze stanem uzupełnienia | OK 27.10 (zrzut, „27 z 42”) |
| Okno z kartą: kolory, plakietki, pełne etykiety, pasek 30/42 | OK 27.10 (zrzut) |
| Utworzenie 42 cech, 8 kategorii, zestaw `TST_Metryczka` w Opcjach, bez duplikatów | OK 26.10 |
| Usuwanie cech TST_ | NIE SPRAWDZONE |
| Strona Opcje (wersja 3484ffc): pasek „Uzupełniono X z Y”, siatki obok siebie, Wczytaj ukryty przy kompletnych polach | OK 29.10 (zrzut) |
| Edycja w siatkach (tekst, Tak/Nie), licznik odświeża się od razu | OK 29.10 |
| Dodaj pole (43. pole w wybranej kategorii) → Zapisz → karta i kafelek „21 z 43” | OK 29.10 |
| Usuń pole → licznik wraca do 42 | OK 29.10 |
| Wczytaj brakujące pola z metryczki | NIE SPRAWDZONE (wszystkie pola są) |
| Strona Opcji jak standardowa (8b5ed6e): sekcja na kategorię, etykieta + checkbox/pole tekstowe | OK 29.10 (zrzut) |
| Dodaj pole → nowe pole od razu w swojej sekcji (RenderKey), Usuń → znika | OK 29.10 |
| Edycja w polach nowej strony (wartości, checkboxy) → Zapisz | OK 29.10 (ręcznie, użytkownik) |
| Klient okienkowy (karta, strona Opcji) | OK 29.10 (ręcznie, użytkownik) |
| Tytuł okien przez `[Caption]` na klasie („Zmień nazwę pola metryczki”) | OK 29.10 |
| Zmień nazwę (d67cc01): okno, nowa etykieta po Zapisz | OK 29.10 |
| Szersze etykiety (LabelWidth 65) – długie pytania w całości | OK 29.10 (zrzut) |

Uwagi z testu 29.10: okna Dodaj/Usuń miały tytuł „Dodaj pole params” (web bierze nazwę klasy); `ToString` dał „Dodaj pole params: Dodaj pole metryczki”, stąd `[Caption]` na klasie.
Przez MCP okna nie da się zatwierdzić (`accept_subform` zamyka bez `AcceptHandler`) – OK klika operator.
Zapisane „Nie” w polu Tak/Nie liczy się do „Uzupełniono” (decyzja 27.10), więc po teście ING RPS ma zapisane „Nie” (licznik 19 zamiast 18).
