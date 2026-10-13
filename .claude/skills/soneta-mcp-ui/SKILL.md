---
name: soneta-mcp-ui
description: >-
  Obsługa programów Soneta (enova365, Triva) przez narzędzia MCP soneta_ui. Nawigacja po modułach,
  przeglądanie list, otwieranie formularzy, edycja danych, dodawanie obiektów, uruchamianie czynności.
  Używaj tego skilla ZAWSZE gdy użytkownik: (1) prosi o odczytanie, wyświetlenie lub wyszukanie
  danych w enova365/Triva (np. kontrahenci, faktury, pracownicy, towary, stany magazynowe);
  (2) chce dodać, edytować lub przeglądać obiekty w programie Soneta; (3) pyta o nawigację
  w modułach enova365 (Handel, Kadry, Księgowość, CRM, itp.); (4) wspomina 'otwórz w enova',
  'pokaż listę', 'znajdź kontrahenta', 'dodaj fakturę', 'sprawdź stan magazynu', 'pokaż pracowników';
  (5) chce wykonać operację na danych ERP przez MCP; (6) chce uruchomić czynność/akcję na obiektach
  (księgowanie, wysyłka KSeF, zatwierdzenie deklaracji, eksport, generowanie raportu, zamknięcie
  okresu); (7) chce wybrać/zmienić bazę danych w enova365/Triva.
version: "1.0"
---

# Obsługa programów Soneta (enova365 / Triva) przez MCP

Skill umożliwia pełną interakcję z działającą instancją enova365 lub Triva poprzez narzędzia MCP `soneta_ui`.
Pozwala przeglądać dane, nawigować po modułach, edytować formularze i dodawać nowe obiekty.

## Konteksty (kind)

Operator znajduje się w jednym z czterech kontekstów. Część narzędzi działa tylko w określonym kontekście — kolumna **Wymaga** w tabeli narzędzi wskazuje gdzie.

| kind | Co oznacza |
|---|---|
| `list` | otwarty folder z widokiem listy obiektów |
| `form` | otwarty formularz obiektu |
| `subform` | otwarty podformularz |
| `menu` | folder z widokiem menu podfolderów |
| `—` | dowolny kontekst |

Aktualny kontekst sprawdzisz przez `where_am_I`.

## Zakres odpowiedzi — parametr `detail`

Narzędzia oznaczone w tabeli parametrem `detail?` zwracają stan okna operatora. Pełny render okna to
zwykle największa odpowiedź w całej sesji — dlatego **domyślnie dostajesz sam nagłówek, a szczegóły
dobierasz dopiero wtedy, gdy są potrzebne**:

| `detail` | Co wraca | Kiedy |
|---|---|---|
| `header` (domyślnie) | rodzaj i tytuł okna, `id` obiektu, aktualna zakładka, ścieżka folderu | **wartość wyjściowa** — cała nawigacja i wszystkie kroki pośrednie |
| `full` | pola formularza, kolumny i **wiersze** listy, filtry, dostępne metody | pojedyncze wywołanie, w którym faktycznie czytasz dane albo potrzebujesz `fieldPath`/`gridPath`/`commandPath` |
| `none` | samo potwierdzenie `{"kind":"message","message":"OK"}` | krok, którego wyniku nie musisz oglądać (przejście zakładki, zamknięcie formularza) |

**Nie podawaj `full` „na zapas".** Wchodząc na listę czy formularz zwykle jeszcze nie wiesz, czy
będziesz czytać dane — najpierw zobacz z nagłówka, gdzie jesteś, a po treść sięgnij osobnym
wywołaniem. Sekwencja `navigate_to_folder` → `retrieve_list` → `open_form` → `switch_form_page`
prowadzona na domyślce kosztuje ułamek tego, co ta sama sekwencja z `full` na każdym kroku.

**Jak dobrać szczegóły później** — nie powtarzaj metody, która coś zmienia (`add_object`,
`execute_action`, `invoke_command`, `update_field_value`, `accept_*`): drugie wywołanie wykonałoby
tę operację ponownie. Bezpieczne są dwa odczyty:

| Czego brakuje | Czym dobrać |
|---|---|
| pola formularza, `fieldPath`, `gridPath`, `commandPath`, kolumny i filtry listy, menu folderów | `where_am_I(detail:"full")` — renderuje aktualne okno, niczego nie zmieniając |
| wiersze listy w folderze operatora | `retrieve_list(detail:"full")` — ten sam odczyt, bez skutków ubocznych |
| dane list **bez** zajmowania okna operatora | `retrieve_folder_list` / `retrieve_object_values` — patrz sekcje niżej; często tańsze niż pełny render okna |

Błędy walidacji są wyjątkiem od skracania: weryfikatory wracają zawsze, również przy `none`.

## Narzędzia MCP `soneta_ui`

