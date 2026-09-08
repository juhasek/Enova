# A1PelnaListaPlacAddon — skompilowany dodatek enova365

Czynność **„Pełna lista płac (XLSX)"** na liście **Płace → Listy płac** (działa na
zaznaczonych pozycjach). Generuje plik `.xlsx` bezpośrednio biblioteką
`DevExpress.Spreadsheet` — **czysta siatka komórek, bez scalania/rozjeżdżania
kolumn**, którego nie dało się wyeliminować przy eksporcie report-snippetu z
podglądu wydruku (zob. `Raporty/A1PelnaListaPlac.md`).

Dane (kolumny: Kod / Imię i Nazwisko / Wydział + po jednej na każdą definicję
elementu + 17 stałych kolumn ZUS/PPK/PIT + Kwota do wypłaty) — logika przeniesiona
1:1 z `Raporty/A1PelnaListaPlacSnippet`.

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
5. **Płace → Listy płac** → zaznacz 1+ pozycji → **Czynności → „Pełna lista płac
   (XLSX)"** → pobierz plik.

### Szybki wariant (bez konfiguracji, ginie przy aktualizacji)

Skopiuj `A1PelnaListaPlacAddon.dll` wprost do:
- `C:\enovaServer\2512.5.6\Soneta.Products.Server.Standard\`
- `C:\enovaServer\2512.5.6\Soneta.Products.Web.Standard\`

i zrestartuj usługi. Działa, jeśli enova skanuje katalog bazowy komponentu —
`ExtPath` jest pewniejszy.

## Weryfikacja po wgraniu

- Czynność pojawia się w menu na liście płac (jeśli nie — dodatek się nie
  załadował: zły `ExtPath` / nie zrestartowano / niezgodna wersja net/enova).
- Wygenerowany `.xlsx`: jedna spójna tabela, **każda kolumna = jedna kolumna
  Excela** (bez scaleń), liczby jako liczby (`=SUMA(...)` działa), nagłówek
  pogrubiony i zamrożony, autofiltr.
- Sumy w kolumnach ZUS/PPK/PIT zgodne z paskiem wypłaty (na realnie przeliczonej
  liście płac).

## Aktualizacja kodu

Zmień `A1PelnaListaPlacWorker.cs`, `dotnet build -c Release`, podmień DLL w
`ExtPath`, zrestartuj usługi.
