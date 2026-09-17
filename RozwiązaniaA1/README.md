# RozwiązaniaA1 — dokumentacja dodatków AltOne dla Skanska

Ten folder zawiera dokumentację **analityczną** (nie kod repozytorium) gotowych, skompilowanych
dodatków enova365 dostarczonych przez firmę AltOne dla Skanska — obecnie **dwóch osobnych DLL**:
`AltOne.Skanska.Ext.dll` (kafeteria benefitów, blokady kartoteki, podzielnik kosztów, eksport
księgowań — patrz niżej) i `AltOne.Skanska.Integracja.dll` (import danych kadrowych z Oracle HCM
— patrz [Integracja z Oracle HCM.md](Integracja%20z%20Oracle%20HCM.md)). W odróżnieniu od
pozostałych folderów tego repo (Raporty/, Cechy/, Weryfikatory/ itd.), które przechowują
fragmenty kodu wklejane do edytora skryptów enova, `RozwiązaniaA1/` dokumentuje **zewnętrzne
dodatki .NET** (`.csproj`/DLL, nie skrypty) — stąd osobny folder, bez pary
plik-bez-rozszerzenia + `.md`.

Nazwa folderu nawiązuje do konwencji nazewnictwa w samym dodatku — wszystkie jego klasy biznesowe
mają przyrostek **„A1"** (np. `BenefityA1ViewInfo`, `DodajZestawA1Worker`, `DodajPodzielnikA1Worker`),
co jest podpisem firmy **AltOne** jako autora rozwiązania.

## Metodologia analizy

**`AltOne.Skanska.Ext.dll`** nie zawierał kodu źródłowego ani PDB, a lokalnie nie udało się
rozwiązać wszystkich referencji do dekompilacji pełnego C# — dokumentacja powstała na podstawie
**statycznej analizy metadanych .NET** (tabele metadanych ECMA-335: TypeDef/MethodDef/Field/
CustomAttribute, sterty `#Strings`/`#US`/`#Blob`) oraz **odczytu bajtkodu IL** kluczowych metod
(bez dekompilacji do C#, ale z rozwiązaniem tokenów na czytelne nazwy metod/pól/stringów).
Analizowana wersja: zestaw 2.0, kompilowany pod `Soneta.Business` / `Soneta.KadryPlace` /
`Soneta.Core` / `Soneta.Ksiega` / `Soneta.CRM` **2604.4.4.0**.

**`AltOne.Skanska.Integracja.dll`** (opisany w
[Integracja z Oracle HCM.md](Integracja%20z%20Oracle%20HCM.md)) udało się za to **w pełni
zdekompilować do czytelnego C#** (`ilspycmd`, z referencjami do enova **2512.5.6** dostępnej
lokalnie) — ta dokumentacja ma więc wyższy stopień pewności, zweryfikowana metoda po metodzie,
nie tylko po nazwach.

Ponieważ analiza jest statyczna (bez środowiska enova do uruchomienia dodatków), część
szczegółów (dokładne progi liczbowe cech, treści niewidoczne w metadanych, zachowanie w warunkach
rzeczywistego obciążenia) mogła nie zostać wychwycona — tam, gdzie wniosek wynika z odczytu
samego kodu a nie z domysłu, jest to zaznaczone.

## Zawartość — AltOne.Skanska.Ext.dll

- **[Benefity.md](Benefity.md)** — główny moduł: kafeteria benefitów pozapłacowych (Karta Sportowa,
  Opieka, Ubezpieczenie), przyznawanych pracownikom i osobom towarzyszącym jako elementy listy płac.
  Opis obejmuje pełny mechanizm zapisu (na poziomie odczytanego IL), nie tylko listę klas.
- **[Benefity - dokument powdrożeniowy.md](Benefity%20-%20dokument%20powdrożeniowy.md)** —
  zestawienie wymagań z analizy przedwdrożeniowej (Medicover, UNUM, Multisport) ze stanem
  potwierdzonym w kodzie: co jest zaimplementowane zgodnie ze specyfikacją, co jest mechanizmem
  generycznym opartym o dane (do weryfikacji w środowisku), a czego nie udało się potwierdzić w tym
  DLL (rekomendacje do dalszej weryfikacji z zespołem wdrożeniowym).
- **[Pozostale-funkcje.md](Pozostale-funkcje.md)** — pozostała zawartość tego samego DLL,
  niezwiązana z Benefitami: podzielnik kosztów (praca zdalna / dodatek brygadzistowski, eksport do
  Oracle GL/PPM), załączniki BHP i przypomnienia (badania lekarskie, okres zasiłkowy), pełny raport
  listy płac, blokada edycji kartoteki dla integracji z Oracle HCM, dashboard, zmiana operatora
  pulpitu.

## Zawartość — AltOne.Skanska.Integracja.dll

- **[Integracja z Oracle HCM.md](Integracja%20z%20Oracle%20HCM.md)** — kompletny mechanizm importu
  danych kadrowych, zatrudnienia, wynagrodzeń i słowników projektów/tasków z bazy pośredniczącej
  zasilanej przez Oracle HCM: architektura (staging DB → kolejka `ImportDane` w enova →
  kartoteka), dwuetapowy proces synchronizacji z workerami, reguły dopasowania pracownika,
  mapowanie każdego typu danych (osoba/adres/rodzina/zatrudnienie/wynagrodzenie/konto
  bankowe/niepełnosprawność/paszport/projekty), zabezpieczenia (limit 100 aktualizacji, wymuszona
  kolejność chronologiczna per pracownik), GUI i konfiguracja. To jest mechanizm, dla którego
  `Ext.dll` blokuje ręczną edycję kartoteki (§1 w [Pozostale-funkcje.md](Pozostale-funkcje.md)).
