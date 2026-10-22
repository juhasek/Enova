# Cechy dokumentu aktualizacji kalendarza – dokumentacja biznesowa

## 1. Po co

Lista „Dokumenty aktualizacji kalendarzy” pokazuje standardowo tylko datę, numer, okres i stan.
Zatwierdzonego dokumentu nie da się anulować (w enova po stanie Zatwierdzony nie ma już żadnego
przejścia). Gdy kierownik zatwierdzi nowy plan na ten sam okres, oba dokumenty mają stan
„Zatwierdzony”, choć obowiązuje tylko nowszy: przy zatwierdzeniu dni z dokumentu są kopiowane
do kalendarza pracownika, a starsza wersja dnia trafia do historii kalendarza.

Cechy poniżej pokazują na liście, który dokument jeszcze obowiązuje, kogo dotyczy i kiedy
został zatwierdzony.

## 2. Cechy (tabela DokAktKalendarzy, kategoria „Aktualizacja kalendarza”)

Wszystkie: algorytm „Wartość wyliczana algorytmem” (`Feature_X { get; }`), projekt
`Soneta.Runtime.Database.Business`.

| Cecha | Typ | Co zwraca |
|---|---|---|
| `Aktualny` | Tak/Nie | Anulowany → Nie. Wypełniany / Do akceptacji → Tak. Zatwierdzony → Tak, jeżeli choć jeden jego dzień (pracownik + data) nie został nadpisany przez **później zatwierdzony** dokument tego samego rodzaju (plan z planem, czas z czasem). Porównanie po `DataZatwierdzenia`. Zatwierdzony dokument bez własnych dni (zatwierdzony plan bez zmian) → Tak. |
| `PracownikKod` | Tekst | Kody pracowników z pozycji dokumentu (umowa → jej pracownik), po przecinku. |
| `PracownikNazwisko` | Tekst | Nazwiska i imiona pracowników j.w. |
| `DzialKod` | Tekst | Kody wydziałów pracowników z etatu na **ostatni dzień okresu** dokumentu. |
| `DzialNazwa` | Tekst | Nazwy tych wydziałów. |
| `Przelozony` | Tekst | Bezpośredni przełożony wg **domyślnej struktury podległości** (`pracownik.StrukturaOraganizacyjna.GetDomyślnyPrzełożony(data)` – literówka „Oraganizacyjna” jest w API enova) na ostatni dzień okresu. Pole „Odpowiedzialny” dokumentu nie nadaje się do tego – przy dokumencie składanym przez pracownika jest tam on sam. |
| `ElementStruktury` | Tekst | Element struktury organizacyjnej wskazany na dokumencie: `Nazwa (Kod)`. Odczyt `Row.Powiazanie.ElementStrukturyOrganizacyjnej` – element podpięty do dokumentu przy zakładaniu (parametr „Element”), w strukturze wskazanej w definicji dokumentu. Pusty, gdy definicja nie jest powiązana ze strukturą organizacyjną. |
| `ZatwierdzonoDataGodzina` | Tekst | Data i godzina zatwierdzenia `rrrr-MM-dd GG:mm` (enova zapisuje `DataZatwierdzenia = DateTime.Now` przy przejściu w stan Zatwierdzony); dla niezatwierdzonych pusta. |
| `ZlozylKto` | Tekst | Kto przekazał dokument do akceptacji – wykonawca ostatniego wykonanego zadania procesu w kroku **Wypełniany** albo **Poprawiany**. Operator programu jako „Kod (Nazwa)”, pracownik z pulpitu jako „Nazwisko Imię (Kod)”. |
| `ZlozylKiedy` | Tekst | Data i godzina tego przekazania `rrrr-MM-dd GG:mm` (czas wykonania zadania). |
| `ZlozylRola` | Tekst | Kim jest składający względem pracowników dokumentu: `Pracownik` (sam pracownik), `Przełożony` (domyślny przełożony któregoś z nich), `Inny pracownik`, `Inny operator` (operator programu, np. administrator składający za pracownika). |
| `ZatwierdzilKto` | Tekst | Kto zatwierdził – wykonawca ostatniego wykonanego zadania w kroku **Do akceptacji**; tylko dla dokumentu w stanie Zatwierdzony. |
| `ZatwierdzilKiedy` | Tekst | Data i godzina zatwierdzenia w procesie. |
| `ZatwierdzilRola` | Tekst | Kim jest zatwierdzający – jak `ZlozylRola`. |

