# Import definicji elementów wynagrodzenia

Narzędzie do zbierania od klienta definicji elementów wynagrodzenia (składników płacowych) i importu
ich do enova365. Plan i status: `Addon/Plan ImportDefinicjiElementow/todo.md`.

| Katalog | Zawartość |
|---|---|
| `schema/` | **jedno źródło prawdy**: `definicja-elementu.schema.json` (generowany), `opisy.json` i `zrodla.json` (ręczne), `PRZEGLAD.md`, `ZMIANY.md` (generowane) |
| `Generator/` | konsolówka .NET 10 budująca schemat z bibliotek serwera enova i opisów ręcznych |
| `Formularz/` | formularz HTML dla klienta: źródła `src/`, build `build.mjs`, wynik `dist/formularz-elementow-wynagrodzenia.html`, testy `test/` |

Etap 3 (dodatek importu) – w przygotowaniu.

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
