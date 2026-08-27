# Generator XML DefinicjaElementu

Dokumentacja biznesowa i techniczna mechanizmu generującego pliki importu XML
(`session/DefinicjaElementu`, business.xml) dla definicji elementów kadrowo-płacowych
w enova365.

## 1. Do czego służy mechanizm

Definiowanie elementów listy płac (dodatków, potrąceń) ręcznie w enova, element po
elemencie, jest pracochłonne przy większej ich liczbie. Mechanizm w tym folderze
(`DefinicjaElementuXmlBuilder`) pozwala wygenerować programowo plik XML zgodny ze
schematem `http://www.soneta.pl/schema/business`, który enova importuje jako gotowe
definicje elementów — bez klikania w każdym polu formularza osobno.

Wejściem jest lista obiektów `DefinicjaElementuDane` (jeden na element listy płac)
z ustawionymi tylko tymi polami, które mają się różnić od wzorca; wyjściem — plik XML
gotowy do importu (Czynności → Import z pliku XML w enova, moduł Kadry i Płace).

## 2. Skąd wzięła się struktura pól

Struktura, kolejność elementów XML i wartości domyślne pochodzą z analizy realnego
eksportu enova dla elementu „Dodatek funkcyjny” (kreator algorytmu, płatny z dołu,
naliczany co miesiąc):

- `DefElementow_wzorzec.xml` — wzorcowy plik session/DefinicjaElementu wyeksportowany
  z enova (w tym samym folderze, zdekodowany do UTF-8 dla czytelności w repozytorium;
  oryginalny eksport z enova jest w UTF-16, patrz pkt 5).
- `DefinicjaElementu_pola.xlsx` — słownik wszystkich 194 pól tego elementu: ścieżka
  pola w XML, wartość we wzorcu, uwagi biznesowe (np. „do kiedy obowiązuje”, „kod
  zewnętrzny”, „GUID referencji, do jakiej listy płac należy”).

Domyślne wartości właściwości w `DefinicjaElementuDane` i klasach pochodnych są
zgodne z wzorcem 1:1 — nowy element utworzony z samymi wartościami domyślnymi
(`new DefinicjaElementuDane { Nazwa = ..., Skrot = ... }`) odtwarza dokładnie tę samą
konfigurację algorytmu, deklaracji, zaokrągleń itd. co „Dodatek funkcyjny”, zmieniając
tylko pola jawnie nadpisane przez wywołującego.

## 3. Struktura modelu (sekcje pól)

Model w pliku `DefinicjaElementuXmlBuilder` odzwierciedla sekcje z
`DefinicjaElementu_pola.xlsx`:

| Sekcja | Klasa danych | Uwagi |
|---|---|---|
| 1. Dane podstawowe | `DefinicjaElementuDane` (pola górnego poziomu) | Nazwa, Skrót, Kolejność, RodzajŹródła (Dodatek/Potrącenie), Zatrudnienie (Etat/UmowaCywilnoprawna) |
| 2. OkresNaliczania | `OkresNaliczaniaDane` | Kiedy i jak często element jest naliczany |
| 3. Algorytm (+ KreatorAlgorytmu) | `AlgorytmDane`, `KreatorAlgorytmuDane`, `WspolczynnikDane`, `CzasKreatoraDane`, `KorektaDane`, ... | Najbardziej rozbudowana sekcja — sposób wyliczania kwoty/podstawy, korekty za nieobecności |
| 4. Deklaracje | `DeklaracjeDane` (+ `KosztyDane`, `ZaliczkaDane`, `UlgaDane`, `SpoleczneDane`, `ZdrowotneDane`, `NarzutyDane`, `PodstawaSkladekDane`) | PIT, ZUS, składki, RCA/RSA/RP7 |
| 5. Nieobecności | `NieobecnosciDane` (+ `UrlopNieobDane`, `ZasilkiDane`) | Wliczanie do podstawy urlopu, ekwiwalentu, zasiłków |
| 6. Zaokrąglenie | `ZaokraglenieDane` | Precyzja i sposób zaokrąglania |
| 7. GUS | `GusDane` | Kategoria sprawozdawcza GUS |
| 8. Rozliczenie | `RozliczenieDane` | Ewidencja, wspólna płatność |
| 9. Zajęcie (komornicze) | `ZajecieDane` | Traktowanie w zajęciach komorniczych/alimentacyjnych |
| 10. Flagi top-level | `DodatkoweDane`, `PracownicyZaGranicaDane` + pola bezpośrednio w `DefinicjaElementuDane` | Korygowany, DoWypłaty, StanowiKUP itd. |
| 11. RuntimeInfo | generowane automatycznie jako puste | Pomijane przy imporcie — enova sama je uzupełnia |

