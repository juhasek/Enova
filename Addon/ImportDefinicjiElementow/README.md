# Import definicji elementów wynagrodzenia

Narzędzie do zbierania od klienta definicji elementów wynagrodzenia (składników płacowych) i importu
ich do enova365. Plan i status: `Addon/Plan ImportDefinicjiElementow/todo.md`.

| Katalog | Zawartość |
|---|---|
| `schema/` | **jedno źródło prawdy**: `definicja-elementu.schema.json` (generowany), `opisy.json` i `zrodla.json` (ręczne), `PRZEGLAD.md`, `ZMIANY.md` (generowane) |
| `Generator/` | konsolówka .NET 10 budująca schemat z bibliotek serwera enova i opisów ręcznych |
| `Formularz/` | formularz HTML dla klienta: źródła `src/`, build `build.mjs`, wynik `dist/formularz-elementow-wynagrodzenia.html`, testy `test/` |
| `Dodatek/` | dodatek enova 2610: `Silnik/` (schemat jako zasób, reguły – port `warunki.js`, analiza, raport), `ImportDefinicjiElementow/` (czynność, baza, zapis), `Testy/` (xUnit) |

## Import do enova (dodatek)

Czynność **„Importuj definicje z pliku”** w menu *Czynności* listy definicji
(*Ustawienia → Kadry i płace → Płace → Elementy wynagrodzenia*).

1. Okno parametrów: plik `elementy-wynagrodzenia.json` (albo ścieżka do pliku na serwerze), tryb
   **nowe i aktualizacja** / **tylko nowe**, opcjonalnie „Zapisz bez okna podglądu” (import wsadowy).
2. Wczytanie i kontrola formatu oraz głównej wersji schematu (inna wersja → komunikat „przekaż klientowi aktualny formularz”).
3. Analiza wg **tych samych reguł co formularz** (schemat wbudowany w `Silnik` z `schema/` – bez duplikowania reguł):
   wymagane pola, typy, zakresy, dozwolone wartości enumów, unikalność; istnienie wartości słowników w bazie
   (klucz wg `poleKlucza` ze schematu; odwołania do definicji z tego samego pliku są dozwolone);
   dopasowanie do istniejących definicji **po nazwie lub skrócie** (konflikt nazwa↔skrót = błąd).
   Definicje **standardowe enova nie są nadpisywane** (ostrzeżenie), zmiana rodzaju elementu = błąd.
4. Podgląd (okno enova): nowe / do aktualizacji z listą różnic „obecnie → z pliku” / bez zmian / pominięte / błędy.
5. Po potwierdzeniu zapis w **jednej transakcji** we własnej sesji: najpierw zakładane są nowe definicje, potem
   ustawiane pola w kolejności formularza enova (tylko pola aktywne; ustawiane tylko, gdy wartość się różni).
   Błąd dowolnego pola albo weryfikatora enova przy zapisie = nic nie jest zapisane, raport wskazuje definicję i pole.
6. Raport HTML do pobrania (wynik, różnice, ostrzeżenia, **uwagi klienta dla wdrożeniowca**); przy imporcie z pliku
   na serwerze raport zapisuje się też obok pliku.

**Build i testy**

```bash
export DOTNET_ROOT=~/.dotnet10
~/.dotnet10/dotnet.exe build Dodatek/ImportDefinicjiElementow/ImportDefinicjiElementow.csproj -c Release
~/.dotnet10/dotnet.exe test Dodatek/Testy/ImportDefinicjiElementow.Testy.csproj -c Release   # 18 testów
```

Biblioteki enova brane są z katalogu serwera (`SonetaDir`, domyślnie `C:\enovaServer\2610\...`, `Private=false`).
Build z paczek NuGet: `-p:SonetaNuGet=true -p:SonetaPackageVersion=<wersja>` (Directory.Build.props) – na tej maszynie
paczki 2610 (~1 GB) nie mieszczą się na dysku.

**Instalacja** (dbmgr z katalogu serwera, obie biblioteki):

```bash
dbmgr extupdate <Baza> <bin>/ImportDefinicjiElementow.Silnik.dll --common --server
dbmgr extupdate <Baza> <bin>/ImportDefinicjiElementow.dll --common --server
```

Potem restart serwera enova. Na tej maszynie (2610) samo zabicie `server.dll` nie wystarcza – orchestrator go
nie podnosi; trzeba zatrzymać wszystkie procesy `dotnet` z `C:\enovaServer\2610` i orchestrator, a potem uruchomić
`orchestrator.exe`. Wycofanie: `dbmgr extremove <Baza> ImportDefinicjiElementow.dll` (i `.Silnik.dll`).

**Test na żywo (baza Claude, 2026-10-22):** import 2 definicji z pliku z formularza → zapisane, wartości w enova
zgodne z plikiem (28 pól zmienionych względem domyślnych; ustawienia identyczne ze standardową „Premia procentowa”,
z której powstał szablon); aktualizacja → dokładnie 3 różnice (priorytet, procent, skrót); ponowny import → „bez zmian”.

## Formularz dla klienta

Jeden plik `Formularz/dist/formularz-elementow-wynagrodzenia.html` – działa offline, z dysku, w Edge/Chrome/Firefox,
bez instalacji i bez serwera. Dane zostają na komputerze klienta (szkic w `localStorage` przeglądarki).

