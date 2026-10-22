
# Najczęściej używane foldery programu Soneta

Poniższa lista zawiera ścieżki folderów (`programFolderPath`). Używaj ich z `navigate_to_folder`, `retrieve_list`, `open_form`. Kolumny `tableName` i `selectorField` mogą być też użyte z `search_object(tableName, objectSelector)` do odszukania obiektu bez wchodzenia na listę — `search_object` zwraca jego dane, a formularz otwiera dopiero `open_form(id)` z identyfikatorem z jego odpowiedzi.

Samą ścieżkę z kolumny `programFolderPath` podaj do `search_object` zamiast `tableName`, gdy zakres wyszukiwania ma odpowiadać tej liście albo gdy tabela nie ma wyszukiwania globalnego (dokumenty, zapisy): `search_object(programFolderPath, filters=["pole==wartość"])`.

## Handel

| programFolderPath | tableName | selectorField | Opis |
|---|---|---|---|
| `Handel/Kartoteki/Towary i usługi` | Towary | EAN, Kod, Nazwa, NumerKatalogowy, PrefiksGTIN13 | Kartoteka towarów i usług |
| `Handel/Sprzedaż/Faktury sprzedaży` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Faktury sprzedaży, paragony, korekty |
| `Handel/Sprzedaż/Zamówienia od odbiorców` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Zamówienia złożone przez odbiorców |
| `Handel/Sprzedaż/Oferty do odbiorców` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Dokumenty ofert handlowych |
| `Handel/Zakup/Faktury zakupu` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Faktury zakupu oraz korekty |
| `Handel/Zakup/Zamówienia do dostawców` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Zamówienia złożone u dostawców |
| `Handel/Zakup/Faktury wewnętrzne` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Faktury wewnętrzne z wewnątrzunijnych faktur zakupu |
| `Handel/Magazyn/Dokumenty razem` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Wszystkie dokumenty magazynowe (PZ, WZ, PW, RW, MM) |
| `Handel/Magazyn/Stany magazynowe` | Towary | EAN, Kod, Nazwa, NumerKatalogowy, PrefiksGTIN13 | Stany towarów w magazynie |
| `Handel/Magazyn/Zasoby` | Zasoby | — | Stany i partie zasobów magazynowych |
| `Handel/Magazyn/Dokumenty wg kategorii/Przyjęcia magazynowe` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Dokumenty PZ oraz ich korekty |
| `Handel/Magazyn/Dokumenty wg kategorii/Wydania magazynowe` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Dokumenty WZ oraz ich korekty |
| `Handel/Magazyn/Dokumenty wg kategorii/Przesunięcia magazynowe` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Dokumenty przesunięć MM |
| `Handel/Magazyn/Dokumenty wg kategorii/Inwentaryzacja` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Inwentaryzacja, spis z natury |
| `Handel/Magazyn/Okresy magazynowe` | OkresyMag | — | Okresy magazynowe, zasoby, obroty |
| `Handel/Dokumenty razem i pozostałe/Wszystkie dokumenty` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Wszystkie dokumenty handlowe razem |
| `Handel/Dokumenty razem i pozostałe/Pozycje dokumentów` | PozycjeDokHan | — | Zestawienie pozycji ze wszystkich dokumentów |
| `Handel/Sprzedaż/Ceny i rabaty/Cennik kontrahenta` | Towary | EAN, Kod, Nazwa, NumerKatalogowy, PrefiksGTIN13 | Cennik towarów dla kontrahenta |
| `Handel/KSeF/Wysyłanie` | DokHandlowe | Numer.Pelny, Numer.Symbol, Obcy.Numer, KomunikatKSeF.NumerDokumentuKSeF | Wysyłanie dokumentów do KSeF |
| `Handel/KSeF/Pobrane` | KSeFPliki | NazwaKontrahenta, NumerKSeF | Pliki faktur pobranych z KSeF |
| `Handel/Zestawienia/Obroty wg towarów` | Towary | EAN, Kod, Nazwa, NumerKatalogowy, PrefiksGTIN13 | Obroty magazynowe wg towarów |
| `Handel/Zestawienia/Obroty wg kontrahentów` | Kontrahenci | EuVAT, Kod, Nazwa, NIP | Obroty magazynowe wg kontrahentów |
| `Handel/Zestawienia/Sprzedaż i zakupy wg towarów` | Towary | EAN, Kod, Nazwa, NumerKatalogowy, PrefiksGTIN13 | Sprzedaż i zakupy wg towarów |
| `Handel/Zestawienia/Sprzedaż i zakupy wg kontrahentów` | Kontrahenci | EuVAT, Kod, Nazwa, NIP | Sprzedaż i zakupy wg kontrahentów |