Pełny, ostemplowany opis każdego pojedynczego pola (ścieżka, znaczenie biznesowe)
znajduje się w `DefinicjaElementu_pola.xlsx`.

## 4. Sposób użycia

```csharp
using Enova.KadryPlace.DefinicjeElementow;

var elementy = new[]
{
    new DefinicjaElementuDane
    {
        Nazwa = "Dodatek stażowy",
        Skrot = "Dod.staż.",
        Kolejnosc = 60,
    },
    new DefinicjaElementuDane
    {
        Nazwa = "Potrącenie za obiady",
        Skrot = "Potr.obiady",
        RodzajZrodla = "Potracenie",
        Kolejnosc = 70,
        Algorytm = { Potracenie = true },
    },
};

DefinicjaElementuXmlBuilder.Zapisz(elementy, @"C:\import\DefElementow.xml");
```

Każdy nieustawiony jawnie parametr przyjmuje wartość taką, jak we wzorcu „Dodatek
funkcyjny” (patrz pkt 2-3). Pola `Algorytm.DodPodstawa` i `Algorytm.ElPodstawa1`
(podpisy pomocnicze kreatora algorytmu) są uzupełniane automatycznie na podstawie
`Nazwa`, tak jak robi to enova przy tworzeniu elementu w kreatorze — nie trzeba ich
ustawiać ręcznie.

## 5. Zasady formatowania wartości (ważne dla poprawności importu)

- **Flagi logiczne** (`Blokada`, `Potracenie`, `Odchylki`, itd.) → tekst `True`/`False`
  (typ `bool` w modelu).
- **Liczby całkowite** (`Kolejnosc`, `Opoznienie`, `Ilosc`, `Priorytet`, `Liczba`,
  `Dni`) → typ `int`, zapisywane bez separatora tysięcy.
- **Kwoty** (`Algorytm.Podstawa`) → typ `decimal`, format `0.00 PLN` (kropka jako
  separator dziesiętny, spacja przed „PLN”).
- **Procenty** (`Koszty.Procent`, `Zaliczka.Procent`) → typ `decimal`, format `0.00%`.
- **Wartości słownikowe/enumeracyjne** (np. `RodzajZrodla`, `Zatrudnienie`,
  `OkresNaliczania.Naliczanie`, `Algorytm.KreatorAlgorytmu.PodstawaZa`) →
  **dokładne stałe tekstowe ze zdefiniowanymi polskimi znakami diakrytycznymi**,
  np. `PłatnaZDołu`, `CoNMiesięcy`, `BezWspółczynnika`, `Naliczać`, `Domyślnie`.
  Enova rozpoznaje te wartości jako ścisłe stałe — literówka lub brak polskiego
  znaku (np. `Naliczac` zamiast `Naliczać`) powoduje odrzucenie wartości przy
  imporcie. Domyślne wartości w modelu są przepisane 1:1 z wzorca — przy nadpisywaniu
  ich ręcznie (np. innym trybem naliczania) należy sprawdzić dokładną pisownię w
  `DefinicjaElementu_pola.xlsx` lub bezpośrednio w enova (podgląd innego elementu z
  takim ustawieniem i eksport do XML).
