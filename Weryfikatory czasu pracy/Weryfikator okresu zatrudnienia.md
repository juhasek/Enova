# Weryfikator okresu zatrudnienia

Dokumentacja biznesowa dla użytkownika.

## 1. Do czego służy weryfikator

Weryfikator pilnuje, aby dni planu pracy nie były zakładane ani modyfikowane poza okresem
zatrudnienia pracownika. Jeśli użytkownik spróbuje zmienić dzień planu wykraczający poza aktywny
etat (przed datą przyjęcia do pracy, po dacie zwolnienia, albo w okresie, gdy pracownik nie ma
żadnego aktywnego etatu), zmiana jest blokowana, a użytkownik dostaje czytelny komunikat
z przyczyną odmowy.

## 2. Kiedy weryfikator się uruchamia

Weryfikator wywoływany jest automatycznie przez system przy każdej próbie zapisania/zmodyfikowania
pojedynczego dnia planu pracy (`Soneta.Kalend.DzienPlanu`) — niezależnie od tego, czy zmiana
wprowadzana jest ręcznie w harmonogramie, czy przez import/generowanie planu. Nie wymaga żadnej
akcji ze strony użytkownika poza samą edycją planu.

Jeśli dzień planu nie ma przypisanego pracownika (np. edycja wzorca/szablonu planu bez konkretnej
osoby), weryfikator nic nie sprawdza i przepuszcza zmianę bez błędu.

## 3. Konfiguracja

Weryfikator jest skryptem typu „Weryfikator dnia planu pracy” i podpina się pod standardowy
mechanizm weryfikatorów kalendarza w Kadrach i Płacach. Aby był aktywny:

- Skrypt musi być zarejestrowany i włączony w konfiguracji weryfikatorów kalendarza/planu pracy
  (administrator systemu dodaje go do listy weryfikatorów obowiązujących globalnie lub dla
  wybranego wzorca/definicji planu pracy — dokładna lokalizacja w menu zależy od wersji i konfiguracji
  Enova, ustal ją z administratorem systemu, jeśli weryfikator ma nie być widoczny w edycji planu).
- Weryfikator nie ma własnych parametrów do ustawienia przez użytkownika — korzysta wyłącznie
  z danych już zapisanych w kartotece pracownika (etat i przypisany do niego okres zatrudnienia:
  data „Od” i data „Do”).

## 4. Komunikaty błędów

| Komunikat | Kiedy się pojawia | Co oznacza dla użytkownika |
|---|---|---|
| `Pracownik nie posiada aktywnego etatu w dniu {data}` | Dzień planu wypada w okresie, dla którego pracownik nie ma żadnego aktywnego etatu (np. przerwa między dwoma umowami, dzień poza jakimkolwiek zatrudnieniem). | Nie można zapisać dnia planu dla tej daty — pracownik w tym dniu formalnie nie jest zatrudniony w żadnym etacie. Sprawdź historię etatów pracownika. |
| `Nie można modyfikować planu pracy w dniach poza okresem zatrudnienia: Okres zatrudnienia {okres}` | Dzień planu jest wcześniejszy niż data „Od” lub późniejszy niż data „Do” aktywnego etatu. | Zmiana dotyczy dnia spoza okresu trwania umowy — albo popraw datę w planie, albo w razie potrzeby skoryguj okres zatrudnienia w kartotece pracownika. |

Brak komunikatu (wynik pusty) oznacza, że dzień mieści się w okresie zatrudnienia i zmiana jest
dozwolona.

## 5. Zasady graniczne

- Dzień planu równy dacie **„Od”** lub dacie **„Do”** okresu zatrudnienia jest traktowany jako
  mieszczący się w zatrudnieniu (dozwolony) — porównania są ścisłe (`<`, `>`), więc same granice
  nie są blokowane.
- Jeśli etat nie ma ustawionej daty „Do” (umowa na czas nieokreślony), górna granica okresu nie
  jest sprawdzana — dni w dowolnie odległej przyszłości są dozwolone, o ile mieszczą się w tym
  etacie.

## 6. Scenariusze testowe

| Lp | Scenariusz | Dane wejściowe | Oczekiwany wynik |
|---|---|---|---|
| 1 | Dzień w środku okresu zatrudnienia | Etat: 01.01.2026–31.12.2026; dzień planu: 15.06.2026 | Brak błędu |
| 2 | Dzień równy dacie rozpoczęcia etatu | Etat: 01.01.2026–31.12.2026; dzień planu: 01.01.2026 | Brak błędu (granica domknięta) |
| 3 | Dzień równy dacie zakończenia etatu | Etat: 01.01.2026–31.12.2026; dzień planu: 31.12.2026 | Brak błędu (granica domknięta) |
| 4 | Dzień przed datą rozpoczęcia etatu | Etat: 01.01.2026–31.12.2026; dzień planu: 31.12.2025 | Błąd: „Nie można modyfikować planu pracy w dniach poza okresem zatrudnienia…” |
| 5 | Dzień po dacie zakończenia etatu | Etat: 01.01.2026–31.12.2026; dzień planu: 01.01.2027 | Błąd: „Nie można modyfikować planu pracy w dniach poza okresem zatrudnienia…” |
| 6 | Dzień w przerwie między dwoma etatami (np. po zwolnieniu, przed ponownym zatrudnieniem) | Etat 1: do 31.03.2026; Etat 2: od 01.06.2026; dzień planu: 15.04.2026 | Błąd: „Pracownik nie posiada aktywnego etatu w dniu 2026-04-15” |
| 7 | Umowa na czas nieokreślony, dzień daleko w przyszłości | Etat: od 01.01.2026, brak daty „Do”; dzień planu: 01.01.2035 | Brak błędu |
| 8 | Umowa na czas nieokreślony, dzień przed zatrudnieniem | Etat: od 01.01.2026, brak daty „Do”; dzień planu: 01.01.2025 | Błąd: „Nie można modyfikować planu pracy w dniach poza okresem zatrudnienia…” |
| 9 | Dzień planu bez przypisanego pracownika (np. edycja wzorca/szablonu) | `dzien.Pracownik == null` | Brak błędu (weryfikacja pomijana) |
| 10 | Pracownik z dwoma kolejnymi etatami, dzień w drugim etacie | Etat 1: 01.01.2025–31.12.2025; Etat 2: 01.01.2026–31.12.2026; dzień planu: 15.03.2026 | Brak błędu — weryfikacja liczona względem etatu aktywnego w tym dniu (Etat 2) |

## 7. Ograniczenia

- Weryfikator sprawdza wyłącznie zgodność dnia planu z okresem zatrudnienia. Nie sprawdza innych
  reguł biznesowych (np. zgodności z normą czasu pracy, kolizji z nieobecnościami itp.) — to zakres
  innych weryfikatorów.
- Treść komunikatu z pkt 4 (druga pozycja) zawiera pełny opis okresu zatrudnienia w formacie
  generowanym automatycznie przez system (`{okresZatrudnienia}`) — jego dokładny format zależy od
  ustawień regionalnych/formatowania dat w Enova.
