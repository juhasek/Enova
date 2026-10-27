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
~/.dotnet10/dotnet.exe test Dodatek/Testy/ImportDefinicjiElementow.Testy.csproj -c Release   # 19 testów
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

## Wersje

| Wersja | Gdzie | Co |
|---|---|---|
| **1.0.0** | tag `ImportDefinicjiElementow-v1.0.0`, gotowy plik `Formularz/dist/v1.0.0/formularz-elementow-wynagrodzenia.html` | ankieta 1.0 (16 pytań ogólnych, jedno na ekran), formularz klienta ze stanami pól |
| **2.0** (bieżąca, `src/`) | `Formularz/dist/formularz-elementow-wynagrodzenia.html` | ankieta 2.0: 172 pytania o **wszystkie 201 pól**, w 11 rozdziałach; wdrożeniowiec wybiera pytania dla klienta |

Format pliku `elementy-wynagrodzenia.json` i dodatek importu są wspólne dla obu wersji (ankieta 2.0 zapisuje `ankieta.wersja = "2.0"`).
Plik z wersji 1.0 wczytuje się do 2.0 (pola z pytań „pola” przechodzą jako zmienione przez klienta).

### Ankieta 2.0 (`schema/ankieta.json`)

- `rozdzialy` – kroki kreatora (Podstawowe, Wysokość, Kiedy, Nieobecności, Urlop i zasiłki, Podatek i ZUS, Wypłata i potrącenia,
  Sprawozdania, Składniki szczególne, Techniczne, Uwagi); klient przechodzi rozdział po rozdziale, rozdział bez pytań jest pomijany.
- Pytania `wybor` (z 1.0) – kafelki odpowiedzi ustawiające naraz wiele pól (szablon + `ustaw`).
- Pytania `pole` (jedno pole, opcjonalnie `etykiety` wartości w języku kadrowym) i `pola` (kilka pól, np. podstawy zasiłków) –
  odpowiedź = wartość pola; enum do 8 wartości i Tak/Nie jako kafelki, reszta jako zwykła kontrolka. Pytanie pojawia się tylko,
  gdy pole jest aktywne przy wcześniejszych odpowiedziach. Każde pytanie ma „Nie wiem” (lista do wyjaśnienia).
- Pierwszeństwo: szablon → odpowiedzi `wybor` → pola zmienione przez użytkownika (odpowiedzi na pytania o pola i zmiany ręczne, `reczne`).
- **Wdrożeniowiec:** „Formularz dla klienta” → zakładka **Pytania ankiety**: stan każdego pytania (lub całego rozdziału) – *Ukryte* /
  *Do wglądu* / *Klient odpowiada* / *Klient musi odpowiedzieć* (bez „Nie wiem”). Pytanie o pola dzieli stan z tymi polami
  (zakładka *Pola definicji*). Na pytania ukryte odpowiada wdrożeniowiec na definicjach przed wysłaniem; ukryte bez odpowiedzi
  nie blokują klienta. Pola z pokazanych pytań nie powtarzają się na stronie definicji klienta.
- **Bez dublowania:** pytanie szczegółowe o pole, które ustawia pokazane pytanie ogólne (np. pomniejszenia za chorobę), pojawia się
  dopiero po odpowiedzi „Inaczej” (także gdy wdrożeniowiec je ukrył) albo gdy pole już zmieniono. Zmiana odpowiedzi ogólnej sprzecznej
  z polami zmienionymi wcześniej pyta: „Zostaw moje ustawienia” / „Zastosuj nową odpowiedź”.
- **Zestawy pytań** (konfiguracja): *Podstawowy* (pytania ogólne + nazwa, kwota, procent…, flaga `podstawowe`), *Rozszerzony*
  (+ 24 pytania `zestaw: "rozszerzona"`), *Pełny*; potem stany można zmieniać pojedynczo. Rozdział „Ustawienia techniczne”
  ma `tylkoWdrozeniowiec` – klient go nigdy nie widzi.
- **Świadome potwierdzenie:** pytanie o pola „Klient musi odpowiedzieć” z wartością podpowiedzianą (szablon, wdrożeniowiec)
  trzeba potwierdzić przyciskiem „Potwierdzam” albo zmienić – do tego czasu „Dalej” i „Zakończ ankietę” są zablokowane.
- Test `ankieta.test.mjs` pilnuje m.in., że ankieta pyta o każde pole schematu (`niepokryte` = []).

## Formularz dla klienta