## Kontrahenci i urzędy

| programFolderPath | tableName | selectorField | Opis |
|---|---|---|---|
| `Kontrahenci i urzędy/Kontrahenci` | Kontrahenci | EuVAT, Kod, Nazwa, NIP | Kartoteka kontrahentów |
| `Kontrahenci i urzędy/Osoby` | KontaktyOsoby | Nazwisko | Osoby związane z kontrahentami |
| `Kontrahenci i urzędy/Kontrahenci wg opiekuna` | Opiekunowie | — | Kontrahenci wg opiekunów handlowych |
| `Kontrahenci i urzędy/Banki` | Banki | EuVAT, Kierunek, Kod, Nazwa, NIP | Słownik banków |
| `Kontrahenci i urzędy/Urzędy skarbowe` | UrzedySkarbowe | EuVAT, Kod, KodUrzeduSkarbowego, Nazwa, NIP | Słownik urzędów skarbowych |

## Ewidencja Środków Pieniężnych

| programFolderPath | tableName | selectorField | Opis |
|---|---|---|---|
| `Ewidencja Środków Pieniężnych/Raporty` | RaportyESP | Numer.Pelny, Numer.Symbol | Raporty zmian w ewidencjach |
| `Ewidencja Środków Pieniężnych/Dokumenty kasowe` | DokumentyKasowe | Numer.Pelny, Numer.Symbol | Wpłaty i wypłaty do kasy |
| `Ewidencja Środków Pieniężnych/Przelewy` | Przelewy | Numer.Pelny, Numer.Symbol, OnlineMgsId | Przelewy bankowe |
| `Ewidencja Środków Pieniężnych/Paczki przelewów` | PaczkiPrzelewow | Numer.Pelny, Numer.Symbol | Paczki przelewów do eksportu |
| `Ewidencja Środków Pieniężnych/Rozrachunki wg dokumentów` | RozrachunkiIdx | — | Rozrachunki wg dokumentów |
| `Ewidencja Środków Pieniężnych/Rozrachunki wg kontrahentów` | — | — | Rozrachunki wg kontrahentów |
| `Ewidencja Środków Pieniężnych/Zobowiązania i należności` | Platnosci | NumerDokumentu, RachunekWirtualny | Zobowiązania i należności |
| `Ewidencja Środków Pieniężnych/Wpłaty i wypłaty` | Zaplaty | NumerDokumentu | Wpłaty i wypłaty |
| `Ewidencja Środków Pieniężnych/Dokumenty rozliczeniowe/Kompensaty` | DokRozliczeniowe | Numer.Pelny, Numer.Symbol | Kompensaty |
| `Ewidencja Środków Pieniężnych/Dokumenty rozliczeniowe/Noty odsetkowe` | DokRozliczeniowe | Numer.Pelny, Numer.Symbol | Noty odsetkowe |
| `Ewidencja Środków Pieniężnych/Dokumenty rozliczeniowe/Wezwania do zapłaty` | DokRozliczeniowe | Numer.Pelny, Numer.Symbol | Wezwania do zapłaty |
| `Ewidencja Środków Pieniężnych/Dokumenty rozliczeniowe/Rozliczenie delegacji` | DokRozliczeniowe | Numer.Pelny, Numer.Symbol | Rozliczenie delegacji |
| `Ewidencja Środków Pieniężnych/Sprawy windykacyjne` | SprawyWindyk | Numer.Pelny, Numer.Symbol | Sprawy windykacyjne |

## Księgowość

