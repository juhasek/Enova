# Pozostałe funkcje dodatku AltOne.Skanska.Ext (poza Benefitami)

Uzupełnienie do [Benefity.md](Benefity.md) — reszta zawartości tego samego `AltOne.Skanska.Ext.dll`.
Poniższe obszary są opisane płycej niż moduł Benefitów (na podstawie nazw klas/metod/pól, stałych
tekstowych z metadanych i deklaracji `[Worker]`/`[FolderView]`/`[SimpleRight]`, bez pełnej analizy
IL każdej metody) — tam gdzie płynie to wprost z kodu, jest to zaznaczone osobno.

## 1. Blokada edycji kartoteki pracownika — integracja z Oracle HCM

Klasa **`Wyzwalacze`** rejestruje ok. 90 wyzwalaczy `BlokujXxx` blokujących edycję pojedynczych pól
kartoteki pracownika (dane osobowe, adres, etat, umowa, kalendarz, nadgodziny, nieobecności — patrz
pełna lista pól w kodzie) w polskiej wersji enova. Powód (odczytany wprost z komunikatu w kodzie,
metoda `WeryfikatorPracownika.IsValid` — zweryfikowane w IL, zob. [Benefity.md](Benefity.md) nie
dotyczy, to osobny weryfikator w tym samym DLL):

> „Edycja wyłączona, dane wprowadzane w systemie Oracle HCM"

Innymi słowy: enova jest w tym wdrożeniu **odbiorcą** danych kadrowych z zewnętrznego systemu Oracle
HCM (najpewniej przez integrację/import), więc pola źródłowe są zablokowane do ręcznej edycji w
enova, żeby nie rozjeżdżały się z danymi z Oracle. Prawo do wyjątkowej edycji reguluje osobne prawo
proste (`[SimpleRight]`): **„Zezwolenie na Edycję Oracle HCM"** (`Soneta.Business.App.Login`,
opis: „Zezwala edycję pracowników integrowanych z systemu Oracle HCM").

## 2. Podzielnik kosztów — praca zdalna i dodatek brygadzistowski

Worker **`DodajPodzielnikA1Worker`** (`DodajPodzielnikA1Params`, parametr: miesiąc) — z listy
pracowników, mechanizm dzielenia/alokowania kosztu **dodatku brygadzistowskiego** (`DodatekBryg`,
„Dodatek brygadzistowski") i **pracy zdalnej** (`PracaZdalna`) pomiędzy obiekty kosztowe (Projekt /
Task / Expenditure Organization — nazewnictwo rodem z Oracle Projects/PPM). Log operacji rozróżnia
dodanie nowego wpisu podzielnika od modyfikacji istniejącego („Dodanie podzielnika”, „Modyfikacja
istniejącego wpisu”, „Podzielnik został zmodyfikowany.”). Efekt tej alokacji zasila najpewniej
późniejszy eksport księgowań (§3) — dodatek brygadzistowski i praca zdalna muszą trafić na właściwe
projekty/zadania w systemie księgowym.

Osobny, niezależny worker **`LimitPracyZdalnejWorker`** liczy/pilnuje limitu dni pracy zdalnej w
okresach: tygodniowym, miesięcznym, kwartalnym, półrocznym, rocznym (cecha `LimitPracyZdalnejRodzaj`
na definicji limitu).

## 3. Eksport księgowań listy płac do Oracle (GL / PPM)

Pakiet klas w `Workers.KsiegowaniaList`:

- **`EksportujKsiegowaniaListPlacWorker`** (na `Soneta.Core.DokEwidencja` — dokument ewidencji
  księgowej) — buduje dwa rodzaje plików wyjściowych z dekretów listy płac:
  - **GL** (General Ledger) — plik z pełną strukturą segmentów księgowych Oracle E-Business
    Suite/Fusion (Segment1-30, ATTRIBUTE1-20, REFERENCE1-20 itd. — pełna struktura interfejsu
    księgowania Oracle),
  - **PPM** (Project Portfolio Management) — plik z danymi projektowymi (Project/Task/Expenditure,
    numer osoby, typ transakcji, kwoty w walucie transakcji/księgi) — najpewniej właśnie tu trafiają
    koszty rozdzielone przez podzielnik (§2).
  - Format wyjściowy: CSV (`CsvExporter`), z konfigurowalnym separatorem/kodowaniem/nagłówkiem
    (`Options`), plik nazywany wg wzorca z prefiksem `GL_`/`PPM_` i sygnaturą czasową.
  - Dokument pominięty w eksporcie, jeśli nie jest powiązany z listą płac (komunikat: „Dokument …
    został pominięty ponieważ nie jest powiązany z listą płac”).
- **`ZmianaStatusu`** — worker zmieniający status eksportu dekretu (`ZmienStatusEksportu`), żeby nie
  eksportować tego samego dekretu dwukrotnie.

## 4. Przypomnienia mailowe HR

