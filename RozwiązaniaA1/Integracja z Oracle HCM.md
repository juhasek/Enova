# Integracja z Oracle HCM (`AltOne.Skanska.Integracja.dll`) — dokumentacja analityczna

Ten dokument opisuje **drugi**, osobny dodatek `.NET` firmy AltOne dla Skanska —
`AltOne.Skanska.Integracja.dll` — nie mylić z `AltOne.Skanska.Ext.dll` opisanym w
[README.md](README.md), [Benefity.md](Benefity.md) i [Pozostale-funkcje.md](Pozostale-funkcje.md).
Oba dodatki współpracują: `Ext.dll` **blokuje** ręczną edycję pól kartoteki pracownika z
komunikatem „Edycja wyłączona, dane wprowadzane w systemie Oracle HCM" (patrz
[Pozostale-funkcje.md §1](Pozostale-funkcje.md#1-blokada-edycji-kartoteki-pracownika--integracja-z-oracle-hcm)),
a **ten** dodatek (`Integracja.dll`) jest właśnie tym mechanizmem, który te dane z Oracle HCM
faktycznie wprowadza.

Analizowana wersja: zestaw `AltOne.Skanska.Integracja`, `AssemblyVersion`/`AssemblyFileVersion`
**2504.3.5.0**, kompilowany pod `.NET 8` / `Soneta.Business`, `Soneta.Kadry`, `Soneta.Kadry` (moduł
`KadryModule`), `Soneta.Place`, `Soneta.Ksiega`, `Soneta.CRM`, `Soneta.HR`, `Soneta.Kasa`. Producent
w metadanych zestawu: „Alt One Wojnarowski Zaworski Spółka Komandytowa".

## 0. Metodologia

W odróżnieniu od analizy `Ext.dll` (czysty odczyt metadanych/IL bez dekompilacji — DLL nie miał
kompletu referencji dostępnych lokalnie), do tego pliku udało się dekompilować **pełny,
czytelny kod C#** (`ilspycmd`, z referencjami do `Soneta.Products.Server.Standard` w wersji
**2512.5.6** — inna niż wersja docelowa Skanska 2604.4.4, ale wystarczająca do rozwiązania
większości typów) i przeczytać go metodą po metodzie — nie tylko nazwy klas/pól. Ten dokument
ma więc wyższy stopień pewności niż `Pozostale-funkcje.md`, zbliżony do `Benefity.md`. DLL nie
zawiera kodu embedded-resource dla samej logiki, ale zawiera trzy pliki `.pageform.xml`
(widoki) i jeden `.pageform.xml` (formularz konfiguracji) jako zasoby — te też zostały odczytane
wprost (nie trzeba było ich odtwarzać z IL).

Ponieważ analiza jest offline (bez środowiska Skanska i bez dostępu do bazy pośredniczącej), nie
dało się zweryfikować danych faktycznie płynących z Oracle HCM ani zachowania w warunkach
rzeczywistego obciążenia — tylko to, co wynika wprost z kodu.

## 1. Architektura ogólna

