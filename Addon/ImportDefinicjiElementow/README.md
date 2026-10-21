# Import definicji elementów wynagrodzenia

Narzędzie do zbierania od klienta definicji elementów wynagrodzenia (składników płacowych) i importu
ich do enova365. Plan i status: `Addon/Plan ImportDefinicjiElementow/todo.md`.

| Katalog | Zawartość |
|---|---|
| `schema/` | **jedno źródło prawdy**: `definicja-elementu.schema.json` (generowany), `opisy.json` i `zrodla.json` (ręczne), `PRZEGLAD.md`, `ZMIANY.md` (generowane) |
| `Generator/` | konsolówka .NET 10 budująca schemat z bibliotek serwera enova i opisów ręcznych |

Etapy 2 (formularz HTML) i 3 (dodatek importu) – w przygotowaniu.

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
