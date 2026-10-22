
# Typowe scenariusze krok po kroku

> **`detail` tylko tam, gdzie naprawdę czytasz.** Wszystkie kroki idą na domyślnym `header`
> (sam nagłówek okna); `detail="full"` pojawia się w jednym kroku sekwencji — tym, który dostarcza
> dane albo ścieżki `fieldPath`/`gridPath`. Gdy szczegółów zabraknie później, dobierz je przez
> `where_am_I(detail="full")`, a nie przez powtórzenie kroku, który coś zmienił.

## Odczyt danych z listy

```
1. navigate_to_folder(programFolderPath)  — otwórz folder (nagłówek wystarczy)
2. retrieve_list(filtersValues?, pageNumber?, detail="full")  — dopiero tu pobierasz dane
3. Prezentuj wyniki użytkownikowi
```

## Podgląd szczegółów obiektu

```
1. navigate_to_folder(programFolderPath)  — otwórz folder
2. retrieve_list(detail="full")  — pobierz listę, znajdź `id` obiektu
3. open_form(id)  — otwórz formularz; nagłówek potwierdza, co się otworzyło
4. switch_form_page(pageID, detail="full")  — przejdź na zakładkę, którą faktycznie czytasz
5. cancel_form(detail="none")  — zamknij formularz bez zapisywania (bezpieczne zamknięcie po podglądzie)
```

Gdy interesuje cię pierwsza zakładka (bez przełączania), pola dobierz przez
`where_am_I(detail="full")` zamiast otwierać formularz od razu z `full`.

## Edycja istniejącego obiektu

```
1. navigate_to_folder(programFolderPath)  — otwórz folder
2. retrieve_list(detail="full")  — znajdź obiekt
3. open_form(id)  — otwórz formularz
4. switch_form_page(pageID, detail="full")  — zakładka z polem do zmiany; stąd bierzesz `fieldPath`
5. update_field_value(fieldsValues=["fieldPath=value"]) — zmień wartość pola oznaczonego 'edytowany'
   (weryfikatory wracają niezależnie od `detail`)
```

## Dodawanie nowego obiektu

```
1. navigate_to_folder(programFolderPath)  — otwórz odpowiedni folder
2. add_object()  — utwórz nowy obiekt
3. where_am_I(detail="full")  — odczytaj pola formularza (add_object jest operacją zmieniającą,
   więc nie powtarzaj go z `full`)
4. update_field_value(fieldsValues=["fieldPath=value"]) — wypełnij wymagane pola
```

## Przeglądanie dużej listy (stronicowanie)

```
1. navigate_to_folder(programFolderPath)
2. retrieve_list(pageNumber=0, detail="full")  — pierwsza strona
3. retrieve_list(pageNumber=1, detail="full")  — kolejna strona
4. ... kontynuuj aż dane się wyczerpią
```

## Nawigacja eksploracyjna (nieznany folder)

```
1. get_modules()  — lista modułów
2. get_folders(moduł)  — foldery z JEDNEGO poziomu w module
3. get_folders(moduł/podfolder)  — wejdź głębiej w podmenu (gdy type=folders)
4. navigate_to_folder(znaleziony_folder) — otwórz właściwy folder
```

Bez `regexFilter` metody `get_folders` / `get_configuration_folders` zwracają foldery
tylko z **jednego poziomu** (podmenu mają `type=folders` — można w nie wejść kolejnym
wywołaniem). Aby od razu przeszukać strukturę **rekurencyjnie w głąb**, podaj `regexFilter`
(dopasowanie do ścieżki folderu). Dopasowanie jest nieczułe na wielkość liter.
`RegexOptions`, np. `IgnoreCase`).

```
get_folders("Handel", regexFilter="faktur")  — rekurencyjnie wszystkie foldery modułu Handel zawierające "faktur" (dowolna wielkość liter)
```

### Limit wyników

Metody listujące (`get_folders`, `get_configuration_folders`, `get_form_pages`, `get_bi_data_models`,
`get_bi_areas`) domyślnie zwracają maksymalnie **15** elementów. W rezultacie jest `count` (całkowita
liczba pasujących elementów) oraz `truncated` (czy wynik ucięto). Gdy `truncated=true`, zawęź wynik
przez `regexFilter`/`area` albo zwiększ `limit`. `limit=0` zwraca wszystkie elementy.

