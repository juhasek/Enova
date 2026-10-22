
# Odpowiedniki operacji na programie dla metody MCP

| Metoda MCP | Operacja w oknie aplikacji                                                                                                                        |
|---|---------------------------------------------------------------------------------------------------------------------------------------------------|
| `get_modules` | Kliknięcie ikony **domku** (Home) na górze okna — wyświetla listę głównych modułów systemu (Handel, Kadry, Księgowość…)                           |
| `get_folders` | Przeglądanie **drzewa folderów** po lewej stronie okna — rozwija podfoldery wskazanego modułu lub folderu nadrzędnego                             |
| `get_configuration_folders` | Kliknięcie ikony  **trybiku** w nagłówku, następnie wybranie funkcji "Opcje" z menu otwiera folder menu konfiguracji programu                     |
| `navigate_to_folder` | Kliknięcie **folderu w drzewie** po lewej stronie — otwiera wybrany widok w bieżącej lub nowej karcie                                             |
| `retrieve_list` | Przeglądanie **aktualnej listy danych** — odczytuje wiersze widoczne na liście, opcjonalnie z filtrami i stronicowaniem                           |
| `get_folder_structure` | Spojrzenie na **panel filtrów i nagłówki kolumn** listy folderu — mówi, czym da się filtrować i co lista pokazuje, bez wchodzenia w folder |
| `retrieve_folder_list` | To samo co otwarcie folderu i przeczytanie listy (`navigate_to_folder` + `retrieve_list`), ale **bez zmiany widoku operatora** — dodatkowo pozwala filtrować i wyświetlać dowolne pola obiektu, nie tylko kolumny widoku |
| `aggregate_folder_values` | **Grupowanie listy i podsumowania pod kolumnami** (przeciągnięcie kolumny na pasek grupowania + suma/liczba/średnia w stopce) — tylko bez okna i z sięganiem do pozycji wiersza |
| `retrieve_list_next_page` | **Przewinięcie listy w dół** — kolejna porcja wierszy z tego samego odczytu                                                        |
| `close_list` | Zamknięcie karty z listą czytaną w tle — zwalnia jej zasoby, gdy przerywasz czytanie                                                                |
| `select_rows` | Kliknięcie **wiersza na liście** (z Ctrl/Shift — wielu wierszy) — zaznacza obiekty bez otwierania formularza                                    |
| `open_form` | Kliknięcie przycisku **Otwórz** nad listą — otwiera pełny formularz szczegółów dla zaznaczonego obiektu                                           |
| `open_subform` | Dwuklik na **wierszu listy osadzonej** w formularzu — otwiera podformularz szczegółów obiektu znajdującego się na liście wewnątrz formularza      |
| `add_subobject` | Kliknięcie przycisku **Dodaj** na **liście osadzonej** w formularzu — tworzy nowy obiekt na liście wewnątrz formularza i otwiera jego podformularz |
| `edit_grid_rows` | **Edycja danych wprost na liście** (grid w trybie edycji w miejscu) — hurtowo dodaje, modyfikuje i usuwa wiersze listy osadzonej bez otwierania podformularza |
| `accept_subform` | Kliknięcie przycisku **OK/Zatwierdź** na podformularzu — zatwierdza dane i wraca do formularza nadrzędnego                                        |
| `save_form` | Kliknięcie przycisku **Zapisz** na formularzu — zatwierdza dane i zapisuje je do bazy danych, zamyka formularz                                    |
| `cancel_form` | Kliknięcie przycisku **X** na formularzu — wycofuje wszystkie zmiany (również na podformularzach) i zamyka formularz bez zapisywania              |
| `switch_form_page` | Kliknięcie **zakładki** w panelu po lewej stronie formularza — przełącza na inną kartę danych (np. Etat, Adresy, Rachunki)                        |
| `update_field_value` | **Wpisanie wartości** w polu formularza — modyfikuje zawartość pól oznaczonych jako edytowalne na aktualnie otwartej zakładce                     |
| `add_object` | Kliknięcie przycisku **Dodaj** nad listą — otwiera pusty formularz nowego obiektu w bieżącym folderze                                             |
| `search_object` | Wpisanie **nazwy/kodu obiektu** w pole wyszukiwania globalnego (tryb `tableName`) albo ustawienie **filtrów na liście** i odszukanie wiersza lokatorem (tryb `programFolderPath` + `filters`) — zwraca dane znalezionych obiektów, formularza nie otwiera |
| `where_am_I` | Sprawdzenie **aktualnego widoku** — zwraca informację o tym, czy operator jest na liście, w menu folderów, czy na formularzu                       |
| `lookup_list` | Kliknięcie **strzałki rozwijającej** przy polu wyboru — wyświetla listę dozwolonych wartości do wpisania w pole formularza lub filtr              |
| `get_databases` | Otwarcie okna **wyboru bazy danych** w ekranie logowania — wyświetla listę baz dostępnych w systemie                                              |
| `select_database` | Wybór **bazy danych** na ekranie logowania i kliknięcie "Otwórz" — przełącza pracę na wskazaną bazę (może wymagać podania hasła)                  |
| `get_actions` | Rozwinięcie menu **Czynności** nad listą lub na formularzu — pokazuje akcje dostępne dla zaznaczonych obiektów lub aktualnego formularza         |
| `execute_action` | Kliknięcie konkretnej **czynności** z menu Czynności — uruchamia akcję; jeśli wymaga parametrów, otwiera formularz parametrów                     |
| `accept_form` | Kliknięcie **OK/Wykonaj** na formularzu parametrów akcji — zatwierdza parametry i uruchamia czynność                                              |
| `list_attachments` | Otwarcie zakładki **Załączniki** na formularzu obiektu — pokazuje listę dopiętych plików wraz z nazwą, opisem i datą modyfikacji                  |
| `get_attachment` | Kliknięcie przycisku **Zapisz jako** (lub dwuklik) na wierszu listy załączników — pobiera zawartość pliku z bazy                                   |
| `update_attachment` | Kliknięcie przycisku **Dodaj** na zakładce Załączniki i wskazanie pliku — dopina plik do obiektu; wskazanie istniejącej nazwy podmienia jego treść |