```
Oracle HCM (system źródłowy)
        │  (proces poza zasięgiem tego dodatku — ETL/eksport Oracle, nieznany z tego kodu)
        ▼
Baza pośrednicząca SQL Server ("staging")           ← konfiguracja: DbServer/DbDatabase/DbLogin/DbPassword
  tabele: HCM_PERSON, HCM_ADDRESS, HCM_FAMILY,          (Kadry i płace/Integracje z Oracle/Konfiguracja)
  HCM_BANK_ACCOUNT, HCM_DISABILITY,
  HCM_EMPLOYMENT_TERMS, HCM_PASSPORT, HCM_SALARY,
  Departaments, Z_KORAB_ACTIVITY_OUTBOUND (Task),
  Z_KORAB_PROJECT_INFORMATION_OUTBOUND (Project),
  Z_MOKO_PJO_WBS_OUTBOUND (ProjectTask)
  — każdy wiersz ma SYNC_STATUS: New / Synchronized / Error
        │
        │  KROK 1: przycisk "Importuj dane z bazy danych"
        │  (ImportDanychDBWorker, Entity Framework Core, AltOneDbContext)
        │  — pobiera wiersze SyncStatus=New, zapisuje w enova jako kolejka `ImportDane`
        │  (surowy JSON w polu Dane), w bazie zewnętrznej ustawia SyncStatus=Synchronized
        ▼
Kolejka `ImportDane` w enova (tabela własna dodatku, moduł "ImportDanych")
  — widoczna w GUI: Kadry i płace / Integracje z Oracle / {Dane kadrowe, Wynagrodzenia,
    Projekty i Taski} — filtrowalna po pracowniku/okresie/statusie/rodzaju danych
        │
        │  KROK 2: przycisk "Synchronizuj dane"
        │  (SynchronizacjaDanychWorker → ProcessOperations.ProcessHcm)
        │  — grupuje po IntegrationId, przetwarza chronologicznie najstarszą grupę,
        │  deserializuje JSON z powrotem do DTO i woła DataOperations/EmployeeMapper
        ▼
Kartoteka pracownika enova (Pracownik/PracHistoria/Etat/Adresy/Rodzina/Umowy/Dodatki/...)
```

Dodatek **nie łączy się z Oracle bezpośrednio** — pośredniczy baza SQL Server ("staging"),
najpewniej zasilana osobnym procesem ETL/eksportem z Oracle HCM (spoza zakresu tego kodu; nazwy
tabel projektowych `Z_KORAB_...`/`Z_MOKO_...` sugerują dedykowane interfejsy wyjściowe Oracle
zbudowane pod tę integrację, ale ich pochodzenie nie jest widoczne z tego DLL).

## 2. Baza pośrednicząca — encje (EF Core, `AltOneDbContext : DbContext`)

Każda tabela ma wspólny szkielet: `ID` (PK, `long`), `INTEGRATION_ID` (klucz logiczny rekordu
źródłowego — po nim grupowane jest przetwarzanie), `CREATED` (znacznik czasu z Oracle — decyduje
o kolejności przetwarzania), `SYNC_STATUS`, `ID_CREATE_AT`/`ID_UPDATED_AT` (znaczniki techniczne
po stronie staging). Tabele projektowe (Task/Project/ProjectTask) nie mają `INTEGRATION_ID` ani
`PERSON_NUMBER` — to słowniki, nie dane osobowe.