| programFolderPath | tableName | selectorField | Opis |
|---|---|---|---|
| `Księgowość/Obroty i salda` | Konta | — | Zestawienie zapisów na kontach |
| `Księgowość/Dziennik/Dekrety` | Dziennik | Numer.Pelny, Numer.Symbol | Dziennik wg dekretów |
| `Księgowość/Dziennik/Zapisy` | ZapisyKsiegowe | — | Dziennik wg zapisów |
| `Księgowość/Plan kont` | Konta | — | Plan kont księgowych |
| `Księgowość/Rozrachunki wg dokumentów` | RozrachunkiIdx | — | Rozrachunki wg dokumentów |
| `Księgowość/Rozrachunki wg kontrahentów` | — | — | Rozrachunki wg kontrahentów |
| `Księgowość/Rozliczenia księgowe` | ZapisyKsiegowe | — | Rozliczenia na kontach |
| `Księgowość/Definicje zestawień księgowych` | ZestawieniaKS | Symbol | Bilans, rachunek wyników |
| `Księgowość/Wyniki zestawień` | WynikiZestKS | Nazwa | Wyniki zestawień księgowych |
| `Księgowość/Sprawozdania finansowe` | SprawozdaniaKS | — | Sprawozdania finansowe |
| `Księgowość/Deklaracje/CIT-8` | Deklaracje | Numer.Pelny, Numer.Symbol | Deklaracja CIT-8 |
| `Księgowość/Deklaracje/Zaliczka PIT skala` | Deklaracje | Numer.Pelny, Numer.Symbol | Zaliczka PIT — skala |
| `Księgowość/RMK/Dokumenty RMK` | DokumentyRMK | Numer.Pelny, Numer.Symbol | Rozliczenia międzyokresowe kosztów |
| `Księgowość/Złe długi` | ZDDokumenty | Numer.Pelny, Numer.Symbol | Dokumenty złych długów |

## Ewidencja dokumentów i VAT

| programFolderPath | tableName | selectorField | Opis |
|---|---|---|---|
| `Ewidencja dokumentów/Dokumenty` | DokEwidencja | Numer.Pelny, Numer.Symbol, NumerDodatkowy, KomunikatKSeF.NumerDokumentuKSeF, NumerDokumentu | Ewidencja dokumentów |
| `Ewidencja dokumentów/Ewidencja dokumentów VAT` | NagEwidencjiVATT | Numer, NumerDokumentu | Dokumenty VAT |
| `Ewidencja dokumentów/Rejestr VAT` | NagRozliczenVATT | NumerDokumentu, NumerEwidencji | Rejestr VAT |
| `Ewidencja dokumentów/Deklaracja VAT-7` | Deklaracje | Numer.Pelny, Numer.Symbol | Deklaracja VAT-7 |
| `Ewidencja dokumentów/Deklaracja VAT-UE` | Deklaracje | Numer.Pelny, Numer.Symbol | Deklaracja VAT-UE |
| `Ewidencja dokumentów/Jednolite pliki kontrolne` | JednolitePK | — | JPK |
| `Ewidencja dokumentów/eDeklaracje` | EDeklaracje | — | Deklaracje elektroniczne |
| `Ewidencja dokumentów/Noty korygujące` | DokRozliczeniowe | Numer.Pelny, Numer.Symbol | Noty korygujące |
| `Ewidencja dokumentów/Matryce dokumentów` | Matryce | Symbol | Szablony dokumentów |

## Kadry i płace