- **`PowiadomienieBadaniaLekarskie`** — wysyła e-mail przypominający o zbliżającym się terminie
  okresowych badań lekarskich, na podstawie cechy `A1TerminBadania` na kartotece. Dwa progi
  przypomnienia widoczne w stałych: 3 miesiące i 1 miesiąc + 1 dzień przed terminem
  (`BadaniaPrzypomnienie3mc`, `BadaniaPrzypomnienie1mc1d`). Loguje liczbę zweryfikowanych,
  pominiętych (zwolniony/brak etatu, termin poza oknem tolerancji) i błędnych przypadków.
- **`PowiadomienieOkresZasilkowy`** — przypomina o zbliżającym się końcu **okresu zasiłkowego**
  (świadczenie chorobowe ZUS) na podstawie cechy przechowującej datę końca okresu (parametryzowana
  nazwa cechy — parametr `CechaOkresZasilkowy`), z konfigurowalną liczbą dni wyprzedzenia
  (`DniPrzed`) i oknem tolerancji. Tworzy zadanie (task) z przypomnieniem dla właściwej osoby.

## 5. Raport pełnej listy płac

**`RaportPelnaListPlacWorker`** (na `Soneta.Place.ListyPlac`) — generuje plik XLSX
(`PelnaListaPlac_yyyyMMdd_HHmmss.xlsx`) z zestawieniem wszystkich wypłat z zaznaczonych list płac:
dane pracownika, kwota brutto/na przelew, oraz **rozbicie składek ZUS pracownik/pracodawca** osobno
dla każdego ryzyka (emerytalna, rentowa, chorobowa, wypadkowa, zdrowotna), Fundusz Pracy, FGŚP, FEP,
PPK pracownik/pracodawca, zaliczka na podatek. Plik zapisywany domyślnie do katalogu „enova -
Raporty” (konfigurowalny parametrem `KatalogDocelowy`).

## 6. Załączniki BHP i szkolenia — dodatkowe zakładki list

Cztery bliźniacze workery (`ListaZalacznikowBHPWorker`, `ListaZalacznikowUkonczoneSzkoleniaWorker`,
`ListaZalacznikowUprawnieniaWorker`, `ListaZalacznikowWorker`) budujące listy załączników
powiązanych z pracownikiem dla trzech kategorii dokumentów BHP: badania (`Badanie.form.xml`),
ukończone szkolenia (`UkonczoneSzkolenie.form.xml`), uprawnienia (`Uprawnienia.form.xml`) — każdy z
własną zakładką na formularzu pracownika udostępniającą podpięte skany/dokumenty.

## 7. Drobne funkcje pomocnicze

- **`OperatorExt`/`RoleExt`** — sprawdzanie, czy operator posiada daną rolę, pobieranie ról
  operatora / operatorów przypisanych do roli (używane w regułach widoczności/uprawnień w innych
  miejscach dodatku).
- **`ZmianaOperatoraPulpitu`** — przełącza operatora skojarzonego z pulpitem pracownika/kierownika
  (np. przy zmianie osoby obsługującej konto), z zabezpieczeniem sprawdzającym dostęp do WWW.
- **`DashboardFirmaLogoExtender`** — kafelek na kokpicie (dashboard) wyświetlający logo firmy
  (Skanska) jako obrazek HTML wbudowany w zasób DLL (`Zasoby/logo_skanska.png`).
- **`NipTools`** (`CRM.Kontrahent`) — czyszczenie numeru NIP z niecyfrowych znaków.
- **`WysylkaEmailExt`/`SzablonWiadomosciExt`/`KontoPocztoweExt`** — pomocnicze metody do wysyłki
  e-mail z szablonu i wyszukiwania kont pocztowych po nazwie (wspólna infrastruktura mailingowa
  używana m.in. przez przypomnienia HR z §4).
- **`StrukturaExt`, `DbTupleExt`/`DbTupleDefinitionExt`, `CfgNodeTools`** — narzędzia ogólne:
  wyszukiwanie struktury organizacyjnej po nazwie, obsługa krotek (komentarze/historia/tuple
  podrzędne workflow), odczyt/zapis konfiguracji.
- **`AttachmentExtenderA1`** — ogólny mechanizm dodawania/tworzenia załączników, używany przez
  funkcje z §6.

## 8. Do potwierdzenia w środowisku docelowym

- Ten dokument opiera się głównie na nazwach klas/metod/pól i stałych tekstowych — w przeciwieństwie
  do [Benefity.md](Benefity.md) nie wszystkie metody zostały prześledzone instrukcja po instrukcji w
  IL. Jeśli potrzebny jest analogiczny poziom szczegółu (np. dokładny algorytm podzielnika kosztów
  albo dokładny format pliku GL/PPM), warto zlecić osobną, pogłębioną analizę tego konkretnego
  obszaru.
- Nazwy cech (`CechaOkresZasilkowy`, `A1TerminBadania` itd.) i ich rzeczywiste wartości/progi są
  danymi konfiguracyjnymi w bazie enova, nie kodem — do zweryfikowania bezpośrednio w środowisku.
