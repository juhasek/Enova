# RozwiązaniaA1 — dokumentacja dodatku AltOne.Skanska.Ext

Ten folder zawiera dokumentację **analityczną** (nie kod repozytorium) gotowego, skompilowanego
dodatku enova365 `AltOne.Skanska.Ext.dll`, dostarczonego przez firmę AltOne dla Skanska. W
odróżnieniu od pozostałych folderów tego repo (Raporty/, Cechy/, Weryfikatory/ itd.), które
przechowują fragmenty kodu wklejane do edytora skryptów enova, `RozwiązaniaA1/` dokumentuje
**zewnętrzny dodatek .NET** (`.csproj`/DLL, nie skrypt) — stąd osobny folder, bez pary
plik-bez-rozszerzenia + `.md`.

Nazwa folderu nawiązuje do konwencji nazewnictwa w samym dodatku — wszystkie jego klasy biznesowe
mają przyrostek **„A1"** (np. `BenefityA1ViewInfo`, `DodajZestawA1Worker`, `DodajPodzielnikA1Worker`),
co jest podpisem firmy **AltOne** jako autora rozwiązania.

## Metodologia analizy

DLL nie zawiera kodu źródłowego ani PDB. Dokumentacja powstała na podstawie **statycznej analizy
metadanych .NET** (tabele metadanych ECMA-335: TypeDef/MethodDef/Field/CustomAttribute, sterty
`#Strings`/`#US`/`#Blob`) oraz **odczytu bajtkodu IL** kluczowych metod (bez dekompilacji do C#,
ale z rozwiązaniem tokenów na czytelne nazwy metod/pól/stringów). Analizowana wersja:
`AltOne.Skanska.Ext.dll` w wersji zestawu 2.0, kompilowana pod `Soneta.Business` / `Soneta.KadryPlace`
/ `Soneta.Core` / `Soneta.Ksiega` / `Soneta.CRM` **2604.4.4.0**.

Ponieważ analiza jest statyczna (bez środowiska enova do uruchomienia dodatku), część szczegółów
(dokładne progi liczbowe cech, treści niewidoczne w metadanych) mogła nie zostać wychwycona — tam,
gdzie wniosek wynika z odczytu samego kodu IL a nie z domysłu, jest to zaznaczone.

## Zawartość

- **[Benefity.md](Benefity.md)** — główny moduł: kafeteria benefitów pozapłacowych (Karta Sportowa,
  Opieka, Ubezpieczenie), przyznawanych pracownikom i osobom towarzyszącym jako elementy listy płac.
  Opis obejmuje pełny mechanizm zapisu (na poziomie odczytanego IL), nie tylko listę klas.
- **[Pozostale-funkcje.md](Pozostale-funkcje.md)** — pozostała zawartość tego samego DLL,
  niezwiązana z Benefitami: podzielnik kosztów (praca zdalna / dodatek brygadzistowski, eksport do
  Oracle GL/PPM), załączniki BHP i przypomnienia (badania lekarskie, okres zasiłkowy), pełny raport
  listy płac, blokada edycji kartoteki dla integracji z Oracle HCM, dashboard, zmiana operatora
  pulpitu.