| programFolderPath | tableName | selectorField | Opis |
|---|---|---|---|
| `Kadry i płace/Kadry/Pracownicy` | Pracownicy | Kod, Nazwisko, PESEL | Kartoteka pracowników |
| `Kadry i płace/Kadry/Zleceniobiorcy` | Pracownicy | Kod, Nazwisko, PESEL | Kartoteka zleceniobiorców |
| `Kadry i płace/Kadry/Wszyscy` | Pracownicy | Kod, Nazwisko, PESEL | Wszyscy zatrudnieni |
| `Kadry i płace/Kadry/Umowy` | Umowy | Numer.Pelny, Numer.Symbol | Umowy cywilnoprawne |
| `Kadry i płace/Kadry/Czas pracy/Nieobecności` | NieobecnosciIdx | — | Nieobecności |
| `Kadry i płace/Kadry/Czas pracy/Limity nieobecności` | LimNieobecnosci | — | Limity urlopowe |
| `Kadry i płace/Kadry/Czas pracy/Wnioski o urlopy, delegacje` | WnioskiUrlopowe | — | Wnioski urlopowe |
| `Kadry i płace/Kadry/Ewidencje/Umowy o pracę` | PracHistorie | — | Umowy o pracę |
| `Kadry i płace/Kadry/Ewidencje/Badania lekarskie` | BadaniaLekarskie | — | Badania lekarskie |
| `Kadry i płace/Kadry/Ewidencje/Szkolenia BHP` | SzkoleniaBHP | — | Szkolenia BHP |
| `Kadry i płace/Płace/Listy płac` | ListyPlac | Numer.Pelny, Numer.Symbol | Listy płac |
| `Kadry i płace/Płace/Wypłaty` | Wyplaty | Numer.Pelny, Numer.Symbol | Wypłaty |
| `Kadry i płace/Płace/Elementy wypłaty` | WypElementy | — | Składniki wypłat |
| `Kadry i płace/Deklaracje ZUS/DRA` | Deklaracje | Numer.Pelny, Numer.Symbol | Deklaracje DRA |
| `Kadry i płace/Deklaracje ZUS/Zgłoszeniowe` | Deklaracje | Numer.Pelny, Numer.Symbol | Deklaracje zgłoszeniowe |
| `Kadry i płace/Deklaracje PIT/PIT-11` | Deklaracje | Numer.Pelny, Numer.Symbol | Deklaracja PIT-11 |
| `Kadry i płace/Deklaracje PIT/PIT-4R` | Deklaracje | Numer.Pelny, Numer.Symbol | Deklaracja PIT-4R |
| `Kadry i płace/Dokumenty PPK/Rozliczenie składek` | Deklaracje | Numer.Pelny, Numer.Symbol | Rozliczenie składek PPK |

## Księga inwentarzowa

| programFolderPath | tableName | selectorField | Opis |
|---|---|---|---|
| `Księga inwentarzowa/Ewidencja środków trwałych` | SrodkiTrwale | Nazwa, NumerInwentarzowy | Środki trwałe |
| `Księga inwentarzowa/Dokumenty środków trwałych` | DokumentyST | Numer.Pelny, Numer.Symbol | Dokumenty środków trwałych |

## CRM

| programFolderPath | tableName | selectorField | Opis |
|---|---|---|---|
| `CRM/Kontrahenci` | Kontrahenci | EuVAT, Kod, Nazwa, NIP | Kartoteka kontrahentów CRM |
| `CRM/Zadania` | Zadania | Numer.Pelny, Numer.Symbol, Dzial, MailId, Nazwa | Zadania CRM |
| `CRM/Moje zadania` | Zadania | Numer.Pelny, Numer.Symbol, Dzial, MailId, Nazwa | Zadania bieżącego operatora |
| `CRM/Zdarzenia` | Zadania | Numer.Pelny, Numer.Symbol, Dzial, MailId, Nazwa | Rejestr zdarzeń CRM |
| `CRM/Aktywności` | Zadania | Numer.Pelny, Numer.Symbol, Dzial, MailId, Nazwa | Aktywności: rozmowy, spotkania, notatki |
| `CRM/Leady` | Leady | Numer.Pelny, Numer.Symbol, Nazwa | Leady sprzedażowe |
| `CRM/Transakcje` | Transakcje | Numer.Pelny, Numer.Symbol, Nazwa | Transakcje sprzedażowe |
| `CRM/Projekty` | Projekty | Numer.Pelny, Numer.Symbol, Nazwa | Projekty CRM |
| `CRM/Kampanie` | Kampanie | Numer.Pelny, Numer.Symbol, Nazwa | Kampanie marketingowe |
| `CRM/Kanban` | — | — | Tablica Kanban |
| `CRM/Kalendarz CRM` | — | — | Kalendarz z zadaniami i zdarzeniami |

## Pozostałe moduły

| programFolderPath | tableName | selectorField | Opis |
|---|---|---|---|
| `Terminarz` | — | — | Zadania do wykonania |
| `DMS` | — | — | Zarządzanie dokumentami |
| `Workflow` | — | — | Procesy workflow |

## foldery konfiguracyjne