- **Pola-referencje pozostawione puste we wzorcu** (`Podstawowa`, `Wydzial`, `Kod`,
  `Zakres`, `Strefa`, `DefinicjaLimitu`, itd.) → `string` domyślnie `null`, co
  generator zapisuje jako pusty, samozamykający się element (`<Kod />`), zgodnie ze
  wzorcem.

## 6. Kodowanie pliku wyjściowego

Nagłówek wzorca deklaruje `encoding="Unicode"`, co w eksportach enova oznacza UTF-16.
`DefinicjaElementuXmlBuilder.Zapisz(...)` zapisuje plik w tym samym kodowaniu
(`Encoding.Unicode`, UTF-16 LE z BOM) — plik z inną deklaracją enova może odrzucić
przy imporcie lub błędnie zinterpretować znaki diakrytyczne.

## 7. Scenariusze testowe

| Lp | Scenariusz | Oczekiwany wynik |
|---|---|---|
| 1 | `new DefinicjaElementuDane { Nazwa = "Dodatek funkcyjny", Skrot = "Dod.funk.", Kolejnosc = 50 }`, `Id = "DefinicjaElementu_25"` | XML strukturalnie i wartościowo identyczny z `DefElementow_wzorzec.xml` (poza atrybutem `guid`, który jest losowy) |
| 2 | Kilka elementów w jednym wywołaniu `Zapisz` | Plik zawiera jeden `<session>` z wieloma węzłami `<DefinicjaElementu>`, każdy z unikalnym `id` (`DefinicjaElementu_1`, `_2`, ...) i unikalnym `guid` |
| 3 | `RodzajZrodla = "Potracenie"`, `Algorytm.Potracenie = true` | Element wygenerowany jako potrącenie, zgodnie z ustawieniami przekazanymi przez wywołującego |
| 4 | Element bez ustawionej `Nazwa` | `DodPodstawa` i `ElPodstawa1` pozostają puste (nie da się wyprowadzić podpisu z pustej nazwy) — pole `Nazwa` jest w praktyce wymagane biznesowo, mechanizm tego nie waliduje |
| 5 | Ręczne ustawienie `Id`/`Guid` | Generator używa podanych wartości zamiast auto-numeracji/losowania |

## 8. Ograniczenia

- Model pokrywa dokładnie strukturę zaobserwowaną we wzorcu „Dodatek funkcyjny”
  (kreator algorytmu, typ podstawy „Kwota”). Elementy oparte o inny typ algorytmu
  (np. „Wzór” zamiast „Kreator Algorytmu”) mogą wymagać innych węzłów wewnątrz
  `Algorytm` niż `KreatorAlgorytmu` — nie są tu modelowane i wymagałyby rozszerzenia.
- Pola `Progi` i `Definicja_relacji_odbiorcy_elementu` są przechowywane jako surowy
  `string` (we wzorcu puste) — ich wewnętrzna struktura XML (dla elementów progowych/
  relacji odbiorcy) nie jest odtworzona.
- Generator nie waliduje logiki biznesowej (np. spójności `RodzajZrodla` z
  `Algorytm.Potracenie`, czy istnienia GUID-ów referencji typu `DefinicjaListyPlac`
  w docelowej bazie) — odpowiada wyłącznie za poprawność struktury i formatu XML.
  Weryfikacja biznesowa (czy element się poprawnie przelicza) odbywa się dopiero po
  imporcie do enova.
- Brak środowiska .NET w tym repozytorium (brak `dotnet`) — kod nie był
  kompilowany/testowany automatycznie w tym repo; został zweryfikowany ręcznie
  pole po polu względem `DefElementow_wzorzec.xml` (kolejność elementów i dokładne
  wartości tekstowe, łącznie ze znakami diakrytycznymi).
