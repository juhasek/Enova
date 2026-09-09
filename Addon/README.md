# A1PelnaListaPlacAddon — skompilowany dodatek enova365

**Status: DZIAŁA — potwierdzone przez użytkownika** (wgrany przez `ExtPath`,
generuje poprawny plik XLSX z rozdzielonymi kolumnami; wersja z 5 slotami kolumn
przetestowana 2026-09-09). **Nowość 2026-09-09: okno pytające o hasło do pliku
przy każdym generowaniu — nieprzetestowane na żywo** (szczegóły niżej).

**Przycisk „Pełna lista płac (XLSX)" na pasku narzędzi** widoku **Płace → Listy
płac** (lista wszystkich list płac; działa na zaznaczonych pozycjach) — od
2026-09-08 nie jest to już pozycja menu *Czynności*, tylko przycisk
(`Target = ActionTarget.ToolbarWithText | ActionTarget.Menu`,
`Icon = ActionIcon.ExcelPreview`). Generuje plik `.xlsx` bezpośrednio biblioteką
`DevExpress.Spreadsheet` — **czysta siatka komórek, bez scalania/rozjeżdżania
kolumn**, którego nie dało się wyeliminować przy eksporcie report-snippetu z
podglądu wydruku (zob. `Raporty/A1PelnaListaPlac.md`).

Dane (kolumny: Kod / Imię i Nazwisko / Wydział + po jednej na każdą definicję
elementu + 17 stałych kolumn ZUS/PPK/PIT + Kwota do wypłaty) — logika przeniesiona
1:1 z `Raporty/A1PelnaListaPlacSnippet`.

## Konfiguracja: Narzędzia → Opcje → A1Testy → Konfiguracja raportu płacowego

Zakładka w oknie Opcji pozwala sterować raportem bez przebudowy DLL-a.

### 5 definiowalnych kolumn opisowych

Pierwsze kolumny raportu to **5 slotów**; każdy ma źródło treści (lista wyboru),
parametr i własny nagłówek. Slot ze źródłem `(kolumna nieużywana)` jest pomijany,
kolejność slotów = kolejność kolumn w pliku.

| Slot | Źródło (domyślnie) | Nagłówek (domyślnie) |
|---|---|---|
| 1 | Kod pracownika | `Kod` |
| 2 | Nazwisko i imię | `Imię i Nazwisko` |
| 3 | Wydział (wg daty wypłaty) | `Wydział` |
| 4 | (kolumna nieużywana) | |
| 5 | (kolumna nieużywana) | |

Dostępne źródła: kod pracownika, nazwisko i imię, nazwisko, imię, wydział i stanowisko
(oba z historii wg daty wypłaty), PESEL, numer listy płac, definicja listy płac, data
wypłaty, okres wypłaty (z listy płac) oraz **cecha pracownika** i **cecha wypłaty**.
Dla źródeł cechowych w kolumnie *parametr* wpisuje się nazwę cechy — dzięki temu klient
dokłada dowolną kolumnę bez zmian w DLL-u (nagłówek pusty = nazwa cechy).
Pusty nagłówek przy pozostałych źródłach = nazwa domyślna źródła.

### Pozostałe parametry

| Parametr | Domyślnie |
|---|---|
| Pokaż kolumny ZUS / PPK / PIT + kwotę do wypłaty (17 kolumn) | tak |
| Prefiks nazwy pliku | `A1_Pelna_Lista_Plac_` (+ data + `.xlsx`) |
| Nazwa arkusza | `Lista płac` |
| Sortuj wg kodu pracownika (zamiast wg nazwiska) | nie |

Kolumny elementów wynagrodzenia są zawsze wyliczane z zaznaczonych list płac (bez zmian).
Sortowanie działa niezależnie od tego, czy kod/nazwisko są w kolumnach.

**Jak to działa:**