Cechy `Zlozyl*` / `Zatwierdzil*` czytają zadania standardowego procesu „Dokumenty aktualizacji kalendarza”
(definicja `00000000-0005-0037-0001-…`, uruchamiany przy dodaniu dokumentu – ustawienie „Twórz proces”
w definicji dokumentu). Sam dokument nie pamięta, kto zmienił stan. Zadania procesu zapisują
wykonawcę (`Executor`) i czas (`ExecutionTime`) każdego kroku. Kroki rozpoznawane po Guid definicji zadania
(Wypełniany `…0037-0010`, Do akceptacji `…0037-0013`, Poprawiany `…0037-0014`). Dokument bez procesu → kolumny puste.

Kod każdej cechy: pliki `Cechy/Dokument aktualizacji kalendarza - <Cecha>`.

## 3. Kod wspólny

`Cechy/Dokument aktualizacji kalendarza - kod wspolny` – wklejany w
**Ustawienia → Systemowe → Dodatkowy kod do kompilacji** (nazwa „A1 Dokumenty aktualizacji”,
identyfikator i plik `A1DokumentyAktualizacjiKalendarza`, projekt `Soneta.Runtime.Database.Business`).
Klasa `A1.Runtime.Business.TblCodeFiles.A1DokumentyAktualizacjiKalendarza`:
`PracownicyDokumentu(dokument)`, `WydzialyDokumentu(dokument)`, `ZadanieZlozenia` / `ZadanieZatwierdzenia(dokument)`,
`Wykonawca(zadanie)`, `DataWykonania(zadanie)`, `RolaWykonawcy(dokument, zadanie)`. Cechy wywołują je pełną nazwą
(w edytorze cechy nie ma miejsca na `using`). Kod wspólny musi być zapisany **przed** cechami.

## 4. Widok listy

Podwidok „Aktualne dokumenty” – skrypt `Widoki/Dokumenty aktualizacji kalendarzy - Aktualne/dodaj-widok.ps1`.
Podwidok „Ewidencja zatwierdzeń (PIP)” – skrypt `Widoki/Dokumenty aktualizacji kalendarzy - Ewidencja zatwierdzen PIP/dodaj-widok.ps1`.

## 5. Status

- Baza Claude (2026-10-14): kod wspólny i 7 cech zapisane przez GUI, **kompilują się** (kompilator
  enova przy zapisie). Widok dodany skryptem, program go widzi (11 kolumn, cechy rozpoznane).
- **Nie sprawdzone na danych** – w bazie Claude nie ma dokumentów aktualizacji. Założenie
  próbnego przez MCP się nie udało: pole „Pracownik” na dokumencie jest typu interfejs
  (`IZrodloPlanu`, MCP go nie ustawia), a „Wybierz pracowników” dla definicji „dowolni pracownicy”
  bierze osoby tylko z elementu struktury wskazanego przy zakładaniu dokumentu – wskazanie
  elementu kończy się błędem „Powiązania można dodawać tylko do wierszy typu Soneta.Kadry.Pracownik”.
  Do sprawdzenia ręcznie w GUI: dwa zatwierdzone plany tego samego pracownika na ten sam okres →
  starszy `Aktualny = Nie`.
- Baza Claude (2026-10-22): dodana cecha `ElementStruktury` (import business XML + zapis formularza
  definicji – kompiluje się bez błędów); kolumna „Element struktury” w widoku.
- Baza Claude (2026-10-22): 6 cech `Zlozyl*` / `Zatwierdzil*` + rozszerzony kod wspólny.
  **Sprawdzone na danych**: dokument PL/DO/2026/10/1 (plan, „dowolni pracownicy”) zatwierdzony
  przez MCP w zadaniu procesu „Do akceptacji” → ZlozylKto/ZatwierdzilKto = „Administrator (Podstawowy
  operator programu)”, ZlozylKiedy 2026-10-14 16:09, ZatwierdzilKiedy 2026-10-22 10:29, role „Inny operator”.
  Przy tym wyszedł błąd `Aktualny` (zatwierdzony dokument bez własnych dni dawał Nie) – poprawiony.
  Żeby Administrator mógł podjąć decyzję, dopisano go do roli „Kadry - zatwierdzanie czasu”
  (zadanie Do akceptacji jest przypisane do tej roli).
  Niesprawdzone: wykonawca przy pracy z pulpitu (oczekiwany pracownik – `Task.Executor` zamienia
  użytkownika WWW na jego Host).