Jeden plik `Formularz/dist/formularz-elementow-wynagrodzenia.html` – działa offline, z dysku, w Edge/Chrome/Firefox,
bez instalacji i bez serwera. Dane zostają na komputerze klienta (szkic w `localStorage` przeglądarki).

**Dwa tryby.** Plik z `dist/` to **pełny formularz wdrożeniowca** (wszystkie 201 pól, zakładki jak w enova).
Klient dostaje **okrojony formularz**, który wdrożeniowiec generuje z pełnego:

1. Wdrożeniowiec ustawia licencję, przygotowuje definicje (z szablonów) i ustawia pola techniczne.
2. Przycisk **„Formularz dla klienta”** → dla każdego pola (lub całej sekcji): *Ukryte* / *Do wglądu* /
   *Klient może zmienić* / *Klient musi uzupełnić*, opcja „Klient może dodawać i usuwać definicje”, wyszukiwarka pól.
3. **„Pobierz formularz dla klienta”** → `formularz-elementow-wynagrodzenia-klient.html` – ta sama strona z wbudowaną
   konfiguracją (`<script id="konfiguracja">`: licencja, stany pól, wstępnie wypełnione definicje; uwagi wdrożeniowca nie trafiają).
4. Klient widzi jedną stronę na definicję: tylko wskazane pola, pogrupowane nagłówkami zakładek enova, z opisami i „?”;
   na dole „Uwagi dla wdrożeniowca”. Licencja zablokowana. Pole „musi uzupełnić” bez wartości = błąd (Tak/Nie jako lista
   do świadomego wyboru). Błędy pól ukrytych/do wglądu nie blokują klienta (podsumowanie: „do sprawdzenia przez wdrożeniowca”).
5. Klient klika „Zapisz plik dla wdrożeniowca” → `elementy-wynagrodzenia.json` w **tym samym formacie** co dotąd:
   wartości wszystkich aktywnych pól (także ukrytych – ustawionych przez wdrożeniowca), `bledy` z pełnej walidacji.
   Import w dodatku bez zmian; plik klienta można też wczytać do pełnego formularza.

**Informacje dla klienta:** ogólna (widok „Formularz dla klienta” → pasek na górze formularza klienta) i do każdej
definicji (zakładka „Uwagi i akceptacja” → ramka na górze strony definicji u klienta).

**Akceptacja klienta:** na dole strony definicji klient wpisuje imię i nazwisko i klika „Akceptuję definicję”
(niedostępne, gdy definicja ma błędy). Akceptacja zapamiętuje odcisk eksportowanych wartości i uwag – każda późniejsza
zmiana ją unieważnia („akceptacja nieaktualna”). W pliku: `definicje[].akceptacja = { osoba, data }` albo `null`,
na liście znaczek „✓ zaakc.”. Import w enova **tylko ostrzega** o niezaakceptowanych (komunikat „Akceptacja”, licznik
w podglądzie, kolumna w raporcie HTML) – nie pomija ich.

Pole ukryte nadal podlega regułom schematu: jeśli wybór klienta je aktywuje, obowiązuje wartość wdrożeniowca albo domyślna.
Szkic klienta w `localStorage` ma osobny klucz (id konfiguracji), więc nie miesza się z pracą wdrożeniowca.

Plik JSON można wczytać z powrotem do formularza („Wczytaj plik…” – zastąp lub dołącz), np. po uwagach wdrożeniowca.
Format pliku: `format`, `wersjaSchematu`, `wersjaEnova`, `dataEksportu`, `ustawienia.licencja`,
`definicje[]` z `id`, `szablon`, `uwagi`, `informacja`, `akceptacja`, `wartosci` (tylko pola aktywne, w kolejności schematu) i `bledy`
(komunikaty walidacji w chwili zapisu). Import odrzuca plik o innej głównej wersji schematu.

**Szablony startowe** budowane są z definicji standardowych enova (`zrodla.json` → `szablony`, wartości z zasobów
`*.dbinit.xml` po przekształceniu pól rekordu na pola formularza – `Generator/Szablony.cs`): wynagrodzenie zasadnicze,
premia uznaniowa, premia procentowa, dodatek stażowy (z progami), dodatek funkcyjny, potrącenie.

### Build i testy

```bash
node Formularz/build.mjs                        # schemat + src → dist (po każdej zmianie schematu lub src)
node --test Formularz/test/warunki.test.mjs     # silnik reguł na prawdziwym schemacie
node Formularz/test/e2e-edge.mjs                # Edge/Chrome headless: szablon, warunki, eksport, import, autozapis, formularz klienta + zrzuty
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