| Tabela (staging) | Klucz osoby | Najważniejsze pola |
|---|---|---|
| `HCM_PERSON` | `PERSON_NUMBER` | `PrimaryEmployer`, `Lastname`/`FirstName`/`MiddleName`, `Gender`, `DateOfBirth`, `TownOfBirth`, `Nationality`, `Maidenname`, `SocialSecurityNumber` (PESEL), `HealthInsuranceOfficeCode`, `TaxId` (NIP), `TaxOfficeId`, `Citizenship` |
| `HCM_ADDRESS` | `PERSON_NUMBER` | `AddressType` (`SKA_1`/`SKA_2`/`SKA_3`), `Street`/`BuildingNo`/`FlatNo`/`PostalCode`/`TownOrCity`/`Municipality`/`County`/`Voivodship`/`CountryCode` |
| `HCM_FAMILY` | `PERSON_NUMBER` | j.w. + `LastName`/`FirstName`/`DateOfBirth`/`Relationship`/`Gender`/`SocialSecurityNumber`/`DissabilityLevel` + adres |
| `HCM_BANK_ACCOUNT` | `PERSON_NUMBER` | `AccountNumberName` (nazwa banku), `AccountNumber` |
| `HCM_DISABILITY` | `PERSON_NUMBER` | `DissabilityLevel` (`PL_1`..`PL_4`), `StartDate`/`EndDate` |
| `HCM_PASSPORT` | `PERSON_NUMBER` | `PassportNumber`, `PassportIssueDate`, `PassportValidityDate` |
| `HCM_EMPLOYMENT_TERMS` | `PERSON_NUMBER` + `AssignmentNumber` | `LegalEntity`, `ContractType`, `EffectiveStartDate`/`ProjectedEndDate`/`HireDate`/`TerminationDate`, `DepartmentCode`, `LocationCode`, `FullTimeEquivalent`, `JobCode`, `ManagerAssignmentNumber`, `ProjectAssignment`/`TaskEconomicalStructure`, `TerminationActionCode`/`TerminationReasonCode` |
| `HCM_SALARY` | `AssignmentNumber` | `ElementName`, `DateFrom`/`DateTo`, `Amount` |
| `Departaments` | — | `DeptUnitID`, `DeptUnitNameEN`/`PL`, `DeptUnitManager`, `Parent`, `BranchCode`/`DivisionCode` (**zaimportowana do EF, ale bez `DepartmentService` użytego przez worker — patrz §6**) |
| `Z_KORAB_ACTIVITY_OUTBOUND` (Task) | — | `SERVICE_TYPE_CODE`/`DESC`, `START_DATE`/`END_DATE` |
| `Z_KORAB_PROJECT_INFORMATION_OUTBOUND` (Project) | — | `PROJECT_NUMBER`, `SHORT_NAME`/`PROJECT_NAME`, `PROJECT_STATUS`, `CONTRACTUAL_START/END_DATE` |
| `Z_MOKO_PJO_WBS_OUTBOUND` (ProjectTask) | — | `PROJECT_NUMBER`, `TASK_NUMBER`, `TASK_NAME` |

## 3. Kolejka `ImportDane` w enova

Własna tabela dodatku (moduł `ImportDanych`, klasa wiersza `ImportDane`), z polami: `Guid`,
`IntegrationId`, `Pracownik` (dopasowany, może być pusty), `DataType` (enum: `Osoba`, `Adres`,
`Rodzina`, `Zatrudnienie`, `Wyplata`, `KontaBankowe`, `Niepelnosprawnosc`, `Paszport`, `Projekt`,
`Task`, `ProjektTask`), `StatusType` (enum: `Zrealizowane`, `DoSynchronizacji`, `Błąd`,
`WieleAktualizacji`, `Ostrzeżenie`, `Oczekuje`, `Pominięte`), `Data` (kiedy wiersz trafił do
kolejki), `Opis` (wynik/błąd przetworzenia — czytelny komunikat), `Dane` (**pełny JSON** DTO
źródłowego — surowy zrzut, widoczny w GUI, służy jako log audytowy), `ClassName` (pełna nazwa
typu DTO), `PersonNumber`, `Created`/`Time` (znacznik z Oracle, rozbity na datę+godzinę — po nim
liczona jest kolejność przetwarzania, `ImportDanychOperations.BuildTimestamp`).

**Dopasowanie pracownika przy tworzeniu wiersza kolejki** (`ImportDanychOperations.AddImportDanych`)
odbywa się przez złożenie kodu pracownika: **prefiks pracodawcy + numer osoby**
(`EmployerPrefixMapper` — trzy stałe wpisy: Skanska S.A. → `SCE`, Skanska Property Poland →
`CDE`, Skanska Residential Development Poland → `RDE`; nierozpoznana nazwa pracodawcy = brak
prefiksu, rekord trafia bez dopasowania i zostanie odrzucony jako błąd przy przetwarzaniu, patrz
`EmployerNotMapped` w `ProcessOperations`). Dla wynagrodzeń numer `AssignmentNumber` decyduje o
ścieżce: prefiks `E` = etat (szukany po cesze `AssignmentNumber` na `PracHistoria`), prefiks `C`
= umowa cywilnoprawna (szukana po cesze `AssignmentNumber` na `Umowa`, tylko nieanulowanej).

## 4. Proces synchronizacji — dwa niezależne kroki