| programFolderPath | tableName | selectorField   | Opis |
|---|---|-----------------|---|
| `Ustawienia/Firma/Nazwa i adres` | — | —               | Dane identyfikacyjne firmy |
| `Ustawienia/Firma/Dane o działalności` | — | —               | Rodzaj działalności |
| `Ustawienia/Systemowe/Uprawnienia/Operatorzy` | Operators | Name, Trusted   | Konta operatorów |
| `Ustawienia/Systemowe/Uprawnienia/Role systemowe` | SystemRoles | Name            | Role i uprawnienia |
| `Ustawienia/Systemowe/Definicje cech` | FeatureDefs | TableName, Name | Definicje cech systemowych |
| `Ustawienia/Systemowe/Definicje dokumentów dodatkowych` | TuplesDefs | NazwaTabeli     | Definicje dokumentów dodatkowych |
| `Ustawienia/Systemowe/Definicje zadań` | TaskDefs | TableName, Name | Definicje zadań systemowych |
| `Ustawienia/Systemowe/Pliki` | — | —               | Pliki systemowe |
| `Ustawienia/Handel/Ogólne` | — | —               | Ustawienia ogólne handlu |
| `Ustawienia/Handel/Definicje dokumentów/Wszystkie` | DefDokHandlowych | Nazwa, Symbol   | Definicje dokumentów — wszystkie |
| `Ustawienia/Handel/Definicje cen` | DefinicjeCen | Nazwa           | Definicje cenników |
| `Ustawienia/Handel/Magazyny` | Magazyny | Nazwa, Symbol   | Konfiguracja magazynów |
| `Ustawienia/Handel/Jednostki` | Jednostki | Kod             | Jednostki miary |
| `Ustawienia/Kadry i płace/Kadry/Ogólne` | — | —               | Ustawienia ogólne kadr |
| `Ustawienia/Kadry i płace/Kadry/Jednostki organizacyjne` | Wydzialy | Kod, Nazwa      | Struktura organizacyjna |
| `Ustawienia/Kadry i płace/Płace/Elementy wynagrodzenia` | DefElementow | Kod, Nazwa      | Definicje składników płac |
| `Ustawienia/Kadry i płace/Płace/Listy płac` | DefListPlac | Symbol          | Definicje list płac |
| `Ustawienia/Kadry i płace/Płace/Wskaźniki` | — | —               | Wskaźniki płacowe |
| `Ustawienia/Kadry i płace/Kalendarze/Kalendarze` | Kalendarze | —               | Kalendarze pracy |
| `Ustawienia/Kadry i płace/Kalendarze/Definicje nieobecności` | DefNieobecnosci | Kod, Nazwa      | Typy nieobecności |
| `Ustawienia/Kadry i płace/Deklaracje/ZUS` | — | —               | Konfiguracja ZUS |
| `Ustawienia/Kadry i płace/Deklaracje/PIT` | — | —               | Konfiguracja PIT |
| `Ustawienia/Księgowość/Ogólne` | — | —               | Ustawienia ogólne księgowości |
| `Ustawienia/Księgowość/Okresy obrachunkowe` | OkresyObrach | Symbol          | Okresy obrachunkowe |
| `Ustawienia/Księgowość/Schematy księgowe` | SchematyKsiegowe | —               | Schematy dekretacji |
| `Ustawienia/Księgowość/Definicje kręgów` | DefinicjeKregow | Symbol          | Kręgi kosztów |
| `Ustawienia/Ewidencja VAT/Ogólne` | — | —               | Ustawienia VAT |
| `Ustawienia/Ewidencja VAT/Definicje stawek VAT` | DefStawekVat | Kod             | Stawki VAT |
| `Ustawienia/Ewidencja dokumentów/Definicje dokumentów` | DefDokumentow | Symbol          | Definicje ewidencji dokumentów |
| `Ustawienia/Ewidencje ŚP/Ewidencje` | EwidencjeSP | Nazwa, Symbol   | Kasy i rachunki bankowe |
| `Ustawienia/Ewidencje ŚP/Sposoby zapłaty` | SposobyZaplaty | Nazwa           | Sposoby zapłaty |
| `Ustawienia/Ewidencje ŚP/Waluty` | Waluty | Symbol          | Waluty |
| `Ustawienia/Ewidencje ŚP/Tabele kursowe` | TabeleKursowe | Nazwa           | Tabele kursów walut |