| Narzędzie                                                             | Wymaga | Wynik | Opis |
|-----------------------------------------------------------------------|---|---|---|
| **Eksploracja**                                                       | | | |
| `get_modules`                                                         | — | — | Lista modułów programu (Handel, Kadry, Księgowość, …) — punkt startowy |
| `get_folders(programFolderPath, regexFilter?, limit?)` | — | — | Podfoldery wskazanego modułu/folderu. Bez `regexFilter` — jeden poziom (podmenu `type=folders`); z `regexFilter` — rekurencyjnie w głąb po ścieżce. `limit` (domyślnie 15; `0`=wszystko) — wynik zawiera `count` (liczba pasujących) i `truncated` |
| `get_configuration_folders(regexFilter?, limit?)`      | — | — | Lista folderów konfiguracyjnych (ustawienia, definicje, role, operatorzy); te same zasady filtrowania i `limit`/`count`/`truncated` co `get_folders` |
| `get_object_structure(tableName, pageFilter?, fieldFilter?, pagesOnly?, includeSystemPages?, includeFeatures?, includeLists?, includeGridColumns?, onlyEditable?, className?, limit?, fieldLimit?)` | — | — | Zakładki, sekcje danych i pola obiektu wprost z definicji formularzy — **bez otwierania okna**. Odpowiada na „jakie pola ma ten obiekt", „jak nazywa się pole", „na której zakładce leży". Pole identyfikuje `dataPath` (patrz niżej — to NIE `fieldPath`). `pagesOnly=true` daje samą listę zakładek z `fieldCount`/`listCount`; `pageFilter`/`fieldFilter` zawężają (regex); `className` wskazuje odmianę obiektu w tabeli z selektorem. `limit` (domyślnie 15; `0`=wszystko) i `fieldLimit` (domyślnie 60; `0`=wszystko) — wynik zawiera `count`, `truncated` i per zakładka `fieldsTruncated` |
| `retrieve_object_values(ids, fields?, fieldsAsRegex?, pages?, fieldLimit?, rowLimit?)` | — | — | Wartości wskazanych pól dla wskazanych obiektów — **bez otwierania formularza**, wiele obiektów jednym wywołaniem. Pola wskazuje `fields` (`dataPath` z `get_object_structure`) albo `pages` (całe zakładki); bez obu — pola pierwszej zwykłej zakładki. Z `fieldsAsRegex=true` wpisy w `fields` są **wyrażeniami regularnymi dopasowywanymi do ścieżki ORAZ do etykiety** — pozwala pobrać dane bez czytania struktury; razem z `pages` zawężają zakres do tych zakładek. Ścieżka z dwukropkiem to podlista (`"Pozycje"` = lista z kolumnami, `"Pozycje:Ilosc"` = jedna kolumna), wraca w `lists` po `rowLimit` wierszy (domyślnie 15; `0`=wszystko) jako `rowsCsv`/`columnsCsv` — patrz „Tabele w odpowiedziach". `fieldLimit` (domyślnie 60; `0`=wszystko) to **wspólny budżet** pól obiektu i kolumn wszystkich podlist — pola idą pierwsze, a po wyczerpaniu budżetu kolejne podlisty są pomijane w całości; po ucięciu `fieldsTruncated=true` i ostrzeżenie z liczbą pominiętych. Ścieżki bez wartości wracają rozdzielone wg przyczyny: `hiddenFields` (ukryte dla tego obiektu), `absentFields` (nieobecne w tej odmianie), `failedFields` (mapa ścieżka→powód awarii). Nieznana ścieżka i wzorzec bez trafień są pomijane z wpisem w `warnings` (odczyt reszty trwa); nieznana zakładka w `pages` i nieprawidłowy wzorzec przerywają wywołanie |
| `get_folder_structure(programFolderPath, includeFilters?, columnLimit?)` | — | — | Opis listy folderu programu — **bez otwierania folderu operatorowi**: `tableName` jej wierszy, `filtersCsv` (filtry widoku z **domyślnymi wartościami**, od których startuje `retrieve_folder_list`; `filterPath` do parametru `filters`), `columnsCsv` (kolumny widoku; `path` do `fields` i do filtrowania), `selector`, domyślny `sort` i `isTree`. Miejsce startowe przed `retrieve_folder_list`. `includeFilters=false` pomija panel filtrów, `columnLimit` (domyślnie 40; `0`=wszystko) ucina kolumny (`columnsTruncated`) |
| `retrieve_folder_list(programFolderPath, filters?, fields?, sort?, pageSize?)` | — | — | Dane listy wskazanego folderu — **bez zajmowania okna operatora**; wiersze te same, które zobaczyłby on na liście (filtry i prawa widoku), ale filtrować i wyświetlać można po **dowolnych polach obiektu**. `filters` w postaci `["ścieżka=wartość"]`, gdzie ścieżka to `filterPath` filtru widoku albo pole obiektu (`path` kolumny lub `dataPath` z `get_object_structure`); wartość jak w filtrze kolumny: `"ABC"`=zaczyna się, `"=ABC"`=dokładnie, `"*ABC"`=zawiera, `"A..C"`=zakres, `"-"`=puste, `"*"`=niepuste. `fields` (dataPath) **zastępuje** kolumny listy. Ścieżki w `fields` i `filters` muszą pochodzić ze struktury obiektu (`get_object_structure`, także cechy i kolumny list) albo z kolumn listy (`get_folder_structure`); ścieżka może przechodzić na obiekt wskazywany polem — `Kontrahent.Adres.Miejscowosc` jest sprawdzane w strukturze **kontrahenta**. Pole spoza struktury jest pomijane z ostrzeżeniem, filtr spoza struktury przerywa wywołanie. `sort` np. `"Data desc, Kod"`. Wynik: `columnsCsv`/`rowsCsv` (kolumna `#id` → `open_form`, `retrieve_object_values`), `pagesInfo` (EmptyList/ThereIsMore/LastPage) i `listHandle` do kontynuacji |
| `aggregate_folder_values(programFolderPath, groupBy?, aggregates?, filters?, sort?, maxRows?, pageSize?)` | — | — | **Liczy** dane listy folderu zamiast je wypisywać — odpowiedź na „ile", „za ile", „wg czego" bez ściągania wszystkich stron. `groupBy` (ścieżki pól) tworzy grupy; puste = jedna grupa z podsumowaniem całej listy. `aggregates` to wpisy `"ścieżka=funkcja"` (`count`, `sum`, `min`, `max`, `avg`; kilka funkcji po przecinku, np. `"Wartosc=sum,avg"`); bez nich wraca sama liczność. **Dwukropek sięga do kolekcji wiersza**: `"Pozycje:Ilosc"` liczy wartości z pozycji, a nie z dokumentu — wtedy jednostką liczenia jest wiersz kolekcji, a pola bez dwukropka są dla wszystkich jego pozycji takie same; kolekcja może być tylko **jedna** na wywołanie. Wartości z symbolem (kwota w walucie, ilość w jednostce) agregowane są po samej liczbie, a symbol dochodzi do grupowania jako kolumna `"<ścieżka>#symbol"` — złotówki nie sumują się z euro. Kolumna `#count` (liczba rekordów w grupie) jest zawsze. `filters` jak w `retrieve_folder_list`, ze ścieżką z dwukropkiem zawężającą wiersze kolekcji. `sort` porządkuje **wynik** po kolumnach wyniku (np. `"Wartosc=sum desc"`). Wszystkie ścieżki muszą pochodzić ze struktury obiektu (ścieżka może przechodzić na obiekt wskazywany polem, np. `Kontrahent.Adres.Miejscowosc`) — nieznana **przerywa** wywołanie. Wyjątkiem są pola kolekcji udostępnianych kodem: metadane nie znają typu ich wiersza, więc ścieżka idzie wprost do odczytu, a gdy nie da żadnej wartości, wraca ostrzeżenie. `maxRows` (domyślnie 10000) ogranicza skan; zatrzymanie na limicie daje `partial=true` — takich liczb nie podawaj jako wyniku. Wynik: `columnsCsv`/`rowsCsv`, `totalCount` (liczba grup), `scannedRows`, `listHandle` do kontynuacji |
| `retrieve_list_next_page(listHandle, pageSize?)` | — | — | Kolejna strona listy otwartej przez `retrieve_folder_list` albo grup z `aggregate_folder_values` — ten sam, spójny odczyt, więc wiersze nie powtarzają się ani nie giną. Po ostatniej stronie odczyt zamyka się sam: `listClosed=true` i brak `listHandle`. Naraz otwarte są najwyżej **3** odczyty — czwarty zamyka najdawniej używany |
| `close_list(listHandle)` | — | — | Zamyka odczyt otwarty przez `retrieve_folder_list` albo `aggregate_folder_values`. Potrzebne tylko przy przerwaniu czytania przed ostatnią stroną |
| `get_bi_data_models(area?, technicalModels?, regexFilter?, limit?)` | — | — | Lekka lista definicji modeli danych BI ("BI/Modele danych"): `modelID`, `name`, `area` oraz `requiredParams`/`optionalParams` (parametry wywołania potrzebne do uruchomienia modelu). `area` zawęża do obszaru merytorycznego; `technicalModels` (domyślnie `false`) — `false` pomija modele techniczne, `true` zwraca wszystkie; `regexFilter` filtruje po nazwie. `limit` (domyślnie 15; `0`=wszystko) — wynik zawiera `count` i `truncated` |
| `get_bi_data_model_info(modelID)`                                     | — | — | Pełna informacja techniczna o modelu danych: `modelID`, `name`, `area` oraz `technicalInfo` (rodzaj, wymiary, miary, parametry, sortowanie, przedział czasu). `modelID` z `get_bi_data_models` |
| `get_bi_areas(regexFilter?, limit?)`                   | — | — | Lista obszarów merytorycznych BI (`areaPath`, `name`, `parent`). `areaPath` przekazuje się jako parametr `area` do `get_bi_data_models`. `regexFilter` filtruje po ścieżce lub nazwie. `limit` (domyślnie 15; `0`=wszystko) — wynik zawiera `count` i `truncated` |
| `retrieve_bi_view(modelID?/name?, parameters?, limit?)`               | — | — | Wykonuje model danych BI (z `get_bi_data_models`) i zwraca dane jako tabele `rowsCsv` (nagłówek z nazw kolumn modelu) i `columnsCsv` (`caption`, `type` kolumn) — patrz „Tabele w odpowiedziach". Model wskazuje `modelID` (priorytet) albo `name`. `parameters` jak filtry w `retrieve_list`: `["nazwa=wartość", ...]` (nazwy z `technicalInfo`; wiele wartości po przecinku; referencje jako `id`, Guid lub ID wiersza). `limit` (domyślnie 100, maks. 1000) — `truncated=true` gdy wynik ucięty |
| `get_databases`                                                       | — | — | Lista baz danych dostępnych w systemie |
| `select_database(name)`                                               | — | list/menu | Wybór bazy — może wymagać interaktywnego logowania |
| `where_am_I(detail?)`                                                 | — | — | Aktualny kontekst operatora (list/form/subform/menu). Bezpieczny odczyt — **to nim dobierasz szczegóły** (`detail:"full"`) po wywołaniu, które zwróciło sam nagłówek |
| **Nawigacja i lista**                                                 | | | |
| `navigate_to_folder(programFolderPath, newTab?, detail?)`             | — | list/form/menu | Otwiera folder. `newTab=true` otwiera w nowej karcie (porównywanie danych) |
| `retrieve_list(filtersValues?, pageNumber?, detail?)`                 | list | list | Pobiera wiersze listy. `pageNumber` używaj w kolejności 0, 1, 2, … **Wiersze wracają tylko przy `detail:"full"`** |
| `select_rows(objectSelector, selectedIds?, detail?)`                   | list / lookup | list | Zaznacza rekord (zmiana `focusedId`); wybór z listy podpowiedzi `Kind=lookup`. `selectedIds` (tablica `id`) zaznacza wiele wierszy naraz — wracają w `selectedIds` odpowiedzi i to na nich zadziałają `get_actions`/`execute_action`; bez niego zaznaczony jest sam wiersz z `objectSelector` |
| **Formularze**                                                        | | | |
| `open_form(id, detail?)`                                              | list / — | form | Otwiera formularz obiektu wskazanego przez `id` z odpowiedzi `retrieve_list`, `retrieve_folder_list` albo `search_object`. Nazwa tabeli nie jest potrzebna — typ wynika z `id`, więc obiekt spoza wczytanej listy otwiera się tak samo |
| `search_object(tableName?, objectSelector?, programFolderPath?, filters?, limit?)` | — | — | Wyszukuje obiekty i zwraca `itemsCsv` — tabelę z nagłówkiem `#id\|#guid\|#caption` plus pola identyfikujące wiersz (patrz „Tabele w odpowiedziach"). Zakres wskazuje **albo** `tableName` (przeszukanie tabeli — nie wszystkie to potrafią), **albo** `programFolderPath` z `filters` (zakresem jest lista folderu, składnia filtrów jak w `retrieve_folder_list`). Z folderem `objectSelector` jest opcjonalny — bez niego obiekt wskazują same `filters`. Oba parametry razem: folder wygrywa, ale niezgodna tabela = błąd. **Nie otwiera formularza** — aby wyświetlić obiekt operatorowi, wywołaj potem `open_form(id)`. `limit` (domyślnie 15; `0`=wszystko, w trybie samych filtrów do pułapu 500) — wynik zawiera `count` i `truncated` |
| `add_object(objectName?, detail?)`                                    | list | form | Tworzy nowy obiekt w aktualnym folderze i otwiera jego formularz |
| `get_form_pages(regexFilter?, limit?)`                 | form/subform | — | Lista zakładek jako `pagesCsv` (nagłówek `pageID\|title` — patrz „Tabele w odpowiedziach") + aktywna zakładka. Bez `regexFilter` wszystkie; z `regexFilter` tylko zakładki pasujące po tytule/pageID. `limit` (domyślnie 15; `0`=wszystko) — wynik zawiera `count` i `truncated`. Wywołuj gdy potrzebujesz `pageID` |
| `switch_form_page(pageID, detail?)`                                   | form/subform | form/subform | Przełącza zakładkę formularza. Domyślnie potwierdza przełączenie; pola zakładki dobierz, gdy będą potrzebne |
| `update_field_value(fieldsValues=["fieldPath=value"], detail?)`      | form/subform | form/subform | Edytuje pola na aktualnej zakładce (tylko pola z `fieldPath`, oznaczone `edytowany`). **Parametr nazywa się `fieldsValues`** — element tablicy ma format `"fieldPath=value"` (`fieldPath` to nazwa pola, NIE nazwa parametru) |
| `lookup_list(fieldPath, detail?)`                                     | form/subform | lookup | Lista dozwolonych wartości dla pola jako `optionsCsv` (nagłówek `id\|value`, a dla list bez identyfikatorów samo `value`); brak pola = wolna wartość |
| `invoke_command(commandPath, moreActions?, detail?)`                 | form/subform | form/list/dialog | Klika przycisk (komendę) na aktualnej zakładce formularza. `commandPath` odczytaj z opisu komendy w renderze formularza (nie zgaduj). `moreActions=true` uruchamia dodatkowe akcje przypięte do przycisku (domyślnie `false`). Przyciski tylko do odczytu są pomijalne |
| `save_form`                                                           | form | list/form | Zapisuje formularz do bazy. Domyślnie NIE zapisuje sam — czeka, aż operator zapisze albo zamknie formularz; zwraca `"saved"` (zapisano) lub `"cancelled"` (operator anulował — formularz zostaje otwarty). Po `"cancelled"` przerwij lub spróbuj innej operacji. Z włączonym parametrem pakietu „Samodzielny zapis formularza" zapisuje natychmiast i zamyka |
| `cancel_form(detail?)`                                                | form | list | Zamyka formularz bez zapisu (również wycofuje zmiany na podformularzach) |
| **Podformularze (listy osadzone)**                                    | | | |
| `open_subform(gridPath, id, detail?)`                           | form/subform | subform | Otwiera podformularz obiektu z listy osadzonej (tylko gdy `canOpen=true`) |
| `add_subobject(gridPath, objectName?, detail?)`                       | form/subform | subform | Dodaje nowy obiekt na liście osadzonej (tylko gdy `canAdd=true`) |
| `edit_grid_rows(gridPath, operations, detail?)`                       | form/subform | form/subform | Hurtowa edycja listy w trybie edycji w miejscu: modyfikacja, dodawanie i usuwanie wierszy. Działa gdy lista ma `editInPlace`/`addInPlace`. `operations` to tablica obiektów: `{"new0":["Pole=Wartość",…]}` dodaje wiersz, `{"id":["Pole=Wartość",…]}` modyfikuje, `{"id":"delete"}` usuwa. Pole adresuje się segmentem `fieldPath` PO dwukropku (np. dla `fieldPath` `new TowarExtender.Ceny:Workers.Cena.Netto` użyj `Workers.Cena.Netto`), a NIE nagłówkiem kolumny (`caption`, np. `Netto`) — nagłówek zwróci błąd „Nie znaleziono kolumny" |
| `accept_subform(detail?)`                                             | subform | form | Zatwierdza podformularz i wraca do formularza nadrzędnego |
| **Czynności (akcje)**                                                 | | | |
| `get_actions(ids?)`                                                   | list/form | — | Lista czynności dostępnych dla obiektów listy (`ids=[…]`) lub formularza (`null`) jako `actionsCsv` (nagłówek `workerID\|caption`). Uwaga: `workerID` zawiera znak `\|` przed nazwą metody, więc w komórce jest on poprzedzony ukośnikiem (`\\\|`) |
| `execute_action(workerID, ids?, detail?)`                       | list/form | dialog / list / form | Uruchamia czynność; może otworzyć formularz parametrów. Pola tego formularza odczytaj przez `where_am_I(detail:"full")` — czynności nie uruchamiaj drugi raz |
| `accept_form(detail?)`                                                | dialog | — | Zatwierdza formularz parametrów uruchamianej czynności |
| **Załączniki**                                                        | | | |
| `list_attachments(id, limit?)`                       | — | — | Lista załączników obiektu: `attachmentID`, `name`, `description`, `mimeType`, `modified`, `visibleInNet` — bez treści. `limit` (domyślnie 15; `0`=wszystko) — wynik zawiera `count` i `truncated`. `id` z `retrieve_list`, `open_form` lub `search_object` |
| `get_attachment(id, name, contentFileName?)`                | — | — | Treść załącznika w Base64 (`contentBase64`) + `mimeType` i `size`. `name` odczytaj z `list_attachments`. `contentFileName` — patrz niżej: plik zapisuje klient, treść nie przechodzi przez rozmowę |
| `update_attachment(id, name?, contentBase64?, contentFileName?, description?, overwrite?)` | — | — | Dodaje załącznik o nazwie `name` albo podmienia treść istniejącego o tej samej nazwie. Bez `name` nazwą staje się nazwa pliku z `contentFileName`. `overwrite=false` tworzy nowy załącznik z unikalną nazwą zamiast nadpisywać. Zapisuje od razu do bazy (bez `save_form`) |
| **Okna dla operatora**                                                | | | |
| `show_content(caption, content, contentType?, contentFileName?, editable?, promptCaption?, buttons?, saveToFile?)` | — | tekst / prompt / akcja | Pokazuje operatorowi treść: `contentType` = `html`, `markdown` (sformatowany dokument — raport, zestawienie, podsumowanie), `text`, `xml` (tekst źródłowy, xml z podświetlaniem składni). `editable=true` daje treść do poprawienia — zawsze jako źródło, także dla `html` i `markdown`. `saveToFile` (sama nazwa pliku) dokłada przycisk zapisu treści do pliku lokalnego |
| `show_options_selector(caption, options, multiSelect?, description?, selected?, labelHeight?, promptCaption?, buttons?)` | — | wybór / prompt / akcja | Pyta operatora o wybór z listy: pojedynczy (radio) albo wielokrotny (`multiSelect=true`). Długie opisy opcji zawiniesz przez `labelHeight` |

### Jak czytać strukturę obiektu

`get_object_structure` opisuje **definicję** formularzy, a nie konkretny obiekt — nie otwiera okna
i nie czyta danych. Dwie przestrzenie adresowe, których nie wolno mieszać:

| Ścieżka | Skąd | Do czego |
|---|---|---|
| `dataPath` | `get_object_structure` | opis pola: gdzie leży w danych (`Kod`, `Adres.Ulica`, `Pozycje:Ilosc`) |
| `fieldPath` / `gridPath` | render otwartego formularza | edycja: `update_field_value`, `lookup_list`, `edit_grid_rows` |

Dwukropek w `dataPath` oddziela kolekcję od pola jej elementu; kolumny listy mają ścieżkę
relatywną do wiersza (pełna = `dataPath` listy + `:` + `path` kolumny).

Wynik jest **nadzbiorem** tego, co operator zobaczy dla konkretnego obiektu. Trzy flagi zakładki
mówią, czego nie da się rozstrzygnąć bez obiektu — i każda wymaga innej reakcji:

| Flaga | Co znaczy | Co z tym zrobić |
|---|---|---|
| `hasDynamicContent` | zawartość zakładki powstaje w kodzie (`Renderable`, metoda renderująca, szablon, dołączany fragment o dynamicznym źródle) — lista pól może być niepełna | po pełny stan sięgnij przez `open_form` |
| `conditional` | warunkowa jest **sama zakładka** (jej `Visibility`/`Renderable` zależy od danych) — pola są znane i kompletne, ale dla tego obiektu zakładki może nie być | nie szukaj brakujących pól; sprawdź, czy zakładka w ogóle jest, dopiero gdy jej potrzebujesz |
| `dynamicTitle` | tytuł jest wyliczany, więc `title` zawiera nazwę zakładki zamiast właściwego napisu | nic — na pola nie wpływa |

Typowa kolejność: `get_object_structure(tableName, pagesOnly=true)` → wybór zakładki →
`get_object_structure(tableName, pageFilter="^NazwaZakładki$")` → `retrieve_object_values` po
wartości → dopiero potem `open_form` i edycja przez `fieldPath` z rendera formularza.

Do **czytania danych** używaj `retrieve_object_values` — nie otwiera okna operatora, obsługuje wiele
obiektów naraz i przyjmuje te same `dataPath`. `open_form` zostaw na sytuacje, gdy chcesz coś
zmienić albo pokazać obiekt operatorowi.

### Wyszukanie obiektu: po tabeli czy w zakresie listy

`search_object` ma dwa tryby i wybór między nimi zmienia zarówno zakres, jak i to, po czym wolno
szukać:

| Tryb | Kiedy | Ograniczenie |
|---|---|---|
| `search_object(tableName, objectSelector)` | znasz kod/numer/guid i nie zależy Ci, na jakiej liście obiekt leży | tylko tabele z wyszukiwaniem globalnym (pracownicy, kontrahenci, towary) i tabele konfiguracyjne; selektor musi trafić w pole indeksowane |
| `search_object(programFolderPath, filters, objectSelector?)` | zakres ma odpowiadać konkretnej liście (okres, magazyn, kategoria dokumentu) **albo** obiekt rozpoznają wartości pól, a nie jedno pole selektora | wymaga folderu z listą (`get_folders`); bez `objectSelector` czyta wiersze przefiltrowanej listy, więc `count` to liczba zwróconych, nie wszystkich |

Tryb folderu jest jedynym sposobem na obiekty tabel bez wyszukiwania globalnego — np. dokument
odnajdziesz przez `search_object(programFolderPath: "Handel/Zakup/Faktury zakupu", filters:
["Numer.Pelny==FZ/1/2026"])`, bo `objectSelector` po numerze pełnym nie ma w tej tabeli indeksu.
Ścieżki folderów wraz z ich `tableName` i `selectorField` masz w [common-folders.md](./common-folders.md).

Gdy z listy potrzebujesz **jednego** obiektu, nie stronicuj jej `retrieve_folder_list` — filtry
podane do `search_object` dają ten obiekt od razu, z `id` gotowym do `open_form`
i `retrieve_object_values`.

### Listy: odczyt bez okna czy przez folder operatora

Do **czytania list** domyślnie używaj `get_folder_structure` + `retrieve_folder_list` — nie ruszają
widoku operatora, więc możesz czytać dowolny folder równolegle do tego, co on robi. Dają też więcej
niż lista w oknie: filtrować i zwracać można **dowolne pola obiektu** (`dataPath`
z `get_object_structure`), nie tylko standardowe kolumny widoku.

`navigate_to_folder` + `retrieve_list` zostaw na sytuacje, gdy operator ma **zobaczyć** listę albo
gdy potrzebujesz czegoś, co dzieje się w oknie: `add_object`, `select_rows`, `get_actions`
i `execute_action` na zaznaczonych wierszach.

Różnica w zakresie danych: `retrieve_folder_list` startuje od **domyślnych wartości filtrów
widoku**, więc nie dziedziczy filtrów, które operator zapisał sobie na liście (typ towaru, stan
magazynowy, okres, zakres dat) — i niczego mu nie nadpisuje. Lista w jego oknie może pokazywać co
innego. Wartości, od których zaczyna się odczyt, zwraca `get_folder_structure` w `filtersCsv` —
zajrzyj tam, gdy w wyniku brakuje obiektów, których się spodziewasz. Zawężaj jawnie przez `filters`,
a gdy potrzebujesz dokładnie tego, co widzi operator, użyj `navigate_to_folder` + `retrieve_list`.

Typowa kolejność: `get_folders` → `get_folder_structure(programFolderPath)` (jakie filtry i kolumny
ma lista) → `retrieve_folder_list(programFolderPath, filters, fields, sort)` →
`retrieve_list_next_page(listHandle)`, dopóki `pagesInfo` to `ThereIsMore`. Gdy wynik ma
`listClosed=true`, lista została przeczytana w całości i uchwyt wygasł. Zwrócone `id`
nadają się od razu do `retrieve_object_values` i `open_form`.

### Liczby zamiast wierszy

Gdy pytanie brzmi „ile", „za ile", „który największy", „w rozbiciu na co" — użyj
`aggregate_folder_values`, a nie stronicowania całej listy. Liczy platforma (te same agregatory,
co podsumowania list w programie), więc nie sumujesz sam sformatowanych tekstów i nie ryzykujesz,
że policzysz tylko pierwszą stronę:

* „ilu kontrahentów z Krakowa" → `groupBy: []`, `filters: ["Adres.Miejscowosc=Kraków"]` → `#count`;
* „wartość faktur wg kontrahenta" → `groupBy: ["Kontrahent"]`, `aggregates: ["BruttoCy=sum"]`,
  `sort: "BruttoCy=sum desc"`;
* „ile sztuk towaru zeszło na dokumentach" → `aggregates: ["Pozycje:Ilosc=sum"]` — dwukropek
  przenosi liczenie na pozycje dokumentów, których nie widać na liście głównej.

Kwoty i ilości wracają rozbite po symbolu (kolumna `#symbol`) — to nie jest nadmiarowa kolumna,
tylko warunek poprawności: bez niej suma mieszałaby waluty. Sprawdź `partial` w odpowiedzi:
`true` oznacza, że skan zatrzymał się na `maxRows` i liczby są niepełne.

Gdy nie znasz jeszcze ścieżek, `get_object_structure` możesz pominąć: wywołaj
`retrieve_object_values` z `fieldsAsRegex=true` i wzorcem opisującym to, czego szukasz — dopasowanie
idzie po ścieżce **i** po etykiecie pola, więc `["[Aa]dres"]` albo `["NIP|REGON"]` zadziała bez
znajomości struktury. Wynik jest ograniczony `fieldLimit`, a `fieldsTruncated=true` mówi, że wzorzec
był zbyt szeroki.

Ograniczenie widoczności, o którym trzeba wiedzieć: `retrieve_object_values` sprawdza widoczność
wynikającą z **logiki obiektu** (metody `IsVisible...`), a nie z układu formularza. Pole ukryte
warunkiem zapisanym w definicji formularza dostaniesz z wartością. Pełny obraz tego, co widzi
operator, daje tylko `open_form`.

Ścieżki, które nie dały wartości, wracają rozdzielone według przyczyny — każda wymaga innej reakcji:

| Lista | Co znaczy | Co z tym zrobić |
|---|---|---|
| `hiddenFields` | logika obiektu ukrywa pole dla **tego** obiektu; w tej odmianie ono istnieje | dla innego obiektu wartość może być — nie skreślaj ścieżki |
| `absentFields` | ta odmiana obiektu w ogóle nie ma takiego pola | nie pytaj o nie ponownie dla tego typu |
| `failedFields` | mapa ścieżka→powód awarii odczytu (także dla podlist) | pole może istnieć — spróbuj przez `open_form` |

Zakładka o `fieldCount=0` nie musi być pusta — gdy ma `listCount>0`, prezentuje kolekcję.
Kilka zakładek jednego obiektu może mieć tę samą nazwę przy różnych tytułach, więc wskazuj je
przez `pageName` **razem z** `title`. Prawa nadaje się na całe zakładki (`rightName`), nie na
pojedyncze pola.

### Jak działają załączniki

Serwer operuje **wyłącznie na treści zakodowanej Base64** — nie czyta i nie zapisuje plików.
Pliki obsługuje klient, rozpoznając je po **nazwach parametrów**: parametr `xxxFileName` wskazuje
plik, a treść trafia do `xxxBase64` (pliki binarne) albo `xxx` (pliki tekstowe). Reguła obowiązuje
w każdym narzędziu, które ma taką parę parametrów — np. `show_content` czyta plik tekstowy przez
`contentFileName` do `content`, a `import_business_xml` przez `xmlFileName` do `xml`;
`export_business_xml` nie przyjmuje treści, więc `xmlFileName` jest tam wynikiem — klient zapisze
do niego wyeksportowany dokument.

Czym są „pliki", zależy od tego, kto Cię uruchomił:

- **aplikacja desktop SonetaFrame** — pliki na dysku operatora; podawaj ścieżkę.
- **agent serwerowy** — **te same pliki**, którymi władasz narzędziami `file_memory_*`; nie ma
  tu dwóch osobnych magazynów. Podawaj samą nazwę pliku (bez katalogów), taką jak w
  `file_memory_ls`. Nie odczytuj pliku przez `file_memory_read`, żeby wkleić jego treść do
  parametru treści — metoda sięgnie po nią sama. Plik binarny (np. PDF) pokazuje przy odczycie
  notkę zamiast treści; jego zawartość przekażesz dalej właśnie przez `xxxFileName`.

Dla załączników parą jest `contentFileName` + `contentBase64`:

- `update_attachment` — podaj `contentFileName`, a klient odczyta plik i podstawi `contentBase64`
  przed wysłaniem; na serwer trafia sama nazwa pliku, więc `name` możesz pominąć — nazwą załącznika
  stanie się nazwa pliku. Gdy plik jest niedostępny, przekaż treść wprost w `contentBase64`.
- `get_attachment` — podaj `contentFileName`, a klient zapisze odebraną treść do tego pliku (metoda
  nie przyjmuje treści, więc plik jest wynikiem, nie źródłem). Bez `contentFileName` treść wraca
  wprost w odpowiedzi i obowiązuje limit **1024 KB** — większy załącznik da błąd z podpowiedzią,
  żeby użyć `contentFileName`.

Ta droga jest tania w kontekście: treść pliku nigdy nie przechodzi przez rozmowę, w odpowiedzi
dostajesz sam komunikat o zapisaniu. Korzystaj z niej zawsze, gdy treść ma trafić dalej, a nie być
przez Ciebie przeczytana.

Załącznik identyfikuje para (obiekt, `name`) — nazwa jest unikalna w obrębie obiektu, dlatego
powtórne `update_attachment` z tą samą nazwą podmienia treść (`created=false` w odpowiedzi).
Załącznik można dopiąć tylko do obiektu guidowanego; obiekt musi być wcześniej odczytany przez
`retrieve_list`, `search_object` lub `open_form`.

**Zapis jest natychmiastowy** — `update_attachment` sam zapisuje do bazy, nie wymaga `save_form`
ani otwartego formularza i nie da się go wycofać przez `cancel_form`.

### Jak działają okna dla operatora

Te dwa narzędzia zachowują się inaczej niż pozostałe: **odpowiedź wraca dopiero po reakcji
operatora**, a nie od razu. Wywołanie czeka, aż ktoś kliknie przycisk w oknie.

Wspólne parametry:
- `promptCaption` — tytuł pola tekstowego pod treścią okna. Gdy go podasz, operator może dopisać
  własne polecenie; gdy pominiesz, pola nie ma.
- `buttons` — dodatkowe przyciski akcji w formacie `["Etykieta|Prompt po naciśnięciu"]`. Nie
  dokładaj przycisków „Wyślij" i „Anuluj" — są zawsze.
- `selected`, `description`, `editable` — patrz opisy parametrów w `methods.list`.

W `show_content` rodzaj treści wybierasz przez `contentType`:
- `html`, `markdown` — sformatowany dokument do przeczytania (raport, podsumowanie). Markdown
  wystarczy przekazać w źródłowej postaci, renderuje go okno.
- `text`, `xml` — tekst źródłowy w edytorze wielolinijkowym; `xml` dodatkowo z podświetlaniem
  składni. To wybór domyślny, gdy `contentType` pominiesz.
- `editable=true` otwiera edytor źródła dla **każdego** rodzaju treści — także `html` i `markdown`
  (operator poprawia źródło, nie wynik renderowania), a zmieniona treść wraca w polu `text`.
- Zamiast `content` możesz podać `contentFileName` — plik odczyta klient, a rodzaj treści rozpozna
  z rozszerzenia (`.md`, `.html`, `.xml`); patrz „Jak działają załączniki".
- `saveToFile` dokłada w oknie przycisk „Zapisz jako…" — operator wskazuje miejsce zapisu treści
  na dysku, a okno zostaje otwarte (zapis nie jest odpowiedzią dla ciebie). Podaj **samą nazwę
  pliku**, bez ścieżki (np. `raport.md`); brakujące rozszerzenie uzupełnia `contentType`. Przy
  `editable=true` zapisywana jest bieżąca zawartość edytora, także zmieniona przez operatora.
- `labelHeight` (tylko `show_options_selector`) — liczba wierszy na etykietę jednej opcji, domyślnie 1.
  Gdy opcje są całymi zdaniami, podaj 2-3; przy jednym wierszu dłuższy tekst nie zmieści się
  w szerokości okna.

W odpowiedzi dostajesz **tylko te pola, które operator faktycznie wypełnił**: `text` (gdy zmienił
tekst), `selected` (zaznaczone opcje), `prompt` (co wpisał), `action` (prompt klikniętego
przycisku). Gdy operator zamknął okno bez decyzji, wraca komunikat tekstowy.

**Kiedy z tego korzystać:** gdy brakuje ci informacji, żeby działać dalej, a zgadywanie byłoby
kosztowne — wybór wariantu dokumentu, potwierdzenie zakresu operacji, akceptacja treści przed
zapisaniem. Lepiej zapytać jednym oknem niż wykonać niewłaściwą operację na danych.

## Dodatkowe dokumenty

* Użyj skill `soneta-programming` do tworzenia algorytmów C#.
* [Podstawowe foldery programu](./common-folders.md) — `programFolderPath` dla `navigate_to_folder()`
* [Zakładki formularzy](./common-form-pages.md) — `pageID` zakładek kluczowych obiektów (Pracownik, Kontrahent, Towar, Bank, US, DefinicjaElementu, Operator) dla `switch_form_page()`
* [Scenariusze działania](./common-scenarios.md) — typowe sekwencje krok-po-kroku
* [Operacje UI](./ui-equivalents.md) — odpowiedniki narzędzi MCP w interfejsie graficznym

## Identyfikatory obiektów — `id`

Każdy obiekt biznesowy w odpowiedziach ma pole **`id`** (w tabelach CSV kolumna `#id`) — krótki
identyfikator postaci `dh7`: litery to skrót typu obiektu, cyfry to numer nadany przy pierwszym
udostępnieniu. To identyfikator nieprzezroczysty: **nie jest numerem rekordu w bazie**, nie licz na
jego postać i nie buduj go samodzielnie.

- **Bierz `id` wyłącznie z odpowiedzi serwera** (`retrieve_list`, `retrieve_folder_list`,
  `search_object`, dane formularza, `lookup_list`, odniesienia `@id#opis`). Identyfikator, którego
  serwer nie wydał, zostanie odrzucony komunikatem „Nieznany identyfikator obiektu".
- **Ten sam obiekt ma zawsze ten sam `id`** w obrębie zalogowania, więc można go porównywać i
  ponownie użyć. Traci ważność po wylogowaniu operatora — wtedy wczytaj listę ponownie.
- Obiekt wskazuje się **wyłącznie** parametrem `id` (albo `ids` dla wielu obiektów) — nazwy tabeli
  nie podaje się nigdzie, gdzie jest `id`: typ obiektu wynika z samego identyfikatora. `tableName`
  potrzebują tylko punkty wejścia bez identyfikatora: `search_object` i `get_object_structure`.
  W `search_object` nazwa tabeli jest zamienna z `programFolderPath` — folder też wskazuje tabelę.
- W jednym wywołaniu `ids` muszą wskazywać obiekty **jednej tabeli** (`retrieve_object_values`,
  `export_business_xml`) — inaczej wywołanie kończy się błędem.
- W `edit_grid_rows` `id` jest kluczem operacji: `{"dh7":["Ilosc=2"]}`, `{"dh7":"delete"}`.
- Pola wskazujące obiekt (typ `row`) można ustawiać wprost identyfikatorem:
  `update_field_value(fieldsValues=["Kontrahent=ko42"])` — bez zgadywania, po którym polu szukać.
- Wiersze, które nie są obiektami bazy (grupy `aggregate_folder_values`), mają `id` puste — nie ma
  czego otwierać.

## Tabele w odpowiedziach — pola z sufiksem `Csv`

Każde pole odpowiedzi, którego nazwa kończy się na **`Csv`**, jest tabelą zapisaną w jednym
stringu, a nie tablicą obiektów. Reguła jest jedna dla wszystkich metod:

- **pierwsza linia to nagłówek**, kolejne to wiersze; wiersze rozdziela znak nowej linii,
- **komórki rozdziela `|`**,
- nagłówki zaczynające się od `#` to kolumny techniczne wiersza, nie dane — `#id` to
  identyfikator obiektu (ten sam, który podajesz do `open_form` czy `edit_grid_rows`),
  `#nodeState` to stan węzła drzewa (`expanded`/`collapsed`). Kolumny puste we wszystkich
  wierszach są pomijane, więc `#nodeState` pojawia się tylko dla list drzewiastych,
- **pusta komórka oznacza brak wartości**, a `1` w kolumnie logicznej oznacza prawdę,
- kolumny, które byłyby puste w każdym wierszu, znikają w całości.

Znaki specjalne w wartości są poprzedzone ukośnikiem wstecznym: `\|` to zwykła kreska w treści,
`\n` i `\r` to znaki nowej linii wewnątrz komórki (pola opisowe), `\\` to sam ukośnik.
Wewnątrz komórki-listy elementy rozdziela `;` (np. dopuszczalne wartości pola wyliczeniowego).

Pierwszy znak komórki może wskazywać typ wartości:

| Zapis | Znaczenie |
|---|---|
| `@ko42#Kowalski` | odniesienie do obiektu: `id` = `ko42`, opis = `Kowalski` |
| `!Brak uprawnień` | wartości nie udało się odczytać — to komunikat błędu, nie dane |
| `\@`, `\!` | zwykły tekst zaczynający się od `@` albo `!` |

Przykład odpowiedzi `retrieve_list`:

```json
{
  "columnsCsv": "fieldPath|editable\nList:Kod|\nList:Nazwa|\nList:CenaNetto|1",
  "rowType": "Towar",
  "rowsCsv": "#id|Kod|Nazwa|Cena netto\nto1|T-0001|Bikini damskie|129.99\nto2|T-0002|Montaż, transport|50.00",
  "truncated": true
}
```

`columnsCsv` opisuje kolumny w tej samej kolejności co nagłówek `rowsCsv`, po jednej linii na
kolumnę — stamtąd bierzesz `fieldPath` do `edit_grid_rows` i `update_field_value`. Gdy kolumny nie
niosą nic ponad podpis, pole nie jest wysyłane.

Reguła jest ta sama niezależnie od metody. Pola tego rodzaju to m.in. `rowsCsv` i `columnsCsv`
(`retrieve_list`, gridy formularzy, `retrieve_bi_view`, podlisty w `retrieve_object_values`),
`foldersCsv`, `pagesCsv`, `fieldsCsv`, `itemsCsv`, `actionsCsv`, `optionsCsv`.

Dwa warianty, poznasz je po liczbie linii:

- **tabela wielu obiektów** — nagłówek plus jedna linia na obiekt (`rowsCsv`, `itemsCsv`,
  `foldersCsv`, `actionsCsv`, `pagesCsv`, `optionsCsv`),
- **pola jednego obiektu** — dokładnie dwie linie: nazwy pól i ich wartości. Tak wygląda
  `fieldsCsv` w `retrieve_object_values`. Każdy obiekt ma własny nagłówek, bo zestawy pól bywają
  różne — pole bez wartości po prostu nie występuje w jego nagłówku, a jego ścieżka trafia do
  jednej z list opisanych niżej.

```json
{
  "id": "to2",
  "caption": "BIKINI - Bikini - Strój kąpielowy damski",
  "fieldsCsv": "Kod|Nazwa|Jednostka|SWW\nBIKINI|Bikini - Strój kąpielowy damski|@je1#szt|"
}
```

## Formaty danych

Wszystkie wartości przesyłane z/do programu muszą używać formatu InvariantCulture:

| Typ      | Format                        | Przykład                                              | filterPattern                                                  |
|----------|-------------------------------|-------------------------------------------------------|----------------------------------------------------------------|
| string   | tekstowy                      | `Kowalski`                                            | `A..K`, `K..`, `..L`, `Kowalski`, `Kow*`, `*als*` (* wildcard) |
| date     | `yyyy-MM-dd`                  | `2026-03-25`                                          | `2026-03-25..2026-03-31`, `2026-03-25..`, `..2026-03-25`, `..` |
| fromto   | `yyyy-MM-dd..yyyy-MM-dd`      | `2026-01-01..2026-03-31`                              | `2026-03-25..2026-03-31`, `2026-03-25..`, `..2026-03-25`, `..` |
| bool     | `Tak` / `Nie`                 | `Tak`                                                 | `Tak`, `Nie`                                                   |
| int      | liczbowy                      | `1234`                                                | `100`, `100..`, `..100`, `100..200`                            |
| number   | kropka dziesiętna             | `1234.56`                                             | `100`, `100..`, `..100`, `100..200`                            |
| currency | kropka dziesiętna z walutą    | `1234.56 PLN`                                         | `100`, `100..`, `..100`, `100..200`                            |
| amount   | kropka dziesiętna z jednostką | `1234.56 szt`                                         | `100`, `100..`, `..100`, `100..200`                            |
| percent  | procent%                      | `50%`                                                 | `10%`, `10%..`, `..10%`, `10%..20%`                            |
| fraction | licznik/mianownik             | `1/3`                                                 | `1/2..1`, `1/4..`                                              |
| row      | objectSelector                | `789`                                                 | objectSelector                                                 |
| code     | snippet C# code               | `Time NormaOkres = Czasy.Norma(Składnik.Okres).Czas;` |                                                                |
| filter   | `["filterPath=Wartość"]`        | `["_Status=Aktywny", "_DataListy=2026-01-01.."]`      |                                                                |

## Format objectSelector
W metodach `retrive_list(filtersValues=???)`, `select_rows`, `open_form`, `search_object`, `update_field_value` i innych, można używać sekwencji `objectSelector`,
którą wykorzystujemy do wyszukiwania konkretnego obiektu (rekordu).

| Wzorzec              | Przykład                               | Znaczenie                                                     |
|----------------------|----------------------------------------|---------------------------------------------------------------|
| ID wiersza           | `123`                                  | pobiera obiekt o wskazanym ID w bazie                         |
| guid                 | `71afb5cb-569b-4d06-a27a-d104a533dab2` | pobiera obiekt o wzkazanym GUID                               |
| selectorField=filter | `Nazwa=BIKI*` lub Nazwa=*jew*`         | wyszukuje obiekt, którego pole ma wartość lub spełnia pattern |

> **Wydajność wzorca selectorField — unikaj gwiazdki na początku.** Pattern z gwiazdką na początku (`*jew*`, `*bikini*`) wymusza pełne skanowanie tabeli (nie korzysta z indeksu) i jest znacznie mniej wydajny. Preferuj wzorzec bez gwiazdki na początku — prefiksowy (`BIKI*`, `Nazwa=Zefir`), który może użyć indeksu. Gwiazdki wiodącej używaj tylko gdy nie znasz początku wartości lub prefiks jest niejednoznaczny.
> - ✅ lepiej: `Nazwa=BIKI*` (prefiks, korzysta z indeksu)
> - ⚠️ gorzej: `Nazwa=*bikini*` (skan całej tabeli — stosuj tylko gdy konieczne)

> **`objectSelector` szuka po polach indeksowanych.** Gdy pole nie ma w tabeli indeksu (np.
> `Numer.Pelny` dokumentu handlowego), selektor kończy się błędem „Nie znaleziono pola" — wtedy
> wskaż obiekt filtrami listy: `search_object(programFolderPath, filters=["Numer.Pelny==FZ/1/2026"])`.
> W `search_object` z `programFolderPath` selektor jest opcjonalny.

> **Pola typu `row` (ref) w `update_field_value` / `edit_grid_rows` — od razu podawaj objectSelector w formie `selectorField=filter`, nie samą wartość.**
> Wartość po `fieldPath=` jest interpretowana jako objectSelector tabeli docelowej. Sama nazwa (np. `Kontrahent=Zefir`) jest traktowana jak `id`/ID wiersza/kod i zwykle kończy się błędem „Nieznany selektor 'Zefir'", bo serwer nie wie, po którym polu szukać. Wskaż pole z `selector.fields` tabeli docelowej:
> - ❌ źle: `update_field_value(fieldsValues=["Kontrahent=Zefir"])`
> - ✅ dobrze: `update_field_value(fieldsValues=["Kontrahent=Nazwa=Zefir"])` (lub `Kontrahent=Kod=Zefir`, `Kontrahent=NIP=...`)
>
> Dostępne pola selektora odczytasz z `lookup_list(fieldPath)` (zwraca `selector.fields`, np. `Kod`, `Nazwa`, `NIP`). Przy niejednoznacznej wartości użyj wzorca `*` (`Kontrahent=Nazwa=*Zefir*`).

## Zasady bezpieczeństwa

- **Pracuj tylko na identyfikatorach zwróconych przez serwer** — nigdy nie zgaduj `fieldPath`, `filterPath`, `pageID`, `gridPath`, `commandPath` ani `workerID`. Odczytuj je z odpowiedzi narzędzi (`retrieve_list`, `open_form`, `get_form_pages`, `get_actions`, …). `fieldPath`/`filterPath`/`gridPath`/`commandPath` to ścieżki do elementów formularza (np. `Grupa.Kod`, a na liście osadzonej z separatorem `:`, np. `Pozycje:Ilosc`) i są ważne tylko w obrębie aktualnej sesji/zakładki.
- **Edytuj tylko pola oznaczone `edytowany`** na aktualnej zakładce — żeby zmienić pole na innej zakładce użyj `switch_form_page`. Pola tylko do odczytu pomiń i poinformuj użytkownika.
- **Domykaj formularze świadomie** — po edycji wywołaj `accept_subform` na każdym otwartym podformularzu, potem `save_form`. Przy samym podglądzie używaj `cancel_form` zamiast `save_form`, żeby uniknąć przypadkowego zapisu.
- **Duże listy stronicuj** przez `pageNumber` (0, 1, 2, …) zamiast próbować pobrać wszystko naraz.
- **Czynności uruchamiaj świadomie** — `execute_action` może wykonać operacje nieodwracalne (księgowanie, wysyłka KSeF, zatwierdzenie deklaracji). Najpierw sprawdź dostępne czynności przez `get_actions`, a parametry zatwierdzaj (`accept_form`) dopiero po weryfikacji.

## Obsługa błędów

| Problem | Rozwiązanie |
|---|---|
| Odpowiedź nie zawiera pól, kolumn ani wierszy | To render skrócony (`detail:"header"` — domyślny i zamierzony). Dobierz szczegóły przez `where_am_I(detail:"full")`; **nie powtarzaj** metody, która coś zmieniła |
| `retrieve_list` nie zwraca wierszy (`data` brak) | Wywołaj `retrieve_list(detail:"full")` — odczyt jest bezpieczny do powtórzenia |
| Odpowiedź to `{"kind":"message","message":"OK"}` | Wywołanie poszło z `detail:"none"`. Stan okna sprawdzisz przez `where_am_I` |
| `Nieprawidłowa wartość … Dozwolone wartości: Full, Header, None` | `detail` przyjmuje wyłącznie `full`, `header` albo `none` |
| `navigate_to_folder` zwraca błąd | Sprawdź ścieżkę przez `get_folders` — ścieżka może być nieprawidłowa |
| `navigate_to_folder` nie działa | Operator jest na formularzu lub w menu — `navigate_to_folder` działa tylko z widoku listy. Użyj `where_am_I` aby sprawdzić kontekst |
| `retrieve_list` nie działa po `navigate_to_folder` | Folder ma widok formularza lub menu, nie listy — `retrieve_list` działa tylko dla folderów z widokiem listy |
| Lista jest pusta | Zasugeruj zmianę lub usunięcie filtrów |
| Pole nie jest `edytowany` | Poinformuj użytkownika — pole jest tylko do odczytu |
| Brak `id` obiektu | Najpierw wykonaj `retrieve_list`, `retrieve_folder_list` albo `search_object` — identyfikatory pochodzą tylko z odpowiedzi serwera |
| `Nieznany identyfikator obiektu` | `id` nie pochodzi z odpowiedzi serwera albo wygasł wraz z wylogowaniem — wczytaj listę zawierającą ten obiekt ponownie i użyj `id` z nowej odpowiedzi |
| Brak `pageID` | Wywołaj `get_form_pages` na otwartym formularzu aby uzyskać listę zakładek wraz z ich identyfikatorami |
| Nieznany folder | Użyj `get_modules` → `get_folders` do eksploracji struktury |
| Nieznany folder konfiguracyjny | Użyj `get_configuration_folders` do eksploracji ustawień programu |
| Brak `gridPath` | Najpierw wykonaj `open_form` lub `navigate_to_folder` (dla formularzy) — `gridPath` jest w opisie listy osadzonej |
| `accept_subform` zwraca błąd | Podformularz nie jest otwarty — sprawdź czy wcześniej wywołano `open_subform` lub `add_subobject` |
| `save_form` nie zapisuje zmian | Upewnij się, że wszystkie podformularze zostały zamknięte przez `accept_subform` przed zapisem |
| Nie wiadomo gdzie jest operator | Użyj `where_am_I` żeby sprawdzić aktualny kontekst (lista, formularz, menu) |
| `search_object`: „Obiekt nieznaleziony" | W tabeli nie ma takiego wiersza. Sprawdź `tableName` i `objectSelector` — użyj `selectorField` z tabeli folderów albo `selector` z `get_folder_structure` |
| `search_object`: „Wyszukiwanie nieudane — spróbuj zmienić zakres wyszukiwania folderu" | Obiekt **istnieje**, ale wypada z zakresu listy: zawęża go filtr widoku (okres, magazyn, zakres, kategoria) albo prawa do danych. Zajrzyj w `get_folder_structure(programFolderPath)` → `filtersCsv` i `condition`, potem powtórz z `programFolderPath` i jawnymi `filters` |
| `search_object`: „Nie jest możliwe wyszukiwanie wg tabeli" | Tabela nie ma wyszukiwania globalnego. Wskaż listę: `search_object(programFolderPath, filters)` — ścieżki z `get_folders` |
| `search_object`: niezgodność `tableName` z `programFolderPath` | Podaj tylko jeden z parametrów — folder sam wyznacza tabelę swojej listy |
| `search_object`: „`filters` działa tylko razem z `programFolderPath`" | Filtry są filtrami listy. W trybie `tableName` zawężaj przez `objectSelector` |
| `search_object` zwraca wiele obiektów | To normalne — wynik jest listą. Wybierz właściwy po `caption`/`fields` i otwórz go przez `open_form`, albo zawęź `objectSelector` |
| Formularz nie otworzył się po `search_object` | `search_object` zwraca tylko dane. Formularz otwiera `open_form(id)` z identyfikatorem z jego odpowiedzi |
| `lookup_list` zwraca pustą listę | Pole nie ma ograniczonego zbioru wartości — jest to pole tekstowe/liczbowe do wpisania dowolnej wartości |
| `lookup_list` zwraca błąd | Sprawdź czy formularz jest otwarty i czy `fieldPath` pochodzi z odpowiedzi `open_form`/`switch_form_page`/`add_object`. Edycję danych na listach osadzonych (gridach) wykonuj metodą `edit_grid_rows` |
| `edit_grid_rows` zwraca „Nie znaleziono kolumny 'X'" | Użyto nagłówka kolumny (`caption`) zamiast nazwy pola. Adresuj pole segmentem `fieldPath` PO dwukropku (np. `Workers.Cena.Netto` z `fieldPath` `new TowarExtender.Ceny:Workers.Cena.Netto`), nie `caption` (`Netto`) |
| `select_database` nie loguje do bazy | Może być wymagana ingerencja operatora (logowanie). Sprawdź nazwę bazy przez `get_databases` |
| `get_actions` zwraca pustą listę | Brak akcji dla podanych obiektów lub formularza — operacje mogą być niedostępne w aktualnym kontekście |
| `execute_action` zwraca błąd | Sprawdź `workerID` (musi pochodzić z `get_actions`) oraz `ids` (`null` dla formularza, tablica `id` dla listy) |
| `accept_form` zwraca błąd | Formularz parametrów akcji nie jest otwarty — `accept_form` działa tylko po `execute_action` które otworzyło formularz parametrów. Dla zatwierdzania podformularzy użyj `accept_subform`, dla zapisu obiektu `save_form` |
| `invoke_command` zwraca błąd „znajduje się na innej zakładce" | `commandPath` dotyczy innej zakładki — przełącz się przez `switch_form_page` albo pobierz aktualne `commandPath` z renderu bieżącej zakładki |
| `invoke_command` zwraca „przycisk tylko do odczytu" | Komenda jest nieaktywna w aktualnym kontekście — pomiń ją i poinformuj operatora |
| `update_attachment`/`get_attachment` zwraca „serwer plików nie czyta" | Podano `contentFileName`, a uruchomiony klient plików nie obsługuje. Przy zapisie przekaż treść w `contentBase64`; przy odczycie pomiń `contentFileName` i odbierz `contentBase64` |
| `get_attachment` zwraca błąd o przekroczonym limicie | Załącznik przekracza 1024 KB — pobierz go z parametrem `contentFileName`, wtedy treść trafi do pliku zamiast do rozmowy |
| `get_attachment` nie znajduje załącznika | Nazwa musi pochodzić z `list_attachments` — nie zgaduj nazw plików |
| `update_attachment` zwraca „nie obsługuje załączników" | Tabela nie jest guidowana — załączniki można dopinać tylko do obiektów guidowanych |
| `update_attachment` zwraca „współdzieli plik z innymi załącznikami" | Załącznik jest odnośnikiem do pliku używanego też przez inne obiekty — nadpisanie zmieniłoby je wszystkie. Dodaj nowy załącznik pod inną nazwą (`overwrite=false`) |
| `update_attachment` zwraca „Rozszerzenie pliku … jest niedozwolone" | Konfiguracja załączników blokuje to rozszerzenie — poinformuj operatora |

## Parametr navigate_to_folder — newTab

- `newTab: true` — otwiera folder w nowej karcie (nie zamyka aktualnego widoku)
- `newTab: false` (domyślnie) — zastępuje aktualny widok
- Używaj `newTab: true` gdy użytkownik chce porównać dane z dwóch folderów