## Przegląd modeli danych BI (analityka)

```
1. get_bi_areas(regexFilter?)  — (opcjonalnie) poznaj dostępne obszary merytoryczne
2. get_bi_data_models(area?, regexFilter?)  — znajdź modele danych (widoki BI); zwraca też parametry wywołania
3. get_bi_data_model_info(modelID)  — (opcjonalnie) pełny technicalInfo: wymiary, miary, sortowanie
4. retrieve_bi_view(modelID, parameters?, limit?) — wykonaj model i pobierz dane (kolumny + wiersze)
```

Użyteczne, gdy użytkownik pyta „jakie analizy/raporty BI są dostępne", „jakie dane mogę
zobaczyć w obszarze X", lub chce odczytać wynik konkretnej analizy BI.
Najpierw `get_bi_areas` zwraca strukturę obszarów (`areaPath`); wartość `areaPath` przekazujesz jako
parametr `area` do `get_bi_data_models`, aby zawęzić modele do danego obszaru (pominięty — bez
filtrowania po obszarze). `regexFilter` filtruje po nazwie modelu / ścieżce obszaru (opcje przez
`get_bi_data_models` zwraca lekką listę z `requiredParams`/`optionalParams`
(parametry wywołania). Po pełną informację o modelu (wymiary, miary, sortowanie) sięgnij po
`get_bi_data_model_info(modelID)`. Mając `modelID` (i ewentualne parametry), `retrieve_bi_view`
zwraca dane wynikowe modelu.

```
get_bi_areas(regexFilter="sprzedaż")  — obszary merytoryczne zawierające "sprzedaż" w nazwie/ścieżce
get_bi_data_models(area="Sprzedaż", regexFilter="marża")  — modele danych z obszaru "Sprzedaż" zawierające "marża" w nazwie
retrieve_bi_view(modelID=123, parameters=["Okres=2024-01-01..2024-12-31"], limit=50)  — wykonuje model danych 123 z parametrem zakresu dat, zwraca maks. 50 wierszy
```

Parametry `retrieve_bi_view` podaje się jak filtry w `retrieve_list`: `["nazwa=wartość", ...]`,
gdzie nazwy odpowiadają parametrom z `technicalInfo` (requiredParams/optionalParams). Wiele wartości
rozdziela się przecinkiem, referencje (np. kontrahent) podaje się jako `id`, Guid lub ID wiersza.

## Edycja obiektu na liście osadzonej w formularzu (podformularz)

```
1. navigate_to_folder(programFolderPath)  — otwórz folder (lub formularz konfiguracyjny)
2. open_form(id, detail="full")  — formularz z listami osadzonymi; stąd bierzesz `gridPath`
3. open_subform(gridPath, id, detail="full")  — podformularz razem z polami do edycji
4. switch_form_page(pageID, detail="full")  — przejdź do zakładki z polem (jeśli trzeba)
5. update_field_value(fieldsValues=["fieldPath=value"])  — zmień wartość pola
6. accept_subform()  — zatwierdź podformularz i wróć do formularza nadrzędnego
7. save_form()  — zapisz zmiany do bazy danych
```

## Dodawanie obiektu na liście osadzonej w formularzu

```
1. navigate_to_folder(programFolderPath)  — otwórz folder (lub formularz konfiguracyjny)
2. open_form(id, detail="full")  — formularz z listami osadzonymi; stąd bierzesz `gridPath`
3. add_subobject(gridPath, detail="full")  — dodaj nowy obiekt i od razu odczytaj pola podformularza
4. update_field_value(fieldsValues=["fieldPath=value"])  — wypełnij pola na podformularzu
5. switch_form_page(pageID, detail="full")  — przejdź do kolejnych zakładek i wypełnij pola
6. accept_subform()  — zatwierdź podformularz
7. save_form()  — zapisz zmiany do bazy danych
```

## Szybki dostęp do obiektu (search_object)

```
1. search_object(tableName, objectSelector)  — wyszukaj obiekty; zwraca dane, NIE otwiera formularza
   albo: search_object(programFolderPath, filters=["pole==wartość"])  — gdy zakres ma odpowiadać
   konkretnej liście, obiekt rozpoznają wartości pól, albo tabela nie ma wyszukiwania globalnego
2. open_form(id, detail="full")  — otwórz formularz wybranego obiektu (bez wchodzenia na listę)
3. switch_form_page(pageID, detail="full")  — przełącz na zakładkę, którą faktycznie czytasz
4. update_field_value(fieldsValues=["fieldPath=value"])  — zmień wartość pola (opcjonalnie)
5. save_form() / cancel_form()  — zapisz lub zamknij bez zapisywania
```

Gdy `search_object` zwróci wiele pozycji, wybierz właściwą po `caption` i `fields`, a dopiero
jej `id` przekaż do `open_form`.

Komunikat „Wyszukiwanie nieudane — spróbuj zmienić zakres wyszukiwania folderu" znaczy, że obiekt
istnieje, ale nie mieści się w zakresie listy: sprawdź `get_folder_structure(programFolderPath)`
(`filtersCsv`, `condition`) i powtórz wyszukanie z `programFolderPath` oraz jawnymi `filters`.

## Edycja konfiguracji programu

```
1. get_configuration_folders()  — lista folderów konfiguracyjnych
2. navigate_to_folder(programFolderPath, detail="full")  — otwórz formularz konfiguracyjny
3. open_subform(gridPath, id, detail="full")  — edytuj obiekty na listach osadzonych (jeśli trzeba)
4. update_field_value(fieldsValues=["fieldPath=value"])  — zmień wartości pól
5. accept_subform()  — zatwierdź podformularz
6. save_form()  — zapisz konfigurację do bazy danych
```

## Wybór bazy danych

```
1. get_databases()  — lista dostępnych baz danych
2. select_database(databaseName)  — wybierz bazę do pracy (może wymagać logowania)
3. get_modules() / navigate_to_folder(...)  — kontynuuj pracę po zalogowaniu
```

## Uruchomienie akcji (czynności) na obiektach z listy

```
1. navigate_to_folder(programFolderPath)  — otwórz folder z listą
2. retrieve_list(detail="full")  — znajdź `id` obiektów
3. get_actions(ids=[…])  — pobierz dostępne czynności
4. execute_action(workerID, ids=[…]) — uruchom akcję; nagłówek powie, czy otworzył się formularz parametrów
5. where_am_I(detail="full") + update_field_value(fieldsValues=["fieldPath=value"])  — odczytaj i wypełnij
   parametry akcji (czynności nie uruchamiaj drugi raz, żeby dostać `full`)
6. accept_form() / cancel_form()  — zatwierdź uruchomienie akcji lub anuluj
```

## Uruchomienie akcji na otwartym formularzu

```
1. open_form(id) — otwórz formularz obiektu (`id` z retrieve_list albo search_object)
2. get_actions(ids=null)  — pobierz czynności dla formularza
3. execute_action(workerID, ids=null)  — uruchom akcję
4. where_am_I(detail="full") + update_field_value(fieldsValues=["fieldPath=value"])  — odczytaj
   formularz parametrów i wypełnij pola (jeśli się otworzył)
5. accept_form() / cancel_form()  — zatwierdź lub anuluj
```

## Przegląd i pobranie załączników obiektu

```
1. retrieve_list() / search_object(...)  — ustal `id` obiektu
2. list_attachments(id)  — nazwy załączników (bez treści)
3. get_attachment(id, name)  — treść w contentBase64 (limit 1024 KB)
   albo, gdy treść ma trafić dalej, a nie do rozmowy:
   get_attachment(id, name, contentFileName) — klient zapisze treść do pliku, bez limitu
```

## Dopięcie pliku do obiektu

```
1. retrieve_list() / search_object(...)  — ustal `id` obiektu
2. update_attachment(id, name, contentBase64) — dodaj załącznik
   albo z pliku:
   update_attachment(id, contentFileName=...) — plik odczyta klient, nazwa = nazwa pliku
3. list_attachments(id)  — weryfikacja
```

> Powtórne `update_attachment` z tą samą nazwą **podmienia treść** (`created=false`).
> Żeby zachować poprzedni plik, użyj `overwrite=false` — powstanie załącznik o unikalnej nazwie.
> Zapis jest natychmiastowy: nie wymaga `save_form` i nie cofa go `cancel_form`.