- `Config.A1RaportPlacowy.pageform.xml` — zasób osadzony w DLL (`<EmbeddedResource>`
  z jawnym `LogicalName`, bo liczą się **trzy ostatnie człony** nazwy zasobu:
  `Config.<Nazwa>.pageform.xml`). Człon `Config` sprawia, że enova wiąże stronę z typem
  `Session` i wstawia ją do drzewa „Ustawienia” (`PageInfoCache.Item`:
  `N1 == "Config" → DataType = typeof(Session)`; `ConfigurationFolderViewAttribute`
  składa drzewo z `DataFormInfo.GetResourcePages(typeof(Session))`). Człony `CaptionHtml`
  rozdzielone `/` budują gałęzie — stąd `A1Testy/Konfiguracja raportu płacowego`.
  Zasoby są zbierane z **każdego** assembly referującego `Soneta.Types`
  (`DataForm` static ctor → `AssemblyAttributes.GetBusinessAssemblies()`), więc DLL
  z `ExtPath` też jest skanowany. Cache stron jest kluczowany datą modyfikacji pliku
  DLL (`Assembly.GetNameKey()`), więc podmiana DLL-a odświeża go sama.
- `A1RaportPlacConfigExtender` — kontekst danych strony
  (`DataContext="{New A1RaportPlacConfigExtender}"`); rejestracja
  `[assembly: Worker(typeof(...))]` bez typu danych (`DataType = DBNull`) sprawia, że
  klasa jest osiągalna w formularzu po nazwie.
- `A1RaportPlacUstawienia` — wartości w **drzewie konfiguracji enova**
  (`CfgManager(session).Root` → węzeł `A1Testy` → `Raport placowy` → atrybuty).
  Odczyt nigdy nie zakłada węzła (działa na sesji tylko-do-odczytu i zwraca domyślne);
  węzeł powstaje przy pierwszym zapisie z okna Opcji.
- `A1ZrodloKolumny` — enum źródeł z `[Caption]` (`Soneta.Types`), renderowany jako lista
  wyboru. W konfiguracji zapisywany jako **nazwa elementu enum (string)** — `CfgAttribute`
  nie zna typu enum, a konwersja `int → enum` przez `TypeConverter` nie działa.

## Hasło do pliku (okno przy generowaniu)

Po kliknięciu przycisku enova pokazuje **okno parametrów z dwoma polami hasła**
(znaki maskowane). Zachowanie:

| Co wpiszesz | Efekt |
|---|---|
| oba pola puste | plik `.xlsx` bez zabezpieczenia (jak dotąd) |
| hasło + to samo powtórzenie | plik zaszyfrowany — Excel pyta o hasło przy otwarciu |
| hasło ≠ powtórzenie | komunikat „Hasło i jego powtórzenie różnią się…", plik nie powstaje |

**Hasło nie jest nigdzie zapisywane** — ani w konfiguracji bazy, ani przez
`ContextBase.SaveProperty`; żyje tylko na czas wykonania czynności. Zgubionego
hasła nie da się odzyskać — plik trzeba wygenerować od nowa.

**Jak to jest zrobione:**

- `A1HasloPlikuParams : Soneta.Business.ContextBase` (konstruktor przyjmujący
  `Context`) wstrzykiwana do workera jako właściwość z `[Context]`. Enova przed
  wykonaniem akcji sama tworzy taki obiekt i pokazuje dla niego formularz — wzór
  z samej enovy: `Soneta.CRM.KontrahenciPrzypiszKategorieWorker+Params`,
  `Soneta.CRM.Workers.LeadyZmienStanWorker+Params`.
- `A1HasloPlikuParams.Ogolne.pageform.xml` — zasób osadzony; liczą się **trzy
  ostatnie człony** nazwy zasobu (po odcięciu `.xml`):
  `<NazwaKlasy>.<NazwaStrony>.pageform`. Typ jest szukany po **samej nazwie
  klasy**, prefiks przestrzeni nazw jest dowolny (w enovie zasób
  `Soneta.CRM.UI.UI.GusParams.DaneZGusParam.pageform.xml` opisuje klasę
  `Soneta.CRM.Workers.GusParams`).
- Pola mają `Class="PasswordEdit"` — maskowanie znaków; tak samo robi to enova w
  `WebOperatorNewPasswordParams.NewPasswordForm.pageform.xml`. Bez tego zasobu
  formularz zbudowałby się automatycznie, ale hasło byłoby widoczne jawnie.