### Krok 1 — „Importuj dane z bazy danych" (`ImportDanychDBWorker`)

Dla każdej z 11 encji staging: pobiera wiersze `SyncStatus=New`, tworzy odpowiadający wiersz
`ImportDane` (status początkowy `DoSynchronizacji`), **w bazie staging** ustawia z powrotem
`SyncStatus=Synchronized` (rekord uznany za „pobrany do enova", niezależnie od tego czy
przetworzenie się powiedzie). Uwaga: **`Departaments` jest zaimportowana do modelu EF
(`AltOneDbContext.Departments`, jest nawet `DepartmentService`), ale worker jej NIE odpytuje** —
tworzy `new DepartmentService(context)` i od razu go porzuca, nie wywołując `ImportFromService`.
Efekt: **dział/wydział nie jest w praktyce synchronizowany tą ścieżką** (mimo że cała
infrastruktura na to pozwala) — przypisanie do wydziału w `HcmEmploymentTermsEntity.
DepartmentCode` jest jedynym realnie używanym źródłem wydziału (patrz §5.4), dopasowywanym do
JUŻ ISTNIEJĄCEGO w enova wydziału po cesze „Nazwa angielska", nie tworzonym z tej tabeli.

Na końcu: skanuje kolejkę `ImportDane` w statusach `DoSynchronizacji`/`Ostrzeżenie`/
`WieleAktualizacji` i **oznacza jako `WieleAktualizacji`** każdą grupę `IntegrationId`
liczącą **więcej niż 100 wierszy** — zabezpieczenie przed zalaniem kolejki (np. błędny eksport z
Oracle powielający ten sam rekord).

### Krok 2 — „Synchronizuj dane" (`SynchronizacjaDanychWorker` → `ProcessOperations.ProcessHcm`)

