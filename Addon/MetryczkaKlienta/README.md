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
- **Sekcja przed „Kadry”** bez zmian standardu: ekran modułu w Multi to menu folderów `[assembly: FolderView]`,
  sortowane po `Priority` (domyślnie 100). Standard: `Kadry i płace/Kadry` = 1, `Płace` = 2 (odczyt z
  `Soneta.KadryPlace.UI.dll`). Dodatek rejestruje `Kadry i płace/Metryczka klienta` z `Priority = 0`.
- Ekran modułu pokazuje tylko kafelki (tytuł i opis), nie treść. Dlatego sekcja ma kafelek „Metryczka”, który otwiera
  okno z cechami (folder z `ObjectType = MetryczkaKlienta`).
- W oknie jest lista (Grid), a nie pola generowane przez `<Template>`. Układ formularza jest cache'owany, a kolejność
  aktywne/nieaktywne zależy od wartości. Lista jest budowana przy każdym odczycie z definicji `TST_*`, więc nowa cecha
  dodana w Opcjach pojawia się sama.

## Okno „Metryczka klienta”

| Element | Działanie |
|---|---|
| Podsumowanie | liczba cech TST_ i liczba uzupełnionych |
| Przycisk „Ukryj / Pokaż metryczkę” | zwija i rozwija obie listy. Stan per operator w drzewie konfiguracji (`CfgNodes`: `A1MetryczkaKlienta/Widocznosc`, atrybut = Guid operatora) |
| Cechy uzupełnione | Bool = Tak, niepusty tekst, liczba ≠ 0, ustawiona data. Grupowane po kategorii, pogrubione (`Appearance FontBold`) |
| Nieaktywne | pozostałe cechy, zwykła czcionka, grupa `Collapsable`, domyślnie zwinięta |
| Czynności → Utwórz cechy testowe (TST_) | zakłada brakujące definicje (idempotentnie: istniejących nie zmienia) i zestaw `TST_Metryczka` z brakującymi pozycjami |
| Czynności → Usuń cechy testowe (TST_) | po potwierdzeniu usuwa zestaw `TST_Metryczka` i wszystkie definicje `TST_*` (z pozycjami słownika) |

Zapis z obu czynności i przycisku idzie przez osobną sesję konfiguracyjną (jedna transakcja, `Commit` + `Save`),
bo okno folderu ma sesję tylko do odczytu.

## Budowanie i instalacja

- SDK .NET 10 (`~/.dotnet10`). Referencje do DLL-i serwera 2610 (`SonetaDir`), jak w `KalkulatorPodstawyZasilku`:
  paczki Soneta.Sdk 2610 z NuGet nie mieszczą się na dysku tej maszyny.
- `dotnet build A1.MetryczkaKlienta -c Release` → `A1.MetryczkaKlienta/bin/Release/A1.MetryczkaKlienta.dll`.
- Zatrzymać procesy 2610 (orchestrator i dotnet), `dbmgr extupdate Claude <dll> --common --server`, uruchomić orchestrator.
  Przy działającym serwerze krok „Compiling database” kończy się timeoutem SQL. Po starcie zalogować się ręcznie w SonetaFrame.
- Wycofanie: Czynności → Usuń cechy testowe, potem `dbmgr extremove Claude A1.MetryczkaKlienta.dll`.

## Test (baza Claude)

1. Kadry i płace → sekcja „Metryczka klienta” jest pierwsza, nad „Kadry”.
2. Kafelek „Metryczka”: przy braku cech komunikat w podsumowaniu.
3. Czynności → Utwórz cechy testowe: 42 utworzone. Ponowne uruchomienie: 0 utworzonych, 42 pominięte.
4. Narzędzia → Opcje → Cechy globalne: cechy TST_ z kategoriami. Ustawić kilka wartości (np. Bool, tekst, liczba).
5. Okno metryczki: ustawione cechy są w „Cechy uzupełnione”, pogrubione i pogrupowane po kategorii, a reszta jest w zwiniętym „Nieaktywne”.
6. Przycisk „Ukryj metryczkę” ukrywa listy. Po ponownym otwarciu okna stan zostaje (per operator).
7. Czynności → Usuń cechy testowe → Tak: definicje TST_ znikają z Opcji i z okna.

### Wyniki testu na żywo (baza Claude, 26.10.2026)

| Krok | Wynik |
|---|---|
| 1. Sekcja „METRYCZKA KLIENTA” pierwsza, nad „KADRY” | OK (zrzut ekranu) |
| 3. Utworzenie 42 cech, 8 kategorii, typy wg mapowania, słownik 3 pozycje | OK (SQL) |
| 3. Ponowne uruchomienie bez duplikatów | OK (nadal 42 definicje i 3 pozycje słownika) |
| 2/5. Okno: 42 cechy, wszystkie w „Nieaktywne” przy pustych wartościach | OK |
| 6. Pokaż/ukryj, stan po ponownym otwarciu okna, zapis w `CfgAttributes` | OK |
| 4. Widoczność w Opcjach (zestaw `TST_Metryczka`) | NIE SPRAWDZONE: wersja z zestawem wgrana, ale zapis kończy się timeoutem SQL |
| 5. Pogrubienie uzupełnionych, grupowanie, zwinięte „Nieaktywne” | NIE SPRAWDZONE (brak wartości do ustawienia, zależy od kroku 4) |
| 7. Usuwanie | NIE SPRAWDZONE |

Timeouty („Soneta.Runtime.Database.Business; Przekroczony czas odpowiedzi serwera… MS SQL”) to brak zasobów, nie blokada.
Zmiana definicji cech lub zestawu przepisuje `RuntimeProjects`. W podglądzie SQL widać tylko `PAGEIOLATCH_SH`
i `SLEEP_BPOOL_STEAL`, bez sesji blokującej. Maszyna ma 4 GB RAM (wolne ~280 MB), SQL Server ~94 MB i ~1,4 mln page faults,
na C: ~1 GB wolnego. Pierwsze utworzenie 42 cech przeszło dopiero w trzeciej próbie.