**Przekazanie klientowi:** wysłać sam plik HTML (mailem / na dysk współdzielony). Klient:
1. otwiera plik w przeglądarce, ustawia licencję enova (srebrna/złota/platynowa),
2. dodaje definicje (najlepiej z szablonu), wypełnia zakładki – przycisk „?” przy każdym polu ma opis i znaczenie opcji,
3. opisuje nietypowe zasady na zakładce „Uwagi dla wdrożeniowca”,
4. klika „Zapisz plik dla wdrożeniowca” → `elementy-wynagrodzenia.json` i odsyła ten plik.

Plik JSON można wczytać z powrotem do formularza („Wczytaj plik…” – zastąp lub dołącz), np. po uwagach wdrożeniowca.
Format pliku: `format`, `wersjaSchematu`, `wersjaEnova`, `dataEksportu`, `ustawienia.licencja`,
`definicje[]` z `id`, `szablon`, `uwagi`, `wartosci` (tylko pola aktywne, w kolejności schematu) i `bledy`
(komunikaty walidacji w chwili zapisu). Import odrzuca plik o innej głównej wersji schematu.

**Szablony startowe** budowane są z definicji standardowych enova (`zrodla.json` → `szablony`, wartości z zasobów
`*.dbinit.xml` po przekształceniu pól rekordu na pola formularza – `Generator/Szablony.cs`): wynagrodzenie zasadnicze,
premia uznaniowa, premia procentowa, dodatek stażowy (z progami), dodatek funkcyjny, potrącenie.

### Build i testy

```bash
node Formularz/build.mjs                        # schemat + src → dist (po każdej zmianie schematu lub src)
node --test Formularz/test/warunki.test.mjs     # silnik reguł na prawdziwym schemacie
node Formularz/test/e2e-edge.mjs                # Edge/Chrome headless: szablon, warunki, eksport, import, autozapis + zrzuty
```

Silnik reguł (`src/warunki.js`) ma tę samą semantykę co import – zmieniać oba naraz.

## Schemat – skąd co się bierze

| Element schematu | Źródło |
|---|---|
| zakładki, sekcje, kolejność i etykiety pól | zasoby `*.pageform.xml` / `*.form.xml` z `Soneta.KadryPlace.UI.dll` (lista zakładek w `zrodla.json`) |
| typ pola, enum, słownik, czy zapisywalne | refleksja na `Soneta.Place.DefinicjaElementu` (`Soneta.KadryPlace.dll`) |
| wymagane, maks. długość, wartość początkowa | atrybuty `[Required]`, `[MaxLength]` i inicjalizacja `DefinicjaElementuRecord` |
| `regulyEnova` | metody `IsVisible…`/`IsReadOnly…`/`GetList…` i atrybuty `Visibility` formularzy – do kontroli przy upgradzie |
| wartości standardowe słowników, definicje standardowe | zasoby `*.dbinit.xml` z `Soneta.KadryPlace.dll` |
| etykiety dla kadrowej, opisy, podpowiedzi, warunki, domyślne, opisy wartości enumów | `opisy.json` (ręcznie, źródło: instrukcja enova + kod) |

Pola, których instrukcja nie opisuje, mają `"opisDoWeryfikacji": true`; wartości domyślne niepewne –
`"domyslnaDoWeryfikacji": true`. Oba zestawy są wypisane w `PRZEGLAD.md`.

### Warunki (DSL)

Te same reguły interpretuje formularz i import:

```json
{ "pole": "Algorytm.Typ", "rowne": "KreatorAlgorytmu" }
{ "pole": "OkresNaliczania.Typ", "wsrod": ["CoNMiesięcy", "CoNLat"] }      // także "poza", "rozne"
{ "pole": "Deklaracje.PozycjaPIT", "pusty": false }
{ "pole": "OkresNaliczania.Ilosc", "wiekszeOd": 1 }
{ "licencja": "zlota" }                                                   // złota lub platynowa
{ "wszystkie": [ … ] }, { "dowolny": [ … ] }, { "nie": { … } }
```

Właściwości pola: `widocznyGdy`, `edytowalnyGdy`, `wymaganeGdy`, `dozwoloneWartosci`
(`[{ "gdy": warunek, "wartosci": [...] }, …, { "wartosci": [...] }]` – pierwsza pasująca),
`domyslna` (wartość albo `{ "wgWarunku": [{ "gdy", "wartosc" }], "inaczej": … }`),
`walidacje` (`min`, `max`, `wzorzec` + `komunikat`, `unikalne`: `blad`/`ostrzezenie`).
Pole niewidoczne lub nieedytowalne nie jest eksportowane ani importowane – enova zostawia swoją wartość.

## Regeneracja schematu (np. po aktualizacji enova)

```bash
export DOTNET_ROOT=~/.dotnet10
~/.dotnet10/dotnet.exe build Generator/Generator.csproj -c Release -o Generator/bin/out
~/.dotnet10/dotnet.exe exec Generator/bin/out/GeneratorSchematu.dll \
    --server "C:/enovaServer/<wersja>/Soneta.Products.Server.Standard" --schema schema
```

Generator nie nadpisuje opisów: dla nowych pól dopisuje do `opisy.json` szkielet z `opisDoWeryfikacji`,
a w `ZMIANY.md` wypisuje nowe/usunięte pola, zmiany typów, wartości enumów i reguł enova
(wtedy przejrzeć warunki w `opisy.json`). Runtime .NET musi pasować do serwera (2610 = .NET 10).