- Zapis: `wb.SaveDocument(ms, DocumentFormat.Xlsx, new EncryptionSettings(hasło,
  EncryptionType.Strong))` (`DevExpress.Docs.v24.1`). `Strong` = szyfrowanie
  zgodne z Excel 2007+; `Compatible` to stary RC4.

**Zweryfikowane lokalnie** (test poza enovą, na DLL-ach z folderu serwera): plik
zapisany z hasłem to kontener OLE (`D0 CF 11 E0`) ze strumieniami `EncryptionInfo`
i `EncryptedPackage`, deskryptor `cipherAlgorithm="AES" hashAlgorithm="SHA512"
keyBits="256" spinCount="100000"`, a treść komórek nie występuje w pliku jawnym
tekstem. To standardowe szyfrowanie MS Office (agile), więc Excel zapyta o hasło.

**Nieprzetestowane na żywej enovie:** czy okno parametrów pokazuje się dla akcji
wywołanej **z paska narzędzi widoku listy** (wzory z enovy to czynności z menu).
Gdyby okno się nie pojawiało, worker traktuje brak parametrów jak „bez hasła"
(zamiast wywalać się `NullReferenceException`) — wtedy trzeba użyć tej samej
czynności z menu *Czynności* albo dodać do akcji `Mode`.

## Gdzie ląduje przycisk (mechanika enova)

Rejestracja workera decyduje o miejscu przycisku:

```csharp
[assembly: Worker<A1PelnaListaPlacWorker, ListyPlac>]   // TABELA -> widok listy płac
// [assembly: Worker<A1PelnaListaPlacWorker, ListaPlac>] // WIERSZ -> formularz jednej listy
```

Klient (`Soneta.Net.Business`, `ViewInfoWindow.MyWorkers`) buduje czynności widoku
listy z dwóch „indeksów”: **0 = typ tabeli** (`ListyPlac`, `IsEnumerableItem == true`)
i **1 = typ wiersza** (`ListaPlac`, `IsEnumerableItem == false`).
`WorkersMenu.RenderToolbarCommands` rysuje przycisk tylko gdy:

```
IsToolbarAction(akcja) && (IsEnumerableItem(index) || Mode ma SingleSession/IsolatedSession)
```

Dlatego worker zarejestrowany na **wierszu** z `Mode = None` nie dawał przycisku na
widoku listy — akcja pojawiała się dopiero na pasku **formularza** konkretnej listy
płac. Rejestracja na **tabeli** daje przycisk tam, gdzie trzeba. Tak samo robi to
sama enova: `[assembly: Worker(typeof(PodsumowanieWyplatListyWorker), typeof(ListyPlac))]`
(„Podsumowanie zaznaczonych list płac”, też z `[Context] ListaPlac[]`).

Zaznaczone wiersze trafiają do workera przez kontekst okna
(`context[selectedRows.GetType()] = selectedRows`). Bez zaznaczenia akcja pokazuje
komunikat „Zaznacz na liście co najmniej jedną listę płac”.

## Gotowy plik

`dist/A1PelnaListaPlacAddon.dll` — zbudowany dla **enova 2512.5.6 / .NET 8**.
Jeśli wersja enova jest inna, przebuduj (niżej).

## Budowanie

Na maszynie z zainstalowanym serwerem enova (dla ścieżek do DLL-i) i .NET SDK:

```
cd Addon
dotnet build -c Release
```

Wynik: `bin/Release/A1PelnaListaPlacAddon.dll`. Ścieżka do DLL-i enova jest
w `A1PelnaListaPlacAddon.csproj` (`<SonetaDir>`), popraw jeśli inna wersja/lokalizacja.

Dodatek celuje w `net8.0` (serwer enova 2512 = net8.0). Referencje mają
`Private=false` — DLL-e enova/DevExpress NIE są kopiowane do `bin`, bo są już na
serwerze.

## Wgranie na serwer

Dodatek musi zostać załadowany przez **każdy** komponent enova, który obsługuje
listę płac w GUI (Web / WebApi) i ewentualnie Server. Rekomendowany sposób —
katalog `ExtPath` (przeżywa aktualizacje enova, nie miesza się z plikami produktu):