1. Pobiera wiersze w statusach `DoSynchronizacji`/`Ostrzeżenie`/`WieleAktualizacji`.
2. `MarkEarlierPendingAsWarning`: dla każdego oczekującego wiersza sprawdza, czy dla **tego
   samego pracownika i tego samego `DataType`** istnieje **wcześniejszy** (po `Created`+`Time`)
   wiersz jeszcze nieprzetworzony — jeśli tak, **blokuje** nowszy wiersz statusem `Ostrzeżenie`
   („Istnieje wcześniejsza integracja, która nie została jeszcze zsynchronizowana"). To wymusza
   **ścisłą kolejność chronologiczną** zmian per pracownik/typ danych — nowsza zmiana nie może
   „wyprzedzić" starszej nieprzetworzonej.
3. Grupuje pozostałe wiersze po `IntegrationId`, sortuje grupy po najwcześniejszym znaczniku
   czasu w grupie, przetwarza **od najstarszej**. Jeśli natrafi na grupę ze statusem
   `WieleAktualizacji` — **przerywa całą dalszą synchronizację** (nowsze grupy czekają, aż
   operator ręcznie rozwiąże problem, patrz §6).
4. W obrębie jednej grupy (`ProcessHcm`) — kolejność przetwarzania wymuszona jest zawsze:
   **`HcmPersonDTO` → `HcmEmploymentTermsDTO` → reszta** (osoba musi istnieć/zostać
   zaktualizowana zanim zapisze się jej zatrudnienie). Jeśli w grupie, w której **żaden** wiersz
   nie miał jeszcze dopasowanego pracownika (nowy pracownik), przetworzenie `HcmPersonDTO` się
   nie powiedzie — **cała reszta grupy zostaje pominięta** ze statusem `Pominięte` („w
   aktualizacji osoby wystąpił błąd") zamiast próbować zapisać dane potomne do nieistniejącego
   pracownika.
5. Każdy wiersz przetwarzany jest w **osobnej sesji/transakcji** (`session.Login.CreateSession`)
   — błąd jednego wiersza nie cofa już zatwierdzonych.

### Akcje ręczne (pozostałe workery)

- **„Synchronizuj Ponownie"** (`SynchronizujPonownieWorker`, na pojedynczym wierszu `ImportDane`)
  — resetuje status na `DoSynchronizacji` z nowym znacznikiem czasu (`Data=Now`), żeby wiersz
  wszedł ponownie do kolejki Kroku 2 (np. po ręcznym poprawieniu przyczyny błędu — słownika,
  wydziału, cechy).
- **„Oznacz jako błąd"** (`SynchronizacjaBladWorker`, na zaznaczonych wierszach) — działa
  **wyłącznie** na wierszach ze statusem `WieleAktualizacji`; przestawia je na `Błąd`, co
  odblokowuje przetwarzanie kolejnych, nowszych grup (patrz krok 2.3 wyżej) — świadoma decyzja
  operatora „to podejrzane grupowe zdarzenie pomijamy".

## 5. Mapowanie danych do kartoteki enova (`EmployeeMapper`, `DataOperations`)

Wspólny wzorzec dla większości typów danych (`DataOperations.upsertXxx`): znajdź/utwórz wpis
historii pracownika **na dzień `Created`/`EffectiveStartDate`** metodą „cięcia okresu"
(`pracownik.Historia.Update(date)` — dokładnie ten sam mechanizm biznesowy, który przy imporcie
XML odtwarza się ręcznie atrybutem `date=` na `<PracHistoria>`, patrz
[[reference-import-pracownika-xml]]), zmapuj dane na TEN wpis, po czym **propaguj to samo
mapowanie na wszystkie PÓŹNIEJSZE zapisy historii** (z flagą `aktualizacjaHistorii=true` —
mapper wtedy pomija efekty uboczne typu ustawianie cechy `OracleHCM`/tworzenie nowych
dodatków/umów, tylko nadpisuje pola), żeby historia pracownika pozostała spójna „do przodu".

### 5.1 Osoba (`HcmPersonDTO` → `PracHistoria`)

Imię/nazwisko/drugie imię, płeć (`M`/inne → Kobieta), data i miejsce urodzenia, obywatelstwo
(`Obywatelstwo.KodKraju`), nazwisko rodowe, **PESEL obowiązkowy tylko dla obywatelstwa `PL`**
(rzuca wyjątek, jeśli brak), kod oddziału NFZ (z `HealthInsuranceOfficeCode`, ucięty do myślnika),
cecha „Narodowość", NIP. Podatki: `TaxOfficeId` (ucięty do myślnika) → wyszukanie
`UrzadSkarbowy` po kodzie w słowniku CRM (rzuca wyjątek, jeśli nie znaleziono).

Jeśli pracownik **nie istnieje** (brak dopasowania po kodzie) — `DataOperations.upsertEmployee`
**tworzy nowego** `Pracownik` (kod = prefiks pracodawcy + `PersonNumber`).

### 5.2 Adres (`HcmAddressDTO`/`HcmFamilyDTO` → `Adres`)

Ulica/nr domu/nr lokalu/kod pocztowy/miejscowość/gmina/powiat/województwo (mapowane ze stringa
na enum `Wojewodztwa` — 16 polskich województw + `nieokreślone` jako fallback dla nierozpoznanej
wartości)/kod kraju. `AddressType` decyduje o polu docelowym: `SKA_1`→Zameldowania,
`SKA_2`→Zamieszkania, `SKA_3`→Korespondencyjny.

### 5.3 Rodzina (`HcmFamilyDTO` → `CzlonekRodziny`)

Dopasowanie istniejącego członka rodziny po (imię, nazwisko, data urodzenia) — inaczej tworzy
nowego. Stopień pokrewieństwa: `S`→Małżonek, `11`→Dziecko, `60`→Inni krewni (inne kody
**nie są obsłużone** — pole zostaje bez zmian, brak błędu). Stopień niepełnosprawności analogicznie
jak w §5.6 (`PL_1`..`PL_4`).

### 5.4 Zatrudnienie (`HcmEmploymentTermsDTO` → `PracHistoria.Etat` / `Umowa`)

Najbardziej rozbudowana ścieżka:

- **`ContractType = B2B_CONTRACT`** → status `Pominięte` („Umowa B2B - pomijana w synchronizacji")
  — integracja świadomie NIE obsługuje kontraktów B2B.
- **Umowy cywilnoprawne** (`CIVIL_SPECIFIED_SERVICE`/`CIVIL_SPECIFIC_WORK`/`CIVIL_INTERNSHIP`/
  `CIVIL_WITHOUT_SOCIAL_SECURITY`) → `createUmowaZlecenie`: tworzy/aktualizuje `Umowa` (dopasowanie
  po cesze `AssignmentNumber`), typ elementu wg `ContractType` (praktyka absolwencka / umowa o
  dzieło 20% k.u. / umowa zlecenia 20% k.u.), wydział po cesze „Nazwa angielska", opis/tytuł
  (ucięty do 80 znaków), okres, kod wykonywanego zawodu (GUS), miejsce pracy (cecha na
  `PracHistoria`, nie na `Umowa`).
- **Umowa o pracę** — wymaga `AssignmentNumber`; **wymaga też jednocześnie projektu
  (`ProjectAssignment`) i zadania (`TaskEconomicalStructure`)** — brak któregokolwiek daje status
  `Błąd`. Typ umowy: `PROB_CONTRACT`→okres próbny, `FIXED_CONTRACT`→czas określony, `10`→czas
  nieokreślony (inne kody → `Błąd`). Zmiana okresu zatrudnienia ma nietrywialną logikę
  „inteligentnego przycinania" (`UpdateEmploymentPeriod`) — nowy okres nadpisuje cały, chyba że
  zmiana jest tylko wydłużeniem/skróceniem **w ramach tego samego typu umowy**, wtedy tylko
  przesuwa właściwą granicę. Wydział — dopasowanie po cesze „Nazwa angielska" (rzuca wyjątek, gdy
  brak). Wymiar etatu z `FullTimeEquivalent` (`Fraction`). Stanowisko po `JobCode` (słownik
  `DefStanowisk` modułu HR). Bezpośredni przełożony — dopasowanie pracownika-managera po
  `ManagerAssignmentNumber` (ten sam mechanizm prefiks+numer co §3). Rozwiązanie umowy:
  `TerminationDate` ustawia koniec okresu Etatu + cechy „Przyczyna odejścia"/„Data rozwiązania
  stos pracy" + `PrzyczRozwUmow` po `TerminationActionCode` (rzuca wyjątek, jeśli nie znaleziono).
- **Projekt domyślny pracownika** (`ApplyProjectTaskFeatures`) — ustawia cechy `DomyslnyProjekt`/
  `DomyslnyTask` na `Pracownik` na podstawie `ProjectAssignment`/`TaskEconomicalStructure`,
  waliduje że istnieje odpowiadająca para w słowniku `ProjektTask` (`{projekt}_{task}`) —
  w przeciwnym razie status `Ostrzeżenie`, nie `Błąd` (dane zapisane, ale do przejrzenia).

### 5.5 Wynagrodzenie (`HcmSalaryDTO`)

Rozgałęzienie po prefiksie `AssignmentNumber`:

- **`E...` (etat)** — `ElementName = "Annual Bonus"` → cechy „Procent bonusa"/„Bonus okres" na
  `PracHistoria` (**nie** tworzy dodatku bezpośrednio — inny mechanizm musi je odczytać). Inne
  nazwy z mapowania `AllowanceMapper` (aktualnie: „Allowance for the contract (leading project)"
  → „Dodatek za czas kontraktu", „Additional duties allowance" → „Dodatkowe czynności") → tworzy
  **nowy dodatek** (`Dodatek`/`DodHistoria`) o tej definicji, podstawie = kwota, okres z
  `DateFrom`/`DateTo`. Wszystko inne (`Monthly salary`/`Hourly salary`/domyślne) → aktualizuje
  **stawkę zaszeregowania** (miesięczna/godzinowa/bez zmiany rodzaju) na Etacie — **status
  wiersza wymuszony na `Oczekuje`** (nie od razu przetwarzane w Kroku 1 tworzenia kolejki — wpis
  czeka, dopóki ktoś/coś nie przełączy go np. przez „Synchronizuj Ponownie"; efektywnie każda
  zmiana wynagrodzenia na etacie wymaga jawnego zatwierdzenia do przetworzenia).
- **`C...` (umowa cywilnoprawna)** — `EmployeeMapper.UpdateUmowaZlecenieSalary`: osobna ścieżka
  (nie przez `PracHistoria`) — znajduje `Umowa` po cesze `AssignmentNumber`, mapuje typ
  rozliczenia (`Monthly salary`→stawka za okres, `Hourly salary`→stawka za godzinę, `One-off
  payment`→kwota do wypłaty) i kwotę na `UmowaHistoria` (z tym samym „cięciem okresu" i
  propagacją do przodu co inne dane).

**Cecha „OracleHCM" jako znacznik do weryfikacji przez HR** — w wielu miejscach (zmiana rodzaju
stawki, kwoty, opisu umowy, wydziału, miejsca pracy, wymiaru etatu, stanowiska, dat okresu) mapper
**wykrywa realną zmianę wartości** i wtedy (tylko gdy nie jest to propagacja `aktualizacjaHistorii`)
ustawia `Features["OracleHCM"] = "Do dostarczenia"` zamiast domyślnego `"Nie dotyczy"` — czyli
**cecha nie mówi "skąd pochodzą dane", tylko "czy ta konkretna zmiana wymaga fizycznego dokumentu/
przejrzenia przez HR"** (np. aneks do umowy przy zmianie stawki). Wyzwalacz w `Ext.dll`
(`Wyzwalacze.SprawdzModyfikujacegoPracHostoriaRow`, patrz [Pozostale-funkcje.md §1](Pozostale-funkcje.md))
resetuje ją z powrotem na „Nie dotyczy", gdy zapisu dokonuje **człowiek** (operator inny niż
techniczne konto `HCM_INTEGRATION`) — czyli ręczna edycja w GUI „kwituje" flagę integracji.

### 5.6 Niepełnosprawność / Paszport / Konto bankowe

- **Niepełnosprawność** — `DissabilityLevel` (`PL_1..PL_4`) → jednocześnie `PFRON.StopienPFRON`
  i `StopienNiepelnosp.Kod` (Lekki/OsobaDo16Roku/Umiarkowany/Znaczny), okres z `StartDate`/
  `EndDate` (rok `>=4711` w dacie końcowej traktowany jako „bezterminowo", `Date.MaxValue`).
- **Paszport** — wymaga dat wydania I ważności (rzuca wyjątek bez nich), zapis do
  `DokumentOsoby` z `Rodzaj=Paszport`.
- **Konto bankowe** — **nowy** rachunek (`RachunekBankowyPracownika`, `Domyslne=true`) tworzony
  tylko gdy numer konta (po oczyszczeniu do samych cyfr) **różni się** od aktualnego domyślnego;
  stary domyślny rachunek dostaje `Blokada=true` (dezaktywacja, nie usunięcie — zachowana
  historia). Bank dopasowywany/tworzony w słowniku CRM po nazwie.

### 5.7 Projekty / Taski / ProjektTask (słowniki)

Trzy proste upserty do słowników (`DictionaryTools.UpsertElement`) w module Księga: „Projekt"
(symbol=`PROJECT_NUMBER`, nazwa=`ShortName`/`ProjectName`, cechy statusu/dat), „Task"
(symbol=`SERVICE_TYPE_CODE`), „ProjektTask" (symbol=`{projekt}_{task}`, z cechami wskazującymi na
oba powiązane elementy — **jeśli referencje nie istnieją, wpis i tak powstaje**, tylko ze
statusem `Ostrzeżenie` i opisem brakujących referencji).

## 6. GUI i konfiguracja

Menu: **Kadry i płace → Integracje z Oracle** (ikona `osoba_dokument`) →
`Dane kadrowe` / `Wynagrodzenia` / `Projekty i Taski` — trzy widoki na tej samej tabeli
`ImportDane`, różniące się tylko domyślnym filtrem `DataType` (Wynagrodzenia = tylko `Wyplata`;
Projekty i Taski = `Projekt`/`Task`/`ProjektTask`; Dane kadrowe = wszystko poza tymi dwoma).
Każdy widok: filtr (Pracownik/Okres/Status/Rodzaj danych) + siatka tylko do odczytu (bez
dodawania/usuwania wierszy ręcznie) z kolumnami Pracownik/Person number/Data/Opis/Rodzaj
danych/Status; otwarcie wiersza pokazuje pełny **surowy JSON** w polu „Dane" — jedyne pole
edytowalne na karcie to „Status" (poza akcjami z toolbara).

Konfiguracja: **Kadry i płace → Integracje z Oracle → Konfiguracja** (osobna strona, prawo
`Page:KonfiguracjaPage`) — cztery pola: Serwer bazy danych / Baza danych / Login / Hasło,
przechowywane jako węzeł konfiguracji `AltOne.Skanska.Integracja/Konfiguracja` (mechanizm
`CfgManager`/`CfgNode`, ten sam co np. w [[reference_enova_zakladka_w_opcjach]]). Hasło
zapisywane jako zwykły atrybut tekstowy konfiguracji — **brak widocznego w tym kodzie
szyfrowania** (do zweryfikowania, czy `CfgManager`/`AttributeType._string` po stronie
`Soneta.Business` szyfruje wartość automatycznie na poziomie silnika, czy nie).

## 7. Do potwierdzenia w środowisku docelowym

- **Kto/co zasila bazę staging** (proces ETL z Oracle HCM) — całkowicie poza tym DLL, nie da się
  potwierdzić stąd częstotliwości/mechanizmu odświeżania danych źródłowych.
- **Czy oba workery („Importuj dane z bazy danych" i „Synchronizuj dane") są uruchamiane
  automatycznie (harmonogram) czy ręcznie** — nic w kodzie nie wskazuje na wewnętrzny scheduler;
  prawdopodobnie zewnętrzny harmonogram (np. zadanie Windows/orchestrator) woła te akcje przez
  `buscall`/API — do potwierdzenia w konfiguracji środowiska Skanska.
  (Uwaga: opisano tu tylko akcje z `[Action(...)]`, mogące być wywoływane ręcznie z toolbara;
  jeśli jest cron/harmonogram, jest poza tym plikiem).
- **Konto operatora `HCM_INTEGRATION`** (sprawdzane w `Wyzwalacze.cs` w `Ext.dll`) — jego
  istnienie/uprawnienia nie są tworzone przez ten dodatek; musi być założone ręcznie w bazie
  docelowej.
- **Szyfrowanie hasła bazy staging** w konfiguracji — patrz §6, nieoczywiste z samego kodu tego
  DLL.
- **Departaments (wydziały) faktycznie niesynchronizowane** — patrz §4 krok 1; jeśli w praktyce
  wydziały MUSZĄ istnieć w enova zanim zadziała dopasowanie po cesze „Nazwa angielska" w §5.4,
  są zakładane innym kanałem (ręcznie albo osobnym mechanizmem) — do potwierdzenia z zespołem
  wdrożeniowym, czy to zamierzone czy niedokończone.
- **`AllowanceMapper`/`EmployerPrefixMapper`** — mapowania na sztywno w kodzie (3 pracodawcy, 3
  nazwy dodatków); rozszerzenie o kolejną spółkę/dodatek wymaga zmiany i rekompilacji DLL, nie
  konfiguracji.
