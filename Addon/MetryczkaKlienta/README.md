# Dodatek A1.MetryczkaKlienta (enova365 2610)

Cechy globalne z metryczki klienta (`Pobrane/metryczka.xlsx`, 42 pozycje) z prefiksem `TST_` oraz sekcja
„Metryczka klienta” w module **Kadry i płace**, wyświetlana jako pierwsza, nad sekcją „Kadry”.

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
  arkusza, z plakietkami ✓ Tak / brak. Budowana przy każdym odczycie z definicji `TST_*`, więc nowa cecha
  dodana w Opcjach pojawia się sama. Przeglądarka enova nie obsługuje zmiennych CSS ani atrybutu `style` –
  kolory tylko w klasach `<style>`.
- „Brak” = Tak/Nie bez zaznaczenia, liczba 0, pusty tekst lub pusta data/miesiąc (decyzja 27.10). enova zapisuje w `Features`
  tylko Bool = Tak, więc świadomego „Nie” nie da się odróżnić od nieuzupełnionego. Warunek w jednym miejscu: `CechaMetryczki.Uzupelniona`.
- Kafelek i karta czytają cechy w świeżej sesji (`MetryczkaKlienta.CechyAktualne`) – sesja ekranu modułu/okna nie widzi
  zmian z Opcji ani skutków czynności Utwórz/Usuń.
- „Usuń cechy testowe” sprząta też pozostałości po usuniętych funkcjach: aktualności „Metryczka klienta – …” i węzeł
  konfiguracji `A1MetryczkaKlienta` (Ukryj/Pokaż).

## Okno „Metryczka klienta” (Ogólne → Metryczka klienta (TST_))

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
- Zatrzymać procesy 2610 (orchestrator i dotnet), `dbmgr extupdate Claude <dll> --common --server`, uruchomić orchestrator.
  Przy działającym serwerze krok „Compiling database” kończy się timeoutem SQL. Po starcie zalogować się ręcznie w SonetaFrame.
- Wycofanie: Czynności → Usuń cechy testowe, potem `dbmgr extremove Claude A1.MetryczkaKlienta.dll`.

## Test (baza Claude)

| Krok | Wynik |
|---|---|
| Kafelek „Metryczka klienta” nad „KADRY” ze stanem uzupełnienia | OK 27.10 (zrzut, „27 z 42”) |
| Okno z kartą: kolory, plakietki, pełne etykiety, pasek 30/42 | OK 27.10 (zrzut) |
| Utworzenie 42 cech, 8 kategorii, zestaw `TST_Metryczka` w Opcjach, bez duplikatów | OK 26.10 |
| Usuwanie cech TST_ | NIE SPRAWDZONE |