1. Utwórz katalog na dodatki, np. `C:\enovaServer\Dodatki\`.
2. Skopiuj tam `A1PelnaListaPlacAddon.dll` (i `.pdb`, opcjonalnie).
3. Wskaż ten katalog komponentom enova. W `C:\enovaServer\2512.5.6\Config\appsettings.json`
   dołóż `ExtPath` w `Overrides` każdego procesu, np.:

   ```json
   "Server": {
     "Enabled": true,
     "Urls": "http://+:23000",
     "Overrides": {
       "DbConfig": "C:/enovaServer/2512.5.6/Config/Lista baz danych.xml",
       "ExtPath": "C:/enovaServer/Dodatki"
     }
   },
   "Web":    { "Overrides": { "Enabled": true, "Urls": "https://+:443", "ExtPath": "C:/enovaServer/Dodatki" } },
   "WebApi": { "Overrides": { "ExtPath": "C:/enovaServer/Dodatki" } },
   "Scheduler": { "Overrides": { "ExtPath": "C:/enovaServer/Dodatki" } }
   ```

   (Alternatywnie parametr uruchomieniowy `--extpath "C:\enovaServer\Dodatki"`
   lub `--ext "C:\enovaServer\Dodatki\A1PelnaListaPlacAddon.dll"` per komponent.)

4. **Zrestartuj usługi enova** (orchestrator pociągnie resztę).
5. **Płace → Listy płac** → zaznacz 1+ pozycji → przycisk **„Pełna lista płac
   (XLSX)"** na pasku narzędzi listy → pobierz plik.

### Szybki wariant (bez konfiguracji, ginie przy aktualizacji)

Skopiuj `A1PelnaListaPlacAddon.dll` wprost do:
- `C:\enovaServer\2512.5.6\Soneta.Products.Server.Standard\`
- `C:\enovaServer\2512.5.6\Soneta.Products.Web.Standard\`

i zrestartuj usługi. Działa, jeśli enova skanuje katalog bazowy komponentu —
`ExtPath` jest pewniejszy.

## Weryfikacja po wgraniu

- Przycisk pojawia się na pasku narzędzi widoku **Płace → Listy płac** (lista
  wszystkich list płac), niezależnie od zaznaczenia. Jeśli go nie ma — dodatek się
  nie załadował: zły `ExtPath` / nie zrestartowano / niezgodna wersja net/enova.
  Ta sama czynność jest też w menu *Czynności* (grupa „Listy płac”).
- Wygenerowany `.xlsx`: jedna spójna tabela, **każda kolumna = jedna kolumna
  Excela** (bez scaleń), liczby jako liczby (`=SUMA(...)` działa), nagłówek
  pogrubiony i zamrożony, autofiltr.
- Sumy w kolumnach ZUS/PPK/PIT zgodne z paskiem wypłaty (na realnie przeliczonej
  liście płac).
- **Okno hasła:** po kliknięciu przycisku pojawia się okno z dwoma polami hasła
  (maskowanymi). Test: (1) oba puste → plik otwiera się normalnie; (2) hasło
  wpisane dwa razy tak samo → Excel przy otwarciu pyta o hasło i wpuszcza po jego
  podaniu; (3) różne hasła w polach → komunikat o niezgodności, plik nie powstaje.
  Jeśli okno w ogóle się nie pokazuje — patrz sekcja „Hasło do pliku”.
- **Narzędzia → Opcje** → gałąź **A1Testy → Konfiguracja raportu płacowego** — zmiana
  parametru, zapis, ponowne wygenerowanie pliku pokazuje zmianę (np. wyłączona kolumna
  „Wydział” znika, własny nagłówek wchodzi zamiast domyślnego). Jeśli gałęzi nie ma:
  DLL nie został podmieniony w komponencie obsługującym GUI (Web) albo nie zrestartowano
  usług.

## Aktualizacja kodu

Zmień `A1PelnaListaPlacWorker.cs`, `dotnet build -c Release`, podmień DLL w
`ExtPath`, zrestartuj usługi.
